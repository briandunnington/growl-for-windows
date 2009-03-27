using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections;

namespace Growl.Daemon
{
	/// <summary>
	/// Provides a common interface to various types of data wrappers.
	/// This allows for some abstraction of the underlying data,
	/// whether it be raw bytes, files, or streams.
	/// </summary>
	public interface IData
	{
		/// <summary>
		/// Returns whether or not the underlying data is a stream or not.
		/// This may need to be consulted to determine which methods would work best.
		/// </summary>
		bool IsStream { get;}

		/// <summary>
		/// Returns the length of the data.
		/// </summary>
		int Length { get;}

		/// <summary>
		/// Returns the entire underlying data as a byte array.
		/// Warning: If the underlying data is a stream, this means the entire stream will be read to completion.
		/// </summary>
		byte[] ByteArray { get; }

		/// <summary>
		/// Reads a byte at the given index.
		/// </summary>
		/// <param name="index">The index at which to read.</param>
		byte this[int index] { get;}

		/// <summary>
		/// Reads a portion of the data into a given byte array.
		/// If the underlying data is a stream, this method is recommended over ByteArray.
		/// If the underlying data is NOT a stream, it should be noted that this will result in copied bytes.
		/// Thus changes to the resulting byte array will not affect this Data object.
		/// </summary>
		int ReadByteArray(out byte[] result, int offset, int length);

		/// <summary>
		/// Reads the entire data into a string (using UTF8 encoding).
		/// Warning: If the underlying data is a stream, this means the entire stream will be read to completion.
		/// </summary>
		string ToString();

		/// <summary>
		/// Reads the entire data into a string using the given encoding.
		/// Warning: If the underlying data is a stream, this means the entire stream will be read to completion.
		/// </summary>
		/// <param name="enc">The encoding to use to read the data.</param>
		string ToString(Encoding enc);
	}

	/// <summary>
	/// Provides a basic unchangeable IData container for raw or string data.
	/// </summary>
	public class Data : IData
	{
		protected Encoding enc;
		protected byte[] buffer;

		/// <summary>
		/// Creates a new Data object using the given buffer.
		/// The buffer is not copied.
		/// That is, the new Data object is simply a wrapper around the given byte array.
		/// </summary>
		/// <param name="buffer">
		///		Byte array to use as underlying data.
		///	</param>
		public Data(byte[] buffer) : this(buffer, false)
		{
			// Nothing to do here
		}

		/// <summary>
		/// Creates a new Data object using a specified subset of the given data.
		/// The data must necessarily be copied (otherwise it would be unsafe).
		/// </summary>
		/// <param name="buffer">
		///		Byte array to extract data from.
		/// </param>
		/// <param name="offset">
		///		The offset within buffer to start reading from.
		/// </param>
		/// <param name="length">
		///		The amount to read from buffer.
		/// </param>
		public Data(byte[] buffer, int offset, int length)
		{
			// By default we use UTF8
			enc = Encoding.UTF8;

			this.buffer = new byte[length];
			Buffer.BlockCopy(buffer, offset, this.buffer, 0, length);
		}

		/// <summary>
		/// Creates a new Data object using the given buffer.
		/// If the copy flag is set, this method will create a new buffer, and copy the data from the given buffer into it.
		/// Thus changes to the given buffer will not affect this Data object.
		/// Otherwise the new Data object will simply form a wrapper around the given data (without copying anything).
		/// </summary>
		/// <param name="buffer">
		///		Byte array to use for underlying data.
		/// </param>
		/// <param name="copy">
		///		Whether or not to copy data from the given buffer into a new buffer.
		///	</param>
		public Data(byte[] buffer, bool copy)
		{
			// By default we use UTF8
			enc = Encoding.UTF8;

			if (copy)
			{
				this.buffer = new byte[buffer.Length];
				Buffer.BlockCopy(buffer, 0, this.buffer, 0, buffer.Length);
			}
			else
			{
				this.buffer = buffer;
			}
		}

		/// <summary>
		/// Creates a new Data object using the data.
		/// The data is not copied.
		/// That is, the new Data object is simply a wrapper around that same data.
		/// </summary>
		/// <param name="data">
		///		Data to use as underlying data.
		///	</param>
		public Data(Data data) : this(data.ByteArray, false)
		{
			// Nothing to do here
		}

