using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    [Serializable]
    public class PluginInfo
    {
        private string folderPath;
        private string assemblyPath;
        private string assemblyName;
        private string typeName;

        public PluginInfo(string folderPath, Type type)
        {
            this.folderPath = folderPath;
            this.assemblyPath = type.Assembly.Location;
            this.assemblyName = type.Assembly.FullName;
            this.typeName = type.FullName;
        }

        public string FolderPath
        {
            get
            {
                return this.folderPath;
            }
        }

        public string AssemblyPath
        {
            get
            {
                return this.assemblyPath;
            }
        }

        public string AssemblyName
        {
            get
            {
                return this.assemblyName;
            }
        }

        public string TypeName
        {
            get
            {
                return this.typeName;
            }
        }
    }
}
