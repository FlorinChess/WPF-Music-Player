using MP3_Player_Practice_WPF.MVVM.Model;
using NAudioHelperLibrary;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Timers;

namespace MP3_Player_Practice_WPF.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        #region Private Members

        private MusicPlayer musicPlayer;

        private Timer timer = new Timer() { Interval = 1000 };

        private readonly string path = Microsoft.Win32.Registry.GetValue(@"HKEY_CLASSES_ROOT\ChromeHTML\shell\open\command", null, null) as string;

        private float _savedVolume;

        #endregion

        #region Properties
        private double _sliderPosition = 0;
        public double SliderPosition
        {
            get => _sliderPosition;
            set
            {
                _sliderPosition = value;
                OnPropertyChanged();
            }
        }

        private float _volume = 10; // default volume - TODO: load from saved preferences file (.json)        
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                musicPlayer.SetVolume(_volume);
                OnPropertyChanged();
            }
        }

        private double _trackLength = 1;

        public double TrackLength
        {
            get => _trackLength;
            set
            {
                _trackLength = value;
                OnPropertyChanged();
            }
        }


        private SongModel _selectedSong;

        public SongModel SelectedSong
        {
            get => _selectedSong;
            set
            {
                if (value != _selectedSong) { _selectedSong = value; }
                OnPropertyChanged();
            }
        }
        public PlaylistModel Playlist { get; set; }

        private bool _isAutoplayEnabled = true;
        public bool IsAutoplayEnabled
        {
            get => _isAutoplayEnabled;
            set
            {
                _isAutoplayEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool IsShuffleEnabled { get; set; }
        public bool IsReplayEnabled { get; set; }
        #endregion

        #region Commands
        public RelayCommand PlayPauseCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand ReplayCommand { get; set; }
        public RelayCommand OnClosingCommand { get; set; }
        public RelayCommand RedirectToDonationCommand { get; set; }
        public RelayCommand PreviousSongCommand { get; set; }
        public RelayCommand NextSongCommand { get; set; }
        public RelayCommand PlaylistSelectionChanged { get; set; }
        public RelayCommand SeekCommand { get; set; }
        public RelayCommand MuteCommand { get; set; }

        #endregion

        #region Constrctor
        public MainViewModel()
        {
            PopulateList();

            timer.Elapsed += Timer_Elapsed;
            musicPlayer = new MusicPlayer(Playlist.Songs[0].FilePath, _volume, MusicPlayer_PlaybackStopped);

            PlayPauseCommand = new RelayCommand((o) => PlayPause());
            StopCommand = new RelayCommand((o) => Stop());
            ReplayCommand = new RelayCommand((o) => Replay());
            OnClosingCommand = new RelayCommand((o) => OnClosing());
            RedirectToDonationCommand = new RelayCommand((o) => RedirectToDonation());
            PreviousSongCommand = new RelayCommand((o) => PreviousSong());
            NextSongCommand = new RelayCommand((o) => NextSong());
            PlaylistSelectionChanged = new RelayCommand((o) => OnSelectionChanged());
            MuteCommand = new RelayCommand((o) => Mute());

            musicPlayer.PlaybackStopped += MusicPlayer_PlaybackStopped;
        }

        #endregion

        #region Methoden

        private void Mute()
        {
            if (Volume > 0)
                _savedVolume = _volume;

            _ = Volume != 0 ? Volume = 0 : Volume = _savedVolume;
        }

        private void NextSong()
        {
            // TODO: don't repeat selected song

            if (musicPlayer != null)
            {
                ResetPlayer();

                int index = Playlist.Songs.IndexOf(_selectedSong);
                if (Playlist.Songs.Count > index)
                {
                    SelectedSong = Playlist.Songs[index + 1];
                }

                PlayPause();
            }
        }

        private void PreviousSong()
        {
            // TODO: don't repeat selected song

            if (musicPlayer != null)
            {
                ResetPlayer();

                int index = Playlist.Songs.IndexOf(_selectedSong);
                if (Playlist.Songs.Count > index && 0 <= index)
                {
                    SelectedSong = Playlist.Songs[index - 1];
                }

                PlayPause();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SliderPosition = musicPlayer.GetPositionInSeconds();
        }

        private void MusicPlayer_PlaybackStopped()
        {
            // TODO: Fix Action in Class Library

            timer.Stop();

            int index = Playlist.Songs.IndexOf(_selectedSong);
            if (Playlist.Songs.Count > index)
            {
                SelectedSong = Playlist.Songs[index + 1];

            }

            ResetPlayer();

            if (_isAutoplayEnabled)
            {
                PlayPause();
            }
        }

        private void PopulateList()
        {
            Playlist = new PlaylistModel() { Name = "Sample Playlist", DateCreated = System.DateTime.Now };
            Playlist.Songs = new ObservableCollection<SongModel>();

            foreach (var musicFile in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)))
            {
                if (musicFile.EndsWith(".mp3") || musicFile.EndsWith(".wav"))
                {
                    Playlist.Songs.Add(new SongModel()
                    {
                        FilePath = musicFile,
                        Name = TagLib.File.Create(musicFile).Tag.Title,
                        Artist = TagLib.File.Create(musicFile).Tag.FirstPerformer,
                    });
                }
            }

            

            //var f = TagLib.File.Create(@"C:\Users\Dell\Music");

            //// Load you image data in MemoryStream
            //TagLib.IPicture pic = f.Tag.Pictures[0];
            //MemoryStream ms = new MemoryStream(pic.Data.Data);
            //ms.Seek(0, SeekOrigin.Begin);

            //// ImageSource for System.Windows.Controls.Image
            //BitmapImage bitmap = new BitmapImage();
            //bitmap.BeginInit();
            //bitmap.StreamSource = ms;
            //bitmap.EndInit();

            //// Create a System.Windows.Controls.Image control
            //System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            //img.Source = bitmap;

            foreach (var song in Playlist.Songs)
            {
                using MusicPlayer mp = new MusicPlayer(song.FilePath, _volume);
                song.Length = mp.GetLengthInSeconds();
            }
        }

        public void PlayPause()
        {
            if (_selectedSong != null)
            {
                TrackLength = (int)_selectedSong.Length;
                musicPlayer.TogglePlayPause(_volume);

                if (timer.Enabled)
                {
                    timer.Stop();
                }
                else
                {
                    timer.Start();
                }
            }
        }
        public void Stop()
        {
            if (timer.Enabled)
            {
                timer.Stop();
            }
            musicPlayer?.Stop();

            ResetPlayer();
        }
        public void Replay()
        {
            if (IsReplayEnabled)
            {

            }
        }
        public void Seek(double value)
        {
            musicPlayer?.SetPosition(value);
        }
        public void OnClosing()
        {
            musicPlayer?.Dispose();
            timer?.Dispose();
        }
        public void RedirectToDonation()
        {
            if (path != null)
            {
                string[] split = this.path.Split('\"');
                var path = split.Length >= 2 ? split[1] : null;
            }
            System.Diagnostics.Process.Start(path, "https://ko-fi.com/florin_chess#paypalModal");
        }
        public void ResetPlayer()
        {
            if (timer.Enabled)
            {
                timer.Stop();
            }
            SliderPosition = 0;

            musicPlayer?.Dispose();
            musicPlayer = new MusicPlayer(_selectedSong.FilePath, _volume);
        }
        public void OnSelectionChanged()
        {
            musicPlayer.PlaybackStopType = PlaybackStopTypes.PlaybackStoppedByUser;

            if (timer.Enabled)
            {
                timer.Stop();
            }
            ResetPlayer();
            PlayPause();
        }
        public void OnSliderDragged(bool isCurrentlyDragging)
        {
            if (!isCurrentlyDragging)
            {
                if (!timer.Enabled)
                    timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        #endregion

    }
}