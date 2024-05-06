using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;
using dwg = System.Drawing; // Renamed this to resolve ambiguouty of "Color" type
using System.Collections.Generic;
using Krusefy.DBSCAN;
using System.Runtime.InteropServices;

namespace Krusefy
{
    public partial class MainWindow : Window
    {
        internal PlayButtonViewmodel playButtonViewmodel = new PlayButtonViewmodel();
        PlaylistHandler playlistHandler;
        public SolidColorBrush Brush { get; set; }
        public MainWindow()
        {
            playlistHandler = new PlaylistHandler(this);
            
            Server server = new Server(playlistHandler, playlistHandler.musicPlayer);
            server.Start();
            InitializeComponent();
            playlistHandler.Startup();

            this.btnPlay.DataContext = playButtonViewmodel;

            // Prevent sleep
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
        }

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

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

            ClusterAnalyzer clusterAnalyzer = new ClusterAnalyzer();
            var colors = clusterAnalyzer.GetAccentColors(new dwg.Bitmap(path));
            Color primaryColor = Color.FromRgb((byte)colors[0].R, (byte)colors[0].G, (byte)colors[0].B);
            Color secondaryColor = Color.FromRgb((byte)colors[1].R, (byte)colors[1].G, (byte)colors[1].B);

            this.Resources["PrimaryAccent"] = new SolidColorBrush(primaryColor);
            this.Resources["SecondaryAccent"] = new SolidColorBrush(secondaryColor);
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
