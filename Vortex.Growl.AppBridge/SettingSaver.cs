using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Vortex.Growl.AppBridge
{
    internal class SettingSaver
    {
        protected string path;

        public SettingSaver(string fileName)
        {
            this.path = Utility.UserSettingFolder + fileName;
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
                FileStream stream = new FileStream(this.path, FileMode.Create, FileAccess.Write);
                using (stream)
                {
                    StreamWriter writer = new StreamWriter(stream);
                    using (writer)
                    {
                        writer.Flush();
                        writer.BaseStream.Seek(0, SeekOrigin.Begin);

                        string data = Serialization.SerializeObject(settings);
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

                        settings = Serialization.DeserializeObject(data);
                    }
                }
            }
            catch
            {
                Console.WriteLine("bad");
            }
            return settings;
        } 
    }
}
