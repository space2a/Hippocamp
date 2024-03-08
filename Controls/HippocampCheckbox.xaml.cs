using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampCheckbox : UserControl
    {
        public delegate void CheckboxPressedAction(HippocampCheckbox hippocampCheckbox);
        public event CheckboxPressedAction CheckboxInt;


        public bool isChecked
        {
            get { return (bool)GetValue(isCheckedProperty); }
            set { SetValue(isCheckedProperty, value); if (isChecked) { Animations.Opacity(0.3, Rc.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), Rc); } 
                else { Animations.Opacity(0.01, Rc.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), Rc); } }
        }

        // Using a DependencyProperty as the backing store for isChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty isCheckedProperty =
            DependencyProperty.Register("isChecked", typeof(bool), typeof(HippocampCheckbox), new PropertyMetadata(false));




        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HippocampCheckbox), new PropertyMetadata("Button"));

        public HippocampCheckbox()
        {
            InitializeComponent();
            MainWindow.theme.applyThemeInGrid(this.Content as Grid);
        }

        private void root_MouseEnter(object sender, MouseEventArgs e)
        {
            Animations.Opacity(0.3, Rc.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), Rc);
        }

        private void root_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isChecked) return;
            Animations.Opacity(0.01, Rc.Opacity, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), Rc);
        }

        private void root_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CheckboxInt.Invoke(this);
        }
    }
}
