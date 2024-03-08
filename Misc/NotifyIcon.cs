using Re_Hippocamp.Windows;
using System;
using System.Windows;
using System.Windows.Forms;

namespace Re_Hippocamp.Misc
{
    internal class NotifyIcon
    {

        private MainWindow MainWindow;
        public System.Windows.Forms.NotifyIcon NI;


        NI notifyIcon;

        public NI DeployNotifyIcon(MainWindow mw)
        {
            notifyIcon = new NI(mw) { Left = Cursor.Position.X, Top = Cursor.Position.Y, Topmost = true };
            notifyIcon.Show();

            MainWindow = mw;
            NI = new System.Windows.Forms.NotifyIcon() { Text = "Hippocamp", BalloonTipText = "Double click on this icon to open Mini-Hippocamp", Visible = true };
            NI.MouseClick += NI_MouseClick;
            NI.BalloonTipClosed += NI_BalloonTipClosed;
            //NI.ContextMenu = new CCM(); :(
            NI.Icon = new System.Drawing.Icon(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/HippocampLogo.ico")).Stream);
            NI.DoubleClick += delegate (object sender, EventArgs e)
            {
                if (notifyIcon.isDeployed)
                    notifyIcon.HideNI();
                else
                    notifyIcon.Deploy();
            };

            return notifyIcon;
        }

        private void NI_BalloonTipClosed(object sender, EventArgs e)
        {
            var thisIcon = (System.Windows.Forms.NotifyIcon)sender;
            thisIcon.Visible = false;
            thisIcon.Dispose();
        }

        private void NI_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
        }
    }
}
