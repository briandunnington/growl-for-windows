using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Net;

namespace Growl.AutoUpdate
{
    public class Updater : IDisposable
    {
        private const string UPDATE_FOLDER = "__update";
        private const string MANIFEST_FILE_NAME = "update.manifest";

        public event CheckForUpdateCompleteEventHandler CheckForUpdateComplete;
        public event ProgressChangedEventHandler DownloadProgressChanged;
        public event EventHandler DownloadComplete;
        public event UpdateErrorEventHandler UpdateError;

        private Growl.CoreLibrary.WebClientEx checker;
        private string appPath;
        private string manifestFile;
        private string currentVersion;
        private string updateLocation;
        private Manifest updatedManifest;
        private bool updateAvailable;
        private string updateTempFolder;
        private bool disposed;

        public Updater(string appPath)
            : this(appPath, null, null)
        {
            ReadCurrentManifest();
        }

        private Updater(string appPath, string currentVersion, string updateLocation)
        {
            this.appPath = appPath;
            this.manifestFile = Path.Combine(this.appPath, MANIFEST_FILE_NAME);
            this.currentVersion = currentVersion;
            this.updateLocation = updateLocation;
            this.updateTempFolder = Path.Combine(Utility.UserSettingFolder, UPDATE_FOLDER);

            this.checker = new Growl.CoreLibrary.WebClientEx();
            checker.Headers.Add("User-Agent", "Element.AutoUpdate.Updater");
            checker.DownloadStringCompleted += new DownloadStringCompletedEventHandler(checker_DownloadStringCompleted);
        }

        public string CurrentVersion
        {
            get
            {
                return this.currentVersion;
            }
        }

        public void CheckForUpdate(bool userInitiated)
        {
            if (!String.IsNullOrEmpty(this.updateLocation))
            {
                string qs = "v=" + this.currentVersion;
                UriBuilder ub = new UriBuilder(this.updateLocation);
                if (ub.Query.Length > 1)
                    qs = ub.Query.Substring(1) + qs;
                ub.Query = qs;

                if (checker.IsBusy) checker.CancelAsync();
                checker.DownloadStringAsync(ub.Uri, userInitiated);
            }
        }

        void checker_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                CheckForUpdateCompleteEventArgs args = null;
                bool userInitiated = (bool)e.UserState;

                if (e.Error == null)
                {
                    this.updatedManifest = Manifest.Parse(e.Result);
                    if (this.updatedManifest != null)
                    {
                        args = new CheckForUpdateCompleteEventArgs(this.updatedManifest, this.currentVersion, userInitiated, null);
                        this.updateAvailable = args.UpdateAvailable;
                    }
                }

                // this could be because e.Error != null or because the Manifest.Parse() failed
                if(args == null)
                {
                    UpdateErrorEventArgs errorArgs = new UpdateErrorEventArgs(e.Error, "Growl was unable to determine if a newer version is available. Please try again later.");
                    args = new CheckForUpdateCompleteEventArgs(null, this.currentVersion, userInitiated, errorArgs);
                }

                this.OnCheckForUpdateComplete(args);
            }
        }

        public void Update()
        {
            if (this.updatedManifest == null)
                throw new UpdateException("You must call CheckForUpdate first to determine if an update is available.");

            if (this.updateAvailable)
            {
                if (Directory.Exists(this.updateTempFolder))
                    Directory.Delete(updateTempFolder, true);
                Directory.CreateDirectory(this.updateTempFolder);

                InstallInfo info = new InstallInfo();
                info.ZipFile = Path.Combine(updateTempFolder, "update.zip");
                info.SetupFile = Path.Combine(updateTempFolder, "setup.exe");
                info.Folder = updateTempFolder;

                Growl.CoreLibrary.WebClientEx downloader = new Growl.CoreLibrary.WebClientEx();
                using (downloader)
                {
                    downloader.Headers.Add("User-Agent", "Element.AutoUpdate.Updater");
                    downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                    downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                    downloader.DownloadFileAsync(new Uri(this.updatedManifest.InstallerLocation), info.ZipFile, info);
                }
            }
        }

        void downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressChangedEventArgs args = new ProgressChangedEventArgs(e.ProgressPercentage, null);
            this.OnDownloadProgressChanged(args);
        }

        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                UpdateErrorEventArgs args = new UpdateErrorEventArgs(e.Error, "An error occurred while downloading the necessary update files. Please try again later.");
                this.OnUpdateError(args);
            }
            else if (e.Cancelled)
            {
                UpdateException ex = new UpdateException("Update was cancelled");
                UpdateErrorEventArgs args = new UpdateErrorEventArgs(ex, "Update cancelled.");
                this.OnUpdateError(args);
            }
            else
            {
                this.OnDownloadComplete(EventArgs.Empty);

                // unzip files
                InstallInfo info = (InstallInfo) e.UserState;
                Installation.Unzipper.UnZipFiles(info.ZipFile, info.Folder, false);

                // start the update installer
                string setupFile = info.SetupFile;
                System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo(setupFile);
                System.Diagnostics.Process.Start(si);

                // exit this application
                ApplicationMain.Program.ExitApp();
            }

            Growl.CoreLibrary.WebClientEx downloader = (Growl.CoreLibrary.WebClientEx)sender;
            downloader.DownloadProgressChanged -= new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
            downloader.DownloadFileCompleted -= new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
            downloader.Dispose();
            downloader = null;
        }

        protected void OnCheckForUpdateComplete(CheckForUpdateCompleteEventArgs args)
        {
            if (this.CheckForUpdateComplete != null)
            {
                this.CheckForUpdateComplete(this, args);
            }
        }

        protected void OnDownloadProgressChanged(ProgressChangedEventArgs args)
        {
            if (this.DownloadProgressChanged != null)
            {
                this.DownloadProgressChanged(this, args);
            }
        }

        protected void OnDownloadComplete(EventArgs e)
        {
            if (this.DownloadComplete != null)
            {
                this.DownloadComplete(this, e);
            }
        }

        protected void OnUpdateError(UpdateErrorEventArgs e)
        {
            if (this.UpdateError != null)
            {
                this.UpdateError(this, e);
            }
        }

        private void ReadCurrentManifest()
        {
            bool exists = File.Exists(this.manifestFile);
            if (exists)
            {
                string xml = File.ReadAllText(this.manifestFile);
                Manifest manifest = Manifest.Parse(xml);
                this.currentVersion = manifest.Version;
                this.updateLocation = manifest.UpdateLocation;
                manifest = null;
            }
        }

        private class InstallInfo
        {
            public string ZipFile;
            public string Folder;
            public string SetupFile;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.checker != null)
                    {
                        this.checker.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(checker_DownloadStringCompleted);
                        this.checker.Dispose();
                        this.checker = null;
                    }
                }
                this.disposed = true;
            }
        }

        #endregion
    }
}
