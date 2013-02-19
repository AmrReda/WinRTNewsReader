  
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WinRTNewsReader.Common.Helpers;
using WinRTNewsReader.Win8App.Models;
using Windows.ApplicationModel.Background;

namespace WinRTNewsReader.Win8App.NewsReaderTasks
{
    public sealed class LoadFeedsTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;

        public LoadFeedsTask()
        {
        }

        private void OnCanceled(IBackgroundTaskInstance taskInstance, BackgroundTaskCancellationReason reason)
        {
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //
            // Associate a cancellation handler with the background task.
            //
            taskInstance.Canceled += OnCanceled;

            if (!ViewModelHelper.IsConnected)
            {
                return;
            }

            //
            // Get the deferral object from the task instance, and take a reference to the taskInstance.
            //
            _deferral = taskInstance.GetDeferral();

            var uris = PersistenceHelper.GetUserFeedsAsync().AsTask().Result;
            var feeds = new List<FeedInfo>();
            foreach (var uri in uris)
            {
                try
                {
                    var fi = new FeedInfo(uri);
                    fi.LoadNonGUI();
                    feeds.Add(fi);
                }
                catch
                {
                    Debug.WriteLine("Failed to load FeedInfo");
                }
            }

            PersistenceHelper.SaveFeedsToDBAsync(feeds).AsTask().Wait();
            _deferral.Complete();
        }
    }
}