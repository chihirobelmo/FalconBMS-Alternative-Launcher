using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using System.Xml;

using FalconBMS.Launcher.Input;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace FalconBMS.Launcher.Windows
{
    public class RSSReader
    {
        private static Article[] article = new Article[0];
        public static void Read(string url, System.Windows.Controls.TextBlock tb)
        {
            try
            {
                XmlReader rdr = XmlReader.Create(url);
                SyndicationFeed feed = SyndicationFeed.Load(rdr);

                foreach (SyndicationItem item in feed.Items)
                {
                    Array.Resize(ref article, article.Length + 1);
                    article[article.Length - 1] = new Article(item);
                }
            }
            catch
            {
                tb.Inlines.Add("Web Access Error");
                return;
            }
        }
        public static void Write(System.Windows.Controls.TextBlock textblock)
        {
            foreach (Article art in article)
            {
                art.Write(textblock);
            }
        }
        private class Article
        {
            private string title;
            private string summary;
            private string link;
            public Article()
            {
            }
            public Article(SyndicationItem item)
            {
                title   = item.Title.Text;
                summary = item.Summary.Text;
                link    = item.Id;
            }

            public void Write(System.Windows.Controls.TextBlock tb)
            {
                tb.Inlines.Add(new Run(title) { FontWeight = FontWeights.Bold, FontSize = 18 });
                tb.Inlines.Add("\n");
                tb.Inlines.Add("\n");
                tb.Inlines.Add(new Run(summary) { FontStyle = FontStyles.Italic, FontSize = 14 });
                tb.Inlines.Add("\n");
                tb.Inlines.Add("\n");
                Hyperlink hyperLink = new Hyperlink() {NavigateUri = new Uri(link)};
                hyperLink.Inlines.Add(new Run(">> Read More") { Foreground = Brushes.Aquamarine, FontSize = 12 });
                hyperLink.RequestNavigate += Try_RequestNavigate;
                tb.Inlines.Add(hyperLink);
                tb.Inlines.Add("\n");
                tb.Inlines.Add("\n");
                tb.Inlines.Add("\n");
            }

            private void Try_RequestNavigate(object sender, RequestNavigateEventArgs e)
            {
                System.Diagnostics.Process.Start(link);
            }
        }
    }
}

