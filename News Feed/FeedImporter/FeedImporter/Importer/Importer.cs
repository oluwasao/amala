using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.IO;
using System.Web;
using System.Net;
using System.Net.Mail;
namespace FeedImporter.Importer
{
    public abstract class Importer
    {        
        //username with which to logon to ftp server
        public virtual string UserName
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["username"]))
                    return "";
                else
                    return
                        ConfigurationManager.AppSettings["username"];
            } 
        }
        //password with which to logon to ftp server
        public virtual string Password
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["password"]))
                    return "";
                else
                    return
                        ConfigurationManager.AppSettings["password"];
            }
        }

        public virtual string ftpLocation
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ftpLocation"]))
                    return "";
                else
                    return
                        ConfigurationManager.AppSettings["ftpLocation"];
            }
        }

        public virtual string LocalFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["localFilePath"]))
                    return "";
                else
                    return
                        ConfigurationManager.AppSettings["localFilePath"];
            }
        }
        public virtual string FtpFileArchivePath
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ftpFileArchivePath"]))
                    return "";
                else
                    return
                        ConfigurationManager.AppSettings["ftpFileArchivePath"];
            }
        }
        public virtual int NumberOfArticlesToCreate
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["numberOfArticlesToScrape"]))
                    return 0;
                else
                    return
                       int.Parse( ConfigurationManager.AppSettings["numberOfArticlesToScrape"]);
            }
        }

        public static string ErrorReportAddressTo
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ErrorReportAddressTo"]))
                    return "";
                else
                    return
                        ConfigurationManager.AppSettings["ErrorReportAddressTo"];
            }
        }

        public static string ErrorReportAddressFrom
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ErrorReportAddressFrom"]))
                    return "";
                else
                    return
                        ConfigurationManager.AppSettings["ErrorReportAddressFrom"];
            }
        }
        public static System.Text.RegularExpressions.RegexOptions _regOptions = RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            | System.Text.RegularExpressions.RegexOptions.IgnoreCase
            | RegexOptions.Multiline;
        public virtual bool Import(string key)
        {
            return true;
        }
        public virtual bool Import(string key, bool isConsole)
        {
            return true;
        }
        public Importer()
        {            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public FileInfo CreateAndSaveFile(string content, string filePath)
        {
            return PublicLogic.SystemIOHelper.CreateAndSaveFile(content, filePath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public void FtpFile(FileInfo file)
        {
            PublicLogic.WebHelper.FtpFile(file, ftpLocation, UserName, Password);
        }

        /// <summary>
        /// Uses a memorystream
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public void FtpFile(string content, string fileName)
        {           
            if (!string.IsNullOrEmpty(ftpLocation))
                //send to ftp server
                PublicLogic.WebHelper.FtpFileFromMemoryStream(fileName, content, ftpLocation, UserName, Password,UnicodeEncoding.UTF8);
            if (!string.IsNullOrEmpty(FtpFileArchivePath))
                //send to another folder
                PublicLogic.WebHelper.FtpFileFromMemoryStream(fileName, content, FtpFileArchivePath, UserName, Password,UnicodeEncoding.UTF8);
            SaveFileLocally(content, fileName);
        }

        /// <summary>
        /// Tries to save a file locally
        /// </summary>
        public void SaveFileLocally(string content, string fileName)
        {
            if (string.IsNullOrEmpty(LocalFilePath))
                return;
            PublicLogic.SystemIOHelper.CreateAndSaveFile(content, LocalFilePath + fileName);
        }

        public static void DealWithException(Exception e)
        {
            if (!string.IsNullOrEmpty(FeedImporter.Importer.Importer.ErrorReportAddressFrom) && !string.IsNullOrEmpty(FeedImporter.Importer.Importer.ErrorReportAddressTo))
            {
                MailMessage message = new MailMessage(FeedImporter.Importer.Importer.ErrorReportAddressFrom, FeedImporter.Importer.Importer.ErrorReportAddressTo, "Equity Tracker News Feed Error", String.Concat(e.Message, Environment.NewLine, e.InnerException));
                SmtpClient email = new SmtpClient("smtp.gmail.com", 587);
                email.EnableSsl = true;
                email.DeliveryMethod = SmtpDeliveryMethod.Network;
                email.Credentials = new System.Net.NetworkCredential(FeedImporter.Importer.Importer.ErrorReportAddressFrom, "redrum23");
                email.Send(message);

            }
        }
    }
}
