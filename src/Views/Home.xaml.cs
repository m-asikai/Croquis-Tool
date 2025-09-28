using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf_test.src.Utils;

namespace Wpf_test
{
    public partial class Home : Page, INotifyPropertyChanged
    {

        private readonly int THUMBNAIL_SIZE = 80;
        private readonly Thickness MARGIN_0 = new(0);
        private readonly Thickness MARGIN_2 = new(3);
        private string SingleTime = string.Empty;
        private Mode mode = Mode.CLASSIC;
        private readonly Brush? selectedColour = new BrushConverter().ConvertFromString("#7A7A7A") as Brush;


        private readonly int[] Times = {60, 60, 60, 60, 60, 120, 120, 120, 120, 120, 300, 300, 600};
        private readonly List<string> fileNames = [];
        private readonly List<(string, string)> customTimes = [];
        private readonly List<Thumbnail> SelectedImages = [];
        private readonly Dictionary<string, (UIElement Panel, List<Button> OtherButtons)> Panels;
        private readonly OpenFileDialog f = new()
        {
            Multiselect = true,
            Title = "Select image(s).",
            Filter = "Image Files(*.BMP;*.JPG;*.PNG;*.WEBP)|*.BMP;*.JPG;*.PNG;*.WEBP"
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        private string imageInfo = "No images selected";

        public string ImageInfo
        {
            get { return imageInfo; }
            set { 
                imageInfo = value;
                OnPropertyChanged();
            }
        }


        private string timeInfo = "No intervals selected";


        public string TimeInfo
        {
            get { return timeInfo; }
            set { 
                timeInfo = value;
                OnPropertyChanged(); 
            }
        }


        public Home()
        {
            InitializeComponent();
            TimeInfo = $"Selected intervals: {Times.Length}.";
            ImageInfo = "No images selected.";
            DataContext = this;
            Loaded += PageLoaded;
            Panels = new Dictionary<string, (UIElement Panel, List<Button> OtherButtons)>
                {
                    { "Classic", (ClassicPanel, new List<Button> { Single, Custom }) },
                    { "Single",  (SinglePanel, new List<Button> { Classic, Custom }) },
                    { "Custom",  (CustomPanel, new List<Button> { Classic, Single }) }
                };
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow parentWindow)
            {
                parentWindow.SetButtonVisibility(false);
            }
        }


        /* -------------------------------------------------------------------------- */
        // CLICK 
        private async void FileHandler_Click(object sender, RoutedEventArgs e)
        {
            
            bool? success = f.ShowDialog();
            fileNames.AddRange(f.FileNames.Except(fileNames));
            List<string> clone = [..f.FileNames];
            if (success == true)
            {
                await Task.Run(() =>
                {
                    foreach (var path in clone)
                    {
                        Console.WriteLine(path);
                        var bitmap = InitBitmap(path);
                        Application.Current.Dispatcher.Invoke(() => { 
                            Image image = new()
                            {
                                Source = bitmap,
                                Height = THUMBNAIL_SIZE,
                                Width = THUMBNAIL_SIZE,
                                Margin = MARGIN_2,
                            };
                            image.MouseLeftButtonDown += ImageSelected;

                            Border border = new()
                            {
                                Child = image,
                                BorderThickness = MARGIN_0,
                                BorderBrush = Brushes.Transparent,
                            };
                            Thumbnail_Gallery.Children.Add(border);
                        });
                    }
                });
                ImageInfo = fileNames.Count > 0 ? $"Images selected: {fileNames.Count}." : "No images selected.";
            }
            
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (fileNames == null || fileNames.Count == 0)
            {
                MessageBox.Show("No images selected.", "Input error.", MessageBoxButton.OK);
                return;
            }
            if (Window.GetWindow(this) is MainWindow parentWindow)
            {
                parentWindow.ResizeMode = ResizeMode.CanResize;
            }

            if (mode == Mode.CLASSIC)
            {
                if (fileNames.Count != Times.Length)
                {
                    MessageBox.Show($"The quantity of selected images does not match the amount of time intervals. Number of selected images: {fileNames.Count}" +
                        $" Number of time intervals: {Times.Length}.", "Input error.", MessageBoxButton.OK);
                    return;
                }
                NavigationService.Navigate(new ImageViewer(fileNames, Times));
            } else if (mode == Mode.CUSTOM)
            {
                if (fileNames.Count != CountCustomTimes())
                {
                    MessageBox.Show($"The quantity of selected images and entered time intervals do not match. Number of selected images: {fileNames.Count}" +
                        $" Number of entered intervals: {CountCustomTimes()}.", "Input error.", MessageBoxButton.OK);
                    return;
                }
                NavigationService.Navigate(new ImageViewer(fileNames, customTimes));
            } else
            {
                if (SingleTime == null)
                {
                    MessageBox.Show("Select a time value.", "Input error.", MessageBoxButton.OK);
                    return;
                }
                NavigationService.Navigate(new ImageViewer(fileNames, SingleTime));
            }
            
        }

