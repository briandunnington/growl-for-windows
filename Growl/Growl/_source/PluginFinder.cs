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

namespace Growl
{
    public class PluginFinder
    {
        const string CURRENTLY_SEARCHING_PLUGIN_PATH = "CURRENTLY_SEARCHING_PLUGIN_PATH";

        public delegate bool TypeFoundDelegate(Type type);

        /// <summary>
        /// Contains a list of assemblies for each display type (used to resolve dependencies in displays)
        /// </summary>
        static Dictionary<string, Dictionary<string, Assembly>> referencedAssemblies = new Dictionary<string, Dictionary<string, Assembly>>();

        static Dictionary<string, Assembly> loadedAssembliesByName = new Dictionary<string, Assembly>();
        static Dictionary<string, Assembly> loadedAssembliesByPath = new Dictionary<string, Assembly>();

        static object locker = new object();


        static PluginFinder()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // get the folder we are currently processing (so we can check there first)
            string folder = (string)AppDomain.CurrentDomain.GetData(CURRENTLY_SEARCHING_PLUGIN_PATH);
            if (!String.IsNullOrEmpty(folder))
            {
                // get the assembly that we are looking for
                Assembly assembly = null;
                Dictionary<string, Assembly> assemblies;
                if (referencedAssemblies.TryGetValue(folder, out assemblies) &&
                    assemblies.TryGetValue(args.Name, out assembly))
                {
                    return assembly;
                }
            }

            if (loadedAssembliesByName.ContainsKey(args.Name))
                return loadedAssembliesByName[args.Name];

            return null;
        }


        public PluginFinder()
        {
        }

