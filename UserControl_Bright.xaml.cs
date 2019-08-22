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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OctTools
{
    /// <summary>
    /// UserControl_Bright.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_Bright : UserControl
    {
        public UserControl_Bright()
        {
            InitializeComponent();
        }

        // 定义委托
        public delegate void BrightDecClick(object sender, RoutedEventArgs e);
        // 定义事件
        public event BrightDecClick BrightDec_Click;
        private void button_BrightDec_Click(object sender, RoutedEventArgs e)
        {
            if (BrightDec_Click != null)
                BrightDec_Click(sender, e);
        }

        // 定义委托
        public delegate void BrightAddClick(object sender, RoutedEventArgs e);
        // 定义事件
        public event BrightAddClick BrightAdd_Click;
        private void button_BrightAdd_Click(object sender, RoutedEventArgs e)
        {
            if (BrightAdd_Click != null)
                BrightAdd_Click(sender, e);
        }

        // 定义委托
        public delegate void BrightSlideMouseDown(object sender, MouseButtonEventArgs e);
        // 定义事件
        public event BrightSlideMouseDown BrightSlide_MouseDown;
        private void image_BrightSlide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (BrightSlide_MouseDown != null)
                BrightSlide_MouseDown(sender, e);
        }

        // 定义委托
        public delegate void BrightSlideMouseUp(object sender, MouseButtonEventArgs e);
        // 定义事件
        public event BrightSlideMouseUp BrightSlide_MouseUp;
        private void image_BrightSlide_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (BrightSlide_MouseUp != null)
                BrightSlide_MouseUp(sender, e);
        }

        // 定义委托
        public delegate void BrightSlideMouseMove(object sender, MouseEventArgs e);
        // 定义事件
        public event BrightSlideMouseMove BrightSlide_MouseMove;
        private void image_BrightSlide_MouseMove(object sender, MouseEventArgs e)
        {
            if (BrightSlide_MouseMove != null)
                BrightSlide_MouseMove(sender, e);
        }

       
    }
}
