using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace Growl
{
    [Serializable]
    public class ForwardComputerPlatformType : ISerializable
    {
        public static ForwardComputerPlatformType Windows = new ForwardComputerPlatformType(PLATFORM_WINDOWS, global::Growl.Properties.Resources.windows);
        public static ForwardComputerPlatformType Mac = new ForwardComputerPlatformType(PLATFORM_MAC, global::Growl.Properties.Resources.mac);
        public static ForwardComputerPlatformType Linux = new ForwardComputerPlatformType(PLATFORM_LINUX, global::Growl.Properties.Resources.linux);
        public static ForwardComputerPlatformType Internet = new ForwardComputerPlatformType(PLATFORM_INTERNET, global::Growl.Properties.Resources.internet);
        public static ForwardComputerPlatformType IPhone = new ForwardComputerPlatformType(PLATFORM_IPHONE, global::Growl.Properties.Resources.iphone);
        public static ForwardComputerPlatformType Mobile = new ForwardComputerPlatformType(PLATFORM_MOBILE, global::Growl.Properties.Resources.mobile);
        public static ForwardComputerPlatformType Other = new ForwardComputerPlatformType(PLATFORM_OTHER, global::Growl.Properties.Resources.other);

        private const string PLATFORM_WINDOWS = "windows";
        private const string PLATFORM_MAC = "mac";
        private const string PLATFORM_LINUX = "linux";
        private const string PLATFORM_INTERNET = "internet";
        private const string PLATFORM_IPHONE = "iphone";
        private const string PLATFORM_MOBILE = "mobile";
        private const string PLATFORM_OTHER = "other";

        private string platform;
        private Image icon;

        private ForwardComputerPlatformType(string platform, Image icon)
        {
            this.platform = platform;
            this.icon = icon;
        }

        public string Name
        {
            get
            {
                return this.platform;
            }
        }

        public Image Icon
        {
            get
            {
                return this.icon;
            }
        }

        public static ForwardComputerPlatformType FromString(string platform)
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
                        return ForwardComputerPlatformType.Windows;
                    case PLATFORM_MAC:
                        return ForwardComputerPlatformType.Mac;
                    case PLATFORM_LINUX:
                        return ForwardComputerPlatformType.Linux;
                    case PLATFORM_INTERNET:
                        return ForwardComputerPlatformType.Internet;
                    case PLATFORM_IPHONE:
                        return ForwardComputerPlatformType.IPhone;
                    case PLATFORM_MOBILE:
                        return ForwardComputerPlatformType.Mobile;
                    default:
                        // if we didnt get a known value, try parsing the string
                        platform = platform.ToLower();
                        if (platform.IndexOf(PLATFORM_WINDOWS) >= 0) return ForwardComputerPlatformType.Windows;
                        else if (platform.IndexOf(PLATFORM_MAC) >= 0) return ForwardComputerPlatformType.Mac;
                        else if (platform.IndexOf(PLATFORM_LINUX) >= 0) return ForwardComputerPlatformType.Linux;
                        else if (platform.IndexOf(PLATFORM_INTERNET) >= 0) return ForwardComputerPlatformType.Internet;
                        else if (platform.IndexOf(PLATFORM_IPHONE) >= 0) return ForwardComputerPlatformType.IPhone;
                        else if (platform.IndexOf(PLATFORM_MOBILE) >= 0) return ForwardComputerPlatformType.Mobile;
                        break;
                }
            }

            // we get here if all else fails
            return ForwardComputerPlatformType.Other;
        }

        #region ISerializable Members

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(ForwardComputerPlatformTypeSerializationHelper));
            info.AddValue("platform", this.platform, typeof(string));
        }

        #endregion

        [Serializable]
        private class ForwardComputerPlatformTypeSerializationHelper : IObjectReference
        {
            private string platform = null;

            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                return ForwardComputerPlatformType.FromString(this.platform);
            }

            #endregion
        }
    }
}
