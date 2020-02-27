using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using WMPLib;

namespace Vi.Logics
{
    public sealed class AudioPlayer
    {
        private readonly WindowsMediaPlayer wmp;
        private readonly DispatcherTimer timer;
        private readonly ObservableCollection<Audio> playList;
        private int currentPosition;

        public Audio CurrentAudio => playList[currentPosition];
        public string[] Playlist => playList.Select((a) => a.Name).ToArray();

        public Status PlayingStatus
        {
            get
            {
                switch (wmp.playState)
                {
                    case WMPPlayState.wmppsStopped:
                        return Status.Stopped;
                    case WMPPlayState.wmppsPaused:
                        return Status.Paused;
                    case WMPPlayState.wmppsPlaying:
                        return Status.Playing;
                    case WMPPlayState.wmppsMediaEnded:
                        return Status.Ended;
                    case WMPPlayState.wmppsTransitioning:
                        return Status.Transitioning;
                    case WMPPlayState.wmppsReady:
                        return Status.Ready;
                    default:
                        return Status.Undefined;
                }
            }
        }

        public double Position
        {
            get { return wmp.controls.currentPosition; }
            set { wmp.controls.currentPosition = value; }
        }

        public TimeSpan PositionSpan => TimeSpan.FromSeconds((int) wmp.controls.currentPosition);

        public int Volume
        {
            get { return wmp.settings.volume; }
            set { wmp.settings.volume = value; }
        }
        Commands commands=new Commands();
        public bool AutoNext { get; set; } = true;
        public bool AutoRestart { get; set; }
        public bool Flag { get; set; } = false;
        public bool Random { get; set; } = false;

        public AudioPlayer()
        {
            wmp = new WindowsMediaPlayer();
            wmp.PlayStateChange += (e) =>
            {
                if (PlayingStatus == Status.Undefined)
                    return;
                PlayingStatusChanged?.Invoke(this, PlayingStatus);
            };
            timer = new DispatcherTimer() {Interval = new TimeSpan(0, 0, 0, 1)};
            timer.Tick += (s, e) =>
            {
                ProgressChanged?.Invoke(this, Position);

                if (PlayingStatus == Status.Stopped || PlayingStatus == Status.Ended)
                    ((DispatcherTimer) s).Stop();

                if ((PlayingStatus == Status.Stopped || PlayingStatus == Status.Ended) && AutoRestart)
                {
                    SelectAudio(currentPosition);
                    return;
                }

                if ((PlayingStatus == Status.Stopped || PlayingStatus == Status.Ended) && AutoNext)
                    SelectAudio(++currentPosition);
                if (Flag == true && PlayingStatus == Status.Playing)
                    Repeat();
                if (Random == true && PlayingStatus == Status.Playing)
                    RandomTrack();
            };
            playList = new ObservableCollection<Audio>();
        }



        public void SelectAudio(int index)
        {
            currentPosition = index;

            if (currentPosition >= playList.Count)
                currentPosition = 0;

            if (currentPosition < 0)
                currentPosition = playList.Count - 1;

            wmp.currentMedia = CurrentAudio.Media;

            ProgressChanged?.Invoke(this, Position);
            AudioSelected?.Invoke(this, CurrentAudio);
            timer.Start();
        }

        public void LoadAudio(string filepath) => playList.Add(new Audio(wmp.newMedia(filepath)));

        public void LoadAudio(params string[] filepath)
        {
            foreach (var file in filepath)
            {
                LoadAudio(file);
            }
        }

        public void Play()
        {
            if (Playlist.Length == 0)
                return;
            timer.Start();
            wmp.controls.play();
        }

        public void Pause()
        {
            timer.Stop();
            wmp.controls.pause();
        }

        public void Stop()
        {
            timer.Stop();
            wmp.controls.stop();
        }

        public void Next()
        {
            Random rnd = new Random();
            if (Random != true)
                SelectAudio(currentPosition + 1);
            if (Random == true)
                SelectAudio(rnd.Next(Playlist.Length));
        }

        public void Previous()
        {
            SelectAudio(currentPosition - 1);
        }

        public void Repeat()
        {
            //Пришлось выдумывать костыль потому что CurrentAudio.DurationTime
            //постоянно возвращал значение после запятой, и не откинуть его было
            //в противном случае проверка никогда бы не закончилась с true
            double valueCurrentAudio = double.Parse(CurrentAudio.DurationTime.TotalSeconds.ToString());
            int newValueCurrentAudio = (int)valueCurrentAudio;
            double x = double.Parse(PositionSpan.TotalSeconds.ToString());
            int y = (int)x;
            if (y >= newValueCurrentAudio - 1)
                Position = 0;

        }

        public void RandomTrack()
        {
            Random rnd = new Random();
            double valueCurrentAudio = double.Parse(CurrentAudio.DurationTime.TotalSeconds.ToString());
            int newValueCurrentAudio = (int)valueCurrentAudio;
            double x = double.Parse(PositionSpan.TotalSeconds.ToString());
            int y = (int)x;
            if (y >= newValueCurrentAudio - 1)
                SelectAudio(rnd.Next(Playlist.Length));
        }

        public event Action<object, Status> PlayingStatusChanged;
        public event Action<object, double> ProgressChanged;
        public event Action<object, Audio> AudioSelected;
    }
}