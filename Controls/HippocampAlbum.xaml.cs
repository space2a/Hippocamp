using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampAlbum : UserControl
    {
        public delegate void PlayAlbum(HippocampAlbum hippocampAlbum);
        public event PlayAlbum PlayAlbumClicked;
        public delegate void MeClicked(HippocampAlbum hippocampAlbum);
        public event MeClicked MeClickedOK;

        public HAlbum Me;

        public HippocampAlbum()
        {
            InitializeComponent();
        }

        public void INI(HAlbum hAlbum, int index)
        {
            Animations.Angle(loading, 0, 3600, new TimeSpan(0, 0, 0, 10));
            Me = hAlbum;
            AlbumArtist.Content = hAlbum.Artist;
            AlbumName.Text = hAlbum.Name;
            SongCount.Content = hAlbum.Songs.Count;
            PlayGrid.Opacity = 0;
            new Thread(() =>
            {
                Thread.Sleep(index * 120);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    AlbumCover1.Fill = new ImageBrush(BO.LoadImage(hAlbum.Cover, 125));
                    AlbumCover2.Fill = new ImageBrush(BO.LoadImage(hAlbum.Cover, 2));
                    BGEllipse.Fill = new ImageBrush(BO.LoadImage(hAlbum.Cover, 1));
                    BGEllipseBLUR.Stroke = BGEllipse.Fill;
                    M.Children.Remove(loading);
                }));
            }).Start();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MeClickedOK.Invoke(this);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Animations.ColorAnimationOBJ(Colors.Black, Colors.White, new TimeSpan(0,0,0, 0, !MainWindow.hippocampSettings.removeAnimations ? 200 : 10), BG);
            Animations.MarginToMargin(new Thickness(0, 20, 0, -20), new Thickness(0), new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 150 : 10), PlayGrid);
            Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 200 : 10), PlayGrid);
            //Animations.Scale(AlbumCover1, 1, 1.2, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10));
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Animations.ColorAnimationOBJ(Colors.White, Colors.Black, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 200 : 10), BG);
            Animations.MarginToMargin(new Thickness(0), new Thickness(0, 20, 0, -20), new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 200 : 10), PlayGrid);
            Animations.Opacity(0, 0.8, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 200 : 10), PlayGrid);
            //Animations.Scale(AlbumCover1, 1.2, 1, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10));
        }

        private void PlayGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PlayAlbumClicked.Invoke(this);
        }
    }
}
