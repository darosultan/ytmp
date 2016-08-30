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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Un4seen.Bass;
using System.Collections.ObjectModel;

namespace ytmp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool dragStarted = false;
        private int _deviceLatencyMS = 0;
        int stream;
        

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
            //Worker workerObject = new Worker();
            //Thread workerThread = new Thread(workerObject.DoWork);
            //workerThread.Start();
            setSlider();

            //playButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
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
            BassPlay();
        }

        public void BassPlay()
        {
            Bass.BASS_StreamFree(stream);
            YTSong song = (YTSong)playListBox.SelectedItem;
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
                    // play the channel
                    Bass.BASS_ChannelPlay(stream, false);

                }
            }
            catch
            {
                int index = playListBox.SelectedIndex;
                playListBox.Items.RemoveAt(index);
                if (index >= playListBox.Items.Count - 1)
                    playListBox.SelectedItem = 0;
                else playListBox.SelectedIndex = index;
                //playListBox.Items.Refresh();
                BassPlay();
            }
            playButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
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
                playButtonIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }
        }

        public void BassStop()
        {
            Bass.BASS_StreamFree(stream);
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
            if (volSlider.Value == 0.0) volIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeOff;
            else if (volSlider.Value < 33) volIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeLow;
            else if (volSlider.Value < 66) volIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMedium;
            else volIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
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

        //private async void loadList()
        //{
        //    if (linkBox.Text != null && linkBox.Text != "")
        //    {
        //        playListBox.Items.Clear();
        //        List<YTSong> songs = YTHelper.gimmeItems(linkBox.Text);
        //        foreach (YTSong song in songs)
        //            playListBox.Items.Add(song);
        //    }
        //}

        private void openButton_Click(object sender, RoutedEventArgs e) //  TODO linki powygasają, inaczej trzeba, nazwy z yt api brać a nie z yt2mp3
        {
            using (new WaitCursor())
            {
                if (linkBox.Text != null && linkBox.Text != "")
                {
                    Cursor = Cursors.Wait;
                    playListBox.Items.Clear();
                    List<YTSong> songs = YTHelper.gimmeItems(linkBox.Text);
                    if (songs != null && songs.Count != 0)
                        foreach (YTSong song in songs)
                            playListBox.Items.Add(song);
                    Cursor = Cursors.Arrow;
                }
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            BassPlayPause();
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

    //public class Worker
    //{
    //    public void DoWork()
    //    {
    //        while (!_shouldStop)
    //        {
    //            Application.Current.MainWindow.posSlider.Dispatcher.Invoke(
    //  System.Windows.Threading.DispatcherPriority.Normal,
    //  new Action(
    //    delegate ()
    //    {
    //        textBox.Text = "Hello World";
    //    }
    //));
    //        }
    //    }

    //    public void RequestStop()
    //    {
    //        _shouldStop = true;
    //    }

    //    private volatile bool _shouldStop = false;
    //}

}
