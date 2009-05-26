using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Security;
using System.Security.Permissions;
using Growl.DisplayStyle;

namespace Growl
{
    public class DisplayLoader
    {
        private bool containsValidModule;
        private IDisplay module;

        public DisplayLoader(string path)
        {
            string[] assemblies = Directory.GetFiles(path, "*.dll");
            for (int a = 0; a < assemblies.Length; a++)
            {
                Assembly assembly = Assembly.LoadFile(assemblies[a]);   // LoadFile means we use the exact .dlls in this folder. LoadFrom could redirect and use previously loaded .dlls (like Growl.CoreLibrary.dll, etc)
                foreach (Type type in assembly.GetTypes())
                {
                    if (type != typeof(IDisplay) && type != typeof(Display) && type != typeof(VisualDisplay) && typeof(IDisplay).IsAssignableFrom(type))
                    {
                        if (this.containsValidModule)
                        {
                            throw new FileLoadException(String.Format("The display '{0}' could not be loaded because it contains more than one IDisplay entry point."), AppDomain.CurrentDomain.FriendlyName);
                        }
                        else
                        {
                            this.module = (IDisplay) CreateInstance(type, BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, new object[] { });
                            containsValidModule = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Indicates if the assembly contains a valid <see cref="IDisplay"/> module
        /// </summary>
        public bool ContainsValidModule
        {
            get
            {
                return this.containsValidModule;
            }
        }

        /// <summary>
        /// The <see cref="IDisplay"/> entry point of the assembly
        /// </summary>
        public IDisplay Display
        {
            get
            {
                return this.module;
            }
        }

        /// <summary>
        /// The full type name of the <see cref="SettingsPanelBase"/> associated with 
        /// the display
        /// </summary>
        public string SettingsPanelTypeName
        {
            get
            {
                string val = this.Display.SettingsPanel.GetType().FullName;
                //string val = this.Display.GetType().FullName;
                return val;
            }
        }

        /// <summary>
        /// The full assembly name of the  <see cref="SettingsPanelBase"/> associated with 
        /// the display
        /// </summary>
        public string SettingsPanelAssemblyName
        {
            get
            {
                string val = this.Display.SettingsPanel.GetType().Assembly.FullName;
                //string val = this.Display.GetType().Assembly.FullName;
                return val;
            }
        }

        /// <summary>
        /// The file path to the assembly that contains the <see cref="SettingsPanelBase"/> associated with 
        /// the display
        /// </summary>
        public string SettingsPanelAssemblyLocation
        {
            get
            {
                string val = this.Display.SettingsPanel.GetType().Assembly.Location;
                return val;
            }
        }

        private static object CreateInstance(Type type, BindingFlags bindingFlags, object[] constructorParams)
        {
            Assembly owningAssembly = type.Assembly;
            object createdInstance = owningAssembly.CreateInstance(type.FullName, false, bindingFlags, null, constructorParams, null, null) as MarshalByRefObject;
            if (createdInstance == null)
            {
                throw new ArgumentException(String.Format("Type '{0}' must derive from MarshalByRefObject", type.Name));
            }
            return createdInstance;
        }
    }
}
