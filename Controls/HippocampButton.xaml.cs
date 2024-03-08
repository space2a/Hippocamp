using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampButton : UserControl
    {


        public bool Bin
        {
            get { return (bool)GetValue(BinProperty); }
            set 
            { 
                SetValue(BinProperty, value);
                if (Bin)
                {
                    Rc.Fill = new SolidColorBrush(Color.FromArgb(255, 199, 20, 20));
                    RcS.Stroke = new SolidColorBrush(Color.FromArgb(255, 199, 20, 20));
                    BinRec.Fill = new SolidColorBrush(Color.FromArgb(255, 199, 20, 20));
                    BinRec.Visibility = Visibility.Visible;
                }
            }
        }

        public static readonly DependencyProperty BinProperty =
            DependencyProperty.Register("Bin", typeof(bool), typeof(HippocampDiscreteTextbox), new PropertyMetadata(false));



        public delegate void ButtonPressedAction(HippocampButton hippocampButton);
        public event ButtonPressedAction ButtonPressed;

        public bool IsMain
        {
            get { return (bool)GetValue(IsMainProperty); }
            set { SetValue(IsMainProperty, value); checkIsMain(); }
        }

        public static readonly DependencyProperty IsMainProperty =
            DependencyProperty.Register("IsMain", typeof(bool), typeof(HippocampButton), new PropertyMetadata(true));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HippocampButton), new PropertyMetadata("Button"));



        public HippocampButton()
        {
            InitializeComponent();
            Rc.Opacity = 0.01;
            MainWindow.theme.applyThemeInGrid(this.Content as Grid);
        }

        private SolidColorBrush activeColor = new SolidColorBrush(MainWindow.theme.getColorFromPropertyName("activeColor"));

        public void checkIsMain()
        {
            if (IsMain)
            {
                Rc.Fill = activeColor;
                RcS.Stroke = activeColor;
            }
            else
            {
                Rc.Fill = new SolidColorBrush(Color.FromArgb(255, 33, 33, 33));
                RcS.Stroke = new SolidColorBrush(Color.FromArgb(255, 33, 33, 33));
            }
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            Animations.ColorAnimationOBJ(Color.FromArgb(255, 199, 20, 20), Color.FromArgb(255,255,255,255), new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), BinRec);
            Animations.Opacity(1, Rc.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), Rc);
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            Animations.ColorAnimationOBJ(Color.FromArgb(255, 255,255,255), Color.FromArgb(255, 199, 20, 20), new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), BinRec);
            Animations.Opacity(0.01, Rc.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), Rc);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ButtonPressed.Invoke(this);
        }
    }
}
