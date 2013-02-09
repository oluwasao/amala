using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;

namespace PublicLogic
{
    public class WebHelper
    {
        /// <summary>
        /// This Method will return the Native HTML as a string from the requested URL
        /// There are no parameters being handed to the page, so dynamic pages will return only the default
        /// </summary>
        /// <param name="url">URL - required - else the HTTP request is not pointing at anything.</param>
        /// <returns>Native HTML - as returned from the server</returns>
        public static string getPageContent(string url)
        {
            string Result = "";

            HttpWebResponse wtResponse = null;
            StreamReader sr = null;

            try
            {
                // Initialize the WebRequest.
                HttpWebRequest wtRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
                wtRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                wtResponse = (HttpWebResponse)wtRequest.GetResponse();

                // Without this, the 1.1 framework leaks connections and generates spurious exceptions later.
                wtRequest.KeepAlive = false;
                
                sr = new StreamReader(wtResponse.GetResponseStream(), GetResponseEncoding(wtResponse.CharacterSet));

                Result = sr.ReadToEnd();
            }
            catch (System.Net.WebException we)
            {
                throw new System.Net.WebException("getPageContent failed to retrieve url " + url.ToString() + ".", we);
            }
            finally
            {
                // Close the response to free resources.
                if (wtResponse != null)
                    wtResponse.Close();

                if (sr != null)
                    sr.Close();
            }

            return Result;
        }

        /// <summary>
        /// This Method will return the Native HTML or the cookie collection in string form from a dynamic page.
        /// </summary>
        /// <param name="url">Url - Required, must start with "http://" or "https".</param>
        /// <param name="nameValuePairs">A string of name value pairs separated by an '&', exctly as you would do if you were adding them to a URL</param>
        /// <param name="type">[HTML|Cookies]</param>
        /// <exception cref="ArgumentNullException">Thrown when a null or zero-length string is specified for <see cref="url"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when a non-http or https <see cref="url"/>.</exception>
        /// <returns>Native HTML - as returned from the server or A string of name-value pairs derived from the Cookie Collection.</returns>
        public static string getPageContentDynamic(string url, string nameValuePairs, string type)
        {
            CookieContainer empty = new CookieContainer();
            return getPageContentDynamic(url, nameValuePairs, type, empty);

        }

        /// <summary>
        /// This Method will return the Native HTML or the cookie collection in string form from a dynamic page.
        /// </summary>
        /// <param name="url">Url - Required, must start with "http://" or "https".</param>
        /// <param name="nameValuePairs">A string of name value pairs separated by an '&', exctly as you would do if you were adding them to a URL</param>
        /// <param name="type">[HTML|Cookies]</param>
        /// <param name="cookies">A cookie collection to pass into the <see cref="WebRequest"/></param>
        /// <exception cref="ArgumentNullException">Thrown when a null or zero-length string is specified for <see cref="url"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when a non-http or https <see cref="url"/>.</exception>
        /// <returns>Native HTML - as returned from the server or A string of name-value pairs derived from the Cookie Collection.</returns>
        public static string getPageContentDynamic(string url, string nameValuePairs, string type, CookieContainer cookies)
        {
            if (url == null || url.Length == 0)
                throw new ArgumentNullException("url", "No Url Provided to GetPageDynamic");

            if (url.Substring(0, 7).ToLower() != "http://" && url.Substring(0, 8).ToLower() != "https://")
                throw new ArgumentException("Non-Http URL provided to getPageContentDynamic.", "url");

            string Result = "";
            StreamReader sr = null;

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] byteArray = encoding.GetBytes(nameValuePairs);

