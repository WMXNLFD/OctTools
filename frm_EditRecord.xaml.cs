using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;

namespace OctTools
{
    /// <summary>
    /// frm_EditRecord.xaml 的交互逻辑
    /// </summary>
    public partial class frm_EditRecord : Window
    {
        public string sRecordID = "";
        public string sCheckTime_Tmp = "";
        public string sDoctor_Tmp = "";
        public string sCheckInfo_Tmp = "";
        public string sRecordID_Tmp = "";
        public bool bIsEditCheckInfo = false;
        public bool bIsDeleCheckInfo = false;

        public frm_EditRecord()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_CheckTime.Text = sCheckTime_Tmp;
            TextBox_Doctor.Text = sDoctor_Tmp;
            TextBox_CheckInfo.Text = sCheckInfo_Tmp;
            bIsEditCheckInfo = false;
            TextBox_Doctor.Focus();
        }

        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            string sSql = "";
            TextBox_Doctor.Text = TextBox_Doctor.Text.Trim();
            TextBox_CheckInfo.Text = TextBox_CheckInfo.Text.Trim();
            if (TextBox_Doctor.Text != sDoctor_Tmp || TextBox_CheckInfo.Text != sCheckInfo_Tmp)
            {
                // 有变化才需要保存
                sSql = "Update Record Set Doctor = '" + TextBox_Doctor.Text + "', CheckInfo = '" + TextBox_CheckInfo.Text + "' where RecordID = " + sRecordID_Tmp;
                int iId = -1;
                string sErr = "";
                if (!MyData.MySqlite.WriteData(sSql, false, ref iId, ref sErr))
                {
                    MyTools.ShowMsg("保存数据时出现错误！", sErr);
                }
                else
                {
                    MyTools.ShowMsg("保存成功！", "");
                }
                sDoctor_Tmp = TextBox_Doctor.Text;
                sCheckInfo_Tmp = TextBox_CheckInfo.Text;
                bIsEditCheckInfo = true;
            }
        }

        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            if ((TextBox_Doctor.Text != sDoctor_Tmp || TextBox_CheckInfo.Text != sCheckInfo_Tmp) && bIsEditCheckInfo == false )
            {
                System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("你修改的数据没有保存，确定要离开？", "温馨提示",MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == System.Windows.Forms.DialogResult.No)
                {
                    e.Handled = false;
                    return;
                }
                else
                {
                    e.Handled = true;
                }
            }
            this.Hide();
        }

        private void button_Delete_Click(object sender, RoutedEventArgs e)
        {
            string sSql = "";
            System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("真的删除检验记录？", "温馨提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                sSql = "Delete From Record where RecordID = " + sRecordID_Tmp;
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

                bIsDeleCheckInfo = true;
                this.Hide();
            }
        }
    }
}
