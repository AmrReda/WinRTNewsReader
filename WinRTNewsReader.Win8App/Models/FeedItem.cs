using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WinRTNewsReader.Common.Helpers;
using WinRTNewsReader.Win8App.NewsFormatting;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.Web.Syndication;

namespace WinRTNewsReader.Win8App.Models
{
    public sealed class FeedItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private SyndicationItem _si;
        private String _title;
        private String _summary;
        private Uri _link;
        private Uri _imageUri;
        private bool _fulltextLoaded;
        private String _fullText;
        private String _fullTextError;

        public FeedItem(String title, String summary, String uri, String image_uri, String fullText)
        {
            _title = title;
            _summary = summary;
            _link = new Uri(uri);
            if (!string.IsNullOrEmpty(image_uri))
            {
                _imageUri = new Uri(image_uri);
            }
            _fullText = fullText;
            _fulltextLoaded = fullText != null;
        }

        public FeedItem(SyndicationItem si)
        {
            _si = si;
            _title = si.Title.Text;
            _summary = si.Summary.Text;

            var doc = si.GetXmlDocument(SyndicationFormat.Rss20);
            var nodes = doc.DocumentElement.GetElementsByTagName("content");
            if (nodes.Length > 0)
            {
                var node = (XmlElement)nodes.First();
                var typeAttr = (XmlAttribute)node.Attributes.GetNamedItem("type");
                if (typeAttr != null && typeAttr.Value.Equals("image/jpeg"))
                {
                    typeAttr = (XmlAttribute)node.Attributes.GetNamedItem("url");
                    if (typeAttr != null)
                    {
                        _imageUri = new Uri(typeAttr.Value);
                    }
                }
            }

            if (si.Links.Count > 0)
            {
                _link = si.Links[0].Uri;
            }
        }

        public void LoadFullText()
        {
            try
            {
                var fulltext = NewsFormatter.Named("Readibility").GetFormattedArticleAsync(_link).AsTask().Result;

                JsonObject obj;
                if (JsonObject.TryParse(fulltext, out obj))
                {
                    FullText = obj.GetNamedString("content");
                }
                FullTextLoaded = true;
            }
            catch
            {
                FullTextLoaded = false;
            }
        }


        public void LoadFullTextAsync()
        {

            NewsFormatter.Named("Readibility").GetFormattedArticleAsync(_link).AsTask().ContinueWith(t =>
            {
                JsonObject obj;
                try
                {
                    var fulltext = t.Result;
                    if (JsonObject.TryParse(fulltext, out obj))
                    {
                        FullText = obj.GetNamedString("content");
                    }
                    FullTextLoaded = true;
                }

                catch (Exception ex)
                {
                    FullText = null;
                    FullTextError = ex.Message;
                    FullTextLoaded = false;
                }

            }, TaskScheduler.Current);
        }

        public String Title
        {
            get { return _title; }
        }

        public Uri Link
        {
            get { return _link; }
        }

        public String Summary
        {
            get { return _summary; }
        }


        public Uri ImageUri
        {
            get { return _imageUri; }
        }

        public bool ImageUriExists
        {
            get { return _imageUri != null; }
        }

        public String FullText
        {
            get { return _fullText; }
            set { this.ViewModelPropertyChanged("FullText", value, ref _fullText, PropertyChanged); }
        }

        public String FullTextError
        {
            get { return _fullTextError; }
            set { this.ViewModelPropertyChanged("FullTextError", value, ref _fullTextError, PropertyChanged); }
        }

        public bool FullTextLoaded
        {
            get { return _fulltextLoaded; }
            set { this.ViewModelPropertyChanged("FullTextLoaded", value, ref _fulltextLoaded, PropertyChanged); }
        }
    }
}