using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ytmp
{
    public partial class YTProfile : UserControl
    {
        //public YTAuth Auth;
        MainWindow parentWindow;
        Assembly _assembly;
        Stream _imageStream;

        public YTProfile()
        {
            InitializeComponent();
            try
            {
                _assembly = Assembly.GetExecutingAssembly();
                _imageStream = _assembly.GetManifestResourceStream("ytmp.ProfBack.png");
            }
            catch
            {
                MessageBox.Show("Error accessing resources!");
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginLogout();
        }

        private void LoginLogout()
        {
            if (parentWindow.Auth == null)
            {
                AuthWindow authWindow = new AuthWindow();
                authWindow.Owner = Application.Current.Windows[0];
                authWindow.ShowDialog();
                parentWindow.Auth = authWindow.auth;
            }
            else
            {
                YTHelper.RevokeToken(parentWindow.Auth.refresh_token);
                parentWindow.Auth = null;
            }

            setButtonText();
            updateProfileInfo();
        }

        private void updateProfileInfo()
        {
            userPlaylists.Items.Clear();
            profPic.Source = null;
            userName.Text = String.Empty;
            if (parentWindow.Auth != null)
            {
                YTProfileInfo info = YTHelper.GetProfileInfo(parentWindow.Auth.Access_token);
                if (info != null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(info.PictureUrl, UriKind.Absolute);
                    bitmap.EndInit();
                    profPic.Source = bitmap;

                    userName.Text = info.Name;
                }
                List<YTPlaylistItem> userList = YTHelper.getMyPlaylists(parentWindow.Auth.Access_token);
                if (userList != null)
                {
                    foreach (YTPlaylistItem item in userList)
                    {
                        userPlaylists.Items.Add(item);
                    }
                }
            }
        }

        private void setButtonText()
        {
            if (parentWindow.Auth == null)
                loginButton.Content = "Login to Youtube";
            else
                loginButton.Content = "Logout";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow(this);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = _imageStream;
            bitmap.EndInit();
            profBack.Source = bitmap;
            setButtonText();
            updateProfileInfo();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            updateProfileInfo();
        }

        private void userPlaylists_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                if (userPlaylists.SelectedIndex >= 0)
                {
                    parentWindow.playerSlide.linkOpen("list=" + ((YTPlaylistItem)userPlaylists.SelectedItem).Id);
                    parentWindow.transi.SelectedIndex = 0;
                    parentWindow.transitionButton.IsChecked = false;
                }
            }
        }
    }
}
