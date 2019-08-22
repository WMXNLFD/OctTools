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
using System.Data;

namespace OctTools
{
    /// <summary>
    /// frm_EditPatient.xaml 的交互逻辑
    /// </summary>
    public partial class frm_EditPatient : Window
    {
        public int iSeleID = -1;
        public DataTable Patient_DataTable;
        public bool IsEdit = false;
        public bool IsDele = false;

        MyData.PatientInfo_Struct ThisPatient = new MyData.PatientInfo_Struct();

        public frm_EditPatient()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (iSeleID == -1)
            {
                ExitWindows();
                return;
            }

            ThisPatient.FileID = Patient_DataTable.Rows[iSeleID]["FileID"].ToString();
            ThisPatient.PatientID = Patient_DataTable.Rows[iSeleID]["PatientID"].ToString();
            ThisPatient.Name = Patient_DataTable.Rows[iSeleID]["Name"].ToString();
            ThisPatient.Birthday = DateTime.Parse(Patient_DataTable.Rows[iSeleID]["Birthday"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
            ThisPatient.Sex = Patient_DataTable.Rows[iSeleID]["Sex"].ToString();
            ThisPatient.Address = Patient_DataTable.Rows[iSeleID]["Address"].ToString();
            ThisPatient.Tele = Patient_DataTable.Rows[iSeleID]["Tele"].ToString();
            ThisPatient.IdentifyID = Patient_DataTable.Rows[iSeleID]["IdentifyID"].ToString();
            ThisPatient.Memo = Patient_DataTable.Rows[iSeleID]["Memo"].ToString();

            TextBox_PatientID.Text = ThisPatient.PatientID;
            TextBox_Name.Text = ThisPatient.Name;
            DatePicker_Birth.DisplayDateStart = DateTime.Parse("1900-01-01");
            DatePicker_Birth.DisplayDateEnd = DateTime.Today;
            DatePicker_Birth.Text = ThisPatient.Birthday;

            if (ThisPatient.Sex == "男")
                ComboBox_Sex.SelectedIndex = 0;
            else
                ComboBox_Sex.SelectedIndex = 1;

            TextBox_Addr.Text = ThisPatient.Address;
            TextBox_Tele.Text = ThisPatient.Tele;
            TextBox_IdentifyID.Text = ThisPatient.IdentifyID;
            TextBox_Memo.Text = ThisPatient.Memo;
        }

        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            ExitWindows();
        }

        private void ExitWindows()
        {
            if (IsEdit)
            {
                //ThisPatient.FileID = Patient_DataTable.Rows[iSeleID]["FileID"].ToString();
                Patient_DataTable.Rows[iSeleID]["PatientID"] = ThisPatient.PatientID;
                Patient_DataTable.Rows[iSeleID]["Name"] = ThisPatient.Name;
                Patient_DataTable.Rows[iSeleID]["Birthday"] = ThisPatient.Birthday;
                Patient_DataTable.Rows[iSeleID]["Sex"] = ThisPatient.Sex;
                Patient_DataTable.Rows[iSeleID]["Address"] = ThisPatient.Address;
                Patient_DataTable.Rows[iSeleID]["Tele"] = ThisPatient.Tele;
                Patient_DataTable.Rows[iSeleID]["IdentifyID"] = ThisPatient.IdentifyID;
                Patient_DataTable.Rows[iSeleID]["Memo"] = ThisPatient.Memo;

                string sSql = "";
                string sErr = "";
                int iId = -1;
                try
                {
                    sSql = "Update Patient Set PATIENTID = '" + ThisPatient.PatientID + "'," +
                           "NAME = '" + ThisPatient.Name + "'," +
                           "BIRTHDAY = '" + ThisPatient.Birthday + "'," +
                           "SEX = '" + ThisPatient.Sex + "'," +
                           "ADDRESS = '" + ThisPatient.Address + "'," +
                           "TELE = '" + ThisPatient.Tele + "'," +
                           "IDENTIFYID = '" + ThisPatient.IdentifyID + "'," +
                           "MEMO = '" + ThisPatient.Memo + "'" + 
                           " Where FILEID = " + ThisPatient.FileID;
                    if (!MyData.MySqlite.WriteData(sSql, false, ref iId, ref sErr))
                    {
                        MyTools.ShowMsg("修改Patient表数据时出现错误！", sErr);
                    }
                }
                catch (Exception ex)
                {
                    MyTools.ShowMsg("修改Patient表数据时出现异常错误！", ex.Message );
                }
            }
            this.Hide();
        }

        private void button_Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult RS = MessageBox.Show("确定删除当前受检人员的资料（包括检验记录）？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (RS == MessageBoxResult.Yes)
            {
                string sSql = "";
                string sErr = "";
                int iId = -1;
                try
                {
                    sSql = "Delete From Mark Where FILEID = " + ThisPatient.FileID;
                    if (!MyData.MySqlite.WriteData(sSql, false, ref iId, ref sErr))
                    {
                        MyTools.ShowMsg("删除Mark表数据时出现错误！", sErr);
                    }

                    sSql = "Delete From Record Where FILEID = " + ThisPatient.FileID;
                    sErr = "";
                    iId = -1;
                    if (!MyData.MySqlite.WriteData(sSql, false, ref iId, ref sErr))
                    {
                        MyTools.ShowMsg("删除Record表数据时出现错误！", sErr);
                    }

                    sSql = "Delete From Patient Where FILEID = " + ThisPatient.FileID;
                    sErr = "";
                    iId = -1;
                    if (!MyData.MySqlite.WriteData(sSql, false, ref iId, ref sErr))
                    {
                        MyTools.ShowMsg("删除Patient表数据时出现错误！", sErr);
                    }

                    Patient_DataTable.Rows[iSeleID].Delete();
                }
                catch
                {
                    MyTools.ShowMsg("删除Patient表数据时出现错误！", sErr);
                }
                IsDele = true;
                ExitWindows();
            }
        }

        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_PatientID.Text != ThisPatient.PatientID || TextBox_Name.Text != ThisPatient.Name ||
                DateTime.Parse(DatePicker_Birth.Text) != DateTime.Parse(ThisPatient.Birthday) ||
                ThisPatient.Sex != ComboBox_Sex.SelectionBoxItem.ToString() || TextBox_Addr.Text != ThisPatient.Address ||
                TextBox_Tele.Text != ThisPatient.Tele || TextBox_IdentifyID.Text != ThisPatient.IdentifyID || TextBox_Memo.Text != ThisPatient.Memo)
            {
                MessageBoxResult RS = MessageBox.Show("确定保存当前的资料？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (RS == MessageBoxResult.Yes)
                {
                    ThisPatient.PatientID = TextBox_PatientID.Text;
                    ThisPatient.Name = TextBox_Name.Text;
                    ThisPatient.Birthday = DateTime.Parse(DatePicker_Birth.Text).ToString("yyyy-MM-dd");
                    ThisPatient.Sex = ComboBox_Sex.SelectionBoxItem.ToString();
                    ThisPatient.Address = TextBox_Addr.Text;
                    ThisPatient.Tele = TextBox_Tele.Text;
                    ThisPatient.IdentifyID = TextBox_IdentifyID.Text;
                    ThisPatient.Memo = TextBox_Memo.Text;
                    IsEdit = true;
                }
            }
        }
    }
}
