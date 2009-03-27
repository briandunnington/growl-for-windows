using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Contains information about a notification image
    /// </summary>
    [Serializable]
    public class ImageData
    {
        /// <summary>
        /// The file path or url of the image
        /// </summary>
        private string url;

        /// <summary>
        /// The binary data of the resource
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Creates a new instance of the <see cref="ImageData"/> class,
        /// using a URL as the resource.
        /// </summary>
        /// <param name="url">The fully qualified url</param>
        private ImageData(string url)
        {
            this.Url = url;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ImageData"/> class,
        /// using binary data as the resource.
        /// </summary>
        /// <param name="data"></param>
        private ImageData(byte[] data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Gets or sets the URL value of this resource
        /// </summary>
        /// <remarks>
        /// If this resource is a binary resource, this property will return <c>null</c>.
        /// </remarks>
        /// <value>
        /// Fully qualified URL. Example: http://www.domain.com/image.png
        /// </value>
        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
                this.data = null;
            }
        }

        /// <summary>
        /// Gets or sets the binary data of this resource
        /// </summary>
        /// <remarks>
        /// If this resource is a URL resource, this property will return <c>null</c>.
        /// </remarks>
        /// <value>
        /// <see cref="byte"/> array
        /// </value>
        public byte[] Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
                this.url = null;
            }
        }

        /// <summary>
        /// Indicates if this resource contains either binary data or a url (as opposed to neither)
        /// </summary>
        public bool IsSet
        {
            get
            {
                return (this.IsRawData || this.IsUrl);
            }
        }

        /// <summary>
        /// Indicates if this resource contains binary data (as opposed to being a URL pointer)
        /// </summary>
        /// <remarks>
        /// If both a URL and binary data are specified for the resource, only the most recently 
        /// set value will be used.
        /// </remarks>
        public bool IsRawData
        {
            get
            {
                return (this.data != null ? true : false);
            }
        }

        /// <summary>
        /// Indicates if this resource is a url (as opposed to being the actual binary data)
        /// </summary>
        /// <remarks>
        /// If both a URL and binary data are specified for the resource, only the most recently 
        /// set value will be used.
        /// </remarks>
        public bool IsUrl
        {
            get
            {
                return (!String.IsNullOrEmpty(this.url) ? true : false);
            }
        }

        /// <summary>
        /// Returns the string representation of the resource
        /// </summary>
        /// <returns>
        /// If this is a URL resource, return the URL.
        /// If this is a binary resource, returns "{data}".
        /// </returns>
        public override string ToString()
        {
            if (this.IsRawData)
                return "{data}";
            else
                return this.Url;
        }

        /// <summary>
        /// Converts the value of a string URL to a <see cref="ImageData"/>
        /// </summary>
        /// <param name="val">The URL</param>
        /// <returns><see cref="ImageData"/></returns>
        static public implicit operator ImageData(string val)
        {
            return new ImageData(val);
        }

        /// <summary>
        /// Converts the value of a byte array to a <see cref="ImageData"/>
        /// </summary>
        /// <param name="data"><see cref="byte"/> array</param>
        /// <returns><see cref="ImageData"/></returns>
        static public implicit operator ImageData(byte[] data)
        {
            return new ImageData(data);
        }

        /// <summary>
        /// Converts a <see cref="ImageData"/> to a string URL
        /// </summary>
        /// <param name="imageData"><see cref="ImageData"/></param>
        /// <returns>string URL</returns>
        static public implicit operator string(ImageData imageData)
        {
            if (imageData == null) return null;
            return imageData.Url;
        }

        /// <summary>
        /// Converts a <see cref="ImageData"/> to an array of bytes
        /// </summary>
        /// <param name="imageData"><see cref="ImageData"/></param>
        /// <returns><see cref="byte"/> array</returns>
        static public implicit operator byte[](ImageData imageData)
        {
            if (imageData == null) return null;
            return imageData.Data;
        }
    }
}
