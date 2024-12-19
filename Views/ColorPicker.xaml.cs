using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace QwertyLauncher.Views
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        
        public ColorPicker()
        {
            InitializeComponent();
            handle = true;

        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(ColorPicker), new PropertyMetadata("#00000000"));

        public string Value
        {
            get => (string)GetValue(ValueProperty); 
            set {
                SetValue(ValueProperty, value);
                hex.Text = value;
            }
        }
        private bool handle = false;
        private void ColorUpdate(Color c)
        {
            c1.Fill = new SolidColorBrush(c);
            c2.Fill = new SolidColorBrush(c);

            A_Min.Color = Color.FromArgb(0x0, c.R, c.G, c.B);
            A_Max.Color = Color.FromArgb(0xFF, c.R, c.G, c.B);
            R_Min.Color = Color.FromArgb(c.A, 0x0, c.G, c.B);
            R_Max.Color = Color.FromArgb(c.A, 0xFF, c.G, c.B);
            G_Min.Color = Color.FromArgb(c.A, c.R, 0x0, c.B);
            G_Max.Color = Color.FromArgb(c.A, c.R, 0xFF, c.B);
            B_Min.Color = Color.FromArgb(c.A, c.R, c.G, 0x0);
            B_Max.Color = Color.FromArgb(c.A, c.R, c.G, 0xFF);

            H_0.Color = HsvToRgb(0, S.Value, V.Value);
            H_60.Color = HsvToRgb(60, S.Value, V.Value);
            H_120.Color = HsvToRgb(120, S.Value, V.Value);
            H_180.Color = HsvToRgb(180, S.Value, V.Value);
            H_240.Color = HsvToRgb(240, S.Value, V.Value);
            H_300.Color = HsvToRgb(300, S.Value, V.Value);
            H_360.Color = H_0.Color;

            S_Min.Color = HsvToRgb(H.Value, 0, V.Value);
            S_Max.Color = HsvToRgb(H.Value, 1, V.Value);
            V_Min.Color = HsvToRgb(H.Value, S.Value, 0);
            V_Max.Color = HsvToRgb(H.Value, S.Value, 1);

            Value = c.ToString();
        }

        private (double H, double S, double V) RgbToHsv(double r, double g, double b)
        {
            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;

            double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
            double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
            double delta = max - min;

            double h = 0;
            if (delta != 0)
            {
                if (max == rNorm)
                {
                    h = 60 * (((gNorm - bNorm) / delta) % 6);
                }
                else if (max == gNorm)
                {
                    h = 60 * (((bNorm - rNorm) / delta) + 2);
                }
                else if (max == bNorm)
                {
                    h = 60 * (((rNorm - gNorm) / delta) + 4);
                }
            }

            double s = (max == 0) ? 0 : delta / max;
            double v = max;

            if (h < 0)
            {
                h += 360;
            }

            return (h, s, v);
        }
        private Color HsvToRgb(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(Convert.ToByte(A.Value), v, t, p);
            else if (hi == 1)
                return Color.FromArgb(Convert.ToByte(A.Value), q, v, p);
            else if (hi == 2)
                return Color.FromArgb(Convert.ToByte(A.Value), p, v, t);
            else if (hi == 3)
                return Color.FromArgb(Convert.ToByte(A.Value), p, q, v);
            else if (hi == 4)
                return Color.FromArgb(Convert.ToByte(A.Value), t, p, v);
            else
                return Color.FromArgb(Convert.ToByte(A.Value), v, p, q);
        }

        // Event
        private void ARGB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (handle)
            {
                handle = false;

                (double h, double s, double v) = RgbToHsv(R.Value, G.Value, B.Value);
                H.Value = h;
                S.Value = s;
                V.Value = v;

                Color c = new Color
                {
                    A = Convert.ToByte(A.Value),
                    R = Convert.ToByte(R.Value),
                    G = Convert.ToByte(G.Value),
                    B = Convert.ToByte(B.Value)
                };
                ColorUpdate(c);
                handle = true;
            }
        }
        private void HSV_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (handle)
            {
                handle = false;
                Color c = HsvToRgb(H.Value, S.Value, V.Value);
                c.A = Convert.ToByte(A.Value);
                R.Value = c.R;
                G.Value = c.G;
                B.Value = c.B;
                ColorUpdate(c);
                handle = true;
            }
        }
        private void Hex_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                _ = Convert.ToInt32(e.Text,16).ToString();
            }
            catch
            {
                e.Handled = true;
            }
            
        }
        private void Hex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (handle && hex.Text.Length == 9)
            {
                try
                {
                    Color c = (Color)ColorConverter.ConvertFromString(Value);
                    A.Value = c.A;
                    R.Value = c.R;
                    G.Value = c.G;
                    B.Value = c.B;
                    (double h, double s, double v) = RgbToHsv(R.Value, G.Value, B.Value);
                    H.Value = h;
                    S.Value = s;
                    V.Value = v;
                    ColorUpdate(c);
                }
                catch
                {
                    e.Handled = true;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Picker.IsOpen = true;
        }

        private void Picker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.Print("Ellipse_MouseDown");
            Picker.IsOpen = false;
            e.Handled = true;
        }

        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            SolidColorBrush brush = (SolidColorBrush)btn.Background;
            Color c = brush.Color;
            A.Value = 255;
            R.Value = c.R;
            G.Value = c.G;
            B.Value = c.B;
            (double h, double s, double v) = RgbToHsv(R.Value, G.Value, B.Value);
            H.Value = h;
            S.Value = s;
            V.Value = v;
            ColorUpdate(c);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Value = "#00000000";
            Picker.IsOpen = false;
            e.Handled = true;
        }
    }
}
