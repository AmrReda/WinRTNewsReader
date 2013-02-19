
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WinRTNewsReader.Common.Helpers;
using Windows.Foundation;

namespace WinRTNewsReader.Win8App.NewsFormatting
{
    public class NewsFormatter
    {
        private static IDictionary<string, NewsFormatter> s_configuredFormatters = FromConfig();

        public static IDictionary<string, NewsFormatter> FromConfig()
        {
            using (var db = PersistenceHelper.GetSettingsDB())
            {
                var stmt = db.PrepareStatement("SELECT Name, UserAgent, Uri FROM news_formatters");

                var nfs = new Dictionary<string, NewsFormatter>();
                while (stmt.HasMore())
                {
                    var nf = new NewsFormatter();
                    nf.Name = stmt.ColumnAsTextAt(0);
                    nf.UserAgent = stmt.ColumnAsTextAt(1);
                    nf.Uri = stmt.ColumnAsTextAt(2);
                    nfs[nf.Name] = nf;
                }
                return nfs;
            }
        }

        public static NewsFormatter Named(string name)
        {
            return s_configuredFormatters[name];
        }

        public string Name
        {
            get;
            internal set;
        }

        public string Uri
        {
            get;
            internal set;
        }

        public string UserAgent
        {
            get;
            internal set;
        }

        public IAsyncOperation<string> GetFormattedArticleAsync(Uri originalUri)
        {
            string serviceUri = string.Concat(Uri, WebUtility.UrlEncode(originalUri.AbsoluteUri));

            return Task.Factory.StartNew(() =>
            {
                using (var req = new HttpClient())
                {
                    var message = new HttpRequestMessage(HttpMethod.Get, serviceUri);
                    message.Headers.Add("User-Agent", UserAgent);
                    var response = req.SendAsync(message).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
            }).AsAsyncOperation();
        }
    }
}