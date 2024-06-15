using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using Button = System.Windows.Controls.Button;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace MusicPlayerWpf
{
    public partial class MainWindow : Window
    {
        private MediaPlayer _player;
        private DispatcherTimer _timer;
        private bool _isPlaying;
        List<FileInfo> filesInfolders = new List<FileInfo>();
        private List<string> selectedTracks;

        public static RoutedCommand CloseCommand = new RoutedCommand();

        public static RoutedCommand PauseCommand = new RoutedCommand();
        public static RoutedCommand RestartCommand = new RoutedCommand();
        public static RoutedCommand OpenFileCommand = new RoutedCommand();
        public static RoutedCommand ExitCommand = new RoutedCommand();


        public MainWindow()
        {
            InitializeComponent();

            _player = new MediaPlayer();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _isPlaying = false;

            this.DataContext = this;
            FilesDG.ItemsSource = filesInfolders;

            selectedTracks = new List<string>();
        }

        private void OpenFileMI_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                ReadMP3File(openFileDialog.FileName);
                FileNameLb.Content = $"File: {openFileDialog.FileName} now start to play!";
                _player.Open(new Uri(openFileDialog.FileName));
                _player.Play();
                _isPlaying = true;
                _timer.Start();
                PlayPauseBtn.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }
        }

        private void OpenFolderMI_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();

            var fileDialogOk = fileDialog.ShowDialog();

            var filename = fileDialog.SelectedPath;

            DirectoryInfo dirInfo = new DirectoryInfo(filename);

            // Get mp3 files in the directory
            FileInfo[] fileInfo = dirInfo.GetFiles("*.mp3");
            foreach (FileInfo f in fileInfo)
            {
                filesInfolders.Add(f);
                selectedTracks.Add(f.FullName); // Add full path to the selectedTracks list
            }

            FilesDG.Items.Refresh();
        }

        private void PlayPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                _player.Pause();
                _isPlaying = false;
                PlayPauseBtn.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            }
            else
            {
                _player.Play();
                _isPlaying = true;
                _timer.Start();
                PlayPauseBtn.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _player.Stop();
            _isPlaying = false;
            _timer.Stop();
            PlayPauseBtn.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            PositionSlider.Value = 0;
            CurrentPositionLabel.Content = "0:00";
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimazieBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void CloseWindow()
        {
            this.Close();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Close");
        }

        private void ReadMP3File(string filePath)
        {
            var file = TagLib.File.Create(filePath);

            // Получение длины трека
            TimeSpan duration = file.Properties.Duration;

            // Получение артиста
            string artist = file.Tag.FirstAlbumArtist ?? file.Tag.FirstArtist ?? "Unknown Artist";

            // Получение названия трека
            string title = file.Tag.Title ?? "Unknown Title";

            // Получение обложки альбома (если доступна)
            BitmapImage albumArt = null;
            if (file.Tag.Pictures.Length > 0)
            {
                var picture = file.Tag.Pictures[0];
                albumArt = new BitmapImage();
                albumArt.BeginInit();
                albumArt.StreamSource = new MemoryStream(picture.Data.Data);
                albumArt.EndInit();
            }

            // Установка максимального значения для слайдера позиции
            PositionSlider.Maximum = duration.TotalSeconds;
            TotalDurationLabel.Content = duration.ToString(@"m\:ss");

            // Отображение информации о треке
            System.Windows.MessageBox.Show($"Title: {title}\nArtist: {artist}\nDuration: {duration}");

            // Отображение обложки альбома (если доступна)
            if (albumArt != null)
            {
                AlbumArtImage.Source = albumArt;
            }
            else
            {
                // Если обложка отсутствует, можно отобразить стандартное изображение или скрыть элемент
                AlbumArtImage.Source = null; // Например, скрываем изображение обложки
            }
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_player.Source != null && _player.NaturalDuration.HasTimeSpan)
            {
                PositionSlider.Value = _player.Position.TotalSeconds;
                CurrentPositionLabel.Content = _player.Position.ToString(@"m\:ss");
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PositionSlider.IsMouseCaptureWithin)
            {
                _player.Position = TimeSpan.FromSeconds(PositionSlider.Value);
            }
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show($"Commits >= 3: 1 point\nCustom design: 1 point\nPlayer can play & work correctly: 2 points\nHot keys: 1 point\nLoad mp3 info & image: 1 point\n*Can choose track from folder: 1 point\n*Save new playlist: 1 point",
                "Points total: 8 points", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ToggleWindowStateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                ((Button)sender).Content = new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore };
            }
            else
            {
                this.WindowState = WindowState.Normal;
                ((Button)sender).Content = new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize };
            }
        }
        private void SavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Playlist files (*.m3u)|*.m3u";
            if (saveFileDialog.ShowDialog() == true)
            {
                SavePlaylistToFile(saveFileDialog.FileName);
            }
        }
        private void SavePlaylistToFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                foreach (var track in selectedTracks)
                {
                    writer.WriteLine(track);
                }
            }

            System.Windows.MessageBox.Show("Playlist saved successfully!", "Save Playlist", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //горячие клавиши
        private void PauseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_isPlaying)
            {
                _player.Pause();
                _isPlaying = false;
                PlayPauseBtn.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            }
            else
            {
                _player.Play();
                _isPlaying = true;
                _timer.Start();
                PlayPauseBtn.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }
        }

        private void RestartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _player.Stop();
            _player.Play();
            _isPlaying = true;
            _timer.Start();
            PlayPauseBtn.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
        }

        private void OpenFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileMI_Click(sender, e);
        }
        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}