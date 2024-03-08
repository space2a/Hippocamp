using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Re_Hippocamp.Misc
{
    public class Theme
    {
        private List<ThemeColor> Colors = new List<ThemeColor>();

        public List<ThemeFile> themeFiles = new List<ThemeFile>();

        private ThemeFile selectedTheme = null;

        public void getThemes()
        {
            Colors.Add(new ThemeColor() { colorName = "primaryColor", colorValue = Color.FromRgb(25, 26, 27) });
            Colors.Add(new ThemeColor() { colorName = "secondaryColor", colorValue = Color.FromRgb(20, 21, 25) });
            Colors.Add(new ThemeColor() { colorName = "tabrectangleColor", colorValue = Color.FromRgb(46, 86, 183) });
            Colors.Add(new ThemeColor() { colorName = "activeColor", colorValue = Color.FromRgb(66, 104, 188) });
            Colors.Add(new ThemeColor() { colorName = "accentColor", colorValue = Color.FromRgb(166, 207, 233) });
            Colors.Add(new ThemeColor() { colorName = "regularText", colorValue = Color.FromRgb(255, 255, 255) });
            Colors.Add(new ThemeColor() { colorName = "PUPprimaryColor", colorValue = Color.FromRgb(41, 42, 45) });
            Colors.Add(new ThemeColor() { colorName = "PUPsecondaryColor", colorValue = Color.FromRgb(32, 33, 36) });
            Colors.Add(new ThemeColor() { colorName = "logoprimaryColor", colorValue = Color.FromRgb(76, 120, 217) });
            Colors.Add(new ThemeColor() { colorName = "logosecondaryColor", colorValue = Color.FromRgb(43, 67, 122) });
            Colors.Add(new ThemeColor() { colorName = "playingbarColor", colorValue = Color.FromRgb(15, 15, 15) });
            Colors.Add(new ThemeColor() { colorName = "valuebarColor", colorValue = Color.FromRgb(46, 86, 180) });
            Colors.Add(new ThemeColor() { colorName = "valuebgbarColor", colorValue = Color.FromRgb(177, 177, 177) });

            Directory.CreateDirectory("Themes");
            var files = Directory.GetFiles("./Themes/", "*.ht", SearchOption.AllDirectories);

            foreach (var f in files)
            {
                var fi = new FileInfo(f);
                if (fi.Name == "_demofields.ht" && !HC.canWrite) continue;
                ThemeFile t = new ThemeFile()
                {
                    name = fi.Name,
                    path = f,
                    quickScanResult = true
                };

                LoadThemeFile(t);

                themeFiles.Add(t);
            }
        }

        private void scanThemeFile(ThemeFile theme)
        {
            string[] lines = File.ReadAllLines(theme.path);

            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                if (l.StartsWith("//")) continue;
                try
                {
                    string property = l.Substring(0, l.LastIndexOf(":"));
                    string value = l.Substring(l.LastIndexOf(":") + 1);

                    int r = 255, g = 255, b = 255;

                    r = int.Parse(value.Substring(0, value.IndexOf(" ")));
                    //HC.WriteLine(value.Substring(value.IndexOf(" "), value.LastIndexOf(" ") - r.ToString().Length));
                    g = int.Parse(value.Substring(value.IndexOf(" "), value.LastIndexOf(" ") - r.ToString().Length));
                    b = int.Parse(value.Substring(value.LastIndexOf(" ")));
                    //HC.WriteLine("Color created r;g;b" + r + ";" + g + ";" + b);
                    theme.colors.Add(new ThemeColor() { colorName = property, colorValue = Color.FromRgb((byte)r,(byte)g,(byte)b) });
                }
                catch (Exception ex)
                {
                    HC.WriteLine("Theme '" + theme.name + "' scanThemeFile error line " + i + " " + ex.ToString() + " line::" + l);
                    theme.errorLine = i;
                    theme.quickScanResult = false;
                }
            }
        }

        private void LoadThemeFile(ThemeFile theme)
        {
            if (theme.name == "default") return;
            scanThemeFile(theme);
        }

        public void selectThemeFile(ThemeFile theme)
        {
            selectedTheme = theme;
        }

        public Color getColorFromPropertyName(string name)
        {
            if(selectedTheme != null)
            {
                foreach (var item in selectedTheme.colors)
                {
                    if (item.colorName.ToLower() == name.ToLower())
                        return item.colorValue;
                }
            }

            foreach (var item in Colors)
            {
                if (item.colorName.ToLower() == name.ToLower())
                    return item.colorValue;
            }
            return Color.FromArgb(255, 255, 0, 0);
        }

        public void applyThemeInGrid(Grid g)
        {
            if (selectedTheme == null) return;
            foreach (Label element in FindVisualChildren<Label>(g))
                element.Foreground = getColor(element.Foreground);

            foreach (TextBlock element in FindVisualChildren<TextBlock>(g))
                element.Foreground = getColor(element.Foreground);

            foreach (Rectangle element in FindVisualChildren<Rectangle>(g))
            {
                if (element.Fill as SolidColorBrush != null)
                    element.Fill = getColor(element.Fill);

                if (element.Stroke as SolidColorBrush != null)
                    element.Stroke = getColor(element.Stroke);
            }

            foreach (Border element in FindVisualChildren<Border>(g))
            {
                if (element.Background as SolidColorBrush != null)
                    element.Background = getColor(element.Background);

                if (element.BorderBrush as SolidColorBrush != null)
                    element.BorderBrush = getColor(element.BorderBrush);
            }


        }

        private SolidColorBrush getColor(Brush solidColorBrush)
        {
            if((solidColorBrush as SolidColorBrush) == null) return null;
            Color f = (solidColorBrush as SolidColorBrush).Color;
            Color fC = Color.FromRgb(f.R, f.G, f.B);

            Color t = f;
            foreach (var item in selectedTheme.colors)
                if(Colors.FindIndex(x => x.colorValue == fC) != -1 && Colors[Colors.FindIndex(x => x.colorValue == fC)].colorName == item.colorName)
                {
                    t = Color.FromArgb(f.A, item.colorValue.R, item.colorValue.G, item.colorValue.B);

                    break;
                }

            return new SolidColorBrush(t);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

    }

    public class ThemeFile
    {
        public string name;
        public string path;

        public List<ThemeColor> colors = new List<ThemeColor>();

        public bool quickScanResult = true; //true = no problems ; false = problem(s) somewhere in the theme file
        public int errorLine = 0;
        public string warnings = "";
    }

    public class ThemeColor
    {
        public string colorName;
        public Color colorValue;
    }
}
