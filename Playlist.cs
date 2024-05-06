using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Krusefy
{
    internal class Playlist
    {
        internal List<Track> Tracks { get; private set; }
        public string Name { get; set; }
        internal string ArtistDir { get; set; }
        public bool IsPlayingFrom { get; set; }

        internal Playlist(string name)
        {
            this.Name = name;
        }

        internal void SetTracks(string[] fileLines)
        {
            this.Tracks = new List<Track> { };
            foreach(string line in fileLines)
            {
                string[] splitLine = line.Split(new string[] { "||" }, StringSplitOptions.None);
                Track newTrack = new Track(splitLine);
                newTrack.InPlaylist = this;
                this.Tracks.Add(newTrack);
            }
            this.Tracks = this.Tracks.OrderBy(x => x.FirstIndex).ToList();
            this.Tracks = this.Tracks.OrderBy(x => x.Year).ToList();
        }

        internal Track GetNextTrack(Track currentlyPlayingTrack)
        {
            int currentIndex = this.Tracks.IndexOf(currentlyPlayingTrack);

            if (currentIndex == this.Tracks.Count - 1)
            {
                return this.Tracks[0];
            }
            else
            {
                return this.Tracks[currentIndex + 1];
            }
        }

        internal Track GetPreviousTrack(Track currentlyPlayingTrack)
        {
            int currentIndex = this.Tracks.IndexOf(currentlyPlayingTrack);
            if (currentIndex == 0)
            {
                return this.Tracks[this.Tracks.Count - 1];
            }
            else
            {
                return this.Tracks[currentIndex - 1];
            }
        }

        internal void CreateTxt()
        {
            // Looks through an artist directory and fills a .txt file with information about files
            // in said directory
            string playlistPath = ".\\playlists\\" + this.Name + ".txt";

            if (!Directory.Exists(this.ArtistDir)) { return; }
            StreamWriter sw = new StreamWriter(playlistPath);
            string[] singlePaths = Directory.GetFiles(this.ArtistDir);

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
            string[] albumPaths = Directory.GetDirectories(this.ArtistDir);
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

        internal async void CreateJSON(string[] fileLines)
        {
            // Create a .json file of the displayed playlist
            string jsonDataPath = ".\\server\\activePlaylist.json";
            StreamWriter swJSON = new StreamWriter(jsonDataPath);
            await swJSON.WriteLineAsync("[");

            for (int i = 0; i < fileLines.Length; i++)
            {
                string line = fileLines[i];
                string[] splitLine = line.Split(new string[] { "||" }, StringSplitOptions.None);

                //mainWindow.Dispatcher.Invoke(() =>
                //{
                //    mainWindow.viewerList.Add(entry);
                //});

                string entryName;
                if (i == fileLines.Length - 1)
                {
                    entryName = "{\"name\":\"" + splitLine[1] + "\"}";
                }
                else
                {
                    entryName = "{\"name\":\"" + splitLine[1] + "\"},";
                }
                await swJSON.WriteLineAsync(entryName);
            }
            //mainWindow.Dispatcher.Invoke(() => { mainWindow.viewerList = viewerList;});
            await swJSON.WriteLineAsync("]");
            swJSON.Close();
        }

    }
}
