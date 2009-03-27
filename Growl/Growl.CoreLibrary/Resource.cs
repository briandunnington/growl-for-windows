using System;
using System.Drawing;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// Represents a resource that can be passed in a GNTP message.
    /// Resources may be either URLs or actual binary data.
    /// </summary>
    [Serializable]
    public class Resource
    {
        /// <summary>
        /// The url of the resource
        /// </summary>
        private string url;

        /// <summary>
        /// The binary data of the resource
        /// </summary>
        private BinaryData data;

        /// <summary>
        /// The image bytes, regardless of type (url or binary). This is used as sort of a cache for images that need to be downloaded.
        /// </summary>
        private byte[] imageBytes;

        private bool alreadyConvertedResource;

        /// <summary>
        /// Creates a new instance of the <see cref="Resource"/> class,
        /// using a URL as the resource.
        /// </summary>
        /// <param name="url">The fully qualified url</param>
        private Resource(string url)
        {
            this.Url = url;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Resource"/> class,
        /// using binary data as the resource.
        /// </summary>
        /// <param name="data"></param>
        private Resource(BinaryData data)
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
        /// Gets or sets the <see cref="BinaryData"/> of this resource
        /// </summary>
        /// <remarks>
        /// If this resource is a URL resource, this property will return <c>null</c>.
        /// </remarks>
        /// <value>
        /// <see cref="BinaryData"/>
        /// </value>
        public BinaryData Data
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
        /// If this is a binary resource, return the special data pointer.
        /// </returns>
        public override string ToString()
        {
            if (this.IsRawData)
                return this.Data.IDPointer;
            else
                return this.Url;
        }

        /// <summary>
        /// Converts the value of a string URL to a <see cref="Resource"/>
        /// </summary>
        /// <param name="val">The URL</param>
        /// <returns><see cref="Resource"/></returns>
        static public implicit operator Resource(string val)
        {
            return new Resource(val);
        }

        /// <summary>
        /// Converts the value of a <see cref="BinaryData"/> object to a <see cref="Resource"/>
        /// </summary>
        /// <param name="data"><see cref="BinaryData"/></param>
        /// <returns><see cref="Resource"/></returns>
        static public implicit operator Resource(BinaryData data)
        {
            return new Resource(data);
        }

        /// <summary>
        /// Converts the value of a <see cref="Image"/> object to a <see cref="Resource"/>
        /// </summary>
        /// <param name="image"><see cref="Image"/></param>
        /// <returns><see cref="Resource"/></returns>
        static public implicit operator Resource(Image image)
        {
            BinaryData data = new BinaryData(ImageConverter.ImageToBytes(image));
            return new Resource(data);
        }

        /// <summary>
        /// Converts a <see cref="Resource"/> to a string URL
        /// </summary>
        /// <param name="resource"><see cref="Resource"/></param>
        /// <returns>string URL</returns>
        static public implicit operator string(Resource resource)
        {
            if (resource == null) return null;
            return resource.Url;
        }

        /// <summary>
        /// Converts a <see cref="Resource"/> to a <see cref="BinaryData"/>
        /// </summary>
        /// <param name="resource"><see cref="Resource"/></param>
        /// <returns><see cref="BinaryData"/></returns>
        static public implicit operator BinaryData(Resource resource)
        {
            if (resource == null) return null;
            return resource.Data;
        }

        /// <summary>
        /// Converts a <see cref="Resource"/> to a <see cref="Image"/>
        /// </summary>
        /// <param name="resource"><see cref="Resource"/></param>
        /// <returns><see cref="Image"/></returns>
        static public implicit operator Image(Resource resource)
        {
            if (resource != null)
            {
                if (!resource.alreadyConvertedResource)
                {
                    if (resource.IsRawData)
                    {
                        resource.imageBytes = resource.Data.Data;
                    }
                    else if (resource.IsUrl)
                    {
                        System.Drawing.Image image = ImageConverter.ImageFromUrl(resource.Url);
                        resource.imageBytes = ImageConverter.ImageToBytes(image);
                    }
                    resource.alreadyConvertedResource = true;
                }
                return ImageConverter.ImageFromBytes(resource.imageBytes);
            }
            return null;
        }
    }
}
