using MP3_Player_Practice_WPF.MVVM.Model;
using NAudioHelperLibrary;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Timers;
using System.Windows.Input;

namespace MP3_Player_Practice_WPF.MVVM.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        #region Private Members

        private MusicPlayer musicPlayer;

        private Timer timer = new Timer() { Interval = 1000 };

        private readonly string path = Microsoft.Win32.Registry.GetValue(@"HKEY_CLASSES_ROOT\ChromeHTML\shell\open\command", null, null) as string;

        private float _savedVolume;

        #endregion

        #region Properties

        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel 
        { 
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The current state of the Musicplayer
        /// </summary>
        public PlaybackState PlaybackState { get; set; }

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
        public ICommand PlayPauseCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand ReplayCommand { get; set; }
        public ICommand OnClosingCommand { get; set; }
        public ICommand RedirectToDonationCommand { get; set; }
        public ICommand PreviousSongCommand { get; set; }
        public ICommand NextSongCommand { get; set; }
        public ICommand PlaylistSelectionChanged { get; set; }
        public ICommand SeekCommand { get; set; }
        public ICommand MuteCommand { get; set; }
        public ICommand NavigateToPlaylist { get; set; }
        public ICommand NavigateToPlaylistCollection { get; set; }
        public ICommand NavigateToSettings { get; set; }

        #endregion

        #region Constrctor
        public MainViewModel()
        {
            PopulateList();

            CurrentViewModel = new PlaylistCollectionViewModel();

            timer.Elapsed += Timer_Elapsed;


            //Must be declared only once; if reload is needed, must resubscribe to PlaybackStopped Event
            musicPlayer = new MusicPlayer(Playlist.Songs[0].FilePath, _volume);

            PlayPauseCommand = new RelayCommand((o) => PlayPause());
            StopCommand = new RelayCommand((o) => Stop());
            ReplayCommand = new RelayCommand((o) => Replay());
            OnClosingCommand = new RelayCommand((o) => OnClosing());
            RedirectToDonationCommand = new RelayCommand((o) => RedirectToDonation());
            PreviousSongCommand = new RelayCommand((o) => PreviousSong());
            NextSongCommand = new RelayCommand((o) => NextSong());
            PlaylistSelectionChanged = new RelayCommand((o) => OnSelectionChanged());
            MuteCommand = new RelayCommand((o) => Mute());
            NavigateToPlaylist = new RelayCommand((o) => 
            {
                if (CurrentViewModel is not PlaylistViewModel)
                    CurrentViewModel = new PlaylistViewModel();
            });
            NavigateToPlaylistCollection = new RelayCommand((o) => 
            {
                if (CurrentViewModel is not PlaylistCollectionViewModel)
                    CurrentViewModel = new PlaylistCollectionViewModel();
            });
            NavigateToSettings = new RelayCommand((o) => 
            { 
                if (CurrentViewModel is not SettingsViewModel) 
                    CurrentViewModel = new SettingsViewModel(); 
            });

            // FIX NEEDED:
            // Option 1: Resubscribe to Event everytime a new Object of MusicPlayer is created
            // Option 2: Recycle same object everytime (memory leak!)            
            musicPlayer.PlaybackStopped += MusicPlayer_PlaybackStopped;
        }

        #endregion

        #region Methods

        private void Mute()
        {
            if (Volume > 0)
                _savedVolume = _volume;

            _ = Volume != 0 ? Volume = 0 : Volume = _savedVolume;
        }

        private void NextSong()
        {
            #region obsolete
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
            #endregion

        }

        private void PreviousSong()
        {
            #region obsolete
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
            #endregion
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SliderPosition = musicPlayer.GetPositionInSeconds();
        }

        private void MusicPlayer_PlaybackStopped()
        {
            #region obsolete
            // TODO: Fix Action in Class Library
            if(timer.Enabled)
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
            #endregion
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
                    timer?.Stop();
                }
                else
                {
                    // Weird exception: timer already disposed

                    timer?.Start();
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
            try
            {
                musicPlayer?.Dispose();
                timer?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnClosing() threw an error: " + ex.Message);
            }
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
            #region oboslete
            if (timer.Enabled)
            {
                timer?.Stop();
            }
            SliderPosition = 0;

            musicPlayer.PlaybackStopped -= MusicPlayer_PlaybackStopped;
            musicPlayer?.Dispose();
            musicPlayer = new MusicPlayer(_selectedSong.FilePath, _volume);
            musicPlayer.PlaybackStopped += MusicPlayer_PlaybackStopped;
            #endregion
        }
        public void OnSelectionChanged()
        {
            // TODO: Move to PlaylistViewModel

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
    public enum PlaybackState
    {
        Playing,
        Pause,
        Stopped
    }
}