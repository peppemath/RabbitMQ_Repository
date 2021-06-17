using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RabbitMQTutorials
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();

            InitializeWebBrowser();
        }

        private void InitializeWebBrowser()
        {
            wbrTutorials.Navigated += WbrTutorials_Navigated;      
        }



        #region NavigatedEventHandlers
        private void WbrTutorials_Navigated(object sender, NavigationEventArgs e)
        {
            var webBrowser = (WebBrowser)sender;
            SetSilent(webBrowser, true); // make it silent
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
        #endregion

        private void tbcTutorials_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabCtrl= (TabControl)sender;

            var item = tabCtrl.SelectedItem;
            var index = tabCtrl.SelectedIndex;

            wbrTutorials.Navigate(BrokerInfo.NavigateUrl[index]);
        }


        #region create Publisher/Consumer click
        private void btnCreatePublisher_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var type = button.Name.Substring("btnCreatePublisher_".Length);

            Window window;
            switch(type)
            {
                case "Simple_HelloWorld":
                    window = new Simple_HelloWorld_Publisher.MainWindow();
                    break;
                case "WorkQueues":
                    window = new WorkQueues_Publisher.MainWindow();
                    break;
                case "PublishSubscriber":
                    window = new PublishSubscriber_Publisher.MainWindow();
                    break;
                case "Routing":
                    window = new Routing_Publisher.MainWindow();
                    break;
                case "Topics":
                    window = new Topics_Publisher.MainWindow();
                    break;
                case "RPC":
                    window = new RPC_Publisher.MainWindow();
                    break;
                case "PublisherConfirm":
                    window = new PublisherConfirm_Publisher.MainWindow();
                    break;
                default:
                    window = null;
                    break;
            }

            window.Show();
        }

        private void btnCreateConsumer_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var type = button.Name.Substring("btnCreateConsumer_".Length);

            Window window;
            switch (type)
            {
                case "Simple_HelloWorld":
                    window = new Simple_HelloWorld_Consumer.MainWindow();
                    break;
                case "WorkQueues":
                    window = new WorkQueues_Consumer.MainWindow();
                    break;
                case "PublishSubscriber":
                    window = new PublishSubscriber_Consumer.MainWindow();
                    break;
                case "Routing":
                    window = new Routing_Consumer.MainWindow();
                    break;
                case "Topics":
                    window = new Topics_Consumer.MainWindow();
                    break;
                case "RPC":
                    window = new RPC_Consumer.MainWindow();
                    break;
                default:
                    window = null;
                    break;
            }

            window.Show();
        }

        
        #endregion

    }
}
