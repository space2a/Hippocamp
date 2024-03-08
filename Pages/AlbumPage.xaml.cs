using Re_Hippocamp.Controls;
using Re_Hippocamp.Misc;

using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Re_Hippocamp.Pages
{
    public partial class AlbumPage : UserControl
    {
        MainWindow mw;
        int playlistIndex; 
        HPlaylist playlist;
        public AlbumPage(MainWindow mainWindow)
        {
            InitializeComponent();
            mw = mainWindow;

            mw.hideSideBarFilledImages();
            this.IsVisibleChanged += AlbumPage_IsVisibleChanged;
        }

        bool de = false;
        private void AlbumPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.ToString() == "False")
            {
                if (de) return;
                de = true;

                closeAlbumView();
            }
        }

        private bool opAlbumView = false;
        private HAlbum me;
        public void openAlbumView(HAlbum hAlbum, bool playlist, int playlistIndex = -1, HPlaylist hPlaylist = null)
        {
            this.playlist = hPlaylist;
            this.playlistIndex = playlistIndex;
            if (opAlbumView) return;
            opAlbumView = true;
            int i = 1;
            me = hAlbum;
            AlbumBackground.Background = new ImageBrush(BO.LoadImage(hAlbum.Cover, 2));
            Animations.Opacity(0, mw.AppBackground.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 1250 : 10), mw.AppBackground);
            //mw.AlbumSideBar.Background = AlbumBackground.Background;

            AlbumCover.Background = new ImageBrush(BO.LoadImage(hAlbum.Cover, 150));
            AlbumCoverFAKE.Background = AlbumBackground.Background;

            AlbumName.Text = hAlbum.Name;
            AlbumArtist.Content = hAlbum.Artist;

            //mw.HideSideBarFilledImages(mw.FakeSideBarGrid);

            //updateSongUI();

            if (playlist)
            {
                AlbumName.Cursor = Cursors.Hand;

                AlbumCover.MouseEnter += delegate (object sender, MouseEventArgs e)
                {
                    EditCover.Visibility = Visibility.Visible;
                };
                EditCover.MouseLeave += delegate (object sender, MouseEventArgs e)
                {
                    EditCover.Visibility = Visibility.Hidden;
                };

                PLSelection.OpacityMask = AlbumCover.Background;

                AlbumName.PreviewMouseLeftButtonDown += AlbumName_PreviewMouseLeftButtonDown;
                AlbumName.LostFocus += AlbumName_LostFocus;
                AlbumName.TextChanged += AlbumName_TextChanged;
            }
            else
            {
                MGRID.Children.Remove(PlaylistGrid);
                PlaylistGrid.Children.Clear();
                PlaylistGrid.Visibility = Visibility.Collapsed;
            }


            mw.TabRectangle.Margin = new Thickness(mw.TabRectangle.Margin.Left, mw.TabRectangle.Margin.Top, mw.TabRectangle.Margin.Right, mw.TabRectangle.Margin.Bottom + 1000);
            Animations.Opacity(0, mw.TabRectangle.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), mw.TabRectangle);

            new Thread(() =>
            {

                double totalDuration = 0;
                foreach (var song in hAlbum.Songs)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => 
                    {
                        HippocampSong hippocampSong = new HippocampSong(mw, MainWindow.GetHPlaylists()) { Margin = new Thickness(0, 5, 0, 5) };
                        AlbumViewSV.Focus();

                        //hippocampSong.ContextMenu = new HippocampContextMenu();
                        if (i == 1)
                            hippocampSong.Margin = new Thickness(0, 15, 0, 5);
                        hippocampSong.IniSong(song, hAlbum, i++);
                        hippocampSong.MouseDoubleClick += HippocampSong_MouseDoubleClick;
                        hippocampSong.MouseRightButtonUp += HippocampSong_MouseRightButtonUp;

                        AlbumViewSP.Children.Add(hippocampSong);

                        totalDuration += song.Duration;
                    }));

                    Thread.Sleep(20);
                }

                TimeSpan tp = new TimeSpan(0, 0, 0, (int)totalDuration);
                string so = "songs";
                if (hAlbum.Songs.Count == 1) so = "song";

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (tp.Hours != 0)
                        AlbumInfo.Content = hAlbum.Songs.Count + " " + so + ", " + tp.Hours + "h " + tp.Minutes + " min";
                    else
                        AlbumInfo.Content = hAlbum.Songs.Count + " " + so + ", " + tp.Minutes + " Min " + tp.Seconds.ToString("00") + "s";
                }));

            }).Start();

        }

        private void AlbumName_TextChanged(object sender, TextChangedEventArgs e)
        {
            savePlaylist();
        }

        private void AlbumName_LostFocus(object sender, RoutedEventArgs e)
        {
            if(oldName != AlbumName.Text) { mw.LoadSideBarPlaylists(); mw.DeployNotification("Playlist settings saved"); }
            AlbumName.IsReadOnly = true;
            AlbumName.Background = new SolidColorBrush(Colors.Transparent);
        }
        string oldName;
        private void AlbumName_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            oldName = AlbumName.Text;
            AlbumName.IsReadOnly = false;
            AlbumName.Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
            AlbumName.SelectionBrush = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        }

        public void bringToFrontHSong(HSong hsong)
        {
            return; //not working.

            foreach (var item in AlbumViewSP.Children)
            {
                var hps = item as HippocampSong;
                if (hps != null)
                {
                    if(hps.hSong == hsong)
                    {
                        GeneralTransform groupBoxTransform = (item as FrameworkElement).TransformToAncestor(AlbumViewSP);
                        Rect rectangle = groupBoxTransform.TransformBounds(new Rect(new Point(0, 0), (item as FrameworkElement).RenderSize));
                        AlbumViewSV.ScrollToVerticalOffset(rectangle.Top + AlbumViewSV.VerticalOffset);
                        HC.WriteLine(hps.hSong.Name + " bringing to view");
                        return;
                    }
                }
            }
        }

        private void closeAlbumView()
        {
            HC.WriteLine("close album view");
            //Animations.Opacity(0.8, SideBar.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), SideBar);
            //Animations.Opacity(0.97, AppBackground.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), AppBackground);
            //
            Animations.Opacity(1, mw.TabRectangle.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), mw.TabRectangle);
            Animations.Opacity(0.97, mw.AppBackground.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), mw.AppBackground);
            //
            //Animations.Opacity(0, AlbumPage.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 350 : 10), AlbumPage, true);
            //Animations.Opacity(0, AlbumSideBar.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 350 : 10), AlbumSideBar);
            AlbumViewSP.Children.Clear();
        }


        public void checkIsPlaying(HSong hSong, HAlbum PlayingAlbum, byte[] pData)
        {
            var ha = mw.getHAlbumFromHSong(hSong);

            foreach (var item in AlbumViewSP.Children)
            {
                var hps = item as HippocampSong;
                if (hps != null)
                {
                    if (ha == me && hps.hSong.Path == hSong.Path)
                        hps.songPlaying(pData);
                    else
                        hps.songNotPlaying();
                }
            }
        }

        private void HippocampSong_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var hSong = (sender as HippocampSong);
            mw.updatePlayingSong(hSong.hSong, hSong.hAlbum);
        }


        private void HippocampSong_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //HContextMenu.Show();
        }

        private void EditCover_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png,*.jpg,*.jpeg)|*.png;*.jpg;*.jpeg;";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AlbumCover.Background = new ImageBrush(BO.LoadImage(File.ReadAllBytes(openFileDialog.FileName), 150));
                AlbumBackground.Background = new ImageBrush(BO.LoadImage(File.ReadAllBytes(openFileDialog.FileName), 2));
                AlbumCoverFAKE.Background = AlbumBackground.Background;
                PLSelection.OpacityMask = AlbumCover.Background;
                savePlaylist();
                mw.DeployNotification("Playlist settings saved");
            }
        }

        private void savePlaylist()
        {
            byte[] cover = BO.ImageSourceToBytes(new PngBitmapEncoder(), (AlbumCover.Background as ImageBrush).ImageSource);
            string name = AlbumName.Text;

            playlist.Name = name;
            playlist.Cover = cover;

            MainWindow.OverwritePlaylist(playlist, playlistIndex);
        }

    }
}
