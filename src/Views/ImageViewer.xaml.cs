using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Wpf_test.src.Utils;

namespace Wpf_test
{
    public partial class ImageViewer : Page, INotifyPropertyChanged
    {

        private readonly DispatcherTimer Timer = new();
        private List<string>? paths;
        private int[] times;
        private int time;
        private bool paused;
        private bool minimized = true;
        private int index;
        private double CPanelWidth = 0;
        private MainWindow? parent;

        private string timerText = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string TimerText
        {
            get { return timerText; }

            set { 
                timerText = value;
                OnPropertyChanged();
            }
        }

        // Classic mode
        public ImageViewer(List<string> paths, int[] times)
        {
            this.times = times;
            InitSession(paths);
        }


        // Custom mode
        public ImageViewer(List<string> paths, List<(string, string)> customTimes)
        {
            times = Parser.ParseMultipleTimeToInt(customTimes);
            if (times == null)
            {
                throw new("Error parsing custom times.");
            }
            InitSession(paths);
        }


        // Single mode
        public ImageViewer(List<string> paths, string singleTime)
        {
            times = [Parser.ParseTimeToInt(singleTime)];
            InitSession(paths);
        }



        // INITIALIZATION
        private void InitSession(List<string> paths)
        {
            DataContext = this;
            this.paths = paths;
            time = times[index];
            InitializeComponent();
            ImageBox.Source = Utils.GetBitmapSource(paths[index]);
            SetTimer();
            TimerText = Parser.ParseToTimeString(time);
            Loaded += PageLoaded;
        }

        private void SetTimer()
        {
            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += OnTimedEvent;
            Timer.Start();

        }



        private void StopSession()
        {
            Timer.Stop();
            if (parent is not null)
            {
                parent.AppTitle.Visibility = Visibility.Visible;
                parent.WindowState = WindowState.Normal;
                parent.ResizeMode = ResizeMode.NoResize;
                parent.Width = 800;
                parent.Height = 450;
                parent.TitlebarHover = false;
            }
            
            NavigationService.Navigate(new Home()); 
        }

        private void ControlPanel_Loaded(object sender, RoutedEventArgs e)
        {
            CPanelWidth = ControlPanel.ActualWidth;
        }

        /* -------------------------------------------------------------------- */

        // CALLBACKS

        private void OnTimedEvent(object? source, EventArgs e)
        {

            time--;
            TimerText = Parser.ParseToTimeString(time);
            if (time == 0)
            {
                if (paths != null && index < paths.Count - 1)
                {
                    index++;
                    time = times[index];
                    TimerText = Parser.ParseToTimeString(time);
                    ImageBox.Source = Utils.GetBitmapSource(paths[index]);
                }
                else
                {
                    StopSession();
                }
            }
        }
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow parent)
            {
                this.parent = parent;
                this.parent.TitlebarHover = true;
                this.parent.SetButtonVisibility(true);
            }
        }

        /* -------------------------------------------------------------------- */

        // CLICK HANDLERS

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (paths != null && index > 0)
            {
                index--;
                time = times[index];
                TimerText = Parser.ParseToTimeString(time);
                ImageBox.Source = Utils.GetBitmapSource(paths[index]);
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            paused = !paused;
            if (paused)
            {
                Timer.Stop();
                if (Pause.Content is Image icon)
                {
                    icon.Source = new BitmapImage(new Uri("Icons/play.png", UriKind.Relative));
                }
            }
            else
            {
                if (Pause.Content is Image icon)
                {
                    icon.Source = new BitmapImage(new Uri("Icons/pause.png", UriKind.Relative));
                }
                Timer.Start();
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopSession();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (paths != null && index < paths.Count - 1)
            {
                index++;
                time = times[index];
                TimerText = Parser.ParseToTimeString(time);
                ImageBox.Source = Utils.GetBitmapSource(paths[index]);
            }
        }


        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            int padding = 50;
            minimized = !minimized;
            if (parent is not null)
            {
                if (!minimized)
                {
                    parent.Width = ImageBox.ActualWidth + padding;
                    ControlPanel.Visibility = Visibility.Collapsed;
                    MinimalButtons.Visibility = Visibility.Visible;
                }
                else
                {
                    ControlPanel.Visibility = Visibility.Visible;
                    if (parent.Width < CPanelWidth)
                    {
                        parent.Width = CPanelWidth + padding;
                    }
                    MinimalButtons.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (parent is not null)
            {
                parent.Topmost = true;   
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (parent is not null)
            {
                parent.Topmost = false;
            }
        }
    }
}
