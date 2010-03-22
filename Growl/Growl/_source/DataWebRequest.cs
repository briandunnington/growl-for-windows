using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Cache;
using System.Net.Mime;
using System.IO;

namespace Growl
{
    public class DataWebRequestFactory : IWebRequestCreate
    {
        #region IWebRequestCreate Members

        public WebRequest Create(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (uri.Scheme != "data") throw new ArgumentException("Unrecognized scheme: " + uri.Scheme);

            ParseResult pr = ParseFromPathAndQuery(Uri.UnescapeDataString(uri.PathAndQuery));
            return new DataWebRequest(uri, pr.Data, pr.ContentType);
        }

        #endregion

        private ParseResult ParseFromPathAndQuery(String paq)
        {
            ParseResult pr;
            Int32 firstCommaIdx = paq.IndexOf(',');
            String pre = paq.Substring(0, Math.Max(0, firstCommaIdx));
            Boolean b64 = pre.EndsWith(";base64");
            String typeStr = pre.Substring(0, pre.Length - (b64 ? ";base64".Length : 0));
            pr.ContentType = new ContentType(String.IsNullOrEmpty(typeStr) ? DataWebRequest.DEFAULT_MEDIATYPE : typeStr);
            String strdata = paq.Substring(firstCommaIdx + 1);
            pr.Data = b64 ? Convert.FromBase64String(strdata) : Encoding.ASCII.GetBytes(strdata);
            return pr;
        }

        private struct ParseResult
        {
            public Byte[] Data;
            public ContentType ContentType;
        }
    }

    [Serializable]
    public class DataWebRequest : WebRequest
    {
        public static readonly string DEFAULT_MEDIATYPE = "text/plain;charset=US-ASCII";

        private readonly ContentType contentType;
        private readonly Byte[] data;
        private readonly Uri uri;

        /// <summary>
        /// Registers DataWebRequest with the System.Net.WebRequest infrastructure
        /// via the <see cref="WebRequest.RegisterPrefix"/> static method. By default,
        /// <see cref="RelaxedDataWebRequestDelegate"/> will be used for the delegated
        /// methods and properties.
        /// </summary>
        public static void Register()
        {
            RegisterPrefix("data:", new DataWebRequestFactory());
        }

        internal DataWebRequest(Uri uri, Byte[] data, ContentType contentType)
        {
            //only called internally (from factory) so we trust the params to be valid
            this.uri = uri;
            this.data = data;
            this.contentType = contentType;
        }

        public override Uri RequestUri { get { return uri; } }

        public override WebResponse GetResponse()
        {
            return new DataWebResponse(uri, contentType, data);
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            //todo
            return base.BeginGetResponse(callback, state);
        }

        public override void Abort() { }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            return GetResponse();
        }

        public override Int64 ContentLength
        {
            get { return 0; }
            set { /* do nothing */ }
        }

        public override String ContentType
        {
            get { return String.Empty; }
            set { /* do nothing */ }
        }

        public override Int32 Timeout
        {
            get { return 0; }
            set { /* do nothing */ }
        }

        public override Stream GetRequestStream()
        {
            return new NullStream();
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            //todo
            return base.BeginGetRequestStream(callback, state);
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return GetRequestStream();
        }

        public override RequestCachePolicy CachePolicy
        {
            get { return WebRequest.DefaultCachePolicy; }
            set { /* do nothing */ }
        }

        public override String Method
        {
            get { return String.Empty; }
            set { /* do nothing */ }
        }

        public override String ConnectionGroupName
        {
            get { return String.Empty; }
            set { /* do nothing */ }
        }

        public override WebHeaderCollection Headers
        {
            get { return new WebHeaderCollection(); }
            set { /* do nothing */ }
        }

        public override ICredentials Credentials
        {
            get { return null; }
            set { /* do nothing */ }
        }

        public override Boolean UseDefaultCredentials
        {
            get { return false; }
            set { /* do nothing */ }
        }

        public override IWebProxy Proxy
        {
            get { return WebRequest.DefaultWebProxy; }
            set { /* do nothing */ }
        }

        public override Boolean PreAuthenticate
        {
            get { return false; }
            set { /* do nothing */ }
        }
    }

    [Serializable]
    public class DataWebResponse : WebResponse
    {
        private readonly ContentType contenttype;
        private readonly Byte[] data;
        private readonly Uri uri;

        internal DataWebResponse(Uri uri, ContentType contenttype, Byte[] data)
        {
            this.uri = uri;
            this.data = data;
            this.contenttype = contenttype;
        }

        public override String ContentType
        {
            get { return contenttype.ToString(); }
            set { throw new InvalidOperationException(); }
        }

        public override WebHeaderCollection Headers
        {
            get { return new WebHeaderCollection(); }
        }

        public override Uri ResponseUri
        {
            get { return uri; }
        }

        public override bool IsFromCache
        {
            get { return false; }
        }

        public override Int64 ContentLength
        {
            get { return data.Length; }
            set { throw new InvalidOperationException(); }
        }

        public override bool IsMutuallyAuthenticated
        {
            get { return false; }
        }

        public override Stream GetResponseStream()
        {
            return new MemoryStream(data);
        }
    }

    internal class NullStream : Stream
    {
        public override void Flush() { }

        public override Int64 Seek(long offset, SeekOrigin origin)
        { return 0; }

        public override void SetLength(long value) { }

        public override Int32 Read(byte[] buffer, int offset, int count)
        { return 0; }

        public override void Write(byte[] buffer, int offset, int count) { }

        public override Boolean CanRead { get { return true; } }

        public override Boolean CanSeek { get { return true; } }

        public override Boolean CanWrite { get { return true; } }

        public override Int64 Length { get { return 0; } }

        public override Int64 Position
        {
            get { return 0; }
            set { }
        }
    }
}
