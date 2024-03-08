using Re_Hippocamp.Misc;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Windows.Devices.Enumeration;
using Windows.Media.Devices;

namespace Re_Hippocamp.PopUpPages
{
    public partial class SettingsPUP : UserControl
    {
        MainWindow MainWindow;
        public SettingsPUP(MainWindow mainWindow)
        {
            InitializeComponent();
            this.MainWindow = mainWindow;
            loadSettings();

            DevicesBox.SelectionItemChanged += DevicesBox_SelectionItemChanged;
            Version.Text = "Hippocamp " + MainWindow.HippocampVersion;
            MainWindow.theme.applyThemeInGrid(this.Content as Grid);

            ThemesBox.SelectionItemChanged += ThemesBox_SelectionItemChanged;
            ThemesBox.addItem(new Controls.HippocampComboxBoxItem("Default", null));
            ThemesBox.setSelectedItem(0);
            foreach (var tf in MainWindow.theme.themeFiles)
            {
                ThemesBox.addItem(new Controls.HippocampComboxBoxItem(tf.name, tf));
                if (tf.name == MainWindow.hippocampSettings.hippocampTheme) ThemesBox.setSelectedItem(ThemesBox.Items.FindIndex(x => x.text == tf.name));
            }
        }

        bool themeWarn = false;
        private void ThemesBox_SelectionItemChanged(Controls.HippocampComboxBoxItem newItem)
        {


            updateSettings();

            if (themeWarn) return;
            themeWarn = true;
            MainWindow.hippocampMessageBox.Show("Setting changed", "Hippocamp need to restart to change the theme.", Controls.HippocampMessageBox.HippocampMessageBoxButtons.Ok);
        }

        private void DevicesBox_SelectionItemChanged(Controls.HippocampComboxBoxItem newItem)
        {
            MainWindow.PUP_willNeedReloadUsers = true;
            updateSettings();
        }

        private async Task getAudioDevicesAsync()
        {
            string audioSelector = MediaDevice.GetAudioRenderSelector();
            var outputDevices = await DeviceInformation.FindAllAsync(audioSelector);
            DevicesBox.addItem(new Controls.HippocampComboxBoxItem("Default", null));
            DevicesBox.setSelectedItem(0);
            foreach (var device in outputDevices)
            {
                HC.WriteLine(device.Name);
                DevicesBox.addItem(new Controls.HippocampComboxBoxItem(device.Name, device));
                if (device.Name == MainWindow.hippocampSettings.audioDevice) DevicesBox.setSelectedItem(DevicesBox.Items.FindIndex(x =>x.text == device.Name));
            }
        }

        private void loadSettings()
        {
            disableHardwareAcceleration.isCoolChecked = MainWindow.hippocampSettings.disableHardwareAcceleration;
            disableMediaKeys.isCoolChecked = MainWindow.hippocampSettings.disableMediaKeys;
            disableTransparency.isCoolChecked = MainWindow.hippocampSettings.disableTransparency;
            disableWindowsOverlay.isCoolChecked = MainWindow.hippocampSettings.disableWindowsOverlay;
            removeAnimations.isCoolChecked = MainWindow.hippocampSettings.removeAnimations;
            removeLoadingAnimation.isCoolChecked = MainWindow.hippocampSettings.removeLoadingAnimation;
            blockMaxVolume.isCoolChecked = MainWindow.hippocampSettings.blockMaxVolume;
            useDiscordRichPresence.isCoolChecked = MainWindow.hippocampSettings.useDiscordRichPresence;
            startWithWindows.isCoolChecked = MainWindow.hippocampSettings.startHippocampWithWindows;
            useCoverColor.isCoolChecked = MainWindow.hippocampSettings.useCoverColor;

            getAudioDevicesAsync();
        }

        private void updateSettings()
        {
            if (!IsInitialized) return;
            MainWindow.hippocampSettings.disableHardwareAcceleration = disableHardwareAcceleration.isCoolChecked;
            MainWindow.hippocampSettings.disableMediaKeys = disableMediaKeys.isCoolChecked;
            MainWindow.hippocampSettings.disableTransparency = disableTransparency.isCoolChecked;
            MainWindow.hippocampSettings.disableWindowsOverlay = disableWindowsOverlay.isCoolChecked ;
            MainWindow.hippocampSettings.removeAnimations = removeAnimations.isCoolChecked;
            MainWindow.hippocampSettings.removeLoadingAnimation = removeLoadingAnimation.isCoolChecked;
            MainWindow.hippocampSettings.blockMaxVolume = blockMaxVolume.isCoolChecked;
            MainWindow.hippocampSettings.useDiscordRichPresence = useDiscordRichPresence.isCoolChecked;
            MainWindow.hippocampSettings.startHippocampWithWindows = startWithWindows.isCoolChecked;
            MainWindow.hippocampSettings.audioDevice = DevicesBox.selectedItem.text;
            MainWindow.hippocampSettings.hippocampTheme = ThemesBox.selectedItem.text;
            MainWindow.hippocampSettings.useCoverColor = useCoverColor.isCoolChecked;

            MainWindow.applySettings();
            HC.WriteAllBytes("settings.hs", BO.ObjectToByteArray(MainWindow.hippocampSettings));
        }

        private void HippocampCoolCheckbox_CheckboxInt(Controls.HippocampCoolCheckbox hippocampCoolCheckbox)
        {
            updateSettings();
        }

        private void disableTransparency_CheckboxInt(Controls.HippocampCoolCheckbox hippocampCoolCheckbox)
        {
            updateSettings();
            MainWindow.hippocampMessageBox.Show("Setting changed", "Hippocamp need to restart to enable/disable the transparency.", Controls.HippocampMessageBox.HippocampMessageBoxButtons.Ok);
        }

        private void Version_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as TextBlock).TextDecorations = TextDecorations.Underline;
        }

        private void Version_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as TextBlock).TextDecorations = null;
        }

        private void Version_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(Directory.Exists(Directory.GetCurrentDirectory()))
                Process.Start(Directory.GetCurrentDirectory());
        }
    }
}
