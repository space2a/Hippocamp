using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Win32;

using Re_Hippocamp.Controls;
using Re_Hippocamp.Misc;
using Re_Hippocamp.Pages;
using Re_Hippocamp.PopUpPages;
using Re_Hippocamp.Serializable;
using Re_Hippocamp.Windows;

using static Re_Hippocamp.Misc.Blur;

namespace Re_Hippocamp
{
    public partial class MainWindow : Window
    {
        public static UserProfile loadedProfile;
        private UserProfiles userProfiles;
        public static Settings hippocampSettings;

        public static HippocampMessageBox hippocampMessageBox;

        public bool PUP_willNeedReloadUsers = false;

        private SongPlayer SongPlayer;

        private NI notifyIconPanel;

        public static string HippocampVersion = "?";

        public int WH
        {
            get { return (int)GetValue(WHProperty); }
            set { SetValue(WHProperty, value); }
        }

        public static readonly DependencyProperty WHProperty =
            DependencyProperty.Register("WH", typeof(int), typeof(MainWindow), new PropertyMetadata(10));
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);


        MediasKey mediasKey = new MediasKey();
        public static Theme theme = new Theme();
        public MainWindow()
        {
            InitializeComponent();
            checkSecondInstanceOfHippocamp();


            if (!Debugger.IsAttached)
            {
                //not running with vs
                HC.canWrite = false;
            }

            ob = new WindowResizer(this);

            HippocampVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            HippocampVersion = HippocampVersion.Substring(0, HippocampVersion.LastIndexOf('.'));

            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            HC.currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine(HC.currentPath);
            File.WriteAllBytes("logs.hlogs", BO.ObjectToByteArray(new HLogs()));

            //NO HC.WRITELINE BEFORE THIS LINE ----------------------------------------------------
            HC.WriteLine("HippocampVersion:" + HippocampVersion);

            hippocampMessageBox = new HippocampMessageBox(MGrid, this);
            hippocampSettings = new Settings();

            FavAnim.Visibility = Visibility.Hidden;
            BigPlayerIcon.Visibility = Visibility.Hidden;
            BigPlayerGrid.Visibility = Visibility.Hidden;


            //MGrid.Children.Clear(); MGrid = null;


            this.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                //Custom ContextMenu testing.

                EnableBlur();

                if (File.Exists("settings.hs"))
                {
                    hippocampSettings = BO.ByteArrayToObject(File.ReadAllBytes("settings.hs")) as Settings;
                    if (hippocampSettings == null) hippocampSettings = new Settings();
                }

                loadProfiles(); //<--- biggest ram usage here
                theme.getThemes(); //absolutly needed


                applySettings();

                theme.applyThemeInGrid(MGrid);

                if (!hippocampSettings.disableTransparency)
                    MakeTransparent();
                else
                {
                    MGrid.Margin = new Thickness(0);
                    SideBar.CornerRadius = new CornerRadius(0);
                    AppBackground.CornerRadius = new CornerRadius(0);
                    PlayerBarBottom.CornerRadius = new CornerRadius(0);
                }


                selectedSideBarGrid = Home;
                HideSideBarFilledImages(null, true);


                base.OnSourceInitialized(e);
                SoundLevel.Visibility = Visibility.Hidden;

                this.StateChanged += MainWindow_StateChanged;

            };

            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIconPanel = notifyIcon.DeployNotifyIcon(this);

            this.Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
                e.Cancel = true;
                bool restart = false;
                if (keyCurrentlyDown == Key.LeftCtrl || keyCurrentlyDown == Key.LeftShift) restart = true;

                if(!restart)
                    hippocampMessageBox.Show("Closing Hippocamp", "Would you like to exit Hippocamp or minimize it to the tray ?", HippocampMessageBox.HippocampMessageBoxButtons.MinimizeExit, true);
                else
                    hippocampMessageBox.Show("Restarting Hippocamp", "Would you like to restart Hippocamp ?", HippocampMessageBox.HippocampMessageBoxButtons.YesNo, true);

