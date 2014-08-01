package com.misakai.spike.network;

/**
 * The listener interface for receiving disconnection events. The class must define a method of no arguments called onDisconnect. 
 */
//@FunctionalInterface //Too soon with android 
public interface DisconnectionHandler {
	/**
	 * Invoked when a disconnection occurs.
	 */
	public void onDisconnect();
}