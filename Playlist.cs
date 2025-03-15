using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Krusefy
{
    public class Playlist
    {
        public List<Track> Tracks { get; private set; }
        public string Name { get; set; }
        public bool IsPlayingFrom { get; set; }

        internal Playlist(string name)
        {
            this.Name = name;
        }

        internal void CreateTracks(string txtFilePath)
        {
            if (!File.Exists(txtFilePath)) { return; }

            string[] fileLines = File.ReadAllLines(txtFilePath);

            this.Tracks = new List<Track> {};
            foreach(string line in fileLines)
            {
                string[] splitLine = line.Split(new string[] { "||" }, StringSplitOptions.None);
                Track newTrack = new Track(splitLine);
                if (!File.Exists(newTrack.Path)) { continue; }
                newTrack.InPlaylist = this;
                this.Tracks.Add(newTrack);
            }
            this.Tracks = this.Tracks.OrderBy(x => x.FirstIndex).ToList();
            this.Tracks = this.Tracks.OrderBy(x => x.Album).ToList();
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
