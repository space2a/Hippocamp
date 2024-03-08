using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Re_Hippocamp.Windows
{
    public partial class LoadingScreen : Window
    {
        public LoadingScreen()
        {
            InitializeComponent();

            Loading.Loaded += Loading_Loaded;
            this.Closed += LoadingScreen_Closed;
            this.Loaded += LoadingScreen_Loaded;
        }

        private void LoadingScreen_Loaded(object sender, RoutedEventArgs e)
        {

        }


        private void LoadingScreen_Closed(object sender, EventArgs e)
        {
        }

        private void Loading_Loaded(object sender, RoutedEventArgs e)
        {
            Loading.Play();
        }

        public void closeWindow()
        {
            Loading.Stop();
            Loading.Source = null;
            Loading.Play();
            Loading.Close();

            Avatar.Fill = null;

            Loaded -= Loading_Loaded;
            Loaded -= Loading_MediaEnded;
            Loading.Loaded -= Loading_Loaded;

            Loading = null;

            this.Closed -= LoadingScreen_Closed;
            this.Loaded -= LoadingScreen_Loaded;

            (this.Content as Grid).Children.Clear();
            this.Content = null;
            this.Close();
        }

        public void INI(UserProfile userProfile, Settings settings)
        {
            if (settings.removeLoadingAnimation)
                Loading.Source = null;

            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = BO.LoadImage(userProfile.Avatar, 100);
            imageBrush.Stretch = Stretch.Uniform;
            Avatar.Fill = imageBrush;
        }

        private void Loading_MediaEnded(object sender, RoutedEventArgs e)
        {
           Loading.Position = new TimeSpan(0, 0, 0, 0, 1);
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
