using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BorgWin10WPF
{
    /// <summary>
    /// Interaction logic for UtilitiesWindow.xaml
    /// </summary>
    public partial class UtilitiesWindow : Window
    {
        public UtilitiesWindow()
        {
            InitializeComponent();
            btnCalculate15fps.Click += BtnCalculate15fps_Click;
            Cursor = Cursors.Arrow;
        }

        private void BtnCalculate15fps_Click(object sender, RoutedEventArgs e)
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            int frames = 0;
            
            int.TryParse(txtHour.Text, out hours);
            int.TryParse(txtMinute.Text, out minutes);
            int.TryParse(txtSecond.Text, out seconds);
            int.TryParse(txtFrame.Text, out frames);

            txt15fpsResult.Text = GetFrame(GetMs(hours, minutes, seconds, frames, 29), 15).ToString();
        }

        public static int GetMs(int hours, int minutes, int seconds, int frames, int fps)
        {
            int startms = ((hours * 60) * 60) * 1000;
            startms += (minutes * 60) * 1000;
            startms += seconds * 1000;
            startms += 1000 * (frames / fps);
            return startms;

        }
        public static int GetFrame(int ms, int fps)
        {
            return (ms / 1000) * fps;
        }
    }
}
