using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Growl.Destinations;

namespace Growl
{
    [Serializable]
    public class TwitterForwardDestination : Growl.Destinations.ForwardDestination
    {
        public static string DefaultFormat = String.Format("{0}: {1} - {2}", PLACEHOLDER_APPNAME, PLACEHOLDER_TITLE, PLACEHOLDER_TEXT);

        private const string PLACEHOLDER_APPNAME = "{APPNAME}";
        private const string PLACEHOLDER_TITLE = "{TITLE}";
        private const string PLACEHOLDER_TEXT = "{TEXT}";
        private const string PLACEHOLDER_PRIORITY = "{PRIORITY}";
        private const string PLACEHOLDER_SENDER = "{SENDER}";

        private string username;
        private string password;
        private string format;
        private Growl.Connector.Priority? minimumPriority = null;
        private bool onlyWhenIdle;

        [NonSerialized]
        private string consumerKey;
        [NonSerialized]
        private string consumerSecret;

        private string token;
        private string tokenSecret;

        public TwitterForwardDestination(string name, bool enabled, string username, string password, string format, Growl.Connector.Priority? minimumPriority, bool onlyWhenIdle)
            : base(name, enabled)
        {
            this.username = username;
            this.password = password;
            this.format = (String.IsNullOrEmpty(format) ? TwitterForwardDestination.DefaultFormat : format);
            this.minimumPriority = minimumPriority;
            this.onlyWhenIdle = onlyWhenIdle;
            this.Platform = KnownDestinationPlatformType.Twitter;
        }

