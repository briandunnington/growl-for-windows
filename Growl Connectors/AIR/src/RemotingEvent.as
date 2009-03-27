package
{
	import flash.events.Event;
	
	/**
	 * This custom Event class adds a message property to a basic Event.
	 */
	public class RemotingEvent extends Event
	{
		/**
		 * The name of the new RemotingEvent type.
		 */
		public static const REMOTING:String = "newRemotingMsg";
		
		/**
		 * A text message that can be passed to an event handler
		 * with this event object.
		 */
		public var message:String;
		
		/**
		 *  Constructor.
		 *  @param message The text to display when the alarm goes off.
		 */
		public function RemotingEvent(message:String)
		{
			super(REMOTING);
	
			this.message = message;
		}
		
		
		/**
		 * Creates and returns a copy of the current instance.
		 * @return	A copy of the current instance.
		 */
		public override function clone():Event
		{
			return new RemotingEvent(message);
		}
		
		
		/**
		 * Returns a String containing all the properties of the current
		 * instance.
		 * @return A string representation of the current instance.
		 */
		public override function toString():String
		{
			return formatToString("RemotingEvent", 
                "type", "bubbles", "cancelable", 
                "eventPhase", "message");
		}
	}
}