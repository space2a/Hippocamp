using Re_Hippocamp.Controls;
using Re_Hippocamp.Misc;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Re_Hippocamp.Pages
{
    public partial class HomePage : UserControl
    {
        MainWindow mw;
        public HomePage(MainWindow mw)
        {
            InitializeComponent();

            this.SizeChanged += HomePage_SizeChanged;
            this.mw = mw;
            oldH = 650;
            Ini();

            ExploreCollection.ButtonPressed += ExploreCollection_ButtonPressed;
            MainWindow.theme.applyThemeInGrid(this.Content as Grid);
        }

        private void ExploreCollection_ButtonPressed(HippocampButton hippocampButton)
        {
            mw.SideBarGrid_MouseLeftButtonDown(mw.Library, null);
        }

        int oldH = 0;
        int topA = 5;
        private void HomePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int h = (int)mw.Height;
            int x = h - oldH;

            if (x >= 45)
            {
                if (topA == 10) return;
                oldH = h;
                topA++;
                Ini(topA);
            }
            else if (x <= -40)
            {
                if (topA == 4) return;
                oldH = h;
                topA--;
                Ini(topA);
            }
        }

        bool artists = false;
        public void Ini(int top = 5)
        {
            DateTime start = DateTime.Now;
            TopSongs.Children.Clear();
            TopText.Content = "Top " + top + " played songs";

            var exp = MainWindow.GetUserExperienceFromProfile(MainWindow.loadedProfile);

            List<SongPlay> songPlays = MainWindow.getTopPlayCount(MainWindow.loadedProfile, top);
            List<HSong> hsong = new List<HSong>();
            List<byte[]> covers = new List<byte[]>();

            foreach (var dA in mw.discoveredAlbums)
            {
                foreach (var s in songPlays)
                {
                    int i = dA.Songs.FindIndex(x => x.Path == s.Path);
                    if (i != -1)
                    {
                        hsong.Add(dA.Songs[i]);
                        covers.Add(dA.Cover);
                    }
                }
            }

            if (songPlays.Count != hsong.Count)
            {
                HC.WriteLine("songPlays.Count IS NOT EQUALS TO hsong.Count", ConsoleColor.DarkRed);
                HC.WriteLine(songPlays.Count + " != " + hsong.Count, ConsoleColor.DarkRed);
                return;
            }

            int xi = 0;
            foreach (var s in songPlays)
            {
                try
                {
                    var song = hsong.Find(x => x.Path == s.Path);
                    int songI = hsong.FindIndex(x => x.Path == s.Path);
                    HippocampHomeSong hippocampHomeSong = new HippocampHomeSong() { Margin = new Thickness(0, 3, 0, 3) };
                    hippocampHomeSong.IniSong(song, covers[songI], xi + 1, mw);
                    TopSongs.Children.Add(hippocampHomeSong);
                }
                catch (Exception ex)
                {
                    HC.WriteLine(ex.ToString(), ConsoleColor.White, ConsoleColor.DarkRed);
                }
                xi++;
            }
            HC.WriteLine("top songs: " + (DateTime.Now - start).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Magenta);
            DateTime startA = DateTime.Now;
            if (!artists)
            {
                artists = true;
                List<HAlbum> ats = new List<HAlbum>();



                foreach (var lpa in exp.lastPlayedArtists)
                {
                    HAlbum ha = mw.discoveredAlbums.Find(x => x.Name + ":" + x.Artist == lpa);
                    if(ha != null) { ats.Add(ha); continue; }
                }
                HC.WriteLine("albums found: " + (DateTime.Now - startA).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Magenta);
                xi = 0;
                foreach (var s in exp.lastPlayedArtists)
                {
                    try
                    {
                        var artist = ats.Find(x => s == x.Name + ":" + x.Artist);
                        if (artist == null) { HC.WriteLine(s + " null", ConsoleColor.DarkRed); continue; }
                        HippocampHomeArtist hippocampHomeArtist = new HippocampHomeArtist();
                        hippocampHomeArtist.INI(artist, mw, xi + 1);
                        RecentArtists.Children.Add(hippocampHomeArtist);
                    }
                    catch (Exception ex)
                    {
                        HC.WriteLine(ex.ToString(), ConsoleColor.White, ConsoleColor.DarkRed);
                    }
                    xi++;
                }

                if(RecentArtists.Children.Count == 0 && TopSongs.Children.Count == 0)
                {
                    Normal.Visibility = Visibility.Collapsed;
                    Welcome.Visibility = Visibility.Visible;
                }else
                {
                    Welcome.Visibility = Visibility.Collapsed;
                    Normal.Visibility = Visibility.Visible;
                }
            }
            HC.WriteLine("end albums : " + (DateTime.Now - startA).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Magenta);
            HC.WriteLine("end: " + (DateTime.Now - start).TotalSeconds + "secs", ConsoleColor.Black, ConsoleColor.Magenta);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
                scrollViewer.LineLeft();
            }
            e.Handled = true;
        }
    }
}