		/// <summary>
		/// Creates a new Data object using a specified subset of the given data.
		/// The data must necessarily be copied (otherwise it would be unsafe).
		/// </summary>
		/// <param name="data">
        ///		Data to use as underlying data.
		/// </param>
		/// <param name="offset">
		///		The offset within data to start reading from.
		/// </param>
		/// <param name="data">
		///		The amount to read from data.
		/// </param>
		public Data(Data data, int offset, int length) : this(data.ByteArray, offset, length)
		{
			// Nothing to do here
		}

		/// <summary>
		/// Creates a new Data object using the given data.
		/// If the copy flag is set, this method will create a new buffer, and copy the buffer from the given data into it.
		/// Thus changes to the given data will not affect this Data object.
		/// Otherwise the new Data object will simply form a wrapper around the given data (without copying anything).
		/// 
		/// Note: If you pass a Data object which uses an internal stream (IsStream = true), the data is always copied.
		/// </summary>
		/// <param name="data">
        ///		Data to use as underlying data.
		/// </param>
		/// <param name="copy">
		///		Whether or not to copy data from the given buffer into a new buffer.
		///	</param>
		public Data(Data data, bool copy) : this(data.ByteArray, copy)
		{
			// Nothing to do here
		}

		/// <summary>
		/// Creates a new Data object wrapping a newly created byte array of the given size.
		/// </summary>
		/// <param name="length">The size to make the underlying byte array.</param>
		public Data(int length) : this((long)length)
		{
			// Nothing to do here
		}

		/// <summary>
		/// Creates a new Data object wrapping a newly created byte array of the given size.
		/// </summary>
		/// <param name="length">The size to make the underlying byte array.</param>
		public Data(long length)
		{
			// By default we use UTF8
			enc = Encoding.UTF8;

			buffer = new byte[length];
		}

		/// <summary>
		/// Creates a new Data object, converting the string using UTF8.
		/// </summary>
		public Data(String str) : this(str, Encoding.UTF8)
		{
			// Nothing to do here
		}

		/// <summary>
		/// Creates a new Data object, converting the string with the given encoding.
		/// </summary>
		public Data(String str, Encoding enc)
		{
			this.enc = enc;
			this.buffer = enc.GetBytes(str);
		}

		public bool IsStream
		{
			get { return false; }
		}

		public int Length
		{
			get { return buffer.Length; }
		}

		public byte[] ByteArray
		{
			get { return buffer; }
		}

		public byte this[int index]
		{
			get { return buffer[index]; }
		}

		public int ReadByteArray(out byte[] result, int offset, int length)
		{
			result = new byte[length];
			
			int available = buffer.Length - offset;
			int realLength = (available < length) ? available : length;

			Buffer.BlockCopy(buffer, offset, result, 0, realLength);
			return realLength;
		}

		/// <summary>
		/// Reads data up to and including the given termination sequence.
		/// Then encodes and returns the resulting data in the set encoding.
		/// This method is a simple extension to LookForTerm.
		/// </summary>
		/// <param name="offset">
		///		The offset to start reading data from the underlying byte array.
		/// </param>
		/// <param name="term">
		///		The termination sequence. Ex "\n", "\r\n", ",", etc.
		/// </param>
		/// <param name="length">
		///		The length of data that was read.
		/// </param>
		/// <returns>
		///		The read data, generated by reading from the offset, up to and including the terminator,
		///		and encoding the result in the currently set encoding.
		///		Null if the terminator is not found.
		///	</returns>
		///	<exception cref="System.ArgumentException">
		///		Thrown when offset >= Data.Length, or if term.Length == 0
		///	</exception>
		///	<exception cref="System.ArgumentNullException">
		///		Thrown when term is null.
		///	</exception>
		public String ReadThroughTerm(int offset, byte[] term, out int length)
		{
			int termStartIndex = LookForTerm(offset, term);

			if (termStartIndex < 0)
			{
				length = 0;
				return null;
			}
			else
			{
				length = termStartIndex + term.Length - offset;
				return enc.GetString(buffer, offset, length);
			}
		}

