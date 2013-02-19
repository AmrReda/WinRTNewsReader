using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinRTNewsReader.Common.Helpers;
using WinRTNewsReader.Win8App.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace WinRTNewsReader.Win8App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        private const string DOWNLOAD_FEED_TASK = "Feed Donwload Builder";
        private const string DOWNLOAD_TASK_ENTRY_POINT = "WinRTNewsReader.Win8App.NewsReaderTasks.LoadFeedsTask";

        Task RegisterBackgroundTaskAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var item in BackgroundTaskRegistration.AllTasks.Values)
                {
                    if (item.Name == DOWNLOAD_FEED_TASK)
                    {
                        item.Unregister(true);
                    }
                }

                RegisterNewTask();
            });
        }

        void RegisterNewTask()
        {
            var builder = new BackgroundTaskBuilder();
            builder.Name = DOWNLOAD_FEED_TASK;
            builder.TaskEntryPoint = DOWNLOAD_TASK_ENTRY_POINT;
            // Run every 15 minutes if the device has internet connectivity
            var trigger = new TimeTrigger(15, false);
            builder.SetTrigger(trigger);
            //auto condition =ref  new SystemCondition(SystemConditionType::InternetAvailable);

            //builder->AddCondition(condition); 
            builder.Register();
        }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {

            RegisterBackgroundTaskAsync();
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter

                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            var rootFrame = (Frame)Window.Current.Content;
            var mainPage = (MainPage)rootFrame.Content;
            var vm = mainPage.ViewModel;
            if (vm != null)
            {
                PersistenceHelper.SaveFeedsToDBAsync(vm.Feeds).AsTask().ContinueWith(t =>
                {
                    deferral.Complete();
                }, TaskScheduler.Current);
            }
        }
    }
}
