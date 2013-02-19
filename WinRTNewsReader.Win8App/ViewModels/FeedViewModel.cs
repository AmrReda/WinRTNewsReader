using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinRTNewsReader.Common.Helpers;
using WinRTNewsReader.Win8App.Models;
using Windows.Foundation.Collections;

namespace WinRTNewsReader.Win8App.ViewModels
{
    public class FeedViewModel : INotifyPropertyChanged
    {
        private FeedInfo _selectedFeed;
        private IObservableVector<object> _feeds;

        public event PropertyChangedEventHandler PropertyChanged;
        public FeedViewModel()
        {
            Task.Factory.StartNew(() =>
            {
                if (ViewModelHelper.IsConnected)
                {
                    LoadFeedsFromTheInternet();
                }
                else
                {
                    if (LoadFeedsFromDB())
                    {
                        SelectedFeed = (FeedInfo)_feeds[0];
                    }
                }
                this.RaisePropertyChanged(PropertyChanged, "Feeds");
            });
        }

        public void ReloadFromInternet()
        {
            Task.Factory.StartNew(() =>
            {
                LoadFeedsFromTheInternet();
                this.RaisePropertyChanged(PropertyChanged, "Feeds");
            });
        }


        public void LoadFeedsFromTheInternet()
        {
            _feeds = new ObservableVector<object>();
            PersistenceHelper.GetUserFeedsAsync().AsTask().ContinueWith(t =>
            {
                foreach (var uri in t.Result)
                {
                    try
                    {
                        var fi = new FeedInfo(uri);
                        fi.BeginLoad();
                        _feeds.Add(fi);
                    }
                    catch
                    {
                        Debug.WriteLine("Failed to load FeedInfo");
                    }
                }

                if (_feeds.Count > 0)
                {
                    SelectedFeed = (FeedInfo)_feeds[0];
                }
            });
        }

        public bool LoadFeedsFromDB()
        {
            try
            {
                _feeds = new ObservableVector<object>(
                new ObservableCollection<object>(
                    PersistenceHelper.GetFeedsFromDBAsync().AsTask().Result.OfType<object>()));
                return _feeds.Count != 0;
            }
            catch
            {
                Debug.WriteLine("Failed to get feeds from DB due to an error.");
                return false;
            }
        }

        public String Title
        {
            get { return _selectedFeed == null ? null : _selectedFeed.Title; }
        }

        public String Description
        {
            get { return _selectedFeed == null ? null : _selectedFeed.Description; }
        }

        public FeedInfo SelectedFeed
        {
            get { return _selectedFeed; }
            set
            {
                this.ViewModelPropertyChanged("SelectedFeed", value, ref _selectedFeed, PropertyChanged);
            }
        }

        public IObservableVector<object> FeedItems
        {
            get { return _selectedFeed == null ? null : _selectedFeed.FeedItems; }
        }

        public IObservableVector<object> Feeds
        {
            get { return _feeds; }
        }
    }
}