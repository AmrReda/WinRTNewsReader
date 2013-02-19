using WinRTNewsReader.Common.Helpers;
using WinRTNewsReader.Win8App.Common;
using WinRTNewsReader.Win8App.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WinRTNewsReader.Win8App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : LayoutAwarePage
    {
        public FeedViewModel ViewModel
        {
            get { return (FeedViewModel)DataContext; }
            set { DataContext = value; }
        }

        public bool IsPortrait
        {
            get { return Window.Current.Bounds.Height > Window.Current.Bounds.Width; }
        }


        private string _lastText;
        public MainPage()
        {
            this.InitializeComponent();
            ft.LayoutUpdated += HandleTextChanged;
            ftpP.LayoutUpdated += HandleTextChanged;
            Window.Current.SizeChanged += DisplayProperties_OrientationChanged;
        }

        private void DisplayProperties_OrientationChanged(object sender, WindowSizeChangedEventArgs args)
        {
            DisplayArticleText();
        }

        private void HandleTextChanged(object sender, object args)
        {
            if (string.Compare(_lastText, ft.Text) == 0)
                return;

            _lastText = ft.Text;
            DisplayArticleText();
        }

        private void DisplayArticleText()
        {
            object style;
            Application.Current.Resources.TryGetValue("ArticleText", out style);

            var content = HtmlToXamlConverter.GetTextElementFromHTML(ft.Text);
            content.Style = style as Style;
            if (IsPortrait)
            {
                contentViewP.FlowDirection = content.FlowDirection;
                contentViewP.Content = content;
            }
            else
            {
                contentView.FlowDirection = content.FlowDirection;
                contentView.Content = content;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var vm = ViewModel;
            if (vm == null)
            {
                vm = new FeedViewModel();
                ViewModel = vm;
            }
        }

        private void HandleReloadClick(object sender, RoutedEventArgs e)
        {
            var vm = ViewModel;
            if (vm != null)
            {
                vm.ReloadFromInternet();
            }
        }

        private void HandleDropDownOpen(object sender, object e)
        {
            var combo = sender as ComboBox;
            if (combo == null)
                return;

            var popup = combo.FindName("Popup");
        }

    }
}
