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
using Path = System.IO.Path;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

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
        public static readonly RoutedCommand SkipNextCommand = new RoutedCommand();
        public static readonly RoutedCommand SkipPreviousCommand = new RoutedCommand();
        //перемешать музыку
        private Random _random = new Random();

        //список треков
        public List<Track> Tracks { get; set; } = new List<Track>();
        private int currentTrackIndex = -1; // Индекс текущего выбранного трека

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

        //прибавить звук
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_player != null)
            {
                _player.Volume = VolumeSlider.Value;
            }
        }

        private void OpenFileMI_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    var fileInfo = TagLib.File.Create(filename);
                    var track = new Track
                    {
                        FilePath = filename,
                        Title = fileInfo.Tag.Title ?? Path.GetFileName(filename),
                        Artist = fileInfo.Tag.FirstPerformer ?? "Unknown Artist",
                        Duration = fileInfo.Properties.Duration.ToString(@"mm\:ss"),
                        AlbumArt = fileInfo.Tag.Pictures.FirstOrDefault()?.Data?.Data
                    };

                    Tracks.Add(track);
                }

                FilesDG.ItemsSource = null;
                FilesDG.ItemsSource = Tracks;

                if (Tracks.Count > 0 && Tracks[0].AlbumArt != null)
                {
                    using (var ms = new MemoryStream(Tracks[0].AlbumArt))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        AlbumArtImage.Source = bitmap;
                    }
                }
            }
        }

        private void OpenFolderMI_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    var fileInfo = TagLib.File.Create(filename);
                    var track = new Track
                    {
                        FilePath = filename,
                        Title = fileInfo.Tag.Title ?? Path.GetFileName(filename),
                        Artist = fileInfo.Tag.FirstPerformer ?? "Unknown Artist",
                        Duration = fileInfo.Properties.Duration.ToString(@"mm\:ss"),
                        AlbumArt = fileInfo.Tag.Pictures.FirstOrDefault()?.Data?.Data
                    };

                    Tracks.Add(track);
                }

                FilesDG.ItemsSource = null;
                FilesDG.ItemsSource = Tracks;

                if (Tracks.Count > 0 && Tracks[0].AlbumArt != null)
                {
                    using (var ms = new MemoryStream(Tracks[0].AlbumArt))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        AlbumArtImage.Source = bitmap;
                    }
                }
            }
        }

        private void PlayPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrackIndex == -1 && Tracks.Count > 0)
            {
                currentTrackIndex = 0;
                PlayTrack();
            }
            else if (_isPlaying)
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

        private void PlayTrack()
        {
            if (currentTrackIndex >= 0 && currentTrackIndex < Tracks.Count)
            {
                var track = Tracks[currentTrackIndex];
                if (!string.IsNullOrEmpty(track.FilePath))
                {
                    _player.Open(new Uri(track.FilePath));
                    _player.Play();
                    _isPlaying = true;
                    _timer.Start();
                    PlayPauseBtn.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;

                    if (track.AlbumArt != null)
                    {
                        using (var ms = new MemoryStream(track.AlbumArt))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = ms;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            AlbumArtImage.Source = bitmap;
                        }
                    }
                    else
                    {
                        AlbumArtImage.Source = null;
                    }

                    // Subscribe to MediaEnded event to automatically play next track
                    _player.MediaEnded += Player_MediaEnded;
                }
            }
        }
        private void Player_MediaEnded(object sender, EventArgs e)
        {
            // Stop the timer
            _timer.Stop();

            // Play next track automatically
            Dispatcher.Invoke(() =>
            {
                SkipNextBtn_Click(null, null);
            });
        }
        private void ShuffleTracks()
        {
            int n = Tracks.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                Track value = Tracks[k];
                Tracks[k] = Tracks[n];
                Tracks[n] = value;
            }

            FilesDG.ItemsSource = null;
            FilesDG.ItemsSource = Tracks;

            // Reset current track index after shuffle
            currentTrackIndex = -1;
        }
        private void ShuffleBtn_Click(object sender, RoutedEventArgs e)
        {
            ShuffleTracks();
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
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Playlist files (*.m3u)|*.m3u";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SavePlaylistToFile(saveFileDialog.FileName);
            }
        }

        private void SavePlaylistToFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                foreach (var track in Tracks)
                {
                    writer.WriteLine(track.FilePath);
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

        private void FilesDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilesDG.SelectedItem is Track selectedTrack)
            {
                currentTrackIndex = Tracks.IndexOf(selectedTrack);
                PlayTrack();
            }
        }

        private void SkipPreviousBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Tracks.Count > 0)
            {
                currentTrackIndex--;
                if (currentTrackIndex < 0)
                {
                    currentTrackIndex = Tracks.Count - 1;
                }
                PlayTrack();
            }
        }

        private void SkipNextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Tracks.Count > 0)
            {
                currentTrackIndex++;
                if (currentTrackIndex >= Tracks.Count)
                {
                    currentTrackIndex = 0;
                }
                PlayTrack();
            }
        }


        private void SkipNextCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SkipNextBtn_Click(sender, e);
        }

        private void SkipPreviousCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SkipPreviousBtn_Click(sender, e);
        }

        public class Track
        {
            public string FilePath { get; set; }
            public string Title { get; set; }
            public string Artist { get; set; }
            public string Duration { get; set; }
            public byte[] AlbumArt { get; set; }
        }
    }
}
