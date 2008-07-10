using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Data;

namespace Vortex.Growl.DisplayStyle
{
	/// <summary>
	/// The remote loader loads assumblies into a remote <see cref="AppDomain"/>
	/// </summary>
    /// <remarks>
    /// This class is used by the Growl program to load displays and is not intended for use
    /// from any application or display code.
    /// </remarks>
	public sealed class RemoteLoader : MarshalByRefObject
	{
		private bool containsValidModule = false;
		private MarshalByRefObject module;

		/// <summary>
		/// Creates a remote assembly loader
		/// </summary>
		public RemoteLoader()
		{
			string[] assemblies = Directory.GetFiles(AppDomain.CurrentDomain.RelativeSearchPath, "*.dll");
            //string[] assemblies = Directory.GetFiles(AppDomain.CurrentDomain.RelativeSearchPath, "*.exe");
			for(int a=0;a<assemblies.Length;a++)
			{
				Assembly assembly = Assembly.LoadFile(assemblies[a]);
				foreach(Type type in assembly.GetTypes())
				{
					if(type != typeof(IDisplay) && type != typeof(Display) && typeof(IDisplay).IsAssignableFrom(type))
					{
						if(this.containsValidModule)
						{
							throw new FileLoadException(String.Format("The display '{0}' could not be loaded because it contains more than one IDisplay entry point."), AppDomain.CurrentDomain.FriendlyName);
						}
						else
						{
							this.module = CreateInstance(type, BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, new object[] {});
							containsValidModule = true;
						}
					}
				}
			}
		}

		public bool ContainsValidModule
		{
			get
			{
				return this.containsValidModule;
			}
		}

		public IDisplay Display
		{
			get
			{
				return (IDisplay) this.module;
			}
		}

        public string SettingsPanelTypeName
        {
            get
            {
                string val = this.Display.SettingsPanel.GetType().FullName;
                //string val = this.Display.GetType().FullName;
                return val;
            }
        }


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
		/// Returns a proxy to an instance of the specified plugin type
		/// </summary>
		/// <param name="typeName">The name of the type to create an instance of</param>
		/// <param name="bindingFlags">The binding flags for the constructor</param>
		/// <param name="constructorParams">The parameters to pass to the constructor</param>
		/// <returns>The constructed object</returns>
		private MarshalByRefObject CreateInstance(Type type, BindingFlags bindingFlags, object[] constructorParams)
		{
			Assembly owningAssembly = type.Assembly;
			MarshalByRefObject createdInstance = owningAssembly.CreateInstance(type.FullName, false, bindingFlags, null,
				constructorParams, null, null) as MarshalByRefObject;
			if (createdInstance == null)
			{
				throw new ArgumentException(String.Format("Type '{0}' must derive from MarshalByRefObject", type.Name));
			}
			return createdInstance;
		}

		public override object InitializeLifetimeService()
		{
			// This lease never expires.
			return null;
		}
	}
}
