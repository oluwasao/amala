using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.Threading;

namespace NewsImporterService
{
    public partial class NewsImporterService : ServiceBase
    {
        public List<Thread> _allThreads;
        public delegate void ParameterizedThreadStart(Object obj);
        public NewsImporterService()
        {
            InitializeComponent();
            _allThreads = new List<Thread>();
        }

        protected override void OnStart(string[] args)
        {            
            int count = 0;
            foreach (string importer in ConfigurationManager.AppSettings.AllKeys)
            {
                if (importer.Split(':')[0].ToLower() == "importer")
                {
                  Thread thread = new Thread(Import);//create thread
                  thread.IsBackground = count == 0;//assign it as main if it is the first
                  thread.Start(importer);
                  _allThreads.Add(thread);//add to the list
                }
            }
        }

        protected override void OnStop()
        {
            try
            {
                //lets see if the main thread is still running
                if (_allThreads.Find(x => x.IsBackground == false) != null)
                    _allThreads.Find(x => x.IsBackground == false).Abort();
                else
                    //we need to go through the others and abort one by one
                    _allThreads.ForEach(delegate(Thread thread)
                    {
                        try
                        {
                            thread.Abort();
                        }
                        catch (ThreadAbortException ex)
                        {
                            System.Diagnostics.EventLog.WriteEntry("Thread Aborted", "Exception : " + ex.Message);
                        }
                    });
            }
            catch (ThreadAbortException ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Thread Aborted", "Exception : " + ex.Message);
            }

        }

        static void Import(Object key)
        {
            try
            {
                string importer = key.ToString();
                switch (importer)
                {
                    case "Importer:ThisDay":
                        FeedImporter.Importer.ThisDayImporter thisday = new FeedImporter.Importer.ThisDayImporter();
                        thisday.Import(importer);
                        break;
                    case "Importer:Guardian":
                        FeedImporter.Importer.GuardianImporter guardian = new FeedImporter.Importer.GuardianImporter();
                        guardian.Import(importer);
                        break;
                    default:
                        break;
                }
            }
            catch (ThreadAbortException ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Thread Aborted", "Exception : " + ex.Message);
            }
        }
    }
}
