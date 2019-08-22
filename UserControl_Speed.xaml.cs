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
    /// UserControl_Speed.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_Speed : UserControl
    {
        public UserControl_Speed()
        {
            InitializeComponent();
        }

        // 定义委托
        public delegate void FrameDecClick(object sender, RoutedEventArgs e);
        // 定义事件
        public event FrameDecClick FrameDec_Click;
        private void button_FrameDec_Click(object sender, RoutedEventArgs e)
        {
            if (FrameDec_Click != null)
                FrameDec_Click(sender, e);
        }

        // 定义委托
        public delegate void FrameAddClick(object sender, RoutedEventArgs e);
        // 定义事件
        public event FrameAddClick FrameAdd_Click;
        private void button_FrameAdd_Click(object sender, RoutedEventArgs e)
        {
            if (FrameAdd_Click != null)
                FrameAdd_Click(sender, e);
        }

        // 定义委托
        public delegate void FrameSlideMouseDown(object sender, MouseButtonEventArgs e);
        // 定义事件
        public event FrameSlideMouseDown FrameSlide_MouseDown;
        private void image_FrameSlide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (FrameSlide_MouseDown != null)
                FrameSlide_MouseDown(sender, e);
        }

        // 定义委托
        public delegate void FrameSlideMouseUp(object sender, MouseButtonEventArgs e);
        // 定义事件
        public event FrameSlideMouseUp FrameSlide_MouseUp;
        private void image_FrameSlide_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (FrameSlide_MouseUp != null)
                FrameSlide_MouseUp(sender, e);
        }

        // 定义委托
        public delegate void FrameSlideMouseMove(object sender, MouseEventArgs e);
        // 定义事件
        public event FrameSlideMouseMove FrameSlide_MouseMove;
        private void image_FrameSlide_MouseMove(object sender, MouseEventArgs e)
        {
            if (FrameSlide_MouseMove != null)
                FrameSlide_MouseMove(sender, e);
        }


    }
}
