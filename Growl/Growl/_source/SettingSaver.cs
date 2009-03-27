using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Growl
{
    internal class SettingSaver
    {
        protected string path;
        protected string pathAlt;

        public SettingSaver(string fileName)
        {
            this.path = GetPath(fileName);
            this.pathAlt = GetPathAlt(fileName);
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
                string data = Serialization.SerializeObject(settings);

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
                string p = this.path;
                bool exists = File.Exists(p);
                if (!exists)
                {
                    p = this.pathAlt;
                    exists = File.Exists(p);
                }

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

                            settings = Serialization.DeserializeObject(data);
                        }
                        stream.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Failed to load settings from '{0}' - {1}", this.path, ex.Message));
            }
            return settings;
        }

        public static string GetPath(string filename)
        {
            return Utility.UserSettingFolder + filename;
        }

        public static string GetPathAlt(string filename)
        {
            return Utility.UserSettingFolderBeta + filename;
        }
    }
}
