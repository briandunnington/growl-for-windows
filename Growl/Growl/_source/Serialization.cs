using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace Growl
{
	/// <summary>
	/// Provides handy tools for serializing and deserializing objects
	/// </summary>
	public sealed class Serialization
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
            string serializedObject = null;
            using (stream)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                formatter = null;
                int length = Convert.ToInt32(stream.Length);
                byte[] buffer = new byte[length];
                stream.Position = 0;
                stream.Read(buffer, 0, length);
                serializedObject = Convert.ToBase64String(buffer);
                stream.Close();
                buffer = null;
            }
            stream = null;
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
            return DeserializeObject(serializedObject, null);
        }

		/// <summary>
		/// Deserializes a string representation of an object back into an
		/// actual object
		/// </summary>
		/// <param name="serializedObject">string representation of the object</param>
		/// <returns>deserialized object</returns>
		public static object DeserializeObject(string serializedObject, LegacyDeserializers.LegacyDeserializationHelper helper)
		{
			// parameter checking
			if(serializedObject == null)
				throw new ArgumentNullException("serializedObject", "DeserializeObject: string cannot be null.");

            object obj = null;
			byte[] bytes = Convert.FromBase64String(serializedObject);

            if (bytes != null && bytes.Length > 0)
            {
                MemoryStream stream = new MemoryStream(bytes);
                using (stream)
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    if (helper != null)
                    {
                        if (helper.Binder != null) formatter.Binder = helper.Binder;
                        if (helper.Surrogate != null) formatter.SurrogateSelector = helper.Surrogate;
                    }
                    
                    obj = formatter.Deserialize(stream);
                    stream.Close();
                    formatter = null;
                }
                stream = null;
            }
            bytes = null;
			return obj;
		}

		# endregion Public Methods
	}
}
