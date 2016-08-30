using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using Un4seen.Bass;

namespace ytmp
{
    public partial class MainWindow : Window
    {
        private bool dragStarted = false;
        private int _deviceLatencyMS = 0;
        int stream;
        YTPlaylist playlist;
        double oldvol;



        public MainWindow()
        {
            InitializeComponent();


            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_LATENCY, IntPtr.Zero))
            {
                BASS_INFO info = new BASS_INFO();
                Bass.BASS_GetInfo(info);
                Console.WriteLine(info.ToString());
                _deviceLatencyMS = info.latency;
            }
            else
                MessageBox.Show(this, "Bass_Init error!");
            setSlider();
        }

        private async void setSlider()
        {
            while (true)
            {
                if(!dragStarted&&stream!=0)
                {
                    double len = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
                    double pos = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream));
                    double sliderPos= Math.Floor((pos / len) * 100.0);
                    posSlider.Value = sliderPos;
                    if (pos == len)
                        BassNext();
                }
                await Task.Delay(500);
            }

        }

        private void ColorZoneDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        void playListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            playlist.playIndex = playListBox.SelectedIndex;
            BassPlay();
        }

        public void BassPlay()
        {
            if (!playListBox.Items.IsEmpty)
            {
                Bass.BASS_StreamFree(stream);
                YTSong song = playlist.Current();

                titleLabel.Content = song.title;

                var fullFilePath = @"https://i.ytimg.com/vi/" + song.id + "/hqdefault.jpg";
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                bitmap.EndInit();
                image.Source = bitmap;
                try
                {
                    stream = Bass.BASS_StreamCreateURL(YTHelper.createDirectLink(song.id), 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN, null, IntPtr.Zero);
                    if (stream != 0)
                    {
                        setVolume();
                        Bass.BASS_ChannelPlay(stream, false);
                    }
                }
                catch
                {
                    int index = playListBox.SelectedIndex;
                    playlist.Remove(playlist.Current());
                    updatePlayListBox();
                    if (index < playListBox.Items.Count - 1)
                        playListBox.SelectedIndex = index;
                    else
                        playListBox.SelectedIndex = 0;
                    BassPlay();
                }
                playListBox.SelectedIndex = playlist.playIndex;
                playButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }
        }

        public void BassNext()
        {
            if (playlist != null)
            {
                if (playlist.playIndex >= playlist.Count() - 1)
                {
                    if ((bool)repeatButton.IsChecked)
                    {
                        playlist.playIndex = 0;
                        BassPlay();
                    }
                    else
                        BassStop();
                }
                else
                {
                    playlist.playIndex++;
                    BassPlay();
                }
            }
        }

        public void BassPrev()
        {
            if (playlist != null)
            {
                if (playlist.playIndex != 0)
                    playlist.playIndex--;
                BassPlay();
            }
        }

        public void BassPlayPause()
        {
            if(Bass.BASS_ChannelIsActive(stream)==BASSActive.BASS_ACTIVE_PLAYING|| Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_STALLED)
            {
                Bass.BASS_ChannelPause(stream);
                playButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            }
            else if(Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PAUSED)
            {
                Bass.BASS_ChannelPlay(stream,false);
                playButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }
            else
            {
                BassPlay();
            }
        }

        public void BassStop()
        {
            Bass.BASS_StreamFree(stream);
            playButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
        }

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            this.dragStarted = false;
            BassSetPos(posSlider.Value);
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.dragStarted = true;
        }

        private void Slider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (!dragStarted&&posSlider.IsMouseDirectlyOver)
                BassSetPos(posSlider.Value);
        }

        private void BassSetPos(double posSlider)
        {
            double len = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
            double pos = ((posSlider * len) / 100.0);
            Bass.BASS_ChannelSetPosition(stream, pos);
        }

        private void volSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            setVolume();
            if (volSlider.Value == 0.0) volIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeOff;
            else if (volSlider.Value < 33) volIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeLow;
            else if (volSlider.Value < 66) volIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMedium;
            else volIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
        }

        private void setVolume()
        {
            float vol = (float)(volSlider.Value / 100);
            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, vol);
        }

        private void Slider_ValueChanged(object sender, MouseButtonEventArgs e)
        {
            if (!dragStarted)
                BassSetPos(posSlider.Value);
        }

        private void volIcon_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) volSlider.Value += 10.0;
            else if (e.Delta<0) volSlider.Value -= 10.0;
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                if (linkBox.Text != null && linkBox.Text != "")
                {
                    playlist= new YTPlaylist(YTHelper.gimmeItems(linkBox.Text));
                    playlist.shuffle = (bool)shuffleButton.IsChecked;
                    if(playlist.Count()!=0)
                        updatePlayListBox();
                    
                    playListBox.SelectedIndex = 0;
                    BassPlay();
                }
            }
        }

        private void updatePlayListBox()
        {
            playListBox.Items.Clear();
            if(playlist!=null)
            for(int i=0; i<playlist.Count();i++)
            {
                playListBox.Items.Add(playlist[i]);
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            BassPlayPause();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            BassNext();
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if(playlist!=null)playlist.shuffle = (bool)shuffleButton.IsChecked;
            updatePlayListBox();
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            BassPrev();
        }

        private void muteButton_Click(object sender, RoutedEventArgs e)
        {
            if (volSlider.Value != 0)
            {
                oldvol = volSlider.Value;
                volSlider.Value = 0.0;
            }
            else
            {
                if (oldvol != 0.0)
                    volSlider.Value = oldvol;
                else
                    volSlider.Value = 100.0;
            }
        }
    }

    public class WaitCursor : IDisposable
    {
        private Cursor _previousCursor;

        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }

        #endregion
    }
}