        public T Search<T>(string path, TypeFoundDelegate del, List<string> ignoreList) where T : class
        {
            T plugin = null;
            try
            {
                Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
                string[] dlls = Directory.GetFiles(path, "*.dll");
                bool containsValidPlugin = false;

                // since we are using the static AppDomain data and referenceAssemblies collection, we have to lock this section
                lock (locker)
                {
                    // remember which folder we are currently processing
                    AppDomain.CurrentDomain.SetData(CURRENTLY_SEARCHING_PLUGIN_PATH, path);

                    // loop through once and load each file - we have to load all assemblies before trying to instantiate the type
                    for (int d = 0; d < dlls.Length; d++)
                    {
                        string dllFullPath = dlls[d];
                        string dllFileName = Path.GetFileName(dllFullPath).ToLower();

                        bool ignore = false;
                        if (ignoreList != null && ignoreList.Contains(dllFileName)) ignore = true;

                        // dont add the common assemblies to the list so we dont waste time looking through their types
                        if (!ignore)
                        {
                            Assembly assembly = LoadAssembly(dllFullPath);
                            assemblies.Add(assembly.FullName, assembly);
                        }
                    }
                    // remember which assemblies we loaded for this display
                    referencedAssemblies.Add(path, assemblies);

                    // now check each assembly for the required interfaces
                    foreach (Assembly assembly in assemblies.Values)
                    {
                        foreach (Type type in assembly.GetExportedTypes())
                        {
                            bool valid = del(type);

                            if (valid)
                            {
                                if (containsValidPlugin)
                                {
                                    throw new FileLoadException(String.Format("The plugin at '{0}' could not be loaded because it contains more than one entry point.", path));
                                }
                                else
                                {
                                    plugin = CreateInstance(type, BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, new object[] { }) as T;
                                    containsValidPlugin = true;
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                // we will still rethrow this (since it is probably pretty bad and we want the user to know about it),
                // but we want to log that it happened so we know why the app crashed
                Utility.WriteDebugInfo(String.Format("The plugin at '{0}' could not be loaded. {1}", path, ex.ToString()));
                throw;
            }
            finally
            {
                // clean up a bit
                referencedAssemblies.Remove(path);
                AppDomain.CurrentDomain.SetData(CURRENTLY_SEARCHING_PLUGIN_PATH, null);
            }

            return plugin;
        }

        public T Load<T>(PluginInfo pi, List<string> ignoreList) where T : class
        {
            //--DateTime st = DateTime.Now;

            T plugin = null;
            try
            {
                //--Console.WriteLine("PluginFinder.Load() 1: {0}", (DateTime.Now - st).TotalSeconds);

                string[] dlls = Directory.GetFiles(pi.FolderPath, "*.dll");

                // since we are using the static AppDomain data and referenceAssemblies collection, we have to lock this section
                lock (locker)
                {
                    // remember which folder we are currently processing
                    AppDomain.CurrentDomain.SetData(CURRENTLY_SEARCHING_PLUGIN_PATH, pi.FolderPath);

                    // we have to load any referenced assemblies. these include anything
                    // that is not the target assembly or Growl base assemblies
                    Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
                    for (int d = 0; d < dlls.Length; d++)
                    {
                        string dllFullPath = dlls[d];
                        string dllFileName = Path.GetFileName(dllFullPath).ToLower();

                        bool ignore = false;
                        if (ignoreList != null && ignoreList.Contains(dllFileName)) ignore = true;
                        if (dllFullPath == pi.AssemblyPath) ignore = true;

                        // dont add the common assemblies to the list so we dont waste time looking through their types
                        if (!ignore)
                        {
                            Assembly assembly = LoadAssembly(dllFullPath);
                            assemblies.Add(assembly.FullName, assembly);
                        }
                    }
                    // remember which assemblies we loaded for this display
                    referencedAssemblies.Add(pi.FolderPath, assemblies);

                    //--Console.WriteLine("PluginFinder.Load() 2: {0}", (DateTime.Now - st).TotalSeconds);

                    Assembly targetAssembly = LoadAssembly(pi.AssemblyPath);

                    //--Console.WriteLine("PluginFinder.Load() 3: {0}", (DateTime.Now - st).TotalSeconds);

                    // now that all required assemblies are loaded, we can instantiate the correct type
                    Type type = targetAssembly.GetType(pi.TypeName);
                    if (type != null)
                    {
                        plugin = CreateInstance(type, BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, new object[] { }) as T;
                    }

                    //--Console.WriteLine("PluginFinder.Load() 4: {0}", (DateTime.Now - st).TotalSeconds);
                }
            }
            catch (Exception ex)
            {
                // we will still rethrow this (since it is probably pretty bad and we want the user to know about it),
                // but we want to log that it happened so we know why the app crashed
                Utility.WriteDebugInfo(String.Format("The plugin '{0}-{1}' at {2} could not be loaded. {3}", pi.AssemblyName, pi.TypeName, pi.FolderPath, ex.ToString()));
                throw;
            }
            finally
            {
                // clean up a bit
                referencedAssemblies.Remove(pi.FolderPath);
                AppDomain.CurrentDomain.SetData(CURRENTLY_SEARCHING_PLUGIN_PATH, null);
            }

            //--Console.WriteLine("PluginFinder.Load() 5: {0}", (DateTime.Now - st).TotalSeconds);

            return plugin;
        }

        /*
        public T Load<T>(PluginInfo pi, List<string> ignoreList) where T : class
        {
            DateTime st = DateTime.Now;

            T plugin = null;
            try
            {
                Assembly targetAssembly = null;
                Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
                string[] dlls = Directory.GetFiles(pi.FolderPath, "*.dll");

                Console.WriteLine("PluginFinder.Load() 1: {0}", (DateTime.Now - st).TotalSeconds);

                // since we are using the static AppDomain data and referenceAssemblies collection, we have to lock this section
                lock (locker)
                {
                    // remember which folder we are currently processing
                    AppDomain.CurrentDomain.SetData(CURRENTLY_SEARCHING_PLUGIN_PATH, pi.FolderPath);

                    Console.WriteLine("PluginFinder.Load() 2: {0}", (DateTime.Now - st).TotalSeconds);

                    // loop through once and load each file - we have to load all assemblies before trying to instantiate the type
                    for (int d = 0; d < dlls.Length; d++)
                    {
                        string dllFullPath = dlls[d];
                        string dllFileName = Path.GetFileName(dllFullPath).ToLower();

                        bool ignore = false;
                        if (ignoreList != null && ignoreList.Contains(dllFileName)) ignore = true;

                        Console.WriteLine("PluginFinder.Load() 3: {0}", (DateTime.Now - st).TotalSeconds);

                        Assembly assembly = LoadAssembly(dllFullPath);

                        Console.WriteLine("PluginFinder.Load() 4: {0}", (DateTime.Now - st).TotalSeconds);

                        // dont add the common assemblies to the list so we dont waste time looking through their types
                        if (!ignore)
                            assemblies.Add(assembly.FullName, assembly);

                        if (assembly.FullName == pi.AssemblyName) targetAssembly = assembly;
                    }
                    // remember which assemblies we loaded for this display
                    referencedAssemblies.Add(pi.FolderPath, assemblies);

                    Console.WriteLine("PluginFinder.Load() 5: {0}", (DateTime.Now - st).TotalSeconds);

                    // now that all required assemblies are loaded, we can instantiate the correct type
                    Type type = targetAssembly.GetType(pi.TypeName);
                    if (type != null)
                    {
                        plugin = CreateInstance(type, BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, new object[] { }) as T;
                    }

                    Console.WriteLine("PluginFinder.Load() 6: {0}", (DateTime.Now - st).TotalSeconds);
                }
            }
            finally
            {
                // clean up a bit
                referencedAssemblies.Remove(pi.FolderPath);
                AppDomain.CurrentDomain.SetData(CURRENTLY_SEARCHING_PLUGIN_PATH, null);
            }

            Console.WriteLine("PluginFinder.Load() 7: {0}", (DateTime.Now - st).TotalSeconds);

            return plugin;
        }
         * */

        private static Assembly LoadAssembly(string path)
        {
            if (loadedAssembliesByPath.ContainsKey(path))
                return loadedAssembliesByPath[path];
            else
            {
                Assembly assembly = Assembly.LoadFile(path);      // LoadFile means we use the exact .dlls in this folder. LoadFrom could redirect and use previously loaded .dlls (like Growl.CoreLibrary.dll, etc)
                if(!loadedAssembliesByName.ContainsKey(assembly.FullName)) loadedAssembliesByName.Add(assembly.FullName, assembly);
                if(!loadedAssembliesByPath.ContainsKey(path)) loadedAssembliesByPath.Add(path, assembly);
                return assembly;
            }
        }

        private static object CreateInstance(Type type, BindingFlags bindingFlags, object[] constructorParams)
        {
            Assembly owningAssembly = type.Assembly;
            object createdInstance = owningAssembly.CreateInstance(type.FullName, false, bindingFlags, null, constructorParams, null, null);
            if (createdInstance == null)
            {
                throw new ArgumentException(String.Format("Could not create type '{0}'", type.Name));
            }
            return createdInstance;
        }
    }
}
