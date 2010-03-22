using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Growl.Destinations;

namespace Growl
{
    [Serializable]
    class KnownDestinationPlatformType : DestinationPlatformType, ISerializable
    {
        public static DestinationPlatformType Windows = new KnownDestinationPlatformType(PLATFORM_WINDOWS, Growl.Properties.Resources.windows);
        public static DestinationPlatformType Mac = new KnownDestinationPlatformType(PLATFORM_MAC, Growl.Properties.Resources.mac);
        public static DestinationPlatformType Linux = new KnownDestinationPlatformType(PLATFORM_LINUX, Growl.Properties.Resources.linux);
        public static DestinationPlatformType Internet = new KnownDestinationPlatformType(PLATFORM_INTERNET, Growl.Properties.Resources.internet);
        public static DestinationPlatformType IPhone = new KnownDestinationPlatformType(PLATFORM_IPHONE, Growl.Properties.Resources.iphone);
        public static DestinationPlatformType Mobile = new KnownDestinationPlatformType(PLATFORM_MOBILE, Growl.Properties.Resources.mobile);
        public static DestinationPlatformType Email = new KnownDestinationPlatformType(PLATFORM_EMAIL, Growl.Properties.Resources.email);
        public static DestinationPlatformType Twitter = new KnownDestinationPlatformType(PLATFORM_TWITTER, Growl.Properties.Resources.twitter);
        public static DestinationPlatformType Rss = new KnownDestinationPlatformType(PLATFORM_RSS, Growl.Properties.Resources.rss);
        public static DestinationPlatformType NotifyIO = new KnownDestinationPlatformType(PLATFORM_NOTIFYIO, Growl.Properties.Resources.notifyio);
        public static DestinationPlatformType Other = DestinationPlatformType.Generic;

        private const string PLATFORM_WINDOWS = "windows";
        private const string PLATFORM_MAC = "mac";
        private const string PLATFORM_LINUX = "linux";
        private const string PLATFORM_INTERNET = "internet";
        private const string PLATFORM_IPHONE = "iphone";
        private const string PLATFORM_MOBILE = "mobile";
        private const string PLATFORM_EMAIL = "email";
        private const string PLATFORM_TWITTER = "twitter";
        private const string PLATFORM_RSS = "rss";
        private const string PLATFORM_NOTIFYIO = "notifyio";

        [NonSerialized]
        private System.Drawing.Image icon;

        private KnownDestinationPlatformType(string platform, System.Drawing.Image icon)
            : base(platform)
        {
            this.icon = icon;
        }

        public override System.Drawing.Image GetIcon()
        {
            return this.icon;
        }

        #region ISerializable Members

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(KnownDestinationPlatformTypeSerializationHelper));
            info.AddValue("platform", this.Name, typeof(string));
        }

        #endregion

        public static DestinationPlatformType FromString(string platform)
        {
            // the passed in value may be a one-word string (usually used with Bonjour discovery),
            // or it may be an Origin-Platform-Name header value (which is not standardized, but more
            // like a User-Agent type value

            if (!String.IsNullOrEmpty(platform))
            {
                // first, check for known values
                switch (platform)
                {
                    case PLATFORM_WINDOWS:
                        return KnownDestinationPlatformType.Windows;
                    case PLATFORM_MAC:
                        return KnownDestinationPlatformType.Mac;
                    case PLATFORM_LINUX:
                        return KnownDestinationPlatformType.Linux;
                    case PLATFORM_INTERNET:
                        return KnownDestinationPlatformType.Internet;
                    case PLATFORM_IPHONE:
                        return KnownDestinationPlatformType.IPhone;
                    case PLATFORM_MOBILE:
                        return KnownDestinationPlatformType.Mobile;
                    case PLATFORM_EMAIL:
                        return KnownDestinationPlatformType.Email;
                    case PLATFORM_TWITTER:
                        return KnownDestinationPlatformType.Twitter;
                    case PLATFORM_RSS:
                        return KnownDestinationPlatformType.Rss;
                    case PLATFORM_NOTIFYIO:
                        return KnownDestinationPlatformType.NotifyIO;
                    default:
                        // if we didnt get a known value, try parsing the string
                        platform = platform.ToLower();
                        if (platform.IndexOf(PLATFORM_WINDOWS) >= 0) return KnownDestinationPlatformType.Windows;
                        else if (platform.IndexOf(PLATFORM_MAC) >= 0) return KnownDestinationPlatformType.Mac;
                        else if (platform.IndexOf(PLATFORM_LINUX) >= 0) return KnownDestinationPlatformType.Linux;
                        else if (platform.IndexOf(PLATFORM_INTERNET) >= 0) return KnownDestinationPlatformType.Internet;
                        else if (platform.IndexOf(PLATFORM_IPHONE) >= 0) return KnownDestinationPlatformType.IPhone;
                        else if (platform.IndexOf(PLATFORM_MOBILE) >= 0) return KnownDestinationPlatformType.Mobile;
                        break;
                }
            }

            // we get here if all else fails
            return KnownDestinationPlatformType.Other;
        }

        [Serializable]
        private class KnownDestinationPlatformTypeSerializationHelper : IObjectReference
        {
            private string platform = null;

            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                return KnownDestinationPlatformType.FromString(platform);
            }

            #endregion
        }
    }
}
