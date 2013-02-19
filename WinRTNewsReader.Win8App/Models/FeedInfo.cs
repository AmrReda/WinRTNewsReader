using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WinRTNewsReader.Common.Helpers;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.Web.Syndication;

namespace WinRTNewsReader.Win8App.Models
{
    public sealed class FeedInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FeedItem _selectedItem;
        private Uri _uri;
        private bool _isLoaded;
        private String _title;
        private String _desc;
        private String _errorDesc;
        private Uri _imageUri;
        private IObservableVector<object> _feedItems;


        public FeedInfo(String uri, String title, String imageUri)
        {
            _uri = new Uri(uri);
            _title = title;
            if (imageUri != null)
            {
                _imageUri = new Uri(imageUri);
            }
            _feedItems = new ObservableVector<object>();
        }
        public FeedInfo(String uri)
        {
            _uri = new Uri(uri);
            _feedItems = new ObservableVector<object>();
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public Uri ImageUri
        {
            get { return _imageUri; }
            set
            {
                if (this.ViewModelPropertyChanged<Uri>("ImageUri", value, ref _imageUri, PropertyChanged))
                {
                    this.RaisePropertyChanged(PropertyChanged, "ImageUriExists");
                }
            }
        }


        public bool ImageUriExists
        {
            get { return _imageUri != null; }
        }

        public String Error
        {
            get { return _errorDesc; }
            set { this.ViewModelPropertyChanged("Error", value, ref _errorDesc, PropertyChanged); }
        }

        public String Description
        {
            get { return _desc; }
            set { this.ViewModelPropertyChanged("Description", value, ref _desc, PropertyChanged); }
        }

        public String Title
        {
            get { return _title; }
            set { this.ViewModelPropertyChanged("Title", value, ref _title, PropertyChanged); }
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { this.ViewModelPropertyChanged("IsLoaded", value, ref _isLoaded, PropertyChanged); }
        }

        public IObservableVector<object> FeedItems
        {
            get { return _feedItems; }
        }

        public FeedItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == null)
                    return;
                this.ViewModelPropertyChanged("SelectedItem", value, ref _selectedItem, PropertyChanged);
            }
        }

        public void LoadNonGUI()
        {
            var client = new SyndicationClient();
            client.BypassCacheOnRetrieve = true;
            try
            {
                var feed = client.RetrieveFeedAsync(_uri).AsTask().Result;

                Title = feed.Title.Text;
                Description = feed.Subtitle.Text;
                ImageUri = feed.ImageUri;
                foreach (var si in feed.Items)
                {
                    var fi = new FeedItem(si);
                    fi.LoadFullText();
                    this._feedItems.Add(fi);
                }
                IsLoaded = true;
            }
            catch
            {
                IsLoaded = false;
            }
        }

        public void BeginLoad()
        {
            var client = new SyndicationClient();
            client.BypassCacheOnRetrieve = true;
            client.RetrieveFeedAsync(_uri).AsTask().ContinueWith(t =>
            {
                var feed = t.Result;
                Title = feed.Title.Text;
                Description = feed.Subtitle.Text;
                ImageUri = feed.ImageUri;

                foreach (var si in feed.Items)
                {
                    var fi = new FeedItem(si);
                    fi.LoadFullTextAsync();

                    RunActionOnGUIThread(() =>
                    {
                        this._feedItems.Add(fi);
                    });
                }

                IsLoaded = true;
            }, TaskScheduler.Default);
        }

        private async void RunActionOnGUIThread(Action act)
        {
            try
            {
                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    new DispatchedHandler(act));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

    }
}