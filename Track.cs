using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krusefy
{
    internal class Track
    {
        public string Queue { get; set; }
        public int FirstIndex { get; set; }
        public string Title { get; set; }
        public string Time {  get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        internal string Path { get; set; }
        internal Playlist InPlaylist { get; set; }
        public bool IsPlaying { get; set; }

        internal Track(string[] splitLine)
        {
            this.FirstIndex = this.parseIndex(splitLine[0]);
            this.Title = splitLine[1];
            this.Time = splitLine[2];
            this.Artist = splitLine[3];
            this.Album = splitLine[4];
            this.Year = splitLine[5];
            this.Path = splitLine[6];
            this.IsPlaying = false;
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
    }
}
