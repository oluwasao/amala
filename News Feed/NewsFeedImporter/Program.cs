using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Net.Mail;

namespace FeedImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.Run(new Form1());
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
#if Debug
            if (!string.IsNullOrEmpty(Importer.Importer.ErrorReportAddressFrom) && !string.IsNullOrEmpty(Importer.Importer.ErrorReportAddressTo))
            {
                MailMessage message = new MailMessage(Importer.Importer.ErrorReportAddressFrom, Importer.Importer.ErrorReportAddressTo, "Equity Tracker News Feed Error", String.Concat(e.Exception.Message, Environment.NewLine, e.Exception.InnerException));
                SmtpClient email = new SmtpClient("smtp.gmail.com",587);
                email.EnableSsl = true;
                email.DeliveryMethod = SmtpDeliveryMethod.Network;
                email.Credentials = new System.Net.NetworkCredential(Importer.Importer.ErrorReportAddressFrom, "redrum23");
                email.Send(message);
            }
#endif
        }
    }
}
