using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.IO;
using Un4seen.Bass;
using Newtonsoft.Json;
using System.Windows.Controls;

namespace ytmp
{
    /// <summary>
    /// Interaction logic for YTPlayer.xaml
    /// </summary>
    public partial class YTPlayer : UserControl
    {
        private bool dragStarted = false;
        private bool changedByTimer = false;
        private int stream;
        public YTPlaylist playlist;
        private double oldvol;
        public string lastOpened;

        public YTPlayer()
        {
            InitializeComponent();

            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_LATENCY, IntPtr.Zero))
            {
                BASS_INFO info = new BASS_INFO();
                Bass.BASS_GetInfo(info);
                Console.WriteLine(info.ToString());
            }

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += setSlider;
            timer.Start();

            if (lastOpened != null)
                linkOpen(lastOpened);
        }

        private void setSlider(object sender, EventArgs e)
        {
            if (!dragStarted && stream != 0)
            {
                long lenbytes = Bass.BASS_ChannelGetLength(stream);
                long posbytes = Bass.BASS_ChannelGetPosition(stream);
                double len = Bass.BASS_ChannelBytes2Seconds(stream, lenbytes);
                double pos = Bass.BASS_ChannelBytes2Seconds(stream, posbytes);
                //double sliderPos= Math.Floor((pos / len) * 100.0);
                posSlider.Maximum = len;
                timerLabel.Content = YTHelper.timeFromSeconds(pos, len);
                changedByTimer = true;
                //posSlider.Value = sliderPos;
                posSlider.Value = pos;
                changedByTimer = false;
                if (posbytes != -1 && posbytes >= lenbytes - 10) //sometimes posbytes gets higher than lenbytes, sometimes posbytes gets stuck lower than lenbytes
                    BassNext();
            }
        }



        void playListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                playlist.playIndex = playListBox.SelectedIndex;
                BassPlay();
            }
        }

        public void BassPlay()
        {
            using (new WaitCursor())
            {
                if (!playListBox.Items.IsEmpty)
                {

                    try
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

                        string directlink = YTHelper.getAudioDirectLink(song.id);
                        if (directlink != null)
                            stream = Bass.BASS_StreamCreateURL(directlink, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN, null, IntPtr.Zero);
                        else
                            BrokenSong();
                        if (stream != 0)
                        {
                            setVolume();
                            Bass.BASS_ChannelPlay(stream, false);
                        }
                    }
                    catch
                    {
                        BrokenSong();
                    }
                    playListBox.SelectedIndex = playlist.playIndex;
                    playButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                }
            }
        }

        public void BrokenSong()
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

        public void BassNext()
        {
            BassStop();
            if (playlist != null)
            {
                if (playlist.playIndex >= playlist.Count() - 1)
                {
                    if ((bool)repeatButton.IsChecked)
                    {
                        playlist.playIndex = 0;
                        BassPlay();
                    }
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
            if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING || Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_STALLED)
            {
                Bass.BASS_ChannelPause(stream);
                playButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            }
            else if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PAUSED)
            {
                Bass.BASS_ChannelPlay(stream, false);
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
            changedByTimer = true;
            posSlider.Value = 0.0;
            changedByTimer = false;
            playButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            timerLabel.Content = "0:00 / 0:00";
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
            if (!dragStarted && !changedByTimer)
                BassSetPos(posSlider.Value);
        }

        private void BassSetPos(double posSlider)
        {
            //double len = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
            //double pos = ((posSlider * len) / 100.0);
            Bass.BASS_ChannelSetPosition(stream, posSlider);
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

        private void volIcon_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) volSlider.Value += 10.0;
            else if (e.Delta < 0) volSlider.Value -= 10.0;
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                linkOpen(linkBox.Text);
            }
        }

        public void linkOpen(string link)
        {
            BassStop();
            if (link != null && link != "")
            {
                playlist = new YTPlaylist(YTHelper.getPlaylistItems(link));
                lastOpened = link;
                linkBox.Text = String.Empty;
                playlist.shuffle = (bool)shuffleButton.IsChecked;
                if (playlist.Count() != 0)
                    updatePlayListBox();
                playlist.playIndex = 0;
                playListBox.SelectedIndex = 0;
                //BassPlay();
            }
        }

        private void updatePlayListBox()
        {
            playListBox.Items.Clear();
            if (playlist != null)
                foreach (YTSong song in playlist.GetList())
                {
                    playListBox.Items.Add(song);
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
            if (playlist != null) playlist.shuffle = (bool)shuffleButton.IsChecked;
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
}
