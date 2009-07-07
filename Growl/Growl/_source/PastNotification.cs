using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace Growl
{
    [Serializable]
    public class PastNotification : IComparable
    {
        [NonSerialized]
        private static Dictionary<string, System.Drawing.Image> imageList = new Dictionary<string, System.Drawing.Image>();

        private DateTime timestamp;
        private Growl.DisplayStyle.NotificationLite notification;
        private string imageKey;
        private System.Drawing.Image image;

        public PastNotification(Growl.DisplayStyle.Notification notification, DateTime timestamp)
        {
            Growl.DisplayStyle.NotificationLite notificationLite = Growl.DisplayStyle.NotificationLite.Clone(notification);

            System.Drawing.Image image = null;
            string imageKey = null;
            if (notification.Image != null && notification.Image.IsSet)
            {
                if (notification.Image.IsRawData)
                {
                    byte[] hash = Growl.Connector.Cryptography.ComputeHash(notification.Image.Data.Data, Growl.Connector.Cryptography.HashAlgorithmType.MD5);
                    imageKey = Growl.Connector.Cryptography.HexEncode(hash);
                }
                else
                {
                    imageKey = Growl.Connector.Cryptography.ComputeHash(notification.Image.Url, Growl.Connector.Cryptography.HashAlgorithmType.MD5);
                }

                if (!imageList.ContainsKey(imageKey))
                {
                    lock (imageList)
                    {
                        System.Drawing.Image originalImage = (System.Drawing.Image)notification.Image;
                        if (originalImage != null)
                        {
                            image = GenerateThumbnail(originalImage, 48, 48);
                            imageList.Add(imageKey, image);
                        }
                    }
                }
                else
                {
                    image = imageList[imageKey];
                }
            }

            this.notification = Growl.DisplayStyle.NotificationLite.Clone(notification);
            this.timestamp = timestamp;
            this.imageKey = imageKey;
            this.image = image;
        }

        private System.Drawing.Image GenerateThumbnail(System.Drawing.Image originalImage, int newWidth, int newHeight)
        {
            System.Drawing.Bitmap bmpResized = new System.Drawing.Bitmap(newWidth, newHeight);
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
            return bmpResized;
        }

        public DateTime Timestamp
        {
            get
            {
                return this.timestamp;
            }
        }

        public Growl.DisplayStyle.NotificationLite Notification
        {
            get
            {
                return this.notification;
            }
        }

        public bool HasImage
        {
            get
            {
                return (this.Image != null);
            }
        }

        public string ImageKey
        {
            get
            {
                return this.imageKey;
            }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return this.image;
            }
        }

        internal void LinkImage()
        {
            // add for later use
            if (!imageList.ContainsKey(this.imageKey))
                imageList.Add(this.imageKey, this.image);
        }

        internal static void ClearImageList()
        {
            imageList.Clear();
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            PastNotification pn = obj as PastNotification;
            if (obj == null || pn == null) return -1;
            return -this.Timestamp.CompareTo(pn.Timestamp);
        }

        #endregion
    }

    public class PastNotificationComparer : System.Collections.Generic.IComparer<PastNotification>
    {
        #region IComparer<PastNotification> Members

        public int Compare(PastNotification x, PastNotification y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.Timestamp.CompareTo(y.Timestamp);
        }

        #endregion
    }
}