        private void ImageSelected(object sender, MouseButtonEventArgs e)
        {

            if (sender is Image selectedImage &&
                selectedImage.Parent is Border border &&
                selectedImage.Source is BitmapImage bitmap)
            {
                Thumbnail thumbnail = new(border, bitmap.UriSource.LocalPath);
                if (border.BorderBrush == selectedColour)
                {
                    border.BorderBrush = Brushes.Transparent;
                    border.BorderThickness = MARGIN_0;
                    selectedImage.Margin = MARGIN_2;
                    SelectedImages.Remove(thumbnail);
                }
                else
                {
                    border.BorderBrush = selectedColour;
                    border.BorderThickness = MARGIN_2;
                    selectedImage.Margin = MARGIN_0;
                    SelectedImages.Add(thumbnail);
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedImages.Count == 0)
            {
                MessageBox.Show("No images selected.", "Input error.", MessageBoxButton.OK);
                return;
            }
            foreach (var thumbnail in SelectedImages) { 
                Thumbnail_Gallery.Children.Remove(thumbnail.image);
                fileNames.Remove(thumbnail.path);
            }
            ImageInfo = fileNames.Count > 0 ? $"Images selected: {fileNames.Count}" : "No images selected.";
        }


        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Time.Text == "" || Quantity.Text == "") { return; }
            try
            {
                Intervals.Items.Add($"{Parser.ParseToTimeString(Parser.ParseTimeToInt(Time.Text))} x {Quantity.Text}");
                Intervals.Visibility = Visibility.Visible;
                customTimes.Add((Time.Text, Quantity.Text));
                TimeInfo = CreateTimeInfo();
                Time.Text = "";
                Quantity.Text = "";
            } catch (FormatException exception)
            {
                MessageBox.Show(exception.Message, "Input error.", MessageBoxButton.OK);
            }
        }

        private void Confirm_Time_Click(object sender, RoutedEventArgs e)
        {
            if (Single_Time.Text == "") return;
            try
            {
                string time = Parser.ParseToTimeString(Parser.ParseTimeToInt(Single_Time.Text));
                SingleTime = Single_Time.Text;
                Selected_TimeLabel.Text = "Selected interval: ";
                Selected_Time.Text = time;
                Single_Time.Text = "";
            }
            catch (FormatException exception)
            {
                MessageBox.Show(exception.Message, "Input error.", MessageBoxButton.OK);
            }
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Intervals.Items.Count == 0) return;
            int index = Intervals.SelectedIndex;
            Intervals.Items.RemoveAt(index);
            customTimes.RemoveAt(index);
            if (Intervals.Items.Count == 0) Intervals.Visibility = Visibility.Collapsed;
        }

        /* -------------------------------------------------------------------------- */
        // UTILS


        private BitmapImage InitBitmap(string path)
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.DecodePixelWidth = THUMBNAIL_SIZE;
            bitmap.DecodePixelHeight = THUMBNAIL_SIZE;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private void Handle_Mode(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.IsEnabled = false;
                if (Panels.TryGetValue(btn.Name, out var config))
                {
                    ClassicPanel.Visibility = Visibility.Collapsed;
                    SinglePanel.Visibility = Visibility.Collapsed;
                    CustomPanel.Visibility = Visibility.Collapsed;

                    if (btn.Name == "Classic") mode = Mode.CLASSIC;
                    if (btn.Name == "Single") mode = Mode.SINGLE;
                    if (btn.Name == "Custom") mode = Mode.CUSTOM;
                    TimeInfo = CreateTimeInfo();
                    config.Panel.Visibility = Visibility.Visible;

                    foreach (var otherBtn in config.OtherButtons)
                        otherBtn.IsEnabled = true;
                }
            }
        }


        private int CountCustomTimes()
        {
            int count = 0;
            foreach (var time in customTimes)
            {       
                count += int.Parse(time.Item2);
            }
            return count;
        }

        private string CreateTimeInfo()
        {
            string timeInfo = "Intervals selected: ";
            switch (mode){
                case Mode.CLASSIC:
                    if (Times.Length == 0)
                    {
                        timeInfo = "No intervals selected";
                        break;
                    }
                    timeInfo += $"{Times.Length}.";
                    break;
                case Mode.SINGLE:
                    timeInfo = "Select an interval.";
                    break;
                case Mode.CUSTOM:
                    timeInfo += $"{CountCustomTimes()}.";
                    break;
                default:
                    timeInfo = "Something went wrong";
                    break;
            }
            return timeInfo;
        }
        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
