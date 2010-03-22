using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using Microsoft.Win32;

namespace Display_Installer
{
    [RunInstaller(true)]
    public class DisplayInstaller : Installer
    {
        public override void Install(IDictionary stateSaver)
        {
            // this is only for debugging - it allows you to attach a debugger to the installer instance
            //System.Diagnostics.Debugger.Launch();

            // make sure to call the base class' Install() method
            base.Install(stateSaver);

            // this is where the user selected to install the files
            string installFolder = Context.Parameters["tempdir"];

            // this is the name of the subfolder used to contain all of your display's files
            string folderName = Context.Parameters["folderName"];

            // detect if/where growl is installed
            Growl.CoreLibrary.Detector detector = new Growl.CoreLibrary.Detector();
            if (detector.IsInstalled)
            {
                // growl was detected - read the Display folder location
                string displayFolder = Path.Combine(detector.DisplaysFolder, folderName);

                // save our display folder information for later (uninstallation)
                stateSaver.Add("DISPLAY_FOLDER", displayFolder);

                // make sure our subfolder exists
                if (!Directory.Exists(displayFolder))
                    Directory.CreateDirectory(displayFolder);

                // copy files from temp location to actual location.
                // (our files were originally installed in a Files subfolder of the path the user selected)
                string sourceFolder = Path.Combine(installFolder, "Files");
                DirectoryInfo d = new DirectoryInfo(sourceFolder);
                FileInfo[] files = d.GetFiles();
                Queue<FileInfo> queue = new Queue<FileInfo>(files);
                foreach (FileInfo file in queue)
                {
                    // move each file from the Files folder to our display folder
                    file.MoveTo(Path.Combine(displayFolder, file.Name));
                }

                // clean up the temporary Files folder
                Directory.Delete(sourceFolder);
            }
            else
            {
                // growl was not detected on this machine
                throw new InstallException("The Growl Display Installer requires that Growl for Windows already be installed on the system.");
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            try
            {
                // make sure to call the base class' Uninstall() method
                base.Uninstall(savedState);

                // delete our display folder and all of its contents
                object obj = savedState["DISPLAY_FOLDER"];
                string displayFolder = obj.ToString();
                Directory.Delete(displayFolder, true);
            }
            catch
            {
                // suppress
            }
        }
    }
}
