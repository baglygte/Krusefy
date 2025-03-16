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
using System.Windows.Threading;

namespace Krusefy
{
    public class PlaylistHandler
    {
        private const string PlaylistDirectory = ".\\playlists\\";
        internal List<Playlist> Playlists = new List<Playlist>();
        public MainWindow mainWindow;
        public MainWindowVM mainWindowVM;
        public MusicPlayer musicPlayer;

        public PlaylistHandler(MainWindow m, MainWindowVM mainWindowVM)
        {
            this.mainWindow = m;
            this.mainWindowVM = mainWindowVM;
            this.musicPlayer = m.MusicPlayer;
            this.mainWindowVM.PlaylistViewerVM.Playlists = Playlists;
        }
        public void ReadPlaylistTxts()
        {
            // Read .txt files from playlists folder
            if (!Directory.Exists(PlaylistDirectory)) { return; }
            if (Directory.GetFiles(PlaylistDirectory).Length == 0) { return; }

            DirectoryInfo dirInfo = new DirectoryInfo(PlaylistDirectory);
            foreach (var info in dirInfo.EnumerateFiles())
            {
                string[] splitstring = info.Name.Split('.');
                AddPlaylist(splitstring[0]);
            }
        }
        internal async void CreatePlaylistTxts(string folderPath)
        {
            // Create a .txt file for each playlist in the folder
            PreparePlaylistsDirectory();

            this.Playlists.Clear();

            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

            foreach (var info in dirInfo.EnumerateDirectories())
            {
                if (info.Attributes.HasFlag(FileAttributes.System) || info.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    continue;
                }

                await Task.Run(() => CreateTxt(info.FullName));
                string[] splitstring = info.FullName.Split('\\');
                AddPlaylist(splitstring[splitstring.Length - 1]);
            }
        }
        private string CreateTxt(string artistFilePath)
        {
            // Looks through an artist directory and fills a .txt file with information about files
            // in said directory

            if (!Directory.Exists(artistFilePath)) { return null; }

            string[] splitstring = artistFilePath.Split('\\');
            string txtFilePath = PlaylistDirectory + splitstring[splitstring.Length - 1] + ".txt";
            StreamWriter sw = new StreamWriter(txtFilePath);

            // Singles are assumed to be in the root artist directory
            string[] singlePaths;
            singlePaths = Directory.GetFiles(artistFilePath);

            foreach (string singlePath in singlePaths)
            {
                if (singlePath.EndsWith(".mp3"))
                {
                    string playlistString = GetTags(singlePath);
                    playlistString += "||" + singlePath;
                    sw.WriteLine(playlistString);
                }
                else if (singlePath.EndsWith(".jpg") | singlePath.EndsWith(".jpeg"))
                {
                    // Do nothing on purpose, so we only write to debug if we encountered an unknown file
                }
                else
                {
                    Debug.WriteLine("Unknown file encountered:" + singlePath);
                }
            }

            // Albums are assumed to be in subdirectories of the artist directory
            string[] albumPaths = Directory.GetDirectories(artistFilePath);
            foreach (string albumPath in albumPaths)
            {
                string[] trackPaths = Directory.GetFiles(albumPath);
                foreach (string trackPath in trackPaths)
                {
                    if (trackPath.EndsWith(".mp3"))
                    {
                        string playlistString = GetTags(trackPath);
                        playlistString += "||" + trackPath;
                        sw.WriteLine(playlistString);
                    }
                    else if (trackPath.EndsWith(".jpg") | trackPath.EndsWith(".jpeg"))
                    {
                        // Do nothing on purpose, so we only write to debug if we encountered an unknown file
                    }
                    else
                    {
                        Debug.WriteLine("Unknown file encountered: " + trackPath);
                    }
                }
            }
            sw.Close();

            return txtFilePath;
        }
        private void AddPlaylist(string playlistName)
        {
            Playlist playlist = new Playlist(playlistName);
            Playlists.Add(playlist);
            playlist.CreateTracks(PlaylistDirectory + playlistName + ".txt");
        }
        private void PreparePlaylistsDirectory()
        {
            DirectoryInfo di = new DirectoryInfo(PlaylistDirectory);
            if (!di.Exists) 
            {
                Directory.CreateDirectory(PlaylistDirectory);
                return;
            }

            foreach (FileInfo fi in di.EnumerateFiles())
            {
                fi.Delete();
            }            
        }
        private string GetTags(string filepath)
        {
            TagLib.File trackFile = TagLib.File.Create(filepath);
            string tagString;
            uint trackNum = trackFile.Tag.Track; // Track number
            uint discNum = trackFile.Tag.Disc; // Disc number
            if (trackNum != 0)
            {
                if (discNum != 0) tagString = discNum.ToString() + "." + trackNum.ToString() + "||";
                else tagString = trackNum.ToString() + "||";
            }
            else tagString = " ||";

            string title = trackFile.Tag.Title; // Title
            TimeSpan duration = trackFile.Properties.Duration;
            string time = duration.Minutes.ToString().PadLeft(2, '0');
            time += ":" + duration.Seconds.ToString().PadLeft(2, '0');
            string artist = trackFile.Tag.FirstPerformer; // Artist
            string album = trackFile.Tag.Album; // Album
            string year = trackFile.Tag.Year.ToString(); // Year

            tagString += title + "||" + time + "||" + artist + "||" + album + "||" + year;

            return tagString;
        }
    }
}
