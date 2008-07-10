using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace Vortex.Growl.AppBridge
{
	/// <summary>
	/// Provides handy tools for serializing and deserializing objects
	/// </summary>
	public class Serialization
	{
		# region constructors

		/// <summary>
		/// Since this class provides only static methods, the default constructor is
		/// private to prevent instances from being created with "new Serialization()".
		/// </summary>
		private Serialization() {}

		# endregion constructors

		# region Public Methods

		/// <summary>
		/// Serializes an object into a string
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <returns>string representation of the object</returns>
		public static string SerializeObject(object obj)
		{
			// parameter checking
			if(obj == null)
				throw new ArgumentNullException("obj", "SerializeObject: object cannot be null.");

			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, obj);
			int length = Convert.ToInt32(stream.Length);
			byte[] buffer = new byte[length];
			stream.Position = 0;
			stream.Read(buffer, 0, length);
			stream.Close();
			string serializedObject = Convert.ToBase64String(buffer);
			return serializedObject;
		}

		/// <summary>
		/// Deserializes a string representation of an object back into an
		/// actual object
		/// </summary>
		/// <param name="serializedObject">string representation of the object</param>
		/// <returns>deserialized object</returns>
		public static object DeserializeObject(string serializedObject)
		{
			// parameter checking
			if(serializedObject == null)
				throw new ArgumentNullException("serializedObject", "DeserializeObject: string cannot be null.");

			byte[] bytes = Convert.FromBase64String(serializedObject);
			MemoryStream stream = new MemoryStream(bytes);
			BinaryFormatter formatter = new BinaryFormatter();
			object obj = formatter.Deserialize(stream);
			stream.Close();
			return obj;
		}

		# endregion Public Methods
	}
}
