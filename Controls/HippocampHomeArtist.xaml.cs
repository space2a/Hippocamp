using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampHomeArtist : UserControl
    {
        MainWindow mainWindow;
        HAlbum album;
        public HippocampHomeArtist()
        {
            InitializeComponent();

            MainWindow.theme.applyThemeInGrid(this.Content as Grid);
            this.MouseLeftButtonUp += HippocampHomeArtist_MouseLeftButtonUp;
        }

        private void HippocampHomeArtist_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mainWindow.openAlbumView(album);
        }

        public void INI(HAlbum hArtist, MainWindow mainWindow, int index)
        {
            if (hArtist == null) { Console.WriteLine("NULLLLL"); this.Visibility = Visibility.Collapsed; return; }
            Animations.Angle(loading, 0, 360, new TimeSpan(0, 0, 0, 2));

            album = hArtist;
            this.mainWindow = mainWindow;
            ArtistName.Text = "";
            if(hArtist.Name != null)
                ArtistName.Text = hArtist.Name;

            new Thread(() =>
            {
                Thread.Sleep(index * 120);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    AlbumCover1.Fill = new ImageBrush(BO.LoadImage(hArtist.Cover, 200));
                    (this.Content as Grid).Children.Remove(loading);
                }));
            }).Start();
        }
    }
}
