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
    /// UserControl_ReadFileBar.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_ReadFileBar : UserControl
    {
        int ibarValue = 0;
        public int iBarValue
        {
            get { return ibarValue; }
            set
            {
                if (value <= 0)
                    ibarValue = 0;
                else if (value >= 100)
                    ibarValue = 100;
                else
                    ibarValue = value;

                ProgressBar_ReadFileBar.Value = ibarValue;
            }
        }

        string sfileName = "";
        public string sFileName
        {
            get { return sfileName; }
            set
            {
                sfileName = value;
                textBox_FileName.Text = value;
            }
        }

        public UserControl_ReadFileBar()
        {
            InitializeComponent();
        }
    }
}
