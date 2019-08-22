using System;
using System.Collections.Generic;
using System.Data;
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
    /// frm_SearchPatient.xaml 的交互逻辑
    /// </summary>
    public partial class frm_SearchPatient : Window
    {
        public bool IsSelectNew = false;
        public MyData.PatientInfo_Struct Patient_New;
        public MyData.CheckRecord_Struct[] Record_New = null;
        public int iSelectRecordID = -1;
        public bool IsFirst = false;
        public bool IsShowInput = false;
        public string sSearchString = "";
        public Window pfrm_Start;

        DataTable Patient_DataTable;
        DataTable Record_DataTable;

        public frm_SearchPatient()
        {
            InitializeComponent();
            IsFirst = false;             // 由调用程序设定
            sSearchString = "";          // 由调用程序设定

            CreatDataTable();

            IsSelectNew = false;
            Patient_New = new MyData.PatientInfo_Struct();
            Record_New = null;
        }
        private void CreatDataTable()
        {
            Patient_DataTable = new DataTable();
            Patient_DataTable.Columns.Add("FileID");
            Patient_DataTable.Columns.Add("PatientID");
            Patient_DataTable.Columns.Add("Name");
            Patient_DataTable.Columns.Add("Birthday");
            Patient_DataTable.Columns.Add("Sex");
            Patient_DataTable.Columns.Add("Address");
            Patient_DataTable.Columns.Add("Tele");
            Patient_DataTable.Columns.Add("IdentifyID");
            Patient_DataTable.Columns.Add("Memo");

            Record_DataTable = new DataTable();
            Record_DataTable.Columns.Add("RecordID");
            Record_DataTable.Columns.Add("FileID");
            Record_DataTable.Columns.Add("CheckTime", typeof(DateTime));
            Record_DataTable.Columns.Add("Doctor");
            Record_DataTable.Columns.Add("CheckInfo");
            Record_DataTable.Columns.Add("SmallPict");
            Record_DataTable.Columns.Add("BigPict");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.Title = MyData.sVer;
            //pfrm_Start.Owner = this;

            NewSearch();
            if (IsShowInput)
            {
                grid_Input.Visibility = Visibility.Visible;
                button_Clear.Visibility = Visibility.Visible;
                textBox_Input.Text = "张三";
                textBox_Input.Focus();
            }
            else
            {
                grid_Input.Visibility = Visibility.Hidden;
                button_Clear.Visibility = Visibility.Hidden;
                textBox_Input.Text = sSearchString;
                button_Input_Click(sender, e);
            }
        }
        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult mr = MessageBox.Show("是否不选择档案就退出？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
            //if (mr == MessageBoxResult.Yes)
            //{
            //    IsSelectNew = false;
            //    this.Hide();
            //}
            pfrm_Start.Show();
            pfrm_Start.IsEnabled = true;
            pfrm_Start.ShowInTaskbar = true;

            this.Hide();
            if (!IsShowInput)
            {   // 不用输入的时候
                this.Close();
            }
        }
        private void NewSearch()
        {
            // For Test
            //DataTable dt = new DataTable();
            //dt.Columns.Add("FileID");
            //dt.Columns.Add("PatientID");
            //dt.Columns.Add("Name");
            //dt.Columns.Add("Birthday");
            //dt.Columns.Add("Sex");
            //dt.Columns.Add("Address");
            //dt.Columns.Add("Tele");
            //for (int i = 0; i < 10; i++)
            //{
            //    DataRow dr = dt.NewRow();
            //    dr["FileID"] = i.ToString();
            //    dr["PatientID"] = i.ToString();
            //    dr["Name"] = i.ToString();
            //    dr["Birthday"] = DateTime.Now.ToString();
            //    dr["Sex"] = (i % 2 == 0) ? "男" : "女";
            //    dr["Address"] = i.ToString();
            //    dr["Tele"] = i.ToString();
            //    dt.Rows.Add(dr);
            //}


            //ThisPatients = new List<MyData.PatientInfo_Struct>();
            //for (int i = 0; i < 10; i++)
            //{
            //    ThisPatients.Add(new MyData.PatientInfo_Struct()
            //    {
            //        FileID = i.ToString(),
            //        PatientID = i.ToString(),
            //        Name = i.ToString(),
            //        Birthday = DateTime.Now.ToString(),
            //        Sex = (i % 2 == 0) ? "男" : "女",
            //        Address = i.ToString(),
            //        Tele = i.ToString(),
            //    });
            //}

            //datagrid_Patient.ItemsSource = dt.DefaultView;
            //datagrid_Patient.SelectedIndex = 1;

            datagrid_Record.ItemsSource = null;
            datagrid_Patient.ItemsSource = null;
            Record_DataTable.Clear();
            Patient_DataTable.Clear();
            textBox_Input.Text = "";
            textBox_Input.Focus();
        }

        private void button_Clear_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult br = MessageBox.Show("真的清空当前的查询信息？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (br == MessageBoxResult.Yes)
                NewSearch();
        }
        private void button_Input_Click(object sender, RoutedEventArgs e)
        {
            string SqlTmp = "";
            string sInput = textBox_Input.Text.Trim();
            if (textBox_Input.Text.Trim() == "")
            {
                MessageBox.Show("没有输入查询信息，不能查询。", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (textBox_Input.Text.Trim() == "*")
            {
                SqlTmp = "Select FileID, PatientID, Name, Birthday, Sex, Address, Tele, IdentifyID, Memo from Patient";
            }
            else
            {
                SqlTmp = "Select FileID, PatientID, Name, Birthday, Sex, Address, Tele, IdentifyID, Memo from Patient where " +
                    "InStr(PatientID,'" + sInput + "')>0 OR " +
                    "InStr(Name,'" + sInput + "')>0 OR " +
                    "InStr(Sex,'" + sInput + "')>0 OR " +
                    "InStr(Address,'" + sInput + "')>0 OR " +
                    "InStr(Tele,'" + sInput + "')>0 OR " +
                    "InStr(IdentifyID,'" + sInput + "')>0 OR " +
                    "InStr(Memo,'" + sInput + "')>0";

                DateTime dt;
                try
                {
                    dt = DateTime.Parse(sInput);
                    SqlTmp += " OR Birthday = '" + sInput + "' OR " +
                              "FileID In (Select FileID From Record Where (" +
                              "InStr(Doctor,'" + sInput + "')>0  OR " +
                              "InStr(CheckInfo,'" + sInput + "')>0 OR " +
                              "CheckTime = '" + sInput + "' ))";
                }
                catch
                {
                    SqlTmp += " OR " +
                              "FileID In (Select FileID From Record Where (" +
                              "InStr(Doctor,'" + sInput + "')>0  OR " +
                              "InStr(CheckInfo,'" + sInput + "')>0 ))";
                }
            }

            string sErrMsg = "";
            DataSet DS = null;
            try
            {
                Patient_DataTable.Clear();
                if (!MyData.MySqlite.ReadData(SqlTmp, ref DS, ref sErrMsg))
                {
                    MessageBox.Show("检索时出错（" + sErrMsg + ")", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (DS == null || DS.Tables[0].Rows.Count <= 0)
                {
                    MessageBox.Show("没有符合条件的记录。", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                    {
                        DataRow dr = Patient_DataTable.NewRow();
                        dr["FileID"] = DS.Tables[0].Rows[i]["FileID"];
                        dr["PatientID"] = DS.Tables[0].Rows[i]["PatientID"];
                        dr["Name"] = DS.Tables[0].Rows[i]["Name"];
                        dr["Birthday"] = DateTime.Parse(DS.Tables[0].Rows[i]["Birthday"].ToString()).ToString("yyyy-MM-dd");
                        dr["Sex"] = DS.Tables[0].Rows[i]["Sex"];
                        dr["Address"] = DS.Tables[0].Rows[i]["Address"];
                        dr["Tele"] = DS.Tables[0].Rows[i]["Tele"];
                        dr["IdentifyID"] = DS.Tables[0].Rows[i]["IdentifyID"];
                        dr["Memo"] = DS.Tables[0].Rows[i]["Memo"];
                        Patient_DataTable.Rows.Add(dr);
                    }
                    datagrid_Patient.ItemsSource = Patient_DataTable.DefaultView;
                    datagrid_Patient.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("检索时出错（Exception = " + ex.Message + ")", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Error);
                Patient_DataTable.Clear();
                return;
            }

        }
        private void button_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Patient_DataTable.Rows.Count > 0)
            {
                frm_EditPatient Frm_EditPatient = new frm_EditPatient();
                Frm_EditPatient.Patient_DataTable = Patient_DataTable;
                Frm_EditPatient.iSeleID = datagrid_Patient.SelectedIndex;
                Frm_EditPatient.IsDele = false;
                Frm_EditPatient.IsEdit = false;
                Frm_EditPatient.ShowDialog();

                // After call
                if (Frm_EditPatient.IsDele)
                {
                    if (Patient_DataTable.Rows.Count > 0)
                    {
                        datagrid_Patient.ItemsSource = Patient_DataTable.DefaultView;
                        datagrid_Patient.SelectedIndex = Frm_EditPatient.iSeleID;
                    }
                    else
                    {
                        datagrid_Record.ItemsSource = null;
                        datagrid_Patient.ItemsSource = null;
                        Record_DataTable.Clear();
                        Patient_DataTable.Clear();
                    }
                }
                else if (Frm_EditPatient.IsEdit)
                {
                    datagrid_Patient.ItemsSource = Patient_DataTable.DefaultView;
                    datagrid_Patient.SelectedIndex = Frm_EditPatient.iSeleID;
                }

                Frm_EditPatient.Close();
            }
        }
        private void button_Select_Click(object sender, RoutedEventArgs e)
        {
            if (Patient_DataTable.Rows.Count > 0 && datagrid_Patient.SelectedIndex >= 0)
            {
                Patient_New.FileID = Patient_DataTable.Rows[datagrid_Patient.SelectedIndex]["FileID"].ToString();
                Patient_New.PatientID = Patient_DataTable.Rows[datagrid_Patient.SelectedIndex]["PatientID"].ToString();
                Patient_New.Name = Patient_DataTable.Rows[datagrid_Patient.SelectedIndex]["Name"].ToString();
                Patient_New.Birthday = Patient_DataTable.Rows[datagrid_Patient.SelectedIndex]["Birthday"].ToString();
                Patient_New.Sex = Patient_DataTable.Rows[datagrid_Patient.SelectedIndex]["Sex"].ToString();
                Patient_New.Address = Patient_DataTable.Rows[datagrid_Patient.SelectedIndex]["Address"].ToString();
                Patient_New.Tele = Patient_DataTable.Rows[datagrid_Patient.SelectedIndex]["Tele"].ToString();
                Patient_New.IdentifyID = Patient_DataTable.Rows[datagrid_Patient.SelectedIndex]["IdentifyID"].ToString();
                Patient_New.Memo = Patient_DataTable.Rows[datagrid_Patient.SelectedIndex]["Memo"].ToString();

                if (Record_DataTable.Rows.Count > 0)
                {
                    string sTmp = "";
                    Record_New = new MyData.CheckRecord_Struct[Record_DataTable.Rows.Count];
                    for (int i = 0; i < Record_DataTable.Rows.Count; i++)
                    {
                        Record_New[i].RecordID = Record_DataTable.Rows[i]["RecordID"].ToString();
                        Record_New[i].FileID = Record_DataTable.Rows[i]["FileID"].ToString();
                        Record_New[i].CheckTime = Record_DataTable.Rows[i]["CheckTime"].ToString();
                        Record_New[i].Doctor = Record_DataTable.Rows[i]["Doctor"].ToString();
                        Record_New[i].CheckInfo = Record_DataTable.Rows[i]["CheckInfo"].ToString();

                        sTmp = System.Environment.CurrentDirectory + "\\Data\\" + Record_New[i].FileID.Trim() + "\\";
                        Record_New[i].SmallPict = sTmp + "Small\\" + Record_DataTable.Rows[i]["SmallPict"].ToString();
                        Record_New[i].BigPict = sTmp + "Big\\" + Record_DataTable.Rows[i]["BigPict"].ToString();
                        Record_New[i].SelectPict = "";
                    }

                    iSelectRecordID = int.Parse(Record_DataTable.Rows[datagrid_Record.SelectedIndex]["RecordID"].ToString());          // 选定的检查记录的RecordID
                }
                else
                {
                    Record_New = null;
                }

                IsSelectNew = true;

                if (IsShowInput)   // 从frm_Start调用
                {
                    pfrm_Start.Show();
                    pfrm_Start.IsEnabled = true;
                    pfrm_Start.ShowInTaskbar = true;

                    this.Hide();
                }
                else       // 从frm_Start调用
                {
                    frm_Main Frm_Main = new frm_Main();
                    Frm_Main.IsFirst = true;
                    Frm_Main.Patient_New = Patient_New;
                    Frm_Main.Record_New = Record_New;
                    Frm_Main.pfrm_Search = this;

                    this.ShowInTaskbar = false;
                    pfrm_Start.Hide();
                    this.Hide();

                    Frm_Main.ShowDialog();
                }
            }
            else
            {
                Patient_New = new MyData.PatientInfo_Struct();
                Record_New = null;
                IsSelectNew = false;
            }
        }
        private void datagrid_Patient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sSqlTmp = "";
            string sErrMsg = "";
            DataSet DS = null;

            try
            {
                Record_DataTable.Clear();
                if (Patient_DataTable.Rows.Count > 0 && datagrid_Patient.SelectedIndex >= 0)
                {
                    sSqlTmp = "Select * from Record where FileID = " + Patient_DataTable.Rows[datagrid_Patient.SelectedIndex]["FileID"] + "";
                    if (!MyData.MySqlite.ReadData(sSqlTmp, ref DS, ref sErrMsg))
                    {
                        MessageBox.Show("检索时出错（" + sErrMsg + ")", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        Record_DataTable.Clear();
                        return;
                    }

                    if (DS != null && DS.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                        {
                            DataRow dr = Record_DataTable.NewRow();
                            dr["RecordID"] = DS.Tables[0].Rows[i]["RecordID"];
                            dr["FileID"] = DS.Tables[0].Rows[i]["FileID"];
                            dr["CheckTime"] = DS.Tables[0].Rows[i]["CheckTime"];
                            dr["Doctor"] = DS.Tables[0].Rows[i]["Doctor"];
                            dr["CheckInfo"] = DS.Tables[0].Rows[i]["CheckInfo"];
                            dr["SmallPict"] = DS.Tables[0].Rows[i]["SmallPict"];
                            dr["BigPict"] = DS.Tables[0].Rows[i]["BigPict"];
                            Record_DataTable.Rows.Add(dr);
                        }
                    }

                    datagrid_Record.ItemsSource = Record_DataTable.DefaultView;
                    datagrid_Record.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("检索时出错（Exception = " + ex.Message + ")", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Error);
                Record_DataTable.Clear();
                return;
            }
        }

        private void button_RecordEdit_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_Record.SelectedIndex >= 0)
            {
                frm_EditRecord Frm_EditRecord = new frm_EditRecord();

                Frm_EditRecord.sCheckTime_Tmp = DateTime.Parse(Record_DataTable.Rows[datagrid_Record.SelectedIndex]["CheckTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                Frm_EditRecord.sDoctor_Tmp = Record_DataTable.Rows[datagrid_Record.SelectedIndex]["Doctor"].ToString();
                Frm_EditRecord.sCheckInfo_Tmp = Record_DataTable.Rows[datagrid_Record.SelectedIndex]["CheckInfo"].ToString();
                Frm_EditRecord.sRecordID_Tmp = Record_DataTable.Rows[datagrid_Record.SelectedIndex]["RecordID"].ToString();
                Frm_EditRecord.bIsEditCheckInfo = false;
                Frm_EditRecord.bIsDeleCheckInfo = false;

                Frm_EditRecord.ShowDialog();

                if (Frm_EditRecord.bIsEditCheckInfo)
                {
                    Record_DataTable.Rows[datagrid_Record.SelectedIndex]["Doctor"] = Frm_EditRecord.sDoctor_Tmp;
                    Record_DataTable.Rows[datagrid_Record.SelectedIndex]["CheckInfo"] = Frm_EditRecord.sCheckInfo_Tmp;
                }
                else if (Frm_EditRecord.bIsDeleCheckInfo)
                {
                    Record_DataTable.Rows[datagrid_Record.SelectedIndex].Delete();
                }
                Frm_EditRecord.Close();
            }
        }
        private void button_RecordSelect_Click(object sender, RoutedEventArgs e)
        {
            button_Select_Click(sender, e);
        }
        private void Button_Mini_Click(object sender, RoutedEventArgs e)
        {
            pfrm_Start.Hide();
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = true;
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            pfrm_Start.Show();
            pfrm_Start.IsEnabled = false;
            this.Show();
        }
        //增加受检人信息双击打开
        private void Datagrid_Patient_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            button_Select_Click(sender, e);
        }
        //增加诊断信息记录双击打开
        private void Datagrid_Record_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            button_RecordSelect_Click(sender, e);
        }



    }       
}
