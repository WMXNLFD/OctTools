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
using System.IO;
using System.Threading;

namespace OctTools
{
    /// <summary>
    /// frm_Import.xaml 的交互逻辑
    /// </summary>
    public partial class frm_Import : Window
    {
        string[] sFilePaths;
        public Window pfrm_Start;
        bool IsAnswer = true;
        string sAnswerMode = "";

        public frm_Import()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.Title = MyData.sVer;

            sFilePaths = null;
            RadioButton_NotCover.IsChecked = true;
            RadioButton_Same.IsChecked = true;
            NewSearch();
            button_Search.Focus();
        }
        private void NewSearch()
        {
            sFilePaths = null;
            ListBox_SelectFile.Items.Clear();
            this.progressBar_Read.Value = 0;
            ButtonEnable(true);
            RepeatDisp(Visibility.Hidden);
        }

        private void button_Search_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Scan File (*.gen2,*.gg)|*.gen2;*.gg"
            };

            openFileDialog.Title = "请选择需要加载的数据文件：";
            openFileDialog.Multiselect = true;
            
            var result = openFileDialog.ShowDialog(this);
            if (result == true)
            {
                sFilePaths = openFileDialog.FileNames;
                //this.FileName.Text = System.IO.Path.GetFileName(sFilePath);
                //this.FilePath.Text = System.IO.Path.GetDirectoryName(sFilePath);

                bool IsExist = false;
                foreach (string sFN in sFilePaths)
                {
                    IsExist = false;
                    for (int i = 0; i < ListBox_SelectFile.Items.Count; i++)
                    {
                        if (sFN == ListBox_SelectFile.Items[i].ToString())
                        {
                            IsExist = true;
                            break;
                        }
                    }
                    if (!IsExist)
                    {
                        ListBox_SelectFile.Items.Add(sFN);
                        ListBox_SelectFile.SelectedIndex = ListBox_SelectFile.Items.Count - 1;
                    }
                }
                button_Import.Focus();
            }
            else
            {
                button_Search.Focus();
            }
        }
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox_SelectFile.Items.Count > 0)
            {
                ListBox_SelectFile.Items.RemoveAt(ListBox_SelectFile.SelectedIndex);
            }
        }
        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            ListBox_SelectFile.Items.Clear();
        }
        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            pfrm_Start.Show();
            pfrm_Start.IsEnabled = true;
            pfrm_Start.ShowInTaskbar = true;
            this.Close();
        }
        private void Button_Mini_Click(object sender, RoutedEventArgs e)
        {
            pfrm_Start.Hide();
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = true;
        }

        int iCoverMode_Patient = MyData.iDataCoverMode_NotCover;
        string Ask_Patient = "Ask";
        int iCoverMode_Record = MyData.iDataCoverMode_NotCover;
        string Ask_Record = "Ask";
        private void button_Import_Click(object sender, RoutedEventArgs e)
        {
            string sTmp = "";
            bool bIsExist = false;

            if (ListBox_SelectFile.Items.Count <= 0)                     // 没有选择文件
            {
                MessageBox.Show("没有选择导入文件。", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ButtonEnable(false);

            int iStep = 100 / ListBox_SelectFile.Items.Count;
            for (int i = 0; i < ListBox_SelectFile.Items.Count; i++)
            {
                sTmp = ListBox_SelectFile.Items[i].ToString();
                if (!File.Exists(sTmp))
                {
                    MessageBox.Show("文件 " + sTmp + " 不存在，请检查。" + System.Environment.NewLine + "按任意键继续......", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    continue;
                }

                mod_ReadGen2.Gen2_Struct Gen2_Struct1;
                mod_GG gg1 = null;
                string sErrString = "";
                string sExtendName = System.IO.Path.GetExtension(sTmp);

                try
                {
                    if (sExtendName == ".gen2")
                    {
                        Gen2_Struct1 = new mod_ReadGen2.Gen2_Struct();
                        if (mod_ReadGen2.ReadGen2FromFile(sTmp, ref Gen2_Struct1, ref sErrString) != MyData.iErr_Succ)
                        {
                            MessageBox.Show("读取文件 " + sTmp + " 时出错：" + sErrString + "，请检查！" + System.Environment.NewLine + "按任意键继续......", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            goto ggEend;
                        }
                    }
                    else  // gg File
                    {
                        gg1 = new mod_GG();
                        Gen2_Struct1 = new mod_ReadGen2.Gen2_Struct();
                        if (gg1.ReadggHeader(sTmp, ref sErrString) != MyData.iErr_Succ)
                        {
                            MessageBox.Show("读取文件 " + sTmp + " 时出错：" + sErrString + "，请检查！" + System.Environment.NewLine + "按任意键继续......", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            goto ggEend;
                        }

                        if (!gg1.ggFileMoveToFrame("Top", 0, ref sErrString))
                        {
                            MessageBox.Show("读取文件 " + sTmp + " 时出错：" + sErrString + "，请检查！" + System.Environment.NewLine + "按任意键继续......", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            goto ggEend;
                        }

                        if (!gg1.ReadRawFrameDataFromFile(ref gg1.ggData.m_ggframe_info, ref gg1.ggData.m_lprawdata, ref gg1.ggData.m_lpimagedata, ref sErrString))
                        {
                            MessageBox.Show("读取文件 " + sTmp + " 时出错：" + sErrString + "，请检查！" + System.Environment.NewLine + "按任意键继续......", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            goto ggEend;
                        }

                        if (mod_ReadGen2.ReadGen2FromGGClass(gg1, ref Gen2_Struct1, ref sErrString) != MyData.iErr_Succ)
                        {
                            MessageBox.Show("读取文件 " + sTmp + " 时出错：" + sErrString + "，请检查！" + System.Environment.NewLine + "按任意键继续......", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            goto ggEend;
                        }
                    }

                    if (Ask_Patient == "Ask")
                    {
                        if (MyData.MySqlite.FindPatient(Gen2_Struct1.m_patientid, ref bIsExist, ref sErrString))
                        {
                            if (bIsExist)
                            {
                                IsAnswer = false;
                                sAnswerMode = "";

                                TextBlock_Question.Text = "数据库已存在档案号 = " + Gen2_Struct1.m_patientid + "的资料，请选择：";
                                RepeatDisp(Visibility.Visible);
                                button_Continue.Focus();

                                while (!IsAnswer)
                                {
                                    MyTools.DoEvents();
                                    Thread.Sleep(100);
                                }

                                RepeatDisp(Visibility.Hidden);
                                if (sAnswerMode == "Stop")
                                {
                                    MyTools.ShowMsg("你已中断了导入功能!", "");
                                    ButtonEnable(true);
                                    if (sExtendName == ".gg")
                                    {
                                        gg1.ggClose();
                                    }
                                    return;
                                }
                                else
                                {
                                    if ((bool)RadioButton_Cover.IsChecked)
                                        iCoverMode_Patient = MyData.iDataCoverMode_Cover;
                                    else if ((bool)RadioButton_NotCover.IsChecked)
                                        iCoverMode_Patient = MyData.iDataCoverMode_NotCover;
                                    else
                                        iCoverMode_Patient = MyData.iDataCoverMode_New;

                                    if ((bool)RadioButton_Same.IsChecked)
                                        Ask_Patient = "Same";
                                    else
                                        Ask_Patient = "Ask";
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("检查数据库时出现错误：" + sErrString + "，请检查！" + System.Environment.NewLine + "按任意键继续......", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            goto ggEend;
                        }
                    }

                    if (Ask_Record == "Ask")
                    {
                        if (MyData.MySqlite.FindRecord(Gen2_Struct1.m_patientid, Gen2_Struct1.m_image_datatime, ref bIsExist, ref sErrString))
                        {
                            if (bIsExist)
                            {
                                IsAnswer = false;
                                sAnswerMode = "";

                                TextBlock_Question.Text = Gen2_Struct1.m_patientid + " 的档案已存在检查时间 = " + Gen2_Struct1.m_image_datatime + " 的记录，请选择：";
                                RepeatDisp(Visibility.Visible);
                                button_Continue.Focus();

                                while (!IsAnswer)
                                {
                                    MyTools.DoEvents();
                                    Thread.Sleep(100);
                                }

                                RepeatDisp(Visibility.Hidden);
                                if (sAnswerMode == "Stop")
                                {
                                    MyTools.ShowMsg("你已中断了导入功能!", "");
                                    ButtonEnable(true);
                                    if (sExtendName == ".gg")
                                    {
                                        gg1.ggClose();
                                    }
                                    return;
                                }
                                else
                                {
                                    if ((bool)RadioButton_Cover.IsChecked)
                                        iCoverMode_Record = MyData.iDataCoverMode_Cover;
                                    else if ((bool)RadioButton_NotCover.IsChecked)
                                        iCoverMode_Record = MyData.iDataCoverMode_NotCover;
                                    else
                                        iCoverMode_Record = MyData.iDataCoverMode_New;

                                    if ((bool)RadioButton_Same.IsChecked)
                                        Ask_Record = "Same";
                                    else
                                        Ask_Record = "Ask";
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("检查数据库时出现错误：" + sErrString + "，请检查！" + System.Environment.NewLine + "按任意键继续......", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            goto ggEend;
                        }
                    }

                    // Insert into DB
                    if (mod_ReadGen2.Gen2DataToDB(sTmp, Gen2_Struct1,
                                                        true,                        // Check PatientID
                                                        false,                       // Check Name
                                                        iCoverMode_Patient,
                                                        true,
                                                        iCoverMode_Record,
                                                        ref sErrString) == MyData.iErr_Succ)
                    {
                        //MyTools.ShowMsg("导入成功!", "");
                    }
                    else
                    {
                        MessageBox.Show("导入文件 " + sTmp + " 时出错：" + sErrString + "，请检查！" + System.Environment.NewLine + "按任意键继续......", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    ggEend:
                    if (sExtendName == ".gg")
                    {
                        gg1.ggClose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导入文件 " + sTmp + " 时出现意外错误：" + ex.Message + "，请检查！" + System.Environment.NewLine + "按任意键继续......", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                progressBar_Read.Value += iStep;
                MyTools.DoEvents();
            }
            progressBar_Read.Value = 100;
            MyTools.ShowMsg("已完成所选文件的导入!", "");
            ButtonEnable(true);
        }
        private void ButtonEnable(bool Mode)
        {
            button_Search.IsEnabled = Mode;
            button_Delete.IsEnabled = Mode;
            button_Clear.IsEnabled = Mode;
            button_Import.IsEnabled = Mode;
        }
        private void RepeatDisp(Visibility Mode)
        {
            TextBlock_ImpotText.Visibility = Mode;
            TextBlock_Question.Visibility = Mode;

            TextBlock_FunctionText.Visibility = Mode;
            RadioButton_Cover.Visibility = Mode;
            RadioButton_NotCover.Visibility = Mode;
            RadioButton_Build.Visibility = Mode;

            TextBlock_AfterText.Visibility = Mode;
            RadioButton_Same.Visibility = Mode;
            RadioButton_Ask.Visibility = Mode;

            TextBlock_ButtonText.Visibility = Mode;
            button_Continue.Visibility = Mode;
            button_Stop.Visibility = Mode;
        }
        private void button_Continue_Click(object sender, RoutedEventArgs e)
        {
            IsAnswer = true;
            sAnswerMode = "Continue";
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            pfrm_Start.Show();
            pfrm_Start.IsEnabled = false;
            this.Show();
        }

        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            IsAnswer = true;
            sAnswerMode = "Stop";
        }
    }
}
