using Re_Hippocamp.Misc;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampSong : UserControl
    {
        public HSong hSong;
        public HAlbum hAlbum;
        public MainWindow MainWindow;
        bool playing = false;

        private List<HPlaylist> hPlaylists;

        HippocampContextMenu hippocampContextMenu;
        HippocampContextMenuItem likeItem = new HippocampContextMenuItem(null) { Text = "Add song to likes" };

        public HippocampSong(MainWindow mainWindow, List<HPlaylist> hPlaylists)
        {
            InitializeComponent();
            BGOutline.Visibility = Visibility.Hidden;
            this.MainWindow = mainWindow;
            PauseIcon.Visibility = Visibility.Hidden;
            PlayIcon.Visibility = Visibility.Hidden;

            this.hPlaylists = hPlaylists;
            this.MouseEnter += HippocampSong_MouseEnter;
        }

        private void HippocampSong_MouseEnter(object sender, MouseEventArgs e)
        {
            createContextMenu(false);
        }

        bool contextMenuCreated = false;
        private void createContextMenu(bool openAfterCreation = true)
        {
            if (contextMenuCreated) { return; }
            contextMenuCreated = true;
            hippocampContextMenu = new HippocampContextMenu();
            this.ContextMenu = hippocampContextMenu;

            likeItem.ClickedItem += delegate (HippocampContextMenuItem x) { LikeImg_MouseLeftButtonUp(null, null); };
            HippocampContextMenuItem playlistMaster = new HippocampContextMenuItem(new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/PlaylistIcon.png"))) { Text = "Add song to playlist", HippocampContextMenu = hippocampContextMenu };

            for (int i = 0; i < hPlaylists.Count; i++)
            {
                HippocampContextMenuItem pItem = new HippocampContextMenuItem(BO.LoadImage(hPlaylists[i].Cover, 50)) { Text = hPlaylists[i].Name };
                playlistMaster.deploySubItem(pItem);
                int index = i;
                pItem.ClickedItem += delegate (HippocampContextMenuItem x)
                {
                    MainWindow.AddSongToPlaylistFromLoadedProfile(hSong, index);
                };
            }

            hippocampContextMenu.Loaded += delegate (object Hsender, RoutedEventArgs He) { HC.WriteLine("hippocampContextMenu.Loaded", ConsoleColor.Red); hippocampContextMenu.createItem(likeItem); hippocampContextMenu.createItem(playlistMaster); };

            if (openAfterCreation)
            {
                hippocampContextMenu.isOpen(true);
            }

            MainWindow.Focus();
        }


        public void IniSong(HSong sd, HAlbum album, int index)
        {
            Artist.Content = "";
            hSong = sd;
            hAlbum = album;

            Index.Content = index;
            if (sd.Name != "")
                Name.Content = sd.Name;
            if (sd.Artist != null)
                Artist.Content = sd.Artist.Replace("; ", ", ").Replace(";", ", ");

            TimeSpan tp = new TimeSpan(0, 0, 0, (int)sd.Duration);
            Length.Content = tp.Minutes + ":" + tp.Seconds.ToString("00");

            checkFav();
        }

        bool liked = false;
        public void checkFav()
        {
            liked = MainWindow.isHSongInLikes(hSong);
            if (liked)
            {
                likeItem.changeTextAndBitmap("Remove song from likes", new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/FavoriteFilled.png")));
                LikeImg.Visibility = Visibility.Visible;
            }
            else
            {
                likeItem.changeTextAndBitmap("Add song to likes", new BitmapImage(new Uri("pack://application:,,,/Re-Hippocamp;component/Resources/Images/FavoriteEmpty.png")));
                LikeImg.Visibility = Visibility.Collapsed;
            }
        }

        public void songPlaying(byte[] cover)
        {
            checkFav();
            if (playing) return;
            playing = true;
            this.Opacity = 1;
            return;
        }

        public void putNewCoverInMYAlbum(byte[] cover)
        {
            HAlbum newHA = BO.ByteArrayToObject(BO.ObjectToByteArray(hAlbum)) as HAlbum;
            newHA.Cover = cover;
            hAlbum = newHA;
        }

        public void songNotPlaying()
        {
            checkFav();
            playing = false;
            this.Opacity = 0.6;
            return;
            if (Index.Foreground == new SolidColorBrush(Colors.White)) return;
            Index.Foreground = new SolidColorBrush(Colors.White);
            Name.Foreground = new SolidColorBrush(Colors.White);
        }


        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!playing)
                Opacity = 0.8;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (playing)
                Opacity = 1;
            else
                Opacity = 0.6;
        }

        private void LikeImg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (hSong == null) return;
            HC.WriteLine("Unlike/Like");
            MainWindow.likeSong(hSong, false);
        }
    }
}
