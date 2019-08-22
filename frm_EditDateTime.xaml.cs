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
    /// frm_EditDateTime.xaml 的交互逻辑
    /// </summary>
    public partial class frm_EditDateTime : Window
    {
        public string sEditTopic = "";
        public string sEditFiled = "";
        public string sEditText = "";
        public bool bEditOK = false;

        public frm_EditDateTime()
        {
            InitializeComponent();
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window pParentWindow = (Window)this.Parent;
            textBox_Topic.Text = sEditTopic;
            textBox_FieldName.Text = sEditFiled;
            textBox_Edit.Text = sEditText;
            textBox_Edit.Focus();
        }


    }
}
