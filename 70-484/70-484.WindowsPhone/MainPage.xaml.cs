using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace _70_484
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region dp Orientation { get; set; }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof (DisplayOrientations), typeof (MainPage),
            new PropertyMetadata(default(DisplayOrientations)));

        public DisplayOrientations Orientation
        {
            get { return (DisplayOrientations) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion

        public ICommand ClickCommand { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            ClickCommand = new ClickCommand(() => ShareTest());
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Tile update
            XmlDocument tileXml =
                TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText01);
            var str = tileXml.GetXml();
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = "Hello World! My very own tile notification";
            TileNotification tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);


            //tile badge update
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            var str1 = badgeXml.GetXml();
            XmlElement badgeElement = (XmlElement) badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", "7");
            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);


            // Secondary badge update
            // Define the badge content
            /*var badgeNotification = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            var badgeAttributes = (XmlElement) badgeNotification.SelectSingleNode("/badge");
            badgeAttributes.SetAttribute("value", "6");
            var secondaryTileBadge = new BadgeNotification(badgeNotification);
            var tileUpdater = BadgeUpdateManager.CreateBadgeUpdaterForSecondaryTile("SecondaryTile.Dynamic");
            tileUpdater.Update(secondaryTileBadge);*/

            // didplay information
            var di = DisplayInformation.GetForCurrentView();
            Observable.FromEventPattern<TypedEventHandler<DisplayInformation, object>, DisplayInformation, object>(
                x => di.OrientationChanged += x, x => di.OrientationChanged -= x)
                .ObserveOnDispatcher()
                .Subscribe(x => Orientation = di.CurrentOrientation);

            Observable.Interval(TimeSpan.FromSeconds(15))
                .Skip(10)
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(x => TestNotification());
        }

        public void TestNotification()
        {
            // The getTemplateContent method returns a Windows.Data.Xml.Dom.XmlDocument object
            // that contains the toast notification XML content.
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText01);
            var str = toastXml.GetXml();
            // You can use the methods from the XML document to specify the required elements for the toast.
            //var images = (XmlElement) toastXml.SelectSingleNode("image");
            //images.SetAttribute("src", "images/toastImageAndText.png");

            var textNodes = toastXml.GetElementsByTagName("text");
            for (var i = 0; i < textNodes.Length; i++)
            {
                var textNumber = i + 1;
                var text = "";
                for (var j = 0; j < 10; j++)
                {
                    text += "Text input " + textNumber;
                }
                textNodes[i].AppendChild(toastXml.CreateTextNode(text));
            }
            // Create a toast notification from the XML, then create a ToastNotifier object
            // to send the toast.
            var toast = new ToastNotification(toastXml);

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void ShareTest()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareImageHandler;
            DataTransferManager.ShowShareUI();
        }

        private async void ShareImageHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "Share Image Example";
            //request.Data.Properties.Description = "Demonstrates how to share an image.";

            // Because we are making async calls in the DataRequested event handler,
            //  we need to get the deferral first.
            DataRequestDeferral deferral = request.GetDeferral();

            // Make sure we always call Complete on the deferral.
            try
            {
                StorageFile thumbnailFile =
                    await Package.Current.InstalledLocation.GetFileAsync("Images\\index100.jpg");
                request.Data.Properties.Thumbnail =
                    RandomAccessStreamReference.CreateFromFile(thumbnailFile);
                StorageFile imageFile =
                    await Package.Current.InstalledLocation.GetFileAsync("Images\\index.jpg");
                request.Data.SetStorageItems(new[] {imageFile});
                //request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(imageFile));
                /*request.Data.SetUri(
                    new Uri(
                        @"http://blogs.msdn.com/b/metroapps/archive/2012/07/15/access-your-application-assets-folder.aspx"));*/
            }
            finally
            {
                deferral.Complete();
            }

            // To support pull operations (event handler will be called on request from target app
            /*e.Request.Data.SetDataProvider(StandardDataFormats.Bitmap,
        new DataProviderHandler(this.OnDeferredRequestedHandler));*/

        }

        private async void OnDeferredRequestedHandler(DataProviderRequest request)
        {
            throw new NotImplementedException();
        }
    }


    public class ClickCommand : ICommand
    {
        private readonly Action m_Action;

        public ClickCommand(Action mAction)
        {
            m_Action = mAction;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            m_Action.Invoke();
            
        }

        public event EventHandler CanExecuteChanged;
    }
}
