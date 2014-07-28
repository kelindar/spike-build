
using Spike.Network.Packets;
using Spike.Network.CustomTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spike.Network
{
	public class TcpChannel : TcpChannelBase<TcpChannel>
	{
		//Events
		
		public event Action<TcpChannel, MyChatMessagesInform> MyChatMessagesInform; 
		
		public event Action<TcpChannel, TestMessageInform> TestMessageInform; 
		    
		//Sends        
		
		public async void JoinMyChat()
		{
			BeginNewPacket(0x84157E5C);
			await SendPacket(false);
		}		 
		
		public async void SendMyChatMessage(string Message)
		{
			BeginNewPacket(0xBD7E2CA4);
			PacketWrite(Message);
			await SendPacket(true);
		}		 
		
		public async void TestMessage(uint Id, MyCustomClass2 MyCustom, int Number2, float Floflo, string Message2)
		{
			BeginNewPacket(0x36290E3E);
			PacketWrite(Id);
			PacketWrite(MyCustom);
			PacketWrite(Number2);
			PacketWrite(Floflo);
			PacketWrite(Message2);
			await SendPacket(true);
		}		 

		//Dispatcher
		protected override void OnReceive(uint key)
		{
			switch (key)
			{
				
				case 0xF6F85E84u:
				{
					var packet = new MyChatMessagesInform();
					BeginReadPacket(true);
					
					packet.Avatar = PacketReadListOfByte();
					packet.Message = PacketReadString();

					//Now Call event
					if (MyChatMessagesInform != null)
						MyChatMessagesInform(this, packet);

					break;
				}
				
				case 0x36290E3Eu:
				{
					var packet = new TestMessageInform();
					BeginReadPacket(true);
					
					packet.Id = PacketReadUInt32();
					packet.MyCustom = PacketReadMyCustomClass();
					packet.Number = PacketReadInt32();
					packet.Message = PacketReadString();

					//Now Call event
					if (TestMessageInform != null)
						TestMessageInform(this, packet);

					break;
				}

				default:
					Debug.WriteLine("Unknow packet : {0:X}", key);
					return;
			}
		}

		//Custom Type
		protected MyCustomClass PacketReadMyCustomClass()
        {
            var value = new MyCustomClass();
			value.Number = PacketReadInt32();
			value.Message = PacketReadString();
			return value;
        }
        protected void PacketWrite(MyCustomClass value)
        {
            			PacketWrite(value.Number);
			PacketWrite(value.Message);
        }

        protected MyCustomClass[] PacketReadListOfMyCustomClass()
        {
            var value = new MyCustomClass[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadMyCustomClass();
            return value;
        }
        protected void PacketWrite(MyCustomClass[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }
		protected MyCustomClass2 PacketReadMyCustomClass2()
        {
            var value = new MyCustomClass2();
			value.Number2 = PacketReadInt32();
			value.Floflo = PacketReadSingle();
			value.Message2 = PacketReadString();
			return value;
        }
        protected void PacketWrite(MyCustomClass2 value)
        {
            			PacketWrite(value.Number2);
			PacketWrite(value.Floflo);
			PacketWrite(value.Message2);
        }

        protected MyCustomClass2[] PacketReadListOfMyCustomClass2()
        {
            var value = new MyCustomClass2[PacketReadInt32()];
            for (int index = 0; index < value.Length; index++)
                value[index] = PacketReadMyCustomClass2();
            return value;
        }
        protected void PacketWrite(MyCustomClass2[] value)
        {
            PacketWrite(value.Length);
            foreach (var element in value)
                PacketWrite(element);
        }

	}
}