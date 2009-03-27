using System;
using System.Collections.Generic;
using System.Text;

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
        private bool hasImage;
        private System.Drawing.Image image;

        public PastNotification(Growl.DisplayStyle.Notification notification, DateTime timestamp)
        {
            if (notification.Image != null && notification.Image.IsSet)
            {
                this.hasImage = true;
                if (notification.Image.IsRawData)
                {
                    byte[] hash = Growl.Connector.Cryptography.ComputeHash(notification.Image.Data.Data, Growl.Connector.Cryptography.HashAlgorithmType.MD5);
                    this.imageKey = Growl.Connector.Cryptography.HexEncode(hash);
                }
                else
                {
                    this.imageKey = Growl.Connector.Cryptography.ComputeHash(notification.Image.Url, Growl.Connector.Cryptography.HashAlgorithmType.MD5);
                }

                if (!imageList.ContainsKey(this.imageKey))
                {
                    System.Drawing.Image image = (System.Drawing.Image)notification.Image;
                    if (image != null)
                    {
                        System.Drawing.Image thumb = GenerateThumbnail(image, 48, 48);
                        imageList.Add(this.imageKey, thumb);
                    }
                }

                if(imageList.ContainsKey(this.imageKey))
                    this.image = imageList[this.imageKey];
            }

            this.timestamp = timestamp;
            this.notification = Growl.DisplayStyle.NotificationLite.Clone(notification);
        }

        private System.Drawing.Image GenerateThumbnail(System.Drawing.Image originalImage, int newWidth, int newHeight)
        {
            // superior image quality
            System.Drawing.Bitmap bmpResized = new System.Drawing.Bitmap(newWidth, newHeight);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmpResized);
            using (g)
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
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
                return this.hasImage;
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
