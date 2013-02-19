using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.Networking.Connectivity;
using Windows.UI.Core;

namespace WinRTNewsReader.Common.Helpers
{
    public static class ViewModelHelper
    {
        public static bool IsConnected
        {
            get { return NetworkInformation.GetInternetConnectionProfile() != null; }
        }

        public static bool ViewModelPropertyChanged<T>(
            this INotifyPropertyChanged viewModel,
            string propName,
            T newValue,
            ref T existingValue,
            PropertyChangedEventHandler handler)
        {
            if (existingValue == null)
            {
                if (newValue == null)
                {
                    return false;
                }
            }
            else if (existingValue.Equals(newValue))
            {
                return false;
            }

            existingValue = newValue;
            RaisePropertyChanged(viewModel, handler, propName);
            return true;
        }




        public static async void RaisePropertyChanged(this INotifyPropertyChanged viewModel, PropertyChangedEventHandler handler, string propName)
        {
            if (handler == null)
                return;

            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                          new Windows.UI.Core.DispatchedHandler(() =>
                          {
                              handler(viewModel, new PropertyChangedEventArgs(propName));
                          }));
        }
    }
}