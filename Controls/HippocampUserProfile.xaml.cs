using System.Windows.Controls;
using System.Windows.Media;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampUserProfile : UserControl
    {

        private UserProfile me;

        public HippocampUserProfile()
        {
            InitializeComponent();
        }

        public void updateProfile(UserProfile userProfile)
        {
            Username.Text = userProfile.Username;
            me = userProfile;

            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = BO.LoadImage(userProfile.Avatar, 150);
            imageBrush.Stretch = Stretch.UniformToFill;
            AvatarImg1.Fill = imageBrush;
        }

        public UserProfile GetUserProfile()
        {
            return me;
        }

    }
}
