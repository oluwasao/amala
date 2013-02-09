using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;


namespace PublicLogic
{
    public static class Global
    {
        /// <summary>
        /// Store one instance of the DateFormatter, it could be used MANY times, no point instanciating it each time...
        /// </summary>
        public static IgluDateFormatter DateFormatter { get; private set; }

        public static double EURConversionRate { get; private set; }
        public static double USDConversionRate { get; private set; }

        /// <summary>
        /// Static Constructor
        /// </summary>
        static Global()
        {
            //Set a timeout for the Web Related methods
            Timeout = 15000;

            //Errors
            ErrorsTo = ConfigurationManager.AppSettings["ErrorsTo"].ToString();
            MailServer = ConfigurationManager.AppSettings["MailServer"].ToString();

            //Get the Timeout in Minutes
            MainLoopInterval = (Convert.ToInt32(ConfigurationManager.AppSettings["MainLoopInterval"].ToString()) * 60000);
            FailedImporterRetry = (Convert.ToInt32(ConfigurationManager.AppSettings["FailedImporterRetry"].ToString()));

            //Initialise the DateFormatter
            DateFormatter = new IgluDateFormatter();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["GlobalDb"].ConnectionString))
            {
                conn.Open();
                using (SqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = "GetExchangeRate";
                    command.CommandType = CommandType.StoredProcedure;

                    //Get the EUR Conversion Rate
                    command.Parameters.Add("@CurrencyCode", SqlDbType.Char, 3).Value = "EUR";
                    EURConversionRate = Convert.ToDouble(command.ExecuteScalar());

                    command.Parameters.Clear();
                    command.Parameters.Add("@CurrencyCode", SqlDbType.Char, 3).Value = "USD";
                    USDConversionRate = Convert.ToDouble(command.ExecuteScalar());
                }
            }
        }

        #region Public Methods
       

        public static void SetLocale()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
        }

        /// <summary>
        /// Check whether a file should be updated, based on it's age (1 or more day old) OR if the filesize doesn't match the one provided
        /// </summary>
        /// <param name="fileName">The File to check</param>
        /// <param name="newFileSize">The Size of the new file</param>
        /// <returns>True if should be updated</returns>
        public static bool ShouldUpdateFile(string fileName, long newFileSize)
        {
            //Check if the File Exists
            if (File.Exists(fileName))
            {
                FileInfo fileInfo = new FileInfo(fileName);

                //If it's more than a day old, go and fetch a new one
                if (((DateTime.Now - fileInfo.LastWriteTime).TotalDays >= 1) || fileInfo.Length != newFileSize)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        /// <summary>
        /// Check whether a file should be updated, based on it's age (1 or more day old)
        /// </summary>
        /// <param name="fileName">The File to check</param>
        /// <returns>True if should be updated</returns>
        public static bool ShouldUpdateFile(string fileName)
        {
            //Check if the File Exists
            if (File.Exists(fileName))
            {
                FileInfo fileInfo = new FileInfo(fileName);

                //If it's more than a day old, go and fetch a new one
                if ((DateTime.Now - fileInfo.LastWriteTime).TotalDays >= 1)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        #endregion

        #region Web Related Methods
        /// <summary>
        /// The UserAgent we will be using for Requests, act asif we are IE6
        /// </summary>
        private static string UserAgent
        {
            get { return "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)"; }
        }

        /// <summary>
        /// Default Timeout for WebRequests
        /// </summary>
        public static int Timeout { get; set; }

        /// <summary>
        /// Create an HTTP Request object with the relevant credentials and URL
        /// </summary>
        /// <param name="fullUrl">The full URL we want to Request (FTP/HTTP/FILE(UNC))</param>
        /// <param name="credentials">The Network Credentials to use, i.e. Username and Password</param>
        /// <returns>A new WebRequest, ready to use</returns>
        private static WebRequest CreateRequest(Uri fullUri, NetworkCredential credentials)
        {
            //Create a http web request
            WebRequest fileRequest = WebRequest.Create(fullUri);

            //Store the Credentials, only if they are something, nulls make the FTP Web Request throw exceptions
            if (credentials != null)
                fileRequest.Credentials = credentials;

            //Set the Timeout
            fileRequest.Timeout = Timeout;

            //Return the new Http Web Request
            return fileRequest;
        }

        /// <summary>
        /// Get a Response from a Server page
        /// </summary>
        /// <param name="destinationUrl"></param>
        /// <returns></returns>
        public static string GetResponse(Uri destinationUrl)
        {
            return GetResponse(destinationUrl, null);
        }

        /// <summary>
        /// Get a Response from a Server page, passing in Cookies (Storing Session)
        /// </summary>
        /// <param name="destinationUrl">The URL You want to call</param>
        /// <param name="container">Any Cookies that are needed</param>
        /// <returns>The Response from the DestinationUrl</returns>
        public static string GetResponse(Uri destinationUrl, CookieContainer container)
        {
            return GetResponse(destinationUrl, null, container);
        }

        /// <summary>
        /// Get a Response from a Server page, passing in Cookies (Storing Session)
        /// </summary>
        /// <param name="destinationUrl">The URL You want to call</param>
        /// <param name="credentials">The Logon credentials to use to login to the remote server</param>
        /// <param name="container">Any Cookies that are needed</param>
        /// <returns>The Response from the DestinationUrl</returns>
        public static string GetResponse(Uri destinationUrl, NetworkCredential credentials, CookieContainer container)
        {
            return GetResponse(destinationUrl, credentials, container, "", "");
        }

        /// <summary>
        /// Get a Response from a Server page, passing in Cookies (Storing Session)
        /// </summary>
        /// <param name="destinationUrl">The URL You want to call</param>
        /// <param name="credentials">The Logon credentials to use to login to the remote server</param>
        /// <param name="container">Any Cookies that are needed</param>
        /// <param name="formBody">Any form body that we want to post, i.e. username=jim&password=bob</param>
        /// <param name="method">Method, i.e. POST/GET</param>
        /// <returns>The Response from the DestinationUrl</returns>
        public static string GetResponse(Uri destinationUri, NetworkCredential credentials, CookieContainer container, string formBody, string method)
        {
            string response = "";

            //Check to make sure we are http/https
            if ((destinationUri.Scheme != Uri.UriSchemeHttp && destinationUri.Scheme != Uri.UriSchemeFtp))
                throw new ArgumentException(string.Format("Invalid Uri Scheme ({0})", destinationUri.Scheme));

            //Create a new HttpWebRequest so we can use our Cookies
            HttpWebRequest webRequest = (HttpWebRequest)CreateRequest(destinationUri, credentials);
            webRequest.CookieContainer = container;

            switch (method)
            {
                case "POST":
                    {
                        webRequest.Method = "POST";
                        webRequest.ContentType = "application/x-www-form-urlencoded";
                        webRequest.ContentLength = formBody.Length;

                        //Set the Body of the Request
                        using (StreamWriter requestStream = new StreamWriter(webRequest.GetRequestStream()))
                        {
                            //Write the Form Body to the Body of the Request
                            requestStream.Write(formBody);
                        }
                    }
                    break;
                case "GET":
                    {
                        webRequest.Method = "GET";
                    }
                    break;
                default:
                    {
                        webRequest.Method = "GET";
                    }
                    break;
            }

            //Try and hit the page, let's see if we get a response
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            //Get StreamReader access to the ResponseStream so that we can just read everything, no matter how big
            using (StreamReader responseStream = new StreamReader(webResponse.GetResponseStream()))
            {
                //Read the Response, all the way to the end
                response = responseStream.ReadToEnd();

                //Close any open connections/streams
                responseStream.Close();
                webResponse.Close();
            }

            //Return the response from the server
            return response;
        }

        /// <summary>
        /// Download a File from a Remote Server (FTP/HTTP)
        /// </summary>
        /// <param name="sourceUrl">The Full URL to the File (http://www.google.co.uk/jim.txt) Or (ftp://jim:bob@www.google.co.uk/jim.txt) Or (\\itchynscratchy\ftp\cosmos\COSMOS.SOD.gz)</param>
        /// <param name="destination">Where to save the file to?</param>
        /// <returns>True if file was downloaded</returns>
        public static bool DownloadFile(Uri sourceUrl, string destination)
        {
            return DownloadFile(sourceUrl, destination, new CookieContainer());
        }

        /// <summary>
        /// Download a File from a Remote Server (FTP/HTTP)
        /// </summary>
        /// <param name="sourceUrl">The Url to Download From</param>
        /// <param name="destination">The Destination to Save the file to</param>
        /// <param name="container">Any cookies that are required to download the file</param>
        /// <returns>True if file was saved to the Destination provided</returns>
        public static bool DownloadFile(Uri sourceUrl, string destination, CookieContainer container)
        {
            return DownloadFile(sourceUrl, null, destination, container);
        }

        /// <summary>
        /// Download a File from a Remote Server (FTP/HTTP)
        /// </summary>
        /// <param name="sourceUrl">The Url to Download From</param>
        /// <param name="destination">The Destination to Save the file to</param>
        /// <param name="container">Any cookies that are required to download the file</param>
        /// <returns>True if file was saved to the Destination provided</returns>
        public static bool DownloadFile(Uri sourceUrl, string destination, NetworkCredential credentials)
        {
            return DownloadFile(sourceUrl, credentials, destination, null);
        }

        /// <summary>
        /// Download a File from a Remote Server (FTP/HTTP)
        /// </summary>
        /// <param name="sourceUrl">The Source Url</param>
        /// <param name="credentials">The Credentials to login with</param>
        /// <param name="destination">Where to save the file to?</param>
        /// <returns>True if file was downloaded</returns>
        public static bool DownloadFile(Uri sourceUrl, NetworkCredential credentials, string destination, CookieContainer container)
        {
            //Create a new Request Object
            WebRequest webRequest = CreateRequest(sourceUrl, credentials);
            WebResponse fileResponse = null;
            long expectedFileSize = 0;

            //Store the Credentials, only if they are something, nulls make the FTP Web Request throw exceptions
            if (credentials != null)
                webRequest.Credentials = credentials;

            //Store the cookies, and set the UserAgent, if this is a web request...
            HttpWebRequest HttpReq = webRequest as HttpWebRequest;
            if (HttpReq != null)
            {
                HttpReq.CookieContainer = container;
                HttpReq.UserAgent = UserAgent;
            }

            //If it's a FTP Request, we need to get the File Size first. As FTP Responses don't provide the ContentLength on DownloadFile
            FtpWebRequest FtpReq = webRequest as FtpWebRequest;
            if (FtpReq != null)
            {
                //Keep the connection alive
                FtpReq.KeepAlive = true;

                //Change the Method of the Request
                FtpReq.Method = WebRequestMethods.Ftp.GetFileSize;
                //Yes, some importers require this much time!
                FtpReq.Timeout = 60000;
                //Get the Response from the server
                fileResponse = webRequest.GetResponse();

                //Get the Expected File Size from the FTP Server
                expectedFileSize = fileResponse.ContentLength;

                //Close the Response
                fileResponse.Close();

                //We can't just change the Method back todownload file, as this request has already happened, we need to create a whole new WebRequest
                webRequest = CreateRequest(sourceUrl, credentials);
            }

            try
            {
                //Try and get a response from the Host, then download the file (if possible)
                fileResponse = webRequest.GetResponse();

                //If it's NOT an FTP Web Response, get the Content Length (FTP Returns -1)
                if (!(fileResponse is FtpWebResponse))
                    expectedFileSize = fileResponse.ContentLength;

                //Get the stream from the Response File
                using (Stream responseStream = fileResponse.GetResponseStream())
                {
                    //Get access to the Local File
                    using (FileStream localFileStream = System.IO.File.Open(destination, FileMode.Create))
                    {
                        byte[] block = new byte[4096];
                        int receivedBytes = 0;

                        //Loop through until our local stream is at least the same size as the remote file
                        while (localFileStream.Length < expectedFileSize)
                        {
                            //Read X Bytes from the server, store how many we actually received, so we know how many to write
                            receivedBytes = responseStream.Read(block, 0, block.Length);

                            //Write the bytes we just received out to the file
                            localFileStream.Write(block, 0, receivedBytes);
                        }

                        //Close the Local File
                        localFileStream.Close();
                    }

                    //Close the Response Stream
                    responseStream.Close();
                }

                //Close the Response
                fileResponse.Close();
            }
            catch (WebException ex)
            {
                throw new WebException("Failed to Retrieve File", ex);
            }

            return true;
        }

        /// <summary>
        /// Sends a file to a Remote Server (FTP/HTTP)
        /// </summary>
        /// <param name="sourceUrl">The Source Url</param>
        /// <param name="credentials">The Credentials to login with</param>
        /// <param name="destination">Where to save the file to?</param>
        /// <returns>True if file was downloaded</returns>
        // public static bool SendFile(File fileToSend, Uri sourceUrl, NetworkCredential credentials, string destination)
        //{
        //    //Create a new Request Object
        //    WebRequest webRequest = CreateRequest(sourceUrl, credentials);
        //    WebResponse fileResponse = null;
        //    long expectedFileSize = 0;

        //    //Store the Credentials, only if they are something, nulls make the FTP Web Request throw exceptions
        //    if (credentials != null)
        //        webRequest.Credentials = credentials;
            

        //    //If it's a FTP Request, we need to get the File Size first. As FTP Responses don't provide the ContentLength on DownloadFile
        //    FtpWebRequest FtpReq = webRequest as FtpWebRequest;
        //    if (FtpReq != null)
        //    {
        //        //Keep the connection alive
        //        FtpReq.KeepAlive = true;

        //        //Change the Method of the Request
        //        FtpReq.Method = WebRequestMethods.Ftp.UploadFile;
        //        //Yes, some importers require this much time!
        //        FtpReq.Timeout = 1200000;                
        //        //Get the Response from the server
        //        fileResponse = webRequest.GetResponse();

        //        //Get the Expected File Size from the FTP Server
        //        expectedFileSize = fileResponse.ContentLength;

        //        //Close the Response
        //        fileResponse.Close();

        //        //We can't just change the Method back todownload file, as this request has already happened, we need to create a whole new WebRequest
        //        webRequest = CreateRequest(sourceUrl, credentials);
        //    }

        //    try
        //    {
        //        //Try and get a response from the Host, then download the file (if possible)
        //        fileResponse = webRequest.GetResponse();

        //        //If it's NOT an FTP Web Response, get the Content Length (FTP Returns -1)
        //        if (!(fileResponse is FtpWebResponse))
        //            expectedFileSize = fileResponse.ContentLength;

        //        //Get the stream from the Response File
        //        using (Stream responseStream = fileResponse.GetResponseStream())
        //        {
        //            //Get access to the Local File
        //            using (FileStream localFileStream = System.IO.File.Open(destination, FileMode.Create))
        //            {
        //                byte[] block = new byte[4096];
        //                int receivedBytes = 0;

        //                //Loop through until our local stream is at least the same size as the remote file
        //                while (localFileStream.Length < expectedFileSize)
        //                {
        //                    //Read X Bytes from the server, store how many we actually received, so we know how many to write
        //                    receivedBytes = responseStream.Read(block, 0, block.Length);

        //                    //Write the bytes we just received out to the file
        //                    localFileStream.Write(block, 0, receivedBytes);
        //                }

        //                //Close the Local File
        //                localFileStream.Close();
        //            }

        //            //Close the Response Stream
        //            responseStream.Close();
        //        }

        //        //Close the Response
        //        fileResponse.Close();
        //    }
        //    catch (WebException ex)
        //    {
        //        throw new WebException("Failed to Retrieve File", ex);
        //    }

        //    return true;
        //}
        #endregion

        #region Extension Methods
        /// <summary>
        /// Exten the String class so that we can check to see if a string IsNumeric easier.
        /// <example>
        /// <code>
        ///     if ("1234".IsNumeric())
        ///         Console.WriteLine("Is A Number!");
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="input">The string that gets checked</param>
        /// <returns>True if numeric</returns>
        public static bool IsNumeric(this string input)
        {
            if (String.IsNullOrEmpty(input.Trim()))
                return false;

            foreach (char letter in input)
                if (!Char.IsNumber(letter) && letter != Convert.ToChar("."))
                    return false;
            return true;
        }

        public static string[] SplitCsvLine(this string input, bool strip)
        {
            string[] columns = System.Text.RegularExpressions.Regex.Split(input, ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))", RegexOptions.Compiled);

            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].StartsWith("\"") && columns[i].EndsWith("\"") && strip)
                    columns[i] = columns[i].Substring(1, columns[i].Length - 2);
                columns[i] = columns[i].Trim();
            }

            return columns;
        }

        public static string[] SplitCsvLine(this string input)
        {
            string[] columns = System.Text.RegularExpressions.Regex.Split(input, ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))", RegexOptions.Compiled);
            for (int i = 0; i < columns.Length; i++)
                columns[i] = columns[i].Trim();

            return columns;
        }
        /// <summary>
        /// Will trim the price in case they come in format 0.00 and will round up in case we get decimals > .00
        /// Will return 0 in case the price is not numeric
        /// </summary>
        /// <param name="input">Price as string from flatfiles</param>
        /// <returns>Price as Integer</returns>

        public static int RoundUpPrice(this string input)
        {
            int price = 0;
            if (IsNumeric(input))
            {
                price = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(input)));
            }

            return price;
        }

        #endregion

        #region Importer Variables
        /// <summary>
        /// How long should we wait before checking if another importer needs to run
        /// </summary>
        public static int MainLoopInterval { get; private set; }
        /// <summary>
        /// If an importer has failed, whats the minimum length of time we should wait before trying to run again (Gives developers time to fix)
        /// </summary>
        public static int FailedImporterRetry { get; private set; }
        #endregion

        #region Error Logging
        public static string MailServer { get; private set; }
        public static string ErrorsTo { get; private set; }

        /// <summary>
        /// This function will deal with any errors that occur, it will also check the current MACHINE running the website against a list of csv MACHINES from the config file, thus only sending errors
        /// out when the machine running isn't in the list, i.e. the web server
        /// </summary>
        /// <param name="exception">The exception that was thrown</param>
        public static string DealWithException(Exception exception)
        {
            //Is the amachine that is running the website in our list?
            Exception InnerException = exception.GetBaseException();
            StringBuilder finalExceptionMessage = new StringBuilder();

            finalExceptionMessage.AppendLine("Import Scheduler:\r\n");
            finalExceptionMessage.AppendFormat("Machine Name:\t\t{0}\r\n", Environment.MachineName.ToString());
            finalExceptionMessage.AppendFormat("TimeStamp:\t\t\t{0}\r\n", DateTime.Now.ToLocalTime());
            finalExceptionMessage.AppendFormat("Application Domain:\t{0}\r\n", AppDomain.CurrentDomain.FriendlyName);
            finalExceptionMessage.AppendFormat("Thread Identity:\t\t{0}\r\n", System.Threading.Thread.CurrentThread.Name);
            finalExceptionMessage.AppendFormat("Executing User:\t\t{0}\r\n", Environment.UserName);
            finalExceptionMessage.AppendFormat("Exception Information:\t\t\r\n---------------------\r\n");
            finalExceptionMessage.AppendFormat("Error Message:\t\t{0}\r\n", exception.ToString());
            finalExceptionMessage.AppendFormat("Error Source:\t\t{0}\r\n", exception.Source);
            finalExceptionMessage.AppendFormat("Target Site:\t\t{0}\r\n", exception.TargetSite);

            // If we have info about a deeper exception, publish.
            if (exception != InnerException)
            {
                finalExceptionMessage.AppendFormat("Base Exception:\t\t{0}\r\n", InnerException.Message);
                finalExceptionMessage.AppendFormat("Base Exception Source:\t{0}\r\n", exception.Source);
            }

            finalExceptionMessage.AppendFormat("\r\nException Stack Trace: \r\n----------------------\r\n");
            finalExceptionMessage.AppendFormat("{0}\r\n\r\n", InnerException.StackTrace);

            //return the string
            return finalExceptionMessage.ToString();
        }

        public static void EmailError(string subject, string message)
        {
            //Setup the MailServer settings so we can send out this email			
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(Global.MailServer);
            client.Send("ImportScheduler@iglu.com", Global.ErrorsTo, subject, message);
        }
        #endregion

        #region File Extraction

        /// <summary>
        /// UnGZip the specified file, leaving all internal files in the same directory
        /// </summary>
        /// <param name="sourceGzip">The GZIP FilePath to Extract</param>
        /// <returns>True if successful</returns>
        public static string UnGzipFile(string sourceGzip)
        {
            return UnGzipFile(sourceGzip, false);
        }

        /// <summary>
        /// UnGZip the specified file, leaving all internal files in the same directory
        /// </summary>
        /// <param name="sourceGzip">The GZIP FilePath to Extract</param>
        /// <param name="force">Whether or not to force this GZip to extract</param>
        /// <returns>True if successful</returns>
        public static string UnGzipFile(string sourceGzip, bool force)
        {
            string path = System.IO.Path.GetDirectoryName(sourceGzip);
            string fileCreated = string.Format("{0}\\{1}", path, Path.GetFileNameWithoutExtension(sourceGzip));

            //Check to see if we should be extracting this file
            if (force || Global.ShouldUpdateFile(fileCreated))
            {
                //Open the GZ File
                using (Stream gzipFileStream = new GZipInputStream(File.OpenRead(sourceGzip)))
                {
                    //Open a new File where we are going to extract to
                    using (FileStream outputFileStream = File.Create(fileCreated))
                    {
                        //Use this block to read from the GZ File
                        byte[] data = new byte[4096];

                        //Keep looping while our outputted file isn't the same size as the GZs contents
                        while (true)
                        {
                            //Read in X Bytes, and store how many bytes we actually managed to read
                            int bytesRead = gzipFileStream.Read(data, 0, data.Length);

                            if (bytesRead > 0)
                                //Write the bytes we read into the output file
                                outputFileStream.Write(data, 0, bytesRead);
                            else
                                break;
                        }

                        //Close the Output File Stream
                        outputFileStream.Close();
                    }

                    //Close the Input GZip File Stream
                    gzipFileStream.Close();
                }
            }
            return fileCreated;
        }

        /// <summary>
        /// Unzip the specified file, leaving all internal files in the same directory
        /// </summary>
        /// <param name="sourceZip">The GZIP FilePath to Extract</param>
        /// <returns>True if successful</returns>
        public static string[] UnzipFile(string sourceZip)
        {
            return UnzipFile(sourceZip, false);
        }

        /// <summary>
        /// Unzip the specified file, leaving all internal files in the same directory
        /// </summary>
        /// <param name="sourceZip">The GZIP FilePath to Extract</param>
        /// <param name="force">Whether or not to force this Zip to extract</param>
        /// <returns>True if successful</returns>
        public static string[] UnzipFile(string sourceZip, bool force)
        {
            string path = System.IO.Path.GetDirectoryName(sourceZip);
            List<string> unzippedFiles = new List<string>();

            //Open the Zip File
            using (ZipInputStream zipFileStream = new ZipInputStream(File.OpenRead(sourceZip)))
            {
                ZipEntry theEntry;
                while ((theEntry = zipFileStream.GetNextEntry()) != null)
                {
                    string outputFile = string.Format("{0}\\{1}", path, theEntry.Name);

                    //Check to see if we should be extracting this file
                    if (force || Global.ShouldUpdateFile(outputFile, theEntry.Size))
                    {
                        //Create a new FileStream for the Outputted File
                        using (FileStream outputFileStream = File.Create(outputFile))
                        {
                            //Use this block to read from the Zip File
                            byte[] data = new byte[4096];

                            //Turns out, #ZipLib isn't always capable of reading the file size! So we can't do this while loop either! We just need to loop until no data is left to read.
                            while (true)
                            {
                                //Read in X Bytes, and store how many bytes we actually managed to read
                                int readBytes = zipFileStream.Read(data, 0, data.Length);

                                //Did we manage to read anything?
                                if (readBytes > 0)
                                {
                                    //Write the bytes we read into the output file
                                    outputFileStream.Write(data, 0, readBytes);
                                }
                                else
                                    break;
                            }

                            //Close the file Stream
                            outputFileStream.Close();
                        }
                    }

                    //Add the output file to a list
                    unzippedFiles.Add(outputFile);
                }

                //Close the Zip File
                zipFileStream.Close();
            }

            //Return whether or not this was successful
            return unzippedFiles.ToArray();
        }
        #endregion
    }

}
