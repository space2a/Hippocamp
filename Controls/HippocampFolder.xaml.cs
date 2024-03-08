using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampFolder : UserControl
    {

        public SavedPath SavedPath;

        public delegate void RemovedAction(HippocampFolder HippocampFolder);
        public event RemovedAction Removed;
        public HippocampFolder()
        {
            InitializeComponent();

            Delete.ButtonPressed += Delete_ButtonPressed;
        }

        public void Ini(SavedPath sp)
        {
            if (sp == null)
                sp = new SavedPath();
            SavedPath = sp;
            FolderPath.Content = sp.Path;
            if (sp.SongsDiscovered == -1)
                Discovered.Content = "Discovered : " + "---";
            else
                Discovered.Content = "Discovered : " + sp.SongsDiscovered;
        }

        private void Delete_ButtonPressed(HippocampButton hippocampButton)
        {
            MainWindow.hippocampMessageBox.Show("Are you sure ?", "Do you really want to remove this folder from Hippocamp ?", HippocampMessageBox.HippocampMessageBoxButtons.NoYes);
            MainWindow.hippocampMessageBox.HippocampMessageBoxValidated += delegate (bool p)
            {
                if (!p)
                {
                    Removed.Invoke(this);
                }
            };
        }

        public SavedPath GetSavedPath()
        {
            return SavedPath;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Animations.Scale(Img, 1, 1.1, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10));
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Animations.Scale(Img, 1.1, 1, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10));
        }

        private void FolderPath_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(Directory.Exists((sender as Label).Content.ToString()))
                Process.Start((sender as Label).Content.ToString());
        }
    }
}
