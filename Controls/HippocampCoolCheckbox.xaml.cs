using Re_Hippocamp.Misc;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Re_Hippocamp.Controls
{

    public partial class HippocampCoolCheckbox : UserControl
    {

        public delegate void CheckboxCoolPressedAction(HippocampCoolCheckbox hippocampCoolCheckbox);
        public event CheckboxCoolPressedAction CheckboxInt;

        public int speed = 0;

        public bool isCoolChecked
        {
            get { return (bool)GetValue(isCoolCheckedProperty); }
            set
            {
                SetValue(isCoolCheckedProperty, value); 
                if (!isCoolChecked) 
                { Check(); }
                else { UnCheck(); }
            }
        }

        // Using a DependencyProperty as the backing store for isChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty isCoolCheckedProperty =
            DependencyProperty.Register("isCoolChecked", typeof(bool), typeof(HippocampCoolCheckbox), new PropertyMetadata(false));

        public HippocampCoolCheckbox()
        {
            InitializeComponent();

            MainWindow.theme.applyThemeInGrid(this.Content as Grid);
        }

        public void Check()
        {
            Animations.MarginToMargin(EllipseCheck.Margin, EllipseMargin.Margin, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? speed : 10), EllipseCheck);
            Animations.ColorAnimationOBJ(MainWindow.theme.getColorFromPropertyName("activeColor"), Color.FromArgb(255, 35, 35, 35), new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? speed : 10), BG);
        }

        public void UnCheck()
        {
            HC.WriteLine("UNCHECK");
            Animations.MarginToMargin(EllipseCheck.Margin, EllipseOG.Margin, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? speed : 10), EllipseCheck);
            Animations.ColorAnimationOBJ(Color.FromArgb(255, 35, 35, 35), MainWindow.theme.getColorFromPropertyName("activeColor"), new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? speed : 10), BG);
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            speed = 200;
            isCoolChecked = !isCoolChecked;
            HC.WriteLine(isCoolChecked);
            try
            {
                CheckboxInt.Invoke(this);
            }
            catch (Exception) {}
        }
    }
}
