using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TagLib;
using System.IO;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace Krusefy
{
    public class PlaylistHandler
    {
        internal List<Playlist> Playlists = new List<Playlist>();
        public MainWindow mainWindow;
        public MainWindowVM mainWindowVM;
        public MusicPlayer musicPlayer;
        public char driveLetter;

        public PlaylistHandler(MainWindow m, MainWindowVM mainWindowVM)
        {
            this.mainWindow = m;
            this.mainWindowVM = mainWindowVM;
            this.musicPlayer = m.MusicPlayer;
        }
        public void Startup()
        {
            if (Directory.GetFiles(".\\playlists\\").Length > 0)
            {
                foreach (string filePath in Directory.GetFiles(".\\playlists\\"))
                {
                    // Get playlist name from file name
                    string[] splitPath = filePath.Split('\\');
                    string fileName = splitPath[splitPath.Length - 1];
                    Playlist playlist = new Playlist(fileName.Substring(0, fileName.Length - 4));
                    
                    string playlistFilePath = ".\\playlists\\" + playlist.Name + ".txt";

                    if (!System.IO.File.Exists(playlistFilePath)) { continue; }

                    string[] fileLines = System.IO.File.ReadAllLines(playlistFilePath);
                    //playlist.CreateJSON(fileLines);
                    playlist.SetTracks(fileLines);

                    this.Playlists.Add(playlist);
                }
                this.mainWindow.playlistManager.ItemsSource = this.Playlists;
            }
        }

        internal async void CreatePlaylists(string[] dirs)
        {
            // Get playlist directory, for local use
            DirectoryInfo di = new DirectoryInfo(".\\playlists\\");
            EmptyDirectory(di);
            this.Playlists.Clear();
            foreach (string dir in dirs)
            {
                // Get playlist name from directory
                string[] splitdir = dir.Split('\\');
                driveLetter = splitdir[0].First();
                string playlistName = splitdir[splitdir.Length - 1];

                Playlist playlist = new Playlist(playlistName);
                playlist.ArtistDir = dir;
                await Task.Run(() => playlist.CreateTxt());
                this.Playlists.Add(playlist);
                mainWindow.playlistManager.Items.Refresh();
            }
        }
        
        private void EmptyDirectory(DirectoryInfo di)
        {
            foreach (FileInfo fi in di.EnumerateFiles())
            {
                fi.Delete();
            }
        }
    }
}
