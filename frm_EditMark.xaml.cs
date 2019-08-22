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
    /// frm_EditMark.xaml 的交互逻辑
    /// </summary>
    public partial class frm_EditMark : Window
    {
        public bool IsEdit = false;
        public bool IsDele = false;
        public int iMarkID = -1;
        public int iFrameID = -1;
        public string sMarkInfo = "";
        public string sCheckTime = "";

        public frm_EditMark()
        {
            InitializeComponent();
            IsEdit = false;
            IsDele = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_CheckTime.Text = sCheckTime;
            TextBox_Position.Text = iFrameID.ToString();
            TextBox_MarkInfo.Text = sMarkInfo;
            button_Exit.Focus();
        }

        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            TextBox_MarkInfo.Text = TextBox_MarkInfo.Text.Trim();
            if (sMarkInfo != TextBox_MarkInfo.Text)
            {
                sMarkInfo = TextBox_MarkInfo.Text;
                IsEdit = true;
            }
        }

        private void button_Delete_Click(object sender, RoutedEventArgs e)
        {
            string sSql = "";

            System.Windows.MessageBoxResult dr = MessageBox.Show("确定删除标记？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (dr == MessageBoxResult.Yes)
            {
                if (iMarkID != -1)
                {
                    sSql = "Delete From Mark where MarkID = " + iMarkID;
                    int iId = -1;
                    string sErr = "";
                    if (!MyData.MySqlite.WriteData(sSql, false, ref iId, ref sErr))
                    {
                        MyTools.ShowMsg("删除数据时出现错误！", sErr);
                    }
                    else
                    {
                        MyTools.ShowMsg("删除成功！", "");
                    }

                    IsDele = true;
                }
                this.Hide();
            }
        }

        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide(); 
        }
    }
}
