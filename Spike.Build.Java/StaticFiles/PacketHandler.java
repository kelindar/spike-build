package com.misakai.spike.network;

/**
 * The listener interface for receiving events. The class must define a method called onReceive. 
 */
//@FunctionalInterface //Too soon with android 
public interface PacketHandler<Packet> {
	/**
	 * Invoked when a Packet is receive.
	 */
	void onReceive(Packet packet);
}