                hippocampMessageBox.HippocampMessageBoxValidated += delegate (bool d)
                {
                    if (d)
                    {
                        if (!restart)
                        {
                            this.WindowState = WindowState.Minimized;
                            new Thread(() =>
                            { //keep windows minimize animation
                                Thread.Sleep(300);
                                Application.Current.Dispatcher.Invoke(new Action(() => { this.Hide(); }));
                            }).Start();
                        }
                        else
                        {
                            SongPlayer.Fade(false);
                            new Thread(() =>
                            {
                                //closing here.
                                Application.Current.Dispatcher.Invoke(new Action(() => { this.Hide(); notifyIcon.NI.Dispose(); }));
                                Thread.Sleep(300);
                                if (restart)
                                {
                                    ProcessStartInfo process = new ProcessStartInfo(Assembly.GetEntryAssembly().Location);
                                    process.Arguments = "wait";
                                    Process.Start(process);
                                }
                                Environment.Exit(0);
                            }).Start();
                        }
                    }
                    else
                    {
                        if (restart) return;
                        Discord.Stop();
                        SongPlayer.Fade(false);
                        new Thread(() =>
                        {
                            //closing here.
                            Application.Current.Dispatcher.Invoke(new Action(() => { this.Hide(); notifyIcon.NI.Dispose(); }));
                            Thread.Sleep(300);
                            Environment.Exit(0);
                        }).Start();
                    }

                };
            };


        }

        WindowState oldState = WindowState.Normal;
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal && oldState == WindowState.Minimized)
                this.Show();

            oldState = this.WindowState;
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        public void applySettings()
        {
            hardwareAccelerationSetting();
            if(hippocampSettings.hippocampTheme != "Default")
            {
                foreach (var tf in theme.themeFiles)
                {
                    HC.WriteLine(tf.name + "==" + hippocampSettings.hippocampTheme);
                    if (tf.name == hippocampSettings.hippocampTheme)
                    {
                        if (!tf.quickScanResult)
                        {
                            hippocampMessageBox.Show("Error with the selected theme", "Hippocamp was not able to load the selected theme (" + '"' + tf.name + '"' + "), error line " + tf.errorLine + ".", HippocampMessageBox.HippocampMessageBoxButtons.Ok);
                        }
                        else theme.selectThemeFile(tf);
                    }
                }
            }
            HC.WriteLine("hippocampSettings.blockMaxVolume :: " + hippocampSettings.blockMaxVolume);
            if (!hippocampSettings.blockMaxVolume)
                SoundSlider.Maximum = 8;
            else
                SoundSlider.Maximum = 10;


            if (hippocampSettings.useDiscordRichPresence)
            {
                Discord.isEnabled = true;
                Discord.Ini();
            }
            else
            {
                Discord.isEnabled = false;
                Discord.Stop();
            }

            if (!hippocampSettings.disableMediaKeys && hippocampSettings.disableWindowsOverlay) mediasKey.Start(this); else mediasKey.Stop();

            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey
    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (hippocampSettings.startHippocampWithWindows)
                    rk.SetValue("Hippocamp", System.Reflection.Assembly.GetExecutingAssembly().Location);
                else
                    rk.DeleteValue("Hippocamp", false);
            }
            catch (Exception) { }
        }

        public void loadProfiles(bool reload = false)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>{ LoadingText.Content = "Loading, please wait..."; showLoading(); AvatarList.Visibility = Visibility.Hidden; CollectionPage.animAlbum = false; }));
            
            DateTime dateTime = DateTime.Now;
            ResetMediaControls();
            PlayingSong = null;
            PlayingAlbum = null;
            notifyIconPanel.ResetView();
            isBPOpen = false;

            userProfiles = null;
            CurrentProfileAvatar.Visibility = Visibility.Visible;
            if (File.Exists("profiles.hups"))
            {
                userProfiles = BO.ByteArrayToObject(File.ReadAllBytes("profiles.hups")) as UserProfiles;
                if(userProfiles == null) { userProfiles = new UserProfiles(); }
                loadedProfile = userProfiles.Profiles.Find(x => x.id == userProfiles.loadedProfileId);

                if (loadedProfile == null)
                {
                    foreach (var item in userProfiles.Profiles)
                    {
                        userProfiles.loadedProfileId = item.id;
                        break;
                    }
                    loadedProfile = userProfiles.Profiles.Find(x => x.id == userProfiles.loadedProfileId);
                }
            }

            HC.WriteLine("file read : " + (DateTime.Now - dateTime).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Yellow);

            isAvatarMenuOpen = false;

            HC.WriteLine("various ui: " + (DateTime.Now - dateTime).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Yellow);
            new Thread(() =>
            {
                HC.WriteLine("ui show : " + (DateTime.Now - dateTime).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Yellow);

                if (userProfiles != null)
                {
                    if (userProfiles.Profiles.Count == 0 || loadedProfile == null) //no user in userprofiles 
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            forceOpenCreateProfile();
                        }));
                        return;
                    }

                    if (loadedProfile == null) { MessageBox.Show("An error occured while reading your profile."); } //Corrupted profile file

                    Application.Current.Dispatcher.Invoke(new Action(() => //ok normal
                    {
                        createSongPlayer();

                        if(discoveredAlbums != null)
                            discoveredAlbums.Clear();
                        if(discoveredArtists != null)
                            discoveredArtists.Clear();
                        if(mixsCreated != null)
                            mixsCreated.Clear();

                        SideBarGrid_MouseLeftButtonDown(Home, null);
                        //Here need to delete all pages.

                        int xd = 0;
                        for (int i = 0; i < SPUserList.Children.Count; i++)
                        {
                            if((SPUserList.Children[i] as Grid).Tag == null)
                                continue;

                            xd++;
                        }

                        for (int i = 0; i < xd; i++)
                            SPUserList.Children.RemoveAt(0);

                        foreach (var up in userProfiles.Profiles)
                        {
                            if (up.id == loadedProfile.id) continue;
                            Grid grid = new Grid() { HorizontalAlignment = HorizontalAlignment.Right, Cursor = Cursors.Hand, Tag = up.id };

                            Rectangle ellipse = new Rectangle() { Height = 24, Width = 24, VerticalAlignment = VerticalAlignment.Bottom, 
                                HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,0,1,1), Cursor = Cursors.Hand, RadiusX = 3, RadiusY = 3 };

                            grid.MouseLeftButtonUp += Ellipse_MouseLeftButtonUp;

                            ImageBrush imageBrush = new ImageBrush();
                            imageBrush.ImageSource = BO.LoadImage(up.Avatar, 80);
                            imageBrush.Stretch = Stretch.Uniform;
                            ellipse.Fill = imageBrush;

                            Label label = new Label() { HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Colors.White),
                                Style = Username.Style, Margin = new Thickness(0, 6, 0, 0), FontSize = 14, Content = up.Username };

                            Rectangle rectangle = new Rectangle() { Fill = new SolidColorBrush(Color.FromArgb(14, 200,200,200)), Cursor = Cursors.Hand, HorizontalAlignment = HorizontalAlignment.Center, Width = 104,
                                RadiusX = 3, RadiusY = 3, Margin = new Thickness(0, 7, 0, 0), Height = 26, VerticalAlignment = VerticalAlignment.Top };

                            grid.Children.Add(rectangle);
                            grid.Children.Add(label);
                            grid.Children.Add(ellipse);
                            grid.MouseEnter += AvatarItemEnter;
                            grid.MouseLeave += AvatarItemLeave;

                            SPUserList.Children.Insert(0, grid);
                        }

                        readingPath = true;
                        loadProfileData();
                    }));
                    // end ok normal
                }
                else //New user welcome to Hippocamp
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        forceOpenCreateProfile().showWelcome();
                    }));
                    return;
                }

                HC.WriteLine("profiles management: " + (DateTime.Now - dateTime).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Yellow);

                int loading = 0; // 20 = 1 second
                int loadingMax = hippocampSettings.loadingTimeWarning;
                while (readingPath) { Thread.Sleep(50); loading++;
                    if (loading >= loadingMax) 
                    {
                        //Warning loading time is over 15 seconds.
                        var d = MessageBox.Show("It seem like the loading time is very long, " +
                            "it is possible that the folders entered contain too many subfolders, " +
                            "it is recommended to enter only folders containing songs and few subfolders." +
                            "\nDo you want to cancel the loading ?", "Long loading time", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (d.ToString() == "Yes")
                        {
                            //cancel
                            readingPath = false;
                        }
                        else
                            loadingMax *= 5;
                    } 
                }


                HC.WriteLine("wait other comps: " + (DateTime.Now - dateTime).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Yellow);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    LoadingText.Content = "Finalizing..." ;

                    LoadSideBarPlaylists();
                }));
                


                HC.WriteLine("loading time: " + (DateTime.Now - dateTime).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Yellow);
                HC.WriteLine("discovered albums:" + discoveredAlbums.Count, ConsoleColor.Black, ConsoleColor.Yellow);
                HC.WriteLine("discovered artists:" + discoveredArtists.Count, ConsoleColor.Black, ConsoleColor.Yellow);

                int sn = 0;
                foreach (var item in discoveredAlbums)
                    sn += item.Songs.Count;

                HC.WriteLine("discovered songs:" + sn, ConsoleColor.Black, ConsoleColor.Yellow);
                HC.WriteLine("end: " + (DateTime.Now - dateTime).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Yellow);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    HomePage homePage = PageContainer.Children[0] as HomePage;
                    if (homePage != null) homePage.Ini();
                    HC.WriteLine("homePage inid:" + sn, ConsoleColor.Black, ConsoleColor.Yellow);
                }));

                Application.Current.Dispatcher.Invoke(new Action(() => { hideLoading(); }));
            }).Start();
        }

        private void showLoading()
        {
            LoadingPB.IsIndeterminate = true;
            LoadingSC.Visibility = Visibility.Visible;
            LoadingSC.Opacity = 1;
        }

        private void hideLoading()
        {
            Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 300 : 10), LoadingSC);
            LoadingPB.IsIndeterminate = false;
        }

        private ProfilePUP forceOpenCreateProfile()
        {
            CurrentProfileAvatar.Visibility = Visibility.Hidden;
            ProfilePUP profilePUP = new ProfilePUP() { Margin = new Thickness(35) }; //no user in userprofiles
            profilePUP.forceCreate();
            profilePUP.xMainWindow = this;
            clearPUP();
            PUP.Children.Insert(1, profilePUP);
            OpenPUP();
            ClosePUPGrid.Visibility = Visibility.Hidden;
            readingPath = false;

            HC.WriteLine("creating new profile");
            hideLoading();
            return profilePUP;
        }

        private void Ellipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) //change user ellipse
        {
            for (int i = 0; i < userProfiles.Profiles.Count; i++)
                if((sender as Grid).Tag.ToString() == userProfiles.Profiles[i].id.ToString())
                {
                    userProfiles.loadedProfileId = userProfiles.Profiles[i].id;
                    saveProfiles();
                    loadProfiles(true);
                }
        }

        public void addNewArtistPlayedCurrentProfile(string name)
        {
            int max = 10;
            UserExperience userExperience = GetUserExperienceFromProfile(loadedProfile);
            if(userExperience.lastPlayedArtists == null) { userExperience.lastPlayedArtists = new List<string>(); }
            int index = userExperience.lastPlayedArtists.FindIndex(x => x == name);
            if (index != -1)
            {
                userExperience.lastPlayedArtists.RemoveAt(index); //remove it from old position
                userExperience.lastPlayedArtists.Insert(0, name);//moves it to 0, most recent
            }
            else
                userExperience.lastPlayedArtists.Insert(0, name);//insert to 0, most recent

            if (userExperience.lastPlayedArtists.Count > max)
                userExperience.lastPlayedArtists.RemoveAt(userExperience.lastPlayedArtists.Count - 1); //if more than 'max' item in list removes from 5 to end

            SaveUserExperience(userExperience);
        }

        

        public static UserExperience GetUserExperienceFromProfile(UserProfile userProfile)
        {
            if (File.Exists(userProfile.id + ".hexp"))
                return BO.ByteArrayToObject(File.ReadAllBytes(userProfile.id + ".hexp")) as UserExperience;
            else
            {
                HC.WriteAllBytes(userProfile.id + ".hexp", BO.ObjectToByteArray(new UserExperience() { UserId = userProfile.id }));
                HC.WriteLine("GetUserExperienceFromProfile :" + "new", ConsoleColor.Cyan);
                return GetUserExperienceFromProfile(userProfile);
            }
        }

        public static Likes GetLikes(UserProfile userProfile)
        {
            UserExperience userExperience = GetUserExperienceFromProfile(userProfile);
            return userExperience.Likes;
        }

        public static void SaveUserExperience(UserExperience userExperience)
        {
            HC.WriteAllBytes(userExperience.UserId + ".hexp", BO.ObjectToByteArray(userExperience));
            HC.WriteLine(userExperience.UserId + "#experience saved", ConsoleColor.DarkGreen);
        }

        public static void IncreaseSongPlayCount(string songPath, UserProfile userProfile, bool save = false)
        {
            UserExperience userExperience = GetUserExperienceFromProfile(userProfile);
            var exp = userExperience.songPlays.Find(x => x.Path == songPath);
            if (exp != null)
                exp.playAmount++;
            else
                userExperience.songPlays.Add(new SongPlay() { Path = songPath, playAmount = 1 });

            if (save)
                SaveUserExperience(userExperience);

            getTopPlayCount(userProfile, 10);
        }

        public static int GetSongPlayCount(string songPath, UserProfile userProfile)
        {
            UserExperience userExperience = GetUserExperienceFromProfile(userProfile);

            var exp = userExperience.songPlays.Find(x => x.Path == songPath);
            if (exp != null)
            {
                HC.WriteLine(exp.Path + " : " + exp.playAmount, ConsoleColor.DarkYellow);
                return exp.playAmount;
            }
            else
            {
                HC.WriteLine("Null");
                return 0;
            }
        }

        public static List<SongPlay> getTopPlayCount(UserProfile userProfile, int top)
        {
            UserExperience userExperience = GetUserExperienceFromProfile(userProfile);
            if(userExperience == null) { HC.WriteLine("userExperience null??"); return new List<SongPlay>(); }
            userExperience.songPlays = userExperience.songPlays.OrderBy(x => x.playAmount).ToList();
            userExperience.songPlays.Reverse();

            int d = 0;
            List<SongPlay> songs = new List<SongPlay>();
            for (int i = 0; i < userExperience.songPlays.Count; i++)
            {
                if (d++ == top) break;
                songs.Add(userExperience.songPlays[i]);
            }

            return songs;
        }

        public static List<HPlaylist> GetHPlaylists()
        {
            UserExperience userExperience = GetUserExperienceFromProfile(loadedProfile);
            return userExperience.playlists;
        }

        public static int CreateNewPlaylist(string PlaylistName)
        {
            UserExperience userExperience = GetUserExperienceFromProfile(loadedProfile);
            var imgdata = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Images/Playlist.png")).Stream;
            byte[] data = new byte[imgdata.Length];
            imgdata.Read(data, 0, (int)imgdata.Length);

            userExperience.playlists.Add(new HPlaylist() { id = userExperience.PlaylistId++, Name = PlaylistName, Cover = data });

            SaveUserExperience(userExperience);
            HC.WriteLine("new playlist", ConsoleColor.Green);
            return userExperience.PlaylistId;
        }

        public static void OverwritePlaylist(HPlaylist newPlaylist, int playlistIndex)
        {
            UserExperience userExperience = GetUserExperienceFromProfile(loadedProfile);

            userExperience.playlists[playlistIndex] = newPlaylist;

            SaveUserExperience(userExperience);
        }

        public static void AddSongToPlaylistFromLoadedProfile(HSong hSong, int playlistIndex, bool skipVerif = false)
        {
            HC.WriteLine("playlistIndex >> " + playlistIndex);
            UserExperience userExperience = GetUserExperienceFromProfile(loadedProfile);
            HPlaylist playlistDestination = userExperience.playlists[playlistIndex];

            if(!skipVerif && playlistDestination.hSongs.FindIndex(x => x == hSong.Path) != -1) 
            { 
                hippocampMessageBox.Show("Song already in playlist", 
                "This song is already present in this playlist, add it again ?", 
                HippocampMessageBox.HippocampMessageBoxButtons.NoYes);

                hippocampMessageBox.HippocampMessageBoxValidated += delegate (bool d)
                {
                    if (!d) AddSongToPlaylistFromLoadedProfile(hSong, playlistIndex, true);
                };
                return;
            }

            playlistDestination.hSongs.Add(hSong.Path);
            SaveUserExperience(userExperience);
        }

        public static bool RemoveSongToPlaylistFromLoadedProfile(HSong hSong, int playlistIndex)
        {
            UserExperience userExperience = GetUserExperienceFromProfile(loadedProfile);
            HPlaylist playlistDestination = userExperience.playlists[playlistIndex];

            bool result = playlistDestination.hSongs.Remove(hSong.Path);

            SaveUserExperience(userExperience);
            return result;
        }

        private void saveProfiles()
        {
            HC.WriteAllBytes("profiles.hups", BO.ObjectToByteArray(userProfiles));
        }

        private bool isAvatarMenuOpen = false;
        private void AvatarGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (PUP.Visibility == Visibility.Visible) return;
            int x = SPUserList.Children.Count;

            if (!isAvatarMenuOpen)
            {
                isAvatarMenuOpen = true;

                foreach (Grid item in SPUserList.Children)
                {
                    item.Margin = new Thickness(78, 0, 0, 0);
                    item.Opacity = 0;
                }
                AvatarList.Visibility = Visibility.Visible;

                new Thread(() =>
                {
                    Thread.Sleep(!hippocampSettings.removeAnimations ? 100 : 10);
                    for (int i = 0; i < x; i++)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() => { Animations.MarginToMargin((SPUserList.Children[i] as Grid).Margin, new Thickness(0,0,0,0), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 300 : 10), SPUserList.Children[i] as Grid); }));
                        Application.Current.Dispatcher.Invoke(new Action(() => { Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), (SPUserList.Children[i] as Grid)); }));
                        Thread.Sleep(!hippocampSettings.removeAnimations ? 40 : 10);
                    }
                }).Start();

            }
            else
            {
                isAvatarMenuOpen = false;



                new Thread(() =>
                {
                    Thread.Sleep(!hippocampSettings.removeAnimations ? 100 : 10);
                    for (var i = x - 1; i >= 0; i--)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() => { Animations.MarginToMargin((SPUserList.Children[i] as Grid).Margin, new Thickness(78, 0,0,0), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 300 : 10), SPUserList.Children[i] as Grid); }));
                        Application.Current.Dispatcher.Invoke(new Action(() => { Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), (SPUserList.Children[i] as Grid)); }));
                        Thread.Sleep(!hippocampSettings.removeAnimations ? 40 : 10);
                    }
                    Thread.Sleep(!hippocampSettings.removeAnimations ? 40 * x : 10);
                    Application.Current.Dispatcher.Invoke(new Action(() => { AvatarList.Visibility = Visibility.Hidden; }));


                }).Start();
            }
        }

        private void clearPUP()
        {
            HC.WriteLine(PUP.Children.Count);
            if (PUP.Children.Count == 3)
                PUP.Children.RemoveAt(1);
        }

        
        private void loadProfileData() //user change event here
        {
            CurrentProfileAvatar.Visibility = Visibility.Visible;

            if (loadedProfile == null) { MessageBox.Show("loadedProfile null"); return; }

            //Extract image data from loadedProfile.Avatar
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = BO.LoadImage(loadedProfile.Avatar, 100);
            imageBrush.Stretch = Stretch.Uniform;
            CurrentProfileAvatar.Fill = imageBrush;
            Username.Content = loadedProfile.Username;

            new Thread(() =>
            {

                ScanPathsInProfile();

            }).Start();
        }

        private bool readingPath = false;
        public List<HAlbum> discoveredAlbums = new List<HAlbum>();
        public List<HAlbum> mixsCreated = new List<HAlbum>();
        public List<HArtist> discoveredArtists = new List<HArtist>();
        private void ScanPathsInProfile()
        {
            DateTime dateTime = DateTime.Now;

            readingPath = true;
            int totalNewSongs = 0;

            long sizeTotal = 0;
            for (int i = 0; i < loadedProfile.savedPaths.Count; i++)
                sizeTotal += DirSize(new DirectoryInfo(loadedProfile.savedPaths[i].Path));

            bool savedDiscovereds = false;

            if (userProfiles.SavedDiscovereds != null && userProfiles.SavedDiscovereds.ownerId == loadedProfile.id)
            {

                if(userProfiles.SavedDiscovereds.lastSize == sizeTotal && userProfiles.SavedDiscovereds.discoveredAlbums.Count != 0)
                {
                    discoveredAlbums = userProfiles.SavedDiscovereds.discoveredAlbums;
                    discoveredArtists = userProfiles.SavedDiscovereds.discoveredArtists;
                    mixsCreated = userProfiles.SavedDiscovereds.mixsCreated;
                    savedDiscovereds = true;
                    HC.WriteLine("savedDiscovered " + (DateTime.Now - dateTime).TotalSeconds + "secs");
                }

            }

            if (!savedDiscovereds)
            {
                //if not saved in loadedprofile
                List<string> files = new List<string>();
                for (int i = 0; i < loadedProfile.savedPaths.Count; i++)
                {
                    if (loadedProfile.savedPaths[i] == null) { continue; }
                    var l = GetFiles(loadedProfile.savedPaths[i].Path, "*.*").ToList();
                    if (loadedProfile.savedPaths[i].SongsDiscovered != l.Count)
                    {
                        totalNewSongs += l.Count - loadedProfile.savedPaths[i].SongsDiscovered;
                        loadedProfile.savedPaths[i].SongsDiscovered = l.Count;
                        userProfiles.Profiles[userProfiles.Profiles.FindIndex(x => x.id == loadedProfile.id)] = loadedProfile;
                        saveProfiles();
                    }
                    HC.WriteLine(loadedProfile.savedPaths[i].Path + ":::" + loadedProfile.savedPaths[i].SongsDiscovered);
                    foreach (var x in l)
                    {
                        //HC.WriteLine(x);
                        if (files.FindIndex(item => item == x) != -1) break;
                        files.Add(x);
                    }
                }

                foreach (var f in files)
                {
                    TagLib.File t = null;
                    try
                    {
                        t = TagLib.File.Create(f);
                    }
                    catch (Exception) { }
                    if (t == null) continue;
                    HAlbum songAlbum = null;

                    foreach (var dA in discoveredAlbums) //search the album name in the discovered albums
                        if (dA.Name == t.Tag.Album)
                        {
                            songAlbum = dA;
                            break;
                        }

                    if (songAlbum == null) //if album not found == new album created
                    {
                        string artists = "";
                        for (int i = 0; i < t.Tag.AlbumArtists.Length; i++)
                        {
                            if (i == t.Tag.AlbumArtists.Length - 1)
                                artists += t.Tag.AlbumArtists[i];
                            else
                                artists += ", " + t.Tag.AlbumArtists[i];
                        }

                        if (artists == "" && t.Tag.FirstAlbumArtist != null)
                            artists = t.Tag.FirstAlbumArtist;

                        if (artists == "" && t.Tag.FirstPerformer != null)
                            artists = t.Tag.FirstPerformer.Replace(";", ",");


                        songAlbum = new HAlbum() { Name = t.Tag.Album, Artist = artists };
                        discoveredAlbums.Add(songAlbum);
                    }

                    if (songAlbum.Cover == null)
                    {
                        //HC.WriteLine("cover:" + t.Tag.Album);
                        FileInfo fileInfo = new FileInfo(f);
                        string c = checkCoverInDir(fileInfo.Directory.FullName);

                        if (c != "")
                        {
                            songAlbum.Cover = File.ReadAllBytes(c);
                        }
                        else
                        {
                            var mStream = new MemoryStream();
                            var firstPicture = t.Tag.Pictures.FirstOrDefault();
                            if (firstPicture != null)
                                songAlbum.Cover = firstPicture.Data.Data;
                        }
                    }

                    string artist = "";
                    if (t.Tag.FirstPerformer != null) artist = t.Tag.FirstPerformer.Replace(";", ",");
                    songAlbum.Songs.Add(new HSong() { Name = t.Tag.Title, Artist = t.Tag.FirstPerformer, Path = f, Duration = t.Properties.Duration.TotalSeconds});
                }

                for (int i = 0; i < discoveredAlbums.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(discoveredAlbums[i].Name))
                        discoveredAlbums[i].Name = "No name";
                }

                discoveredAlbums = discoveredAlbums.OrderBy(x => x.Name).ToList();
                //all albums are discovered.

                foreach (var album in discoveredAlbums) //discovering artists...
                {
                    var a = discoveredArtists.FindIndex(x => x.Name.ToLower() == album.Artist.ToLower());
                    if (a != -1)
                    {
                        discoveredArtists[a].hAlbums.Add(album);
                        int mixsA = mixsCreated.FindIndex(x => x.Name == "Mix " + album.Artist);
                        if(mixsA != -1)
                        {
                            for (int i = 0; i < discoveredArtists[a].hAlbums.Count; i++)
                            {
                                for (int iss = 0; iss < discoveredArtists[a].hAlbums[i].Songs.Count; iss++)
                                {
                                    mixsCreated[mixsA].Songs.Add(discoveredArtists[a].hAlbums[i].Songs[iss]);
                                    if (mixsCreated[mixsA].Cover == null)
                                        mixsCreated[mixsA].Cover = discoveredArtists[a].hAlbums[i].Cover;
                                }
                            }
                        }
                        else
                        {
                            HC.WriteLine("Mix " + album.Artist + " non trouver");
                        }
                    }
                    else
                    {
                        HArtist hArtist = new HArtist() { Name = album.Artist };
                        hArtist.hAlbums.Add(album);
                        discoveredArtists.Add(hArtist);
                        HAlbum hAlbum = new HAlbum() { Name = "Mix " + album.Artist, Artist = album.Artist };
                        HC.WriteLine("New mix");
                        mixsCreated.Add(hAlbum);
                    }
                }

                for (int i = 0; i < discoveredArtists.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(discoveredArtists[i].Name))
                        discoveredArtists[i].Name = "No name";
                }

                discoveredArtists = discoveredArtists.OrderBy(x => x.Name).ToList();
                mixsCreated = mixsCreated.OrderBy(x => x.Name).ToList();

                if (false) //might need to be disabled in the future, we should retreive song data from mixs now.
                {
                    for (int dA = 0; dA < discoveredArtists.Count; dA++)
                    {
                        for (int dAH = 0; dAH < discoveredArtists[dA].hAlbums.Count; dAH++)
                        {
                            discoveredArtists[dA].hAlbums[dAH].Songs.Clear();
                        }
                    }
                }

                //all artist & mixs are discovered/created
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                //if (totalNewSongs > 0)
                //    hippocampMessageBox.Show("New song" + (totalNewSongs > 1 ? "s" : "") + " discovered !", totalNewSongs + " new song" + (totalNewSongs > 1 ? "s" : "") + " discovered in your collection !", HippocampMessageBoxButtons.Ok);
                //else if (totalNewSongs < 0)
                //    hippocampMessageBox.Show("Song" + (totalNewSongs > 1 ? "s" : "") + " removed", totalNewSongs + " song" + (totalNewSongs > 1 ? "s" : "") + " removed from your collection", HippocampMessageBoxButtons.Ok);

                readingPath = false;

                if (!savedDiscovereds)
                {
                    userProfiles.SavedDiscovereds = new SavedDiscovereds() { discoveredAlbums = discoveredAlbums, discoveredArtists = discoveredArtists, mixsCreated = mixsCreated, lastSize = sizeTotal, ownerId = loadedProfile.id };
                    saveProfiles();
                    userProfiles.SavedDiscovereds = null;
                }

            }));

            //HC.WriteAllBytes("discoveredartists", BO.ObjectToByteArray(discoveredArtists));
            //HC.WriteAllBytes("discoveredalbums", BO.ObjectToByteArray(discoveredAlbums));
        }

        private long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = null;
            try
            {
                fis = d.GetFiles();
            }
            catch (Exception) { return size; }
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        private string checkCoverInDir(string path)
        {
            if (File.Exists(path + "/cover.png")) return path + "/cover.png";
            if (File.Exists(path + "/cover.jpg")) return path + "/cover.jpg";
            if (File.Exists(path + "/cover.jpeg")) return path + "/cover.jpeg";
            if (File.Exists(path + "/Cover.png")) return path + "/Cover.png";
            if (File.Exists(path + "/Cover.jpg")) return path + "/Cover.jpg";
            if (File.Exists(path + "/Cover.jpeg")) return path + "/Cover.jpeg";

            return "";
        }


        private IEnumerable<string> GetFiles(string root, string searchPattern)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                if (!readingPath) break;
                var path = pending.Pop();
                string[] next = null;
                try
                {
                    next = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories).Where(s => s.EndsWith(".mp3") || s.EndsWith(".flac")).ToArray();
                }
                catch { }
                if (next != null && next.Length != 0)
                    foreach (var file in next) yield return file;
                try
                {
                    next = Directory.GetDirectories(path);
                    foreach (var subdir in next) pending.Push(subdir);
                }
                catch { }
            }
        }


        #region Transparency

        private Window _window;

        public void MakeTransparent()
        {
            _window = this;
            var mainWindowPtr = new WindowInteropHelper(this).Handle;
            var mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            if (mainWindowSrc != null)
                if (mainWindowSrc.CompositionTarget != null)
                    mainWindowSrc.CompositionTarget.BackgroundColor = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);

            var margins = new Margins
            {
                cxLeftWidth = 0,
                cxRightWidth = Convert.ToInt32(_window.Width) * Convert.ToInt32(_window.Width),
                cyTopHeight = 0,
                cyBottomHeight = Convert.ToInt32(_window.Height) * Convert.ToInt32(_window.Height)
            };

            if (mainWindowSrc != null) DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Margins
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);

        #endregion Transparency

        #region Resize
        WindowResizer ob;

        private void Resize(object sender, MouseButtonEventArgs e)
        {
            ob.resizeWindow(sender);
        }

        private void DisplayResizeCursor(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ob.displayResizeCursor(sender);
        }

        private void ResetCursor(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ob.resetCursor();
        }

        private void ResizeWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ResizeCorners.Visibility = Visibility.Visible;
        }

        private void ResizeCorners_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.RightButton == MouseButtonState.Pressed)
            //    WidgetWindow_Deactivated(sender, e);
        }

        #endregion Resize

        #region WindowControls

        private void AppBackground_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void MinimizeGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(this.WindowState != WindowState.Maximized)
            {
                this.Topmost = true;
                this.WindowState =  WindowState.Maximized;
            }
            else
            {
                this.Topmost = false;
                this.WindowState = WindowState.Normal;
            }
        }

        private void CloseGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void MaximizeZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
                MaximizeGrid_MouseLeftButtonDown(sender, e);
            AppBackground_MouseLeftButtonDown(sender, e);
        }

        #endregion WindowControls    

        #region SideBar

        private void ViewboxSideBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(e.NewSize.Width < 125 && e.PreviousSize.Width > 125)
            {
                Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), LabelHippocampTitle);
                Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), HippocampHippocampTitle);
            }
            else if(e.NewSize.Width > 125 && e.PreviousSize.Width < 125)
            {
                Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), LabelHippocampTitle);
                Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), HippocampHippocampTitle);
            }
        }

        private void ControlGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            var l = sender as Grid;
            if (l.Name == "MinimizeGrid")
                Animations.Opacity(0.9, 0.5, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), MinimizeRec);
            else if (l.Name == "MaximizeGrid")
                Animations.Opacity(0.9, 0.5, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), MaximizeBor);
            else if (l.Name == "CloseGrid")
                Animations.Opacity(0.9, 0.5, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), CloseGri);
        }

        private void ControlGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            var l = sender as Grid;
            if (l.Name == "MinimizeGrid")
                Animations.Opacity(0.5, 0.9, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), MinimizeRec);
            else if (l.Name == "MaximizeGrid")
                Animations.Opacity(0.5, 0.9, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), MaximizeBor);
            else if (l.Name == "CloseGrid")
                Animations.Opacity(0.5, 0.9, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), CloseGri);
        }

        CollectionPage cp;
        private bool CreatePage(Grid g)
        {
            string tag = g.Tag as string;
            if (checkNotSamePage(tag)) return false;

            if(tag == "HomePage")
            {
                HomePage hp = new HomePage(this);
                return addPageToContainer(hp);
            }
            else if (tag == "CollectionPage")
            {
                //if (cp == null) cp = new CollectionPage(this);
                CollectionPage cpx = new CollectionPage(this);
                return addPageToContainer(cpx);
            }
            else if (tag == "LikePage")
            {
                LikePage lP = new LikePage(this);
                return addPageToContainer(lP);
            }
            else if(tag == "NewPlaylist")
            {
                CreateNewPlaylist("New Playlist");
                CreateGenericsSideBarSPGridOBJ();
                return false;
            }
            else if (tag.IndexOf("HPCAMPPlaylist.") != -1)
            {
                string pname = tag.Remove(0, "HPCAMPPlaylist.".Length);
                var playlists = GetUserExperienceFromProfile(loadedProfile).playlists;
                HPlaylist p = playlists.Find(x => x.Name + "#" + x.id == pname);

                if(p != null)
                {
                    var pl = generateAlbumFromPaths(p.hSongs, p.Name, p.Cover);
                    pl.Songs.Reverse();
                    openAlbumView(pl, null, true, playlists.FindIndex(x => x.Name + "#" + x.id == pname), p);
                }
                return false;
            }

            HC.WriteLine("ERROR IN CreatePage(Grid g), HIPPOCAMP WAS NOT ABLE TO CREATE THE PAGE YOU ASKED FOR (" + tag + ").", ConsoleColor.Blue, ConsoleColor.DarkRed);
            return false;
        }

        public HAlbum generateAlbumFromPaths(List<string> paths, string name, byte[] cover)
        {
            var album = new HAlbum() { Artist = MainWindow.loadedProfile.Username, Name = name, Cover = cover };

            int i = 1;
            foreach (var song in paths)
            {
                foreach (var a in discoveredAlbums)
                {
                    foreach (var s in a.Songs)
                    {
                        if (s.Path == song)
                        {
                            var ns = new HSong() { Name = s.Name, Artist = s.Artist, Duration = s.Duration, Path = s.Path, Cover = a.Cover };
                            album.Songs.Add(ns);
                            continue;
                        }
                    }
                }
            }

            return album;
        }


        public void LoadSideBarPlaylists()
        {
            PlaylistsSidebarSP.Children.Clear();
            CreateGenericsSideBarSPGridOBJ();
        }

        public void CreateGenericsSideBarSPGridOBJ()
        {
            var playlists = GetUserExperienceFromProfile(loadedProfile).playlists;
            if (playlists.Count > 0) SideBarSeparator.Visibility = Visibility.Visible; else SideBarSeparator.Visibility = Visibility.Collapsed;
            PlaylistsSidebarSP.Children.Clear();
            foreach (var playlist in playlists)
            {
                Grid g = new Grid() { Tag = "HPCAMPPlaylist." + playlist.Name + "#" + playlist.id, Height = 40, Margin = new Thickness(10, 10, 10, 2.5), Cursor = Cursors.Hand, Background = new SolidColorBrush(Colors.Transparent) };
                g.MouseLeave += SideBarGrid_MouseLeave;
                g.MouseEnter += SideBarGrid_MouseEnter;
                g.MouseLeftButtonDown += SideBarGrid_MouseLeftButtonDown;

                Label label = new Label()
                {
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = 15,
                    Style = Username.Style,
                    Foreground = new SolidColorBrush(theme.getColorFromPropertyName("regularText")),
                    Margin = new Thickness(15, 1, 0, -3),
                    Content = playlist.Name
                };

                Image image = new Image()
                {
                    Opacity = 0.4,
                    Margin = new Thickness(8, 1, 0, -1),
                    Source = new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/PauseEmpty.png")),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 38
                };

                g.Children.Add(label);
                //g.Children.Add(image);
                PlaylistsSidebarSP.Children.Add(g);
            }
        }

        public bool checkNotSamePage(string tag)
        {
            if (PageContainer.Children.Count > 0)
                if (PageContainer.Children[0].ToString().IndexOf(tag) != -1) return true;

            return false;
        }

        private bool addPageToContainer(UIElement uIElement)
        {
            int ind = 0;
            if(PageContainer.Children.Count > 0)
            {
                ind++;
                PageContainer.Children[0].Visibility = Visibility.Collapsed;
                PageContainer.Children[0].Visibility = Visibility.Visible;
                Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 350 : 10), PageContainer.Children[0] as FrameworkElement);
                Animations.BlurRadius(250, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), PageContainer.Children[0] as FrameworkElement);

                new Thread(() =>
                {

                    Thread.Sleep(300);
                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        PageContainer.Children[0].Visibility = Visibility.Collapsed;
                    }));
                    Thread.Sleep(300);
                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        PageContainer.Children.RemoveAt(0);
                    }));

                }).Start();
                //MessageBox.Show(PageContainer.Children[0].ToString() + "/" + uIElement.ToString());
            }

            PageContainer.Children.Add(uIElement);

            Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 350 : 10), PageContainer.Children[ind] as FrameworkElement);
            Animations.BlurRadius(0, 250, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), PageContainer.Children[ind] as FrameworkElement);


            GC.Collect();
            return true;
        }

        private Grid selectedSideBarGrid;
        private bool canMoveTabSideBar = true;
        public void SideBarGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!canMoveTabSideBar) return;
            //if (selectedSideBarGrid == sender as Grid) return;
            canMoveTabSideBar = false;
            new Thread(() => { Thread.Sleep(600); canMoveTabSideBar = true; }).Start();

            Grid g = sender as Grid;
            bool r = CreatePage(g);
            HC.WriteLine("CreatePage(g) return:" + r, ConsoleColor.Blue);
            if (!r) return;

            Grid lastSelectedGrid = selectedSideBarGrid;
            selectedSideBarGrid = g;
            HideSideBarFilledImages(lastSelectedGrid);
            MoveTabRectangleToSelectedSideBar();
            Animations.Opacity(1, g.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 175 : 10), g);
            Animations.Scale(g.Children[g.Children.Count - 1], 0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 175 : 10));
        }

        public void hideSideBarFilledImages()
        {
            foreach (var g in SideBarSP.Children)
            {
                Grid grid = g as Grid;
                if (grid == null) continue;
                if (grid.Children.Count == 0) continue;
                grid.Children[grid.Children.Count - 1].Visibility = Visibility.Hidden;
                Animations.Opacity(0.7, grid.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 175 : 10), grid);
                Animations.Scale(grid.Children[grid.Children.Count - 1], 1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 175 : 10));
            }
        }

        private void MoveTabRectangleToSelectedSideBar()
        {
            int scaleFactor = 0;
            for (scaleFactor = 0; scaleFactor < SideBarSP.Children.Count; scaleFactor++)
                if (SideBarSP.Children[scaleFactor] == selectedSideBarGrid)
                    break;

            HC.WriteLine(scaleFactor);
            Animations.MarginToMargin(TabRectangle.Margin, new Thickness(0, 52 * scaleFactor + ((scaleFactor > 2) ? -35 : 0), 0, 0), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 175 : 10), TabRectangle);
            
            GC.Collect();
        }
      
        private void MoveToSelectedSideBarGrid(Grid lastSelectedGrid)
        {
            int newPosition = SideBarSP.Children.IndexOf(selectedSideBarGrid);
            int lastPosition = SideBarSP.Children.IndexOf(lastSelectedGrid);

            Grid newPage = GridPages.Children[newPosition] as Grid;
            Grid oldPage = GridPages.Children[lastPosition] as Grid;

            //if (newPage.Name == "LikesPage") openLikesView();
            //if (oldPage.Name == "LikesPage") closeLikesView();

            Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 350 : 10), oldPage);
            Animations.BlurRadius(250, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), oldPage);

            Animations.Opacity(1,0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 350 : 10), newPage);
            Animations.BlurRadius(0, 250, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), newPage);
        }
        
        public void HideSideBarFilledImages(Grid lastSelectedGrid = null, bool force = false)
        {
            foreach (var g in SideBarSP.Children)
            {
                Grid grid = g as Grid;
                if (selectedSideBarGrid != grid)
                {
                    if (grid == null) continue;
                    if (grid.Children.Count == 0) continue;
                    grid.Children[grid.Children.Count - 1].Visibility = Visibility.Hidden;
                    Animations.Opacity(0.7, grid.Opacity, new TimeSpan(0, 0, 0, 0, !force ? !hippocampSettings.removeAnimations ? 175 : 10 : 10), grid);
                    Animations.Scale(grid.Children[grid.Children.Count - 1], 1, 0, new TimeSpan(0, 0, 0, 0, !force ? !hippocampSettings.removeAnimations ? 175 : 10 : 0));
                    if (lastSelectedGrid != null && grid == lastSelectedGrid)
                        grid.Children[grid.Children.Count - 1].Visibility = Visibility.Visible;
                }
                else grid.Children[grid.Children.Count - 1].Visibility = Visibility.Visible;
            }
        }

        private void SideBarGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (selectedSideBarGrid != sender as Grid)
                Animations.Opacity(0.5, (sender as Grid).Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 175 : 10), (sender as Grid));
        }

        private void SideBarGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if(selectedSideBarGrid != sender as Grid)
                Animations.Opacity(0.7, (sender as Grid).Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 175 : 10), (sender as Grid));
        }

        #endregion SideBar

        private void AvatarGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AvatarGrid_MouseLeftButtonUp(null, null);
            ProfilePUP profilePUP = new ProfilePUP() { xMainWindow = this, Margin = new Thickness(35) };
            PUP.Children.Insert(1, profilePUP);
            OpenPUP();
        }

        private void OpenPUP()
        {
            Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), PUP);
            ClosePUPGrid.Visibility = Visibility.Visible;
        }

        public void ClosePUP()
        {
            if (PUP_willNeedReloadUsers)
            {
                PUP_willNeedReloadUsers = false;
                loadProfiles(true);
            }
            Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), PUP);
            new Thread(() =>
            {
                Thread.Sleep(300);
                Application.Current.Dispatcher.Invoke(new Action(() => { clearPUP(); }));
            }).Start();
        }

        private void ClosePUP_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ClosePUP();
        }

        private void Favorite_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            likeSong(PlayingSong, true);
        }

        private void ResetMediaControls()
        {
            RepeatControl.Children[1].Visibility = Visibility.Hidden;
            notifyIconPanel.RepeatControl.Children[1].Visibility = Visibility.Hidden;
            ShuffleControl.Children[1].Visibility = Visibility.Hidden;
            notifyIconPanel.ShuffleControl.Children[1].Visibility = Visibility.Hidden;

            PlayerMediaGrid.Visibility = Visibility.Hidden;
            PageContainer.Margin = new Thickness(0);
            FakeSideBarGrid.Margin = new Thickness(9, 150, -22, -54);

            SideBar.Margin = new Thickness(0);
            AppBackground.Margin = new Thickness(0);

            SideBar.CornerRadius = new CornerRadius(3, 0, 0, 3);
            AppBackground.CornerRadius = new CornerRadius(0, 3, 3, 0);
        }

        private HAlbum PlayingAlbum;
        private HSong PlayingSong;
        private bool canInteractWSP = false;
        private void createSongPlayer()
        {
            if (SongPlayer != null) SongPlayer.AbortPlayer(); //close the position thread in Re-Hippocamp.Misc.SongPlayer

            SongPlayer = new SongPlayer(this, loadedProfile); //creating the songplayer
            HC.WriteAllBytes("settings.hs", BO.ObjectToByteArray(MainWindow.hippocampSettings));

            SongPlayer.SongPositionChanged += SongPlayer_SongPositionChanged;
            SongPlayer.SongPlayingEnded += SongPlayer_SongPlayingEnded;
            SongPlayer.SongChangedPosition += SongPlayer_SongChangedPosition;

            canInteractWSP = true;
            updateSoundImage();
        }

        private void SongPlayer_SongChangedPosition(int index)
        {
            if (!canInteractWSP) return;

            if (index != -1) updateSongUI();
        }

        SongPlayer.SongPlayerState oldstate = SongPlayer.SongPlayerState.Stopped;
        public bool forceOldState = false;

        private void SongPlayer_SongPositionChanged(TimeSpan timeSpan)
        {
            if (!canInteractWSP) return;


            MediaSlider.Value = timeSpan.TotalSeconds;
            MediaSlider.Maximum = SongPlayer.SongDuration.TotalSeconds;
            SongPosition.Content = timeSpan.Minutes + ":" + timeSpan.Seconds.ToString("00");
            SongDuration.Content = SongPlayer.SongDuration.Minutes + ":" + SongPlayer.SongDuration.Seconds.ToString("00");
            MediaSliderBP.Value = MediaSlider.Value;
            MediaSliderBP.Maximum = MediaSlider.Maximum;
            SongPositionBP.Content = SongPosition.Content;
            SongDurationBP.Content = SongDuration.Content;

            if (PlayingSong != null && SongPlayer.State == SongPlayer.SongPlayerState.Playing)
                Discord.updatePresence(PlayingSong, SongPosition.Content.ToString(), SongDuration.Content.ToString());

            if (SongPlayer.State != oldstate || forceOldState)
            {
                forceOldState = false;
                oldstate = SongPlayer.State;

                if (SongPlayer.State == SongPlayer.SongPlayerState.Playing)
                {

                    PlayLogo.Visibility = Visibility.Hidden;
                    notifyIconPanel.PlayLogo.Visibility = Visibility.Hidden;
                    PlayLogoBP.Visibility = Visibility.Hidden;
                    PauseLogoBP.Visibility = Visibility.Visible;
                    PauseLogo.Visibility = Visibility.Visible;
                    notifyIconPanel.PauseLogo.Visibility = Visibility.Visible;
                    ThumbPlayPause.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/PauseEmpty.png"));
                    if(PlayingSong != null && PlayingSong.Name != null)
                        this.Title = "Hippocamp: " + PlayingSong.Name;
                }
                else if (SongPlayer.State == SongPlayer.SongPlayerState.Pause)
                {
                PauseLogo.Visibility = Visibility.Hidden;
                notifyIconPanel.PauseLogo.Visibility = Visibility.Hidden;
                PauseLogoBP.Visibility = Visibility.Hidden;
                PlayLogo.Visibility = Visibility.Visible;
                PlayLogoBP.Visibility = Visibility.Visible;
                notifyIconPanel.PlayLogo.Visibility = Visibility.Visible;
                ThumbPlayPause.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/PlayEmpty.png"));
                this.Title = "Hippocamp";
                
                new Thread(() =>
                {
                
                    for (int i = 0; i < 2; i++)
                    {
                        Discord.Stop();
                        Thread.Sleep(500);
                    }
                
                }).Start();
                }
                else
                {
                    this.Title = "Hippocamp";
                }
            }

            HC.WriteLine(SongPlayer.State);
        }

        bool isBPOpen = false;

        public void updatePlayingSong(HSong hSong, HAlbum hAlbum)
        {
            PlayingAlbum = hAlbum;
            bool r = SongPlayer.PlaySong(hSong, hAlbum, SongThumbnail.createThumbnail(hSong, hAlbum));
            updateFavIcons(isHSongInLikes(hSong));
            HC.WriteLine("SongPlayer.PlaySong: " + r, ConsoleColor.Magenta);
            if (r)
            {
                if (PlayerMediaGrid.Visibility == Visibility.Hidden)
                {
                    Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), PlayerMediaGrid);
                    Animations.MarginToMargin(new Thickness(0), new Thickness(0, 0, 0, 63), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), PageContainer);
                    Animations.MarginToMargin(new Thickness(0), new Thickness(0, 0, 0, 63), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), AppBackground);
                    Animations.MarginToMargin(new Thickness(0), new Thickness(0, 0, 0, 63), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), SideBar);
                    Animations.MarginToMargin(new Thickness(9, 150, -22, -54), new Thickness(9, 150, -22, 12), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), FakeSideBarGrid);
                    SideBar.CornerRadius = new CornerRadius(3, 0, 0, 0);
                    AppBackground.CornerRadius = new CornerRadius(0, 3, 0, 0);
                }
            }
            else
            {
                PlayerMediaGrid.Visibility = Visibility.Hidden;
                PageContainer.Margin = new Thickness(0);
            }
        }

        private void updateSongUI()
        {
            HSong hSong = null;
            if (!canInteractWSP) return;
            if (PlayingAlbum == null) return;

            HC.WriteLine("INDEX:" + SongPlayer.SongIndex);
            if(SongPlayer.SongIndex == -1) { HC.WriteLine("SongPlayer.SongIndex = -1 ??????????????"); return; }
            hSong = PlayingAlbum.Songs.ElementAt(SongPlayer.SongIndex);

            if (hSong == null) return;
            if (PlayingAlbum == null) return;

            PlayingSong = hSong;
            updateFavIcons(isHSongInLikes(hSong));

            //MediaSlider.Width = 450;

            PlayingNameSong.Text = "";
            PlayingArtistSong.Text = "";
            if (PlayingSong.Name != null)
            {
                if (hSong.Name.Length > 29)
                    PlayingNameSong.Text = hSong.Name.Substring(0, 26) + "...";
                else
                    PlayingNameSong.Text = hSong.Name;

                PlayingArtistSong.Text = hSong.Artist;

                //if (PlayingSong.Name.Length > 10)
                //    MediaSlider.Width = 400;

            }

            addNewArtistPlayedCurrentProfile(PlayingAlbum.Name + ":" + PlayingAlbum.Artist);
            IncreaseSongPlayCount(hSong.Path, loadedProfile, true); //will increase the hSong play count.

            byte[] pData = getSongOrAlbumCover(hSong, PlayingAlbum);
            if (pData == null) pData = PlayingAlbum.Cover;
            SongCover.Fill = new ImageBrush(BO.LoadImage(pData, 120));
            SongCoverBlur.Fill = new ImageBrush(BO.LoadImage(pData, 2));

            if (hippocampSettings.useCoverColor)
            {
                foreach (Border element in Theme.FindVisualChildren<Border>(PlayerMediaGrid))
                {
                    if (element.Name == "S1" || element.Name == "S2")
                        element.Background = new ImageBrush(BO.LoadImage(pData, 1));
                }
            }


            notifyIconPanel.DeployView(pData, hSong);
            forceOldState = true;

            BPAlbum.Background = new ImageBrush(BO.LoadImage(pData, 0));
            BPAlbumB.Background = new ImageBrush(BO.LoadImage(pData, 100));
            BPAlbumRevert.Background = BPAlbum.Background;
            BPArtists.Content = PlayingArtistSong.Text;
            BPTitle.Content = PlayingNameSong.Text;
            BPTitleB.Content = PlayingNameSong.Text;
            BPTitle.Foreground = new ImageBrush(BO.LoadImage(pData, 1));
            BPAlbumBG.Fill = BPTitle.Foreground;
            var im = new ImageBrush(BO.LoadImage(pData, 0));
            im.Stretch = Stretch.UniformToFill;
            BPAlbumBGB.Fill = im;

            //check
            if(PageContainer.Children.Count != 0)
            {
                LikePage likePage = PageContainer.Children[0] as LikePage;
                if (likePage != null) likePage.checkIsPlaying(hSong, PlayingAlbum, pData);

                AlbumPage albumPage = PageContainer.Children[0] as AlbumPage;
                if (albumPage != null) albumPage.checkIsPlaying(hSong, PlayingAlbum, pData);
            }

        }

        public static byte[] getSongOrAlbumCover(HSong hSong, HAlbum hAlbum)
        {
            if (hSong.Cover != null) return hSong.Cover;

            TagLib.File t = TagLib.File.Create(hSong.Path);
            byte[] pData = null;
            try
            {
                var mStream = new MemoryStream();
                var firstPicture = t.Tag.Pictures.FirstOrDefault();
                if (firstPicture != null)
                {
                    pData = firstPicture.Data.Data;
                }
            }
            catch (Exception)
            {
                pData = hAlbum.Cover;
            }

            if(pData == null) pData = hAlbum.Cover;
            return pData;
        }

        DateTime lastTimeChange = DateTime.MinValue;
        private void MediaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) //pas fiable à 100%
        {
            if (!IsInitialized) return;
            //if(Mouse.LeftButton == MouseButtonState.Pressed) { HC.WriteLine("mouse pressed ignoring."); return; }
            if((int)e.NewValue != (int)SongPlayer.SongPosition.TotalSeconds)
            {
                //Manual change.
                if ((DateTime.Now - lastTimeChange).TotalMilliseconds < 250) return;
                HC.WriteLine((DateTime.Now - lastTimeChange).TotalMilliseconds  + "ms");
                lastTimeChange = DateTime.Now;
                HC.WriteLine("MANUAL CHANGE. " + (int)e.NewValue + "/" + (int)SongPlayer.SongPosition.TotalSeconds);
                SongPlayer.SongPosition = new TimeSpan(0, 0, 0, (int)e.NewValue);
            }
        }

        public void PlayerPausePlay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            changePausePlayState();
        }

        private void openBigPlayer()
        {
            //BigPlayerGrid.Visibility = Visibility.Visible;
            Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), BigPlayerGrid);
            Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), All);
            Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), CurrentProfileAvatar);
            Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), PlayerMediaGrid);
            Animations.Opacity(0, 0.975, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), PlayerBarBottom);
            Animations.Opacity(0.5, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), CloseBigPlayer);
        }

        private void closeBigPlayer()
        {
            //BigPlayerGrid.Visibility = Visibility.Visible;
            Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), BigPlayerGrid);
            Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), All);
            Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), CurrentProfileAvatar);
            Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), PlayerMediaGrid);
            Animations.Opacity(0.975, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), PlayerBarBottom);
            Animations.Opacity(0, 0.5, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), CloseBigPlayer);
        }

        public void changePausePlayState()
        {
            if (SongPlayer.State == SongPlayer.SongPlayerState.Playing)
                SongPlayer.SetPlayerState(SongPlayer.SongPlayerState.Pause);
            else if (SongPlayer.State == SongPlayer.SongPlayerState.Pause)
                SongPlayer.SetPlayerState(SongPlayer.SongPlayerState.Playing);
        }

        private void SongPlayer_SongPlayingEnded()
        {
            if (!canInteractWSP) return;
            playNextSongInAlbum();
        }

        public void playNextSongInAlbum()
        {
            SongPlayer.nextSong();
            return;


            HC.WriteLine("NEXT SONG FROM: " + PlayingSong.Name);
            int posInAlbum = PlayingAlbum.Songs.FindIndex(x => x == PlayingSong);

            if (posInAlbum >= PlayingAlbum.Songs.Count -1) { } //last song of the album, album ended.
            else if (posInAlbum < PlayingAlbum.Songs.Count - 1)
            {
                updatePlayingSong(PlayingAlbum.Songs[posInAlbum + 1], PlayingAlbum);
            } //we can play the next song in the album
        }

        public void playPrecedentSongInAlbum()
        {
            SongPlayer.previousSong();
            return;

            int posInAlbum = PlayingAlbum.Songs.FindIndex(x => x == PlayingSong);

            if (posInAlbum == 0) { } //first song of the album, cannot go backward here.
            else if (posInAlbum > 0)
            {
                updatePlayingSong(PlayingAlbum.Songs[posInAlbum -1], PlayingAlbum);
            } //we can play the next song in the album
        }

        public void openAlbumView(HAlbum hAlbum, HSong bringToFrontHSong = null, bool isplaylist = false, int playlistIndex = -1, HPlaylist hPlaylist = null)
        {
            AlbumPage albumPage = new AlbumPage(this);
            albumPage.Opacity = 0;
            addPageToContainer(albumPage);
            albumPage.openAlbumView(hAlbum, isplaylist, playlistIndex, hPlaylist);

            if (PlayingSong != null && PlayingAlbum != null)
                albumPage.checkIsPlaying(PlayingSong, PlayingAlbum, new byte[0]);
            albumPage.Opacity = 1;
            if (bringToFrontHSong != null) albumPage.bringToFrontHSong(bringToFrontHSong);
        }

        private void NextSongAlbum_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            playNextSongInAlbum();
        }

        private void PlayPrecedentSongAlbum_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            playPrecedentSongInAlbum();
        }

        private void PlayPauseThumbButtonInfo_Click(object sender, EventArgs e) { changePausePlayState(); }

        private void NextSongThumbButtonInfo_Click(object sender, EventArgs e) { NextSongAlbum_MouseLeftButtonUp(null, null); }

        private void PrecedentSongThumbButtonInfo_Click(object sender, EventArgs e) { PlayPrecedentSongAlbum_MouseLeftButtonUp(null, null); }

        public void BringAppToViewThumbButtonInfo_Click(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Normal;
            new Thread(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { this.Topmost = true; }));
                Thread.Sleep(25);
                Application.Current.Dispatcher.Invoke(new Action(() => { this.Topmost = false; }));
            }).Start();
        }

        public void ShuffleControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SongPlayer.Shuffle)
            {
                SongPlayer.Shuffle = false;
                ShuffleControl.Children[1].Visibility = Visibility.Hidden;
                notifyIconPanel.ShuffleControl.Children[1].Visibility = Visibility.Hidden;
            }
            else
            {
                SongPlayer.Shuffle = true;
                ShuffleControl.Children[1].Visibility = Visibility.Visible;
                notifyIconPanel.ShuffleControl.Children[1].Visibility = Visibility.Visible;
            }
        }

        public void RepeatControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SongPlayer.Repeat)
            {
                SongPlayer.Repeat = false;
                RepeatControl.Children[1].Visibility = Visibility.Hidden;
                notifyIconPanel.RepeatControl.Children[1].Visibility = Visibility.Hidden;
            }
            else
            {
                SongPlayer.Repeat = true;
                RepeatControl.Children[1].Visibility = Visibility.Visible;
                notifyIconPanel.RepeatControl.Children[1].Visibility = Visibility.Visible;
            }
        }

        private void Sound_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SongPlayer.isMuted = !SongPlayer.isMuted;
            if (SongPlayer.isMuted) SoundSlider.Value = 0;
            updateSoundImage();
            return;
        }

        private double lastVolume = 100;
        private void updateSoundImage()
        {
            if (SongPlayer.isMuted)
            {
                SoundIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/Speaker3.png"));
            }
            else
            {
                SoundSlider.Value = lastVolume;
                if(SongPlayer.PlayerVolume > .7) SoundIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/Speaker.png"));
                else if(SongPlayer.PlayerVolume >=.5) SoundIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/Speaker1.png"));
                else if(SongPlayer.PlayerVolume < .35 && SongPlayer.PlayerVolume >= 0.01) SoundIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/Speaker2.png"));
                else if(SongPlayer.PlayerVolume <= 0.019) { SoundIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/Speaker3.png")); SoundSlider.Value = 0; }
            }
        }

        private void SoundIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            SoundLevel.Visibility = Visibility.Visible;
        }

        private void SoundIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            SoundLevel.Visibility = Visibility.Hidden;
        }

        private void SoundSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsInitialized) return;
            try
            {
                SongPlayer.PlayerVolume = (double)e.NewValue / 10;
                SongPlayer.isMuted = false;
                lastVolume = e.NewValue;
                updateSoundImage();
            }
            catch (Exception)
            {

            }
        }

        private void SoundSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var slider = (Slider)sender;
                Point position = e.GetPosition(slider);
                double d = 1.0d / slider.ActualWidth * position.X;
                var p = slider.Maximum * d;
                slider.Value = p;
            }
        }

        private bool checkSecondInstanceOfHippocamp()
        {
            string n = System.AppDomain.CurrentDomain.FriendlyName;
            HC.WriteLine("searching for " + n + "...");
            Process[] processes = Process.GetProcessesByName(n.Replace(".exe", ""));
            if(processes.Length <= 1)
            { HC.WriteLine(n + " not found good (" + processes.Length + ")"); return false; }
            else
            {
                HC.WriteLine(n + " found, processing.");
                MHippocamp.show(n.Replace(".exe", ""));
                Environment.Exit(10);
                return true;
            }
        }

        public bool likeSong(HSong pl, bool anim = false)
        {
            if (pl == null) { HC.WriteLine("PL WAS NULL.", ConsoleColor.DarkRed); return false; }
            if (pl == null) return false;
            bool liked = isHSongInLikes(pl);

            UserExperience userExperience = GetUserExperienceFromProfile(loadedProfile);
            if (liked)
            {
                userExperience.Likes.hSongs.Remove(pl.Path);
                liked = false;
            }
            else
            {
                userExperience.Likes.hSongs.Add(pl.Path);
                liked = true;
            }

            SaveUserExperience(userExperience);
            updateFavIcons(liked, anim);
            saveProfiles();

            return liked;
            //updateLikesSP();
        }

        private void updateFavIcons(bool l, bool anim = false)
        {

            LikePage likePage = PageContainer.Children[0] as LikePage;
            if (likePage != null) likePage.updateLikesSP();

            AlbumPage albumPage = PageContainer.Children[0] as AlbumPage;
            if (albumPage != null) albumPage.checkIsPlaying(PlayingSong, PlayingAlbum, null);

            if (l)
            {
                if (anim)
                {
                    new Thread(() =>
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            FavAnim.Opacity = 1;
                            FavAnim.Visibility = Visibility.Visible;
                            Animations.Scale(FavAnim, 0, 1.1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 125 : 10));
                            FilledFav.Visibility = Visibility.Visible;
                            FilledFav.Opacity = 1;
                            Animations.Scale(FilledFav, 0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 200 : 10));
                        }));
                        Thread.Sleep(!hippocampSettings.removeAnimations ? 150 : 0);
                        Application.Current.Dispatcher.Invoke(new Action(() => { Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), FavAnim); }));
                    }).Start();
                }
                else { FilledFav.Visibility = Visibility.Visible; }

                ThumbFavorite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/FavoriteFilled.png"));
            }
            else
            {
                if (anim)
                {
                    Animations.Scale(FilledFav, 1,0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 200 : 10));
                    Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 200 : 10), FilledFav);
                }
                else
                    FilledFav.Visibility = Visibility.Hidden;

                ThumbFavorite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/FavoriteEmptyThumb.png"));
            }
        }

        public bool isHSongInLikes(HSong hSong)
        {
            if (hSong == null) return false;
            var ld = userProfiles.Profiles[userProfiles.Profiles.FindIndex(x => x.id == loadedProfile.id)];
            foreach (var l in GetUserExperienceFromProfile(loadedProfile).Likes.hSongs)
            {
                if (l == hSong.Path) return true;
            }
            return false;
        }

        private void ThumbFavorite_Click(object sender, EventArgs e)
        {
            likeSong(PlayingSong);
        }

        private void hardwareAccelerationSetting()
        {
            HC.WriteLine("hippocampSettings.disableHardwareAcceleration:" + hippocampSettings.disableHardwareAcceleration);
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hippocampSettings.disableHardwareAcceleration)
            {
                if (hwndSource != null)
                {
                    HC.WriteLine("disabled HardwareAcceleration");
                    hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
                }
            }
            else
            {
                if (hwndSource != null)
                {
                    HC.WriteLine("enabled HardwareAcceleration");
                    hwndSource.CompositionTarget.RenderMode = RenderMode.Default;
                }
            }
        }

        private void window_Activated(object sender, EventArgs e)
        {
            hardwareAccelerationSetting();
        }

        private void window_Deactivated(object sender, EventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                HC.WriteLine("disabled HardwareAcceleration");
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }
        }

        private void PlayingNameSong_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as TextBlock).TextDecorations = TextDecorations.Baseline;
        }

        private void PlayingNameSong_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as TextBlock).TextDecorations = null;
        }

        public HAlbum getHAlbumFromHSong(HSong hSong)
        {
            if (hSong == null) return null;
            foreach (var a in discoveredAlbums)
            {
                foreach (var s in a.Songs)
                {
                    if (s.Path == hSong.Path)
                    {
                        //HippocampSong hippocampSong = new HippocampSong(this) { Margin = new Thickness(0, 5, 0, 5) };
                        return a;
                    }
                }
            }
            return null;
        }

        private void PlayingNameSong_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //AlbumViewSP.Children.Clear();
            //if (AlbumPage.Visibility == Visibility.Visible) closeAlbumView();
            //if (LikesPage.Visibility == Visibility.Visible) closeLikesView();

            var ha = getHAlbumFromHSong(PlayingSong);
            if(ha != null)
                openAlbumView(ha);
        }

        private void Settings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AvatarGrid_MouseLeftButtonUp(null, null);
            SettingsPUP settingsPUP = new SettingsPUP(this) { Margin = new Thickness(35) };
            PUP.Children.Insert(1, settingsPUP);
            OpenPUP();
        }

        private void window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.XButton2)
                HC.WriteLine("XButton2");
            else if (e.ChangedButton == MouseButton.XButton1)
            {
                for (int i = 0; i < 5; i++)
                {
                    SideBarGrid_MouseLeftButtonDown(Home, null);
                }
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            BigPlayerIcon.Visibility = Visibility.Visible;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            BigPlayerIcon.Visibility = Visibility.Hidden;
        }

        private void BigPlayerIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            openBigPlayer();
        }

        private void CloseBigPlayer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            closeBigPlayer();
        }

        Storyboard storyboard = new Storyboard();
        private void BigPlayerGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            storyboard.Stop();
            storyboard = new Storyboard();
            if (e.Delta > 0)
            {
                if (SongPlayer.PlayerVolume <= .95)
                    SongPlayer.PlayerVolume += 0.05;

                if (!hippocampSettings.blockMaxVolume)
                {
                    if (SongPlayer.PlayerVolume >= 0.8)
                        SongPlayer.PlayerVolume = 0.8;
                }
            }
            else
            {
                if (SongPlayer.PlayerVolume >= 0.05)
                    SongPlayer.PlayerVolume -= 0.05;

                if (PBVolume.Value < 5)
                    SongPlayer.PlayerVolume = 0;
            }

            PBVolume.Value = SongPlayer.PlayerVolume * 100;
            if (!hippocampSettings.blockMaxVolume)
                PBVolume.Maximum = 80;
            else
                PBVolume.Maximum = 100;

            HC.WriteLine("PBVOLUME:" + PBVolume.Value);
            HC.WriteLine(SongPlayer.PlayerVolume);
            PBVolumeG.Visibility = Visibility.Visible;
            lastVolume = SongPlayer.PlayerVolume * 10;
            HC.WriteLine(lastVolume);
            updateSoundImage();

            OpacityAnim(0, 1, new TimeSpan(0, 0, 0, 1, 500), PBVolumeG);
        }


        public void DeployNotification(string content)
        {
            HippocampNotification hippocampNotification = new HippocampNotification() { Text = content};
            NotificationSP.Children.Add(hippocampNotification);
            Animations.MarginToMargin(new Thickness(0, 10, -20, 10), new Thickness(-20, 10, 20, 10), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 150 : 10), hippocampNotification);
            new Thread(() =>
            {
                Thread.Sleep(2500);
                Application.Current.Dispatcher.Invoke(new Action(() => { Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 1000 : 10), hippocampNotification); }));
                Thread.Sleep(1100);
                Application.Current.Dispatcher.Invoke(new Action(() => { NotificationSP.Children.Remove(NotificationSP); }));
            }).Start();
        }


        private void OpacityAnim(double to, double from, TimeSpan speed, FrameworkElement obj, bool secondPhase = false)
        {
            var a = new DoubleAnimation
            {
                From = from,
                To = to,
                FillBehavior = FillBehavior.Stop,
                Duration = speed
            };
            storyboard = new Storyboard();

            storyboard.Children.Add(a);

            Storyboard.SetTarget(a, obj);
            Storyboard.SetTargetProperty(a, new PropertyPath(FrameworkElement.OpacityProperty));
            storyboard.Completed += delegate {
                obj.Opacity = to;
                storyboard.Stop();
            };
            storyboard.Begin();
        }

        private void AvatarItemEnter(object sender, MouseEventArgs e)
        {
            Rectangle r = (sender as Grid).Children[0] as Rectangle;
            Animations.ColorAnimationOBJ((r.Fill as SolidColorBrush).Color, Color.FromArgb(50, 255, 255, 255), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 200 : 10), r);
        }

        private void AvatarItemLeave(object sender, MouseEventArgs e)
        {
            Rectangle r = (sender as Grid).Children[0] as Rectangle;
            Animations.ColorAnimationOBJ((r.Fill as SolidColorBrush).Color, Color.FromArgb(14, 200, 200, 200), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 200 : 10), r);
        }

        Key keyCurrentlyDown = Key.Cancel;
        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            keyCurrentlyDown = e.Key;
        }

        private void window_KeyUp(object sender, KeyEventArgs e)
        {
            keyCurrentlyDown = Key.Cancel;
        }

        private void LabelHippocampTitle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DeployNotification("xx");
        }

        private void SoundSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta > 0)
            {
                if (SoundSlider.Value < SoundSlider.Maximum)
                    SoundSlider.Value += 0.5;
            }else if(e.Delta < 0)
            {
                if (SoundSlider.Value > SoundSlider.Minimum)
                    SoundSlider.Value -= 0.5;
            }
        }
    }
}

