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
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Windows.Threading;

namespace OctTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class frm_Main : Window
    {
        public bool IsFirst = false;                // 是否第一次进入
        public string sSearchString = "";
        public Window pfrm_Search;
        public static bool IsSelectNew = false;     // 新选择的档案
        public MyData.PatientInfo_Struct Patient_New = new MyData.PatientInfo_Struct();
        public MyData.CheckRecord_Struct[] Record_New;

        // 当前选定的档案
        bool IsSelectCurrent = false;   
        DateTime dSelectDate = DateTime.Now;
        MyData.PatientInfo_Struct Patient_Current;
        MyData.CheckRecord_Struct[] Record_Current;
        DataTable Record_DataTable;
        string[] Record_Time;

        mod_ReadGen2.Gen2_Struct Gen2Current = new mod_ReadGen2.Gen2_Struct();
        mod_GG gg1 = new mod_GG();
        mod_ShowPict PB;
        BitmapImage PatientPict_Current;
        RecordMarkImage[] MarkPict;
        double CutAngle = 0;

        bool SelectDateIsShow = false;

        // 中文拼音比较规则  
        [SQLiteFunction(FuncType = FunctionType.Collation, Name = "PinYin")]
        class SQLiteCollation_PinYin : SQLiteFunction
        {
            public override int Compare(string x, string y)
            {
                return string.Compare(x, y);
            }
        }

        public frm_Main()
        {
            InitializeComponent();
            SQLiteFunction.RegisterFunction(typeof(SQLiteCollation_PinYin));    // 注入  
                                                                                //Config.Reload();

            Patient_New = new MyData.PatientInfo_Struct();
            Record_New = null;

            // 选择日期不可见
            listBox_SelectDate.Visibility = Visibility.Hidden;
            SelectDateIsShow = false;

            IsSelectNew = false;       // 没有选择档案
            IsSelectCurrent = false;   // 没有选择档案
            CreatDataTable();

            DetectLine = null;
            ZoomIsPress = true;
            button_Zoom.Visibility = Visibility.Hidden;
            BigPictIsPress = false;
        }
        private void CreatDataTable()
        {
            Record_DataTable = new DataTable();
            Record_DataTable.Columns.Add("RecordID");
            Record_DataTable.Columns.Add("FileID");
            Record_DataTable.Columns.Add("CheckTime");
            Record_DataTable.Columns.Add("Doctor");
            Record_DataTable.Columns.Add("CheckInfo");
            Record_DataTable.Columns.Add("SmallPict");
            Record_DataTable.Columns.Add("BigPict");
            Record_DataTable.Columns.Add("SelectPict");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Search 模块在有选中时进来
            Patient_Current = Patient_New;
            Record_Current = Record_New;
            SetWindows();

            this.Title = MyData.sVer;
            this.ShowInTaskbar = true;
            this.Show();


            Gen2Current = new mod_ReadGen2.Gen2_Struct();
            PB = new mod_ShowPict();
            PB.RecordPictWidth = (int)Grid_Record.ActualWidth;
            iDelay = MyData.DelayDefault;
            CutAngle = MyData.AngleDefault;

            IsSelectCurrent = true;
            if (Record_Current != null && Record_Current.Length > 0)
            {
                GetCheckTimeToArry(Record_Current, ref Record_Time);
                dSelectDate = DateTime.Parse(Record_Time[0]);
            }
            else
            {
                Record_Time = null;
                dSelectDate = DateTime.Now;        // 取当天时间
            }

            ChangSelectDate();
            DispPatient();
        }

        //double WinWidth_Designed = 1600;
        //double WinHeight_Designed = 900;
        double WinWidth_Real = 1600;
        double WinHeight_Real = 900;
        double WinStartTop = 0;
        double WinStartLeft = 0;

        private void SetWindows()
        {
            label_Logo.Content = "——  " + MyData.sVer;
            this.Title = MyData.sVer;

            WinStartTop = this.Top;
            WinStartLeft = this.Left;

            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            WinWidth_Real = System.Windows.SystemParameters.WorkArea.Width;
            WinHeight_Real = System.Windows.SystemParameters.WorkArea.Height;

            this.Top = 0;
            this.Left = 0;
            this.Width = WinWidth_Real;
            this.Height = WinHeight_Real;

            if (this.grid_SmallPict.Width <= this.grid_SmallPict.Height)
            {
                this.image_SmallPict.Width = this.grid_SmallPict.Width - 10;
                this.image_SmallPict.Height = this.grid_SmallPict.Width - 10;
            }
            else
            {
                this.image_SmallPict.Width = this.grid_SmallPict.Height - 10;
                this.image_SmallPict.Height = this.grid_SmallPict.Height - 10;
            }
            this.image_SmallPict.Margin = new Thickness((this.grid_SmallPict.ActualWidth - this.image_SmallPict.ActualWidth) / 2,
                                                        (this.grid_SmallPict.ActualHeight - this.image_SmallPict.ActualHeight) / 2, 0, 0);

            if (this.grid_BigPict.ActualWidth <= this.grid_BigPict.ActualHeight)
            {
                this.Canvas_BigPict1.Width = this.grid_BigPict.ActualWidth - 10;
                this.Canvas_BigPict1.Height = this.grid_BigPict.ActualWidth - 10;
                this.Canvas_BigPict2.Width = this.grid_BigPict.ActualWidth - 10;
                this.Canvas_BigPict2.Height = this.grid_BigPict.ActualWidth - 10;
                this.image_BigPict.Width = this.grid_BigPict.ActualWidth - 10;
                this.image_BigPict.Height = this.grid_BigPict.ActualWidth - 10;
            }
            else
            {
                this.Canvas_BigPict1.Width = this.grid_BigPict.ActualHeight - 10;
                this.Canvas_BigPict1.Height = this.grid_BigPict.ActualHeight - 10;
                this.Canvas_BigPict2.Width = this.grid_BigPict.ActualHeight - 10;
                this.Canvas_BigPict2.Height = this.grid_BigPict.ActualHeight - 10;
                this.image_BigPict.Width = this.grid_BigPict.ActualHeight - 10;
                this.image_BigPict.Height = this.grid_BigPict.ActualHeight - 10;
            }

            DrawCutLine(-1);
            DrawRectanglePict(-1);

            DrawBigPictRules();
            DrawRecordRule();
        }

        public static bool IsExit = false;
        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            pfrm_Search.Show();
            pfrm_Search.ShowInTaskbar = true;
            this.Hide();
            this.Close();
        }
        private void button_Mini_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DispPatient()
        {
            string sTmp = "";
            DateTime dt;

            if (IsSelectCurrent)
            {
                image_Logo.Visibility = Visibility.Hidden;
                //button_Import.Visibility = Visibility.Collapsed;
                //button_Output.Visibility = Visibility.Visible;
                //button_Detect.Visibility = Visibility.Collapsed;
                //button_SavePict.Visibility = Visibility.Visible;
                //button_Compare.Visibility = Visibility.Visible;
                //button_Bright.Visibility = Visibility.Visible;
                //button_Color.Visibility = Visibility.Visible;
                //button_Setup.Visibility = Visibility.Collapsed;

                grid_Control.Visibility = Visibility.Visible;

                //Grid_PatientInfo2.Visibility = Visibility.Hidden;
                Grid_PatientInfo.Visibility = Visibility.Visible;

                button_Zoom.Visibility = Visibility.Hidden;
                ZoomIsPress = false;
                BigPictIsPress = false;
                button_Zoom_Click();

                textBox_CurrentPatient.Content = "档案号：" + Patient_Current.PatientID;
                sTmp = MyTools.GetStringSubLen(Patient_Current.Name,0,16).Trim();
                dt = DateTime.Parse(Patient_Current.Birthday);
                label_PatientInfo.Content = sTmp + "，" + Patient_Current.Sex.Trim() + "，" + (DateTime.Now.Year - DateTime.Parse(Patient_Current.Birthday).Year).ToString() + "岁";

                listBox_SelectDate.Visibility = Visibility.Hidden;
                SelectDateIsShow = false;

                //textBox_CurrentPatient.Content = "档案号：未选择";
                //label_PatientIfor.Content = "";
                //listBox_CheckRecord.Items.Clear();
                //button_SearchPatient.IsEnabled = false;

                    //label_CheckInfo.Content = "";
                    //textBox_CheckInfo.Text = "";
            }
            else  // 没有档案
            {
                image_Logo.Visibility = Visibility.Visible;
                //button_Import.Visibility = Visibility.Collapsed;
                button_Output.Visibility = Visibility.Collapsed;
                button_Detect.Visibility = Visibility.Collapsed;
                button_SavePict.Visibility = Visibility.Collapsed;
                button_Compare.Visibility = Visibility.Collapsed;
                button_Bright.Visibility = Visibility.Collapsed;
                button_Color.Visibility = Visibility.Collapsed;
                button_Speed.Visibility = Visibility.Collapsed;
                //button_Setup.Visibility = Visibility.Collapsed;
                //BitmapImage BigSource = new BitmapImage(new Uri("Pict/YSD-TLOGO.PNG", UriKind.Relative));
                //image_BigPict.Stretch = Stretch.Uniform;
                image_BigPict.Source = null;

                //ScaleTransform st = new ScaleTransform();
                //st.ScaleX = 0.5;
                //st.ScaleY = 0.5;
                //st.CenterX = image_BigPict.ActualWidth / 2;
                //st.CenterY = image_BigPict.ActualHeight / 2;
                //image_BigPict.RenderTransform = st;

                BitmapImage SmallSource = new BitmapImage(new Uri("Pict/YSDLogo.PNG", UriKind.Relative));
                image_SmallPict.Source = SmallSource;
                CutAngle = MyData.AngleDefault;
                DrawRectanglePict(-1);

                canvas_BigPictLeftRule.Children.Clear();
                canvas_BigPictBottomRule.Children.Clear();

                image_Record.Source = null;
                DrawCutLine(-1);
                canvas_RecordLeftRule.Children.Clear();
                canvas_RecordBottomRule.Children.Clear();
                ClearAllMark();

                Grid_PatientInfo.Visibility = Visibility.Hidden;
                //Grid_PatientInfo2.Visibility = Visibility.Visible;

                button_Zoom.Visibility = Visibility.Hidden;
                grid_Control.Visibility = Visibility.Hidden;
                image_FramePos.Visibility = Visibility.Hidden;
            }
        }

        private void GetCheckTimeToArry(MyData.CheckRecord_Struct[] CR, ref string[] DTArray)
        {
            string sStr = "";
            bool IsExist = false;
            List<string> strList = new List<string>();
            for (int i = 0; i < CR.Length; i++)
            {
                sStr = CR[i].CheckTime;
                IsExist = false;
                foreach (string ss in strList)
                {
                    if (ss == sStr)
                    {
                        IsExist = true;
                        break;
                    }
                }

                if (!IsExist)
                    strList.Add(sStr);
            }
            strList.Sort();
            DTArray = strList.ToArray();
            Array.Reverse(DTArray);
        }
        private void GetCheckRecord_OnDateTime(string DateTimeString, MyData.CheckRecord_Struct[] SourceRecord,  ref DataTable TTable)
        {
            string sTmp = "";
            TTable.Clear();

            DateTime T1 = DateTime.Parse(DateTimeString + " 00:00:00");
            DateTime T2 = DateTime.Parse(DateTimeString + " 23:59:59");
            if (SourceRecord != null && SourceRecord.Length > 0)
            {
                for (int i = 0; i < SourceRecord.Length; i++)
                {
                    DateTime T3 = DateTime.Parse(SourceRecord[i].CheckTime);
                    if (T3 >= T1 && T3 <= T2)
                    {
                        sTmp = System.Environment.CurrentDirectory + "\\Data\\" + SourceRecord[i].FileID.Trim() + "\\";

                        DataRow dr = TTable.NewRow();
                        dr["RecordID"] = SourceRecord[i].RecordID;
                        dr["FileID"] = SourceRecord[i].FileID;
                        dr["CheckTime"] = DateTime.Parse(SourceRecord[i].CheckTime).ToString("yyyy-MM-dd HH:mm:ss").Substring(11, 8);
                        dr["Doctor"] = SourceRecord[i].Doctor;
                        dr["CheckInfo"] = SourceRecord[i].CheckInfo;
                        dr["SmallPict"] = SourceRecord[i].SmallPict;
                        dr["BigPict"] = SourceRecord[i].BigPict;
                        dr["SelectPict"] = "";
                        //Record_DataTable.Rows.Add(dr);
                        TTable.Rows.Add(dr);
                    }
                }
            }
        }

        //private void button_SearchPatient_Click(object sender, RoutedEventArgs e)
        //{
        //    IsSelectNew = false;   // 没有选择档案
        //    Patient_New = new MyData.PatientInfo_Struct();
        //    Record_New = null;
        //    frm_SearchPatient Frm_SearchPatient = new frm_SearchPatient();
        //    Frm_SearchPatient.ShowDialog();

        //    if (Frm_SearchPatient.IsSelectNew)    // 有选中返回
        //    {
        //        IsSelectCurrent = true;
        //        Patient_Current = Frm_SearchPatient.Patient_New;
        //        Record_Current = Frm_SearchPatient.Record_New;
        //        if (Record_Current != null && Record_Current.Length > 0)
        //        {
        //            GetCheckTimeToArry(Record_Current, ref Record_Time);
        //            dSelectDate = DateTime.Parse(Record_Time[0]);
        //        }
        //        else
        //        {
        //            Record_Time = null;
        //            dSelectDate = DateTime.Now;        // 取当天时间
        //        }

        //        ChangSelectDate();
        //        DispPatient();
        //    }
        //    else    // 没选中返回就不变
        //    {
        //        IsSelectCurrent = false;
        //        Patient_Current = new MyData.PatientInfo_Struct();
        //        Record_New = null;
        //        DispPatient();
        //    }
        //    Frm_SearchPatient.Close();
        //}

        private void listBox_CheckRecord_SelectionChanged(object sender, SelectionChangedEventArgs e)        // 观看选定的记录  
        {
            listBox_CheckRecord_SelectionChanged();
        }
        private void listBox_CheckRecord_SelectionChanged()
        {
            string sErr = "";
            string sFileName = "";

            PB.gg1 = null;
            gg1 = null;
            MarkPict = null;
            Detect_Clear();

            if (Record_DataTable.Rows.Count > 0 && listBox_CheckRecord.SelectedIndex >= 0)
            {
                sFileName = Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["BigPict"].ToString();

                if (System.IO.Path.GetExtension(sFileName) == ".gen2")
                {
                    Gen2Current = new mod_ReadGen2.Gen2_Struct();
                    if (mod_ReadGen2.ReadGen2FromFile(sFileName, ref Gen2Current, ref sErr) != MyData.iErr_Succ)
                    {
                        MyTools.ShowMsg("读取数据失败!", sErr);
                        return;
                    }

                    // 两个图像的显示
                    PB.pGen2 = Gen2Current;
                    PB.Gen2ToData();                        // 转变成 PictData
                    PB.Gen2ToPict();                        // 转变成 Pict
                    PatientPict_Current = PB.Pict;
                    image_BigPict.Source = PatientPict_Current;

                    BitmapImage SImage = PatientPict_Current.Clone();
                    image_SmallPict.Source = PatientPict_Current;

                    // 标尺的更改
                    image_Record.Source = null;
                    canvas_RecordLeftRule.Children.Clear();
                    canvas_RecordBottomRule.Children.Clear();

                    DrawBigPictRules();
                    // DrawRecordRule(2,2);           // gen2时，下面的标尺不用画

                    // 下面的控制不能用
                    button_Speed.Visibility = Visibility.Collapsed;
                    SetPlayKey(true);
                    grid_Control.IsEnabled = false;
                    image_Record.Source = null;
                    DrawCutLine(-1);
                    DrawCurrentPos(-1);
                    ClearAllMark();
                }
                else // if ( System.IO.Path.GetExtension(sFileName)== ".gg"  ) 
                {
                    gg1 = new mod_GG();
                    Gen2Current = new mod_ReadGen2.Gen2_Struct();

                    if (gg1.ReadggHeader(sFileName, ref sErr) != MyData.iErr_Succ)
                    {
                        MyTools.ShowMsg("读取数据失败!", "读取gg头数据错误 = " + sErr);
                        return;
                    }

                    Pop_ReadFileBar.IsOpen = true;
                    UserControl_ReadFileBar.sFileName = gg1.ggData.m_szFileName;
                    UserControl_ReadFileBar.iBarValue = 5;
                    MyTools.DoEvents();

                    // 分配给 ReadAllFramDataFromFile 中的进度条幅度 = 90;
                    if (!gg1.ReadAllFramDataFromFile(ref gg1.ggAllFrameInfo, ref gg1.ggAllRawData, ref gg1.ggRecordBeltData, 0, UserControl_ReadFileBar, ref sErr))
                    {
                        Pop_ReadFileBar.StaysOpen = false;
                        Pop_ReadFileBar.IsOpen = false;
                        MyTools.DoEvents();

                        MyTools.ShowMsg("读取数据失败!", "读取gg帧带数据错误 = " + sErr);
                        return;
                    }


                    if (!gg1.ReadFramDataFromMemo(0, ref gg1.ggData.m_ggframe_info, ref gg1.ggData.m_lprawdata, ref gg1.ggData.m_lpimagedata, ref sErr))
                    {
                        MyTools.ShowMsg("读取数据失败!", "读取帧数据失败 = " + sErr);
                        return;
                    }

                    if (mod_ReadGen2.ReadGen2FromGGClass(gg1, ref Gen2Current, ref sErr) != MyData.iErr_Succ)
                    {
                        MyTools.ShowMsg("读取数据失败!", "从gg类读数据失败 = " + sErr);
                        return;
                    }

                    if (!gg1.ReadMarkFromDB(int.Parse(Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["FileID"].ToString()), 
                                           int.Parse(Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["RecordID"].ToString()), 
                                           ref gg1.ggMark, ref sErr) )
                    {
                        MyTools.ShowMsg("读取数据失败!", "读取标记数据失败 = " + sErr);
                        return;
                    }
                    //Gen2Current = gg1.Gen2Array[0];

                    // 两个图像的显示
                    gg1.iCurrentPos = 0;
                    PB.pGen2 = Gen2Current;
                    PB.Gen2ToData();                        // 转变成 PictData
                    PB.Gen2ToPict();
                    PatientPict_Current = PB.Pict;
                    image_BigPict.Source = PatientPict_Current;

                    BitmapImage SImage = PatientPict_Current.Clone();
                    image_SmallPict.Source = PatientPict_Current;

                    UserControl_ReadFileBar.iBarValue = 100;
                    MyTools.DoEvents();

                    // 标尺的更改
                    image_Record.Source = null;
                    canvas_RecordLeftRule.Children.Clear();
                    canvas_RecordBottomRule.Children.Clear();

                    DrawBigPictRules();
                    DrawRecordRule();           // gg时，下面的标尺需要画

                    // 下面的控制能用
                    button_Speed.Visibility = Visibility.Visible;
                    grid_Control.IsEnabled = true;
                    SetPlayKey(true);

                    PB.gg1 = gg1;
                    PB.GGRecordToPict(PB.RecordPictWidth);
                    image_Record.Source = PB.RecordPict;
                    CutAngle = MyData.AngleDefault;
                    DrawCutLine(1);

                    //draw Record Mark
                    ClearAllMark();
                    MarkPict = new RecordMarkImage[gg1.ggData.numberof_frames];
                    DrawMarkPict(-1,"Draw");                             // 全部画出来

                    // Draw Pos
                    DrawCurrentPos(gg1.iCurrentPos);

                    Pop_ReadFileBar.StaysOpen = false;
                    Pop_ReadFileBar.IsOpen = false;
                }

                // SmallPict
                DrawRectanglePict(1);

                button_Output.Visibility = Visibility.Visible;
                button_Detect.Visibility = Visibility.Visible;
                button_SavePict.Visibility = Visibility.Visible;
                button_Compare.Visibility = Visibility.Visible;
                button_Bright.Visibility = Visibility.Visible;
                button_Color.Visibility = Visibility.Visible;
                button_Zoom.Visibility = Visibility.Hidden;

                ZoomIsPress = false;
                BigPictIsPress = false;
                button_Zoom_Click();

                // CheckInfo
                label_CheckInfo.Content = "医生：" + Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["Doctor"].ToString() + "，检查时间：" +
                                          DateTime.Parse(Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["CheckTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss").Substring(11, 8);
                textBox_CheckInfo.Text = Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["CheckInfo"].ToString();
                button_CheckInFoEdit.IsEnabled = true;
            }
            else     // 没有记录
            {
                // 两个图像的显示
                PB.pGen2 = new mod_ReadGen2.Gen2_Struct();
                PatientPict_Current = null;
                image_BigPict.Source = null;

                image_SmallPict.Source = null;
                DrawRectanglePict(-1);

                // 标尺的更改
                image_Record.Source = null;
                canvas_RecordLeftRule.Children.Clear();
                canvas_RecordBottomRule.Children.Clear();

                //int iStepX = (int)(canvas_BigPictBottomRule.ActualWidth / MyData.dBigPictWidth);
                //int iStepY = (int)(canvas_BigPictBottomRule.ActualHeight / MyData.dBigPictWidth);
                //DrawBigPictRules(iStepX, iStepY);
                // DrawRecordRule(2,2);           // gen2时，下面的标尺不用画

                // 下面的控制不能用
                button_Output.Visibility = Visibility.Collapsed;
                button_Detect.Visibility = Visibility.Collapsed;
                button_SavePict.Visibility = Visibility.Collapsed;
                button_Compare.Visibility = Visibility.Collapsed;
                button_Bright.Visibility = Visibility.Collapsed;
                button_Color.Visibility = Visibility.Collapsed;
                button_Speed.Visibility = Visibility.Collapsed;

                SetPlayKey(true);
                grid_Control.IsEnabled = false;

                image_Record.Source = null;
                CutAngle = MyData.AngleDefault;
                DrawCutLine(-1);
                image_FramePos.Visibility = Visibility.Hidden;
                label_CheckInfo.Content = "";
                textBox_CheckInfo.Text = "";

                label_CheckInfo.Content = "没有检查记录。";
                button_CheckInFoEdit.IsEnabled = false;
            }

            // Zoom
            //ScaleTransform st = new ScaleTransform();
            sfr.ScaleX = 1;
            sfr.ScaleY = 1;
            sfr.CenterX = 0;
            sfr.CenterY = 0;
            //image_BigPict.RenderTransform = st;
            translater.X = 0;
            translater.Y = 0;
        }

        private void listBox_CheckRecord_MouseDoubleClick(object sender, MouseButtonEventArgs e)     // 选定记录
        {
            if (listBox_CheckRecord == null || listBox_CheckRecord.SelectedItem == null)
            {
                return;
            }

            if (Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["SelectPict"].ToString() == "")    // 原来没选
            {
                Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["SelectPict"] = "\\Pict\\Ti.png";
            }
            else    // 原来选中，现在需要取消
            {
                Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["SelectPict"] = "";
            }
        }
        private void button_CheckTime_Click(object sender, RoutedEventArgs e)   // 选择时间
        {
            if (SelectDateIsShow)
            {
                listBox_SelectDate.Visibility = Visibility.Hidden;
                SelectDateIsShow = false;
            }
            else
            {
                listBox_SelectDate.Items.Clear();
                if (Record_Time != null && Record_Time.Length > 0 && Record_DataTable.Rows.Count > 0)
                {
                    string sCurrentTime = Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["CheckTime"].ToString();
                    int SelectID = -1;
                    for (int i = 0; i < Record_Time.Length; i++)
                    {
                        listBox_SelectDate.Items.Add(DateTime.Parse(Record_Time[i]).ToString("yyyy-MM-dd"));
                        if (sCurrentTime == Record_Time[i])
                        {
                            SelectID = i;
                        }
                    }
                    listBox_SelectDate.Visibility = Visibility.Visible;
                    listBox_SelectDate.SelectedIndex = SelectID;
                    SelectDateIsShow = true;
                }
                else
                {
                    listBox_SelectDate.Visibility = Visibility.Hidden;
                    SelectDateIsShow = false;
                }
            }
        }
        private void listBox_SelectDate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listBox_SelectDate.SelectedIndex >= 0)
            {
                dSelectDate = DateTime.Parse(Record_Time[listBox_SelectDate.SelectedIndex]);
                ChangSelectDate();

                listBox_SelectDate.Visibility = Visibility.Hidden;
                SelectDateIsShow = false;
            }
        }
        private void ChangSelectDate()
        {
            // 按选定的时间 dSelectDate 检索出检查记录
            Record_DataTable.Rows.Clear();
            listBox_CheckRecord.ItemsSource = null;
            listBox_CheckRecord.Items.Clear();

            GetCheckRecord_OnDateTime(dSelectDate.ToString("yyyy-MM-dd"), Record_Current, ref Record_DataTable);
            listBox_CheckRecord.ItemsSource = Record_DataTable.DefaultView;
            button_CheckTime.Content = dSelectDate.ToString("yyyy-MM-dd");

            if (listBox_CheckRecord.Items.Count > 0)
            {
                listBox_CheckRecord.SelectedIndex = 0;
            }
            else
            {
                listBox_CheckRecord_SelectionChanged();
            }
        }
        private void button_CheckInFoEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Record_DataTable.Rows.Count <= 0)           // 没有记录
                return;

            frm_EditRecord Frm_EditRecord = new frm_EditRecord();

            Frm_EditRecord.sRecordID_Tmp = Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["RecordID"].ToString();
            Frm_EditRecord.sCheckTime_Tmp = DateTime.Parse(button_CheckTime.Content + " " + Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["CheckTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
            Frm_EditRecord.sDoctor_Tmp = Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["Doctor"].ToString();
            Frm_EditRecord.sCheckInfo_Tmp = Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["CheckInfo"].ToString();
            Frm_EditRecord.sRecordID_Tmp = Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["RecordID"].ToString();
            Frm_EditRecord.bIsEditCheckInfo = false;
            Frm_EditRecord.bIsDeleCheckInfo = false;

            Frm_EditRecord.ShowDialog();

            if (Frm_EditRecord.bIsEditCheckInfo)
            {
                Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["Doctor"] = Frm_EditRecord.sDoctor_Tmp;
                Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["CheckInfo"] = Frm_EditRecord.sCheckInfo_Tmp;

                label_CheckInfo.Content = "医生：" + Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["Doctor"].ToString() + "，检查时间：" +
                                          DateTime.Parse(Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["CheckTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss").Substring(11, 8);
                textBox_CheckInfo.Text = Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["CheckInfo"].ToString();

                if (Record_Current.Length > 0)
                {
                    for (int i = 0; i < Record_Current.Length; i++)
                    {
                        if (Frm_EditRecord.sCheckTime_Tmp == DateTime.Parse(Record_Current[i].CheckTime).ToString("yyyy-MM-dd HH:mm:ss"))
                        {
                            Record_Current[i].Doctor = Frm_EditRecord.sDoctor_Tmp;
                            Record_Current[i].CheckInfo = Frm_EditRecord.sCheckInfo_Tmp;
                            break;
                        }
                    }
                }
            }
            else if (Frm_EditRecord.bIsDeleCheckInfo)
            {
                if (Record_Current.Length > 0)
                {
                    MyData.CheckRecord_Struct[] Record_Tmp = new MyData.CheckRecord_Struct[Record_Current.Length - 1];

                    for (int i = 0; i < Record_Current.Length; i++)
                    {
                        if (Frm_EditRecord.sRecordID_Tmp != Record_Current[i].RecordID)
                        {
                            Record_Tmp[i] = Record_Current[i];
                        }
                    }

                    Record_Current = Record_Tmp;
                    Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex].Delete();
                    ChangSelectDate();
                }
            }
            Frm_EditRecord.Close();
        }
        private void button_Compare_Click(object sender, RoutedEventArgs e)
        {
            frm_Compare Frm_Compare = new frm_Compare();
            Frm_Compare.Owner = this;
            Frm_Compare.pfrm_Main = this;
            this.ShowInTaskbar = false;
            this.Hide();
            Frm_Compare.ShowDialog();
        }
        private void button_Import_Click(object sender, RoutedEventArgs e)
        {
            frm_Import Frm_Import = new frm_Import();
            Frm_Import.ShowDialog();
        }

        public static Image SaveImage = null; 
        private void button_SavePict_Click(object sender, RoutedEventArgs e)
        {
            frm_CopyToPng Frm_CopyToPng = new frm_CopyToPng(image_BigPict);
            Frm_CopyToPng.ShowDialog();
        }

        /// <summary>
        /// 绘画标尺 =====================================================
        /// </summary>
        private void DrawBigPictRules()
        {
            SolidColorBrush SC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));         // ("#FF696969")   ("#FF797979")
            double UnitPoint_X = Canvas_BigPict1.ActualWidth / (MyData.dBigPictWidth / MyData.dAirRate);
            UnitPoint_X *= sfr.ScaleX;
            double UnitPoint_Y = Canvas_BigPict1.ActualHeight / (MyData.dBigPictWidth / MyData.dAirRate);
            UnitPoint_Y *= sfr.ScaleY;

            // BigPict (X)bottom Rule
            this.canvas_BigPictBottomRule.Children.Clear();
            Line x_axis = new Line();//x轴
            x_axis.Stroke = SC;
            x_axis.StrokeThickness = 2;
            x_axis.X1 = 0;
            x_axis.Y1 = this.canvas_BigPictBottomRule.ActualHeight - 1;
            x_axis.X2 = this.canvas_BigPictBottomRule.ActualWidth;
            x_axis.Y2 = this.canvas_BigPictBottomRule.ActualHeight - 1;
            this.canvas_BigPictBottomRule.Children.Add(x_axis);

            // X轴标尺
            int iPos = 1;
            double iSmallStart = this.canvas_BigPictBottomRule.ActualHeight - 5;
            double iBigStart = this.canvas_BigPictBottomRule.ActualHeight - 10;
            double iEnd = this.canvas_BigPictBottomRule.ActualHeight - 1;
            while (iPos * UnitPoint_X <= this.canvas_BigPictBottomRule.ActualWidth)
            {
                x_axis = new Line();//x轴
                x_axis.Stroke = SC;
                x_axis.StrokeThickness = 1;
                x_axis.X1 = iPos * UnitPoint_X;
                x_axis.X2 = iPos * UnitPoint_X;
                x_axis.Y2 = iEnd;

                if (iPos % 5 == 0)      // 大标注
                    x_axis.Y1 = iBigStart;
                else
                    x_axis.Y1 = iSmallStart;

                this.canvas_BigPictBottomRule.Children.Add(x_axis);

                iPos++;
            }

            // BigPict Y(Left) Rule
            this.canvas_BigPictLeftRule.Children.Clear();
            Line y_axis = new Line();//x轴
                                     //y_axis.Stroke = System.Windows.Media.Brushes.DarkGray;
            y_axis.Stroke = SC;
            y_axis.StrokeThickness = 2;
            y_axis.X1 = 0;
            y_axis.Y1 = 0;
            y_axis.X2 = 0;
            y_axis.Y2 = this.canvas_BigPictLeftRule.ActualHeight;
            this.canvas_BigPictLeftRule.Children.Add(y_axis);

            // Y轴标尺
            iPos = 1;
            iSmallStart = 5;
            iBigStart = 10;
            iEnd = 0;

            while (iPos * UnitPoint_Y <= this.canvas_BigPictLeftRule.ActualHeight)
            {
                y_axis = new Line();//x轴
                y_axis.Stroke = SC;
                y_axis.StrokeThickness = 1;
                y_axis.Y1 = iPos * UnitPoint_Y;
                y_axis.X2 = iEnd;
                y_axis.Y2 = iPos * UnitPoint_Y;

                if (iPos % 5 == 0)      // 大标注
                    y_axis.X1 = iBigStart;
                else
                    y_axis.X1 = iSmallStart;

                this.canvas_BigPictLeftRule.Children.Add(y_axis);

                iPos++;
            }
        }
        private void DrawRecordRule()
        {
            int UnitPoint_X = 10;
            if (gg1.ggData.numberof_frames > 0)
                UnitPoint_X = (int)((canvas_RecordBottomRule.ActualWidth- canvas_RecordLeftRule.ActualWidth) / gg1.ggData.numberof_frames);
            else
                UnitPoint_X = 5;

            double UnitPoint_Y = 2;
            if (canvas_RecordLeftRule.ActualHeight > 0)
                UnitPoint_Y = canvas_RecordLeftRule.ActualHeight / (MyData.dBigPictWidth / MyData.dAirRate);

            SolidColorBrush SC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));

            // Record X(bottom) Rule
            this.canvas_RecordBottomRule.Children.Clear();
            Line Record_x_axis = new Line();//x轴
            Record_x_axis.Stroke = SC;
            Record_x_axis.StrokeThickness = 2;
            Record_x_axis.X1 = 0;
            Record_x_axis.Y1 = 0;
            Record_x_axis.X2 = this.canvas_RecordBottomRule.ActualWidth;
            Record_x_axis.Y2 = 0;
            this.canvas_RecordBottomRule.Children.Add(Record_x_axis);

            // X轴标尺
            int iPos = 0;
            double iSmallStart = 5;
            double iBigStart = 10;
            while (iPos * UnitPoint_X <= this.canvas_RecordBottomRule.ActualWidth)
            {
                Record_x_axis = new Line();//x轴
                Record_x_axis.Stroke = SC;
                Record_x_axis.StrokeThickness = 1;
                Record_x_axis.X1 = canvas_RecordLeftRule.ActualWidth + iPos * UnitPoint_X;
                Record_x_axis.X2 = canvas_RecordLeftRule.ActualWidth + iPos * UnitPoint_X;
                Record_x_axis.Y2 = 0;

                if (iPos % 5 == 0)      // 大标注
                    Record_x_axis.Y1 = iBigStart;
                else
                    Record_x_axis.Y1 = iSmallStart;

                this.canvas_RecordBottomRule.Children.Add(Record_x_axis);

                iPos++;
            }

            // Record Y(Left) Rule
            this.canvas_RecordLeftRule.Children.Clear();
            Line Record_y_axis = new Line();//y轴
            //y_axis.Stroke = System.Windows.Media.Brushes.DarkGray;
            Record_y_axis.Stroke = SC;
            Record_y_axis.StrokeThickness = 2;
            Record_y_axis.X1 = 0;
            Record_y_axis.Y1 = 0;
            Record_y_axis.X2 = 0;
            Record_y_axis.Y2 = this.canvas_RecordLeftRule.ActualHeight;
            this.canvas_RecordLeftRule.Children.Add(Record_y_axis);

            // Y轴标尺
            iPos = 0;
            iSmallStart = 5;
            iBigStart = 10;

            while (iPos * UnitPoint_Y <= this.Grid_Record.ActualHeight/2)
            {
                Record_y_axis = new Line();//x轴              // 上半截
                Record_y_axis.Stroke = SC;
                Record_y_axis.StrokeThickness = 1;
                Record_y_axis.Y1 = canvas_RecordBottomRule.ActualHeight + Grid_Record.ActualHeight/2 - iPos * UnitPoint_Y;
                Record_y_axis.X2 = 0;
                Record_y_axis.Y2 = Record_y_axis.Y1;

                if (iPos % 5 == 0)      // 大标注
                    Record_y_axis.X1 = iBigStart;
                else
                    Record_y_axis.X1 = iSmallStart;
                this.canvas_RecordLeftRule.Children.Add(Record_y_axis);

                Record_y_axis = new Line();//x轴             // 下半截
                Record_y_axis.Stroke = SC;
                Record_y_axis.StrokeThickness = 1;
                Record_y_axis.Y1 = canvas_RecordBottomRule.ActualHeight + Grid_Record.ActualHeight / 2 + iPos * UnitPoint_Y; ;
                Record_y_axis.X2 = 0;
                Record_y_axis.Y2 = Record_y_axis.Y1;

                if (iPos % 5 == 0)      // 大标注
                    Record_y_axis.X1 = iBigStart;
                else
                    Record_y_axis.X1 = iSmallStart;
                this.canvas_RecordLeftRule.Children.Add(Record_y_axis);

                iPos++;
            }
        }

        /// <summary>
        /// 彩色控制 =====================================================
        /// </summary>
        private void button_Color_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_CheckRecord.Items.Count > 0)
            {
                int iSelectMode = 0;
                switch (PB.DisplayMode)
                {
                    case MyData.DisplayMode_Yello:
                        iSelectMode = 0;
                        break;
                    case MyData.DisplayMode_Orig:
                        iSelectMode = 1;
                        break;
                    case MyData.DisplayMode_WhiteGray:
                        iSelectMode = 2;
                        break;
                    case MyData.DisplayMode_BW:
                        iSelectMode = 3;
                        break;
                    case MyData.DisplayMode_IronTable:
                    case MyData.DisplayMode_RainTable:
                    default:
                        iSelectMode = 0;
                        break;
                }

                UserControl_Color.listBox_Color.SelectedIndex = iSelectMode;
                Pop_Color.IsOpen = true;
            }
        }
        private void listBox_Color_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            switch(UserControl_Color.listBox_Color.SelectedIndex)
            {
                case 0:
                    PB.DisplayMode = MyData.DisplayMode_Yello;
                    break;
                case 1:
                    PB.DisplayMode = MyData.DisplayMode_Orig;
                    break;
                case 2:
                    PB.DisplayMode = MyData.DisplayMode_WhiteGray;
                    break;
                case 3:
                    PB.DisplayMode = MyData.DisplayMode_BW;
                    break;
                default:
                    PB.DisplayMode = MyData.DisplayMode_Yello;
                    break;
            }
            SaveCurrentRefe();

            // 两个图像的显示
            PB.Gen2ToPict();
            PatientPict_Current = PB.Pict;
            image_BigPict.Source = PatientPict_Current;

            BitmapImage SImage = PatientPict_Current.Clone();
            image_SmallPict.Source = PatientPict_Current;

            if (gg1 != null)
            {
                PB.GGRecordToPict(PB.RecordPictWidth);
                image_Record.Source = PB.RecordPict;

                DrawCurrentPos(gg1.iCurrentPos);
            }
        }

        /// <summary>
        /// 亮度控制 =====================================================
        /// 亮度、对比度、平滑度在屏幕上的显示范围是 = -100 -- 0 -- 100，共 200 分格 
        /// 但在ShowPict()功能中，变化的范围是： -255 -- 255 共500分格
        /// 所以屏幕上每分隔 = 显示实际值的2.5
        /// </summary>
        double PointsPerGrid = 2.5;              // 屏幕上每分隔的点数
        bool IsBrightSlideMouseDown = false;
        Point Pos0;
        private void button_Bright_Click(object sender, RoutedEventArgs e)
        {
            double dPictWidthEachPoint = (UserControl_Bright.line_BrightBar.X2 - UserControl_Bright.line_BrightBar.X1) / 200;     // Bar上 单位长度的点数

            // Bright Init  图像亮度调整值 (-100, 100) ==================================================
            double dPictValue = PB.PictBrightness;                          // 屏幕刻度值
            UserControl_Bright.label_BrightValue.Content = "亮度：" + Math.Round(dPictValue,0).ToString();

            double dLeft = 100 + PB.PictBrightness;                        // 游标位置 =0时
            Thickness thick = UserControl_Bright.image_BrightSlide.Margin;
            thick.Left = dLeft * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
            UserControl_Bright.image_BrightSlide.Margin = thick;

            // Contract Init  图像对比系数调整 -100, 100  ===============================================
            dPictValue = PB.PictContract;                                                   // 屏幕刻度值
            UserControl_Bright.label_ContractValue.Content = "对比：" + Math.Round(dPictValue, 0).ToString();

            dLeft = 100 + PB.PictContract;                                                  // 游标位置
            thick = UserControl_Bright.image_ContractSlide.Margin;
            thick.Left = dLeft * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
            UserControl_Bright.image_ContractSlide.Margin = thick;

            // Saturation Init  图像平滑系数调整 -100, 100  ===============================================
            dPictValue = PB.PictSaturation;                                                   // 屏幕刻度值
            UserControl_Bright.label_SaturationValue.Content = "平滑：" + Math.Round(dPictValue, 0).ToString();

            dLeft = 100 + PB.PictSaturation;                                                  // 游标位置
            thick = UserControl_Bright.image_SaturationSlide.Margin;
            thick.Left = dLeft * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
            UserControl_Bright.image_SaturationSlide.Margin = thick;

            IsBrightSlideMouseDown = false;
            Pop_Brightness.IsOpen = true;
        }
        private void button_BrightDec_Click(object sender, RoutedEventArgs e)
        {
            double dPictWidthEachPoint = (UserControl_Bright.line_BrightBar.X2 - UserControl_Bright.line_BrightBar.X1) / 200;     // Bar上 单位长度的点数
            double dPictValue = 0;                                                          // 屏幕刻度值
            double dLeft = 0;

            Button ThisButton = (Button)sender;
            Image ThisImageSlide = null;
            
            if (ThisButton.Name == "button_BrightDec")
            {
                dPictValue = PB.PictBrightness;                          // 屏幕刻度值
                dPictValue--;
                if (dPictValue < -100)
                    dPictValue = -100;
                PB.PictBrightness = (int)(dPictValue);
                UserControl_Bright.label_BrightValue.Content = "亮度：" + Math.Round(dPictValue, 0).ToString();

                // 游标
                dLeft = (255 + PB.PictBrightness) / PointsPerGrid;                        // 游标位置
                ThisImageSlide = UserControl_Bright.image_BrightSlide;
            }
            else if (ThisButton.Name == "button_ContractDec")
            {
                dPictValue = PB.PictContract                ;                           // 屏幕刻度值
                dPictValue--;
                if (dPictValue < -100)
                    dPictValue = -100;
                PB.PictContract = (int)dPictValue;
                UserControl_Bright.label_ContractValue.Content = "对比：" + Math.Round(dPictValue, 0).ToString();

                // 游标
                dLeft = 100 + PB.PictContract;                                                  // 游标位置
                ThisImageSlide = UserControl_Bright.image_ContractSlide;
            }
            else  // if (ThisButton.Name == "button_SaturationDec")
            {
                dPictValue = PB.PictSaturation;
                dPictValue--;
                if (dPictValue < -100)
                    dPictValue = -100;
                PB.PictSaturation = (int)dPictValue;
                UserControl_Bright.label_SaturationValue.Content = "平滑：" + Math.Round(dPictValue, 0).ToString();

                // 游标
                dLeft = 100 + PB.PictSaturation;                                                  // 游标位置
                ThisImageSlide = UserControl_Bright.image_SaturationSlide;
            }

            // 保存当前的参数
            SaveCurrentRefe();

            // 画图
            PatientPict_Current = PB.Pict;
            image_BigPict.Source = PatientPict_Current;

            BitmapImage SImage = PatientPict_Current.Clone();
            image_SmallPict.Source = PatientPict_Current;

            if (gg1 != null)
            {
                PB.GGRecordToPict(PB.RecordPictWidth);
                image_Record.Source = PB.RecordPict;

                DrawCurrentPos(gg1.iCurrentPos);
            }


            Thickness thick = ThisImageSlide.Margin;
            thick.Left = dLeft * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
            ThisImageSlide.Margin = thick;
        }
        private void button_BrightAdd_Click(object sender, RoutedEventArgs e)
        {
            double dPictWidthEachPoint = (UserControl_Bright.line_BrightBar.X2 - UserControl_Bright.line_BrightBar.X1) / 200;     // Bar上 单位长度的点数
            double dPictValue = 0;                                                          // 屏幕刻度值
            double dLeft = 0;

            Button ThisButton = (Button)sender;
            Image ThisImageSlide = null;

            if (ThisButton.Name == "button_BrightAdd")
            {
                dPictValue = PB.PictBrightness;                          // 屏幕刻度值
                dPictValue++;
                if (dPictValue > 100)
                    dPictValue = 100;
                PB.PictBrightness = (int)(dPictValue);
                UserControl_Bright.label_BrightValue.Content = "亮度：" + Math.Round(dPictValue, 0).ToString();

                // 游标
                dLeft = 100 + PB.PictBrightness;                        // 游标位置
                ThisImageSlide = UserControl_Bright.image_BrightSlide;
            }
            else if (ThisButton.Name == "button_ContractAdd")
            {
                dPictValue = PB.PictContract;                           // 屏幕刻度值
                dPictValue++;
                if (dPictValue > 100)
                    dPictValue = 100;
                PB.PictContract = (int)dPictValue;
                UserControl_Bright.label_ContractValue.Content = "对比：" + Math.Round(dPictValue, 0).ToString();

                // 游标
                dLeft = 100 + PB.PictContract;                                                  // 游标位置
                ThisImageSlide = UserControl_Bright.image_ContractSlide;
            }
            else  // if (ThisButton.Name == "button_SaturationAdd")
            {
                dPictValue = PB.PictSaturation;
                dPictValue++;
                if (dPictValue > 100)
                    dPictValue = 100;
                PB.PictSaturation = (int)dPictValue;
                UserControl_Bright.label_SaturationValue.Content = "平滑：" + Math.Round(dPictValue, 0).ToString();

                // 游标
                dLeft = 100 + PB.PictSaturation;                                                  // 游标位置
                ThisImageSlide = UserControl_Bright.image_SaturationSlide;
            }
            SaveCurrentRefe();

            // 画图
            PatientPict_Current = PB.Pict;
            image_BigPict.Source = PatientPict_Current;

            BitmapImage SImage = PatientPict_Current.Clone();
            image_SmallPict.Source = PatientPict_Current;

            if (gg1 != null)
            {
                PB.GGRecordToPict(PB.RecordPictWidth);
                image_Record.Source = PB.RecordPict;

                DrawCurrentPos(gg1.iCurrentPos);
            }

            Thickness thick = ThisImageSlide.Margin;
            thick.Left = dLeft * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
            ThisImageSlide.Margin = thick;
        }
        private void image_BrightSlide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsBrightSlideMouseDown = true;

            FrameworkElement fEle = sender as FrameworkElement;
            fEle.CaptureMouse();
            fEle.Cursor = Cursors.Hand;

            Pos0 = e.GetPosition(null);
            //dBrightLeft = image_BrightSlide.Margin.Left + 8;        // 从中心点起计算
        }
        private void image_BrightSlide_MouseMove(object sender, MouseEventArgs e)
        {
            Image ThisImageSlide = null;
            Point Pos1;
            Thickness thick;
            double dBrightLeft_New;
            double dPictWidthEachPoint = (UserControl_Bright.line_BrightBar.X2 - UserControl_Bright.line_BrightBar.X1) / 200;     // Bar上 单位长度的点数
            double dPictValue;

            if (IsBrightSlideMouseDown)
            {
                // 当前鼠标位置
                Pos1 = e.GetPosition(null);

                // 移动值
                double xPos = Pos1.X - Pos0.X;
                if (xPos == 0)                                                                // 没有移动
                    return;

                // 游标
                ThisImageSlide = (Image)sender;
                thick = ThisImageSlide.Margin;                                    // BrightSlide的位置
                xPos += thick.Left;

                if ((xPos + 8) < UserControl_Bright.line_BrightBar.X1)
                    xPos = UserControl_Bright.line_BrightBar.X1 - 8;
                else if ((xPos + 8) > UserControl_Bright.line_BrightBar.X2)
                    xPos = UserControl_Bright.line_BrightBar.X2 - 8;

                thick.Left = xPos;
                ThisImageSlide.Margin = thick;

                // 计算对应值
                dBrightLeft_New = thick.Left + 8 - UserControl_Bright.line_BrightBar.X1;                    // 从游标中心点起计算
                dPictValue = dBrightLeft_New / dPictWidthEachPoint - 100;               // 屏幕刻度值

                if (ThisImageSlide.Name == "image_BrightSlide")
                {
                    PB.PictBrightness = (int)(dPictValue);
                    UserControl_Bright.label_BrightValue.Content = "亮度：" + Math.Round(dPictValue, 0).ToString();
                }
                else if (ThisImageSlide.Name == "image_ContractSlide")
                {
                    PB.PictContract = (int)dPictValue;
                    UserControl_Bright.label_ContractValue.Content = "对比：" + Math.Round(dPictValue, 0).ToString();
                }
                else if (ThisImageSlide.Name == "image_SaturationSlide")
                {
                    PB.PictSaturation = (int)dPictValue;
                    UserControl_Bright.label_SaturationValue.Content = "平滑：" + Math.Round(dPictValue, 0).ToString();
                }

                // 画图
                PatientPict_Current = PB.Pict;
                image_BigPict.Source = PatientPict_Current;

                BitmapImage SImage = PatientPict_Current.Clone();
                image_SmallPict.Source = PatientPict_Current;

                if (gg1 != null)
                {
                    PB.GGRecordToPict(PB.RecordPictWidth);
                    image_Record.Source = PB.RecordPict;

                    DrawCurrentPos(gg1.iCurrentPos);
                }

                Pos0 = Pos1;
                //dBrightLeft = dBrightLeft_New;
            }
        }
        private void image_BrightSlide_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveCurrentRefe();

            IsBrightSlideMouseDown = false;
            FrameworkElement ele = sender as FrameworkElement;
            ele.ReleaseMouseCapture();
        }
        private void SaveCurrentRefe()
        {
            MyData.BrightCurrent = PB.PictBrightness;
            MyData.ContractCurrent = PB.PictContract;
            MyData.SaturationCurrent = PB.PictSaturation;
            MyData.ColorCurrent = PB.DisplayMode;
            MyData.DelayDefault = iDelay;
            MyData.AngleDefault = (int)(CutAngle+0.5);
            string sErr = "";
            MyData.MySqlite.SetRefe(ref sErr);
        }

        /// <summary>
        /// 绘制 Rectangle_Pict   ===================================
        /// </summary>
        bool IsDown_SmallPict = false;
        Point sP0, sP1;
        private void DrawRectanglePict(int iMode )
        {
            if (iMode == -1)     // Clear
            {
                this.Rectangle_Pict.StrokeThickness = 0;
            }
            else
            {
                this.Rectangle_Pict.Width = this.image_SmallPict.ActualWidth;
                this.Rectangle_Pict.Height = this.image_SmallPict.ActualHeight;
                this.Rectangle_Pict.StrokeThickness = 1;

                //Point point = image_SmallPict.TranslatePoint(new Point(), grid_SmallPict);   // 获取SmallPict相对父控件的位置

                //Thickness thick = new Thickness(point.X, point.Y, 0, 0);                     // 开始的时候，Rectangle_Pict与 SmallPict 的位置是重叠的
                this.Rectangle_Pict.Margin = image_SmallPict.Margin;
            }
        }
        private void Rectangle_Pict_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen)                              // 要移动切割线，只有Zoom按键被设置同时测量功能已关闭的时候，本功能才启用
            {
                FrameworkElement fEle = sender as FrameworkElement;
                //fEle.CaptureMouse();                                           // 不需要捕获鼠标
                fEle.Cursor = Cursors.Hand;
            }
        }
        private void Rectangle_Pict_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen)                              // 要移动切割线，只有Zoom按键被设置同时测量功能已关闭的时候，本功能才启用
            {
                FrameworkElement fEle = sender as FrameworkElement;
                //fEle.CaptureMouse();                                           // 不需要捕获鼠标
                fEle.Cursor = Cursors.Arrow;
            }
        }
        private void Rectangle_Pict_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen)                              // 要移动切割线，只有Zoom按键被设置同时测量功能已关闭的时候，本功能才启用
            {
                FrameworkElement fEle = sender as FrameworkElement;
                fEle.CaptureMouse();                                           // 不需要捕获鼠标
                fEle.Cursor = Cursors.Hand;
                IsDown_SmallPict = true;
                sP0 = e.GetPosition(grid_SmallPict);
            }
        }
        private void Rectangle_Pict_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen && IsDown_SmallPict)                              // 要移动切割线，只有Zoom按键被设置同时测量功能已关闭的时候，本功能才启用
            {
                Rectangle_Pict_MouseMove(sender, e);

                FrameworkElement fEle = sender as FrameworkElement;
                fEle.ReleaseMouseCapture();                                           // 不需要捕获鼠标
                IsDown_SmallPict = false;
            }
        }
        private void Rectangle_Pict_MouseMove(object sender, MouseEventArgs e)
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen && IsDown_SmallPict)                              // 要移动切割线，只有Zoom按键被设置同时测量功能已关闭的时候，本功能才启用
            {
                sP1 = e.GetPosition(grid_SmallPict);
                double xPos = (sP1.X - sP0.X);
                double yPos = (sP1.Y - sP0.Y);
                if (xPos == 0 && yPos == 0)                     // 没有移动
                    return;

                double xScale = sfr.ScaleX;             //取出放大系数
                double yScale = sfr.ScaleY;             //取出放大系数

                //获得目标经过变换之后的Rect
                Rect targetRect =
                    Canvas_BigPict2.RenderTransform.TransformBounds(new Rect(Canvas_BigPict2.RenderSize));

                // 获取SmallPict相对父控件的位置
                Point SmallPict_point = image_SmallPict.TranslatePoint(new Point(), grid_SmallPict);   
                Thickness thick = Rectangle_Pict.Margin;

                // X方向的位置
                if (xPos < 0)         // 左移
                {
                    if ((Rectangle_Pict.Margin.Left + xPos) < SmallPict_point.X)
                    {
                        thick.Left = SmallPict_point.X;
                    }
                    else
                        thick.Left += xPos;
                }
                else
                {
                    if ((Rectangle_Pict.Margin.Left + Rectangle_Pict.ActualWidth + xPos) > (SmallPict_point.X + image_SmallPict.ActualWidth))
                    {
                        thick.Left = SmallPict_point.X + image_SmallPict.ActualWidth - Rectangle_Pict.ActualWidth;
                    }
                    else
                    {
                        thick.Left += xPos;
                    }
                }

                // Y方向的位置
                if ((Rectangle_Pict.Margin.Top + yPos) < SmallPict_point.Y)
                {
                    thick.Top = SmallPict_point.Y;
                }
                else if ((Rectangle_Pict.Margin.Top + Rectangle_Pict.ActualHeight + yPos) > (SmallPict_point.Y + image_SmallPict.ActualHeight))
                {
                    thick.Top = SmallPict_point.Y + image_SmallPict.ActualHeight - Rectangle_Pict.ActualHeight;
                }
                else
                {
                    thick.Top += yPos;
                }

                Rectangle_Pict.Margin = thick;

                // BigPict相应的移动
                translater.X = -(Rectangle_Pict.Margin.Left - SmallPict_point.X) * targetRect.Width / image_SmallPict.ActualWidth;
                translater.Y = -(Rectangle_Pict.Margin.Top - SmallPict_point.Y) * targetRect.Height / image_SmallPict.ActualHeight;
                sP0 = sP1;
            }
        }

        /// <summary>
        /// 放大与缩小控制 + 测量入口 ===================================
        /// </summary>
        bool ZoomIsPress = false;
        bool BigPictIsPress = false;
        Point BigPictPos0, BigPictPos1;        // 专为BigPict而设的鼠标位置变量
        struct struct_Lines
        {
            public Line Line1;
            public Ellipse C1, C2;
            public Rectangle R;
            public int SetLinePoint;               // 本次操作是对应线条的那一端： 1=前端, 2=后端 
        }
        List<struct_Lines> DetectLine = null;
        private void button_Zoom_Click(object sender, RoutedEventArgs e)
        {
            button_Zoom_Click();
        }
        private void button_Zoom_Click()
        {
            if (ZoomIsPress)
            {
                ZoomIsPress = false;
                button_Zoom.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                image_BigPict.Cursor = Cursors.Arrow;

                sfr.ScaleX = 1;
                sfr.ScaleY = 1;
                sfr.CenterX = image_BigPict.ActualWidth / 2;
                sfr.CenterY = image_BigPict.ActualHeight / 2;
                image_BigPict.RenderTransform = sfr;
            }
            else
            {
                ZoomIsPress = true;
                button_Zoom.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Cyan"));
                image_BigPict.Cursor = Cursors.Hand;
            }
            BigPictIsPress = false;
        }

        private void image_BigPict_MouseWheel(object sender, MouseWheelEventArgs e)  // 放大与缩小
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen)
            {
                double d = e.Delta / Math.Abs(e.Delta);
                double Rota = 0.05;
                double ThisScale = sfr.ScaleX + d * Rota;

                if (ThisScale < 1 && d < 0)
                {
                    ThisScale = 1;
                    Rota = (ThisScale - sfr.ScaleX) / d;
                }
                else if (ThisScale > 20)
                {
                    return;
                }

                //获取鼠标在缩放之前的目标上的位置
                Point targetZoomFocus1 = e.GetPosition(Canvas_BigPict2);

                //获取目标在缩放之前的Rect
                Rect beforeScaleRect = Canvas_BigPict2.RenderTransform.TransformBounds(new Rect(Canvas_BigPict2.RenderSize));

                //缩放的中心点为左上角（0,0）
                //sfr.ScaleX = _scaleValue;
                //sfr.ScaleY = _scaleValue;
                sfr.ScaleX = ThisScale;
                sfr.ScaleY = ThisScale;

                //获取鼠标在缩放之后的目标上的位置
                Point targetZoomFocus2 = new Point(targetZoomFocus1.X * (1 + d * Rota), targetZoomFocus1.Y * (1 + d * Rota));

                //获取目标在缩放之后的Rect
                Rect afterScaleRect = Canvas_BigPict2.RenderTransform.TransformBounds(new Rect(Canvas_BigPict2.RenderSize));

                //算的缩放前后鼠标的位置间的差
                Vector v = targetZoomFocus2 - targetZoomFocus1;


                //if (afterScaleRect.X - v.X > 0)
                //{
                //    //目标左边界与可视左边界对齐
                //    translater.X = 0;
                //}
                //else if (afterScaleRect.X + afterScaleRect.Width - v.X < Canvas_BigPict2.RenderSize.Width)
                //{
                //    //目标右边界与可视右边界对齐
                //    translater.X = Canvas_BigPict2.RenderSize.Width - afterScaleRect.Size.Width;
                //}
                //else
                {
                    //减去鼠标点在缩放前后之间的差值，实际上就是以鼠标点为中心进行缩放
                    translater.X -= v.X;
                    //translater.X -= 50;
                    //sfr.CenterX -= v.X;
                }

                //if (afterScaleRect.Y - v.Y > 0)
                //{
                //    translater.Y = 0;
                //}
                //else if (afterScaleRect.Y + afterScaleRect.Height - v.Y < Canvas_BigPict2.RenderSize.Height)
                //{
                //    translater.Y = Canvas_BigPict2.RenderSize.Height - afterScaleRect.Size.Height;
                //}
                //else
                {
                    translater.Y -= v.Y;
                }

                // 调整标尺
                DrawBigPictRules();

                // 调整鹰眼
                Rectangle_Pict.Width = image_SmallPict.ActualWidth / ThisScale;
                Rectangle_Pict.Height = image_SmallPict.ActualHeight / ThisScale;

                Point point = image_SmallPict.TranslatePoint(new Point(), grid_SmallPict);   // 获取SmallPict相对父控件的位置
                Thickness thick = Rectangle_Pict.Margin;
                thick.Left = point.X - translater.X * image_SmallPict.ActualWidth / afterScaleRect.Width;
                thick.Top = point.Y - translater.Y * image_SmallPict.ActualHeight / afterScaleRect.Height;
                Rectangle_Pict.Margin = thick;
            }
        }
        private void image_BigPict_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)   // 开始拖动
        {
            if (ZoomIsPress)         // 使用图像移动功能，只有Zoom按键被设置的时候，本功能才启用
            {
                FrameworkElement fEle = sender as FrameworkElement;
                fEle.CaptureMouse();
                //fEle.Cursor = Cursors.Hand;

                BigPictPos0 = e.GetPosition(null);
            }

            if (Pop_Detect.IsOpen)       // 使用测量功能
            {
                Detect_MouseLeftButtonDown(sender,e);
            }
            BigPictIsPress = true;
        }
        private void image_BigPict_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)     // 结束拖动
        {
            if (Pop_Detect.IsOpen && BigPictIsPress)
            {
                Detect_MouseLeftButtonUp(sender, e);
            }

            if (ZoomIsPress)         // 只有Zoom按键被设置的时候，本功能才启用
            {
                BigPictIsPress = false;
                FrameworkElement ele = sender as FrameworkElement;
                ele.ReleaseMouseCapture();
            }
        }
        private void image_BigPict_MouseMove(object sender, MouseEventArgs e)                   // 正在拖动
        {
            if (Pop_Detect.IsOpen && BigPictIsPress)
            {
                Detect_MouseLeftButtonMove(sender, e);
            }
            else  if (ZoomIsPress && BigPictIsPress)         // 只有Zoom按键被设置，同时BigPict被指向的时候，本功能才启用
            {
                // 当前鼠标位置
                BigPictPos1 = e.GetPosition(this);

                // 图形原来位置
                Point Center0 = new Point(sfr.CenterX, sfr.CenterY);

                // 移动值
                //double xPos = (BigPictPos1.X - BigPictPos0.X) * xScale;
                //double yPos = (BigPictPos1.Y - BigPictPos0.Y) * yScale;
                double xPos = (BigPictPos1.X - BigPictPos0.X);
                double yPos = (BigPictPos1.Y - BigPictPos0.Y);
                if (xPos == 0 && yPos == 0)                     // 没有移动
                    return;

                double xScale = sfr.ScaleX;             //取出放大系数
                double yScale = sfr.ScaleY;             //取出放大系数

                //获得目标经过变换之后的Rect
                Rect targetRect =
                    Canvas_BigPict2.RenderTransform.TransformBounds(new Rect(Canvas_BigPict2.RenderSize));

                if (yScale >= 1)    // 放大了
                {
                    if (yPos > 0)  //鼠标下移-->向下拖动
                    {
                        if ((translater.Y + yPos) > 0)
                        {
                            translater.Y = 0;
                        }
                        else
                        {
                            translater.Y += yPos;
                        }
                    }
                    else // yPos < 0  //鼠标上移-->向上拖动
                    {
                        if ((translater.Y + yPos) < (Canvas_BigPict1.Height - targetRect.Height))
                        {
                            translater.Y = Canvas_BigPict1.Height - targetRect.Height;
                        }
                        else
                        {
                            translater.Y += yPos;
                        }
                    }
                }

                if (xScale >= 1)    // 放大了
                {
                    if (xPos > 0)  //鼠标右移-->向右拖动
                    {
                        if ((translater.X + xPos) > 0)
                        {
                            translater.X = 0;
                        }
                        else
                        {
                            translater.X += xPos;
                        }
                    }
                    else // xPos < 0  //鼠标左移-->向左拖动
                    {
                        if ((translater.X + xPos) < (Canvas_BigPict1.Width - targetRect.Width))
                        {
                            translater.X = Canvas_BigPict1.Width - targetRect.Width;
                        }
                        else
                        {
                            translater.X += xPos;
                        }
                    }
                }
                BigPictPos0 = BigPictPos1;

                // 调整鹰眼
                Point point = image_SmallPict.TranslatePoint(new Point(), grid_SmallPict);   // 获取SmallPict相对父控件的位置
                Thickness thick = Rectangle_Pict.Margin;
                thick.Left = point.X - translater.X * image_SmallPict.ActualWidth / targetRect.Width;
                thick.Top = point.Y - translater.Y * image_SmallPict.ActualHeight / targetRect.Height;
                Rectangle_Pict.Margin = thick;
            }
            else
            {
            }
        }

        /// <summary>
        /// Detect位置响应  ===================================
        /// </summary>
        private void Button_Detect_Click(object sender, RoutedEventArgs e)
        {
            Pop_Detect.IsOpen = true;
            Pop_Detect.StaysOpen = true;
            Detect_Clear();
        }
        private void Detect_Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            Detect_Clear();
            Pop_Detect.IsOpen = false;
        }
        private void Detect_Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            Detect_Clear();
        }
        private void Detect_Clear()
        {
            if (DetectLine != null)
            {
                for (int i = DetectLine.Count; i > 0; i--)
                {
                    Line im = Canvas_BigPict2.FindName("DetectLine" + (i - 1).ToString()) as Line;   //找到添加的线
                    if (im != null)//判断是否找到，以免在未添加前就误点了   
                    {
                        Canvas_BigPict2.Children.Remove(im);//移除对应按钮控件   
                        Canvas_BigPict2.UnregisterName("DetectLine" + (i - 1).ToString());//还需要把对用的名字注销掉，否则再次点击Button_Add会报错   
                    };

                    Ellipse ie = Canvas_BigPict2.FindName("DetectBox0" + (i - 1).ToString()) as Ellipse;   //找到第一点的box
                    if (ie != null)//判断是否找到，以免在未添加前就误点了   
                    {
                        Canvas_BigPict2.Children.Remove(ie);//移除对应按钮控件   
                        Canvas_BigPict2.UnregisterName("DetectBox0" + (i - 1).ToString());//还需要把对用的名字注销掉，否则再次点击Button_Add会报错   
                    };

                    ie = Canvas_BigPict2.FindName("DetectBox1" + (i - 1).ToString()) as Ellipse;   //找到第二点的box
                    if (ie != null)//判断是否找到，以免在未添加前就误点了   
                    {
                        Canvas_BigPict2.Children.Remove(ie);//移除对应按钮控件   
                        Canvas_BigPict2.UnregisterName("DetectBox1" + (i - 1).ToString());//还需要把对用的名字注销掉，否则再次点击Button_Add会报错   
                    };

                    Rectangle ir = Canvas_BigPict2.FindName("DetectRect0" + (i - 1).ToString()) as Rectangle;   //找到第一点的box
                    if (ir != null)//判断是否找到，以免在未添加前就误点了   
                    {
                        Canvas_BigPict2.Children.Remove(ir);//移除对应按钮控件   
                        Canvas_BigPict2.UnregisterName("DetectRect0" + (i - 1).ToString());//还需要把对用的名字注销掉，否则再次点击Button_Add会报错   
                    };
                }

            }
            DetectLine = null;
            BigPictIsPress = false;

            UserControl_Detect.TextBox_Length.Text = "";
            UserControl_Detect.TextBox_Width.Text = "";
            UserControl_Detect.TextBox_Perimeter.Text = "";
            UserControl_Detect.TextBox_Area.Text = "";
        }                                                           // 清除由测量动作所建立的东东
        private void Detect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fEle = sender as FrameworkElement;
            fEle.CaptureMouse();
            //fEle.Cursor = Cursors.Hand;
            Point P0 = e.GetPosition(Canvas_BigPict2);       // 相对于Canvas_BigPict2的坐标

            if ((bool)UserControl_Detect.RadioButton_Line.IsChecked)
            {
                if (DetectLine == null)            // 第一次画点
                {
                    DetectLine = new List<struct_Lines>();
                    struct_Lines NewOne = new struct_Lines();

                    NewOne.Line1 = new Line();
                    NewOne.Line1.X1 = P0.X;
                    NewOne.Line1.Y1 = P0.Y;
                    NewOne.Line1.X2 = P0.X;
                    NewOne.Line1.Y2 = P0.Y;
                    NewOne.Line1.Name = "DetectLine0";
                    NewOne.Line1.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.Line1.StrokeThickness = 1;

                    NewOne.C1 = new Ellipse();
                    NewOne.C1.Width = 3;
                    NewOne.C1.Height = 3;
                    NewOne.C1.Name = "DetectBox00";
                    NewOne.C1.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.C1.StrokeThickness = 1;
                    NewOne.C1.Margin = new Thickness(NewOne.Line1.X1 - 1, NewOne.Line1.Y1 - 1, 0, 0);
                    NewOne.C1.MouseDown += new MouseButtonEventHandler(image_BigPict_MouseLeftButtonDown);
                    NewOne.C1.MouseMove += new MouseEventHandler(image_BigPict_MouseMove);
                    NewOne.C1.MouseUp += new MouseButtonEventHandler(image_BigPict_MouseLeftButtonUp);

                    NewOne.C2 = new Ellipse();
                    NewOne.C2.Width = 3;
                    NewOne.C2.Height = 3;
                    NewOne.C2.Name = "DetectBox10";
                    NewOne.C2.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.C2.StrokeThickness = 1;
                    NewOne.C2.Margin = new Thickness(NewOne.Line1.X2 - 1, NewOne.Line1.Y2 - 1, 0, 0);
                    NewOne.C2.MouseDown += new MouseButtonEventHandler(image_BigPict_MouseLeftButtonDown);
                    NewOne.C2.MouseMove += new MouseEventHandler(image_BigPict_MouseMove);
                    NewOne.C2.MouseUp += new MouseButtonEventHandler(image_BigPict_MouseLeftButtonUp);

                    NewOne.SetLinePoint = 2;

                    DetectLine.Add(NewOne);
                    Canvas_BigPict2.Children.Add(DetectLine[0].Line1);
                    Canvas_BigPict2.RegisterName(DetectLine[0].Line1.Name, DetectLine[0].Line1);  //注册名字，以便以后使用
                    Canvas_BigPict2.Children.Add(DetectLine[0].C1);
                    Canvas_BigPict2.RegisterName(DetectLine[0].C1.Name, DetectLine[0].C1);        //注册名字，以便以后使用
                    Canvas_BigPict2.Children.Add(DetectLine[0].C2);
                    Canvas_BigPict2.RegisterName(DetectLine[0].C2.Name, DetectLine[0].C2);        //注册名字，以便以后使用

                    UserControl_Detect.TextBox_Length.Text = "0.000";
                    UserControl_Detect.TextBox_Width.Text = "";
                    UserControl_Detect.TextBox_Perimeter.Text = "";
                    UserControl_Detect.TextBox_Area.Text = "";
                }
                else       // 移动点
                {
                    struct_Lines OldOne = DetectLine[0];
                    if ( (P0.X >= OldOne.Line1.X1-1) && (P0.X <= OldOne.Line1.X1 + 1) && (P0.Y >= OldOne.Line1.Y1 -1) && (P0.Y <= OldOne.Line1.Y1 + 1) )
                    {
                        OldOne.SetLinePoint = 1;
                    }
                    else if ((P0.X >= OldOne.Line1.X2 - 1) && (P0.X <= OldOne.Line1.X2 + 1) && (P0.Y >= OldOne.Line1.Y2 - 1) && (P0.Y <= OldOne.Line1.Y2 + 1))
                    {
                        OldOne.SetLinePoint = 2;
                    }
                    else
                    {
                        // 不是已画好的线
                        OldOne.SetLinePoint = 0;
                    }
                    DetectLine[0] = OldOne;
                }
            }
            else if ((bool)UserControl_Detect.RadioButton_Rectangle.IsChecked)
            {
                if (DetectLine == null)            // 第一次画点
                {
                    DetectLine = new List<struct_Lines>();
                    struct_Lines NewOne = new struct_Lines();

                    NewOne.R = new Rectangle();
                    NewOne.R.Width = 0;
                    NewOne.R.Height = 0;
                    NewOne.R.Margin = new Thickness(P0.X, P0.Y, P0.X, P0.Y) ;
                    NewOne.R.Name = "DetectRect00";
                    NewOne.R.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.R.StrokeThickness = 1;
                    NewOne.R.Margin = new Thickness(P0.X, P0.Y, 0, 0);

                    NewOne.C1 = new Ellipse();
                    NewOne.C1.Width = 3;
                    NewOne.C1.Height = 3;
                    NewOne.C1.Name = "DetectBox00";
                    NewOne.C1.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.C1.StrokeThickness = 1;
                    NewOne.C1.Margin = new Thickness(P0.X - 1, P0.Y - 1, 0, 0);
                    NewOne.C1.MouseDown += new MouseButtonEventHandler(image_BigPict_MouseLeftButtonDown);
                    NewOne.C1.MouseMove += new MouseEventHandler(image_BigPict_MouseMove);
                    NewOne.C1.MouseUp += new MouseButtonEventHandler(image_BigPict_MouseLeftButtonUp);

                    NewOne.C2 = new Ellipse();
                    NewOne.C2.Width = 3;
                    NewOne.C2.Height = 3;
                    NewOne.C2.Name = "DetectBox10";
                    NewOne.C2.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.C2.StrokeThickness = 1;
                    NewOne.C2.Margin = new Thickness(P0.X - 1, P0.Y - 1, 0, 0);
                    NewOne.C2.MouseDown += new MouseButtonEventHandler(image_BigPict_MouseLeftButtonDown);
                    NewOne.C2.MouseMove += new MouseEventHandler(image_BigPict_MouseMove);
                    NewOne.C2.MouseUp += new MouseButtonEventHandler(image_BigPict_MouseLeftButtonUp);

                    NewOne.SetLinePoint = 2;

                    DetectLine.Add(NewOne);
                    Canvas_BigPict2.Children.Add(DetectLine[0].R);
                    Canvas_BigPict2.RegisterName(DetectLine[0].R.Name, DetectLine[0].R);  //注册名字，以便以后使用
                    Canvas_BigPict2.Children.Add(DetectLine[0].C1);
                    Canvas_BigPict2.RegisterName(DetectLine[0].C1.Name, DetectLine[0].C1);        //注册名字，以便以后使用
                    Canvas_BigPict2.Children.Add(DetectLine[0].C2);
                    Canvas_BigPict2.RegisterName(DetectLine[0].C2.Name, DetectLine[0].C2);        //注册名字，以便以后使用

                    UserControl_Detect.TextBox_Length.Text = "0.0000";
                    UserControl_Detect.TextBox_Width.Text = "0.0000";
                    UserControl_Detect.TextBox_Perimeter.Text = "0.0000";
                    UserControl_Detect.TextBox_Area.Text = "0.0000";
                }
                else                               // 移动点
                {
                    struct_Lines OldOne = DetectLine[0];
                    Point D1 = new Point(OldOne.R.Margin.Left, OldOne.R.Margin.Top);
                    Point D2 = new Point(OldOne.R.Margin.Left + OldOne.R.Width, OldOne.R.Margin.Top + OldOne.R.Height);

                    if ((P0.X >= D1.X - 1) && (P0.X <= D1.X + 1) && (P0.Y >= D1.Y - 1) && (P0.Y <= D1.Y + 1))
                    {
                        OldOne.SetLinePoint = 1;
                    }
                    else if ((P0.X >= D2.X - 1) && (P0.X <= D2.X + 1) && (P0.Y >= D2.Y - 1) && (P0.Y <= D2.Y + 1))
                    {
                        OldOne.SetLinePoint = 2;
                    }
                    else
                    {
                        // 不是已画好的线
                        OldOne.SetLinePoint = 0;
                    }
                    DetectLine[0] = OldOne;
                }
            }
            else // 多边形
            {
                // 结束上一条线

                if ((bool)UserControl_Detect.RadioButton_Line.IsChecked)
                {
                    // 计算线长
                }
                else   // Any
                {
                }
            }
        }
        private void Detect_MouseLeftButtonMove(object sender, MouseEventArgs e)
        {
            // 前面已经判断是：Pop_Detect.IsOpen && BigPictIsPress

            // 当前鼠标位置
            Point P1 = e.GetPosition(Canvas_BigPict2); ;

            if ((bool)UserControl_Detect.RadioButton_Line.IsChecked)
            {
                struct_Lines OldOne = DetectLine[0];
                if (OldOne.SetLinePoint != 1 && OldOne.SetLinePoint != 2)
                {
                    return;
                }

                if (OldOne.SetLinePoint == 1)     // 处理前点
                {
                    Point P0 = new Point(DetectLine[0].Line1.X1, DetectLine[0].Line1.Y1);  // 前点有无移动

                    OldOne.Line1.X1 = P1.X;
                    OldOne.Line1.Y1 = P1.Y;
                    Thickness thick = OldOne.C1.Margin;
                    thick.Left = P1.X - 1;
                    thick.Top = P1.Y - 1;
                    OldOne.C1.Margin = thick;
                }
                else // (OldOne.SetLinePoint == 2)  // 后点
                {
                    Point P0 = new Point(DetectLine[0].Line1.X2, DetectLine[0].Line1.Y2);  // 前点有无移动

                    OldOne.Line1.X2 = P1.X;
                    OldOne.Line1.Y2 = P1.Y;
                    Thickness thick = OldOne.C2.Margin;
                    thick.Left = P1.X - 1;
                    thick.Top = P1.Y - 1;
                    OldOne.C2.Margin = thick;
                }

                // 计算线长
                DetectLine[0] = OldOne;

                double LineLength_X = Math.Abs(DetectLine[0].Line1.X2 - DetectLine[0].Line1.X1);
                LineLength_X = (MyData.dBigPictWidth * LineLength_X) / (MyData.dAirRate * Canvas_BigPict1.ActualWidth);           //sfr.ScaleX * 

                double LineLength_Y = Math.Abs(DetectLine[0].Line1.Y2 - DetectLine[0].Line1.Y1);
                LineLength_Y = (MyData.dBigPictWidth * LineLength_Y) / (MyData.dAirRate * Canvas_BigPict1.ActualHeight);         // * sfr.ScaleY 

                double LineLength = Math.Sqrt(Math.Pow(LineLength_X, 2) + Math.Pow(LineLength_Y, 2));
                UserControl_Detect.TextBox_Length.Text = LineLength.ToString("F4");
            }
            else if ((bool)UserControl_Detect.RadioButton_Rectangle.IsChecked)
            {
                Point P0;
                struct_Lines OldOne = DetectLine[0];
                Point D0 = new Point(OldOne.R.Margin.Left, OldOne.R.Margin.Top);
                Point D1 = new Point(OldOne.R.Margin.Left + OldOne.R.Width, OldOne.R.Margin.Top + OldOne.R.Height);

                if (OldOne.SetLinePoint != 1 && OldOne.SetLinePoint != 2)
                {
                    return;
                }

                Thickness thick = OldOne.R.Margin;
                if (OldOne.SetLinePoint == 1)     // 处理前点
                {
                    thick.Left = P1.X;
                    thick.Top = P1.Y;
                    OldOne.R.Margin = thick;
                    OldOne.R.Width = Math.Abs(P1.X - D1.X);
                    OldOne.R.Height = Math.Abs(P1.Y - D1.Y); ;

                    thick = OldOne.C1.Margin;
                    thick.Left = P1.X - 1;
                    thick.Top = P1.Y - 1;
                    OldOne.C1.Margin = thick;
                }
                else // (OldOne.SetLinePoint == 2)  // 后点
                {
                    thick.Right = P1.X;
                    thick.Bottom = P1.Y;
                    OldOne.R.Margin = thick;
                    OldOne.R.Width = Math.Abs(P1.X - D0.X);
                    OldOne.R.Height = Math.Abs(P1.Y - D0.Y); ;

                    thick = OldOne.C2.Margin;
                    thick.Left = P1.X - 1;
                    thick.Top = P1.Y - 1;
                    OldOne.C2.Margin = thick;
                }

                // 计算线长
                DetectLine[0] = OldOne;

                P0 = new Point(OldOne.R.Margin.Left, OldOne.R.Margin.Top);
                P1 = new Point(OldOne.R.Margin.Left + OldOne.R.Width, OldOne.R.Margin.Top + OldOne.R.Height);

                double LineLength = Math.Abs(P1.X - P0.X);
                LineLength = (MyData.dBigPictWidth * LineLength) / (MyData.dAirRate * Canvas_BigPict1.ActualWidth);   // 毫米
                UserControl_Detect.TextBox_Length.Text = LineLength.ToString("F4");

                double LineWidth = Math.Abs(P1.Y - P0.Y);
                LineWidth = (MyData.dBigPictWidth * LineWidth) / (MyData.dAirRate * Canvas_BigPict1.ActualHeight);   // 毫米
                UserControl_Detect.TextBox_Width.Text = LineWidth.ToString("F4");

                double dd = 2 * (LineLength + LineWidth);
                UserControl_Detect.TextBox_Perimeter.Text = dd.ToString("F4");

                dd = LineLength * LineWidth;
                UserControl_Detect.TextBox_Area.Text = dd.ToString("F4");
            }
            else
            {
            }
            MyTools.DoEvents();
        }
        private void Detect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 当前鼠标位置
            Point P1 = e.GetPosition(Canvas_BigPict2);

            if ((bool)UserControl_Detect.RadioButton_Line.IsChecked)
            {
                struct_Lines OldOne = DetectLine[0];
                if (OldOne.SetLinePoint != 1 && OldOne.SetLinePoint != 2)
                {
                    return;
                }

                if (OldOne.SetLinePoint == 1)     // 处理前点
                {
                    Point P0 = new Point(DetectLine[0].Line1.X1, DetectLine[0].Line1.Y1);  // 前点有无移动

                    OldOne.Line1.X1 = P1.X;
                    OldOne.Line1.Y1 = P1.Y;
                    Thickness thick = OldOne.C1.Margin;
                    thick.Left = P1.X - 1;
                    thick.Top = P1.Y - 1;
                    OldOne.C1.Margin = thick;
                }
                else // (OldOne.SetLinePoint == 2)  // 后点
                {
                    Point P0 = new Point(DetectLine[0].Line1.X2, DetectLine[0].Line1.Y2);  // 前点有无移动

                    OldOne.Line1.X2 = P1.X;
                    OldOne.Line1.Y2 = P1.Y;
                    Thickness thick = OldOne.C2.Margin;
                    thick.Left = P1.X - 1;
                    thick.Top = P1.Y - 1;
                    OldOne.C2.Margin = thick;
                }

                // 计算线长
                OldOne.SetLinePoint = 0;
                DetectLine[0] = OldOne;

                double LineLength_X = Math.Abs(DetectLine[0].Line1.X2 - DetectLine[0].Line1.X1);
                LineLength_X = (MyData.dBigPictWidth * LineLength_X) / (MyData.dAirRate * Canvas_BigPict1.ActualWidth);           //sfr.ScaleX * 

                double LineLength_Y = Math.Abs(DetectLine[0].Line1.Y2 - DetectLine[0].Line1.Y1);
                LineLength_Y = (MyData.dBigPictWidth * LineLength_Y) / (MyData.dAirRate * Canvas_BigPict1.ActualHeight);         // * sfr.ScaleY 

                double LineLength = Math.Sqrt(Math.Pow(LineLength_X, 2) + Math.Pow(LineLength_Y, 2));
                UserControl_Detect.TextBox_Length.Text = LineLength.ToString("F4");
            }
            else if ((bool)UserControl_Detect.RadioButton_Rectangle.IsChecked)
            {
                Point P0;
                struct_Lines OldOne = DetectLine[0];
                Point D0 = new Point(OldOne.R.Margin.Left, OldOne.R.Margin.Top);
                Point D1 = new Point(OldOne.R.Margin.Left + OldOne.R.Width, OldOne.R.Margin.Top + OldOne.R.Height);

                if (OldOne.SetLinePoint != 1 && OldOne.SetLinePoint != 2)
                {
                    return;
                }

                Thickness thick = OldOne.R.Margin;
                if (OldOne.SetLinePoint == 1)     // 处理前点
                {
                    thick.Left = P1.X;
                    thick.Top = P1.Y;
                    OldOne.R.Margin = thick;
                    OldOne.R.Width = Math.Abs(P1.X - D1.X);
                    OldOne.R.Height = Math.Abs(P1.Y - D1.Y); ;

                    thick = OldOne.C1.Margin;
                    thick.Left = P1.X - 1;
                    thick.Top = P1.Y - 1;
                    OldOne.C1.Margin = thick;
                }
                else // (OldOne.SetLinePoint == 2)  // 后点
                {
                    thick.Right = P1.X;
                    thick.Bottom = P1.Y;
                    OldOne.R.Margin = thick;
                    OldOne.R.Width = Math.Abs(P1.X - D0.X);
                    OldOne.R.Height = Math.Abs(P1.Y - D0.Y); ;

                    thick = OldOne.C2.Margin;
                    thick.Left = P1.X - 1;
                    thick.Top = P1.Y - 1;
                    OldOne.C2.Margin = thick;
                }
                OldOne.SetLinePoint = 0;

                // 计算线长和面积
                DetectLine[0] = OldOne;
                UserControl_Detect.TextBox_Length.Text = "P1.X = " + P1.X.ToString() + ", P1.Y = " + P1.Y.ToString();

                P0 = new Point(OldOne.R.Margin.Left, OldOne.R.Margin.Top);
                P1 = new Point(OldOne.R.Margin.Left + OldOne.R.Width, OldOne.R.Margin.Top + OldOne.R.Height);

                double LineLength = Math.Abs(P1.X - P0.X);
                LineLength = (MyData.dBigPictWidth * LineLength) / (MyData.dAirRate * Canvas_BigPict1.ActualWidth);   // 毫米
                UserControl_Detect.TextBox_Length.Text = LineLength.ToString("F4");

                double LineWidth = Math.Abs(P1.Y - P0.Y);
                LineWidth = (MyData.dBigPictWidth * LineWidth) / (MyData.dAirRate * Canvas_BigPict1.ActualHeight);   // 毫米
                UserControl_Detect.TextBox_Width.Text = LineWidth.ToString("F4");

                double dd = 2 * LineLength + 2 * LineWidth;
                UserControl_Detect.TextBox_Perimeter.Text = dd.ToString("F4");

                dd = LineLength * LineWidth;
                UserControl_Detect.TextBox_Area.Text = dd.ToString("F4");
            }
            else
            {
            }
        }
        private void Detect_RadioButton_Line_Change(object sender, RoutedEventArgs e)
        {
            Detect_Clear();
        }

        /// <summary>
        /// 播放速度控制  ===================================
        /// </summary>
        bool IsSpeedSlideMouseDown = false;
        public int iDelay = 100;
        public int iDelayMax = 5000;   // Max = 5 second
        public double dCutAngle = 0;
        private void button_Speed_Click(object sender, RoutedEventArgs e)
        {
            // Init  图像位置 (0 -- gg1.ggData.numberof_frames) ==================================================
            double dPictWidthEachPoint = (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1) / gg1.ggData.numberof_frames;     // Bar上 每帧长度
            UserControl_Speed.label_FrameValue.Content = "当前位置：" + gg1.iCurrentPos.ToString() + " 帧";
            Thickness thick = UserControl_Speed.image_FrameSlide.Margin;
            thick.Left = gg1.iCurrentPos * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
            UserControl_Speed.image_FrameSlide.Margin = thick;
            //IsSpeedSlideMouseDown = false;

            // Delay Init ========================================================================================
            dPictWidthEachPoint = (UserControl_Bright.line_BrightBar.X2 - UserControl_Bright.line_BrightBar.X1) / iDelayMax;     // Bar上 每毫秒的宽度
            UserControl_Speed.label_DelayValue.Content = "播放延时：" + iDelay.ToString() + " 毫秒";
            thick = UserControl_Speed.image_DelaySlide.Margin;
            thick.Left = iDelay * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
            UserControl_Speed.image_DelaySlide.Margin = thick;

            // Angle Init: 0, 360  ===============================================
            dPictWidthEachPoint = (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1) / 360;     // Bar上 每角度的宽度
            UserControl_Speed.label_AngleValue.Content = "截面角度：" + dCutAngle.ToString() + " 度";
            thick = UserControl_Speed.image_AngleSlide.Margin;
            thick.Left = dCutAngle * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
            UserControl_Speed.image_AngleSlide.Margin = thick;

            IsSpeedSlideMouseDown = false;
            Pop_Speed.IsOpen = true;
        }
        private void button_FrameDec_Click(object sender, RoutedEventArgs e)
        {
            double dPictWidthEachPoint;     // Bar上 单位长度的点数
            Button ThisButton = (Button)sender;
            Thickness thick;

            if (ThisButton.Name == "button_FrameDec")
            {
                dPictWidthEachPoint = (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1) / gg1.ggData.numberof_frames;     // Bar上 单位长度的点数
                if (gg1.iCurrentPos > 0)
                {
                    gg1.iCurrentPos--;
                    UserControl_Speed.label_FrameValue.Content = "当前位置：" + gg1.iCurrentPos.ToString() + " 帧";
                    thick = UserControl_Speed.image_FrameSlide.Margin;
                    thick.Left = gg1.iCurrentPos * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
                    UserControl_Speed.image_FrameSlide.Margin = thick;
                    ShowFrame(gg1.iCurrentPos);
                }
            }
            else if (ThisButton.Name == "button_DelayDec")
            {
                dPictWidthEachPoint = (UserControl_Bright.line_BrightBar.X2 - UserControl_Bright.line_BrightBar.X1) / iDelayMax;     // Bar上 每毫秒的宽度
                if (iDelay > 0)
                {
                    iDelay--;
                    UserControl_Speed.label_DelayValue.Content = "播放延时：" + iDelay.ToString() + " 毫秒";
                    thick = UserControl_Speed.image_DelaySlide.Margin;
                    thick.Left = iDelay * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
                    UserControl_Speed.image_DelaySlide.Margin = thick;
                }
            }
            else  // if (ThisButton.Name == "button_AngleDec")
            {
                dPictWidthEachPoint = (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1) / 360;     // Bar上 每角度的宽度
                if (dCutAngle > 0)
                {
                    dCutAngle--;
                    UserControl_Speed.label_AngleValue.Content = "截面角度：" + dCutAngle.ToString() + " 度";
                    thick = UserControl_Speed.image_AngleSlide.Margin;
                    thick.Left = dCutAngle * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
                    UserControl_Speed.image_AngleSlide.Margin = thick;

                    string sErrString = "";
                    if (gg1.ReadRecordBeltData(gg1.ggAllRawData, ref gg1.ggRecordBeltData, dCutAngle, ref sErrString))
                    {
                        PB.GGRecordToPict(PB.RecordPictWidth);
                        image_Record.Source = PB.RecordPict;

                        DrawCurrentPos(gg1.iCurrentPos);
                    }
                    else
                    {
                        MyTools.ShowMsg("读取帧数据出错！", sErrString);
                    }
                }
            }
            SaveCurrentRefe();
        }
        private void button_FrameAdd_Click(object sender, RoutedEventArgs e)
        {
            double dPictWidthEachPoint;     // Bar上 单位长度的点数
            Button ThisButton = (Button)sender;
            Thickness thick;

            if (ThisButton.Name == "button_FrameAdd")
            {
                dPictWidthEachPoint = (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1) / gg1.ggData.numberof_frames;     // Bar上 单位长度的点数
                if (gg1.iCurrentPos < gg1.ggData.numberof_frames)
                {
                    gg1.iCurrentPos++;
                    UserControl_Speed.label_FrameValue.Content = "当前位置：" + gg1.iCurrentPos.ToString() + " 帧";
                    thick = UserControl_Speed.image_FrameSlide.Margin;
                    thick.Left = gg1.iCurrentPos * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
                    UserControl_Speed.image_FrameSlide.Margin = thick;
                    ShowFrame(gg1.iCurrentPos);
                }
            }
            else if (ThisButton.Name == "button_DelayAdd")
            {
                dPictWidthEachPoint = (UserControl_Bright.line_BrightBar.X2 - UserControl_Bright.line_BrightBar.X1) / iDelayMax;     // Bar上 每毫秒的宽度
                if (iDelay < iDelayMax)
                {
                    iDelay++;
                    UserControl_Speed.label_DelayValue.Content = "播放延时：" + iDelay.ToString() + " 毫秒";
                    thick = UserControl_Speed.image_DelaySlide.Margin;
                    thick.Left = iDelay * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
                    UserControl_Speed.image_DelaySlide.Margin = thick;
                }
            }
            else  // if (ThisButton.Name == "button_AngleAdd")
            {
                dPictWidthEachPoint = (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1) / 360;     // Bar上 每角度的宽度
                if (dCutAngle < 360)
                {
                    dCutAngle++;
                    UserControl_Speed.label_AngleValue.Content = "截面角度：" + dCutAngle.ToString() + " 度";
                    thick = UserControl_Speed.image_AngleSlide.Margin;
                    thick.Left = dCutAngle * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
                    UserControl_Speed.image_AngleSlide.Margin = thick;

                    string sErrString = "";
                    if (gg1.ReadRecordBeltData(gg1.ggAllRawData, ref gg1.ggRecordBeltData, dCutAngle, ref sErrString))
                    {
                        PB.GGRecordToPict(PB.RecordPictWidth);
                        image_Record.Source = PB.RecordPict;

                        DrawCurrentPos(gg1.iCurrentPos);
                    }
                    else
                    {
                        MyTools.ShowMsg("读取帧数据出错！", sErrString);
                    }
                }
            }
            SaveCurrentRefe();
        }
        private void image_FrameSlide_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsSpeedSlideMouseDown = true;

            FrameworkElement fEle = sender as FrameworkElement;
            fEle.CaptureMouse();
            fEle.Cursor = Cursors.Hand;

            Pos0 = e.GetPosition(null);
            //dBrightLeft = image_BrightSlide.Margin.Left + 8;        // 从中心点起计算
        }
        private void image_FrameSlide_MouseMove(object sender, MouseEventArgs e)
        {
            Image ThisImageSlide = null;
            Point Pos1;
            Thickness thick;
            double dBrightLeft_New;
            double dPictWidthEachPoint = 0;     // Bar上 单位长度的点数

            if (IsSpeedSlideMouseDown)         // 只有按键被，本功能才启用
            {
                // 当前鼠标位置
                Pos1 = e.GetPosition(null);

                // 移动值
                double xPos = Pos1.X - Pos0.X;
                if (xPos == 0)                                                                // 没有移动
                    return;

                // 游标
                ThisImageSlide = (Image)sender;
                thick = ThisImageSlide.Margin;                                    // Slide的位置
                xPos += thick.Left;

                if ((xPos + 8) < UserControl_Speed.line_FrameBar.X1)
                    xPos = UserControl_Speed.line_FrameBar.X1 - 8;
                else if ((xPos + 8) > UserControl_Speed.line_FrameBar.X2)
                    xPos = UserControl_Speed.line_FrameBar.X2 - 8;

                thick.Left = xPos;
                ThisImageSlide.Margin = thick;

                // 计算对应值
                dBrightLeft_New = thick.Left + 8 - UserControl_Speed.line_FrameBar.X1;  // 从游标中心点起计算，游标的X值

                if (ThisImageSlide.Name == "image_FrameSlide")
                {
                    dPictWidthEachPoint = gg1.ggData.numberof_frames / (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1) ;     // Bar上 单位长度的点数

                    int iP = (int)(dBrightLeft_New * dPictWidthEachPoint);
                    if (iP < 0) iP = 0;
                    if (iP >= (int)gg1.ggData.numberof_frames) iP = (int)gg1.ggData.numberof_frames - 1;
                    gg1.iCurrentPos = iP;
                    UserControl_Speed.label_FrameValue.Content = "当前位置：" + gg1.iCurrentPos.ToString() + " 帧";
                    ShowFrame(gg1.iCurrentPos);
                }
                else if (ThisImageSlide.Name == "image_DelaySlide")
                {
                    dPictWidthEachPoint = 5000 / (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1);     // Bar上 单位长度的点数
                    iDelay = (int)(dBrightLeft_New * dPictWidthEachPoint); 
                    UserControl_Speed.label_DelayValue.Content = "播放延时：" + iDelay.ToString() + " 毫秒";
                }
                else if (ThisImageSlide.Name == "image_AngleSlide")
                {
                    dPictWidthEachPoint = 360 / (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1);     // Bar上 单位长度的点数
                    dCutAngle = (int)(dBrightLeft_New * dPictWidthEachPoint);
                    UserControl_Speed.label_AngleValue.Content = "截面角度：" + dCutAngle.ToString() + " 度";

                    string sErrString = "";
                    if (gg1.ReadRecordBeltData(gg1.ggAllRawData, ref gg1.ggRecordBeltData, dCutAngle, ref sErrString))
                    {
                        PB.GGRecordToPict(PB.RecordPictWidth);
                        image_Record.Source = PB.RecordPict;

                        DrawCurrentPos(gg1.iCurrentPos);
                    }
                    else
                    {
                        MyTools.ShowMsg("读取帧数据出错！", sErrString);
                    }
                }

                // 画图
                //PatientPict_Current = PB.Pict;
                //image_BigPict.Source = PatientPict_Current;

                //BitmapImage SImage = PatientPict_Current.Clone();
                //image_SmallPict.Source = PatientPict_Current;

                Pos0 = Pos1;
                //dBrightLeft = dBrightLeft_New;
            }
        }
        private void image_FrameSlide_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveCurrentRefe();

            IsSpeedSlideMouseDown = false;
            FrameworkElement ele = sender as FrameworkElement;
            ele.ReleaseMouseCapture();
        }

        /// <summary>
        /// 播放控制  ===================================
        /// </summary>
        public static bool IsPlayStop = true;
        private void button_Play_Click(object sender, RoutedEventArgs e)
        {
            string sErr = "";
            SetPlayKey(false);
            IsPlayStop = false;

            if (gg1.iCurrentPos == gg1.ggData.numberof_frames -1 )
            {
                gg1.iCurrentPos = 0;
            }
            for (int i = gg1.iCurrentPos; i < gg1.ggData.numberof_frames; i++)
            {
                if (IsPlayStop)   // if pause-key is pressed
                    break;

                gg1.iCurrentPos = i;
                if (!gg1.ReadFramDataFromMemo(i, ref gg1.ggData.m_ggframe_info, ref gg1.ggData.m_lprawdata, ref gg1.ggData.m_lpimagedata, ref sErr))
                {
                    MyTools.ShowMsg("读取数据失败!", "读取帧数据失败 = " + sErr);
                    return;
                }

                if (mod_ReadGen2.ReadGen2FromGGClass(gg1, ref Gen2Current, ref sErr) != MyData.iErr_Succ)
                {
                    MyTools.ShowMsg("读取数据失败!", "从gg类读数据失败 = " + sErr);
                    return;
                }

                PB.pGen2 = Gen2Current;
                PB.Gen2ToData();
                PB.Gen2ToPict();
                PatientPict_Current = PB.Pict;
                image_BigPict.Source = PatientPict_Current;
                DrawCurrentPos(i);
                MyTools.DoEvents();
                System.Threading.Thread.Sleep(iDelay);
            }
            IsPlayStop = true;
            SetPlayKey(true);
        }
        private void button_Pause_Click(object sender, RoutedEventArgs e)
        {
            IsPlayStop = true;  // IsPlayStop = true;
        }
        private void button_First_Click(object sender, RoutedEventArgs e)
        {
            gg1.iCurrentPos = 0;
            ShowFrame(gg1.iCurrentPos);
        }
        private void button_Back_Click(object sender, RoutedEventArgs e)
        {
            if (gg1.iCurrentPos > 0)
            {
                gg1.iCurrentPos--;
                ShowFrame(gg1.iCurrentPos);
            }
        }
        private void button_Right_Click(object sender, RoutedEventArgs e)
        {
            if (gg1.iCurrentPos < gg1.ggData.numberof_frames - 1 )
            {
                gg1.iCurrentPos++;
                ShowFrame(gg1.iCurrentPos);
            }
        }
        private void button_End_Click(object sender, RoutedEventArgs e)
        {
            gg1.iCurrentPos = (int)(gg1.ggData.numberof_frames - 1);
            ShowFrame(gg1.iCurrentPos);
        }
        private void ShowFrame(int iFrameID)
        {
            string sErr = "";
            if (!gg1.ReadFramDataFromMemo(iFrameID, ref gg1.ggData.m_ggframe_info, ref gg1.ggData.m_lprawdata, ref gg1.ggData.m_lpimagedata, ref sErr))
            {
                MyTools.ShowMsg("读取数据失败!", "读取帧数据失败 = " + sErr);
                return;
            }

            if (mod_ReadGen2.ReadGen2FromGGClass(gg1, ref Gen2Current, ref sErr) != MyData.iErr_Succ)
            {
                MyTools.ShowMsg("读取数据失败!", "从gg类读数据失败 = " + sErr);
                return;
            }

            PB.pGen2 = Gen2Current;
            PB.Gen2ToData();
            PB.Gen2ToPict();
            PatientPict_Current = PB.Pict;
            image_BigPict.Source = PatientPict_Current;
            DrawCurrentPos(iFrameID);
        }
        private void SetPlayKey(bool Mark)
        {
            StackPanel_RightIcon.IsEnabled = Mark;
            button_Right.IsEnabled = Mark;
            button_End.IsEnabled = Mark;
            button_Back.IsEnabled = Mark;
            button_First.IsEnabled = Mark;

            if (Mark)
            {
                StackPanel_RightIcon.IsEnabled = true;
                button_Play.Visibility = Visibility.Visible;
                button_Play.IsEnabled = true;
                button_Pause.Visibility = Visibility.Hidden;
                button_Pause.IsEnabled = false;
            }
            else
            {
                StackPanel_RightIcon.IsEnabled = false;
                button_Play.Visibility = Visibility.Hidden;
                button_Play.IsEnabled = false;
                button_Pause.Visibility = Visibility.Visible;
                button_Pause.IsEnabled = true;
            }
        }


        /// <summary>
        /// Pos位置响应  ===================================
        /// </summary>
        public bool IsFramePosMouseDown = false;
        public int iFramePosLength = 100;
        private void DrawCurrentPos(int iPos)      // 画出当前帧的位置
        {
            if (iPos == -1)
            {
                image_FramePos.Visibility = Visibility.Hidden;
                line_FramePos.Visibility = Visibility.Hidden;
                FrameInfo.Visibility = Visibility.Hidden;
                Button_Mark.Visibility = Visibility.Hidden;
                Button_UnMark.Visibility = Visibility.Hidden;
            }
            else
            {
                // 顶上光标的位置
                image_FramePos.Visibility = Visibility.Visible;
                int iStepX = (int)((canvas_RecordBottomRule.ActualWidth - canvas_RecordLeftRule.ActualWidth) / gg1.ggData.numberof_frames);
                double Left = canvas_RecordLeftRule.ActualWidth + iPos * iStepX + iStepX / 2 - image_FramePos.ActualWidth / 2;      // = 左尺宽度(10) + (i-1)帧占的位置 + 本帧中间位置 - 图标半宽度
                Thickness thick = image_FramePos.Margin;
                thick.Left = Left;
                image_FramePos.Margin = thick;


                // 直线的位置
                line_FramePos.Visibility = Visibility.Visible;
                line_FramePos.X1 = iPos * iStepX + iStepX / 2;
                line_FramePos.X2 = line_FramePos.X1;

                // 显示位置信息
                FrameInfo.Visibility = Visibility.Visible;
                FrameInfo.Content = "位置：第 " + iPos.ToString() + " 帧 / 共 " + gg1.ggData.numberof_frames.ToString() + " 帧";

                // 显示是否已标记
                if (gg1.ggMark[iPos, 0] == "True")
                {
                    Button_Mark.Visibility = Visibility.Visible;
                    Button_UnMark.Visibility = Visibility.Hidden;
                    // Uri uri = null;
                    // uri = new Uri("pack://application:,,,/Pict/Mark01.ico", UriKind.Relative);
                    // uri = new Uri("pack://siteoforigin:,,,/Pict/Mark01.ico", UriKind.Relative);
                }
                else
                {
                    Button_Mark.Visibility = Visibility.Hidden;
                    Button_UnMark.Visibility = Visibility.Visible;
                }
            }
        }
        private void image_FramePos_MouseEnter(object sender, MouseEventArgs e)
        {
            string sPos = "第 " + gg1.iCurrentPos.ToString() + " 帧/共 " + gg1.ggData.numberof_frames.ToString() + " 帧";
            image_FramePos.ToolTip = sPos;
        }
        private void Line_FramePos_MouseEnter(object sender, MouseEventArgs e)
        {
            string sPos = "第 " + gg1.iCurrentPos.ToString() + " 帧/共 " + gg1.ggData.numberof_frames.ToString() + " 帧";
            line_FramePos.ToolTip = sPos;
        }
        private void image_FramePos_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsFramePosMouseDown = true;

            FrameworkElement fEle = sender as FrameworkElement;
            fEle.CaptureMouse();
            fEle.Cursor = Cursors.Hand;

            Pos0 = e.GetPosition(null);
        }
        private void image_FramePos_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsFramePosMouseDown = false;
            FrameworkElement ele = sender as FrameworkElement;
            ele.ReleaseMouseCapture();
        }
        private void image_FramePos_MouseMove(object sender, MouseEventArgs e)
        {
            // 当前鼠标位置
            Point Pos1 = e.GetPosition(null);

            if (IsFramePosMouseDown)           // 只有按键被，本功能才启用
            {
                // 控件位置
                Window window = Window.GetWindow(Grid_FramePos);
                Point point = Grid_FramePos.TransformToAncestor(window).Transform(new Point(0, 0));
                double X1 = point.X;
                double X2 = X1 + Grid_FramePos.ActualWidth;
                X1 += 13;                 // 标尺起点

                if (Pos1.X < X1)
                    Pos1.X = X1;
                else if (Pos1.X > X2)
                    Pos1.X = X2;

                int iStepX = (int)((canvas_RecordBottomRule.ActualWidth - canvas_RecordLeftRule.ActualWidth) / gg1.ggData.numberof_frames);   // 每帧的宽度
                int iFrame = (int)( (Pos1.X - X1) / iStepX);
                if (iFrame < 0)
                {
                    iFrame = 0;
                }
                else if (iFrame >= gg1.ggData.numberof_frames)
                {
                    iFrame = (int)(gg1.ggData.numberof_frames - 1);
                }

                if (gg1.iCurrentPos != iFrame)
                {
                    gg1.iCurrentPos = iFrame;
                    ShowFrame(gg1.iCurrentPos);
                }
            }
        }
        private void Grid_FramePos_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gg1 != null)
            {
                // 当前鼠标位置
                Point Pos1 = e.GetPosition(null);

                // 控件位置 （下面用窗体的足额对坐标）
                Window window = Window.GetWindow(Grid_FramePos);
                Point point = Grid_FramePos.TransformToAncestor(window).Transform(new Point(0, 0));    // Grid_FramePos的左角位置
                double X1 = point.X;
                double X2 = X1 + Grid_FramePos.ActualWidth;
                X1 += canvas_RecordLeftRule.ActualWidth;                 // 标尺的真正起点

                if (Pos1.X < X1)
                    Pos1.X = X1;
                else if (Pos1.X > X2)
                    Pos1.X = X2;

                int iStepX = (int)((canvas_RecordBottomRule.ActualWidth - canvas_RecordLeftRule.ActualWidth) / gg1.ggData.numberof_frames);   // 每帧的宽度
                int iFrame = (int)((Pos1.X - X1) / iStepX);
                if (iFrame < 0)
                {
                    iFrame = 0;
                }
                else if (iFrame >= gg1.ggData.numberof_frames)
                {
                    iFrame = (int)(gg1.ggData.numberof_frames - 1);
                }

                if (gg1.iCurrentPos != iFrame)
                {
                    gg1.iCurrentPos = iFrame;
                    ShowFrame(gg1.iCurrentPos);
                }
            }
        }
        private void image_Record_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid_FramePos_MouseDown(sender, e);
        }

        /// <summary>
        /// Mark位置响应  ===================================
        /// </summary>
        private void DrawMarkPict(int iFrameID, string Mode)
        {
            // iFrameID = -1              : All Mark
            // iFrameID = 0 -- MaxFrameID : Single Frame

            // Mode = Clear or Draw

            if (iFrameID >= 0 && iFrameID < gg1.ggData.numberof_frames)   // 指定某一帧
            {
                if (Mode == "Draw")
                {
                    DrawOneMark(iFrameID, "Draw");
                }
                else
                {
                    DrawOneMark(iFrameID, "Clear");
                }
            }
            else  // 不是指定某一帧
            {
                // 先全部清掉，iFrameID参数不符合要求的也全部清掉
                ClearAllMark();

                // 如果是对全体进行Draw操作
                if (iFrameID == -1 && Mode == "Draw")       // 全部画出来
                {
                    for (int i = 0; i < gg1.ggData.numberof_frames; i++)
                    {

                        if (gg1.ggMark[i, 0] == "True")
                        {
                            DrawOneMark(i, "Draw");
                        }
                    }
                }
            }
        }
        private void ClearAllMark()
        {
            if (Grid_FramePos.Children.Count > 0)
            {
                int ImageNum = Grid_FramePos.Children.Count;
                for (int i = ImageNum; i > 0; i--)
                {
                    UIElement ctl = Grid_FramePos.Children[i-1];
                    if (ctl is RecordMarkImage)
                    {
                        RecordMarkImage im = ctl as RecordMarkImage;
                        string ImageName = im.Name;
                        if (im.Name.Length > 8 && im.Name.Substring(0, 8) == "MarkPict")
                        {
                            Grid_FramePos.Children.Remove(im);//移除对应按钮控件   
                            Grid_FramePos.UnregisterName(ImageName);//还需要把对用的名字注销掉，否则再次点击Button_Add会报错  
                        }
                    }
                }
            }
        }
        private void DrawOneMark(int iFrameID, string Mode)
        {
            if (Mode == "Draw")
            {
                if (gg1.ggMark[iFrameID, 0] == "True")
                {
                    if (MarkPict[iFrameID] == null)
                    {
                        MarkPict[iFrameID] = new RecordMarkImage();
                        MarkPict[iFrameID].Stretch = Stretch.Uniform;
                        MarkPict[iFrameID].HorizontalAlignment = HorizontalAlignment.Left;
                        MarkPict[iFrameID].VerticalAlignment = VerticalAlignment.Top;
                        MarkPict[iFrameID].Height = image_FramePos.ActualHeight;
                        MarkPict[iFrameID].Width = image_FramePos.ActualWidth;
                        MarkPict[iFrameID].Name = "MarkPict" + iFrameID.ToString();
                        MarkPict[iFrameID].ToolTip = "第 " + iFrameID.ToString() + " 帧标记：" + System.Environment.NewLine + gg1.ggMark[iFrameID, 3];

                        Grid_FramePos.Children.Add(MarkPict[iFrameID]);
                        Grid_FramePos.RegisterName("MarkPict" + iFrameID.ToString(), MarkPict[iFrameID]);  //注册名字，以便以后使用
                        //MarkPict[iFrameID].ToolTip = "第 " + iFrameID.ToString() + " 帧标记：" + System.Environment.NewLine + gg1.ggMark[iFrameID, 3];
                        //MarkPict[iFrameID].MouseEnter += new System.Windows.Input.MouseEventHandler(MarkPict_MousEnter);

                        // Display
                        int iStepX = (int)((canvas_RecordBottomRule.ActualWidth - canvas_RecordLeftRule.ActualWidth) / gg1.ggData.numberof_frames);
                        double Left = canvas_RecordLeftRule.ActualWidth + iFrameID * iStepX + iStepX / 2 - image_FramePos.ActualWidth / 2;      // = 左尺宽度(10) + (i-1)帧占的位置 + 本帧中间位置 - 图标半宽度
                        Thickness thick = new Thickness(Left, 0, 0, 0);
                        MarkPict[iFrameID].Margin = thick;
                        MarkPict[iFrameID].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MarkPict[iFrameID].ToolTip = "第 " + iFrameID.ToString() + " 帧标记：" + System.Environment.NewLine + gg1.ggMark[iFrameID, 3];
                    }
                }
            }
            else  // Clear
            {
                if (MarkPict[iFrameID] != null)
                {
                    Image im = Grid_FramePos.FindName("MarkPict" + iFrameID.ToString()) as Image;   //找到添加的按钮
                    if (im != null)//判断是否找到，以免在未添加前就误点了   
                    {
                        Grid_FramePos.Children.Remove(im);//移除对应按钮控件   
                        Grid_FramePos.UnregisterName("MarkPict" + iFrameID.ToString());//还需要把对用的名字注销掉，否则再次点击Button_Add会报错   
                    };
                    MarkPict[iFrameID] = null;
                }
            }
        }
        private void Button_Mark_Click(object sender, RoutedEventArgs e)
        {
            frm_EditMark Frm_EditMark = new frm_EditMark();

            Frm_EditMark.iFrameID = gg1.iCurrentPos;
            if (gg1.ggMark[gg1.iCurrentPos, 0] == "True")
            {
                Frm_EditMark.iMarkID = int.Parse(gg1.ggMark[gg1.iCurrentPos, 2]);
            }
            else
            {
                Frm_EditMark.iMarkID = -1;
            }
            Frm_EditMark.sMarkInfo = gg1.ggMark[gg1.iCurrentPos, 3];
            Frm_EditMark.sCheckTime = DateTime.Parse(Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["CheckTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");

            Frm_EditMark.ShowDialog();

            if (Frm_EditMark.IsDele)
            {
                gg1.ggMark[gg1.iCurrentPos, 0] = "False";
                //gg1.ggMark[gg1.iCurrentPos, 1] = "";
                //gg1.ggMark[gg1.iCurrentPos, 2] = "";
                gg1.ggMark[gg1.iCurrentPos, 3] = "";
            }
            else if (Frm_EditMark.IsEdit)
            {
                string sSql = "";
                string sErr = "";
                int iId = -1;
                if (Frm_EditMark.iMarkID != -1)
                {
                    // 原来已经存在有 Mark 的
                    sSql = "Update Mark Set MARK = '" + Frm_EditMark.sMarkInfo + "' where MarkID = " + Frm_EditMark.iMarkID;
                    if (!MyData.MySqlite.WriteData(sSql, false, ref iId, ref sErr))
                    {
                        MyTools.ShowMsg("保存数据时出现错误！", sErr);
                    }
                    else
                    {
                        MyTools.ShowMsg("保存数据成功！", "");
                    }
                }
                else
                {
                    sSql = "Insert into Mark (FILEID, RECORDID, FRAMEID, MARK) Values(" +
                            Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["FileID"].ToString() + "," +
                            Record_DataTable.Rows[listBox_CheckRecord.SelectedIndex]["RecordID"].ToString() + "," +
                            gg1.iCurrentPos.ToString() + "," +
                            "'" + Frm_EditMark.sMarkInfo + "')";
                    if (!MyData.MySqlite.WriteData(sSql, true, ref iId, ref sErr))
                    {
                        MyTools.ShowMsg("保存数据时出现错误！", sErr);
                    }
                    else
                    {
                        MyTools.ShowMsg("保存数据成功！", "");
                    }
                    gg1.ggMark[gg1.iCurrentPos, 0] = "True";
                    gg1.ggMark[gg1.iCurrentPos, 2] = iId.ToString();
                }
                gg1.ggMark[gg1.iCurrentPos, 3] = Frm_EditMark.sMarkInfo;
            }
            Frm_EditMark.Close();

            // 屏幕上显示
            if (gg1.ggMark[gg1.iCurrentPos, 0] == "True")   // 已经有标记
            {
                DrawMarkPict(gg1.iCurrentPos, "Draw");
                Button_Mark.Visibility = Visibility.Visible;
                Button_UnMark.Visibility = Visibility.Hidden;
            }
            else                                            // 没有标记
            {
                DrawMarkPict(gg1.iCurrentPos, "Clear");
                Button_Mark.Visibility = Visibility.Hidden;
                Button_UnMark.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// CutLine响应  ===================================
        /// </summary>
        bool IsCutLineDown = false;
        Point CutLineP0, CutLineP1;
        private void DrawCutLine(int Mode)     // 切割线 -1=隐藏
        {
            if (Mode == -1)
            {
                Line_CutLine_0.Visibility = Visibility.Hidden;
                Line_CutLine_1.Visibility = Visibility.Hidden;
                return;
            }

            double R = image_BigPict.ActualWidth / 2;
            double P0x = 0;
            double P0y = 0;
            double P1x = 0;
            double P1y = 0;
            if (CutAngle >= MyData.PI_90 && CutAngle < MyData.PI_180)            // 第一象限
            {
                P0x = R * Math.Cos( CutAngle - MyData.PI_90);
                P0y = -R * Math.Sin(CutAngle - MyData.PI_90);
            }
            else if (CutAngle >= MyData.PI_180 && CutAngle < MyData.PI_270)       // 第二象限
            {
                P0x = -R * Math.Cos(MyData.PI_270 - CutAngle);
                P0y = -R * Math.Sin(MyData.PI_270 - CutAngle);
            }
            else if (CutAngle > MyData.PI_270 && CutAngle < MyData.PI_360)       // 第三象限
            {
                P0x = -R * Math.Cos(CutAngle - MyData.PI_270);
                P0y = R * Math.Sin(CutAngle - MyData.PI_270);
            }
            else if (CutAngle >= 0 && CutAngle < MyData.PI_90)                        // 第四象限
            {
                P0x = R * Math.Cos(MyData.PI_90 - CutAngle);
                P0y = R * Math.Sin(MyData.PI_90 - CutAngle);
            }
            Line_CutLine_0.Visibility = Visibility.Visible;
            Line_CutLine_1.Visibility = Visibility.Visible;

            Line_CutLine_0.X1 = image_BigPict.ActualWidth / 2;
            Line_CutLine_0.Y1 = image_BigPict.ActualHeight / 2;
            Line_CutLine_0.X2 = image_BigPict.ActualWidth / 2 + P0x;
            Line_CutLine_0.Y2 = image_BigPict.ActualWidth / 2 + P0y;

            P1x = -P0x;
            P1y = -P0y;
            Line_CutLine_1.X1 = image_BigPict.ActualWidth / 2;
            Line_CutLine_1.Y1 = image_BigPict.ActualHeight / 2;
            Line_CutLine_1.X2 = image_BigPict.ActualWidth / 2 + P1x;
            Line_CutLine_1.Y2 = image_BigPict.ActualWidth / 2 + P1y;
        }
        private void Line_CutLine_0_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen )                              // 要移动切割线，只有Zoom按键被设置同时测量功能已关闭的时候，本功能才启用
            {
                FrameworkElement fEle = sender as FrameworkElement;
                //fEle.CaptureMouse();                                           // 不需要捕获鼠标
                fEle.Cursor = Cursors.SizeAll;
            }
        }
        private void Line_CutLine_0_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen)                              // 要移动切割线，只有Zoom按键被设置同时测量功能已关闭的时候，本功能才启用
            {
                FrameworkElement fEle = sender as FrameworkElement;
                //fEle.ReleaseMouseCapture();
                fEle.Cursor = Cursors.Hand;
            }
        }
        private void Line_CutLine_0_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen)                              // 要移动切割线，只有Zoom按键被设置同时测量功能已关闭的时候，本功能才启用
            {
                FrameworkElement fEle = sender as FrameworkElement;
                fEle.CaptureMouse();                                           // 需要捕获鼠标
                fEle.Cursor = Cursors.SizeAll;
                IsCutLineDown = true;
            }
        }

        //GG文件输出图片
        
        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            //frm_CopyToPng Frm_CopyToPng = new frm_CopyToPng(image_BigPict);
            //Frm_CopyToPng.ShowDialog();        

            //Image pImage = null;
            //frm_CopyToPng Frm_CopyToPng = new frm_CopyToPng(image_BigPict);
            //Image pImage = image_BigPict;

            while (gg1.iCurrentPos < gg1.ggData.numberof_frames - 1)
            {
                //Image pImage = PatientPict_Current;
                string sFileName = "D:\\1" + "\\" + gg1.iCurrentPos.ToString() + ".png"; //当前帧数转为字符

                PB.pGen2 = Gen2Current;
                PB.Gen2ToData();                        // 转变成 PictData
                PB.Gen2ToPict();                        // 转变成 Pict
                PatientPict_Current = PB.Pict;
                BitmapSource BS = PatientPict_Current;
                PngBitmapEncoder PBE = new PngBitmapEncoder();
                PBE.Frames.Add(BitmapFrame.Create(BS));
                using (Stream stream = File.Create(sFileName))
                {
                    PBE.Save(stream);
                }

                //循环进行下一帧图片
                gg1.iCurrentPos++;
                ShowFrame(gg1.iCurrentPos);
            }
            MyTools.ShowMsg("输出图片成功！", "");
        }

        private void Line_CutLine_0_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen && IsCutLineDown)                              // 要移动切割线，只有Zoom按键被设置同时测量功能已关闭的时候，本功能才启用
            {
                Line_CutLine_0_MouseMove(sender, e);

                // 调整RecordPict
                string sErrString = "";
                if (gg1.ReadRecordBeltData(gg1.ggAllRawData, ref gg1.ggRecordBeltData, CutAngle, ref sErrString))
                {
                    PB.GGRecordToPict(PB.RecordPictWidth);
                    image_Record.Source = PB.RecordPict;

                    DrawCurrentPos(gg1.iCurrentPos);
                }
                else
                {
                    MyTools.ShowMsg("读取帧数据出错！", sErrString);
                }

                FrameworkElement fEle = sender as FrameworkElement;
                fEle.ReleaseMouseCapture();
                fEle.Cursor = Cursors.Hand;
                IsCutLineDown = false;
            }
        }
        private void Line_CutLine_0_MouseMove(object sender, MouseEventArgs e)
        {
            if (ZoomIsPress && !Pop_Detect.IsOpen && IsCutLineDown)                 // 要移动切割线，只有: (Zoom按键被设置 and 测量功能已关闭 and IsCutLineDown)的时候，本功能才启用
            {
                FrameworkElement fEle = sender as FrameworkElement;

                // 当前鼠标位置
                CutLineP1 = e.GetPosition(Canvas_BigPict1);

                //获得目标经过变换之后的Rect
                Rect targetRect =
                    Canvas_BigPict2.RenderTransform.TransformBounds(new Rect(Canvas_BigPict2.RenderSize));

                // 圆心位置
                double CenX = targetRect.X + targetRect.Width / 2;
                double CenY = targetRect.Y + targetRect.Height / 2;

                double Angle_New = 0;
                if ((CutLineP1.X - CenX) == 0)  // Y轴上
                {
                    if ((CutLineP1.Y - CenY) > 0)
                        Angle_New = 0;                     // 0度
                    else   // (CutLineP1.Y - CenY) < 0
                        Angle_New = -MyData.PI;            // 180 度
                }
                else
                {
                    Angle_New = Math.Atan((CutLineP1.Y - CenY) / (CutLineP1.X - CenX));
                    if ((CutLineP1.Y - CenY) >= 0 && (CutLineP1.X - CenX) > 0)  // 第四象限
                    {
                        Angle_New = MyData.PI_90 - Angle_New;
                    }
                    else if ((CutLineP1.Y - CenY) <= 0 && (CutLineP1.X - CenX) > 0)  // 第一象限
                    {
                        Angle_New = MyData.PI_90 - Angle_New;
                    }
                    else if ((CutLineP1.Y - CenY) <= 0 && (CutLineP1.X - CenX) < 0)  // 第二象限
                    {
                        Angle_New = MyData.PI_270 - Angle_New;
                    }
                    else if ((CutLineP1.Y - CenY) >= 0 && (CutLineP1.X - CenX) < 0)  // 第二象限
                    {
                        Angle_New = MyData.PI_270 - Angle_New;
                    }
                }

                if (fEle.Name == "Line_CutLine_0")       // 移动的是上半截
                {
                    CutAngle = Angle_New;
                }
                else
                {
                    CutAngle = Angle_New + MyData.PI;
                }

                if (CutAngle >= MyData.PI_360)
                    CutAngle -= MyData.PI_360;
                DrawCutLine(1);
            }
        }


        //private void MarkPict_MousEnter(object sender, RoutedEventArgs e)
        //{
        //    int iFrameID = -1;

        //    try
        //    {
        //        // 获取控件名称
        //        Image im = sender as Image;
        //        if (im.Name.Length > 5 && im.Name.Substring(0, 5) == "MarkPict")
        //        {
        //            iFrameID = int.Parse(im.Name.Substring(5));

        //            if (gg1.ggMark[iFrameID, 0] == "True"  )
        //            {
        //                im.ToolTip = gg1.ggMark[iFrameID, 3];
        //            }
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        /// <summary>
        /// 参数设置  ===================================
        /// </summary>
        private void button_Setup_Click(object sender, RoutedEventArgs e)
        {
            Pop_Setup.IsOpen = true;
        }

        //private void DispGG(object sender, EventArgs e)
        //{
        //    if (DispGGStop)
        //    {
        //        textBox_CheckInfo.Text = "第" + iFrameNo.ToString() + "帧" + Environment.NewLine;
        //    }
        //}


        //void bw_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    while (!DispGGClose)
        //    {
        //        if (DispGG)
        //        {
        //            try
        //            {
        //                UpdateGGDelegate updateGGDelegate = new UpdateGGDelegate(DrawGG);
        //                this.Dispatcher.Invoke(updateGGDelegate);

        //                //textBox_CheckInfo.Text = "第" + iFrameNo.ToString() + "帧" + Environment.NewLine;
        //                //DispGG = false;
        //            }
        //            catch (Exception ex)
        //            {
        //                string aa = ex.Message;
        //            }
        //        }
        //        System.Threading.Thread.Sleep(5); //停1秒
        //    }
        //}

        //private delegate void UpdateGGDelegate();
        //private void DrawGG()
        //{
        //    if (DispGG)
        //    {
        //        textBox_CheckInfo.Text = "第" + iFrameNo.ToString() + "帧" + Environment.NewLine;
        //        //image_BigPict.Source = ShowPict;
        //        DispGG = false;
        //    }
        //}
    }
}
