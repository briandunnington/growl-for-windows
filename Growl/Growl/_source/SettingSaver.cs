using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace Growl
{
    internal class SettingSaver
    {
        protected string path;
        protected LegacyDeserializers.LegacyDeserializationHelper helper;

        public SettingSaver(string fileName)
            : this(fileName, null)
        {
        }

        public SettingSaver(string fileName, LegacyDeserializers.LegacyDeserializationHelper helper)
        {
            this.path = GetPath(fileName);
            this.helper = helper;
        }

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public void Save(object settings)
        {
            try
            {
                // try to serialize first, before we overwrite the file
                string data = null;
                if(settings != null) data = Serialization.SerializeObject(settings);

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
                    stream.Close();
                    writer = null;
                }
                stream = null;
            }
            catch
            {
                throw;
            }
        }

        public object Load()
        {
            object settings = null;
            try
            {
                string p = this.path;
                bool exists = File.Exists(p);

                if (exists)
                {
                    FileStream stream = new FileStream(p, FileMode.Open, FileAccess.Read);
                    using (stream)
                    {
                        StreamReader reader = new StreamReader(stream);
                        using (reader)
                        {
                            reader.BaseStream.Seek(0, SeekOrigin.Begin);
                            string data = reader.ReadToEnd();
                            reader.Close();

                            settings = Serialization.DeserializeObject(data, this.helper);
                            data = null;
                        }
                        stream.Close();
                        reader = null;
                    }
                    stream = null;
                }
                else
                {
                    Utility.WriteDebugInfo("Settings file '{0}' does not exist.", p);
                }
            }
            catch(Exception ex)
            {
                Utility.WriteDebugInfo("Failed to load settings from '{0}' - {1}", this.path, ex.Message);
            }
            return settings;
        }

        public static string GetPath(string filename)
        {
            return Utility.UserSettingFolder + filename;
        }
    }
}
