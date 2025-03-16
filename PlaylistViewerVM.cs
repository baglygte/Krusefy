using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Krusefy
{
    public class PlaylistViewerVM : INotifyPropertyChanged
    {
        private List<Playlist> _playlists;

        public List<Playlist> Playlists
        {
            get { return _playlists; }
            set
            {
                _playlists = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
