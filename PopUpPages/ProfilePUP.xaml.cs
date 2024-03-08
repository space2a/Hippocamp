using Re_Hippocamp.Controls;
using Re_Hippocamp.Misc;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Re_Hippocamp.PopUpPages
{
    public partial class ProfilePUP : UserControl
    {
        private UserProfiles UserProfiles;
        private UserProfile currentEditingProfile = null;

        public MainWindow xMainWindow;

        private bool ini = false;

        public ProfilePUP()
        {
            InitializeComponent();

            loadProfiles();
            EditProfileContent.Visibility = Visibility.Hidden;
            SaveProfileButton.ButtonPressed += SaveProfileButton_ButtonPressed;
            DeleteUserProfile.ButtonPressed += DeleteUserProfile_ButtonPressed;
            CreateProfileWelcome.ButtonPressed += CreateProfileWelcome_ButtonPressed;
            AddNewFolder.ButtonPressed += AddNewFolder_ButtonPressed;

            originalAvatar = BO.ImageSourceToBytes(new PngBitmapEncoder(), (AvatarImg1.Fill as ImageBrush).ImageSource);
            SPCollectionDirectories.Children.Clear();
            Welcome.Visibility = Visibility.Hidden;

            LEYE.Maximum = AvatarGenerator.LEYEMax;
            REYE.Maximum = AvatarGenerator.REYEMax;
            MOUTH.Maximum = AvatarGenerator.RMouthMax;
            COLOR.Maximum = 360;

            CustomAvatar.Visibility = Visibility.Hidden;
            MainWindow.theme.applyThemeInGrid(this.Content as Grid);
            ini = true;
        }

        private void CreateProfileWelcome_ButtonPressed(HippocampButton hippocampButton)
        {
            Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, 250), Welcome);
        }

        private void AddNewFolder_ButtonPressed(HippocampButton hippocampButton)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            System.Windows.Forms.NativeWindow win32Parent = new System.Windows.Forms.NativeWindow();
            win32Parent.AssignHandle(new WindowInteropHelper(xMainWindow).Handle);
            if (openFolderDialog.ShowDialog(win32Parent) == System.Windows.Forms.DialogResult.OK)
            {
                var sn = new SavedPath() { Path = openFolderDialog.Folder, ignoredSongs = new List<string>() };

                HippocampFolder hippocampFolder = new HippocampFolder();
                hippocampFolder.Ini(sn);
                SPCollectionDirectories.Children.Add(hippocampFolder);
                hippocampFolder.Removed += delegate (HippocampFolder hf)
                {
                    SPCollectionDirectories.Children.Remove(hf);
                };
            }
        }

        public void forceCreate()
        {
            EditProfileContent.Visibility = Visibility.Visible;
            EditProfile.Margin = new Thickness(0);
            (EditProfile.Children[0] as Border).CornerRadius = new CornerRadius(10);
            SideBar.Visibility = Visibility.Hidden;
            DeleteUserProfile.Visibility = Visibility.Hidden;
            SaveProfileButton.Text = "Create profile";
            forced = true;
            resetImg();
        }

        public void showWelcome()
        {

            Welcome.Visibility = Visibility.Visible;
        }

        private byte[] originalAvatar;

        private void resetImg()
        {
            //../Resources/Images/Avatar.png
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = AvatarGenerator.generateAvatar();
            imageBrush.Stretch = Stretch.Uniform;
            AvatarImg1.Fill = imageBrush;

            CustomAvatar.Visibility = Visibility.Hidden;
        }

        private void DeleteUserProfile_ButtonPressed(HippocampButton hippocampButton)
        {
            MainWindow.hippocampMessageBox.Show("Are you sure ?", "Do you really want to delete this profile ?", HippocampMessageBox.HippocampMessageBoxButtons.NoYes);
            MainWindow.hippocampMessageBox.HippocampMessageBoxValidated += delegate (bool p)
            {
                if (!p)
                {
                    for (int i = 0; i < UserProfiles.Profiles.Count; i++)
                    {
                        if (UserProfiles.Profiles[i].id == currentEditingProfile.id)
                        {
                            UserProfiles.Profiles.RemoveAt(i);
                            saveProfilesFile(false);

                            Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), EditProfileContent);
                            return;
                        }
                    }
                }
            };
        }

        private void loadProfiles()
        {
            SP.Children.Clear();
            if (File.Exists("profiles.hups"))
            {
                UserProfiles = BO.ByteArrayToObject(File.ReadAllBytes("profiles.hups")) as UserProfiles;
                if (UserProfiles == null) { MessageBox.Show("An error occured while reading your profile."); } //Corrupted profile file
                else
                {
                    //reading the profiles..
                    foreach (var up in UserProfiles.Profiles)
                    {
                        HippocampUserProfile userProfile = new HippocampUserProfile() { Width = 180 };
                        userProfile.updateProfile(up);
                        userProfile.MouseLeftButtonUp += UserProfile_MouseLeftButtonUp;
                        SP.Children.Add(userProfile);
                    }
                }
            }
        }

        private void UserProfile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CustomAvatar.Visibility = Visibility.Hidden;

            if (EditProfileContent.Visibility == Visibility.Hidden)
            {
                EditProfileContent.Visibility = Visibility.Visible;
                Animations.Opacity(1, 0, new TimeSpan(0,0,0,0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), EditProfileContent);
            }

            currentEditingProfile = (sender as HippocampUserProfile).GetUserProfile();

            UsernameBox.setText(currentEditingProfile.Username);

            if (UserProfiles.Profiles.Count == 1)
                DeleteUserProfile.Visibility = Visibility.Hidden;
            else
                DeleteUserProfile.Visibility = Visibility.Visible;

            SaveProfileButton.Text = "Save profile";

            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = BO.LoadImage(currentEditingProfile.Avatar, 150);
            imageBrush.Stretch = Stretch.Uniform;
            AvatarImg1.Fill = imageBrush;

            SPCollectionDirectories.Children.Clear();

            foreach (var savedPath in currentEditingProfile.savedPaths)
            {
                HippocampFolder hippocampFolder = new HippocampFolder();
                hippocampFolder.Ini(savedPath);
                SPCollectionDirectories.Children.Add(hippocampFolder);

                hippocampFolder.Removed += delegate (HippocampFolder hf)
                {
                    SPCollectionDirectories.Children.Remove(hf);
                };
            }

        }

        private void SaveProfileButton_ButtonPressed(Controls.HippocampButton hippocampButton)
        {
            if (UserProfiles == null) { UserProfiles = new UserProfiles() { IDG = 0 }; }

            if(UsernameBox.getText() == "")
            {
                MainWindow.hippocampMessageBox.Show("Error", "Please make sure all fields are filled in.", Controls.HippocampMessageBox.HippocampMessageBoxButtons.Ok);
                return;
            }

            List<SavedPath> sp = new List<SavedPath>();
            foreach (var spX in SPCollectionDirectories.Children)
            {
                var s = spX as HippocampFolder;
                sp.Add(s.GetSavedPath());
            }

            bool newU = false;
            if(currentEditingProfile == null) //creating a NEW profile
            {
                currentEditingProfile = new UserProfile() { Username = UsernameBox.getText(), id = UserProfiles.IDG++, Avatar = BO.ImageSourceToBytes(new PngBitmapEncoder(), (AvatarImg1.Fill as ImageBrush).ImageSource), savedPaths = sp };
                UserProfiles.Profiles.Add(currentEditingProfile);
                newU = true;
            }
            else
            {
                for (int i = 0; i < UserProfiles.Profiles.Count; i++)
                {
                    if(UserProfiles.Profiles[i].id == currentEditingProfile.id)
                    {
                        UserProfiles.Profiles[i] = new UserProfile() { Username = UsernameBox.getText(), id = currentEditingProfile.id, Avatar = BO.ImageSourceToBytes(new PngBitmapEncoder(), (AvatarImg1.Fill as ImageBrush).ImageSource), savedPaths = sp };
                    }
                }
            }

            saveProfilesFile(newU);

        }

        bool forced = false;
        private void saveProfilesFile(bool created)
        {
            xMainWindow.PUP_willNeedReloadUsers = true;
            HC.WriteAllBytes("profiles.hups", BO.ObjectToByteArray(UserProfiles));
            if (created)
            {
                MainWindow.hippocampMessageBox.Show("Success", "Profile created.", Controls.HippocampMessageBox.HippocampMessageBoxButtons.Ok);
                if(xMainWindow != null && forced)
                    xMainWindow.ClosePUP();
            }
            else
                MainWindow.hippocampMessageBox.Show("Success", "Profile saved.", Controls.HippocampMessageBox.HippocampMessageBoxButtons.Ok);

            currentEditingProfile = null;
            Animations.Opacity(0, 1, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), EditProfileContent);
            loadProfiles();
        }

        private void CreateNewProfile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (EditProfileContent.Visibility == Visibility.Hidden)
            {
                EditProfileContent.Visibility = Visibility.Visible;
                Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), EditProfileContent);
            }

            DeleteUserProfile.Visibility = Visibility.Hidden;
            SaveProfileButton.Text = "Create profile";
            currentEditingProfile = null;
            UsernameBox.setText("");
            SPCollectionDirectories.Children.Clear();
            var hipFolder = new HippocampFolder();
            hipFolder.Ini(new SavedPath() { Path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) });
            SPCollectionDirectories.Children.Add(hipFolder);
            resetImg();
        }

        private void AvatarGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            Animations.Opacity(1, 0, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), ChangeAvatar);
        }

        private void AvatarGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Animations.Opacity(0,1, new TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), ChangeAvatar);
        }

        private void AvatarGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png,*.jpg,*.jpeg)|*.png;*.jpg;*.jpeg;";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    ImageBrush imageBrush = new ImageBrush();
                    imageBrush.ImageSource = BO.LoadImage(File.ReadAllBytes(openFileDialog.FileName), 300);
                    imageBrush.Stretch = Stretch.Uniform;
                    AvatarImg1.Fill = imageBrush;
                }
                catch (Exception)
                {
                    MainWindow.hippocampMessageBox.Show("Error", "An error occurred while processing your image", HippocampMessageBox.HippocampMessageBoxButtons.Ok);
                }
            }
        }

        private void LEYE_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!ini) return;
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = AvatarGenerator.generateAvatar((int)LEYE.Value, (int)REYE.Value, (int)MOUTH.Value, (int)COLOR.Value);
            imageBrush.Stretch = Stretch.Uniform;
            AvatarImg1.Fill = imageBrush;
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CustomAvatar.Visibility = Visibility.Visible;
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CustomAvatar.Visibility = Visibility.Hidden;
        }
    }
}
