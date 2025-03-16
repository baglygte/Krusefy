using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Timers;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Drawing;
using System.Windows.Threading;
using System.IO;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Threading;


namespace Krusefy
{
    public class MusicPlayer
    {
        BlockAlignReductionStream stream = null;
        DirectSoundOut output = null;
        WaveStream pcm = null;
        DispatcherTimer playbackTimer;
        internal Track CurrentlyPlayingTrack { get; set; }
        internal List<Track> Queue = new List<Track>();
        MainWindow mainWindow;
        long timeInterval = 200; //ms interval of how often to update seekbar position
        double trackPosition; // Current position in playing track in ms
        double trackLength; // Length of currently playing track in ms
        long nextCall;

        private CancellationTokenSource waveformingCancellationTokenSource = new CancellationTokenSource();
        private Task currentWaveformingTask = null;

        private const string waveformFilename = "waveform.png";

        private MainWindowVM mainWindowVM;

        public MusicPlayer(MainWindow m, MainWindowVM mainWindowVM) // Constructor
        {
            this.mainWindow = m;
            this.mainWindowVM = mainWindowVM;
            InitTimer();
            
        }
        private Track GetNextTrack()
        {
            if (this.CurrentlyPlayingTrack == null) { return null; }
            if (this.Queue.Count > 0)
            {
                Track nextTrack = this.Queue[0];
                foreach (Track track in this.Queue)
                {
                    track.DecrementQueue();
                }
                this.Queue.RemoveAt(0);
                return nextTrack;
            }

            return this.CurrentlyPlayingTrack.InPlaylist.GetNextTrack(this.CurrentlyPlayingTrack);
        }

        private Track GetPreviousTrack()
        {
            if (this.CurrentlyPlayingTrack == null) { return null; }
            return this.CurrentlyPlayingTrack.InPlaylist.GetPreviousTrack(this.CurrentlyPlayingTrack);
        }

        internal async void PlayTrack(Track trackToPlay)
        {
            if (trackToPlay == null) { return; }
            this.mainWindow.playButtonViewmodel.IsPlaying = true;
            this.mainWindow.labelArtist.Content = trackToPlay.Artist;
            this.mainWindow.labelTitle.Content = trackToPlay.Title;
            this.mainWindow.labelAlbum.Content = trackToPlay.Album;

            if (CurrentlyPlayingTrack != null)
            {
                this.CurrentlyPlayingTrack.SetIsPlaying(false);
            }
            trackToPlay.SetIsPlaying(true);
            this.CurrentlyPlayingTrack = trackToPlay;

            DisposeWave();

            this.mainWindow.Dispatcher.Invoke(() =>
            {
                mainWindowVM.AlbumArtViewerVM.LoadImageFromPath(trackToPlay.FindAlbumArt());
                this.mainWindow.SetAccentColors(trackToPlay.FindAlbumArt());
            });

            Mp3FileReader mp3reader = new Mp3FileReader(trackToPlay.Path);
            pcm = WaveFormatConversionStream.CreatePcmStream(mp3reader);
            stream = new BlockAlignReductionStream(pcm);
            output = new DirectSoundOut(200);
            trackLength = stream.TotalTime.TotalMilliseconds;
            trackPosition = 0;
            output.Init(stream);
            output.Play();

            nextCall = DateTime.Now.Ticks / 10000 + timeInterval;
            playbackTimer.Start();


            await RunWaveformingAsync(trackToPlay.Path);
        }

        private async Task RunWaveformingAsync(string mp3FilePath)
        {

            if (currentWaveformingTask != null && !currentWaveformingTask.IsCompleted)
            {
                waveformingCancellationTokenSource.Cancel();
                try
                {
                    await currentWaveformingTask;
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
            }

            waveformingCancellationTokenSource = new CancellationTokenSource();
            currentWaveformingTask = Waveforming(mp3FilePath, waveformingCancellationTokenSource.Token);
        }

        private async Task Waveforming(string mp3FilePath, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                DirectoryInfo dirInfo = new DirectoryInfo(".");
                string waveformPath = dirInfo.FullName + "\\" + waveformFilename;
                if (File.Exists(waveformPath))
                {
                    mainWindow.seekbarWaveform.Source = null;
                    File.Delete(waveformPath);
                }

                await Task.Run(() => CreateWaveform(mp3FilePath));

                BitmapImage waveform = BitmapFromUri(new Uri(waveformPath));
                mainWindow.seekbarWaveform.Source = waveform;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Waveforming cancelled");
            }

        }

