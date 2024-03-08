using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Devices;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace Re_Hippocamp.Misc
{

    public class SongPlayer
    {
        private bool isAborted = false;

        public int SongIndex = 0;

        public bool isMuted
        {
            get { return mediaPlayer.IsMuted; }
            set
            {
                mediaPlayer.IsMuted = value;
            }
        }

        public TimeSpan SongDuration;
        public TimeSpan SongPosition
        {
            get { return mediaPlayer.PlaybackSession.Position; }
            set 
            {
                mediaPlayer.PlaybackSession.Position = value;
                SongPositionChanged.Invoke(SongPosition);
            }
        }

        private bool manualShuffle;
        public bool Shuffle
        {
            get { return playbackList.ShuffleEnabled; }
            set
            {
                playbackList.ShuffleEnabled = value;
                manualShuffle = value;
            }
        }

        private bool manualRepeat;
        public bool Repeat
        {
            get { return playbackList.AutoRepeatEnabled; }
            set
            {
                playbackList.AutoRepeatEnabled = value;
                manualRepeat = value;
            }
        }

        public double PlayerVolume
        {
            get { return mediaPlayer.Volume; }
            set { SafePlayerVolume = value; mediaPlayer.Volume = value; }
        }

        public double SafePlayerVolume = 1;


        public SongPlayerState State;
        public enum SongPlayerState
        {
            Playing,
            Pause,
            Stopped
        }

        public delegate void PositionChanged(TimeSpan timeSpan);
        public event PositionChanged SongPositionChanged;

        public delegate void SongChanged(int index);
        public event SongChanged SongChangedPosition;


        public delegate void SongEnded();
        public event SongEnded SongPlayingEnded;

        private MainWindow MainWindow;

        MediaPlayer mediaPlayer = new MediaPlayer();
        public SongPlayer(MainWindow mainWindow, UserProfile loadedProfile)
        {
            MainWindow = mainWindow;
            if (loadedProfile == null) throw new ArgumentNullException("LoadedProfile cannot be null, it is required to play songs.");

            loadedProfile.PlayerVolume = 1; //should be delete, only for debugging.
            mediaPlayer.Volume = loadedProfile.PlayerVolume;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            SystemMediaTransportControls systemControls = mediaPlayer.SystemMediaTransportControls;

            systemControls.IsEnabled = false;
            systemControls.IsNextEnabled = true;
            systemControls.IsPreviousEnabled = true;
            systemControls.IsPlayEnabled = false;
            systemControls.IsPauseEnabled = false;
            systemControls.ButtonPressed += SystemControls_ButtonPressed;

            applyAudioDeviceAsync();

            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            ThreadWork();

            fakeVolume.ValueChanged += FakeVolume_ValueChanged;
        }

        private async Task applyAudioDeviceAsync()
        {
            var outputDevices = await DeviceInformation.FindAllAsync(MediaDevice.GetAudioRenderSelector());

            if (MainWindow.hippocampSettings.audioDevice != "Default")
            {
                int index = outputDevices.ToList().FindIndex(x => x.Name == MainWindow.hippocampSettings.audioDevice);
                if (index != -1)
                {
                    mediaPlayer.AudioDevice = outputDevices[index];
                    HC.WriteLine("Successfully changed AudioDevice to : " + outputDevices[index].Name);
                }
                else { HC.WriteLine("Unable to find " + MainWindow.hippocampSettings.audioDevice + " in the AudioDevices collection"); }
            }
            else
            {
                HC.WriteLine("No AudioDevice changes.");
            }
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            mediaPlayer.CommandManager.IsEnabled = !MainWindow.hippocampSettings.disableWindowsOverlay;
            HC.WriteLine("mediaPlayer.CommandManager.IsEnabled=" + mediaPlayer.CommandManager.IsEnabled);
        }

        private void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            MessageBox.Show(args.ErrorMessage);
        }

        public DeviceInformation getAudioDevice()
        {
            return mediaPlayer.AudioDevice;
        }

        private void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => 
            {
                if (args.Button == SystemMediaTransportControlsButton.Play)
                {
                    mediaPlayer.Pause();
                    SetPlayerState(SongPlayerState.Playing);
                }
                else if (args.Button == SystemMediaTransportControlsButton.Pause)
                {
                    mediaPlayer.Play();
                    SetPlayerState(SongPlayerState.Pause);
                }
                else if (args.Button == SystemMediaTransportControlsButton.Next)
                {
                    State = SongPlayerState.Playing;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    { SongPositionChanged.Invoke(SongPosition); }));
                }
                else if (args.Button == SystemMediaTransportControlsButton.Previous)
                {
                    State = SongPlayerState.Playing;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    { SongPositionChanged.Invoke(SongPosition); }));
                }
            }));
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            //Application.Current.Dispatcher.Invoke(new Action(() =>
            //{ SongPlayingEnded.Invoke(); }));
        }

        public bool ListeningToLikes = false;
        MediaPlaybackList playbackList = new MediaPlaybackList();
        int errors = 0;
        public bool PlaySong(HSong hSong, HAlbum hAlbum, byte[] thumbnail)
        {
            mediaPlayer.Volume = SafePlayerVolume;
            playbackList = new MediaPlaybackList() { ShuffleEnabled = manualShuffle, AutoRepeatEnabled = manualRepeat };
            playbackList.ItemFailed += PlaybackList_ItemFailed;
            HC.WriteLine("Should start with: " + hSong.Name + " from " + hAlbum.Name + " songs: " + hAlbum.Songs.Count);
            uint start = 0;

            uint index = 0;
            if (hSong != null)
            {
                if (hAlbum.Name == "HPCAMPLikes") ListeningToLikes = true; else ListeningToLikes = false;
                foreach (var s in hAlbum.Songs)
                {
                    if (!File.Exists(s.Path)) continue;

                    var nI = new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(s.Path)));
                    playbackList.Items.Add(nI);

                    if (s.Name == hSong.Name)
                    {
                        HC.WriteLine(s.Name + " FOUND");
                        start = index;
                    }
                    index++;

                    var dP = nI.GetDisplayProperties();
                    if (s.Name != null)
                        dP.MusicProperties.Title = s.Name;
                    if (s.Artist != null)
                        dP.MusicProperties.Artist = s.Artist;

                    if(s.Cover != null)
                        dP.Thumbnail = RandomAccessStreamReference.CreateFromStream(ConvertTo(SongThumbnail.createThumbnail(s, hAlbum)).Result);
                    else
                        dP.Thumbnail = RandomAccessStreamReference.CreateFromStream(ConvertTo(thumbnail).Result);

                    dP.Type = MediaPlaybackType.Music;
                    nI.ApplyDisplayProperties(dP);

                }

                mediaPlayer.Source = playbackList;
                try
                {
                    playbackList.MoveTo(start);
                }
                catch (Exception ex) 
                {
                    if (errors < 5) 
                    { errors++; HC.WriteLine("Error when moving to " + start + "/" + playbackList.Items.Count + " retrying (" + errors + "/5)" + "...\n" + ex.ToString()); return PlaySong(hSong, hAlbum, thumbnail); }
                    if(errors >= 5)
                    {
                        HC.WriteLine("Too many errors in SongPlayer, aborting.", ConsoleColor.DarkRed);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            MainWindow.hippocampMessageBox.Show("An error occured", "Hippocamp was unable to read this song file.", Controls.HippocampMessageBox.HippocampMessageBoxButtons.Ok, true);
                        }));
                        errors = 0;
                        return false;
                    }
                }


                mediaPlayer.Play();
                errors = 0;
                SongIndex = (int)playbackList.CurrentItemIndex;
                State = SongPlayerState.Playing;
                SongPositionChanged.Invoke(SongPosition);
                playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
                return true;
            }
            else throw new Exception("An error occured while trying to play the song " + hSong == null ? "unknow song" : hSong.Name + ".");
        }

        bool canShowError = true;
        private void PlaybackList_ItemFailed(MediaPlaybackList sender, MediaPlaybackItemFailedEventArgs args)
        {
            if (!canShowError) return;
            canShowError = false;
            SetPlayerState(SongPlayerState.Pause);
            mediaPlayer.Pause();
            Application.Current.Dispatcher.Invoke(new Action(() => 
            {
                MainWindow.hippocampMessageBox.Show("An error occured", "Hippocamp was unable to read this song file.", Controls.HippocampMessageBox.HippocampMessageBoxButtons.Ok, true);
            }));
           
            new Thread(() => //delay in case it spams
            {
                Thread.Sleep(1000);
                canShowError = true;
            }).Start();
        }
        
        public void nextSong()
        {
            HC.WriteLine("UI CHANGE");
            if (playbackList.CurrentItemIndex == playbackList.Items.Count - 1 && !manualShuffle) return;
        
            try
            {
                playbackList.MoveNext();
            }
            catch (Exception) { }
        }
        
        public void previousSong()
        {
            HC.WriteLine("UI CHANGE");
            if (playbackList.CurrentItemIndex == 0 && !manualShuffle) return;
            try
            {
                playbackList.MovePrevious();
            }
            catch (Exception) { }
        }
        
        private void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            try
            {
                mediaPlayer.SystemMediaTransportControls.IsEnabled = !MainWindow.hippocampSettings.disableWindowsOverlay;

            }
            catch (Exception)
            {
            }
            if(playbackList == null) return;
            if(playbackList.Items == null) return;
            int pos = 0;
            try
            {
                foreach (var item in playbackList.Items)
                {
                    if (playbackList.CurrentItem == item)
                        SongIndex = pos;

                    pos++;
                }
            }
            catch (Exception)
            {
                return;
            }

            if (SongIndex == -1) return;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            { SongChangedPosition.Invoke(SongIndex); }));
        }

        async Task<IRandomAccessStream> ConvertTo(byte[] arr)
        {
            IRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            object p = await randomAccessStream.WriteAsync(arr.AsBuffer());
            randomAccessStream.Seek(0); // Just to be sure.                   
            return randomAccessStream;
        }

        public void SetPlayerState(SongPlayerState state)
        {
            //process parameter.....

            switch (state)
            {
                case SongPlayerState.Playing: 
                    mediaPlayer.Play();
                    Fade(true);
                    State = SongPlayerState.Playing;
                    break;

                case SongPlayerState.Pause:
                    Fade(false);
                    //mediaPlayer.Pause();
                    State = SongPlayerState.Pause;
                    break;

                case SongPlayerState.Stopped:
                    mediaPlayer.Pause();
                    State = SongPlayerState.Stopped;
                    break;
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            { SongPositionChanged.Invoke(SongPosition); }));
        }

        public void Fade(bool fIn) //if true fade in else fade out
        {
            if(fIn)
                VolumeAnimation(0, SafePlayerVolume, false);
            else
                VolumeAnimation(SafePlayerVolume, 0, true);
        }

        Slider fakeVolume = new Slider();
        Storyboard storyboard;
        private void VolumeAnimation(double from, double to, bool pause)
        {

            if (!pause)
            {
                mediaPlayer.Volume = 0;
                mediaPlayer.Play();
            }

            object obj = mediaPlayer;

            var a = new DoubleAnimation
            {
                From = from,
                To = to,
                FillBehavior = FillBehavior.Stop,
                Duration = new TimeSpan(0,0,0,0,300)
            };

            storyboard = new Storyboard();

            storyboard.Children.Add(a);
            
            Storyboard.SetTarget(a, fakeVolume);
            Storyboard.SetTargetProperty(a, new PropertyPath(Slider.ValueProperty));
            storyboard.Completed += delegate 
            {
                fakeVolume.Value = to;
                if (pause)
                    mediaPlayer.Pause();

            };

            storyboard.Begin();
        }

        private void FakeVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = (double)e.NewValue;
        }


        private void ThreadWork()
        {
            new Thread(() =>
            {
                TimeSpan oldTimeSpan = new TimeSpan(0);
                while (!isAborted)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => 
                    {
                        //need to be as fast as possible
                        if (mediaPlayer != null)
                        {
                            var timeSpan = mediaPlayer.PlaybackSession.Position;
                            if (timeSpan != oldTimeSpan)
                            {
                                try
                                {
                                    oldTimeSpan = timeSpan;
                                    SongDuration = mediaPlayer.PlaybackSession.NaturalDuration;
                                    SongPositionChanged.Invoke(timeSpan);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }));

                    Thread.Sleep(1000);
                }

            }).Start();
        }

        public void AbortPlayer()
        {
            mediaPlayer.SystemMediaTransportControls.ButtonPressed -= SystemControls_ButtonPressed;
            mediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
            mediaPlayer.MediaFailed -= MediaPlayer_MediaFailed;
            mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
            mediaPlayer.Pause();
            mediaPlayer.Dispose();
            mediaPlayer = null;
            playbackList.Items.Clear();
            playbackList = null;
        }

    }
}