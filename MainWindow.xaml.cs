using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;
using dwg = System.Drawing; // Renamed this to resolve ambiguouty of "Color" type
using System.Collections.Generic;
using Krusefy.KMedoidPartitioning;

namespace Krusefy
{
    public partial class MainWindow : Window
    {
        readonly MainWindowViewmodel viewModel = new MainWindowViewmodel();
        internal PlayButtonViewmodel playButtonViewmodel = new PlayButtonViewmodel();
        PlaylistHandler playlistHandler;
        public SolidColorBrush Brush { get; set; }
        public MainWindow()
        {
            //this.viewModel.HeaderBack = new SolidColorBrush(Colors.Red);
            this.DataContext = this.viewModel;

            playlistHandler = new PlaylistHandler(this);
            
            Server server = new Server(playlistHandler, playlistHandler.musicPlayer);
            server.Start();
            InitializeComponent();
            playlistHandler.Startup();

            this.btnPlay.DataContext = playButtonViewmodel;
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDiag = new FolderBrowserDialog();
            folderDiag.SelectedPath = "C:\\\\";

            DialogResult res = folderDiag.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderDiag.SelectedPath))
            {
                string[] directories = Directory.GetDirectories(folderDiag.SelectedPath);
                playlistHandler.CreatePlaylists(directories);
            }
        }

        public void SetAlbumArt(string path)
        {
            // Set the album art
            albumArtViewer.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));

            // Sample some pixels from the album art
            dwg.Bitmap img = new dwg.Bitmap(path);
            List<float[]> sampleColors = new List<float[]>();
            for (int ww = 0; ww < img.Width; ww+=50)
            {
                for (int hh = 0; hh < img.Height; hh+=50)
                {
                    dwg.Color color = img.GetPixel(ww, hh);
                    sampleColors.Add(new float[] { color.R, color.G, color.B });
                }
            }
            KMedoidPartitioner kMedoidPartitioner = new KMedoidPartitioner(sampleColors);
            var skrt = kMedoidPartitioner.GetClusterPoints(2);
            float[] colorFloat = skrt[0];
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)colorFloat[0], (byte)colorFloat[1], (byte)colorFloat[2]));
            //this.viewModel.HeaderBack = brush;
            this.Resources["PrimaryAccent"] = brush;
            //brra.Color = brush.Color;
        }

        public void SetWaveForm(string path)
        {
            BitmapImage waveform = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            seekbarWaveform.Source = waveform;
        }

        private void playlistManager_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (playlistManager.SelectedItem != null)
            {
                Playlist selectedPlaylist = (Playlist)playlistManager.SelectedItem;
                playlistHandler.SetActivePlaylist(selectedPlaylist);
            }
        }

        private void playlistViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (playlistViewer.SelectedItem == null) { return; }

            playlistHandler.musicPlayer.PlayTrack((Track)playlistViewer.SelectedItem);
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            playlistHandler.musicPlayer.PlayPause();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            playlistHandler.musicPlayer.Next();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            playlistHandler.musicPlayer.Prev();
        }

        private void seekbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double positionPercentage = e.GetPosition(seekbar).X/seekbar.ActualWidth;
            seekbar.Value = positionPercentage * seekbar.Maximum;
            playlistHandler.musicPlayer.SeekTo(positionPercentage);
        }

        private void cntxtRemoveFromQueue_Click(object sender, RoutedEventArgs e)
        {
            if (playlistViewer.SelectedItem == null) { return;}
            playlistHandler.musicPlayer.RemoveFromQueue((Track)playlistViewer.SelectedItem);
        }

        private void cntxtAddToQueue_Click(object sender, RoutedEventArgs e)
        {
            if (playlistViewer.SelectedItem == null) { return; }

            playlistHandler.musicPlayer.AddToQueue((Track)playlistViewer.SelectedItem);
        }
    }
}
