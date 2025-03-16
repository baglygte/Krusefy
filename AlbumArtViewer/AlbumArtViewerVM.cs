using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Krusefy
{
    public class AlbumArtViewerVM : INotifyPropertyChanged
    {
        private BitmapImage _albumImage;

        public BitmapImage AlbumImage
        {
            get { return _albumImage; }
            set
            {
                if (_albumImage != value)
                {
                    _albumImage = value;
                    OnPropertyChanged();
                }
            }
        }

        public void LoadImageFromPath(string path)
        {
            if (path == null)
            {
                AlbumImage = new BitmapImage();
                return;
            }
            AlbumImage = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
