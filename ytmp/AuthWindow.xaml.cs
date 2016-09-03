using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using mshtml;

namespace ytmp
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private Uri uri;
        public YTAuth auth;

        public AuthWindow()
        {
            InitializeComponent();
            uri = YTHelper.GetAutenticationUri();
            authBrowser.Navigate(uri);
        }

        private void authBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var doc = ((WebBrowser)sender).Document;
            var element = findElementByID(doc, "code");
            string authCode=null;
            if (element != null)
            {
                authCode = element.getAttribute("value");
                auth = YTHelper.Exchange(authCode);
                this.Close();
            }
            
        }

        private static IHTMLElement findElementByID(object doc, string id)
        {
            IHTMLDocument2 thisDoc;
            if (!(doc is IHTMLDocument2))
                return null;
            else
                thisDoc = (IHTMLDocument2)doc;

            try
            {
                var element = thisDoc.all.OfType<IHTMLElement>()
                    .Where(n => n != null && n.id != null)
                    .Where(e => e.id == id).First();
                return element;
            }
            catch
            {
                return null;
            }
        }
    }
}