            // Initialize the WebRequest.
            HttpWebRequest wtRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));

            try
            {
                // Set the required properties of the WebRequest.
                wtRequest.CookieContainer = cookies;
                wtRequest.Method = "POST";
                wtRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                wtRequest.ContentType = "application/x-www-form-urlencoded";
                wtRequest.ContentLength = byteArray.Length;
                wtRequest.Timeout = 90000;

                // Without this, the 1.1 framework leaks connections and generates spurious exceptions later.
                wtRequest.KeepAlive = false;

                using (Stream newStream = wtRequest.GetRequestStream())
                {
                    newStream.Write(byteArray, 0, byteArray.Length);
                    newStream.Close();
                }

                // Assign the response object of 'WebRequest' to a 'WebResponse' variable.
                using (HttpWebResponse wtResponse = (HttpWebResponse)wtRequest.GetResponse())
                {
                    // If we are looking for cookies for the session, we loop through and grab them here...
                    if (type == "cookies")
                    {
                        string showMike = "";
                        foreach (string key in wtResponse.Headers.Keys)
                        {
                            showMike = showMike + key + "=" + wtResponse.Headers[key] + ";";
                        }
                        Result = showMike;
                    }
                    else if (type == "HTML")
                    {

                        // If we are looking for the raw HTML this is where we convert it into something meaningfull...
                        // Read the response and convert it to a string to hand back to the caller.	                        
                        using (sr = new StreamReader(wtResponse.GetResponseStream(),GetResponseEncoding(wtResponse.CharacterSet)))
                        {
                            Result = sr.ReadToEnd();
                            sr.Close();
                        }
                    }
                    wtResponse.Close();
                }
            }
            catch (System.Net.WebException we)
            {
                throw new System.Net.WebException("getPageContent failed to retrieve url " + url.ToString() + ".", we);
            }
            return Result;
        }

        /// <summary>
        /// Strips html of character escapes
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Sanitize(string input)
        {
            input = input.Replace("\\", "");
            input = input.Replace("\t", "");
            input = input.Replace("\n", "");
            input = input.Replace("\r", "");
            return input;
        }

        /// <summary>
        /// Gets the encoding from the response
        /// </summary>
        /// <param name="characterSet"></param>
        /// <returns></returns>
        public static Encoding GetResponseEncoding(string characterSet)
        {
            if (!string.IsNullOrEmpty(characterSet))
                return Encoding.GetEncoding(characterSet);

            return Encoding.GetEncoding(1252);
        }

        /// <summary>
        /// ftp file to sepcified location
        /// </summary>
        /// <param name="file">fileInfo class</param>
        /// <param name="ftpLocation">location on the ftp server</param>
        /// <param name="username">ftp username</param>
        /// <param name="password">ftp password</param>
        public static void FtpFile(FileInfo file, string ftpLocation, string username, string password)
        {
            try
            {
                Uri uri = new Uri(ftpLocation + file.Name);

                //send request to the server
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.KeepAlive = false;
                request.UseBinary = true;
                request.UsePassive = false;
                request.ContentLength = file.Length;
                

                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;

                // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
                FileStream fs = file.OpenRead();

                // Stream to which the file to be upload is written
                Stream strm = request.GetRequestStream();
                // Read from the file stream 2kb at a time
                contentLen = fs.Read(buff, 0, buffLength);

                // Till Stream content ends
                while (contentLen != 0)
                {
                    // Write Content from the file stream to the FTP Upload Stream
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }

                // Close the file stream and the Request Stream
                strm.Close();
                fs.Close();
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
            }
        }
        
        public static void FtpFile(string path,string content, string ftpLocation, string username, string password)
        {
            try
            {
                Uri uri = new Uri(ftpLocation + path);
                //create a memory stream
                MemoryStream ms = new MemoryStream();
                //use a StreamWriter  to write to the memory stream
                StreamWriter sw = new StreamWriter(ms);
                sw.Write(content);
                sw.Flush();

                //send request to the server
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.KeepAlive = false;
                request.UseBinary = true;
                request.UsePassive = false;
                
                // Stream to which the file to be upload is written                
                Stream strm = request.GetRequestStream();
                //write to the ftpstream from the memory stream
                ms.WriteTo(strm);                
                strm.Close();
                ms.Close();
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
            }
        }

        public static void FtpFileFromMemoryStream(string path, string content, string ftpLocation, string username, string password)
        {
            try
            {

                Uri uri = new Uri(ftpLocation + path);
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.Write(content);
                sw.Flush();

                //send request to the server
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.KeepAlive = false;
                request.UseBinary = false;
                request.UsePassive = false;

                Stream strm = request.GetRequestStream();
                ms.WriteTo(strm);

                sw.Close();
                ms.Close();
                strm.Close();
            }
            catch (WebException ex)
            {
                String status = ((FtpWebResponse)ex.Response).StatusDescription;
            }
        }

    }
}

