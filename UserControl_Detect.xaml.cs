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
    /// UserControl_Detect.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_Detect : UserControl
    {
        public UserControl_Detect()
        {
            InitializeComponent();
        }

        // 定义委托
        public delegate void Delegate_Detect_MouseClick(object sender, RoutedEventArgs e);
        // 定义事件
        public event Delegate_Detect_MouseClick Detect_Button_Exit_MouseClick;
        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            if (Detect_Button_Exit_MouseClick != null)
                Detect_Button_Exit_MouseClick(sender, e);
        }

        public delegate void Delegate_Detect_ClearClick(object sender, RoutedEventArgs e);
        // 定义事件
        public event Delegate_Detect_ClearClick Detect_Button_Clear_MouseClick;
        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            if (Detect_Button_Clear_MouseClick != null)
                Detect_Button_Clear_MouseClick(sender, e);
        }

        public delegate void Delegate_Detect_RadioButton_Change(object sender, RoutedEventArgs e);
        // 定义事件
        public event Delegate_Detect_RadioButton_Change Detect_RadioButton_Change;
        private void RadioButton_Line_Checked(object sender, RoutedEventArgs e)
        {
            if (Detect_RadioButton_Change != null)
                Detect_RadioButton_Change(sender, e);
        }
    }
}
