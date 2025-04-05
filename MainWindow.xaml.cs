using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;
using dwg = System.Drawing; // Renamed this to resolve ambiguouty of "Color" type
using System.Collections.Generic;
using Krusefy.DBSCAN;
using System.Runtime.InteropServices;
using System.Windows.Shapes;

namespace Krusefy
{
    public partial class MainWindow : Window
    {
        internal PlayButtonVM playButtonViewmodel = new PlayButtonVM();
        private PlaylistHandler playlistHandler;
        public SolidColorBrush Brush { get; set; }
        public MusicPlayer MusicPlayer { get; set; }

        private IntPtr _hookId = IntPtr.Zero;
        private HookProc _proc;

        public MainWindow()
        {
            InitializeComponent();


            MainWindowVM mainWindowVM = new MainWindowVM();

            mainWindowVM.PlaylistContentViewerVM = new PlaylistContentViewerVM();
            mainWindowVM.AlbumArtViewerVM = new AlbumArtViewerVM();
            mainWindowVM.PlaylistViewerVM = new PlaylistViewerVM();

            DataContext = mainWindowVM;

            MusicPlayer = new MusicPlayer(this, mainWindowVM);
            playlistHandler = new PlaylistHandler(this, mainWindowVM);

            _proc = HookCallback;
            _hookId = SetHook(_proc);

            Server server = new Server(playlistHandler, playlistHandler.musicPlayer);
            server.Start();

            playlistHandler.ReadPlaylistTxts();

            this.btnPlay.DataContext = playButtonViewmodel;

            //// Prevent sleep
            //SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
        }

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        private IntPtr SetHook(HookProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (IsMultimediaKey(vkCode))
                {
                    // Handle the multimedia key press
                    HandleMultimediaKey(vkCode);
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private bool IsMultimediaKey(int vkCode)
        {
            // Define the virtual key codes for multimedia keys
            int[] multimediaKeys = { VK_MEDIA_PLAY_PAUSE, VK_MEDIA_STOP, VK_MEDIA_PREV_TRACK, VK_MEDIA_NEXT_TRACK };
            return Array.Exists(multimediaKeys, key => key == vkCode);
        }

        private void HandleMultimediaKey(int vkCode)
        {
            switch (vkCode)
            {
                case VK_MEDIA_PLAY_PAUSE:
                    MusicPlayer.PlayPause();
                    break;
                case VK_MEDIA_STOP:
                    break;
                case VK_MEDIA_PREV_TRACK:
                    MusicPlayer.Prev();
                    break;
                case VK_MEDIA_NEXT_TRACK:
                    MusicPlayer.Next();
                    break;
            }
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const int VK_MEDIA_STOP = 0xB2;
        private const int VK_MEDIA_PREV_TRACK = 0xB1;
        private const int VK_MEDIA_NEXT_TRACK = 0xB0;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookId);
            base.OnClosed(e);
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDiag = new FolderBrowserDialog();
            folderDiag.SelectedPath = "C:\\\\";

            DialogResult res = folderDiag.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderDiag.SelectedPath))
            {
                playlistHandler.CreatePlaylistTxts(folderDiag.SelectedPath);
            }
        }

        public void SetAccentColors(string imagePath)
        {
            if (!File.Exists(imagePath)) { return; }
            ClusterAnalyzer clusterAnalyzer = new ClusterAnalyzer();
            var colors = clusterAnalyzer.GetAccentColors(new dwg.Bitmap(imagePath));
            Color primaryColor = Color.FromRgb((byte)colors[0].R, (byte)colors[0].G, (byte)colors[0].B);
            Color secondaryColor = Color.FromRgb((byte)colors[1].R, (byte)colors[1].G, (byte)colors[1].B);

            this.Resources["PrimaryAccent"] = new SolidColorBrush(primaryColor);
            this.Resources["SecondaryAccent"] = new SolidColorBrush(secondaryColor);
        }

        public void SetWaveForm(string path)
        {
            BitmapImage waveform = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            seekbarWaveform.Source = waveform;
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            playlistHandler.musicPlayer.PlayPause();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            playlistHandler.musicPlayer.Next();
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            playlistHandler.musicPlayer.Prev();
        }

        private void Seekbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double positionPercentage = e.GetPosition(seekbar).X/seekbar.ActualWidth;
            seekbar.Value = positionPercentage * seekbar.Maximum;
            playlistHandler.musicPlayer.SeekTo(positionPercentage);
        }

        private void NowPlaying_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MusicPlayer.CurrentlyPlayingTrack == null) { return; }

            MainWindowVM mainWindowVM = (MainWindowVM)DataContext;
            PlaylistContentViewerVM playlistContentViewerVM = mainWindowVM.PlaylistContentViewerVM;
            playlistContentViewerVM.Playlist = MusicPlayer.CurrentlyPlayingTrack.InPlaylist;
        }
    }
}
