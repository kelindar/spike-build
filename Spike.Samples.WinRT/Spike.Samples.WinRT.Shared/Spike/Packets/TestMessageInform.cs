using Spike.Network.CustomTypes;

namespace Spike.Network.Packets
{
    public sealed class TestMessageInform
    {
	
		public uint Id { get; set; }
	
		public MyCustomClass MyCustom { get; set; }
	
		public int Number { get; set; }
	
		public string Message { get; set; }

    }
}