		/// <summary>
		/// Looks for the given termination sequence in the data, starting from the given offset.
		/// 
		/// Example:
		/// If the underlying data represents the following string:
		/// Host: deusty.com\r\nCheese: Yes Please
		/// 
		/// And this method is called with offset=0, and term="\r\n",
		/// then this method will return 16.
		/// </summary>
		/// <param name="offset">
		///		The offset from which to start looking for the termination sequence.
		/// </param>
		/// <param name="term">
		///		The termination sequence to look for.
		/// </param>
		/// <returns>
		///		Returns the starting position of the given term, if found.
		///		Otherwise, returns -1.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		///		Thrown when offset >= Data.Length, or if term.Length == 0
		///	</exception>
		///	<exception cref="System.ArgumentNullException">
		///		Thrown when term is null.
		///	</exception>
		public int LookForTerm(int offset, byte[] term)
		{
			if (offset >= buffer.Length) throw new ArgumentException("offset is >= Length", "offset");
			
			if (term == null) throw new ArgumentNullException("term");
			if (term.Length == 0) throw new ArgumentException("term.Length == 0", "term");

			// Look for the terminating sequence in the buffer
			int i = offset;
			bool found = false;

			while (i < buffer.Length && !found)
			{
				bool match = i + term.Length < buffer.Length;

				for (int j = 0; match && j < term.Length; j++)
				{
					match = (buffer[i + j] == term[j]);
				}

				if (match)
					found = true;
				else
					i++;
			}

			if (found)
			{
				return i - offset;
			}
			else
			{
				return -1;
			}
		}

		/// <summary>
		/// Reads the entire data into a string.
		/// If an encoding was set in the constructor, this encoding is used.
		/// Otherwise, uses the default encoding (UTF8).
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return enc.GetString(buffer);
		}

		/// <summary>
		/// Reads the entire data into a string using the given encoding.
		/// </summary>
		/// <param name="encoding">
		///		The encoding to use when converting from raw bytes to a string.
		/// </param>
		/// <returns>
		///		A string from the data in the given encoding.
		/// </returns>
		public string ToString(Encoding encoding)
		{
			return encoding.GetString(buffer);
		}

		/// <summary>
		/// Writes the data to the given filepath.
		/// If the file doesn't exist, it is created.
		/// If it does exist, it is overwritten.
		/// </summary>
		/// <param name="filepath">
		///		The filepath (relative or absolute) to write to.
		/// </param>
		/// <returns>
		///		True if the write finished successfully.
		///		False otherwise.
		/// </returns>
		public bool WriteToFile(String filepath)
		{
			Exception e;
			return WriteToFile(filepath, out e);
		}

