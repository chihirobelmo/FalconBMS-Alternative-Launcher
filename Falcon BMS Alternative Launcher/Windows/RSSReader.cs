using System;
using System.Collections.Generic;
using System.Threading;
using System.ServiceModel.Syndication;
using System.Xml;

using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace FalconBMS.Launcher.Windows
{
    public class RSSReader
    {
        static List<Article> s_articles = new List<Article>(10);

        public static void Read(string url)
        {
            Read(url, null);
        }

        public static void Read(string url, string top)
        {
            try
            {
                XmlReader rdr = XmlReader.Create(url);
                SyndicationFeed feed = SyndicationFeed.Load(rdr);

                foreach (SyndicationItem item in feed.Items)
                {
                    Article article = new Article(item, top);
                    s_articles.Add(article);
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
                return;
            }
        }

        public static void Write(System.Windows.Controls.TextBlock textblock)
        {
            //Self-test to assert we're running on the correct UI thread.
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            int textblockThreadId = textblock.Dispatcher.Thread.ManagedThreadId;
            System.Diagnostics.Debug.Assert(currentThreadId == textblockThreadId);
            
            try
            {
                s_articles.Sort();

                foreach (Article article in s_articles)
                {
                    article.Write(textblock);
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex);
                textblock.Inlines.Add("RSS Writing Error");
                return;
            }
        }

        private class Article : IComparable<Article>
        {
            private string webSite;
            private string title;
            private string summary;
            private string link;
            public DateTime dateTime;
            public Article()
            {
            }
            public Article(SyndicationItem item)
            {
                title    = item.Title.Text;
                summary  = item.Summary.Text;
                link     = item.Id;
                dateTime = item.PublishDate.DateTime;
            }
            public Article(SyndicationItem item, string website)
            {
                this.webSite = website;

                title    = item.Title.Text;
                summary  = item.Summary.Text;
                link     = item.Id;
                dateTime = item.PublishDate.DateTime;
            }

            public void Write(System.Windows.Controls.TextBlock tb)
            {
                if (webSite != null)
                {
                    tb.Inlines.Add(new Run("from ") { FontStyle = FontStyles.Italic, FontSize = 12 });
                    Hyperlink topLink = new Hyperlink() { NavigateUri = new Uri(link) };
                    topLink.Inlines.Add(new Run(webSite.Replace("https://","")) { FontStyle = FontStyles.Italic, Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0xff, 0xff, 0xff)), FontSize = 12 });
                    topLink.RequestNavigate += Try_RequestNavigateTop;
                    tb.Inlines.Add(topLink);
                    tb.Inlines.Add("\n");
                }

                tb.Inlines.Add(new Run(title) { FontWeight = FontWeights.Bold, FontSize = 18 });
                tb.Inlines.Add("\n");
                tb.Inlines.Add("\n");

                tb.Inlines.Add(new Run(summary) { FontSize = 14 });
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

            private void Try_RequestNavigateTop(object sender, RequestNavigateEventArgs e)
            {
                System.Diagnostics.Process.Start(webSite);
            }

            int IComparable<Article>.CompareTo(Article other)
            {
                // Default sort-order by date, descending.
                return (-1 * DateTime.Compare(this.dateTime, other.dateTime));
            }
        }
    }
}

