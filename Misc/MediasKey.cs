using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Re_Hippocamp.Misc
{
    internal class MediasKey
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID_PLAYPAUSE = 9001;
        private const int HOTKEY_ID_NEXT = 9002;
        private const int HOTKEY_ID_PREVIOUS = 9003;


        private const uint VK_PLAY = 0xB3;
        private const uint VK_NEXT = 0xB0;
        private const uint VK_PREV = 0xB1;

        private IntPtr _windowHandle;
        private HwndSource _source;
        MainWindow MainWindow;

        private bool registered = false;

        public void Start(MainWindow mainWindow)
        {
            if (registered) return;
            registered = true;
            MainWindow = mainWindow;

            _windowHandle = new WindowInteropHelper(mainWindow).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID_PLAYPAUSE, 0x0000, VK_PLAY);
            RegisterHotKey(_windowHandle, HOTKEY_ID_NEXT, 0x0000, VK_NEXT);
            RegisterHotKey(_windowHandle, HOTKEY_ID_PREVIOUS, 0x0000, VK_PREV);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            int vkey = (((int)lParam >> 16) & 0xFFFF);
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID_PLAYPAUSE:
                            if (vkey == VK_PLAY)
                            {
                                MainWindow.changePausePlayState();
                            }
                            handled = true;
                            break;

                        case HOTKEY_ID_NEXT:
                            if (vkey == VK_NEXT)
                            {
                                MainWindow.playNextSongInAlbum();
                            }
                            handled = true;
                            break;

                        case HOTKEY_ID_PREVIOUS:
                            if (vkey == VK_PREV)
                            {
                                MainWindow.playPrecedentSongInAlbum();
                            }
                            handled = true;
                            break;


                    }
                    break;
            }
            return IntPtr.Zero;
        }

        public void Stop()
        {
            if (!registered) return;
            _source.RemoveHook(HwndHook);

            UnregisterHotKey(_windowHandle, HOTKEY_ID_PLAYPAUSE);
            UnregisterHotKey(_windowHandle, HOTKEY_ID_NEXT);
            UnregisterHotKey(_windowHandle, HOTKEY_ID_PREVIOUS);
            registered = false;
        }
    }
}
