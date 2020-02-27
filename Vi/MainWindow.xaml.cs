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
using Microsoft.Win32;
using Vi.Logics;


namespace Vi
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AudioPlayer Player=new AudioPlayer();
        public MainWindow()
        {
            InitializeComponent();
            Player.PlayingStatusChanged += (s, e) => PlayAudio.Name = e == Status.Playing ? "Pause" : "Play";
            Player.AudioSelected += (s, e) =>
            {
                SliderTrack.Maximum = (int)e.Duration;
                TrackName.Content = e.Name;
                DurationTrack.Content = e.DurationTime.ToString(@"mm\:ss");
                playList.SelectedItem = e.Name;
            };
            Player.ProgressChanged += (s, e) =>
            {
                SliderTrack.Value = (int)e;
                TimeTrack.Content = ((AudioPlayer)s).PositionSpan.ToString(@"mm\:ss");
            };
        }

        private void AddTracks_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Audio Files|*.wav;*.aac;*.wma;*.wmv;*.avi;*.mpg;*.mpeg;" +
                         "*.m1v;*.mp2;*.mp3;*.mpa*.mpe;*.m3u;*.mp4;*.mov;*.3g2;*.3gp2;*.3gp;" +
                         "*.3gpp;*.m4a;*.cda;*.aif;*.aifc;*.aiff;*.mid;*.midi;*.flac;"
            };

            if (dialog.ShowDialog() == true)
            {
                Player.LoadAudio(dialog.FileNames);
                playList.Items.Clear();
                Player.Playlist.ToList().ForEach(item => playList.Items.Add(item));
            }
        }

        private void RandomTracks_OnClick(object sender, RoutedEventArgs e)
        {
            Player.Random = !Player.Random;
        }

        private void NextTrack_OnClick(object sender, RoutedEventArgs e)
        {
            Player.Next();
        }

        private void PlayAudio_OnClick(object sender, RoutedEventArgs e)
        {
            if ((string)((Button)sender).Name == "Play")
                Player.Play();
            else if ((string)((Button)sender).Name == "Pause")
                Player.Pause();
        }

        private void PreviousTrack_OnClick(object sender, RoutedEventArgs e)
        {
            Player.Previous();
        }

        private void RepeatTrack_OnClick(object sender, RoutedEventArgs e)
        {
            Player.Flag = !Player.Flag;
        }

        private void ProgramOff_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SliderVolume_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Volume = (int)((Slider)sender).Value;
        }

        private void SliderTrack_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Player.Position = ((Slider)sender).Value;
        }

        private void PlayList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
                return;
            Player.SelectAudio(((ListBox)sender).SelectedIndex);
        }

        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
