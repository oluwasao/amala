using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Net.Mail;
namespace NewsImporterConsole
{
    public class Program
    {
        protected static int Timeout
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["closeTime"]))
                    return 5000;
                return int.Parse(ConfigurationManager.AppSettings["closeTime"]);
            }
        }
        static void Main(string[] args)
        {
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(Program_UnhandledException);
            foreach (string importer in ConfigurationManager.AppSettings.AllKeys)
            {
                if (importer.Split(':')[0].ToLower() == "importer")
                {
                    Import(importer);
                }
            }
        }

        static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!string.IsNullOrEmpty(FeedImporter.Importer.Importer.ErrorReportAddressFrom) && !string.IsNullOrEmpty(FeedImporter.Importer.Importer.ErrorReportAddressTo))
            {
                MailMessage message = new MailMessage(FeedImporter.Importer.Importer.ErrorReportAddressFrom, FeedImporter.Importer.Importer.ErrorReportAddressTo, "Equity Tracker News Feed Error", String.Concat(((Exception)e.ExceptionObject).Message, Environment.NewLine, ((Exception)e.ExceptionObject).InnerException));
                SmtpClient email = new SmtpClient("smtp.gmail.com", 587);
                email.EnableSsl = true;
                email.DeliveryMethod = SmtpDeliveryMethod.Network;
                email.Credentials = new System.Net.NetworkCredential(FeedImporter.Importer.Importer.ErrorReportAddressFrom, "redrum23");
                email.Send(message);                

                //abort thread
                Thread.CurrentThread.Abort();
            }
        }
        
        static void Import(Object key)
        {
            string importer = key.ToString();
            switch (importer)
            {
                case "Importer:ThisDay":
                    FeedImporter.Importer.ThisDayImporter thisday = new FeedImporter.Importer.ThisDayImporter();
                    Console.WriteLine(string.Format("ThisDay Feed Import Started at {0}", DateTime.Now.ToString()));                    
                    thisday.Import(importer);
                    Console.WriteLine(string.Format("ThisDay Feed Import Finished at {0}", DateTime.Now.ToString()));                    
                    break;
                case "Importer:Guardian":
                    FeedImporter.Importer.GuardianImporter guardian = new FeedImporter.Importer.GuardianImporter();
                    Console.WriteLine(string.Format("Guardian Feed Import Started at {0}", DateTime.Now.ToString()));                    
                    guardian.Import(importer);
                    Console.WriteLine(string.Format("Guardian Feed Import Finished at {0}", DateTime.Now.ToString()));
                    break;
                default:
                    break;
            }
        }
    }
}
