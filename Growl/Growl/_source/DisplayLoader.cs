using System;
using System.Collections;
using System.Collections.Generic;
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
        const string CURRENTLY_LOADING_DISPLAY_PATH = "CURRENTLY_LOADING_DISPLAY_PATH";

        /// <summary>
        /// Contains a list of assemblies for each display type (used to resolve dependencies in displays)
        /// </summary>
        static Dictionary<string, Dictionary<string, Assembly>> referencedAssemblies = new Dictionary<string, Dictionary<string, Assembly>>();

        /// <summary>
        /// Indicates if the display contains a valid IDisplay module or not
        /// </summary>
        private bool containsValidModule;

        /// <summary>
        /// The IDisplay module
        /// </summary>
        private IDisplay module;


        static DisplayLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // get the folder we are currently processing
            string folder = (string)AppDomain.CurrentDomain.GetData(CURRENTLY_LOADING_DISPLAY_PATH);

            // get the assembly that we are looking for
            Assembly assembly = referencedAssemblies[folder][args.Name];
            return assembly;
        }

        public DisplayLoader(string path)
        {
            // remember which folder we are currently processing
            AppDomain.CurrentDomain.SetData(CURRENTLY_LOADING_DISPLAY_PATH, path);

            Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
            string[] dlls = Directory.GetFiles(path, "*.dll");

            // loop through once and load each file - we have to load all assemblies before trying to do .GetTypes since some types might reside in other assemblies
            for (int d = 0; d < dlls.Length; d++)
            {
                Assembly assembly = Assembly.LoadFile(dlls[d]);   // LoadFile means we use the exact .dlls in this folder. LoadFrom could redirect and use previously loaded .dlls (like Growl.CoreLibrary.dll, etc)
                assemblies.Add(assembly.FullName, assembly);
            }
            // remember which assemblies we loaded for this display
            referencedAssemblies.Add(path, assemblies);

            // now check each assembly for the required interfaces
            foreach(Assembly assembly in assemblies.Values)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type != typeof(IDisplay) && type != typeof(Display) && type != typeof(VisualDisplay) && !type.IsAbstract && typeof(IDisplay).IsAssignableFrom(type))
                    {
                        if (this.containsValidModule)
                        {
                            throw new FileLoadException(String.Format("The display '{0}' could not be loaded because it contains more than one IDisplay entry point.", AppDomain.CurrentDomain.FriendlyName));
                        }
                        else
                        {
                            this.module = (IDisplay) CreateInstance(type, BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, new object[] { });
                            containsValidModule = true;
                        }
                    }
                }
            }

            // clean up a bit
            referencedAssemblies.Remove(path);
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
