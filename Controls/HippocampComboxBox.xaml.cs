using Re_Hippocamp.Misc;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampComboxBox : UserControl
    {
        public List<HippocampComboxBoxItem> Items = new List<HippocampComboxBoxItem>();

        public HippocampComboxBoxItem selectedItem;

        public delegate void SelectionChanged(HippocampComboxBoxItem newItem);
        public event SelectionChanged SelectionItemChanged;

        public bool isOpen
        {
            get { return (bool)GetValue(isOpenProperty); }
            set { SetValue(isOpenProperty, value); }
        }

        public static readonly DependencyProperty isOpenProperty =
            DependencyProperty.Register("isOpen", typeof(bool), typeof(HippocampComboxBox), new PropertyMetadata(false));

        public HippocampComboxBox()
        {
            InitializeComponent();

            SP.Height = 0;
            MainWindow.theme.applyThemeInGrid(this.Content as Grid);
        }

        private void BG_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseOrOpen();
        }

        public void CloseOrOpen()
        {
            if (isOpen)
            {
                //Animations.(BG.CornerRadius, new Thickness(5, 5, 5, 0), new TimeSpan(0, 0, 0, 0, (!MainWindow.hippocampSettings.removeAnimations ? 250 : 10)), BG);
                //Animations.MarginToMargin(BG.CornerRadius, new Thickness(5, 5, 0, 5), new TimeSpan(0, 0, 0, 0, (!MainWindow.hippocampSettings.removeAnimations ? 250 : 10)), BG);

                Animations.Height(SP.Height, 0, new TimeSpan(0, 0, 0, 0, (!MainWindow.hippocampSettings.removeAnimations ? 250 : 10)), SP);
            }
            else
            {
                //Animations.MarginToMargin(new Thickness(5), new Thickness(5, 5, 5, 0), new TimeSpan(0, 0, 0, 0, (!MainWindow.hippocampSettings.removeAnimations ? 250 : 10)), BG);
                Animations.Height(SP.Height, 30 * SP.Children.Count, new TimeSpan(0, 0, 0, 0, (!MainWindow.hippocampSettings.removeAnimations ? 250 : 10)), SP);
            }

            isOpen = !isOpen;
        }

        public void addItem(HippocampComboxBoxItem hippocampComboxBoxItem)
        {
            Items.Add(hippocampComboxBoxItem);
            HC.WriteLine("new object added");
            addVisual(hippocampComboxBoxItem.text);
            HC.WriteLine("new object created");
        }

        private void addVisual(string text)
        {
            HC.WriteLine(text + "creating");
            Grid grid = new Grid() { Tag = Items.Count -1 , Height = 30 };

            Border border = new Border() { Margin = new Thickness(1), Background = new SolidColorBrush(Color.FromArgb(100, 0,0,0)), CornerRadius = new CornerRadius(3)};
            grid.MouseLeftButtonUp += Border_MouseLeftButtonUp;

            Label label = new Label() { Foreground = new SolidColorBrush(Colors.White), Content = text, VerticalContentAlignment = VerticalAlignment.Center, Style = Selected.Style };

            grid.Children.Add(border);
            grid.Children.Add(label);
            SP.Children.Add(grid);
            HC.WriteLine(text + "created");
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectedItem = Items[int.Parse((sender as Grid).Tag.ToString())];
            Selected.Content = selectedItem.text;
            CloseOrOpen();
            SelectionItemChanged.Invoke(selectedItem);
        }

        public void setSelectedItem(int index)
        {
            selectedItem = Items[index];
            Selected.Content = selectedItem.text;
        }
    }

    public class HippocampComboxBoxItem
    {
        public string text;
        public object obj;
        public HippocampComboxBoxItem(string text, Object obj)
        {
            this.text = text;
            this.obj = obj;
        }
    }

}
