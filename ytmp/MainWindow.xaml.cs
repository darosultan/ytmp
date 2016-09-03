using System.Windows;
using System.Windows.Input;
using System.IO;
using Newtonsoft.Json;
using System;

namespace ytmp
{
    public partial class MainWindow : Window
    {
        public string configFile;
        private string configDir;
        public YTAuth Auth;

        public MainWindow()
        {
            InitializeComponent();

            configDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace('\\', '/') + "/YTMP";
            configFile = configDir + "/config.json";
            if (File.Exists(configFile))
            {
                YTConfig config = JsonConvert.DeserializeObject<YTConfig>(File.ReadAllText(configFile));
                //playerSlide.linkBox.Text = config.url;
                playerSlide.lastOpened = config.url;
                playerSlide.volSlider.Value = config.volume;
                playerSlide.repeatButton.IsChecked = config.repeat;
                playerSlide.shuffleButton.IsChecked = config.shuffle;
                Auth = config.auth;
            }
            else
            {
                if (!Directory.Exists(configDir))
                    Directory.CreateDirectory(configDir);
                File.Create(configFile);
            }
        }

        private void transitionButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)transitionButton.IsChecked)
                transi.SelectedIndex = 1;
            else
                transi.SelectedIndex = 0;
        }

        private void ColorZoneDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e) //!TODO add this to alt+f4
        {
            YTConfig config = new YTConfig(playerSlide.lastOpened, playerSlide.volSlider.Value, (bool)playerSlide.repeatButton.IsChecked, (bool)playerSlide.shuffleButton.IsChecked, Auth);
            string json = JsonConvert.SerializeObject(config);
            File.WriteAllText(configFile, json);
            Close();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

    }
}
