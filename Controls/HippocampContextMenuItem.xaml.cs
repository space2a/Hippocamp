using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Re_Hippocamp.Controls
{

    public partial class HippocampContextMenuItem : UserControl
    {

        public HippocampContextMenu HippocampContextMenu;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HippocampContextMenuItem), new PropertyMetadata(""));

        public delegate void Clicked(HippocampContextMenuItem hippocampContextMenuItem);
        public event Clicked ClickedItem;

        public HippocampContextMenuItem(BitmapSource bitmapSource)
        {
            InitializeComponent();

            if(bitmapSource != null)
                Icon.Source = bitmapSource;

            SIArrow.Visibility = Visibility.Collapsed;
        }


        public void changeTextAndBitmap(string t, BitmapSource bitmapSource)
        {
            Text = t;
            new Thread(() =>
            {
                //Application.Current.Dispatcher.Invoke(new Action(() => { Animations.BlurRadius(10, 0, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 100 : 10), Changes); }));
                //Thread.Sleep(!MainWindow.hippocampSettings.removeAnimations ? 120 : 10);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (bitmapSource != null)
                        Icon.Source = bitmapSource;
                    Text = t;
                    //Animations.BlurRadius(0, 10, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 100 : 10), Changes);
                }));
            }).Start();
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            T.TextDecorations = null;
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            T.TextDecorations = TextDecorations.Underline;
        }

        public void deploySubItem(HippocampContextMenuItem hippocampContextMenuItem)
        {
            SubItems.Visibility = Visibility.Hidden; //no longer collapsed.
            SIArrow.Visibility = Visibility.Visible;
            hippocampContextMenuItem.HippocampContextMenu = HippocampContextMenu;
            ItemsSP.Children.Add(hippocampContextMenuItem);
        }

        private void root_MouseEnter(object sender, MouseEventArgs e)
        {
            if (SubItems.Visibility != Visibility.Collapsed) Animations.Opacity(1, SubItems.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), SubItems);
        }

        private void root_MouseLeave(object sender, MouseEventArgs e)
        {
            if (SubItems.Visibility != Visibility.Collapsed) Animations.Opacity(0, SubItems.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), SubItems);
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClickedItem.Invoke(this);
            if(HippocampContextMenu != null)
                HippocampContextMenu.IsOpen = false;
        }
    }
}
