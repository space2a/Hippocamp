using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampDiscreteTextbox : UserControl
    {

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(HippocampDiscreteTextbox), new PropertyMetadata("Title"));

        public int CharMax
        {
            get { return (int)GetValue(CharMaxProperty); }
            set { SetValue(CharMaxProperty, value); }
        }

        public static readonly DependencyProperty CharMaxProperty =
            DependencyProperty.Register("CharMax", typeof(int), typeof(HippocampDiscreteTextbox), new PropertyMetadata(-1));


        public string PlaceHolder
        {
            get { return (string)GetValue(PlaceHolderProperty); }
            set { SetValue(PlaceHolderProperty, value); }
        }

        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.Register("PlaceHolder", typeof(string), typeof(HippocampDiscreteTextbox), new PropertyMetadata("Placeholder text"));



        public HippocampDiscreteTextbox()
        {
            InitializeComponent();
            new Thread(() =>
            {
                Thread.Sleep(200);
                Application.Current.Dispatcher.Invoke(new Action(() => 
                {
                    TB.Text = PlaceHolder;
                    TB.FontStyle = FontStyles.Italic;
                    TB.Foreground = new SolidColorBrush(Color.FromArgb(255, 180, 180, 180));
                }));
            }).Start();
        }

        public string getText()
        {
            if (TB.Text == PlaceHolder) return "";
            return TB.Text;
        }

        public void setText(string t)
        {
            TB.Text = t;
            if (t != "")
            {
                TB.FontStyle = FontStyles.Normal;
                TB.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
            else
            {
                TextBox_LostFocus(TB, null);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if((sender as TextBox).Text == "")
            {
                (sender as TextBox).Text = PlaceHolder;
                (sender as TextBox).FontStyle = FontStyles.Italic;
                (sender as TextBox).Foreground = new SolidColorBrush(Color.FromArgb(255, 180, 180, 180));
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text == PlaceHolder)
            {
                (sender as TextBox).Text = "";
                (sender as TextBox).FontStyle = FontStyles.Normal;
                (sender as TextBox).Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
        }
    }
}
