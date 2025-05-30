﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Krusefy
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        public AlbumArtViewerVM AlbumArtViewerVM { get; set; }
        public PlaylistContentViewerVM PlaylistContentViewerVM { get; set; }
        public PlaylistViewerVM PlaylistViewerVM { get; set; }

        public void UpdateAlbumArt(string path)
        {
            AlbumArtViewerVM.LoadImageFromPath(path);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
