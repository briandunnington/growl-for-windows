package  
{
	import com.hurlant.crypto.symmetric.IMode;
	import com.hurlant.crypto.symmetric.IPad;
	import com.hurlant.crypto.symmetric.ISymmetricKey;
	import com.hurlant.crypto.symmetric.IVMode;
	import com.hurlant.crypto.symmetric.NullPad;
	import flash.utils.ByteArray;

	public class PlainTextMode extends IVMode implements IMode
	{
		public function PlainTextMode()
		{
			super(new PlainTextKey(), new NullPad());
		}
		
		public function encrypt(block:ByteArray):void
		{
		}
		
		public function decrypt(block:ByteArray):void
		{
		}
		
		public function toString():String
		{
			return "NONE";
		}
	}
}