using LZF.NET;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Spike.Network
{
    public abstract class TcpChannelBase<T> where T : TcpChannelBase<T>
    {
        public event Action<T> Connected;
        public event Action<T> Disconnected;

        private Socket socket;
        
        private byte[] SendBuffer;
        private int SendBufferPosition;

        private byte[] ReceiveBuffer;
        private int ReceiveBufferPosition;
        private int ReceiveBufferSize;

        public async void Connect(string host, int port)
        {
			socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            

			await Task.Run(() => socket.Connect (host, port));

            SendBuffer = new byte[4096];
            SendBufferPosition = 0;
            
			ReceiveBuffer = new byte[4096];
            ReceiveBufferPosition = 0;
            ReceiveBufferSize = 0;

            if (Connected != null)
                Connected((T)this);

            try
            {
                while (true)
                {
					ReceiveBufferSize = await Task<int>.Run(() => 
						{return socket.Receive(ReceiveBuffer);}
					);
					 
					ReceiveBufferPosition = 0;

                    if (ReceiveBufferSize != (PacketReadInt32() + 4))
                    {
                        Debug.WriteLine("No fragmentation");
                        Disconnect();
                        return;
                    }
                    OnReceive(PacketReadUInt32());
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            socket.Dispose();
            if (Disconnected != null)
                Disconnected((T)this);
        }

        protected void BeginReadPacket(bool compressed)
        {
            if (compressed)
            {
                var compressedBuffer = new byte[ReceiveBufferSize - 8];
                var uncompressedBuffer = new byte[4096];
                System.Buffer.BlockCopy(ReceiveBuffer, 8, compressedBuffer, 0, compressedBuffer.Length);
                var cipher = new CLZF();
                var uncompressedSize = cipher.lzf_decompress(compressedBuffer, compressedBuffer.Length, uncompressedBuffer, uncompressedBuffer.Length);
                System.Buffer.BlockCopy(uncompressedBuffer, 0, ReceiveBuffer, 8, uncompressedSize);
                ReceiveBufferSize = uncompressedSize + 8;
            }
        }

        protected void BeginNewPacket(uint key)
        {
            SendBufferPosition = 4;
            PacketWrite(key);
        }

        private void SetSize()
        {
            var size = SendBufferPosition - 4;
            SendBuffer[0] = ((byte)(size >> 24));
            SendBuffer[1] = ((byte)(size >> 16));
            SendBuffer[2] = ((byte)(size >> 8));
            SendBuffer[3] = ((byte)size);
        }

        protected async Task SendPacket(bool compressed)
        {
            if (compressed && SendBufferPosition > 8)
            {
                //TODO make this better
                var cipher = new CLZF();
                var uncompressedBytes = new byte[SendBufferPosition - 8];
                System.Buffer.BlockCopy(SendBuffer, 8, uncompressedBytes, 0, uncompressedBytes.Length);
                var compressedBytes = new byte[4096];
                var size = cipher.lzf_compress(uncompressedBytes, uncompressedBytes.Length, compressedBytes, compressedBytes.Length);
                System.Buffer.BlockCopy(compressedBytes, 0, SendBuffer, 8, size);
                SendBufferPosition = size + 8;
            }

            SetSize();
            
			await Task.Run(() => {
				socket.Send(SendBuffer, SendBufferPosition, SocketFlags.None);
			});            
        }

        #region Spike Primary Type
        // Byte
        protected void PacketWrite(byte value)
        {
            SendBuffer[SendBufferPosition++] = value;
        }
        protected byte PacketReadByte()
        {
            return ReceiveBuffer[ReceiveBufferPosition++];
        }
        protected byte[] PacketReadListOfByte()
        {
            var value = new byte[PacketReadInt32()];
            System.Buffer.BlockCopy(ReceiveBuffer, ReceiveBufferPosition, value, 0, value.Length);
            ReceiveBufferPosition += value.Length;
            return value;
        }
        protected void PacketWrite(byte[] value)
        {
            PacketWrite(value.Length);
            System.Buffer.BlockCopy(value, 0, SendBuffer, SendBufferPosition, value.Length);
            SendBufferPosition += value.Length;
        }

        // SByte
        //Don't existe in spike protocol

        // UInt16
        protected ushort PacketReadUInt16()
        {
            return (ushort)((ReceiveBuffer[ReceiveBufferPosition++] << 8)
                | ReceiveBuffer[ReceiveBufferPosition++]);
        }
        protected void PacketWrite(ushort value)
        {
            PacketWrite((byte)(value >> 8));
            PacketWrite((byte)value);
        }
        protected ushort[] PacketReadListOfUInt16()
        {
            var value = new ushort[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadUInt16();
            return value;
        }
        protected void PacketWrite(ushort[] value)
        {
            PacketWrite(value.Length);
            var byteSize = System.Buffer.ByteLength(value);
            System.Buffer.BlockCopy(SendBuffer, SendBufferPosition, value, 0, byteSize);
            SendBufferPosition += byteSize;
        }

        // Int16
        protected short PacketReadInt16()
        {
            return (short)((ReceiveBuffer[ReceiveBufferPosition++] << 8)
                | ReceiveBuffer[ReceiveBufferPosition++]);
        }
        protected void PacketWrite(short value)
        {
            PacketWrite((byte)(value >> 8));
            PacketWrite((byte)value);
        }
        protected short[] PacketReadListOfInt16()
        {
            var value = new short[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadInt16();
            return value;
        }
        protected void PacketWrite(short[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }

        // UInt32
        protected uint PacketReadUInt32()
        {
            return (uint)(ReceiveBuffer[ReceiveBufferPosition++] << 24
                 | (ReceiveBuffer[ReceiveBufferPosition++] << 16)
                 | (ReceiveBuffer[ReceiveBufferPosition++] << 8)
                 | (ReceiveBuffer[ReceiveBufferPosition++]));
        }
        protected void PacketWrite(uint value)
        {
            PacketWrite((byte)(value >> 24));
            PacketWrite((byte)(value >> 16));
            PacketWrite((byte)(value >> 8));
            PacketWrite((byte)value);
        }
        protected uint[] PacketReadListOfUInt32()
        {
            var value = new uint[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadUInt32();
            return value;
        }
        protected void PacketWrite(uint[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }

        // Int32
        protected int PacketReadInt32()
        {
            return ReceiveBuffer[ReceiveBufferPosition++] << 24
                 | (ReceiveBuffer[ReceiveBufferPosition++] << 16)
                 | (ReceiveBuffer[ReceiveBufferPosition++] << 8)
                 | (ReceiveBuffer[ReceiveBufferPosition++]);
        }

        protected void PacketWrite(int value)
        {
            PacketWrite((byte)(value >> 24));
            PacketWrite((byte)(value >> 16));
            PacketWrite((byte)(value >> 8));
            PacketWrite((byte)value);
        }
        protected int[] PacketReadListOfInt32()
        {
            var value = new int[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadInt32();
            return value;
        }
        protected void PacketWrite(int[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }


        // UInt64
        protected ulong PacketReadUInt64()
        {
            ulong value = ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++];
            return value;
        }
        protected void PacketWrite(ulong value)
        {
            PacketWrite((byte)(value >> 56));
            PacketWrite((byte)(value >> 48));
            PacketWrite((byte)(value >> 40));
            PacketWrite((byte)(value >> 32));
            PacketWrite((byte)(value >> 24));
            PacketWrite((byte)(value >> 16));
            PacketWrite((byte)(value >> 8));
            PacketWrite((byte)value);
        }
        protected ulong[] PacketReadListOfUInt64()
        {
            var value = new ulong[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadUInt64();
            return value;
        }
        protected void PacketWrite(ulong[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }

        // Int64
        protected long PacketReadInt64()
        {
            long value = ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++]; value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++];
            return value;
        }
        protected void PacketWrite(long value)
        {
            PacketWrite((byte)(value >> 56));
            PacketWrite((byte)(value >> 48));
            PacketWrite((byte)(value >> 40));
            PacketWrite((byte)(value >> 32));
            PacketWrite((byte)(value >> 24));
            PacketWrite((byte)(value >> 16));
            PacketWrite((byte)(value >> 8));
            PacketWrite((byte)value);
        }
        protected long[] PacketReadListOfInt64()
        {
            var value = new long[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadInt64();
            return value;
        }
        protected void PacketWrite(long[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }
        // Boolean
        protected bool PacketReadBoolean()
        {
            return ReceiveBuffer[ReceiveBufferPosition++] != 0;
        }
        protected void PacketWrite(bool value)
        {
            PacketWrite((byte)(value ? 1 : 0));
        }
        public bool[] PacketReadListOfBoolean()
        {
            var value = new bool[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadBoolean();
            return value;
        }
        protected void PacketWrite(bool[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }

        // Single
        protected float PacketReadSingle()
        {
            var value = BitConverter.ToSingle(ReceiveBuffer, ReceiveBufferPosition);
            ReceiveBufferPosition += sizeof(float);
            return value;
        }
        protected void PacketWrite(float value)
        {
            PacketWrite(BitConverter.GetBytes(value));
        }
        protected float[] PacketReadListOfSingle()
        {
            var value = new float[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadSingle();
            return value;
        }
        protected void PacketWrite(float[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }

        // Double
        protected double PacketReadDouble()
        {
            var value = BitConverter.ToDouble(ReceiveBuffer, ReceiveBufferPosition);
            ReceiveBufferPosition += sizeof(double);
            return value;
        }
        protected void PacketWrite(double value)
        {
            PacketWrite(BitConverter.GetBytes(value));
        }
        protected double[] PacketReadListOfDouble()
        {
            var value = new double[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadDouble();
            return value;
        }
        protected void PacketWrite(double[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }

        // String
        protected string PacketReadString()
        {
            var bytes = PacketReadListOfByte();
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
        protected void PacketWrite(string value)
        {
            PacketWrite(Encoding.UTF8.GetBytes(value));
        }
        protected string[] PacketReadListOfString()
        {
            var value = new string[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadString();
            return value;
        }
        protected void PacketWrite(string[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }

        // DateTime
        protected DateTime PacketReadDateTime()
        {
            short year = PacketReadInt16();
            short month = PacketReadInt16();
            short day = PacketReadInt16();
            short hour = PacketReadInt16();
            short minute = PacketReadInt16();
            short second = PacketReadInt16();
            short millisecond = PacketReadInt16();

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }
        protected void PacketWrite(DateTime value)
        {
            PacketWrite((short)value.Year);
            PacketWrite((short)value.Month);
            PacketWrite((short)value.Day);
            PacketWrite((short)value.Hour);
            PacketWrite((short)value.Minute);
            PacketWrite((short)value.Second);
            PacketWrite((short)value.Millisecond);
        }

        protected DateTime[] PacketReadListOfDateTime()
        {
            var value = new DateTime[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadDateTime();
            return value;
        }
        protected void PacketWrite(DateTime[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }
        #endregion

        protected abstract void OnReceive(uint key);

        #region Dynamics
        [Obsolete("DynamicType is obsolete. Consider using JSON or XML serialized objects instead.", false)]
        protected void PacketWriteDynamic(object value)
        {
            if (value is byte)
            {
                PacketWrite(true);
                PacketWrite(@"Byte");
                PacketWrite((byte)value);
            }
            else if (value is ushort)
            {
                PacketWrite(true);
                PacketWrite(@"UInt16");
                PacketWrite((ushort)value);
            }
            else if (value is short)
            {
                PacketWrite(true);
                PacketWrite(@"Int16");
                PacketWrite((short)value);
            }
            else if (value is uint)
            {
                PacketWrite(true);
                PacketWrite(@"UInt32");
                PacketWrite((uint)value);
            }
            else if (value is int)
            {
                PacketWrite(true);
                PacketWrite(@"Int32");
                PacketWrite((int)value);
            }
            else if (value is ulong)
            {
                PacketWrite(true);
                PacketWrite(@"UInt64");
                PacketWrite((ulong)value);
            }
            else if (value is long)
            {
                PacketWrite(true);
                PacketWrite(@"Int64");
                PacketWrite((long)value);
            }
            else if (value is float)
            {
                PacketWrite(true);
                PacketWrite(@"Single");
                PacketWrite((float)value);
            }
            else if (value is double)
            {
                PacketWrite(true);
                PacketWrite(@"Double");
                PacketWrite((double)value);
            }
            else if (value is bool)
            {
                PacketWrite(true);
                PacketWrite(@"Boolean");
                PacketWrite((bool)value);
            }
            else if (value is string)
            {
                PacketWrite(true);
                PacketWrite(@"String");
                PacketWrite((string)value);
            }
            else if (value is DateTime)
            {
                PacketWrite(true);
                PacketWrite(@"DateTime");
                PacketWrite((DateTime)value);
            }
            else
                PacketWrite(false);
        }
        [Obsolete("DynamicType is obsolete. Consider using JSON or XML serialized objects instead.", false)]
        protected object PacketReadDynamic()
        {
            if (PacketReadBoolean())
            {
                switch (PacketReadString())
                {
                    case "Byte":
                        return PacketReadByte();
                    case "UInt16":
                        return PacketReadUInt16();
                    case "Int16":
                        return PacketReadInt16();
                    case "UInt32":
                        return PacketReadUInt32();
                    case "Int32":
                        return PacketReadInt32();
                    case "UInt64":
                        return PacketReadUInt64();
                    case "Int64":
                        return PacketReadInt64();
                    case "Single":
                        return PacketReadSingle();
                    case "Double":
                        return PacketReadDouble();
                    case "Boolean":
                        return PacketReadBoolean();
                    case "String":
                        return PacketReadString();
                    case "DateTime":
                        return PacketReadDateTime();
                }
            }
            return null;
        }
        [Obsolete("DynamicType is obsolete. Consider using JSON or XML serialized objects instead.", false)]
        protected object[] PacketReadListOfDynamic()
        {
            var value = new object[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadDynamic();
            return value;
        }
        [Obsolete("DynamicType is obsolete. Consider using JSON or XML serialized objects instead.", false)]
        protected void PacketWriteDynamic(object[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWriteDynamic(element);
        }
        #endregion


    }
}
