using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;

namespace FeedImporter.Importer
{
    public class GuardianImporter:Importer
    {
        //public static Regex firstPageRegex = new Regex(@"<a\s*href\=[""'](?<link>/capital_market.+?)[""']", _regOptions);

        public static Regex linksUrlRegex = new Regex("contentheading[\\s\\S]*?<a\\s*href=\"(?<link>[\\s\\S]{1,}?)\"[\\s\\S]{1,}?></a>", _regOptions);
        //use this for their well formed html tags 
        //public static Regex feedFullRegex = new Regex(@"<p class[\s\S]+?</p>", _regOptions);
        
        //use this where the tags are not well formed
        //public static Regex feedFullRegex = new Regex(@"<p>[\s\S]+", _regOptions);
        //public static Regex contentFullRegex = new Regex(@"<FONT SIZE =\+[1]\>(?<header>[\s\S]{1,}?)\</font>[\s\S]{1,}?" 
        //    + "<FONT SIZE =\\+[1]\\>(?<uppercase>.{1}?)\\<[\\s\\S]+?</b>(?<maintext>[\\s\\S]+?)\\</p>", _regOptions);
        public static Regex headerRegEx = new Regex("contentheading[\\s\\S]*?<a\\s*href=\"[\\s\\S]{1,}?>(?<header>[\\s\\S]{1,}?)</a>",_regOptions);
        public static Regex contentFullRegex = new Regex("article-content[\\s\\S]*?<div>(?<maintext>[\\s\\S]*?)</div>", _regOptions);
        public GuardianImporter(): base()
        {

        }
        //Version 1.0
        //public override bool Import(string key, bool isConsole)
        //{            
        //    bool success = false;
        //    try
        //    {
        //        string mainUrl = ConfigurationManager.AppSettings[key];
        //        //get the main page
        //        string mainPage = PublicLogic.WebHelper.Sanitize(
        //           PublicLogic.WebHelper.getPageContent(mainUrl)
        //           );

        //        //get the link from here to the next Page
        //        if (firstPageRegex.IsMatch(mainPage))
        //        {
        //            //get the url to the next page
        //            string feedHubUrl = firstPageRegex.Match(mainPage).Groups["link"].Value;
        //            string feedHub = PublicLogic.WebHelper.Sanitize(
        //           PublicLogic.WebHelper.getPageContent(string.Format("{0}{1}", mainUrl, feedHubUrl)
        //           ));

        //            //grab individaul links and get each
        //            if (linksUrlRegex.IsMatch(feedHub))
        //            {
        //                int count = 0;
                        
        //                foreach (Match m in linksUrlRegex.Matches(feedHub))
        //                {
        //                    Regex feedFullRegEx;
        //                    string feedUrl = m.Groups["link"].Value;
        //                    string feedPage = PublicLogic.WebHelper.Sanitize(
        //                       PublicLogic.WebHelper.getPageContent(feedUrl));
        //                    //get the main paragraph
        //                    if (TryMatchFeedPage(feedPage, out feedFullRegEx))
        //                    {
        //                        string mainContent = feedFullRegEx.Match(feedPage).Value;
        //                        //now get the paragraph and header
        //                        StringBuilder feedBuilder = new StringBuilder();
        //                        string header = "";
        //                        string content = "";
        //                        if (contentFullRegex.IsMatch(mainContent))
        //                        {
        //                            header = contentFullRegex.Match(mainContent).Groups["header"].Value;
        //                            content = contentFullRegex.Match(mainContent).Groups["uppercase"].Value +
        //                                contentFullRegex.Match(mainContent).Groups["maintext"].Value;
        //                            feedBuilder.AppendFormat("<h2>{0}</h2></br>", header);
        //                            feedBuilder.Append(Environment.NewLine);
        //                            feedBuilder.Append(content);
        //                            //write to file

        //                            WriteToFile(feedBuilder.ToString(), count);
        //                            success = true;
        //                            count++;
        //                        }
        //                        //if the count is the same as the maximum number break
        //                        if (count == NumberOfArticlesToCreate)
        //                            break;
        //                    }
        //                }
        //            }
        //        }
        //        if (isConsole)
        //            if (success)
        //                Console.WriteLine("Guardian Import Successful");
        //            else
        //                Console.WriteLine("Guardian Import Failed");
        //    }
            
        //   catch (Exception ex)
        //   {
        //       throw new Exception(string.Format("Error : {0} ", ex.Message));
        //   }
        //    return success;
        //}
        //Version 1.1
        //public override bool Import(string key, bool isConsole)
        //{
        //    bool success = false;
        //    try
        //    {
        //        string mainUrl = ConfigurationManager.AppSettings[key];
        //        //get the main page
        //        string mainPage = PublicLogic.WebHelper.Sanitize(
        //           PublicLogic.WebHelper.getPageContent(mainUrl)
        //           );

        //        //get the link from here to the next Page
        //        if (firstPageRegex.IsMatch(mainPage))
        //        {
        //            //get the url to the next page
        //            string feedHubUrl = firstPageRegex.Match(mainPage).Groups["link"].Value;
        //            string feedHub = PublicLogic.WebHelper.Sanitize(
        //           PublicLogic.WebHelper.getPageContent(string.Format("{0}{1}", mainUrl, feedHubUrl)
        //           ));

