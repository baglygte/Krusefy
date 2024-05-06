using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Krusefy
{
    internal class PlayButtonViewmodel : INotifyPropertyChanged
    {
        private bool _IsPlaying;
        public bool IsPlaying
        {
            get { return _IsPlaying; }
            set { this._IsPlaying = value; this.OnPropertyChanged("IsPlaying"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
