using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MP3_Player_Practice_WPF.Converters
{
    public class VolumeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(50 < (double)value && (double)value <= 100)
                return "pack://application:,,,/Icons/volume_full_icon.png";
            else if(0 < (double)value && (double)value < 50)
                return "pack://application:,,,/Icons/volume_half_icon.png";
            else
                return "pack://application:,,,/Icons/volume_mute_icon.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
