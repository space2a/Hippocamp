using Re_Hippocamp.Controls;
using Re_Hippocamp.Misc;

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Re_Hippocamp.Pages
{

    public partial class LikePage : UserControl
    {
        public Settings hippocampSettings;

        private MainWindow mW;
        public LikePage(MainWindow mainWindow)
        {
            InitializeComponent();
            hippocampSettings = MainWindow.hippocampSettings;
            mW = mainWindow;

            ExploreCollection.ButtonPressed += ExploreCollection_ButtonPressed;

            openLikesView();

            this.IsVisibleChanged += LikePage_IsVisibleChanged;
            MainWindow.theme.applyThemeInGrid(this.Content as Grid);
        }

        private void LikePage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.ToString() == "True") return;
            closeLikesView();
        }



        private void openLikesView()
        {
            //Animations.Opacity(0, TabRectangle.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), TabRectangle);
            Animations.ColorAnimationOBJ(MainWindow.theme.getColorFromPropertyName("tabrectangleColor"), Color.FromArgb(220, 215, 71, 71), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), mW.TabRectangle.Children[0], "Border.Background");
            Animations.ColorAnimationOBJ(MainWindow.theme.getColorFromPropertyName("tabrectangleColor"), Color.FromArgb(220, 215, 71, 71), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), mW.TabRectangle.Children[1], "Border.Background");
            Animations.ColorAnimationOBJ(MainWindow.theme.getColorFromPropertyName("secondaryColor"), Color.FromArgb(220, 215, 71, 71), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), mW.AppBackground, "Border.Background");
            //Animations.Opacity(0, mW.SideBar.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), mW.SideBar);
            


            //Animations.Opacity(1, mW.LikesSideBar.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 270 : 10), mW.LikesSideBar);
            updateLikesSP();

            new Thread(() =>
            {
                Thread.Sleep(250);
                Application.Current.Dispatcher.Invoke(new Action(() => { this.ClipToBounds = false; }));
            }).Start();
        }

        private HAlbum likeAlbum = new HAlbum();
        public void updateLikesSP()
        {
            likeAlbum.Songs.Clear();
            Likes Likes = MainWindow.GetUserExperienceFromProfile(MainWindow.loadedProfile).Likes;
            likeAlbum = new HAlbum() { Artist = MainWindow.loadedProfile.Username, Name = "HPCAMPLikes", Cover = BO.ImageSourceToBytes(new PngBitmapEncoder(), (LikeCover.Background as ImageBrush).ImageSource) };
            AlbumArtist.Content = likeAlbum.Artist;

            double totalDuration = 0;
            if (LikeViewSP.Children.Count != Likes.hSongs.Count)
            {
                int i = 1;
                LikeViewSP.Children.Clear();
                foreach (var song in Likes.hSongs)
                {
                    foreach (var a in mW.discoveredAlbums)
                    {
                        foreach (var s in a.Songs)
                        {
                            if (s.Path == song)
                            {
                                var ns = new HSong() { Name = s.Name, Artist = s.Artist, Duration = s.Duration, Path = s.Path, Cover = a.Cover };
                                likeAlbum.Songs.Add(ns);
                                continue;
                            }
                        }
                    }
                }

                likeAlbum.Songs.Reverse();
                
                foreach (var song in likeAlbum.Songs)
                {
                    HippocampSong hippocampSong = new HippocampSong(mW, MainWindow.GetHPlaylists()) { Margin = new Thickness(0, 5, 0, 5) };
                    LikeViewSV.Focus();
                    if (i == 1)
                        hippocampSong.Margin = new Thickness(0, 15, 0, 5);

                    hippocampSong.IniSong(song, BO.ByteArrayToObject(BO.ObjectToByteArray(likeAlbum)) as HAlbum, i++);
                    hippocampSong.MouseDoubleClick += HippocampSong_MouseDoubleClick;
                    LikeViewSP.Children.Add(hippocampSong);
                    totalDuration += song.Duration;
                }

                if (likeAlbum.Songs.Count == 0)
                {
                    LikesPagesEmpty.Visibility = Visibility.Visible;
                    AlbumInfo.Visibility = Visibility.Hidden;
                }
                else
                {
                    LikesPagesEmpty.Visibility = Visibility.Hidden;
                    AlbumInfo.Visibility = Visibility.Visible;
                    TimeSpan tp = new TimeSpan(0, 0, 0, (int)totalDuration);
                    string so = "songs";
                    if (likeAlbum.Songs.Count == 1) so = "song";
                    if (tp.Hours != 0)
                        AlbumInfo.Content = likeAlbum.Songs.Count + " " + so + ", " + tp.Hours + "h " + tp.Minutes + " min";
                    else
                        AlbumInfo.Content = likeAlbum.Songs.Count + " " + so + ", " + tp.Minutes + " Min " + tp.Seconds.ToString("00") + "s";
                }

            }


            LikeViewSV.Focus();
        }

        public void checkIsPlaying(HSong hSong, HAlbum PlayingAlbum, byte[] pData)
        {
            HC.WriteLine("checkIsPlaying");
            foreach (var item in LikeViewSP.Children)
            {
                var hps = item as HippocampSong;
                if (hps != null)
                {
                    if (hps.hSong == hSong && hps.hAlbum == PlayingAlbum)
                        hps.songPlaying(pData);
                    else
                        hps.songNotPlaying();
                }
            }
        }

        private void HippocampSong_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var hSong = (sender as HippocampSong);
            HC.WriteLine("Song " + hSong.hSong.Name + ", likes album: " + hSong.hAlbum.Name + ":" + hSong.hAlbum.Songs.Count);
            mW.updatePlayingSong(hSong.hSong, hSong.hAlbum);
        }

        bool doneAn = false;
        private void closeLikesView()
        {
            if (doneAn) return;
            doneAn = true;
            this.ClipToBounds = true;
            LikeViewSV.ScrollToTop();
            Animations.ColorAnimationOBJ(Color.FromArgb(220, 215, 71, 71), MainWindow.theme.getColorFromPropertyName("tabrectangleColor"), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), mW.TabRectangle.Children[0], "Border.Background");
            Animations.ColorAnimationOBJ(Color.FromArgb(220, 215, 71, 71), MainWindow.theme.getColorFromPropertyName("tabrectangleColor"), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), mW.TabRectangle.Children[1], "Border.Background");
            Animations.ColorAnimationOBJ(Color.FromArgb(220, 215, 71, 71), MainWindow.theme.getColorFromPropertyName("secondaryColor"), new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), mW.AppBackground, "Border.Background");

            //Animations.Opacity(0.95, mW.SideBar.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), mW.SideBar);

            //Animations.Opacity(0, mW.LikesSideBar.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 270 : 10), mW.LikesSideBar);
        }

        private void ExploreCollection_ButtonPressed(HippocampButton hippocampButton)
        {
            mW.SideBarGrid_MouseLeftButtonDown(mW.Library, null);
        }
    }
}