		/// <summary>
		/// Writes the data to the given filepath.
		/// If the file doesn't exist, it is created.
		/// If it does exist, it is overwritten.
		/// </summary>
		/// <param name="filepath">
		///		The filepath (relative or absolute) to write to.
		/// </param>
		/// <param name="e">
		///		If this method returns false, e will be set to the exception that occurred.
		///		Otherwise e will be set to null.
		/// </param>
		/// <returns>
		///		True if the write finished successfully.
		///		False otherwise.
		/// </returns>
		public bool WriteToFile(String filepath, out Exception e)
		{
			e = null;

			bool result = false;

			FileStream fs = null;
			try
			{
				fs = File.Create(filepath);

				fs.Write(buffer, 0, buffer.Length);

				result = true;
			}
			catch (Exception ex)
			{
				e = ex;
			}
			finally
			{
				if (fs != null) fs.Close();
			}

			return result;
		}
	}

	/// <summary>
	/// Provides a basic changeable IData container for raw or string data.
	/// </summary>
	public class MutableData : Data
	{
		/// <summary>
		/// Creates a new zero-length MutableData object.
		/// </summary>
		public MutableData() : base(new byte[0], false)
		{
			// Nothing to do here
		}

		public MutableData(byte[] buffer) : base(buffer, false)
		{
			// Nothing to do here
		}

		public MutableData(byte[] buffer, int offset, int length) : base(buffer, offset, length)
		{
			// Nothing to do here
		}

		public MutableData(byte[] buffer, bool copy) : base(buffer, copy)
		{
			// Nothing to do here
		}

		public MutableData(Data data) : base(data, false)
		{
			// Nothing to do here
		}

		public MutableData(Data data, int offset, int length) : base(data, offset, length)
		{
			// Nothing to do here
		}

		public MutableData(Data data, bool copy) : base(data, copy)
		{
			// Nothing to do here
		}

		public MutableData(int length) : base(length)
		{
			// Nothing to do here
		}

		public MutableData(long length) : base(length)
		{
			// Nothing to do here
		}

		public MutableData(String str) : base(str, Encoding.UTF8)
		{
			// Nothing to do here
		}

		public MutableData(String str, Encoding enc) : base(str, enc)
		{
			// Nothing to do here
		}

		/// <summary>
		/// Increases the length of the underlying byte array by the given length.
		/// Does nothing if the given length is non-positive.
		/// To truncate data, use the setLength method, or one of the trim methods.
		/// </summary>
		/// <param name="extraLength">
		///		The length in bytes.
		///	</param>
		public void IncreaseLength(int extraLength)
		{
			// Ignore the request if non-positive extra length given
			if (extraLength <= 0) return;

			int newLength = buffer.Length + extraLength;

			byte[] newBuffer = new byte[newLength];

			Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
			buffer = newBuffer;
		}

		/// <summary>
		/// Sets the length of the underlying byte array.
		/// If the given length is the same as the current length, this method does nothing.
		/// If the given length is greater than the current length,
		/// a new bigger array will be created and the bytes will be copied into it.
		/// If the given length is less than the current length,
		/// a new smaller array will be created and the bytes will be copied into it, with excess data being truncated.
		/// </summary>
		/// <param name="length">
		///		The length in bytes.
		/// </param>
		/// <exception cref="System.ArgumentException">
		///		Thrown only if the length parameter is negative.
		///	</exception>
		public void SetLength(int length)
		{
			// Throw exception if negative length given
			if (length < 0) throw new ArgumentException("length must be >= 0", "length");

			// Ignore the request if we're already using the given length
			if (length == buffer.Length) return;

			byte[] newBuffer = new byte[length];

			// Depending on length, we may be increasing or decreasing our buffer
			// If we're increasing it, we need to copy the full buffer
			// If we're decreasing it, we can only copy a portion of the buffer
			int count = (buffer.Length < length) ? buffer.Length : length;

			Buffer.BlockCopy(buffer, 0, newBuffer, 0, count);
			buffer = newBuffer;
		}

		/// <summary>
		/// Trims a given amount from the beginning of the underlying buffer.
		/// </summary>
		/// <param name="length">
		///		The number of bytes to trim.
		///	</param>
		public void TrimStart(int length)
		{
			// Throw exception if negative length given
			if (length < 0) throw new ArgumentException("length must be >= 0", "length");

			// Ignore the request if length is zero
			if (length == 0) return;

			// Make sure we don't try to trim more than what exists
			int offset = (length < buffer.Length) ? length : buffer.Length;
			int count = buffer.Length - offset;

			byte[] newBuffer = new byte[count];

			Buffer.BlockCopy(buffer, offset, newBuffer, 0, count);
			buffer = newBuffer;
		}

		/// <summary>
		/// Trims a given amount from the end of the underlying buffer.
		/// </summary>
		/// <param name="length">
		///		The number of bytes to trim.
		///	</param>
		public void TrimEnd(int length)
		{
			// Throw exception if negative length given
			if (length < 0) throw new ArgumentException("length must be >= 0", "length");

			// Ignore the request if length is zero
			if (length == 0) return;

			// Make sure we don't try to trim more than what exists
			int count = (length < buffer.Length) ? length : buffer.Length;
			int offset = buffer.Length - count;

			byte[] newBuffer = new byte[count];

			Buffer.BlockCopy(buffer, offset, newBuffer, 0, count);
			buffer = newBuffer;
		}

		/// <summary>
		/// This method automatically increases the length of the data by the proper length,
		/// and copies the bytes from the given data object into the mutable data array.
		/// </summary>
		/// <param name="data">
		///		An IData object to copy bytes from.
		///	</param>
		public void AppendData(IData data)
		{
			// We're not going to bother checking to see if data is null.
			// The NullReferenceException will automatically get thrown for us if it is.

			AppendData(data.ByteArray);
		}

		/// <summary>
		/// Reads from the given data, starting at the given offset and reading the given length,
		/// and appends the read data to the underlying buffer.
		/// The underlying buffer length is automatically increased as needed.
		/// 
		/// This method properly handles reading from stream data (data.IsStream == true).
		/// </summary>
		/// <param name="data">
		///		The data to append to the end of the underlying buffer.
		/// </param>
		/// <param name="offset">
		///		The offset from which to start copying from the given data.
		/// </param>
		/// <param name="length">
		///		The amount to copy from the given data.
		/// </param>
		public void AppendData(IData data, int offset, int length)
		{
			if (data.IsStream)
			{
				int amountRead = 0;
				do
				{
					byte[] byteArray;
					amountRead += data.ReadByteArray(out byteArray, offset, length);

					AppendData(byteArray);

				}while(amountRead < length);
			}
			else
			{
				AppendData(data.ByteArray, offset, length);
			}
		}

		/// <summary>
		/// This method automatically increases the length of the data by the proper length,
		/// and copies the data from the given byte array into the mutable data array.
		/// </summary>
		/// <param name="byteArray">
		///		The array of bytes to append to the end of the current array.
		///	</param>
		public void AppendData(byte[] byteArray)
		{
			AppendData(byteArray, 0, byteArray.Length);
		}

		/// <summary>
		/// This method automatically increases the length of the data by the proper length,
		/// and copies the data from the given byte array into the underlying buffer.
		/// The data is copied starting at the given offset up to the given length.
		/// </summary>
		/// <param name="byteArray">
		///		The array of bytes to append to the end of the underlying buffer.
		/// </param>
		/// <param name="offset">
		///		The offset from which to start copying data from the given byteArray.
		/// </param>
		/// <param name="length">
		///		The amount of data to copy from the given byteArray.
		/// </param>
		public void AppendData(byte[] byteArray, int offset, int length)
		{
			byte[] newBuffer = new byte[buffer.Length + length];

			Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
			Buffer.BlockCopy(byteArray, offset, newBuffer, buffer.Length, length);
			buffer = newBuffer;
		}
	}

	/// <summary>
	/// The FileData class provides a wrapper around a FileStream.
	/// </summary>
	public class FileData : IData, IDisposable
	{
		/// <summary>
		/// Reads the entire file, and stores the result in a Data object wrapping the read bytes.
		/// For quickly reading small files, this method may be preferred to creating FileData objects,
		/// as those should be properly disposed.
		/// </summary>
		/// <param name="filePath">
		///		Relative or absolute path to file.
		/// </param>
		/// <returns>
		///		A regular Data object, which wraps the read bytes from the file.
		/// </returns>
		public static Data ReadFileData(string filePath)
		{
			Data result = null;
			FileStream fs = null;

			try
			{
				fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				result = new Data(fs.Length);

				int amountRead = fs.Read(result.ByteArray, 0, result.Length);
				int totalAmountRead = amountRead;

				while((amountRead > 0) && (totalAmountRead < result.Length))
				{
					amountRead = fs.Read(result.ByteArray, totalAmountRead, result.Length - totalAmountRead);
					totalAmountRead += amountRead;
				}

				if (totalAmountRead < result.Length)
				{
					throw new Exception("Unable to read file");
				}
			}
			catch
			{
				result = null;
			}
			finally
			{
				if(fs != null) fs.Close();
			}

			return result;
		}

		protected Stream stream;

		/// <summary>
		/// Creates a Data wrapper around a FileStream.
		/// You should dispose of this object when you're done using it.
		/// </summary>
		/// <param name="filePath">
		///		Relative or absolute path to file.
		///	</param>
		public FileData(string filePath)
		{
			stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		}

		/// <summary>
		/// Creates a Data wrapper around a FileStream.
		/// You should dispose of this object when you're done using it.
		/// </summary>
		/// <param name="filePath">
		///		Relative or absolute path to file.
		/// </param>
		/// <param name="sequential">
		///		If true, uses FileOptions.SequentialScan:
		///		Indicates that the file is to be accessed sequentially from beginning to end.
		///		The system can use this as a hint to optimize file caching.
		///		If an application moves the file pointer for random access, optimum caching may not occur;
		///		however, correct operation is still guaranteed.
		///		
		///		Specifying this flag can increase performance for applications that read large files using sequential access.
		///		Performance gains can be even more noticeable for applications that read large files mostly sequentially,
		///		but occasionally skip over small ranges of bytes.
		/// </param>
		public FileData(string filePath, bool sequential)
		{
			// From the FileStream documentation:
			// The default buffer size is 8192 bytes (8 KB).

			if(sequential)
				stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.SequentialScan);
			else
				stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		}

		public bool IsStream
		{
			get { return true; }
		}

		public int Length
		{
			get { return (int)stream.Length; }
		}

		public byte[] ByteArray
		{
			get
			{
				byte[] buffer = new byte[stream.Length];

				if (stream.Position != 0)
				{
					stream.Seek(0, SeekOrigin.Begin);
				}
				stream.Read(buffer, 0, (int)stream.Length);
				return buffer;
			}
		}

		public byte this[int index]
		{
			get
			{
				byte[] buffer = new byte[1];

				if (stream.Position != index)
				{
					stream.Seek(index, SeekOrigin.Begin);
				}
				stream.Read(buffer, 0, 1);
				return buffer[0];
			}
		}

		public int ReadByteArray(out byte[] result, int offset, int length)
		{
			result = new byte[length];

			if (stream.Position != offset)
			{
				stream.Seek(offset, SeekOrigin.Begin);
			}
			return stream.Read(result, 0, length);
		}

		public override string ToString()
		{
			return this.ToString(Encoding.UTF8);
		}

		public string ToString(Encoding encoding)
		{
			return encoding.GetString(this.ByteArray);
		}

		public void Dispose()
		{
			stream.Close();
			stream = null;
		}
	}

	/// <summary>
	/// Reads a file from disk, and compresses it, storing the compressed version in an IData container.
	/// </summary>
	public class GZipFileData : IData, IDisposable
	{
		protected Stream stream;

		public GZipFileData(string fileName)
		{
			Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			byte[] buffer = new byte[fileStream.Length];

			// Read the file to ensure it is readable.
			int count = fileStream.Read(buffer, 0, buffer.Length);

			if (count != buffer.Length)
			{
				fileStream.Close();
				Console.WriteLine("Test Failed: Unable to read data from file");
				return;
			}
			fileStream.Close();

			stream = new MemoryStream();

			// Use the newly created memory stream for the compressed data.
			GZipStream gzipStream = new GZipStream(stream, CompressionMode.Compress, true);
			gzipStream.Write(buffer, 0, buffer.Length);

			// Close the stream.
			gzipStream.Close();
			Console.WriteLine("GZipFileData: Original size:{0}, Compressed size:{1}", buffer.Length, stream.Length);
		}

		public bool IsStream
		{
			get { return true; }
		}

		public int Length
		{
			get { return (int)stream.Length; }
		}

		public byte[] ByteArray
		{
			get
			{
				byte[] buffer = new byte[stream.Length];

				if (stream.Position != 0)
				{
					stream.Seek(0, SeekOrigin.Begin);
				}
				stream.Read(buffer, 0, (int)stream.Length);
				return buffer;
			}
		}

		public byte[] byteArray()
		{
			byte[] buffer = new byte[stream.Length];

			if (stream.Position != 0)
			{
				stream.Seek(0, SeekOrigin.Begin);
			}
			stream.Read(buffer, 0, (int)stream.Length);
			return buffer;
		}

		public byte this[int index]
		{
			get
			{
				byte[] buffer = new byte[1];

				if (stream.Position != index)
				{
					stream.Seek(index, SeekOrigin.Begin);
				}
				stream.Read(buffer, 0, 1);
				return buffer[0];
			}
		}

		public int ReadByteArray(out byte[] result, int offset, int length)
		{
			result = new byte[length];

			if (stream.Position != offset)
			{
				stream.Seek(offset, SeekOrigin.Begin);
			}
			return stream.Read(result, 0, length);
		}

		public override string ToString()
		{
			return this.ToString(Encoding.UTF8);
		}

		public string ToString(Encoding encoding)
		{
			return encoding.GetString(this.ByteArray);
		}

		public void Dispose()
		{
			stream.Close();
			stream = null;
		}
	}
}
