using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Krusefy
{
    public partial class PlaylistViewer : UserControl
    {
        private MainWindow MainWindow => (MainWindow)Window.GetWindow(this);
        public PlaylistViewer()
        {
            InitializeComponent();
        }
        private void PlaylistViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView.SelectedItem != null)
            {
                Playlist selectedPlaylist = (Playlist)listView.SelectedItem;

                MainWindowVM mainWindowVM = (MainWindowVM)MainWindow.DataContext;
                PlaylistContentViewerVM playlistContentViewerVM = mainWindowVM.PlaylistContentViewerVM;
                playlistContentViewerVM.Playlist = selectedPlaylist;
            }
        }

        private void PlaylistViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) { return; }
            if (listView.SelectedItem != null)
            {
                Playlist selectedPlaylist = (Playlist)listView.SelectedItem;

                MainWindowVM mainWindowVM = (MainWindowVM)MainWindow.DataContext;
                PlaylistContentViewerVM playlistContentViewerVM = mainWindowVM.PlaylistContentViewerVM;
                playlistContentViewerVM.Playlist = selectedPlaylist;
            }
        }
    }
}
