
using Spike.Network.Packets;
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
		
		public event Action<TcpChannel, MyChatMessages> MyChatMessages; 
		    
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

		//Dispatcher
		protected override void OnReceive(uint key)
		{
			switch (key)
			{
				
				case 0xF6F85E84u:
				{
					var packet = new MyChatMessages();
					BeginReadPacket(true);
					
					packet.Avatar = PacketReadListOfByte();
					packet.Message = PacketReadString();

					//Now Call event
					if (MyChatMessages != null)
						MyChatMessages(this, packet);

					break;
				}

				default:
					Debug.WriteLine("Unknow packet : {0:X}", key);
					return;
			}
		}
	}
}