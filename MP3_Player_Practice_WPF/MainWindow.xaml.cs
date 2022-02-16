using MP3_Player_Practice_WPF.Core;
using MP3_Player_Practice_WPF.MVVM.ViewModel;
using System.Windows;

namespace MP3_Player_Practice_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel wm = new();
        bool isUserDraggingSlider = false;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = wm;
            var resizer = new WindowResizer(this);
            ExitButton.Click += (s, e) => Close();
            MaximizeButton.Click += (s, e) => WindowState ^= WindowState.Maximized; // XOR operator: expression functions the same as WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
        }
        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isUserDraggingSlider = true;
            wm.OnSliderDragged(isUserDraggingSlider);
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isUserDraggingSlider = false;
            wm.Seek(TrackBar.Value);
            wm.OnSliderDragged(isUserDraggingSlider);
        }
    }
}