        public BitmapImage BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }
        void CreateWaveform(string filePath)
        {
            using (var waveStream = new AudioFileReader(filePath))
            {
                int width = 1920;
                int height = 200;
                int bytesPerSample = waveStream.WaveFormat.BitsPerSample / 8;
                long samples = waveStream.Length / (bytesPerSample);
                int samplesPerPixel = (int)(samples / width);

                ISampleProvider provider = waveStream.ToSampleProvider();

                Bitmap b = new Bitmap(width, height);
                using (var g = Graphics.FromImage(b))
                {
                    int x = 0;

                    while (x < width)
                    {
                        float peak = GetNextPeak(samplesPerPixel, provider);

                        g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(50, 50, 50))), x, height, x, height / 2 + peak * height);
                        g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(50, 50, 50))), x, 0, x, height / 2 - peak * height);
                        x++;
                    }
                    g.Flush();
                    b.Save("waveform.png", System.Drawing.Imaging.ImageFormat.Png);
                    g.Dispose();
                }
                waveStream.Dispose();
            }

            float GetNextPeak(int samplesPerPixel, ISampleProvider provider)
            {
                float[] readBuffer = new float[samplesPerPixel];
                int samplesRead = provider.Read(readBuffer, 0, readBuffer.Length);
                float sum = (samplesRead == 0) ? 0 : readBuffer.Take(samplesRead).Select(s => Math.Abs(s)).Sum();
                return sum / samplesRead;
            }
        }

        void InitTimer()
        {
            playbackTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            playbackTimer.Tick += new EventHandler(TimerEvent);
            playbackTimer.Interval = TimeSpan.FromMilliseconds(timeInterval);
        }

        void TimerEvent(object source, EventArgs e)
        {
            long timeError = nextCall - DateTime.Now.Ticks / 10000; // Error on how much time it should have taken vs. how much it actually took
            long actTime = timeInterval - timeError; // How much time was actually spent between timer calls

            if (actTime <= 0) // This occurs when the timer thread hangs
            {
                Debug.WriteLine("Thread hanged.");
            }
            trackPosition += actTime; // Increment locally tracked time

            if (trackPosition >= trackLength)
            {
                trackPosition = 0;
                Next(); // Play next track if the position exceeds the current track length
                return;
            }

            mainWindow.Dispatcher.Invoke(() =>
            { // Invoke an update in the seekbar value
                mainWindow.seekbar.Value = (trackPosition / trackLength) * 100;
            });
            nextCall = DateTime.Now.Ticks / 10000 + timeInterval; // When the next timer call should ideally be
            playbackTimer.IsEnabled = true; // Enable the timer
        }
        public void SeekTo(double percentage)
        {
            // Set the seekbar position
            if (stream == null) { return; } // Don't do anything if nothing's playing

            trackPosition = percentage * trackLength; // Set the locally tracked time/position

            long newPos = (long)Math.Round(stream.Length * percentage); // Get the byte position
            newPos -= newPos % stream.WaveFormat.BlockAlign; // Align position to the block size
            
            stream.Position = newPos; // Set the position of the stream
        }
        
        internal void AddToQueue(Track track)
        {
            this.Queue.Add(track);
            track.Queue = this.Queue.Count.ToString();
        }

        internal void RemoveFromQueue(Track track)
        {
            if (!this.Queue.Contains(track)) return;
            int removeIndex = this.Queue.IndexOf(track);
            for (int i = removeIndex+1; i < this.Queue.Count; i++)
            {
                this.Queue[i].DecrementQueue();
            }
            track.Queue = "";
            this.Queue.Remove(track);
        }

        public void PlayPause()
        {
            if (output == null)
                return;
            if (output.PlaybackState == PlaybackState.Playing)
            {
                playbackTimer.Stop();
                output.Pause();
                this.mainWindow.playButtonViewmodel.IsPlaying = false;
            }
            else if (output.PlaybackState == PlaybackState.Paused)
            {
                playbackTimer.Start();
                output.Play(); 
                this.mainWindow.playButtonViewmodel.IsPlaying = true;
            }
        }
        
        public void Next()
        {
            PlayTrack(this.GetNextTrack());
        }
        
        public void Prev()
        {
            PlayTrack(this.GetPreviousTrack());
        }

        private void DisposeWave()
        {
            if (output != null)
            {
                output.Dispose();
                output = null;
            }
        }
    }
}

