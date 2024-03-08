using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Re_Hippocamp.Controls
{
    public partial class HippocampMessageBoxUI : UserControl
    {
        public Grid parent;
        public Window moveableWindow;
        public HippocampMessageBox myManager;

        public HippocampMessageBoxUI()
        {
            InitializeComponent();

            this.Visibility = Visibility.Hidden;
            SP.Children.Clear();
        }

        public void deployMessageBox()
        {
            this.Visibility = Visibility.Visible;
        }

        public void setValues(string t, string c)
        {
            MsgBoxTitle.Content = t;
            MsgBoxContent.Text = c;
        }

        public void createButtons(HippocampMessageBox.HippocampMessageBoxButtons hippocampMessageBoxButtons)
        {
            SP.Children.Clear();

            if (hippocampMessageBoxButtons == HippocampMessageBox.HippocampMessageBoxButtons.Ok)
                createButton("Ok", true);
            else if (hippocampMessageBoxButtons == HippocampMessageBox.HippocampMessageBoxButtons.OkCancel)
            {
                createButton("Ok", true);
                createButton("Cancel", false);
            }
            else if (hippocampMessageBoxButtons == HippocampMessageBox.HippocampMessageBoxButtons.YesNo)
            {
                createButton("Yes", true);
                createButton("No", false);
            }
            else if (hippocampMessageBoxButtons == HippocampMessageBox.HippocampMessageBoxButtons.NoYes)
            {
                createButton("Yes", false);
                createButton("No", true);
            }
            else if (hippocampMessageBoxButtons == HippocampMessageBox.HippocampMessageBoxButtons.MinimizeExit)
            {
                createButton("Minimize", true);
                createButton("Exit", false);
            }
        }

        private void createButton(string Content, bool isMain)
        {
            HippocampButton hippocampButton = new HippocampButton() { Margin = new Thickness(5, 0, 5, 0), Height = isMain? 37 : 35, Width = isMain ? 75 : 70, Text = Content, IsMain = isMain };
            SP.Children.Add(hippocampButton);

            hippocampButton.ButtonPressed += delegate (HippocampButton b)
            {
                myManager.Validate(b.IsMain);
            };
        }

        private void Rectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && moveableWindow != null)
                moveableWindow.DragMove();

            myManager.cancelBox();
        }
    }


    public class HippocampMessageBox
    {
        private HippocampMessageBoxUI hippocampMessageBoxUI;
        public delegate void HippocampMessageBoxValidatedAction(bool pressedMain);
        public event HippocampMessageBoxValidatedAction HippocampMessageBoxValidated;

        private bool isAlreadyDeployed = false;

        private bool authorizeCancel = false;

        public HippocampMessageBox(Grid parent, Window moveableWindow = null)
        {
            hippocampMessageBoxUI = new HippocampMessageBoxUI();
            hippocampMessageBoxUI.parent = parent;
            hippocampMessageBoxUI.moveableWindow = moveableWindow;
            hippocampMessageBoxUI.myManager = this;
            parent.Children.Add(hippocampMessageBoxUI);
        }

        public void Validate(bool i)
        {
            try
            {
                HippocampMessageBoxValidated.Invoke(i);
            }
            catch (System.Exception) { }
            EndBox();
        }

        public void EndBox()
        {
            isAlreadyDeployed = false;
            Animations.Opacity(0, 1, new System.TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), hippocampMessageBoxUI);

            new Thread(() =>
            {
                Thread.Sleep(260);
                Application.Current.Dispatcher.Invoke(new Action(() => { hippocampMessageBoxUI.SP.Children.Clear(); }));
            }).Start();
        }

        public void cancelBox()
        {
            if (authorizeCancel)
            {
                removeEventHandlers();
                EndBox();
            }
        }
        
        private void removeEventHandlers()
        {
            if (HippocampMessageBoxValidated != null)
                foreach (Delegate d in HippocampMessageBoxValidated.GetInvocationList())
                    HippocampMessageBoxValidated -= (HippocampMessageBoxValidatedAction)d;
        }

        public void Show(string Title, string Content, HippocampMessageBoxButtons buttons, bool authorizeCancel = false)
        {
            if (isAlreadyDeployed) return;

            removeEventHandlers();

            Panel.SetZIndex(hippocampMessageBoxUI, 99999);
            this.authorizeCancel = authorizeCancel;
            hippocampMessageBoxUI.createButtons(buttons);
            hippocampMessageBoxUI.setValues(Title, Content);
            hippocampMessageBoxUI.deployMessageBox();
            hippocampMessageBoxUI.Visibility = Visibility.Visible;
            isAlreadyDeployed = true;
            Animations.Opacity(1, 0, new System.TimeSpan(0, 0, 0, 0, !MainWindow.hippocampSettings.removeAnimations ? 250 : 10), hippocampMessageBoxUI);
        }

        public enum HippocampMessageBoxButtons
        {
            Ok,
            YesNo,
            NoYes,
            OkCancel,
            MinimizeExit
        }
    }
}