        //            //grab individaul links and get each
        //            if (linksUrlRegex.IsMatch(feedHub))
        //            {
        //                int count = 0;

        //                foreach (Match m in linksUrlRegex.Matches(feedHub))
        //                {
        //                    Regex feedFullRegEx;
        //                    string feedUrl = m.Groups["link"].Value;
        //                    string feedPage = PublicLogic.WebHelper.Sanitize(
        //                       PublicLogic.WebHelper.getPageContent(feedUrl));
        //                    //get the main paragraph
        //                    if (TryMatchFeedPage(feedPage, out feedFullRegEx))
        //                    {                             
        //                        //now get the paragraph and header
        //                        StringBuilder feedBuilder = new StringBuilder();
        //                        string header = "";
        //                        string content = "";
        //                        if (contentFullRegex.IsMatch(feedPage))
        //                        {
        //                            header = contentFullRegex.Match(feedPage).Groups["header"].Value;
        //                            content = contentFullRegex.Match(feedPage).Groups["uppercase"].Value +
        //                                contentFullRegex.Match(feedPage).Groups["maintext"].Value;
        //                            feedBuilder.AppendFormat("<h2>{0}</h2></br>", header);
        //                            feedBuilder.Append(Environment.NewLine);
        //                            feedBuilder.Append(content);
        //                            //write to file

        //                            WriteToFile(feedBuilder.ToString(), count);
        //                            success = true;
        //                            count++;
        //                        }
        //                        //if the count is the same as the maximum number break
        //                        if (count == NumberOfArticlesToCreate)
        //                            break;
        //                    }
        //                }
        //            }
        //        }
        //        if (isConsole)
        //            if (success)
        //                Console.WriteLine("Guardian Import Successful");
        //            else
        //                Console.WriteLine("Guardian Import Failed");
        //    }

        //    catch (Exception ex)
        //    {
        //        throw new Exception(string.Format("Error : {0} ", ex.Message));
        //    }
        //    return success;
        //}

        public override bool Import(string key, bool isConsole)
        {
            
            bool success = false;
            try
            {             
                string mainUrl = ConfigurationManager.AppSettings[key];
                //get the main page
                string mainPage = PublicLogic.WebHelper.Sanitize(
                   PublicLogic.WebHelper.getPageContent(string.Format("{0}/{1}",mainUrl,"/index.php?option=com_content&view=category&layout=blog&id=27&Itemid=422"))
                   );
              
                    //get the url to the next page                   

                    //grab individaul links and get each
                if (linksUrlRegex.IsMatch(mainPage))
                    {
                        int count = 0;

                        foreach (Match m in linksUrlRegex.Matches(mainPage))
                        {
                            Regex feedFullRegEx;
                            string feedUrl = m.Groups["link"].Value;
                            string feedPage = PublicLogic.WebHelper.Sanitize(
                               PublicLogic.WebHelper.getPageContent(string.Format("{0}/{1}",mainUrl,feedUrl)));
                            //get the main paragraph
                            if (TryMatchFeedPage(feedPage, out feedFullRegEx))
                            {
                                //now get the paragraph and header
                                StringBuilder feedBuilder = new StringBuilder();
                                string header = "";
                                string content = "";
                                if (contentFullRegex.IsMatch(feedPage) && headerRegEx.IsMatch(feedPage))
                                {
                                    header = headerRegEx.Match(feedPage).Groups["header"].Value;
                                    content = contentFullRegex.Match(feedPage).Groups["maintext"].Value;
                                    feedBuilder.AppendFormat(string.Format("<h2>{0}</h2><br />", header));
                                    feedBuilder.Append(Environment.NewLine);
                                    feedBuilder.Append(content);
                                    //write to file

                                    WriteToFile(feedBuilder.ToString(),  count);
                                    success = true;
                                    count++;                                    
                                }
                                //if the count is the same as the maximum number break
                                if (count == NumberOfArticlesToCreate)
                                    break;
                            }
                        }
                    }
               
                if (isConsole)
                    if (success)
                        Console.WriteLine("Guardian Import Successful");
                    else
                        Console.WriteLine("Guardian Import Failed");
            }

            catch (Exception ex)
            {
                throw new Exception(string.Format("Error : {0} ", ex.Message));
            }
            return success;
        }
        
        public override bool Import(string key)
        {
          return  Import(key, false);
        }

        private bool WriteToFile(string s, int count)
        {
            string fileName = string.Format("{0}.{1}.txt", "Guardian", count + 1);
            //File.WriteAllText(LocalFilePath + fileName, s);
            FtpFile(s, fileName);
            return true;
        }

        private bool TryMatchFeedPage(string feedPage, out Regex feedFullRegex)
        {
            feedFullRegex = null;
            if (Regex.IsMatch(feedPage, @"<p class[\s\S]+?</p>", _regOptions))
            {
                feedFullRegex = new Regex(@"<p class[\s\S]+?</p>", _regOptions);
                return true;
            }
            else if(Regex.IsMatch(feedPage, @"<p>[\s\S]+", _regOptions))
            {
                feedFullRegex = new Regex(@"<p>[\s\S]+", _regOptions);
                return true;
            }

            return false;
        }
    }
}
