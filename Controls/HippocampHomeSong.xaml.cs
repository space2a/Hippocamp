using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampHomeSong : UserControl
    {
        HSong HSong;
        MainWindow mW;

        public HippocampHomeSong()
        {
            InitializeComponent();
            MainWindow.theme.applyThemeInGrid(this.Content as Grid);
        }

        public void IniSong(HSong sd, byte[] cover, int index, MainWindow mw)
        {
            HSong = sd;
            mW = mw;
            Artist.Content = "";

            Index.Content = index;
            if (sd.Name != "")
                Name.Content = sd.Name;
            if (sd.Artist != null)
                Artist.Content = sd.Artist.Replace("; ", ", ").Replace(";", ", ");

            TimeSpan tp = new TimeSpan(0, 0, 0, (int)sd.Duration);
            Length.Content = tp.Minutes + ":" + tp.Seconds.ToString("00");
            Cover.Fill = new ImageBrush(BO.LoadImage(cover, 100));
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var ha = mW.getHAlbumFromHSong(HSong);
            if (ha != null)
                mW.openAlbumView(ha, HSong);
        }
    }
}
