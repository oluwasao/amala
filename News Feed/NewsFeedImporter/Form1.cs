using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Collections;
using System.Diagnostics;
namespace FeedImporter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGuardianImporter_Click(object sender, EventArgs e)
        {
            Scrape(lstThisDay.SelectedItems.Cast<object>().ToArray());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (string importer in ConfigurationManager.AppSettings.AllKeys)
                //if it contains importer
                if (importer.Split(':')[0].ToLower() == "importer")
                    lstThisDay.Items.Add(importer);
        }

        private void btnScrapeAll_Click(object sender, EventArgs e)
        {
            Scrape(lstThisDay.Items.Cast<object>().ToArray());
        }

        /// <summary>
        /// Scrapes the importers in the item
        /// </summary>
        /// <param name="items"></param>
        private void Scrape(Array items)
        {
            StringBuilder builder = new StringBuilder();
            lblStatus.Visible = false;
            foreach (string importer in items)
            {
                switch (importer)
                {
                    case "Importer:ThisDay":
                        Importer.ThisDayImporter thisday = new FeedImporter.Importer.ThisDayImporter();
                        if (thisday.Import(importer))                        
                            builder.Append("ThisDay Import Successful");                         
                        else                        
                            builder.Append("ThisDay Import Failed");
                        builder.Append(Environment.NewLine);
                        break;
                    case "Importer:Guardian":
                        Importer.GuardianImporter guardian = new FeedImporter.Importer.GuardianImporter();                        
                        if (guardian.Import(importer))                        
                            builder.Append("Guardian Import Successful");                                                    
                        else                        
                            builder.Append("Guardian Import Failed");
                        builder.Append(Environment.NewLine);                        
                        break;
                    default:
                        break;
                }
            }

            lblStatus.Text = builder.ToString();
            lblStatus.Visible = true;
        }
    }
}
