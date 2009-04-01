package  
{
	import com.hurlant.crypto.hash.SHA256;
	import com.hurlant.crypto.symmetric.CBCMode;
	import com.hurlant.crypto.symmetric.ECBMode;
	import com.hurlant.crypto.symmetric.ICipher;
	import com.hurlant.crypto.symmetric.ISymmetricKey;
	import com.hurlant.crypto.symmetric.IVMode;
	import com.hurlant.crypto.symmetric.PKCS5;
	import flash.utils.ByteArray;
	import com.hurlant.crypto.symmetric.IMode;
	import com.hurlant.crypto.symmetric.NullPad;
	import com.hurlant.crypto.symmetric.AESKey;
	import com.hurlant.crypto.symmetric.DESKey;
	import com.hurlant.crypto.symmetric.TripleDESKey;
	import com.hurlant.crypto.hash.MD5;
	import com.hurlant.crypto.hash.SHA1;
	import com.hurlant.crypto.hash.IHash;
	import com.hurlant.util.Hex;
	import com.hurlant.crypto.prng.Random;
	
	public class MessageBuilder 
	{
		private var messageType:String;
		private var password:String;
		private var keyHashAlgorithm:String = HashAlgorithm.MD5;
		private var encryptionAlgorithm:String = EncryptionAlgorithm.PlainText;
		private var bytes:ByteArray;
		
		public function MessageBuilder(messageType:String, password:String, keyHashAlgorithm:String, encryptionAlgorithm:String)
		{
			this.messageType = messageType;
			this.password = password;
			this.keyHashAlgorithm = keyHashAlgorithm;
			this.encryptionAlgorithm = encryptionAlgorithm;
			this.bytes = new ByteArray();
		}
		
		public function addHeader(name:String, value:String):void
		{
			var s:String = name + ": " + value + "\r\n";
			this.bytes.writeUTFBytes(s);
		}
		
		public function addBlankLine():void
		{
			this.bytes.writeUTFBytes("\r\n");
		}
		
		public function getBytes():ByteArray
		{
			var encryptedBytes:ByteArray = new ByteArray();
			encryptedBytes.writeBytes(this.bytes, 0, this.bytes.length);
			encryptedBytes.position = 0;
			
			var key:ByteArray;
			var encryptionInfo:String = encryptionAlgorithm.toUpperCase();
			var hashInfo:String = "";
			if (this.password != null && this.password != "")
			{
				var r:Random = new Random;
				var salt:ByteArray = new ByteArray;
				r.nextBytes(salt, 16);
				
				var hasher:IHash = getHasher(this.keyHashAlgorithm);
				var passwordBytes:ByteArray = new ByteArray();
				passwordBytes.writeUTFBytes(this.password);
				passwordBytes.writeBytes(salt, 0, salt.length);
				key = hasher.hash(passwordBytes);
				var keyHash:ByteArray = hasher.hash(key);
				
				var cipher:IMode = getCipher(key);	// auto-generate the IV
				cipher.encrypt(encryptedBytes);
				
				if (this.encryptionAlgorithm != EncryptionAlgorithm.PlainText)
				{
					var iv:ByteArray = (cipher as IVMode).IV;
					var ivHex:String = Hex.fromArray(iv);
					encryptionInfo = encryptionInfo + ":" + ivHex.toUpperCase();
				}
				
				var keyHashHex:String = Hex.fromArray(keyHash);
				var saltHex:String = Hex.fromArray(salt);
				hashInfo = this.keyHashAlgorithm + ":" + keyHashHex.toUpperCase() + "." + saltHex;
			}
			
			var headerBytes:ByteArray = new ByteArray();
			headerBytes.writeUTFBytes("GNTP/1.0 " + this.messageType + " " + encryptionInfo + " " + hashInfo + "\r\n");
			
			var allBytes:ByteArray = new ByteArray();
			allBytes.writeBytes(headerBytes, 0, headerBytes.length);
			allBytes.writeBytes(encryptedBytes, 0, encryptedBytes.length);
			allBytes.writeUTFBytes("\r\n\r\n");

			//ExternalInterface.call(getJSFunctionName("debug"), encryptedBytes.length);
			
			return allBytes;
		}
		
		public function toString():String
		{
			var b:ByteArray = this.getBytes();
			b.position = 0;
			var s:String = b.readUTFBytes(b.length);
			return s;
		}

		private function getHasher(name:String):IHash
		{
			var hasher:IHash;
			switch (name) 
			{
				case HashAlgorithm.SHA1 :
					hasher = new SHA1();
					break;
				case HashAlgorithm.SHA256 :
					hasher = new SHA256();
					break;
				default :
					hasher = new MD5();
					break;
			}
			return hasher;
		}
		
		private function getCipher(key:ByteArray):IMode
		{
			var cipher:IMode;
			switch (this.encryptionAlgorithm) 
			{
				case EncryptionAlgorithm.AES :
					key = getKeyFromSize(key, 24);
					cipher = new CBCMode(new AESKey(key), new PKCS5);
					break;
				case EncryptionAlgorithm.DES :
					key = getKeyFromSize(key, 8);
					cipher = new CBCMode(new DESKey(key), new PKCS5);
					break;
				case EncryptionAlgorithm.TripleDES :
					key = getKeyFromSize(key, 24);
					cipher = new CBCMode(new TripleDESKey(key), new PKCS5);
					break;
				default :
					cipher = new PlainTextMode();
					break;
			}

			return cipher;
		}
		
		private function getCipherWithIV(key:ByteArray, iv:ByteArray):IMode
		{
			var cipher:IMode = getCipher(key);
			if(cipher is IVMode) (cipher as IVMode).IV = iv;
			return cipher;
		}
		
		private function getKeyFromSize(key:ByteArray, size:int):ByteArray
		{
			var start:int = 0;
			var end:int = start + size;

            // parameter checking
            if (end > key.length)
				throw new Error("GetKeyFromSize: The requested key size is longer than the supplied key. Requested size: " + size.toString() + ", key length: " + key.length.toString());

			var newKey:ByteArray = new ByteArray();
			newKey.writeBytes(key, 0, size);
			newKey.position = 0;
			return newKey;
		}

		// DEBUG ONLY
		private function getJSFunctionName(functionName:String):String
		{
			var fullName:String = "Growl." + functionName;
			return fullName;
		}
	}
}