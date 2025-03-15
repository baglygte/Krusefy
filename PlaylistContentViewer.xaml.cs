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
    public partial class PlaylistContentViewer : UserControl
    {
        private MainWindow MainWindow => (MainWindow)Window.GetWindow(this);

        public PlaylistContentViewer()
        {
            InitializeComponent();
        }

        private void PlaylistContentViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (listView.SelectedItem == null) { return; }
            if (!(DataContext is PlaylistContentViewerVM)) { return; }

            Track selectedTrack = listView.SelectedItem as Track;

            MainWindowVM mainWindowVM = (MainWindowVM)MainWindow.DataContext;
            mainWindowVM.UpdateAlbumArt(selectedTrack.FindAlbumArt());
        }

        private void PlaylistContentViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView.SelectedItem == null) { return; }

            MainWindow.MusicPlayer.PlayTrack((Track)listView.SelectedItem);
        }

        private void RemoveFromQueue(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItem == null) { return; }
            MainWindow.MusicPlayer.RemoveFromQueue((Track)listView.SelectedItem);
        }

        private void AddToQueue(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItem == null) { return; }

            MainWindow.MusicPlayer.AddToQueue((Track)listView.SelectedItem);
        }
    }
}
