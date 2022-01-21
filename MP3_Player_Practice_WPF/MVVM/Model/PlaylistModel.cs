using System;
using System.Collections.ObjectModel;

namespace MP3_Player_Practice_WPF.MVVM.Model
{
    public class PlaylistModel
    {
        public string Name { get; set; }
        public ObservableCollection<SongModel> Songs { get; set; }
        public DateTime DateCreated { get; set; }
        public int Count { get; set; }
    }
}
