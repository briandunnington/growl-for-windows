using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace Vortex.Growl.DisplayStyle
{
    /// <summary>
    /// Provides a means to persist user settings to disk by serializing the object
    /// to a file.
    /// </summary>
    /// <remarks>
    /// This class is provided as a convenience to display developers who inherit from
    /// the <see cref="Display"/> class. If you would like to persist your display settings
    /// in another manner, feel free to do so.
    /// </remarks>
    internal class SettingSaver
    {
        /// <summary>
        /// The path to the file containing the serialized data.
        /// </summary>
        protected string path;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="filePath">The path to the folder where the file will be saved.</param>
        /// <param name="fileName">The name of the file</param>
        public SettingSaver(string filePath, string fileName)
        {
            this.path = filePath + fileName;
        }

        /// <summary>
        /// The path to the file containing the serialized data.
        /// </summary>
        public string Path
        {
            get
            {
                return this.path;
            }
        }

        /// <summary>
        /// Serializes all of the data in <paramref name="settings"/> and writes the data to the file
        /// specified in the <see cref="Path"/>.
        /// </summary>
        /// <param name="settings">The object containing the settings to serialize.</param>
        public void Save(object settings)
        {
            try
            {
                // try to serialize first, before we overwrite the file
                string data = SerializeObject(settings);

                FileStream stream = new FileStream(this.path, FileMode.Create, FileAccess.Write);
                using (stream)
                {
                    StreamWriter writer = new StreamWriter(stream);
                    using (writer)
                    {
                        writer.Flush();
                        writer.BaseStream.Seek(0, SeekOrigin.Begin);
                        writer.Write(data);
                        writer.Flush();
                        writer.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deserializes the data from the file on disk and returns the reconstituted object.
        /// </summary>
        /// <returns>Object containing the original settings that were persisted</returns>
        public object Load()
        {
            object settings = null;
            try
            {
                FileStream stream = new FileStream(this.path, FileMode.Open, FileAccess.Read);
                using (stream)
                {
                    StreamReader reader = new StreamReader(stream);
                    using (reader)
                    {
                        reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        string data = reader.ReadToEnd();

                        settings = DeserializeObject(data);
                    }
                }
            }
            catch
            {
                Console.WriteLine("bad");
            }
            return settings;
        }

        /// <summary>
        /// Serializes an object into a string
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>string representation of the object</returns>
        private static string SerializeObject(object obj)
        {
            // parameter checking
            if (obj == null)
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
        private static object DeserializeObject(string serializedObject)
        {
            // parameter checking
            if (serializedObject == null)
                throw new ArgumentNullException("serializedObject", "DeserializeObject: string cannot be null.");

            byte[] bytes = Convert.FromBase64String(serializedObject);
            MemoryStream stream = new MemoryStream(bytes);
            BinaryFormatter formatter = new BinaryFormatter();
            object obj = formatter.Deserialize(stream);
            stream.Close();
            return obj;
        }

    }
}
