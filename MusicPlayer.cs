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


namespace Krusefy
{
    public class MusicPlayer
    {
        BlockAlignReductionStream stream = null;
        DirectSoundOut output = null;
        WaveStream pcm = null;
        DispatcherTimer playbackTimer;
        private Track CurrentlyPlayingTrack { get; set; }
        internal List<Track> Queue = new List<Track>();
        MainWindow mainWindow;
        long timeInterval = 200; //ms interval of how often to update seekbar position
        double trackPosition; // Current position in playing track in ms
        double trackLength; // Length of currently playing track in ms
        long nextCall;

        public MusicPlayer(MainWindow m) // Constructor
        {
            this.mainWindow = m;
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
                this.mainWindow.playlistViewer.Items.Refresh();
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

        internal void PlayTrack(Track trackToPlay)
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
            this.mainWindow.playlistViewer.Items.Refresh();
            this.mainWindow.playlistManager.Items.Refresh();

            DisposeWave();

            this.SetAlbumArt(trackToPlay.Path);

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

            this.Waveforming(trackToPlay.Path);
        }

        private async void Waveforming(string filePath)
        {
            string waveformPath = "C:\\Users\\bagly\\Documents\\C#\\Krusefy\\bin\\Debug\\waveform.png";
            if (File.Exists(waveformPath))
            {
                mainWindow.seekbarWaveform.Source = null;
                File.Delete(waveformPath);
            }

            await Task.Run(() => CreateWaveform(filePath));

            BitmapImage waveform = BitmapFromUri(new Uri(waveformPath));
            mainWindow.seekbarWaveform.Source = waveform;
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

        void SetAlbumArt(string trackPath)
        {
            // Look for album art in same folder as track
            string[] splitString = trackPath.Split('\\');
            string albumPath = null;
            for(int i = 0; i<splitString.Length-1; i++)
            {
                albumPath += splitString[i] + '\\';
            }
            string[] files = Directory.GetFiles(albumPath);
            foreach(string path in files)
            {
                splitString = path.Split('.');
                string fileType = splitString[splitString.Length-1].ToLower();
                if(fileType.EndsWith("jpg")||fileType.EndsWith("jpeg")||fileType.EndsWith("png"))
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        mainWindow.SetAlbumArt(path);
                    });

                    return;
                }
            }
        }
        void InitTimer()
        {
            playbackTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            playbackTimer.Tick += new EventHandler(TimerEvent);
            playbackTimer.Interval = TimeSpan.FromMilliseconds(timeInterval);
        }
        //void OnUpdateTimerTick(object sender,EventArgs e)
        //{

        //}
        //void SetTimer(bool startTimer)
        //{
        //    //Start or stop the timer
        //    if(startTimer) // Start the timer
        //    {
        //        if(playbackTimer!=null) playbackTimer.Close(); // Kill existing timer
        //        playbackTimer = new Timer(timeInterval); // Make a new timer
        //        playbackTimer.Elapsed += TimerEvent; // Add event
        //        playbackTimer.Enabled = true; // Enable the timer
        //        nextCall = DateTime.Now.Ticks; // Reset the next timer call
        //    }
        //    else // Stop the timer
        //    {
        //        playbackTimer.Close();
        //    }
        //}
        void TimerEvent(object source, EventArgs e)
        {
            //Debug.WriteLine(trackPosition.ToString() + "|" + trackLength.ToString());
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
            this.mainWindow.playlistViewer.Items.Refresh();
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
            this.mainWindow.playlistViewer.Items.Refresh();
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

