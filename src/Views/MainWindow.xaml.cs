using System.Windows;
using System.Timers;

namespace Wpf_test
{
    public partial class MainWindow : Window
    {

        private bool titlebarHover;

        public bool TitlebarHover
        {
            get { return titlebarHover; }
            set { titlebarHover = value; }
        }


        public MainWindow()
        {
            InitializeComponent();
            if (ResizeMode != ResizeMode.NoResize) ResizeMode = ResizeMode.NoResize;
        }


        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (TitlebarHover)
            {
                ViewerControls.Visibility = Visibility.Visible;
                AppTitle.Visibility = Visibility.Visible;
                Line.Visibility = Visibility.Visible;
            }
        }

        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (TitlebarHover)
            {
                ViewerControls.Visibility = Visibility.Collapsed;
                AppTitle.Visibility = Visibility.Collapsed;
                Line.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
                ViewerFullscreenButton.Content = "🗗";
            }
            else
            {
                WindowState = WindowState.Normal;
                ViewerFullscreenButton.Content = "🗖";
            }
        }

        public void SetButtonVisibility(bool visible)
        {
            if (visible)
            {
                ViewerControls.Visibility = Visibility.Visible;
                MainControls.Visibility = Visibility.Collapsed;
            }
            else
            {
                ViewerControls.Visibility = Visibility.Collapsed;
                MainControls.Visibility = Visibility.Visible;
            }

        }


    }

}