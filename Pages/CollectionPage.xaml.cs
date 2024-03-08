using Re_Hippocamp.Controls;
using Re_Hippocamp.Misc;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Re_Hippocamp.Pages
{
    public partial class CollectionPage : UserControl
    {
        public Settings hippocampSettings;

        private bool areAlbumsLoaded = false;
        private bool areArtistsLoaded = false;
        private bool areMixsLoaded = false;

        private MainWindow mW;

        public static bool animAlbum = false;
        public CollectionPage(MainWindow mainWindow)
        {
            InitializeComponent();
            hippocampSettings = MainWindow.hippocampSettings;
            mW = mainWindow;

            ArtistsCB.CheckboxInt += ArtistsCB_CheckboxInt;
            ArtistsCB.isChecked = ArtistsCB.isChecked; //refresh
            MixsCB.isChecked = MixsCB.isChecked; //refresh
            AlbumsCB.CheckboxInt += AlbumsCB_CheckboxInt;
            MixsCB.CheckboxInt += MixsCB_CheckboxInt;

            AlbumsCB_CheckboxInt(AlbumsCB);

            if (mW.discoveredAlbums.Count == 0) //is empty
            {
                CollectionPageMGrid.Visibility = Visibility.Hidden;
                CollectionPageGridMsg.Visibility = Visibility.Visible;
            }
            else
            {
                CollectionPageMGrid.Visibility = Visibility.Visible;
                CollectionPageGridMsg.Visibility = Visibility.Hidden;
                //addAlbumsToAlbumPage(discoveredAlbums);
                //addArtistsToArtistPage(discoveredArtists);
            }
            MainWindow.theme.applyThemeInGrid(this.Content as Grid);
            this.IsVisibleChanged += CollectionPage_IsVisibleChanged1;
        }

        bool doneAn = false;
        private void CollectionPage_IsVisibleChanged1(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (doneAn) return;
            doneAn = true;

            if (e.NewValue.ToString() == "True") return;

            ArtistsCB.CheckboxInt -= ArtistsCB_CheckboxInt;
            AlbumsCB.CheckboxInt -= AlbumsCB_CheckboxInt;
            MixsCB.CheckboxInt -= MixsCB_CheckboxInt;

            WPMixs.Children.Clear();
            WPArtists.Children.Clear();
            WPAlbums.Children.Clear();

            (this.Content as Grid).Children.Clear();
            this.Content = null;
        }

        private bool isSearchCollectionOpen = false;
        private void OpenSearchCollection_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            return;
            if (isSearchCollectionOpen)
                closeSearchCollection();
            else
            {
                //Open collection
                isSearchCollectionOpen = true;
                Animations.Width(SearchCollectionGrid.Width, 200, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 200 : 10), SearchCollectionGrid);
                Animations.Opacity(0.5, BGSearchCollectionGrid.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 200 : 10), BGSearchCollectionGrid);
            }
        }

        private void closeSearchCollection(bool eraseContent = false)
        {
            return;
            SearchBoxCollection.Text = "";
            Animations.Width(SearchCollectionGrid.Width, 40, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 200 : 10), SearchCollectionGrid);
            Animations.Opacity(0.3, BGSearchCollectionGrid.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 200 : 10), BGSearchCollectionGrid);
            isSearchCollectionOpen = false;
            if (eraseContent)
                SearchBoxCollection.Text = "";
        }

        private void SearchCollectionTextChanged_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            WrapPanel wrapPanel = null;
            if (text == "")
            {
                foreach (UIElement c in WPAlbums.Children)
                    c.Visibility = Visibility.Visible;
                foreach (UIElement c in WPMixs.Children)
                    c.Visibility = Visibility.Visible;
                foreach (UIElement c in WPArtists.Children)
                    c.Visibility = Visibility.Visible;
            }
            else
            {
                if (WPAlbums.Opacity == 1 || WPMixs.Opacity == 1)
                {
                    if (WPMixs.Opacity == 1) wrapPanel = WPMixs;
                    else if (WPAlbums.Opacity == 1) wrapPanel = WPAlbums;
                    if (wrapPanel == null) { HC.WriteLine("wrapPanel NULL", ConsoleColor.DarkRed); (sender as TextBox).Text = ""; return; }
                    foreach (UIElement c in wrapPanel.Children)
                    {
                        HippocampAlbum hippocampAlbum = c as HippocampAlbum;
                        if (hippocampAlbum != null)
                        {
                            if (hippocampAlbum.AlbumName.Text.ToLower().IndexOf(text.ToLower()) == -1) c.Visibility = Visibility.Collapsed; else c.Visibility = Visibility.Visible;

                            if (hippocampAlbum.AlbumArtist.Content.ToString().ToLower().IndexOf(text.ToLower()) == -1 && c.Visibility == Visibility.Collapsed) c.Visibility = Visibility.Collapsed; else c.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (WPArtists.Opacity == 1)
                {
                    foreach (UIElement c in WPArtists.Children)
                    {
                        HippocampArtist hippocampArtist = c as HippocampArtist;
                        if (hippocampArtist != null)
                            if (hippocampArtist.Me.Name.ToLower().IndexOf(text.ToLower()) == -1) c.Visibility = Visibility.Collapsed; else c.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void SearchCollection_LostFocus(object sender, RoutedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            if (text == "")
                closeSearchCollection();
        }

        private void checkHC(HippocampCheckbox hippocampCheckbox)
        {
            foreach (var item in SPControlCollection.Children)
                if (item != hippocampCheckbox)
                    (item as HippocampCheckbox).isChecked = false;

            hippocampCheckbox.isChecked = true;
        }

        private void CollectionPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (CollectionPage.Visibility == Visibility.Visible)
            //    if (!areAlbumsLoaded) addAlbumsToAlbumPage(discoveredAlbums);
        }

        private void hideAllWPCollection(WrapPanel you)
        {
            SearchBoxCollection.Text = "";
            foreach (WrapPanel item in CollectionWPs.Children)
                if (item != you)
                    Animations.Opacity(0, item.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), item, true);

            Animations.Opacity(1, you.Opacity, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), you);
        }

        private void AlbumsCB_CheckboxInt(HippocampCheckbox hippocampCheckbox)
        {
            checkHC(hippocampCheckbox);
            hideAllWPCollection(WPAlbums);
            closeSearchCollection();

            if (!areAlbumsLoaded) addAlbumsToAlbumPage(mW.discoveredAlbums);
        }

        private void ArtistsCB_CheckboxInt(HippocampCheckbox hippocampCheckbox)
        {
            checkHC(hippocampCheckbox);
            hideAllWPCollection(WPArtists);
            closeSearchCollection();

            if (!areArtistsLoaded) addArtistsToArtistPage(mW.discoveredArtists);
        }

        private void addAlbumsToAlbumPage(List<HAlbum> discoveredAlbums, WrapPanel WPAlbum = null)
        {
            if (WPAlbum == null)
            {
                WPAlbum = WPAlbums;
                areAlbumsLoaded = true;
            }
            else areMixsLoaded = true;

            WPAlbum.Children.Clear();
            int an = 10;
            new Thread(() =>
            {

                for (int i = 0; i < discoveredAlbums.Count; i++)
                {
                    var dA = discoveredAlbums[i];
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        HippocampAlbum hippocampAlbum = new HippocampAlbum() { Margin = new Thickness(10), Opacity = 0 };
                        hippocampAlbum.INI(dA, i + 1);
                        hippocampAlbum.MeClickedOK += HippocampAlbum_MouseLeftButtonUp;
                        hippocampAlbum.PlayAlbumClicked += HippocampAlbum_PlayAlbumClicked;
                        WPAlbum.Children.Add(hippocampAlbum);
                        Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), hippocampAlbum);
                    }));
                    if (an > 0 && an-- > 0)
                        if (!animAlbum)
                            Thread.Sleep(10 + (an * 10));
                    else
                        if (!animAlbum)
                            Thread.Sleep(10);
                }


                //discoveredAlbums.Clear();
                //discoveredAlbums = null;
                HC.WriteLine("Thread addAlbumsToAlbumPage ended");
                animAlbum = true;
            }).Start();
        }


        private void HippocampAlbum_PlayAlbumClicked(HippocampAlbum hippocampAlbum)
        {
            var hSong = hippocampAlbum.Me.Songs[0];
            mW.updatePlayingSong(hSong, hippocampAlbum.Me);
        }

        private void HippocampAlbum_MouseLeftButtonUp(HippocampAlbum hippocampAlbum)
        {
            mW.openAlbumView(hippocampAlbum.Me);
        }

        private void addArtistsToArtistPage(List<HArtist> discoveredArtists)
        {
            WPArtists.Children.Clear();
            areArtistsLoaded = true;
            int an = 10;
            new Thread(() =>
            {

                int i = 0;
                foreach (var dA in discoveredArtists)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        HippocampArtist hippocampArtist = new HippocampArtist() { Margin = new Thickness(10) };
                        hippocampArtist.INI(dA, i++);
                        WPArtists.Children.Add(hippocampArtist);
                        Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !hippocampSettings.removeAnimations ? 250 : 10), hippocampArtist);
                    }));
                    if (an > 0 && an-- > 0)
                        if (!animAlbum)
                            Thread.Sleep(10 + (an * 10));
                        else
                        if (!animAlbum)
                            Thread.Sleep(10);
                }


                //discoveredArtists.Clear();
                //discoveredArtists = null;
                HC.WriteLine("Thread addArtistsToArtistPage ended");
            }).Start();

        }

        private void MixsCB_CheckboxInt(HippocampCheckbox hippocampCheckbox)
        {
            checkHC(hippocampCheckbox);
            hideAllWPCollection(WPMixs);
            closeSearchCollection();

            if (!areMixsLoaded) addAlbumsToAlbumPage(mW.mixsCreated, WPMixs);
        }

    }
}
