using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace Growl
{
    class PastNotificationManager
    {
        private static string historyFolder;

        public static string HistoryFolder
        {
            get
            {
                return historyFolder;
            }
            set
            {
                Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(value);
                historyFolder = value;
            }
        }

        private List<PastNotification> pastNotifications = new List<PastNotification>();

        public void LoadPastNotifications()
        {
            ReloadPastNotifications();
        }

        internal void ReloadPastNotifications()
        {
            if (this.pastNotifications == null) this.pastNotifications = new List<PastNotification>();
            this.pastNotifications.Clear();

            DateTime cutoffTime = DateTime.Now.Date.AddDays(-8); // this needs to change at some point to it is not hard-coded in case the HistoryListView control changes
            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(HistoryFolder);
            System.IO.FileInfo[] files = d.GetFiles("*.notification", System.IO.SearchOption.AllDirectories);

            foreach (System.IO.FileInfo file in files)
            {
                if (file.CreationTime < cutoffTime)
                {
                    file.Delete();
                }
                else
                {
                    string data = System.IO.File.ReadAllText(file.FullName);
                    try
                    {
                        object obj = Serialization.DeserializeObject(data);
                        if (obj != null)
                        {
                            try
                            {
                                PastNotification pn = (PastNotification)obj;
                                this.pastNotifications.Add(pn);
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utility.WriteDebugInfo(String.Format("Deserialization of history item failed. {0} :: {1} :: {2}", ex.Message, ex.StackTrace, data));
                    }
                    data = null;
                }
            }
        }

        internal void ClearHistory()
        {
            DeleteHistory();
            this.PastNotifications.Clear();
        }

        /*
// remove invalid entries //TODO: maybe move this out into some kind of scheduled timer process
DateTime cutoff = DateTime.Now.AddDays(-MAX_NUMBER_OF_DAYS).Date;
foreach (PastNotification pn in invalidNotifications)
{
    if (pn.Timestamp < cutoff)
        this.pastNotifications.Remove(pn);
}
invalidNotifications.Clear();
invalidNotifications = null;
 * */

        public List<PastNotification> PastNotifications
        {
            get
            {
                return this.pastNotifications;
            }
        }

        public static PastNotification Save(Growl.DisplayStyle.Notification notification, string requestID, DateTime timestamp)
        {
            Growl.DisplayStyle.NotificationLite notificationLite = Growl.DisplayStyle.NotificationLite.Clone(notification);
            string path = Growl.CoreLibrary.PathUtility.Combine(historyFolder, Growl.CoreLibrary.PathUtility.GetSafeFolderName(notificationLite.ApplicationName));
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(path);

            // save image data
            string imageFile = null;
            if (notification.Image != null && notification.Image.IsSet)
            {
                System.Drawing.Image image = (System.Drawing.Image)notification.Image;
                using (image)
                {
                    System.Drawing.Image thumbnail = GenerateThumbnail(image, 48, 48);
                    using (thumbnail)
                    {
                        string imagefilename = String.Format(@"{0}.img", requestID);
                        imageFile = Growl.CoreLibrary.PathUtility.Combine(path, imagefilename);

                        thumbnail.Save(imageFile);
                    }
                }
            }

            PastNotification pn = new PastNotification(notificationLite, timestamp, imageFile);

            // save text data
            string filename = String.Format(@"{0}.notification", requestID);
            string filepath = Growl.CoreLibrary.PathUtility.Combine(path, filename);
            System.IO.StreamWriter w = System.IO.File.CreateText(filepath);
            using (w)
            {
                string data = Serialization.SerializeObject(pn);
                w.Write(data);
                data = null;
            }

            return pn;
        }

        public static void DeleteHistory()
        {
            string[] subfolders = System.IO.Directory.GetDirectories(HistoryFolder);
            foreach (string subfolder in subfolders)
            {
                System.IO.Directory.Delete(subfolder, true);
            }
            string[] files = System.IO.Directory.GetFiles(HistoryFolder);
            if (files != null)
            {
                foreach (string file in files)
                {
                    System.IO.File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Gets the specified from the cache
        /// </summary>
        /// <param name="applicationName">The application that owns the resource</param>
        /// <param name="resourceID">The resource ID</param>
        /// <returns>
        /// <see cref="Image"/> if the resource exists in the cache, <c>null</c> otherwise
        /// </returns>
        public static Image GetImage(PastNotification pn)
        {
            Image image = null;
            if (pn != null && pn.HasImage)
            {
                try
                {
                    image = Growl.CoreLibrary.ImageConverter.ImageFromUrl(pn.ImageFile);
                }
                catch
                {
                }
            }
            return image;
        }

        private static System.Drawing.Image GenerateThumbnail(System.Drawing.Image originalImage, int newWidth, int newHeight)
        {
            System.Drawing.Bitmap bmpResized = new System.Drawing.Bitmap(newWidth, newHeight);
            lock (bmpResized)
            {
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmpResized);
                using (g)
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                    g.DrawImage(
                        originalImage,
                        new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmpResized.Size),
                        new System.Drawing.Rectangle(System.Drawing.Point.Empty, originalImage.Size),
                        System.Drawing.GraphicsUnit.Pixel);
                }
            }
            return bmpResized;
        }
    }
}
