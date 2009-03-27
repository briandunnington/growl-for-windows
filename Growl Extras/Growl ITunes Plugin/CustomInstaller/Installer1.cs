using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using Microsoft.Win32;

namespace CustomInstaller
{
    [RunInstaller(true)]
    public partial class Installer1 : Installer
    {
        public Installer1()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);

            string installFolder = Context.Parameters["tempdir"];

            string iTunesPath = null;
            try
            {
                //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\iTunes.exe
                RegistryKey hklm = Registry.LocalMachine;
                RegistryKey itunesKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\iTunes.exe");
                if(itunesKey != null)
                {
                    object obj = itunesKey.GetValue("");
                    if(obj != null)
                    {
                        iTunesPath = obj.ToString();
                    }
                }
            }
            catch
            {
                // suppress
            }

            if(iTunesPath != null)
            {
                string itunesFolder = iTunesPath.ToLower().Replace("itunes.exe", "");
                string pluginsFolder = itunesFolder + @"Plug-ins\";
                string pluginFolder = pluginsFolder + @"GrowlPlugin\";

                savedState.Add("PLUGIN_FOLDER", pluginFolder);

                if (!Directory.Exists(pluginsFolder))
                    Directory.CreateDirectory(pluginsFolder);
                if (!Directory.Exists(pluginFolder))
                    Directory.CreateDirectory(pluginFolder);

                // copy files from temp location to actual location
                string sourceFolder = installFolder + @"Files\";
                DirectoryInfo d = new DirectoryInfo(sourceFolder);
                FileInfo[] files = d.GetFiles();
                Queue<FileInfo> queue = new Queue<FileInfo>(files);
                foreach(FileInfo file in queue)
                {
                    file.MoveTo(pluginFolder + file.Name);
                }
                Directory.Delete(sourceFolder);
            }
            else
            {
                throw new InstallException("The Growl Plug-in for iTunes requires that iTunes already be installed on the computer.");
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            try
            {
                base.Uninstall(savedState);

                object obj = savedState["PLUGIN_FOLDER"];
                string pluginFolder = obj.ToString();
                Directory.Delete(pluginFolder, true);
            }
            catch
            {
                // suppress
            }
        }
    }
}
