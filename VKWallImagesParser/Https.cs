﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VKWallImagesParser
{
    class Https
    {
        private const string UA_Mozilla = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0";
        private WebProxy myproxy;
        private bool useProxy;

        public Https(WebProxy proxy=null)
        {
            if (proxy != null)
            {
                myproxy = proxy;
                useProxy = true;
            }
            else useProxy = false;
        }

        public HttpResponse GET(string host, CookieContainer cc)
        {
            if (cc == null) cc = new CookieContainer();
            HttpWebResponse webResponse = null;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(host);
                webRequest.Headers = GetHeader();
                webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                webRequest.UserAgent = UA_Mozilla;
                if (useProxy) webRequest.Proxy = myproxy;
                webRequest.Method = "GET";
                webRequest.ProtocolVersion = HttpVersion.Version11;
                webRequest.AllowAutoRedirect = false;
                webRequest.CookieContainer = cc;
                webRequest.ProtocolVersion = HttpVersion.Version10;
                webRequest.KeepAlive = true;

                webResponse = (HttpWebResponse)webRequest.GetResponse();
                //cc.Add(webResponse.Cookies);

                return new HttpResponse(webResponse);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (webResponse != null)
                {
                    webResponse.Close();
                    cc.Add(webResponse.Cookies);
                }
            }
        }

        public HttpResponse POST(string host, CookieContainer cc, NameValueCollection param)
        {
            HttpWebResponse webResponse = null;
            if (cc == null) cc = new CookieContainer();
            try
            {
                if (param.Count == 0)
                    throw new ArgumentNullException();

                List<string> parametersList = param.AllKeys.Select(key => String.Format("{0}={1}", key, param[key])).ToList();
                string parameters = String.Join("&", parametersList);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(host);
                webRequest.Headers = GetHeader();
                webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                webRequest.UserAgent = UA_Mozilla;
                if (useProxy) webRequest.Proxy = myproxy;
                webRequest.Method = "POST";
                webRequest.ProtocolVersion = HttpVersion.Version11;
                webRequest.AllowAutoRedirect = false;
                webRequest.CookieContainer = cc;
                webRequest.ProtocolVersion = HttpVersion.Version10;
                webRequest.KeepAlive = true;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = parameters.Length;
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.ServerCertificateValidationCallback = AcceptAllCertifications;

                using (var requestStream = new StreamWriter(webRequest.GetRequestStream()))
                {
                    requestStream.Write(parameters);
                }

                webResponse = (HttpWebResponse)webRequest.GetResponse();
                //cc.Add(webResponse.Cookies);

                return new HttpResponse(webResponse);

            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (webResponse != null)
                {
                    webResponse.Close();
                    cc.Add(webResponse.Cookies);
                }


            }
        }

        private WebHeaderCollection GetHeader()
        {
            WebHeaderCollection Headers = new WebHeaderCollection();
            Headers = new WebHeaderCollection();
            Headers.Add("Accept-Language", "en-US,en;q=0.8,en-US;q=0.5,en;q=0.3");
            Headers.Add("Accept-Encoding", "gzip, deflate");
            return Headers;
        }

        public static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }
    public class HttpResponse
    {
        public HttpResponse(HttpWebResponse webResponse)
        {
            this.CharacterSet = webResponse.CharacterSet;
            this.ContentEncoding = webResponse.ContentEncoding;
            this.ContentLength = webResponse.ContentLength;
            this.ContentType = webResponse.ContentType;
            this.Cookies = webResponse.Cookies;
            this.Headers = webResponse.Headers;
            this.IsMutuallyAuthenticated = webResponse.IsMutuallyAuthenticated;
            this.LastModified = webResponse.LastModified;
            this.Method = webResponse.Method;
            this.ProtocolVersion = webResponse.ProtocolVersion;
            this.ResponseUri = webResponse.ResponseUri;
            this.Server = webResponse.Server;
            this.StatusCode = webResponse.StatusCode;
            this.StatusDescription = webResponse.StatusDescription;
            this.SupportsHeaders = webResponse.SupportsHeaders;
            this.Instance = webResponse;
            this.Content = GetBody();
        }

        public string CharacterSet { get; private set; }
        public string ContentEncoding { get; private set; }
        public long ContentLength { get; private set; }
        public string ContentType { get; private set; }
        public CookieCollection Cookies { get; private set; }
        public WebHeaderCollection Headers { get; private set; }
        public bool IsMutuallyAuthenticated { get; private set; }
        public DateTime LastModified { get; private set; }
        public string Method { get; private set; }
        public Version ProtocolVersion { get; private set; }
        public Uri ResponseUri { get; private set; }
        public string Server { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string StatusDescription { get; private set; }
        public bool SupportsHeaders { get; private set; }
        public HttpWebResponse Instance { get; private set; }
        public string Content { get; private set; }

        private string GetBody()
        {
            if (this.Instance == null)
                return null;

            StreamReader reader = new StreamReader(Decompress(this.Instance), Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private Stream Decompress(HttpWebResponse webResponse)
        {
            Stream responseStream = webResponse.GetResponseStream();
            if (webResponse.ContentEncoding.ToLower().Contains("gzip"))
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            else if (webResponse.ContentEncoding.ToLower().Contains("deflate"))
                responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);

            return responseStream;
        }
    }
}