        public override string Description
        {
            get
            {
                return String.Format("@{0}", this.Username);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public string Username
        {
            get
            {
                return this.username;
            }
            set
            {
                this.username = value;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }

        public string Format
        {
            get
            {
                return this.format;
            }
            set
            {
                this.format = value;
            }
        }

        public Growl.Connector.Priority? MinimumPriority
        {
            get
            {
                return this.minimumPriority;
            }
            set
            {
                this.minimumPriority = value;
            }
        }

        public bool OnlyWhenIdle
        {
            get
            {
                return this.onlyWhenIdle;
            }
            set
            {
                this.onlyWhenIdle = value;
            }
        }

        public override bool Available
        {
            get
            {
                return true;
            }
            protected set
            {
                throw new NotSupportedException("The .Available property is read-only.");
            }
        }

        public override string AddressDisplay
        {
            get
            {
                string priorityText = PrefPriority.GetByValue(this.MinimumPriority).Name;
                string priorityDisplay = (this.MinimumPriority != null && this.MinimumPriority.HasValue ? priorityText : Properties.Resources.AddProwl_AnyPriority);
                string idleDisplay = (this.OnlyWhenIdle ? Properties.Resources.LiteralString_IdleOnly : Properties.Resources.LiteralString_Always);
                return String.Format("({0}/{1})", priorityDisplay, idleDisplay);
            }
        }

        public override DestinationBase Clone()
        {
            TwitterForwardDestination clone = new TwitterForwardDestination(this.Description, this.Enabled, this.Username, this.Password, this.Format, this.MinimumPriority, this.OnlyWhenIdle);
            return clone;
        }

        public override void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Connector.RequestInfo requestInfo, bool isIdle)
        {
            // IGNORE REGISTRATION NOTIFICATIONS
            requestInfo.SaveHandlingInfo("Forwarding to Twitter cancelled - Application Registrations are not forwarded.");
        }

        public override void ForwardNotification(Growl.Connector.Notification notification, Growl.Connector.CallbackContext callbackContext, Growl.Connector.RequestInfo requestInfo, bool isIdle, ForwardedNotificationCallbackHandler callbackFunction)
        {
            bool send = true;

            // if a minimum priority is set, check that
            if (this.MinimumPriority != null && this.MinimumPriority.HasValue && notification.Priority < this.MinimumPriority.Value)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Twitter ({0}) cancelled - Notification priority must be at least '{1}' (was actually '{2}').", this.Username, this.MinimumPriority.Value.ToString(), notification.Priority.ToString()));
                send = false;
            }

            // if only sending when idle, check that
            if (this.OnlyWhenIdle && !isIdle)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Twitter ({0}) cancelled - Currently only configured to forward when idle", this.Username));
                send = false;
            }

            if (send)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarded to Twitter '{0}' - Minimum Priority:'{1}', Actual Priority:'{2}'", this.Description, (this.MinimumPriority != null && this.MinimumPriority.HasValue ? this.MinimumPriority.Value.ToString() : "<any>"), notification.Priority.ToString()));

                string message = this.format;
                message = message.Replace(PLACEHOLDER_APPNAME, notification.ApplicationName);
                message = message.Replace(PLACEHOLDER_TITLE, notification.Title);
                message = message.Replace(PLACEHOLDER_TEXT, notification.Text);
                message = message.Replace(PLACEHOLDER_PRIORITY, PrefPriority.GetFriendlyName(notification.Priority));
                message = message.Replace(PLACEHOLDER_SENDER, notification.MachineName);
                //Utility.WriteLine(message);

                // trim
                if (message.Length > 140) message = message.Substring(0, 140);
                //byte[] bytes = System.Text.Encoding.UTF8.GetBytes("status=" + message);
                string data = "status=" + message;

                // send async (using threads instead of async WebClient/HttpWebRequest methods since they seem to have a bug with KeepAlives in infrequent cases)
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(SendAsync), data);
            }
        }

        private void SendAsync(object state)
        {
            try
            {
                byte[] data = (byte[])state;

                string url = "http://twitter.com/statuses/update.xml";

                string credentials = String.Format("{0}:{1}", this.Username, this.Password);
                string encodedCredentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credentials));
                string authorizationHeaderValue = String.Format("Basic {0}", encodedCredentials);

                Growl.CoreLibrary.WebClientEx wc = new Growl.CoreLibrary.WebClientEx();
                wc.Headers.Add(System.Net.HttpRequestHeader.UserAgent, "Growl for Windows/2.0");
                wc.Headers.Add(System.Net.HttpRequestHeader.Authorization, authorizationHeaderValue);
                wc.Headers.Add(System.Net.HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

                using (wc)
                {
                    try
                    {
                        byte[] bytes = wc.UploadData(url, "POST", data);
                        string response = System.Text.Encoding.ASCII.GetString(bytes);
                        Utility.WriteDebugInfo(String.Format("Twitter forwarding response: {0}", response));
                    }
                    catch (Exception ex)
                    {
                        Utility.WriteDebugInfo(String.Format("Twitter forwarding failed: {0}", ex.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.WriteDebugInfo(ex.ToString());
            }
        }

        // TODO: not ready for real use yet - waiting on better solution from Twitter with regard to consumer keys and open source apps
        private void SendAsync_xAuth(object state)
        {
            try
            {
                // retrieve consumer key and secret
                /* If you want to user your own Twitter Consumer Key/Secret, you can set the appropriate
                 * values in the user.config file and they will be used instead of Growl for Window's
                 * own values. If you use Growl for Window's values, they will be retrieved from the
                 * internet.
                 * */
                if (String.IsNullOrEmpty(this.consumerKey) || String.IsNullOrEmpty(this.consumerSecret))
                {
                    this.consumerKey = Properties.Settings.Default.TwitterConsumerKey;
                    this.consumerSecret = Properties.Settings.Default.TwitterConsumerSecret;
                    if (String.IsNullOrEmpty(this.consumerKey) || String.IsNullOrEmpty(this.consumerSecret))
                    {
                        // TODO
                        GetDefaultConsumerKey();
                    }
                }

                // set up authentication
                oAuthTwitter oAuth = new oAuthTwitter();
                oAuth.ConsumerKey = this.consumerKey;
                oAuth.ConsumerSecret = this.consumerSecret;

                if (String.IsNullOrEmpty(this.token) || String.IsNullOrEmpty(this.tokenSecret))
                {
                    oAuth.xAuthAccessTokenGet(this.Username, this.Password);
                    // SAVE THESE for use in the future
                    this.token = oAuth.Token;
                    this.tokenSecret = oAuth.TokenSecret;
                }
                else
                {
                    oAuth.Token = this.token;
                    oAuth.TokenSecret = this.tokenSecret;
                }

                // post the status update
                string data = (string)state;
                try
                {
                    string response = oAuth.oAuthWebRequest(oAuthTwitter.Method.POST, "http://twitter.com/statuses/update.xml", data);
                    Utility.WriteDebugInfo(String.Format("Twitter forwarding response: {0}", response));
                }
                catch (Exception ex)
                {
                    Utility.WriteDebugInfo(String.Format("Twitter forwarding failed: {0}", ex.Message));
                    HandleException(ex);
                }
            }
            catch (Exception ex)
            {
                Utility.WriteDebugInfo(ex.ToString());
                HandleException(ex);
            }
        }

        private void GetDefaultConsumerKey()
        {
            /*
            Growl.CoreLibrary.WebClientEx wc = new Growl.CoreLibrary.WebClientEx();
            using (wc)
            {
                wc.Headers.Add(HttpRequestHeader.UserAgent, "Growl for Windows/2.0");
                wc.DownloadString("http://www.growlforwindows.com/gfw/test.html");
            }
             * */

            Console.WriteLine("TODO: fetch ConsumerKey and ConsumerSecret from somewhere.");
            this.consumerKey = "FAKE.FAKE.FAKE";
            this.consumerSecret = "FAKE.FAKE.FAKE";
        }

        private void HandleException(Exception ex)
        {
            // if this is a 401 error, clear the token and tokensecret so we can request a new one next time
            if (ex != null)
            {
                WebException webex = ex as WebException;
                if (webex != null)
                {
                    if (webex.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse response = (HttpWebResponse)webex.Response;
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            this.consumerKey = null;
                            this.consumerSecret = null;

                            this.token = null;
                            this.tokenSecret = null;
                        }
                    }
                }
            }
        }
    }

    public class OAuthBase
    {

        /// <summary>
        /// Provides a predefined set of algorithms that are supported officially by the protocol
        /// </summary>
        public enum SignatureTypes
        {
            HMACSHA1,
            PLAINTEXT,
            RSASHA1
        }

        /// <summary>
        /// Provides an internal structure to sort the query parameter
        /// </summary>
        protected class QueryParameter
        {
            private string name = null;
            private string value = null;

            public QueryParameter(string name, string value)
            {
                this.name = name;
                this.value = value;
            }

            public string Name
            {
                get { return name; }
            }

            public string Value
            {
                get { return value; }
            }
        }

        /// <summary>
        /// Comparer class used to perform the sorting of the query parameters
        /// </summary>
        protected class QueryParameterComparer : IComparer<QueryParameter>
        {

            #region IComparer<QueryParameter> Members

            public int Compare(QueryParameter x, QueryParameter y)
            {
                if (x.Name == y.Name)
                {
                    return string.Compare(x.Value, y.Value);
                }
                else
                {
                    return string.Compare(x.Name, y.Name);
                }
            }

            #endregion
        }

        protected const string OAuthVersion = "1.0";
        protected const string OAuthParameterPrefix = "oauth_";

        //
        // List of know and used oauth parameters' names
        //        
        protected const string OAuthConsumerKeyKey = "oauth_consumer_key";
        protected const string OAuthCallbackKey = "oauth_callback";
        protected const string OAuthVersionKey = "oauth_version";
        protected const string OAuthSignatureMethodKey = "oauth_signature_method";
        protected const string OAuthSignatureKey = "oauth_signature";
        protected const string OAuthTimestampKey = "oauth_timestamp";
        protected const string OAuthNonceKey = "oauth_nonce";
        protected const string OAuthTokenKey = "oauth_token";
        protected const string OAuthTokenSecretKey = "oauth_token_secret";
        protected const string OAuthVerifier = "oauth_verifier";
        protected const string XAuthUsername = "x_auth_username";
        protected const string XAuthPassword = "x_auth_password";
        protected const string XAuthMode = "x_auth_mode";


        protected const string HMACSHA1SignatureType = "HMAC-SHA1";
        protected const string PlainTextSignatureType = "PLAINTEXT";
        protected const string RSASHA1SignatureType = "RSA-SHA1";

        protected Random random = new Random();

        protected string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        /// <summary>
        /// Helper function to compute a hash value
        /// </summary>
        /// <param name="hashAlgorithm">The hashing algoirhtm used. If that algorithm needs some initialization, like HMAC and its derivatives, they should be initialized prior to passing it to this function</param>
        /// <param name="data">The data to hash</param>
        /// <returns>a Base64 string of the hash value</returns>
        private string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }

            byte[] dataBuffer = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Internal function to cut out all non oauth query string parameters (all parameters not begining with "oauth_")
        /// </summary>
        /// <param name="parameters">The query string part of the Url</param>
        /// <returns>A list of QueryParameter each containing the parameter name and value</returns>
        private List<QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }

            List<QueryParameter> result = new List<QueryParameter>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
        /// </summary>
        /// <param name="value">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        public string UrlEncode(string value)
        {
            StringBuilder result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    //some symbols produce > 2 char values so the system urlencoder must be used to get the correct data
                    if (String.Format("{0:X2}", (int)symbol).Length > 3)
                    {
                        result.Append(HttpUtility.UrlEncode(value.Substring(value.IndexOf(symbol), 1)).ToUpper());
                    }
                    else
                    {
                        result.Append('%' + String.Format("{0:X2}", (int)symbol));
                    }
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Normalizes the request parameters according to the spec
        /// </summary>
        /// <param name="parameters">The list of parameters already sorted</param>
        /// <returns>a string representing the normalized parameters</returns>
        protected string NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            QueryParameter p = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate the signature base that is used to produce the signature
        /// </summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>        
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="signatureType">The signature type. To use the default values use <see cref="OAuthBase.SignatureTypes">OAuthBase.SignatureTypes</see>.</param>
        /// <returns>The signature base</returns>
        public string GenerateSignatureBase(Uri url, string consumerKey, string token, string tokenSecret, string verifier, string xAuthUsername, string xAuthPassword, string httpMethod, string timeStamp, string nonce, string signatureType, out string normalizedUrl, out string normalizedRequestParameters)
        {
            if (token == null)
            {
                token = string.Empty;
            }

            if (tokenSecret == null)
            {
                tokenSecret = string.Empty;
            }

            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentNullException("httpMethod");
            }

            if (string.IsNullOrEmpty(signatureType))
            {
                throw new ArgumentNullException("signatureType");
            }

            normalizedUrl = null;
            normalizedRequestParameters = null;

            List<QueryParameter> parameters = GetQueryParameters(url.Query);
            parameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));
            parameters.Add(new QueryParameter(OAuthNonceKey, nonce));
            parameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
            parameters.Add(new QueryParameter(OAuthSignatureMethodKey, signatureType));
            parameters.Add(new QueryParameter(OAuthConsumerKeyKey, consumerKey));

            if (!string.IsNullOrEmpty(token))
            {
                parameters.Add(new QueryParameter(OAuthTokenKey, token));
            }

            if (!string.IsNullOrEmpty(verifier))
            {
                parameters.Add(new QueryParameter(OAuthVerifier, verifier));
            }

            if (!string.IsNullOrEmpty(xAuthUsername))
            {
                parameters.Add(new QueryParameter(XAuthUsername, UrlEncode(xAuthUsername)));
            }

            if (!string.IsNullOrEmpty(xAuthPassword))
            {
                parameters.Add(new QueryParameter(XAuthPassword, UrlEncode(xAuthPassword)));
                parameters.Add(new QueryParameter(XAuthMode, "client_auth"));

            }

            parameters.Sort(new QueryParameterComparer());

            normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
            {
                normalizedUrl += ":" + url.Port;
            }
            normalizedUrl += url.AbsolutePath;
            normalizedRequestParameters = NormalizeRequestParameters(parameters);

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
            signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        /// <summary>
        /// Generate the signature value based on the given signature base and hash algorithm
        /// </summary>
        /// <param name="signatureBase">The signature based as produced by the GenerateSignatureBase method or by any other means</param>
        /// <param name="hash">The hash algorithm used to perform the hashing. If the hashing algorithm requires initialization or a key it should be set prior to calling this method</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }

        /// <summary>
        /// Generates a signature using the HMAC-SHA1 algorithm
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier, string xAuthUsername, string xAuthPassword, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out string normalizedRequestParameters)
        {
            return GenerateSignature(url, consumerKey, consumerSecret, token, tokenSecret, verifier, xAuthUsername, xAuthPassword, httpMethod, timeStamp, nonce, SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedRequestParameters);
        }

        /// <summary>
        /// Generates a signature using the specified signatureType 
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="signatureType">The type of signature to use</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret, string verifier, string xAuthUsername, string xAuthPassword, string httpMethod, string timeStamp, string nonce, SignatureTypes signatureType, out string normalizedUrl, out string normalizedRequestParameters)
        {
            normalizedUrl = null;
            normalizedRequestParameters = null;

            switch (signatureType)
            {
                case SignatureTypes.PLAINTEXT:
                    return HttpUtility.UrlEncode(string.Format("{0}&{1}", consumerSecret, tokenSecret));
                case SignatureTypes.HMACSHA1:
                    string signatureBase = GenerateSignatureBase(url, consumerKey, token, tokenSecret, verifier, xAuthUsername, xAuthPassword, httpMethod, timeStamp, nonce, HMACSHA1SignatureType, out normalizedUrl, out normalizedRequestParameters);

                    HMACSHA1 hmacsha1 = new HMACSHA1();
                    hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(consumerSecret), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)));

                    return GenerateSignatureUsingHash(signatureBase, hmacsha1);
                case SignatureTypes.RSASHA1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
            }
        }

        /// <summary>
        /// Generate the timestamp for the signature        
        /// </summary>
        /// <returns></returns>
        public virtual string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// Generate a nonce
        /// </summary>
        /// <returns></returns>
        public virtual string GenerateNonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return random.Next(123400, 9999999).ToString();
        }

    }

    public class oAuthTwitter : OAuthBase
    {
        public enum Method { GET, POST };
        public const string REQUEST_TOKEN = "http://twitter.com/oauth/request_token";
        public const string AUTHORIZE = "http://twitter.com/oauth/authorize";
        public const string ACCESS_TOKEN = "http://twitter.com/oauth/access_token";
        public const string XAUTH_ACCESS_TOKEN = "https://api.twitter.com/oauth/access_token";

        private string _consumerKey = "";
        private string _consumerSecret = "";
        private string _token = "";
        private string _tokenSecret = "";
        private string _verifier = "";
        private string _xAuthUsername = "";
        private string _xAuthPassword = "";

        #region Properties
        public string ConsumerKey
        {
            get
            {
                return _consumerKey;
            }
            set { _consumerKey = value; }
        }

        public string ConsumerSecret
        {
            get
            {
                return _consumerSecret;
            }
            set { _consumerSecret = value; }
        }

        public string Token { get { return _token; } set { _token = value; } }
        public string TokenSecret { get { return _tokenSecret; } set { _tokenSecret = value; } }
        public string Verifier { get { return _verifier; } set { _verifier = value; } }
        public string xAuthUsername { get { return _xAuthUsername; } set { _xAuthUsername = value; } }
        public string xAuthPassword { get { return _xAuthPassword; } set { _xAuthPassword = value; } }

        #endregion

        /// <summary>
        /// Get the link to Twitter's authorization page for this application.
        /// </summary>
        /// <returns>The url with a valid request token, or a null string.</returns>
        public string AuthorizationLinkGet()
        {
            string ret = null;

            string response = oAuthWebRequest(Method.GET, REQUEST_TOKEN, String.Empty);
            if (response.Length > 0)
            {
                //response contains token and token secret.  We only need the token.
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    ret = AUTHORIZE + "?oauth_token=" + qs["oauth_token"];
                }
            }
            return ret;
        }

        /// <summary>
        /// Exchange the request token for an access token.
        /// </summary>
        /// <param name="authToken">The oauth_token is supplied by Twitter's authorization page following the callback.</param>
        public void AccessTokenGet(string authToken, string verifier)
        {
            this.Token = authToken;
            this.Verifier = verifier;

            string response = oAuthWebRequest(Method.GET, ACCESS_TOKEN, String.Empty);

            if (response.Length > 0)
            {
                //Store the Token and Token Secret
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    this.Token = qs["oauth_token"];
                }
                if (qs["oauth_token_secret"] != null)
                {
                    this.TokenSecret = qs["oauth_token_secret"];
                }
            }
        }

        /// <summary>
        /// Exchange the username and password for an access token.
        /// </summary>
        /// <param name="username">Twitter Username.</param>
        /// <param name="username">Twitter Password.</param>
        public void xAuthAccessTokenGet(string username, string password)
        {
            this.xAuthUsername = username;
            this.xAuthPassword = password;

            string response = oAuthWebRequest(Method.GET, XAUTH_ACCESS_TOKEN, String.Empty);

            if (response.Length > 0)
            {
                //Store the Token and Token Secret
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    this.Token = qs["oauth_token"];
                }
                if (qs["oauth_token_secret"] != null)
                {
                    this.TokenSecret = qs["oauth_token_secret"];
                }
            }
        }



        /// <summary>
        /// Submit a web request using oAuth.
        /// </summary>
        /// <param name="method">GET or POST</param>
        /// <param name="url">The full url, including the querystring.</param>
        /// <param name="postData">Data to post (querystring format)</param>
        /// <returns>The web server response.</returns>
        public string oAuthWebRequest(Method method, string url, string postData)
        {
            string outUrl = "";
            string querystring = "";
            string ret = "";


            //Setup postData for signing.
            //Add the postData to the querystring.
            if (method == Method.POST)
            {
                if (postData.Length > 0)
                {
                    //Decode the parameters and re-encode using the oAuth UrlEncode method.
                    NameValueCollection qs = HttpUtility.ParseQueryString(postData);
                    postData = "";
                    foreach (string key in qs.AllKeys)
                    {
                        if (postData.Length > 0)
                        {
                            postData += "&";
                        }
                        qs[key] = HttpUtility.UrlDecode(qs[key]);
                        qs[key] = this.UrlEncode(qs[key]);
                        postData += key + "=" + qs[key];

                    }
                    if (url.IndexOf("?") > 0)
                    {
                        url += "&";
                    }
                    else
                    {
                        url += "?";
                    }
                    url += postData;
                }
            }

            Uri uri = new Uri(url);

            string nonce = this.GenerateNonce();
            string timeStamp = this.GenerateTimeStamp();

            //Generate Signature
            string sig = this.GenerateSignature(uri,
                this.ConsumerKey,
                this.ConsumerSecret,
                this.Token,
                this.TokenSecret,
                this.Verifier,
                this.xAuthUsername,
                this.xAuthPassword,
                method.ToString(),
                timeStamp,
                nonce,
                out outUrl,
                out querystring);

            querystring += "&oauth_signature=" + HttpUtility.UrlEncode(sig);

            //Convert the querystring to postData
            if (method == Method.POST)
            {
                postData = querystring;
                querystring = "";
            }

            if (querystring.Length > 0)
            {
                outUrl += "?";
            }

            ret = WebRequest(method, outUrl + querystring, postData);

            return ret;
        }

        /// <summary>
        /// Web Request Wrapper
        /// </summary>
        /// <param name="method">Http Method</param>
        /// <param name="url">Full url to the web resource</param>
        /// <param name="postData">Data to post in querystring format</param>
        /// <returns>The web server response.</returns>
        public string WebRequest(Method method, string url, string postData)
        {
            HttpWebRequest webRequest = null;
            StreamWriter requestWriter = null;
            string responseData = "";

            webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method.ToString();
            webRequest.ServicePoint.Expect100Continue = false;
            //webRequest.UserAgent  = "Growl for Windows/2.0";
            //webRequest.Timeout = 20000;

            if (method == Method.POST)
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";

                //POST the data.
                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                try
                {
                    requestWriter.Write(postData);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    requestWriter.Close();
                    requestWriter = null;
                }
            }

            responseData = WebResponseGet(webRequest);

            webRequest = null;

            return responseData;

        }

        /// <summary>
        /// Process the web response.
        /// </summary>
        /// <param name="webRequest">The request object.</param>
        /// <returns>The response data.</returns>
        public string WebResponseGet(HttpWebRequest webRequest)
        {
            StreamReader responseReader = null;
            string responseData = "";

            try
            {
                responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                responseData = responseReader.ReadToEnd();
            }
            catch
            {
                throw;
            }
            finally
            {
                webRequest.GetResponse().GetResponseStream().Close();
                responseReader.Close();
                responseReader = null;
            }

            return responseData;
        }
    }
}
