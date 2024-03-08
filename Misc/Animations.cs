using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Re_Hippocamp
{
    public static class Animations
    {
        public static void ColorAnimationOBJ(Color from, Color to, TimeSpan dur, DependencyObject obj, string Obj = "Rectangle.Fill")
        {
            ColorAnimation colorChangeAnimation = new ColorAnimation();
            colorChangeAnimation.From = from;
            colorChangeAnimation.To = to;
            colorChangeAnimation.Duration = dur;

            PropertyPath colorTargetPath = new PropertyPath("(" + Obj + ").(SolidColorBrush.Color)");
            Storyboard CellBackgroundChangeStory = new Storyboard();
            Storyboard.SetTarget(colorChangeAnimation, obj);
            Storyboard.SetTargetProperty(colorChangeAnimation, colorTargetPath);
            CellBackgroundChangeStory.Children.Add(colorChangeAnimation);
            CellBackgroundChangeStory.Begin();
        }

        public static void MarginToMargin(Thickness from, Thickness to, TimeSpan dur, FrameworkElement obj)
        {
            var a = new ThicknessAnimation
            {
                From = from,
                To = to,
                FillBehavior = FillBehavior.Stop,
                Duration = dur
            };

            var storyboard = new Storyboard();

            storyboard.Children.Add(a);

            Storyboard.SetTarget(a, obj);
            Storyboard.SetTargetProperty(a, new PropertyPath(FrameworkElement.MarginProperty));
            storyboard.Completed += delegate { obj.Margin = to; };
            storyboard.Begin();
        }

        public static void BlurRadius(int to, int from, TimeSpan speed, FrameworkElement obj)
        {
            var a = new DoubleAnimation
            {
                From = from,
                To = to,
                FillBehavior = FillBehavior.Stop,
                Duration = speed
            };

            var storyboard = new Storyboard();

            storyboard.Children.Add(a);

            Storyboard.SetTarget(a, obj);
            Storyboard.SetTargetProperty(a, new PropertyPath("Effect.Radius"));
            storyboard.Completed += delegate { obj.Effect = new BlurEffect() { Radius = to }; };
            storyboard.Begin();
        }

        public static void ShadowOpacity(double to, double from, TimeSpan speed, FrameworkElement obj)
        {
            var a = new DoubleAnimation
            {
                From = from,
                To = to,
                FillBehavior = FillBehavior.Stop,
                Duration = speed
            };

            var storyboard = new Storyboard();

            storyboard.Children.Add(a);

            Storyboard.SetTarget(a, obj);
            Storyboard.SetTargetProperty(a, new PropertyPath("Effect.Opacity"));
            storyboard.Completed += delegate { (obj.Effect as DropShadowEffect).Opacity = to; };
            storyboard.Begin();
        }

        public static void Opacity(double to, double from, TimeSpan speed, FrameworkElement obj, bool collapse = false)
        {
            if (to > 0)
                obj.Visibility = Visibility.Visible;

            var a = new DoubleAnimation
            {
                From = from,
                To = to,
                FillBehavior = FillBehavior.Stop,
                Duration = speed
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(a);

            Storyboard.SetTarget(a, obj);
            Storyboard.SetTargetProperty(a, new PropertyPath(FrameworkElement.OpacityProperty));
            storyboard.Completed += delegate { storyboard.Stop(); storyboard = null; a = null; obj.Opacity = to; if (to == 0) { if (collapse) { obj.Visibility = Visibility.Collapsed; } else { obj.Visibility = Visibility.Hidden; } } };
            storyboard.Begin();
        }

        public static void Stroke(double to, double from, TimeSpan speed, FrameworkElement obj)
        {
            var a = new DoubleAnimation
            {
                From = from,
                To = to,
                FillBehavior = FillBehavior.Stop,
                Duration = speed
            };

            var storyboard = new Storyboard();

            storyboard.Children.Add(a);

            Storyboard.SetTarget(a, obj);
            Storyboard.SetTargetProperty(a, new PropertyPath(System.Windows.Shapes.Rectangle.StrokeThicknessProperty));
            storyboard.Completed += delegate { (obj as Rectangle).StrokeThickness = to; };
            storyboard.Begin();
        }

        public static void Width(double from, double to, TimeSpan speed, FrameworkElement obj)
        {
            var a = new DoubleAnimation
            {
                From = from,
                To = to,
                FillBehavior = FillBehavior.Stop,
                Duration = speed
            };

            var storyboard = new Storyboard();

            storyboard.Children.Add(a);

            Storyboard.SetTarget(a, obj);
            Storyboard.SetTargetProperty(a, new PropertyPath(FrameworkElement.WidthProperty));
            storyboard.Completed += delegate { obj.Width = to; };
            storyboard.Begin();
        }

        public static void Height(double from, double to, TimeSpan speed, FrameworkElement obj)
        {
            var a = new DoubleAnimation
            {
                From = from,
                To = to,
                FillBehavior = FillBehavior.Stop,
                Duration = speed
            };

            var storyboard = new Storyboard();

            storyboard.Children.Add(a);

            Storyboard.SetTarget(a, obj);
            Storyboard.SetTargetProperty(a, new PropertyPath(FrameworkElement.HeightProperty));
            storyboard.Completed += delegate { obj.Height = to; };
            storyboard.Begin();
        }


        public static void Scale(object sender, double from, double to, TimeSpan speed)
        {
            Storyboard storyboard = new Storyboard();

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            (sender as UIElement).RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            (sender as UIElement).RenderTransform = scale;

            DoubleAnimation growAnimation = new DoubleAnimation();
            DoubleAnimation growAnimation2 = new DoubleAnimation();
            growAnimation.Duration = speed;
            growAnimation2.Duration = speed;
            growAnimation.From = from;
            growAnimation2.From = from;
            growAnimation.To = to;
            growAnimation2.To = to;
            storyboard.Children.Add(growAnimation);
            storyboard.Children.Add(growAnimation2);

            Storyboard.SetTargetProperty(growAnimation, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(growAnimation2, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(growAnimation, (sender as UIElement));
            Storyboard.SetTarget(growAnimation2, (sender as UIElement));

            storyboard.Begin();
        }

        public static void Angle(object sender, double from, double to, TimeSpan speed)
        {
            Storyboard storyboard = new Storyboard();

            RotateTransform rotate = new RotateTransform();
            (sender as UIElement).RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            (sender as UIElement).RenderTransform = rotate;

            DoubleAnimation growAnimation = new DoubleAnimation();
            growAnimation.Duration = speed;
            growAnimation.From = from;
            growAnimation.To = to;
            storyboard.Children.Add(growAnimation);

            Storyboard.SetTargetProperty(growAnimation, new PropertyPath("RenderTransform.Angle"));
            Storyboard.SetTarget(growAnimation, (sender as UIElement));

            storyboard.Begin();
        }

        public static void PauseOrResumeAllAnimations(FrameworkElement fe)
        {
            foreach (DictionaryEntry i in fe.Resources)
            {
                ((Storyboard)fe.Resources[i.Key]).Stop(fe);
            }
        }

    }
}