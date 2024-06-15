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

        public static RoutedCommand CloseCommand = new RoutedCommand();

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

            // таким образом мы можем получить файлы в директории
            FileInfo[] fileInfo = dirInfo.GetFiles("*.mp3");
            foreach (FileInfo f in fileInfo)
            {
                filesInfolders.Add(f);
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

            // Установка максимального значения для слайдера позиции
            PositionSlider.Maximum = duration.TotalSeconds;
            TotalDurationLabel.Content = duration.ToString(@"m\:ss");

            // Вывод информации (можно и нужно адаптировать под ваш интерфейс)
            System.Windows.MessageBox.Show($"Title: {title}\nArtist: {artist}\nDuration: {duration}");
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
    }
}