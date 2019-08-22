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

namespace OctTools
{
    /// <summary>
    /// frm_Exit.xaml 的交互逻辑
    /// </summary>
    public partial class frm_Exit : Window
    {
        public frm_Exit()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            frm_Main.IsExit = true;
            this.Close();
        }

        private void button_No_Click(object sender, RoutedEventArgs e)
        {
            frm_Main.IsExit = false;
            this.Close();
        }

        double WinWidth_Real = 1600;
        double WinHeight_Real = 900;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            WinWidth_Real = System.Windows.SystemParameters.WorkArea.Width;
            WinHeight_Real = System.Windows.SystemParameters.WorkArea.Height;

            this.Top = 0;
            this.Left = 0;
            this.Width = WinWidth_Real;
            this.Height = WinHeight_Real;

            button_OK.Focus();
        }
    }
}
