using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using Microsoft.Win32;

namespace Growl.Installation
{
    [RunInstaller(true)]
    public partial class GrowlInstaller : Installer
    {
        public GrowlInstaller()
        {
            InitializeComponent();
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            // Call the base implementation.
            base.Install(stateSaver);

            try
            {
                // 1. configure protocol handler
                string targetDir = this.Context.Parameters["targetDir"];
                string exePath = String.Format("{0}Growl.exe", targetDir);
                string command = String.Format("\"{0}\" \"%1\"", exePath);
                RegistryKey growlProtocolHandlerKey = Registry.ClassesRoot.CreateSubKey("growl");
                using (growlProtocolHandlerKey)
                {
                    growlProtocolHandlerKey.SetValue("", "URL:Growl protocol");
                    growlProtocolHandlerKey.SetValue("URL Protocol", "");

                    RegistryKey defaultIconKey = growlProtocolHandlerKey.CreateSubKey("DefaultIcon");
                    using (defaultIconKey)
                    {
                        defaultIconKey.SetValue("", exePath);

                        RegistryKey shellKey = growlProtocolHandlerKey.CreateSubKey("shell");
                        using (shellKey)
                        {
                            RegistryKey openKey = shellKey.CreateSubKey("open");
                            using (openKey)
                            {
                                RegistryKey commandKey = openKey.CreateSubKey("command");
                                using (commandKey)
                                {
                                    commandKey.SetValue("", command);
                                }
                            }
                        }
                    }
                }

                // 2. create proxy.config if necessary (but dont overwrite if it already exists)
                string proxyPath = System.IO.Path.Combine(targetDir, "proxy.config");
                if (!System.IO.File.Exists(proxyPath))
                {
                    System.IO.File.WriteAllText(proxyPath, Properties.Resources.proxy);
                }

                // 3. set a registry key that will later tell other software if Growl is installed or not (and where)
                RegistryKey key = Registry.CurrentUser.OpenSubKey(Growl.CoreLibrary.Detector.REGISTRY_KEY, true);
                if (key == null)
                    key = Registry.CurrentUser.CreateSubKey(Growl.CoreLibrary.Detector.REGISTRY_KEY);
                key.SetValue(null, exePath);
            }
            catch
            {
                // dont fail because of this (for now)
            }
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            // Call the base implementation.
            base.Uninstall(savedState);

            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree("growl");
                Registry.CurrentUser.DeleteSubKeyTree(Growl.CoreLibrary.Detector.REGISTRY_KEY);
            }
            catch
            {
                // dont fail if we couldnt remove the registry key
            }
        }
    }
}