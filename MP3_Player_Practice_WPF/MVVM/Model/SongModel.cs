using System;
using System.Drawing;

namespace MP3_Player_Practice_WPF.MVVM.Model
{
    public class SongModel
    {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; } = "Unknown Artist";
        public double Length { get; set; }
    }
}
