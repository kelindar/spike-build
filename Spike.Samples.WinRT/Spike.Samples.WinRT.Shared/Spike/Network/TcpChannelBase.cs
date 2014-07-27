/************************************************************************
*
* Copyright (C) 2009-2014 Misakai Ltd
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
* 
*************************************************************************/

using LZF.NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Spike.Network
{
    public abstract class TcpChannelBase<T> where T : TcpChannelBase<T>
    {
        public event Action<T> Connected;
        public event Action<T> Disconnected;

        private StreamSocket socket;
        private DataWriter SocketWriter;
        private DataReader SocketReader;

        private byte[] SendBuffer;
        private int SendBufferPosition;

        private byte[] ReceiveBuffer;
        private int ReceiveBufferPosition;
        private uint ReceiveBufferSize;

        public async void Connect(string host, int port)
        {
            socket = new StreamSocket();
            await socket.ConnectAsync(new HostName(host), port.ToString());

            SocketWriter = new DataWriter(socket.OutputStream);
            SocketReader = new DataReader(socket.InputStream);
            SocketReader.InputStreamOptions = InputStreamOptions.Partial;

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
                    ReceiveBufferSize = await SocketReader.LoadAsync(4096);
                    for (int index = 0; index < ReceiveBufferSize; index++)
                        ReceiveBuffer[index] = SocketReader.ReadByte();
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


        #region Sends

        protected void BeginNewPacket(uint key)
        {
            SendBufferPosition = 4;
            PacketWrite(key);
        }

        protected void PacketWrite(byte value)
        {
            SendBuffer[SendBufferPosition++] = value;
        }
        protected void PacketWrite(sbyte value)
        {
            PacketWrite((byte)value);
        }

        protected void PacketWrite(uint value)
        {
            PacketWrite((byte)(value >> 24));
            PacketWrite((byte)(value >> 16));
            PacketWrite((byte)(value >> 8));
            PacketWrite((byte)value);
        }

        protected void PacketWrite(int value)
        {
            PacketWrite((uint)value);            
        }

        private void SetSize()
        {
            var size = SendBufferPosition - 4;
            SendBuffer[0] = ((byte)(size >> 24));
            SendBuffer[1] = ((byte)(size >> 16));
            SendBuffer[2] = ((byte)(size >> 8));
            SendBuffer[3] = ((byte)size);
        }
        protected void PacketWrite(String value)
        {
            PacketWrite(Encoding.UTF8.GetBytes(value));
        }
        protected void PacketWrite(byte[] value)
        {
            PacketWrite(value.Length);
            System.Buffer.BlockCopy(value,0,SendBuffer,SendBufferPosition,value.Length);
            SendBufferPosition += value.Length;
        }

        protected async Task SendPacket(bool compressed)
        {
            if (compressed && SendBufferPosition > 8)
            {
                //todo
                var cipher = new CLZF();
                var uncompressedBytes = new byte[SendBufferPosition -8];
                System.Buffer.BlockCopy(SendBuffer, 8, uncompressedBytes, 0, uncompressedBytes.Length);
                var compressedBytes = new byte[4096];
                var size = cipher.lzf_compress(uncompressedBytes, uncompressedBytes.Length, compressedBytes, compressedBytes.Length);
                System.Buffer.BlockCopy(compressedBytes, 0, SendBuffer, 8, size);
                SendBufferPosition = size + 8;
            }

            SetSize();
            for (int index = 0; index < SendBufferPosition; index++)
                SocketWriter.WriteByte(SendBuffer[index]);

            await SocketWriter.StoreAsync();
            //Debug.WriteLine("Packet send");
        }

        #endregion 

        #region receive
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
                ReceiveBufferSize = (uint)uncompressedSize + 8;
            }
        }

        protected byte[] PacketReadListOfByte()
        {
            int size = PacketReadInt32();
            var value = new byte[size];
            System.Buffer.BlockCopy(ReceiveBuffer, ReceiveBufferPosition, value, 0, size);
            ReceiveBufferPosition += size;            
            return value;
        }

        protected string PacketReadString()
        {
            var bytes = PacketReadListOfByte();
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);            
        }

        public bool PacketReadBoolean()
        {
            return ReceiveBuffer[ReceiveBufferPosition++] != 0;
        }
        public ushort PacketReadUInt16()
        {
            return (ushort)PacketReadInt16();
        }
        public short PacketReadInt16()
        {
            return (short)((ReceiveBuffer[ReceiveBufferPosition++] << 8) | ReceiveBuffer[ReceiveBufferPosition++]);
            
        }

        public uint PacketReadUInt32()
        {
            return (uint)PacketReadInt32();
        }
        public int PacketReadInt32()
        {
            return ReceiveBuffer[ReceiveBufferPosition++] << 24
                 | (ReceiveBuffer[ReceiveBufferPosition++] << 16)
                 | (ReceiveBuffer[ReceiveBufferPosition++] << 8)
                 | (ReceiveBuffer[ReceiveBufferPosition++]);
        }

        public ulong PacketReadUInt64()
        {
            return (ulong)PacketReadInt64();
        }
        public long PacketReadInt64()
        {
            long value = ReceiveBuffer[ReceiveBufferPosition++];
            value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++];
            value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++];
            value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++];
            value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++];
            value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++];
            value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++];
            value <<= 8;
            value |= ReceiveBuffer[ReceiveBufferPosition++];
            return value;
        }

        public int[] PacketReadListOfInt32() {
            var value = new int[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadInt32();
            return value;
        }

        public short[] PacketReadListOfInt16()
        {
            var value = new short[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadInt16();
            return value;
        }

        public DateTime PacketReadDateTime()
        {
            short year = PacketReadInt16();
            short month = PacketReadInt16();
            short day = PacketReadInt16();
            short hour = PacketReadInt16();
            short minute = PacketReadInt16();
            short second = PacketReadInt16();
            short millisecond = PacketReadInt16();

            return new DateTime(year, month, day, hour, minute, second, millisecond);
            //return new DateTime(long.Parse(ReadString()));
        }

        protected abstract void OnReceive(uint key);
        #endregion

    }
}
