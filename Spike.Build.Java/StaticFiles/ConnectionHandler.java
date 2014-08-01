package com.misakai.spike.network;

/**
 * The listener interface for receiving connection events. The class must define a method of no arguments called onConnect. 
 */
//@FunctionalInterface //Too soon with android 
public interface ConnectionHandler {
	/**
	 * Invoked when a connection occurs.
	 */
	public void onConnect();
}
