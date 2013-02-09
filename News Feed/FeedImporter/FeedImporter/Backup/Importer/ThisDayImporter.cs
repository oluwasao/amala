using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;

namespace FeedImporter.Importer
{
   public class ThisDayImporter: Importer
    {
       private const string headerPattern = @"\<h2[\s\S]{1,}?\>(?<header>[\s\S]{1,}?)\</h2>";
       private const string mainParaPattern = @"<P\s*align=justify>(?<feed>[\s\S]+?)</P>";
       private const string linkPattern = @"<a\s*href\=[""'](?<link>.+?)[""']\>";

       public ThisDayImporter():base()
       {           
       }

       public override bool Import(string key, bool isConsole)
       {
           
           bool success = false;
           int linksScraped = 0;
           try
           {
               #region Login
               string loginPage = PublicLogic.WebHelper.getPageContentDynamic("http://www.thisdayonline.info/login.php",
                   "textfield=oluwasao&textfield2=jordan23&submit=login", "cookies");
               #endregion
               string mainUrl = ConfigurationManager.AppSettings[key];
               string mainPage = PublicLogic.WebHelper.Sanitize(
                   PublicLogic.WebHelper.getPageContent(string.Format("{0}/news.php?theme=64", mainUrl))
                   );

               //get the source for all the links
               Regex mainRegex = new Regex(@"All\s*news[\s\S]{1,}?\<ul\s*class[\s\S]{1,}?[""']news[\s\S]{1,}?[""'][\s\S]+?\</ul>", _regOptions);
               if (mainRegex.IsMatch(mainPage))
               {
                   string linksMain = mainRegex.Match(mainPage).Value;
                  
                   //get the links from the main value
                   if (Regex.IsMatch(linksMain, @"<li><a\s*href\=[""'](?<link>.+?)[""']\>", _regOptions))
                   {
                       foreach (Match m in Regex.Matches(linksMain, @"<li><a\s*href\=[""'](?<link>.+?)[""']\>", _regOptions))
                       {
                           //for each link get the page.
                           string feed = PublicLogic.WebHelper.Sanitize
                               (
                               PublicLogic.WebHelper.getPageContent(string.Format("{0}{1}", mainUrl, m.Groups["link"].Value))
                               );
                           //strip the data
                           //create a file
                           if (GetFeed(feed, linksScraped))//if the scrape was a success.
                           {
                               linksScraped++;
                               success = true;
                           }
                           if (linksScraped == NumberOfArticlesToCreate)
                               break;
                       }
                   }
               }
               if (isConsole)
                   if (success)
                       Console.WriteLine("ThisDay Import Successful");
                   else
                       Console.WriteLine("ThisDay Import Failure");
           }
           catch (Exception ex)
           {
               throw new Exception(string.Format("Error : {0} ", ex.Message));               
           }
          
           return success;
       }
       public override bool Import(string key)
       {
           return Import(key, false);
       }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="page"></param>
       /// <returns></returns>
       private bool GetFeed(string page, int count)
       {
           if (Regex.IsMatch(page, @"access\s*?denied", _regOptions))
               return false;
           else
           {
               StringBuilder feedBuilder = new StringBuilder();
               if (Regex.IsMatch(page, mainParaPattern, _regOptions)
                   && Regex.IsMatch(page, headerPattern, _regOptions))
               //header               
               {
                   feedBuilder.AppendFormat("<h2>{0}</h2><br />", Regex.Match(page, headerPattern, _regOptions).Groups["header"].Value);
                   feedBuilder.Append(Environment.NewLine);

                   //main paragraph

                   foreach (Match m in Regex.Matches(page, mainParaPattern, _regOptions))                   
                       feedBuilder.Append(m.Groups["feed"].Value);                   
                   WriteToFile(feedBuilder.ToString(), count);
               }
               else
                   return false;
           }
           return true;
       }

       private bool WriteToFile(string s, int count)
       {
           string fileName = string.Format("{0}.{1}.txt", "Thisday", count + 1);           
           //File.WriteAllText(LocalFilePath + fileName, s);
           FtpFile(s, fileName);
           return true;
       }
    }
}
