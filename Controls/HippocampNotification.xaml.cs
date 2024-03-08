using System.Windows;
using System.Windows.Controls;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampNotification : UserControl
    {


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HippocampNotification), new PropertyMetadata("Notification Text"));



        public HippocampNotification()
        {
            InitializeComponent();
        }
    }
}
