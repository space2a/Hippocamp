using Re_Hippocamp.Misc;

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Re_Hippocamp.Windows
{
    public partial class NI : Window
    {
        #region Window styles
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion


        public bool isDeployed = false;

        private MainWindow _mainWindow;

        public NI(MainWindow mainWindow)
        {
            InitializeComponent();

            _mainWindow = mainWindow;
            this.Opacity = 0;

            this.MouseEnter += NI_MouseEnter;
            this.MouseLeave += NI_MouseLeave;

            ResetView();
            //HideNI();


            this.Loaded += NI_Loaded;
        }

        private void NI_Loaded(object sender, RoutedEventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource; //HardwareAcceleration not needed on this window.
            if (hwndSource != null)
            {
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }

            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

        }

        public void ResetView()
        {
            Controls.Visibility = Visibility.Hidden;
            SLNOH.Visibility = Visibility.Visible;
            BG.Background = new SolidColorBrush(Color.FromArgb(255, 66,104,188));
        }

        public void DeployView(byte[] cover, HSong hSong)
        {
            Controls.Visibility = Visibility.Visible;
            SLNOH.Visibility = Visibility.Hidden;

            PlayingNameSong.Content = "";
            PlayingArtistSong.Content = "";

            if (hSong.Name != null)
            {
                if(hSong.Name.Length > 17)
                    PlayingNameSong.Content = hSong.Name.Substring(0, 18) + "...";
                else
                    PlayingNameSong.Content = hSong.Name;
            }

            if (hSong.Artist != null)
                PlayingArtistSong.Content = hSong.Artist;

            BG.Background = new ImageBrush(BO.LoadImage(cover, 2));
            SongCover.Fill = new ImageBrush(BO.LoadImage(cover, 125));
        }

        private void NI_MouseLeave(object sender, MouseEventArgs e)
        {
            return;
            OpacityAnim(1, 1, new TimeSpan(0, 0, 0, 2, 0), this, false);
        }

        private void NI_MouseEnter(object sender, MouseEventArgs e)
        {
            storyboard.Stop();
            storyboard = new Storyboard();
            this.Opacity = 1;
        }

        public void Deploy()
        {
            MoveBottomRightEdgeOfWindowToMousePosition();
            OpacityAnim(1, 1, new TimeSpan(0, 0, 0, 2, 0), this, false);
            _mainWindow.Show();
            new Thread(() =>
            {
                Thread.Sleep(100);
                MHippocamp.show(System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ""));
            }).Start();
            isDeployed = true;
            //anim...
        }

        public void HideNI()
        {
            sticked = false;
            storyboard.Stop();
            storyboard = new Storyboard();
            //anim opacity 0
            //Opacity(0, NI.Opacity, new TimeSpan(0, 0, 0, 0, 250), this, true);
            this.Opacity = 0;
            isDeployed = false;
            this.Hide();
        }

        Storyboard storyboard = new Storyboard();

        public void OpacityAnim(double to, double from, TimeSpan speed, FrameworkElement obj, bool secondPhase = false)
        {
            if (sticked) return;
            var a = new DoubleAnimation
            {
                From = from,
                To = to,
                FillBehavior = FillBehavior.Stop,
                Duration = speed
            };
            storyboard = new Storyboard();

            storyboard.Children.Add(a);

            Storyboard.SetTarget(a, obj);
            Storyboard.SetTargetProperty(a, new PropertyPath(FrameworkElement.OpacityProperty));
            storyboard.Completed += delegate {
                if (!secondPhase)
                {
                    OpacityAnim(0, from, new TimeSpan(0, 0, 0, 1, 500), obj, true);
                }
                else HideNI();
            };
            storyboard.Begin();
        }

        private void MoveBottomRightEdgeOfWindowToMousePosition()
        {
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(GetMousePosition());
            Left = mouse.X - (ActualWidth / 2);

            if (mouse.Y < 150)
                Top = mouse.Y + 20;
            else
                Top = mouse.Y - ActualHeight - 20;
        }

        public System.Windows.Point GetMousePosition()
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            return new System.Windows.Point(point.X, point.Y);
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.Show();
            _mainWindow.BringAppToViewThumbButtonInfo_Click(null, null);
        }

        private void ShuffleControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.ShuffleControl_MouseLeftButtonUp(sender, e);
        }

        private void RepeatControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.RepeatControl_MouseLeftButtonUp(sender, e);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.PlayerPausePlay_MouseLeftButtonUp(sender, e);
        }

        private void PlayPrecedentSongAlbum_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.playPrecedentSongInAlbum();
        }

        private void NextSongAlbum_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.playNextSongInAlbum();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            StickGrid.Opacity = 1;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            StickGrid.Opacity = 0.2;
        }

        private bool sticked = false;
        private void StickGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            storyboard.Stop();
            storyboard = new Storyboard();
            if (sticked)
            {
                Stick.Visibility = Visibility.Hidden;
                this.Topmost = true;
                sticked = false;
            }
            else
            {
                Stick.Visibility = Visibility.Visible;
                this.Topmost = false;
                sticked = true;
            }

        }

        private void BG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Opacity = 0;
            isDeployed = false;
            this.Hide();
        }
    }
}
