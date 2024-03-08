using Re_Hippocamp.Misc;

using System.Windows;
using System.Windows.Controls;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampContextMenu : ContextMenu
    {
        private UIElement P;

        public HippocampContextMenu()
        {
            InitializeComponent();
        }

        public void IniMW(UIElement P)
        {
            this.P = P;
        }


        public void createItem(HippocampContextMenuItem newItem)
        {
            foreach (StackPanel element in Theme.FindVisualChildren<StackPanel>(this))
            {
                if (element.Children.Contains(newItem)) return;
                newItem.HippocampContextMenu = this;
                element.Children.Add(newItem);
            }
        }

        public void isOpen(bool s) { this.IsOpen = s; }
    }
}
