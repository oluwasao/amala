using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StockExchangeImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            string tempDownloadPath = "";
            string tempFileNames = "";
            string tempFeedUrl = "";

            //check the config file if it exists
            //get the download directory
            //get all the urls
            System.IO.StringReader reader = new System.IO.StringReader("");
            using (TextReader tr = new StreamReader("Config.txt"))
            {
                ///MarketNews/Downloads/Capnet Files/Capnet Files for 03-01 2013.zip
               tempDownloadPath = tr.ReadLine();
               tempFileNames = tr.ReadLine();
               tempFeedUrl = tr.ReadLine();
            }


            //If everything has been configured correctly then read            
            if(!string.IsNullOrEmpty(tempDownloadPath) && !string.IsNullOrEmpty(tempFileNames) && !string.IsNullOrEmpty(tempFeedUrl))
            {
                List<string> fileNames = new List<string>(); 
                foreach (string tempFile in tempFileNames.Split('=')[1].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    //get all the possible variations
                    string correctFormat = tempFile.Trim().Replace(" ", "-");
                    string[] splits = correctFormat.Split('-');
                    string variation1 = string.Format("{0}-{1} {2}", splits[0], splits[1], splits[2]);
                    string variation2 = string.Format("{0} {1}-{2}", splits[0], splits[1], splits[2]);
                    string variation3 = string.Format("{0} {1} {2}", splits[0], splits[1], splits[2]);
                    
                    fileNames.Add(correctFormat);
                    fileNames.Add(variation1);
                    fileNames.Add(variation2);
                    fileNames.Add(variation3);                    
                }

                GetFiles(fileNames, tempDownloadPath.Split('=')[1].Trim(), tempFeedUrl.Split('=')[1].Trim());
            }
            //Also Get todays feed
            if(!string.IsNullOrEmpty(tempDownloadPath) && !string.IsNullOrEmpty(tempFeedUrl))
            {
                DateTime now = DateTime.Now;
                List<string> candidateDates = new List<string>()
                {
                     now.ToString("dd-MM-yyyy"),
                     now.ToString("dd-MM yyyy"),                     
                     now.ToString("dd MM-yyyy"),
                     now.ToString("dd MM yyyy")
                };

                GetFiles(candidateDates, tempDownloadPath.Split('=')[1].Trim(), tempFeedUrl.Split('=')[1].Trim());
            }                        
        }

        private static void GetFiles(List<string> fileNames, string downloadDestination, string rootUrl)
        {
            PublicLogic.Global.Timeout = 15000;
            foreach (string candidateDate in fileNames)
            {
               string file = candidateDate.Trim();
               UriBuilder builder = new UriBuilder(string.Format("{0} {1}",rootUrl, file));
                Uri uri = builder.Uri;
                PublicLogic.Global.DownloadFile(uri, string.Format("{0}\\{1}", downloadDestination, file.Replace(" ", "-")));
                  
            }
        }
    }
}
