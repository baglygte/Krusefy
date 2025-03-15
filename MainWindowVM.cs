using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
//using Prism.Commands;

namespace Krusefy
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        public AlbumArtViewerVM AlbumArtViewerVM { get; set; }
        public PlaylistContentViewerVM PlaylistContentViewerVM { get; set; }

        //public DelegateCommand<string> UpdateAlbumArtCommand { get { return new DelegateCommand<string>(OnUpdateAlbumArt); } }


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
