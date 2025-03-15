using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Krusefy
{
    public class Track : INotifyPropertyChanged
    {
        private bool _isPlaying = false;
        private string _queue;

        public string Queue { get { return _queue; } set { _queue = value; OnPropertyChanged(); } }
        public int FirstIndex { get; set; }
        public string Title { get; set; }
        public string Time {  get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        internal string Path { get; set; }
        internal Playlist InPlaylist { get; set; }
        public bool IsPlaying { get { return _isPlaying; } set { _isPlaying = value; OnPropertyChanged(); } }

        internal Track(string[] splitLine)
        {
                this.FirstIndex = this.parseIndex(splitLine[0]);
                this.Title = splitLine[1];
                this.Time = splitLine[2];
                this.Artist = splitLine[3];
                this.Album = splitLine[4];
                this.Year = splitLine[5];
                this.Path = splitLine[6];
        }

        internal void SetIsPlaying(bool setValue)
        {
            this.IsPlaying = setValue;
            this.InPlaylist.IsPlayingFrom = setValue;
        }
        internal void DecrementQueue()
        {
            if (this.Queue == "") return;
            if (this.Queue == "1") { this.Queue = ""; return; }

            this.Queue = (int.Parse(this.Queue)-1).ToString();
        }
        private int parseIndex(string str)
        {
            if(str == null || str.Length == 0 || str == " ") return 1;

            string[] splitIndex = str.Split('.');
            if (splitIndex.Length == 1)
            {
                return int.Parse(splitIndex[0]);
            }
            else
            {
                return int.Parse(splitIndex[1]);
            }
        }

        internal string FindAlbumArt()
        {
            //mainWindow.Dispatcher.Invoke(() =>
            //{
            //    mainWindow.SetAlbumArt(path);
            //    mainWindow.SetAccentColors(path);
            //});
            // Look for album art in same folder as track
            string[] splitString = this.Path.Split('\\');
            string albumPath = null;
            for (int i = 0; i < splitString.Length - 1; i++)
            {
                albumPath += splitString[i] + '\\';
            }
            string[] files = Directory.GetFiles(albumPath);
            foreach (string path in files)
            {
                splitString = path.Split('.');
                string fileType = splitString[splitString.Length - 1].ToLower();
                if (fileType.EndsWith("jpg") || fileType.EndsWith("jpeg") || fileType.EndsWith("png"))
                {
                    return path;
                }
            }

            return null;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
