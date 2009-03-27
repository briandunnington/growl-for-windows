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
//using System.Data;

namespace Growl.DisplayStyle
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
		private bool containsValidModule;
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
					if(type != typeof(IDisplay) && type != typeof(Display) && type != typeof(VisualDisplay) && typeof(IDisplay).IsAssignableFrom(type))
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
				return (IDisplay) this.module;
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
		/// Returns a proxy to an instance of the specified plugin type
		/// </summary>
		/// <param name="type">The type to create an instance of</param>
		/// <param name="bindingFlags">The binding flags for the constructor</param>
		/// <param name="constructorParams">The parameters to pass to the constructor</param>
		/// <returns>The constructed object</returns>
		private static MarshalByRefObject CreateInstance(Type type, BindingFlags bindingFlags, object[] constructorParams)
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

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"></see> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"></see> property.
        /// </returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public override object InitializeLifetimeService()
		{
			// This lease never expires.
			return null;
		}
	}
}
