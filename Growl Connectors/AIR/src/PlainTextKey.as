package  
{
	import com.hurlant.crypto.symmetric.ISymmetricKey;
	import flash.utils.ByteArray;

	public class PlainTextKey implements ISymmetricKey
	{
		public function PlainTextKey()
		{
			
		}
		
		public function encrypt(block:ByteArray, index:uint=0):void
		{
		}
		
		public function decrypt(block:ByteArray, index:uint=0):void
		{
		}
		
		public function getBlockSize():uint
		{
			return 4096;
		}
		
		public function toString():String
		{
			return "NONE";
		}
		
		public function dispose():void
		{
		}
	}
}