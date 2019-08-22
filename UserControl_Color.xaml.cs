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
    /// UserControl_Color.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_Color : UserControl
    {
        public UserControl_Color()
        {
            InitializeComponent();
        }

        // 定义委托
        public delegate void ColorMouseDoubleClick(object sender, MouseButtonEventArgs e);
        // 定义事件
        public event ColorMouseDoubleClick Color_MouseDoubleClick;
        private void listBox_Color_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Color_MouseDoubleClick != null)
                Color_MouseDoubleClick(sender, e);
        }

    }
}
