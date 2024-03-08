using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampArtist : UserControl
    {
        public HArtist Me;

        public HippocampArtist()
        {
            InitializeComponent();
        }

        public void INI(HArtist hArtist, int index)
        {
            Animations.Angle(loading, 0, 3600, new TimeSpan(0, 0, 0, 10));
            Me = hArtist;
            ArtistName.Text = hArtist.Name;

            int songs = 0;
            bool ac = false;

            SongCount.Content = songs;
            AlbumCount.Content = hArtist.hAlbums.Count;

            new Thread(() =>
            {
                Thread.Sleep(index * 120);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {

                    for (int i = 0; i < hArtist.hAlbums.Count; i++)
                    {
                        if (!ac && hArtist.hAlbums[i].Cover != null)
                        {
                            AlbumCover1.Fill = new ImageBrush(BO.LoadImage(hArtist.hAlbums[i].Cover, 125));
                            AlbumCover2.Fill = new ImageBrush(BO.LoadImage(hArtist.hAlbums[i].Cover, 2));
                            (this.Content as Grid).Children.Remove(loading);
                        }

                        songs += hArtist.hAlbums[i].Songs.Count;
                    }
                }));
            }).Start();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Animations.ColorAnimationOBJ(Colors.Black, Colors.White, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 150 : 10), BG);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Animations.ColorAnimationOBJ(Colors.White, Colors.Black, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 150 : 10), BG);
        }

    }
}
