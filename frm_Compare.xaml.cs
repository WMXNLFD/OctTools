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
    /// frm_Compare.xaml 的交互逻辑
    /// </summary>
    public partial class frm_Compare : Window
    {
        public Window pfrm_Main;
        public bool IsReturn = false;

        public struct PatientForSelect_struct
        {
            public string PatientName;
            public string PatientID;
        }

        // 所有人员的个人信息
        MyData.PatientInfo_Struct[] AllPatient_struct = null;
        int SelectID_Left = -1;              // 左边选定的人员，在AllPatient_struct[]的ID： -1 = no select, 0--n = AllPatient_struct[ID]
        int SelectID_Right = -1;             // 右边选定的人员，在AllPatient_struct[]的ID： -1 = no select, 0--n = AllPatient_struct[ID]
        mod_ReadGen2.Gen2_Struct Gen2Current_Left = new mod_ReadGen2.Gen2_Struct();
        mod_ReadGen2.Gen2_Struct Gen2Current_Right = new mod_ReadGen2.Gen2_Struct();
        mod_ShowPict PB_Left = new mod_ShowPict();
        mod_ShowPict PB_Right = new mod_ShowPict();
        mod_GG gg1_Left = new mod_GG();
        mod_GG gg1_Right = new mod_GG();
        BitmapImage PatientPict_Current_Left;
        BitmapImage PatientPict_Current_Right;
        RecordMarkImage[] MarkPict_Left, MarkPict_Right;

        int iDelay_Left = 100;
        int iDelay_Right = 100;
        public double dCutAngle_Left = 0;
        public double dCutAngle_Right = 0;

        bool bisLink = false;
        bool bIsLink
        {
            get { return bisLink; }
            set
            {
                bisLink = value;
                if (bisLink)        // 图像关联
                {
                    button_Link.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Pict/Link03.PNG", UriKind.RelativeOrAbsolute)));
                    button_Link.OpacityMask = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Pict/Link03.PNG", UriKind.RelativeOrAbsolute)));
                }
                else                // 图像没关联
                {
                    button_Link.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Pict/UnLink03.PNG", UriKind.RelativeOrAbsolute)));
                    button_Link.OpacityMask = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Pict/UnLink03.PNG", UriKind.RelativeOrAbsolute)));
                }
            }
        }

        string sCurrentPos = "Left";

        // 选定人员的检查记录
        MyData.CheckRecord_Struct Record_Left;
        MyData.CheckRecord_Struct Record_Right;


        // 窗体
        double WinWidth_Real = 1600;
        double WinHeight_Real = 900;
        public frm_Compare()
        {
            InitializeComponent();
            IsReturn = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindows();
            CreatDataTable();

            this.ShowInTaskbar = true;
            this.Title = MyData.sVer;
            this.IsEnabled = true;
            this.Show();

            bIsLink = false;        // 图像没有关联
            button_Link.Visibility = Visibility.Hidden;

            string sErr = "";
            if (!MyData.MySqlite.GetAllPatient(ref AllPatient_struct, ref sErr))
            {
                MyTools.ShowMsg("查询数据错误！", sErr);
                AllPatient_struct = null;
            }
            else
            {
            }

            gg1_Left = new mod_GG();
            PB_Left = new mod_ShowPict();
            PB_Left.RecordPictWidth = (int)Grid_Record_Left.ActualWidth;
            PB_Left.gg1 = gg1_Left;
            ZoomIsPress_Left = true;
            button_Zoom_Left.Visibility = Visibility.Hidden;
            BigPictIsPress_Left = false;
            iDelay_Left = MyData.DelayDefault;
            dCutAngle_Left = MyData.AngleDefault;
            DetectLine_Left = null;

            gg1_Right = new mod_GG();
            PB_Right = new mod_ShowPict();
            PB_Right.RecordPictWidth = (int)Grid_Record_Right.ActualWidth;
            PB_Right.gg1 = gg1_Right;
            ZoomIsPress_Right = true;
            button_Zoom_Right.Visibility = Visibility.Hidden;
            BigPictIsPress_Right = false;
            iDelay_Right = MyData.DelayDefault;
            dCutAngle_Right = MyData.AngleDefault;
            DetectLine_Right = null;

            SelectID_Left = -1;
            SelectID_Right = -1;
            NewPatientForSelectPict();
            DispPict("Left");
            DispPict("Right");


            // 先选定左边的图像
                sCurrentPos = "Left";
            Border_LeftPictBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF29C07"));    // new SolidColorBrush(Colors.Red);
            Border_RightPictBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF437590"));
        }
        private void SetWindows()
        {
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            WinWidth_Real = System.Windows.SystemParameters.WorkArea.Width;
            WinHeight_Real = System.Windows.SystemParameters.WorkArea.Height;
            this.Top = 0;
            this.Left = 0;
            this.Width = WinWidth_Real;
            this.Height = WinHeight_Real;

            label_Logo.Content = MyData.sVer;
            if (this.grid_LeftPict.ActualWidth <= this.grid_LeftPict.ActualHeight)
            {
                this.Canvas_LeftPict1.Width = this.grid_LeftPict.ActualWidth - 10;
            }
            else
            {
                this.Canvas_LeftPict1.Width = this.grid_LeftPict.ActualHeight - 10;
            }
            this.Canvas_LeftPict1.Height = this.Canvas_LeftPict1.Width;
            this.Canvas_LeftPict2.Width = this.Canvas_LeftPict1.Width;
            this.Canvas_LeftPict2.Height = this.Canvas_LeftPict1.Width;
            this.image_LeftPict.Width = this.Canvas_LeftPict1.Width;
            this.image_LeftPict.Height = this.Canvas_LeftPict1.Width;
            this.Canvas_LeftPict1.Margin = new Thickness((this.grid_LeftPict.ActualWidth - this.Canvas_LeftPict1.ActualWidth) / 2,
                                                         (this.grid_LeftPict.ActualHeight - this.Canvas_LeftPict1.ActualHeight) / 2, 0, 0);

            if (this.grid_RightPict.ActualWidth <= this.grid_RightPict.ActualHeight)
            {
                this.Canvas_RightPict1.Width = this.grid_RightPict.ActualWidth - 10;
            }
            else
            {
                this.Canvas_RightPict1.Width = this.grid_RightPict.ActualHeight - 10;
            }
            this.Canvas_RightPict1.Height = this.Canvas_RightPict1.Width;
            this.Canvas_RightPict2.Width = this.Canvas_RightPict1.Width;
            this.Canvas_RightPict2.Height = this.Canvas_RightPict1.Width;
            this.image_RightPict.Width = this.Canvas_RightPict1.Width;
            this.image_RightPict.Height = this.Canvas_RightPict1.Width;
            this.Canvas_RightPict1.Margin = new Thickness((this.grid_RightPict.ActualWidth - this.Canvas_RightPict1.ActualWidth) / 2,
                                                         (this.grid_RightPict.ActualHeight - this.Canvas_RightPict1.ActualHeight) / 2, 0, 0);

            DrawBigPictRules("Left",-1);
            DrawBigPictRules("Right", -1);
            DrawRecordRules("Left", -1);
            DrawRecordRules("Right", -1);
        }

        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            pfrm_Main.Show();
            pfrm_Main.ShowInTaskbar = true;
            pfrm_Main.IsEnabled = true;
            this.Hide();
            this.Close();
        }
        private void button_Mini_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        DataTable Record_DataTable = null;
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
        }


        // 顶部人员
        int iPatientForSelect_Start = 0;           // 顶部人员从 AllPatient_struct 的那个位置开始
        int iPatientForSelect_ButtonNum = 0;       // 共有多少个Button
        int iPatientForSelect_Button_Current = -1; // 按下的按键编号
        Button[] PatientForSelect_Button;
        int iButtonHeight = 0;
        int iButtonWidth = 0;
        private void NewPatientForSelectPict()
        {
            iButtonHeight = (int)(this.stackPanel_PatientForSelect.ActualHeight);
            iButtonWidth = (int)(iButtonHeight * 120 / 52);
            iPatientForSelect_ButtonNum = (int)(this.stackPanel_PatientForSelect.ActualWidth / iButtonWidth);
            PatientForSelect_Button = new Button[iPatientForSelect_ButtonNum];               

            iPatientForSelect_Start = 0;
            SetPatientForSelectPict(iPatientForSelect_Start);
        }
        private void SetPatientForSelectPict(int iStartID)
        {
            this.stackPanel_PatientForSelect.Children.Clear();
            if (AllPatient_struct == null || AllPatient_struct.Length <= 0 || iStartID > AllPatient_struct.Length)
            {
                return;
            }

            int iPos = iStartID;
            Label l1;
            for (int i = 0; i < iPatientForSelect_ButtonNum; i++)
            {
                if ((iStartID + i) >= AllPatient_struct.Length) break;

                PatientForSelect_Button[i] = new Button();
                PatientForSelect_Button[i].Height = iButtonHeight;
                PatientForSelect_Button[i].Width = iButtonWidth;
                PatientForSelect_Button[i].FontSize = 11.5;
                PatientForSelect_Button[i].Name = "B" + i.ToString();
                PatientForSelect_Button[i].Click += new RoutedEventHandler(button_SelectPatient_Top_Click);

                //Style st = (Style)this.FindResource("PatientFoSelectButton");
                //PatientForSelect_Button[i].Style = st;
                PatientForSelect_Button[i].Style = this.Resources["PatientFoSelectButton"] as Style;
                PatientForSelect_Button[i].ApplyTemplate();
                PatientForSelect_Button[i].Content = AllPatient_struct[iStartID + i].Name;

                l1 = PatientForSelect_Button[i].Template.FindName("PatientIDText", PatientForSelect_Button[i]) as Label;
                l1.Content = AllPatient_struct[iStartID + i].PatientID;
                this.stackPanel_PatientForSelect.Children.Add(PatientForSelect_Button[i]);
            }
        }
        private void button_SelectPatient_Top_Click(object sender, RoutedEventArgs e)
        {
            MyData.PatientInfo_Struct Select_Patient_struct;               // 用于保存选定位置的人员信息
            MyData.CheckRecord_Struct[] Select_Record_struct = null;       // 用于保存选定位置的记录信息
            string sErr = "";
            string sTmp = "";

            int iButtonID = int.Parse(((Button)sender).Name.Substring(1));
            iPatientForSelect_Button_Current = iButtonID;                  // 被按下的按键编号
            try
            {
                Select_Patient_struct = AllPatient_struct[iPatientForSelect_Start + iButtonID];
                if (!MyData.MySqlite.GetPatientRecord(Select_Patient_struct.FileID, ref Select_Record_struct, ref sErr))
                {
                    MyTools.ShowMsg("查询数据失败！", sErr);
                    return;
                }

                Record_DataTable.Clear();
                if (Select_Record_struct != null && Select_Record_struct.Length > 0)
                {
                    for (int i = 0; i < Select_Record_struct.Length; i++)
                    {
                        sTmp = System.Environment.CurrentDirectory + "\\Data\\" + Select_Record_struct[i].FileID.Trim() + "\\";

                        DataRow dr = Record_DataTable.NewRow();
                        dr["RecordID"] = Select_Record_struct[i].RecordID;
                        dr["FileID"] = Select_Record_struct[i].FileID;
                        dr["CheckTime"] = Select_Record_struct[i].CheckTime;
                        dr["Doctor"] = Select_Record_struct[i].Doctor;
                        dr["CheckInfo"] = Select_Record_struct[i].CheckInfo;
                        dr["SmallPict"] = sTmp + "Small\\" + Select_Record_struct[i].SmallPict;
                        dr["BigPict"] = sTmp + "Big\\" + Select_Record_struct[i].BigPict;
                        Record_DataTable.Rows.Add(dr);
                    }

                    this.datagrid_Record.ItemsSource = Record_DataTable.DefaultView;
                    datagrid_Record.SelectedIndex = 0;
                }
                else
                {
                    this.datagrid_Record.ItemsSource = null;
                }

                this.Patien_PatientID.Content = Select_Patient_struct.PatientID;
                this.Patien_PatientName.Content = Select_Patient_struct.Name;
                this.Patien_PatientSex.Content = Select_Patient_struct.Sex;
                this.Patien_PatientBirthday.Content = Select_Patient_struct.Birthday;
                this.Patien_PatientAddr.Content = Select_Patient_struct.Address;
                this.Patien_PatientTele.Content = Select_Patient_struct.Tele;
                this.Patien_PatientIdentifyID.Content = Select_Patient_struct.IdentifyID;
                this.Patien_PatientMemo.Text = Select_Patient_struct.Memo;

                Pop_ForPatient.PlacementTarget = (Button)sender;
                Pop_ForPatient.HorizontalOffset = 50;
                Pop_ForPatient.VerticalOffset = 0;
                Pop_ForPatient.IsOpen = true;
            }
            catch (Exception ex)
            {
                MyTools.ShowMsg("查询数据失败！", ex.Message);
            }
        }
        private void DispPict(string sPos)
        {
            int iSelectID = -1;
            Label label_SelectLabel = null;
            Image image_SelectImage = null;
            Image image_SelectRecord = null;
            Grid grid_SelectControl = null;
            Button button_SelectZoom = null;

            Button button_SelectPatientButton = null;
            Canvas canvas_BigPictLeftRule;
            Canvas canvas_BigPictBottomRule;
            Canvas canvas_RecordLeftRule;
            Canvas canvas_RecordBottomRule;
            Image image_FramePos;
            BitmapImage PatientPict_Current;

            MyData.PatientInfo_Struct Select_Patient_struct;      // 用于保存选定位置的人员信息
            MyData.CheckRecord_Struct Select_Record_struct;       // 用于保存选定位置的记录信息

            mod_ReadGen2.Gen2_Struct Gen2Current = new mod_ReadGen2.Gen2_Struct();
            mod_ShowPict PB;
            mod_GG gg1;

            RecordMarkImage[] MarkPict;
            double dCutAngle;

            string sTmp = "";
            DateTime dt;

            if (sPos == "Left")
            {
                iSelectID = SelectID_Left;
                label_SelectLabel = label_LeftPatient;
                image_SelectImage = image_LeftPict;
                image_SelectRecord = image_Record_Left;
                grid_SelectControl = Grid_Control_Left;
                button_SelectPatientButton = button_LeftPatient;
                button_SelectZoom = button_Zoom_Left;

                Gen2Current = Gen2Current_Left;
                PB = PB_Left;
                gg1 = gg1_Left;
                PatientPict_Current = PatientPict_Current_Left;
                //image_SelectRecord = image_Record_Left;
                //grid_SelectControl = Grid_Control_Left;

                canvas_BigPictLeftRule = canvas_LeftPictLeftRule;
                canvas_BigPictBottomRule = canvas_LeftPictBottomRule;
                canvas_RecordLeftRule = canvas_RecordLeftRule_Left;
                canvas_RecordBottomRule = canvas_RecordBottomRule_Left;
                image_FramePos = image_FramePos_Left;
                MarkPict = MarkPict_Left;
            }
            else
            {
                iSelectID = SelectID_Right;
                label_SelectLabel = label_RightPatient;
                image_SelectImage = image_RightPict;
                image_SelectRecord = image_Record_Right;
                grid_SelectControl = Grid_Control_Right;
                button_SelectPatientButton = button_RightPatient;
                button_SelectZoom = button_Zoom_Right;

                Gen2Current = Gen2Current_Right;
                PB = PB_Right;
                gg1 = gg1_Right;
                PatientPict_Current = PatientPict_Current_Right;
                //image_SelectRecord = image_Record_Right;
                //grid_SelectControl = Grid_Control_Right;

                canvas_BigPictLeftRule = canvas_RightPictLeftRule;
                canvas_BigPictBottomRule = canvas_RightPictBottomRule;
                canvas_RecordLeftRule = canvas_RecordLeftRule_Right;
                canvas_RecordBottomRule = canvas_RecordBottomRule_Right;
                image_FramePos = image_FramePos_Right;
                MarkPict = MarkPict_Right;
            }

            gg1.mod_GG_Init();
            PB.gg1 = gg1;
            MarkPict = null;
            Detect_Clear(sPos);

            if (iSelectID == -1)    // 还没有选择病人
            {
                label_SelectLabel.Content = "";
                image_SelectImage.Source = null;
                image_SelectRecord.Source = null;
                DrawBigPictRules(sPos, -1);
                DrawRecordRules(sPos, -1);
                DrawCurrentPos(sPos, -1);

                SetPlayKey(sPos, true);
                grid_SelectControl.IsEnabled = false;
                //button_SelectZoom.IsEnabled = false;
                button_SelectPatientButton.Visibility = Visibility.Hidden;
                label_SelectLabel.Visibility = Visibility.Hidden;

                bIsLink = false;
                button_Link.Visibility = Visibility.Hidden;
            }
            else     // 有选择病人
            {
                if (sPos == "Left")
                {
                    Select_Patient_struct = AllPatient_struct[SelectID_Left];
                    Select_Record_struct = Record_Left;
                    dCutAngle_Left = MyData.AngleDefault;
                }
                else
                {
                    Select_Patient_struct = AllPatient_struct[SelectID_Right];
                    Select_Record_struct = Record_Right;
                    dCutAngle_Right = MyData.AngleDefault;
                }

                sTmp = MyTools.GetStringSubLen(Select_Patient_struct.Name, 0, 16).Trim();
                dt = DateTime.Parse(Select_Patient_struct.Birthday);
                label_SelectLabel.Content = "受检人：" + sTmp + "，性别：" + Select_Patient_struct.Sex.Trim() + "，年龄：" +
                    (DateTime.Now.Year - DateTime.Parse(Select_Patient_struct.Birthday).Year).ToString() + "岁，医生：" +
                    Select_Record_struct.Doctor;

                button_SelectPatientButton.Visibility = Visibility.Visible;
                label_SelectLabel.Visibility = Visibility.Visible;
                bIsLink = false;                                      // 档案有变化，同步功能取消
                button_Link.Visibility = Visibility.Hidden;

                if (Select_Record_struct.BigPict != "")  // 有图像
                {
                    string sErr = "";
                    string sFileName = "";

                    sFileName = Select_Record_struct.BigPict;
                    if (System.IO.Path.GetExtension(sFileName) == ".gen2")
                    {
                        Gen2Current = new mod_ReadGen2.Gen2_Struct();
                        if (mod_ReadGen2.ReadGen2FromFile(sFileName, ref Gen2Current, ref sErr) != MyData.iErr_Succ)
                        {
                            MyTools.ShowMsg("读取数据失败!", sErr);
                            return;
                        }

                        // BigPict图像的显示
                        PB.pGen2 = Gen2Current;
                        PB.Gen2ToData();                        // 转变成 PictData
                        PB.Gen2ToPict();
                        PatientPict_Current = PB.Pict;
                        image_SelectImage.Source = PatientPict_Current;

                        // Record 图像
                        image_SelectRecord.Source = null;
                        DrawCurrentPos(sPos, -1);
                        ClearAllMark(sPos);

                        // 标尺的更改
                        DrawBigPictRules(sPos,1);
                        DrawRecordRules(sPos,-1);             // gen2时，下面的标尺不用画
                        DrawCutLine(sPos, -1);

                        // 下面的控制不能用
                        SetPlayKey(sPos, true);
                        grid_SelectControl.IsEnabled = false;
                        //button_SelectZoom.IsEnabled = true;
                    }
                    else // if ( System.IO.Path.GetExtension(sFileName)== ".gg"  ) 
                    {
                        gg1 = new mod_GG();
                        gg1.mod_GG_Init();
                        Gen2Current = new mod_ReadGen2.Gen2_Struct();
                        if (gg1.ReadggHeader(sFileName, ref sErr) != MyData.iErr_Succ)
                        {
                            MyTools.ShowMsg("读取数据失败!", "读取gg头数据错误 = " + sErr);
                            gg1.ggFileClose();
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
                            gg1.ggFileClose();
                            return;
                        }
                        gg1.ggFileClose();


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

                        if (!gg1.ReadMarkFromDB(int.Parse(Select_Record_struct.FileID.ToString()),
                                               int.Parse(Select_Record_struct.RecordID.ToString()),
                                               ref gg1.ggMark, ref sErr))
                        {
                            MyTools.ShowMsg("读取数据失败!", "读取标记数据失败 = " + sErr);
                            return;
                        }

                        // 两个图像的显示
                        gg1.iCurrentPos = 0;
                        //Gen2Current = gg1.Gen2Array[0];
                        PB.pGen2 = Gen2Current;
                        PB.Gen2ToData();                        // 转变成 PictData
                        PB.Gen2ToPict();
                        PatientPict_Current = PB.Pict;
                        image_SelectImage.Source = PatientPict_Current;

                        UserControl_ReadFileBar.iBarValue = 100;
                        MyTools.DoEvents();

                        PB.gg1 = gg1;
                        MarkPict = new RecordMarkImage[gg1.ggData.numberof_frames];
                        if (sPos == "Left")
                        {
                            MarkPict_Left = MarkPict;
                            PB_Left = PB;
                            gg1_Left = gg1;
                        }
                        else
                        {
                            MarkPict_Right = MarkPict;
                            PB_Right = PB;
                            gg1_Right = gg1;
                        }

                        // 标尺的更改
                        image_SelectRecord.Source = null;
                        canvas_RecordLeftRule.Children.Clear();
                        canvas_RecordBottomRule.Children.Clear();

                        DrawBigPictRules(sPos,1);
                        DrawRecordRules(sPos,1);           // gg时，下面的标尺需要画

                        // 下面的控制能用
                        grid_SelectControl.IsEnabled = true;
                        SetPlayKey(sPos, true);

                        PB.GGRecordToPict(PB.RecordPictWidth);
                        image_SelectRecord.Source = PB.RecordPict;
                        DrawCutLine(sPos, 1);

                        //draw Record Mark
                        ClearAllMark(sPos);
                        DrawMarkPict(sPos, - 1, "Draw");                             // 全部画出来

                        // Draw Pos
                        DrawCurrentPos(sPos, gg1.iCurrentPos);

                        Pop_ReadFileBar.StaysOpen = false;
                        Pop_ReadFileBar.IsOpen = false;
                    }

                    if (sPos == "Left")
                    {
                        ZoomIsPress_Left = false;
                        BigPictIsPress_Left = false;
                        button_Zoom_Left_Click();
                    }
                    else
                    {
                        ZoomIsPress_Right = false;
                        BigPictIsPress_Right = false;
                        button_Zoom_Right_Click();
                    }

                    if (image_RightPict.Source != null && image_LeftPict.Source != null)
                    {
                        button_Link.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    image_SelectImage.Source = null;
                    image_SelectRecord.Source = null;
                    DrawBigPictRules(sPos, -1);
                    DrawRecordRules(sPos, -1);
                    DrawCurrentPos(sPos, -1);
                    DrawCutLine(sPos, -1);
                    grid_SelectControl.IsEnabled = false;
                }

            }
        }

        // 选边
        private void Border_RightPictBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            sCurrentPos = "Right";
            Border_RightPictBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF29C07"));    // new SolidColorBrush(Colors.Red);
            Border_LeftPictBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF437590")); 
        }
        private void Border_LeftPictBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            sCurrentPos = "Left";
            Border_LeftPictBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF29C07"));    // new SolidColorBrush(Colors.Red);
            Border_RightPictBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF437590"));
        }


        // 左右选择档案
        private void button_LeftRecord_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Name == "button_LeftPatient")
            {
                PopUp_RecordInfo("Left");
            }
            else
            {
                PopUp_RecordInfo("Right");
            }

            Pop_ForRecord.PlacementTarget = (Button)sender;
            Pop_ForRecord.HorizontalOffset = ((Button)sender).Width;
            Pop_ForRecord.VerticalOffset = 0;
            Pop_ForRecord.IsOpen = true;
        }
        private void PopUp_RecordInfo(string sPos)
        {
            MyData.PatientInfo_Struct Select_Patient_struct;      // 用于保存选定位置的人员信息
            MyData.CheckRecord_Struct Select_Record_struct;       // 用于保存选定位置的记录信息
            if (sPos == "Left")
            {
                Select_Patient_struct = AllPatient_struct[SelectID_Left];
                Select_Record_struct = Record_Left;
            }
            else
            {
                Select_Patient_struct = AllPatient_struct[SelectID_Right];
                Select_Record_struct = Record_Right;
            }

            this.Record_PatientID.Content = Select_Patient_struct.PatientID;
            this.Record_PatientName.Content = Select_Patient_struct.Name;
            this.Record_PatientSex.Content = Select_Patient_struct.Sex;
            this.Record_PatientBirthday.Content = Select_Patient_struct.Birthday;
            this.Record_PatientAddr.Content = Select_Patient_struct.Address;
            this.Record_PatientTele.Content = Select_Patient_struct.Tele;
            this.Record_PatientIdentifyID.Content = Select_Patient_struct.IdentifyID;
            this.Record_PatientMemo.Text = Select_Patient_struct.Memo;
            this.Record_CheckTime.Content = Select_Record_struct.CheckTime;
            this.Record_Doctor.Content = Select_Record_struct.Doctor;
            this.Record_CheckInfo.Text = Select_Record_struct.CheckInfo;
        }
        private void button_RecordSelect_Click(object sender, RoutedEventArgs e)
        {
            int iButtonID = iPatientForSelect_Button_Current;
            MyData.CheckRecord_Struct Select_Record_struct = new MyData.CheckRecord_Struct();
            Select_Record_struct.RecordID = Record_DataTable.Rows[datagrid_Record.SelectedIndex]["RecordID"].ToString();
            Select_Record_struct.FileID = Record_DataTable.Rows[datagrid_Record.SelectedIndex]["FileID"].ToString();
            Select_Record_struct.CheckTime = Record_DataTable.Rows[datagrid_Record.SelectedIndex]["CheckTime"].ToString();
            Select_Record_struct.Doctor = Record_DataTable.Rows[datagrid_Record.SelectedIndex]["Doctor"].ToString();
            Select_Record_struct.CheckInfo = Record_DataTable.Rows[datagrid_Record.SelectedIndex]["CheckInfo"].ToString();
            Select_Record_struct.SmallPict = Record_DataTable.Rows[datagrid_Record.SelectedIndex]["SmallPict"].ToString();
            Select_Record_struct.BigPict = Record_DataTable.Rows[datagrid_Record.SelectedIndex]["BigPict"].ToString();

            if (sCurrentPos == "Left")
            {
                SelectID_Left = iPatientForSelect_Start + iButtonID;
                Record_Left = Select_Record_struct;
            }
            else
            {
                SelectID_Right = iPatientForSelect_Start + iButtonID;
                Record_Right = Select_Record_struct;
            }
            DispPict(sCurrentPos);

            Pop_ForPatient.IsOpen = false;
        }
        private void button_RecordExit_Click(object sender, RoutedEventArgs e)
        {
            Pop_ForRecord.IsOpen = false;
        }
        private void button_PatientExit_Click(object sender, RoutedEventArgs e)
        {
            Pop_ForPatient.IsOpen = false;
        }

        frm_SearchPatient Frm_SearchPatient2;
        private void button_Search_Click(object sender, RoutedEventArgs e)
        {
            Frm_SearchPatient2 = new frm_SearchPatient();
            Frm_SearchPatient2.IsFirst = false;
            Frm_SearchPatient2.sSearchString = "";
            Frm_SearchPatient2.IsShowInput = true;
            Frm_SearchPatient2.pfrm_Start = this;
            Frm_SearchPatient2.Owner = this;
            this.ShowInTaskbar = false;
            this.IsEnabled = false;
            IsReturn = true;
            Frm_SearchPatient2.ShowDialog();
        }

        public void SearchBack()
        {
            MyData.PatientInfo_Struct Select_Patient_struct;
            MyData.CheckRecord_Struct Select_Record_struct;

            int iSelectID = -1;

            if (Frm_SearchPatient2.IsSelectNew)    // 有选中返回
            {
                Select_Patient_struct = Frm_SearchPatient2.Patient_New;
                for (int i = 0; i < AllPatient_struct.Length; i++)
                {
                    if (AllPatient_struct[i].FileID == Select_Patient_struct.FileID)
                    {
                        iSelectID = i;
                        break;
                    }
                }

                Select_Record_struct = new MyData.CheckRecord_Struct();
                if (Frm_SearchPatient2.iSelectRecordID >= 0)
                {
                    for (int i = 0; i < Frm_SearchPatient2.Record_New.Length; i++)
                    {
                        if (Frm_SearchPatient2.Record_New[i].RecordID == Frm_SearchPatient2.iSelectRecordID.ToString())
                        {
                            Select_Record_struct = Frm_SearchPatient2.Record_New[i];
                            break;
                        }
                    }
                }

                if (sCurrentPos == "Left")
                {
                    SelectID_Left = iPatientForSelect_Start + iSelectID;
                    Record_Left = Select_Record_struct;
                }
                else
                {
                    SelectID_Right = iPatientForSelect_Start + iSelectID;
                    Record_Right = Select_Record_struct;
                }

                DispPict(sCurrentPos);
            }
            else    // 没选中返回就不变
            {
            }
            Frm_SearchPatient2.Close();
            Frm_SearchPatient2 = null;
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            if (IsReturn)
            {
                IsReturn = false;
                SearchBack();
            }
        }


        // 画标尺
        private void DrawBigPictRules(string sPosition, int iMode )   // 当 UnitPoint_X>0 UnitPoint_Y>0 才需要画
        {
            if (iMode == -1)         // 擦除
            {
                if (sPosition == "Left")
                {
                    this.canvas_LeftPictLeftRule.Children.Clear();
                    this.canvas_LeftPictBottomRule.Children.Clear(); 
                }
                else
                {
                    this.canvas_RightPictLeftRule.Children.Clear();
                    this.canvas_RightPictBottomRule.Children.Clear();
                }
                return;
            }

            Canvas ThisCanvas;
            int iPos = 0;
            double iSmallStart = 0;
            double iBigStart = 0;
            double iEnd = 0;
            double UnitPoint_X, UnitPoint_Y;

            // BigPict (X)bottom Rule
            if (sPosition == "Left")
            {
                ThisCanvas = this.canvas_LeftPictBottomRule;
                UnitPoint_X = Canvas_LeftPict1.ActualWidth / (MyData.dBigPictWidth / MyData.dAirRate);
                UnitPoint_X *= LeftSfr.ScaleX;
                UnitPoint_Y = Canvas_LeftPict1.ActualHeight / (MyData.dBigPictWidth / MyData.dAirRate);
                UnitPoint_Y *= LeftSfr.ScaleY;
            }
            else
            {
                ThisCanvas = this.canvas_RightPictBottomRule;
                UnitPoint_X = Canvas_RightPict1.ActualWidth / (MyData.dBigPictWidth / MyData.dAirRate);
                UnitPoint_X *= RightSfr.ScaleX;
                UnitPoint_Y = Canvas_RightPict1.ActualHeight / (MyData.dBigPictWidth / MyData.dAirRate);
                UnitPoint_Y *= RightSfr.ScaleY;
            }

            SolidColorBrush SC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));         // ("#FF696969")   ("#FF797979")
            ThisCanvas.Children.Clear();
            if (UnitPoint_X > 0)
            {
                Line x_axis = new Line();//x轴
                x_axis.Stroke = SC;
                x_axis.StrokeThickness = 2;
                x_axis.X1 = 0;
                x_axis.Y1 = ThisCanvas.ActualHeight - 1;
                x_axis.X2 = ThisCanvas.ActualWidth;
                x_axis.Y2 = ThisCanvas.ActualHeight - 1;
                ThisCanvas.Children.Add(x_axis);

                // X轴标尺
                iPos = 1;
                iSmallStart = ThisCanvas.ActualHeight - 5;
                iBigStart = ThisCanvas.ActualHeight - 10;
                iEnd = ThisCanvas.ActualHeight - 1;
                while (iPos * UnitPoint_X <= ThisCanvas.ActualWidth)
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

                    ThisCanvas.Children.Add(x_axis);

                    iPos++;
                }
            }

            // BigPict Y(Left) Rule
            if (sPosition == "Left")
            {
                ThisCanvas = this.canvas_LeftPictLeftRule;
            }
            else
            {
                ThisCanvas = this.canvas_RightPictLeftRule;
            }

            ThisCanvas.Children.Clear();
            if (UnitPoint_Y > 0)
            {
                Line y_axis = new Line();//x轴
                                         //y_axis.Stroke = System.Windows.Media.Brushes.DarkGray;
                y_axis.Stroke = SC;
                y_axis.StrokeThickness = 2;
                y_axis.X1 = 0;
                y_axis.Y1 = 0;
                y_axis.X2 = 0;
                y_axis.Y2 = ThisCanvas.ActualHeight;
                ThisCanvas.Children.Add(y_axis);

                // Y轴标尺
                iPos = 1;
                iSmallStart = 5;
                iBigStart = 10;
                iEnd = 0;

                while (iPos * UnitPoint_Y <= ThisCanvas.ActualHeight)
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

                    ThisCanvas.Children.Add(y_axis);

                    iPos++;
                }
            }
        }
        private void DrawRecordRules(string sPosition, int iMode)
        {
            if (iMode == -1)         // 擦除
            {
                if (sPosition == "Left")
                {
                    this.canvas_RecordLeftRule_Left.Children.Clear();
                    this.canvas_RecordBottomRule_Left.Children.Clear();
                }
                else
                {
                    this.canvas_RecordLeftRule_Right.Children.Clear();
                    this.canvas_RecordBottomRule_Right.Children.Clear();
                }
                return;
            }

            Canvas ThisCanvasLeftRule, ThisCanvasBottomRule;
            SolidColorBrush SC = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5386A1"));
            int iPos = 0;
            double iSmallStart = 0;
            double iBigStart = 0;
            int UnitPoint_X = 10, UnitPoint_Y = 2;

            if (sPosition == "Left")
            {
                ThisCanvasLeftRule = this.canvas_RecordLeftRule_Left;
                ThisCanvasBottomRule = this.canvas_RecordBottomRule_Left;
                if (gg1_Left.ggData.numberof_frames > 0)
                    UnitPoint_X = (int)((canvas_RecordBottomRule_Left.ActualWidth - canvas_RecordLeftRule_Left.ActualWidth) / gg1_Left.ggData.numberof_frames);
                else
                    UnitPoint_X = 5;

                if (canvas_RecordLeftRule_Left.ActualHeight > 0)
                    UnitPoint_Y = (int)(canvas_RecordLeftRule_Left.ActualHeight / (MyData.dBigPictWidth / MyData.dAirRate));
            }
            else
            {
                ThisCanvasLeftRule = this.canvas_RecordLeftRule_Right;
                ThisCanvasBottomRule = this.canvas_RecordBottomRule_Right;
                if (gg1_Right.ggData.numberof_frames > 0)
                    UnitPoint_X = (int)((canvas_RecordBottomRule_Right.ActualWidth - canvas_RecordLeftRule_Right.ActualWidth) / gg1_Right.ggData.numberof_frames);
                else
                    UnitPoint_X = 5;

                if (canvas_RecordLeftRule_Right.ActualHeight > 0)
                    UnitPoint_Y = (int)(canvas_RecordLeftRule_Right.ActualHeight / (MyData.dBigPictWidth / MyData.dAirRate));
            }

            // Record X(bottom) Rule
            ThisCanvasBottomRule.Children.Clear();
            Line Record_x_axis = new Line();//x轴
            Record_x_axis.Stroke = SC;
            Record_x_axis.StrokeThickness = 2;
            Record_x_axis.X1 = 0;
            Record_x_axis.Y1 = 0;
            Record_x_axis.X2 = ThisCanvasBottomRule.ActualWidth;
            Record_x_axis.Y2 = 0;
            ThisCanvasBottomRule.Children.Add(Record_x_axis);

            // X轴标尺
            iPos = 0;
            iSmallStart = 5;
            iBigStart = 10;
            while (iPos * UnitPoint_X <= ThisCanvasBottomRule.ActualWidth)
            {
                Record_x_axis = new Line();//x轴
                Record_x_axis.Stroke = SC;
                Record_x_axis.StrokeThickness = 1;
                Record_x_axis.X1 = ThisCanvasLeftRule.ActualWidth + iPos * UnitPoint_X;
                Record_x_axis.X2 = Record_x_axis.X1;
                Record_x_axis.Y2 = 0;

                if (iPos % 5 == 0)      // 大标注
                    Record_x_axis.Y1 = iBigStart;
                else
                    Record_x_axis.Y1 = iSmallStart;

                ThisCanvasBottomRule.Children.Add(Record_x_axis);

                iPos++;
            }

            // Record Y(Left) Rule
            ThisCanvasLeftRule.Children.Clear();
            Line Record_y_axis = new Line();//x轴
            //y_axis.Stroke = System.Windows.Media.Brushes.DarkGray;
            Record_y_axis.Stroke = SC;
            Record_y_axis.StrokeThickness = 2;
            Record_y_axis.X1 = 0;
            Record_y_axis.Y1 = 0;
            Record_y_axis.X2 = 0;
            Record_y_axis.Y2 = ThisCanvasLeftRule.ActualHeight;
            ThisCanvasLeftRule.Children.Add(Record_y_axis);

            // Y轴标尺
            iPos = 0;
            iSmallStart = 5;
            iBigStart = 10;

            while (iPos * UnitPoint_Y <= Grid_Record_Left.ActualHeight/2)   // 上半截
            {
                Record_y_axis = new Line();//x轴
                Record_y_axis.Stroke = SC;
                Record_y_axis.StrokeThickness = 1;
                Record_y_axis.Y1 = ThisCanvasBottomRule.ActualHeight + Grid_Record_Left.ActualHeight/2 - iPos * UnitPoint_Y;
                Record_y_axis.X2 = 0;
                Record_y_axis.Y2 = Record_y_axis.Y1;

                if (iPos % 5 == 0)      // 大标注
                    Record_y_axis.X1 = iBigStart;
                else
                    Record_y_axis.X1 = iSmallStart;

                ThisCanvasLeftRule.Children.Add(Record_y_axis);

                Record_y_axis = new Line();//x轴                          // 下半截
                Record_y_axis.Stroke = SC;
                Record_y_axis.StrokeThickness = 1;
                Record_y_axis.Y1 = ThisCanvasBottomRule.ActualHeight + Grid_Record_Left.ActualHeight / 2 + iPos * UnitPoint_Y;
                Record_y_axis.X2 = 0;
                Record_y_axis.Y2 = Record_y_axis.Y1;

                if (iPos % 5 == 0)      // 大标注
                    Record_y_axis.X1 = iBigStart;
                else
                    Record_y_axis.X1 = iSmallStart;

                ThisCanvasLeftRule.Children.Add(Record_y_axis);

                iPos++;
            }
        }

        /// <summary>
        /// 彩色控制 =====================================================
        /// </summary>
        private void button_Color_Click(object sender, RoutedEventArgs e)
        {
            int iSelectID = -1;
            //Label label_SelectLabel = null;
            Image image_BigPict = null;
            //Image image_SelectRecord = null;
            //Grid grid_SelectControl = null;
            //Button button_SelectPatientButton = null;
            //Canvas canvas_BigPictLeftRule;
            //Canvas canvas_BigPictBottomRule;
            //Canvas canvas_RecordLeftRule;
            //Canvas canvas_RecordBottomRule;

            //MyData.PatientInfo_Struct Select_Patient_struct;      // 用于保存选定位置的人员信息
            //MyData.CheckRecord_Struct Select_Record_struct;       // 用于保存选定位置的记录信息

            //mod_ReadGen2.Gen2_Struct Gen2Current = new mod_ReadGen2.Gen2_Struct();
            mod_ShowPict PB; ;
            BitmapImage PatientPict_Current;

            //string sTmp = "";
            //DateTime dt;

            if (sCurrentPos == "Left")
            {
                iSelectID = SelectID_Left;
                //label_SelectLabel = label_LeftPatient;
                image_BigPict = image_LeftPict;
                //image_SelectRecord = image_LeftRecord;
                //grid_SelectControl = Grid_Control_Left;
                //button_SelectPatientButton = button_LeftPatient;

                //Gen2Current = Gen2Current_Left;
                PB = PB_Left;
                PatientPict_Current = PatientPict_Current_Left;
                //image_SelectRecord = image_LeftRecord;
                //grid_SelectControl = Grid_Control_Left;

                //canvas_BigPictLeftRule = canvas_LeftPictLeftRule;
                //canvas_BigPictBottomRule = canvas_LeftPictBottomRule;
                //canvas_RecordLeftRule = canvas_LeftRecordLeftRule;
                //canvas_RecordBottomRule = canvas_LeftRecordBottomRule;
            }
            else
            {
                iSelectID = SelectID_Right;
                //label_SelectLabel = label_RightPatient;
                image_BigPict = image_RightPict;
                //image_SelectRecord = image_RightRecord;
                //grid_SelectControl = Grid_Control_Right;
                //button_SelectPatientButton = button_RightPatient;

                //Gen2Current = Gen2Current_Right;
                PB = PB_Right;
                PatientPict_Current = PatientPict_Current_Right;
                //image_SelectRecord = image_RightRecord;
                //grid_SelectControl = Grid_Control_Right;

                //canvas_BigPictLeftRule = canvas_RightPictLeftRule;
                //canvas_BigPictBottomRule = canvas_RightPictBottomRule;
                //canvas_RecordLeftRule = canvas_RightRecordLeftRule;
                //canvas_RecordBottomRule = canvas_RightRecordBottomRule;
            }

            if (iSelectID == -1)    // 还没有选择病人
            {
                return;
            }

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
        private void listBox_Color_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int iSelectID = -1;
            Image image_BigPict = null;
            mod_ShowPict PB; ;
            BitmapImage PatientPict_Current;

            if (sCurrentPos == "Left")
            {
                iSelectID = SelectID_Left;
                image_BigPict = image_LeftPict;
                PB = PB_Left;
                PatientPict_Current = PatientPict_Current_Left;
            }
            else
            {
                iSelectID = SelectID_Right;
                image_BigPict = image_RightPict;
                PB = PB_Right;
                PatientPict_Current = PatientPict_Current_Right;
            }

            if (iSelectID == -1)    // 还没有选择病人
            {
                return;
            }

            switch (UserControl_Color.listBox_Color.SelectedIndex)
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
            // 两个图像的显示
            PB.Gen2ToPict();
            PatientPict_Current = PB.Pict;
            image_BigPict.Source = PatientPict_Current;
        }


        /// <summary>
        /// 亮度控制 =====================================================
        /// 亮度、对比度、平滑度在屏幕上的显示范围是 = -100 -- 0 -- 100，共 200 分格 
        /// 但在ShowPict()功能中，变化的范围是： -255 -- 255 共500分格
        /// 所以屏幕上每分隔 = 显示实际值的2.5
        /// </summary>
        //double PointsPerGrid = 2.5;              // 屏幕上每分隔的点数
        bool IsBrightSlideMouseDown = false;
        Point Pos0;
        private void button_Bright_Click(object sender, RoutedEventArgs e)
        {
            int iSelectID = -1;
            Image image_BigPict = null;
            mod_ShowPict PB; ;
            BitmapImage PatientPict_Current;

            if (sCurrentPos == "Left")
            {
                iSelectID = SelectID_Left;
                PB = PB_Left;
                PatientPict_Current = PatientPict_Current_Left;
                image_BigPict = image_LeftPict;
            }
            else
            {
                iSelectID = SelectID_Right;
                PB = PB_Right;
                PatientPict_Current = PatientPict_Current_Right;
                image_BigPict = image_RightPict;
            }

            if (iSelectID == -1)    // 还没有选择病人
            {
                return;
            }

            double dPictWidthEachPoint = (UserControl_Bright.line_BrightBar.X2 - UserControl_Bright.line_BrightBar.X1) / 200;     // Bar上 单位长度的点数

            // Bright Init  图像亮度调整值 (-100,100) ==================================================
            double dPictValue = PB.PictBrightness;                          // 屏幕刻度值
            UserControl_Bright.label_BrightValue.Content = "亮度：" + Math.Round(dPictValue, 0).ToString();

            double dLeft = 100 + PB.PictBrightness;                        // 游标位置
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
            int iSelectID = -1;
            Image image_BigPict = null;
            mod_ShowPict PB; ;
            BitmapImage PatientPict_Current;

            if (sCurrentPos == "Left")
            {
                iSelectID = SelectID_Left;
                PB = PB_Left;
                PatientPict_Current = PatientPict_Current_Left;
                image_BigPict = image_LeftPict;
            }
            else
            {
                iSelectID = SelectID_Right;
                PB = PB_Right;
                PatientPict_Current = PatientPict_Current_Right;
                image_BigPict = image_RightPict;
            }

            if (iSelectID == -1)    // 还没有选择病人
            {
                return;
            }

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
                dLeft = 100 + PB.PictBrightness;                        // 游标位置
                ThisImageSlide = UserControl_Bright.image_BrightSlide;
            }
            else if (ThisButton.Name == "button_ContractDec")
            {
                dPictValue = PB.PictContract;                           // 屏幕刻度值
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

            // 画图
            PatientPict_Current = PB.Pict;
            image_BigPict.Source = PatientPict_Current;

            Thickness thick = ThisImageSlide.Margin;
            thick.Left = dLeft * dPictWidthEachPoint;                                       // 乘上 Bar上单位长度的点数
            ThisImageSlide.Margin = thick;
        }
        private void button_BrightAdd_Click(object sender, RoutedEventArgs e)
        {
            int iSelectID = -1;
            Image image_BigPict = null;
            mod_ShowPict PB; ;
            BitmapImage PatientPict_Current;

            if (sCurrentPos == "Left")
            {
                iSelectID = SelectID_Left;
                PB = PB_Left;
                PatientPict_Current = PatientPict_Current_Left;
                image_BigPict = image_LeftPict;
            }
            else
            {
                iSelectID = SelectID_Right;
                PB = PB_Right;
                PatientPict_Current = PatientPict_Current_Right;
                image_BigPict = image_RightPict;
            }

            if (iSelectID == -1)    // 还没有选择病人
            {
                return;
            }

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

            // 画图
            PatientPict_Current = PB.Pict;
            image_BigPict.Source = PatientPict_Current;

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
            int iSelectID = -1;
            Image image_BigPict = null;
            mod_ShowPict PB; ;
            BitmapImage PatientPict_Current;

            if (sCurrentPos == "Left")
            {
                iSelectID = SelectID_Left;
                PB = PB_Left;
                PatientPict_Current = PatientPict_Current_Left;
                image_BigPict = image_LeftPict;
            }
            else
            {
                iSelectID = SelectID_Right;
                PB = PB_Right;
                PatientPict_Current = PatientPict_Current_Right;
                image_BigPict = image_RightPict;
            }

            if (iSelectID == -1)    // 还没有选择病人
            {
                return;
            }

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

                Pos0 = Pos1;
                //dBrightLeft = dBrightLeft_New;
            }
        }
        private void image_BrightSlide_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsBrightSlideMouseDown = false;
            FrameworkElement ele = sender as FrameworkElement;
            ele.ReleaseMouseCapture();
        }


        /// <summary>
        /// 放大与缩小控制  + 测量入口 ===================================
        /// </summary>
        bool ZoomIsPress_Left = true;
        bool BigPictIsPress_Left = false;
        Point BigPictPos0_Left, BigPictPos1_Left;        // 专为BigPict而设的鼠标位置变量

        bool ZoomIsPress_Right = true;
        bool BigPictIsPress_Right = false;
        Point BigPictPos0_Right, BigPictPos1_Right;        // 专为BigPict而设的鼠标位置变量
        
        struct struct_Lines
        {
            public Line Line1;
            public Ellipse C1, C2;
            public Rectangle R;
            public int SetLinePoint;               // 本次操作是对应线条的那一端： 1=前端, 2=后端 
        }
        List<struct_Lines> DetectLine_Left = null;
        List<struct_Lines> DetectLine_Right = null;

        private void button_Zoom_Left_Click(object sender, RoutedEventArgs e)
        {
            button_Zoom_Left_Click();
        }
        private void button_Zoom_Left_Click()
        {
            if (ZoomIsPress_Left)
            {
                ZoomIsPress_Left = false;
                button_Zoom_Left.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                image_LeftPict.Cursor = Cursors.Arrow;

                LeftSfr.ScaleX = 1;
                LeftSfr.ScaleY = 1;
                LeftSfr.CenterX = image_LeftPict.ActualWidth / 2;
                LeftSfr.CenterY = image_LeftPict.ActualHeight / 2;
                //image_LeftPict.RenderTransform = LeftSfr;
            }
            else
            {
                ZoomIsPress_Left = true;
                button_Zoom_Left.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Cyan"));
                image_LeftPict.Cursor = Cursors.Hand;
            }
            BigPictIsPress_Left = false;
        }

        private void button_Zoom_Right_Click(object sender, RoutedEventArgs e)
        {
            button_Zoom_Right_Click();
        }
        private void button_Zoom_Right_Click()
        {
            if (ZoomIsPress_Right)
            {
                ZoomIsPress_Right = false;
                button_Zoom_Right.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                image_RightPict.Cursor = Cursors.Arrow;

                RightSfr.ScaleX = 1;
                RightSfr.ScaleY = 1;
                RightSfr.CenterX = image_RightPict.ActualWidth / 2;
                RightSfr.CenterY = image_RightPict.ActualHeight / 2;
                //image_RightPict.RenderTransform = RightSfr;
            }
            else
            {
                ZoomIsPress_Right = true;
                button_Zoom_Right.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Cyan"));
                image_RightPict.Cursor = Cursors.Hand;
            }
            BigPictIsPress_Right = false;
        }

        private void image_LeftPict_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Image ThisBigImage = sender as Image;
            double d;
            double Rota = 0.05;
            double ThisScale;
            ScaleTransform Sfr;
            TranslateTransform translater;
            Canvas Canvas_BigPict2;
            string sPosition;

            if (ZoomIsPress_Left && (!Pop_Detect_Left.IsOpen || !Pop_Detect_Right.IsOpen) )
            {

                if (ThisBigImage.Name == "image_LeftPict" && !Pop_Detect_Left.IsOpen)
                {
                    Sfr = LeftSfr;
                    Canvas_BigPict2 = Canvas_LeftPict2;
                    translater = LeftTranslater;
                    sPosition = "Left";
                }
                else if (ThisBigImage.Name == "image_RightPict" && !Pop_Detect_Right.IsOpen)
                {
                    Sfr = RightSfr;
                    Canvas_BigPict2 = Canvas_RightPict2;
                    translater = RightTranslater;
                    sPosition = "Right";
                }
                else
                    return;

                d = e.Delta / Math.Abs(e.Delta);
                Rota = 0.05;
                ThisScale = Sfr.ScaleX + d * Rota;

                if (ThisScale < 1 && d < 0)
                {
                    ThisScale = 1;
                    Rota = (ThisScale - Sfr.ScaleX) / d;
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
                Sfr.ScaleX = ThisScale;
                Sfr.ScaleY = ThisScale;

                //获取鼠标在缩放之后的目标上的位置
                Point targetZoomFocus2 = new Point(targetZoomFocus1.X * (1 + d * Rota), targetZoomFocus1.Y * (1 + d * Rota));

                //获取目标在缩放之后的Rect
                Rect afterScaleRect = Canvas_BigPict2.RenderTransform.TransformBounds(new Rect(Canvas_BigPict2.RenderSize));

                //算的缩放前后鼠标的位置间的差
                Vector v = targetZoomFocus2 - targetZoomFocus1;


                //减去鼠标点在缩放前后之间的差值，实际上就是以鼠标点为中心进行缩放
                translater.X -= v.X;
                translater.Y -= v.Y;

                // 调整标尺
                DrawBigPictRules(sPosition, 1);

                if (bIsLink)
                {
                    if (sPosition == "Left")
                    {
                        RightSfr.ScaleX = LeftSfr.ScaleX;
                        RightSfr.ScaleY = LeftSfr.ScaleY;
                        RightTranslater.X = LeftTranslater.X;
                        RightTranslater.Y = LeftTranslater.Y;
                    }
                    else
                    {
                        LeftSfr.ScaleX = RightSfr.ScaleX;
                        LeftSfr.ScaleY = RightSfr.ScaleY;
                        LeftTranslater.X = RightTranslater.X;
                        LeftTranslater.Y = RightTranslater.Y;
                    }
                }
            }
        }
        private void image_LeftPict_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BigPictIsPress_Left = false;
            BigPictIsPress_Right = false;
            FrameworkElement fEle = sender as FrameworkElement;
            if (fEle.Name == "image_LeftPict" && sCurrentPos != "Left")
            {
                Border_LeftPictBorder_MouseDown(sender, e);
            }
            else if (fEle.Name == "image_RightPict" && sCurrentPos != "Right")
            {
                Border_RightPictBorder_MouseDown(sender, e);
            }
            else if (fEle.Name == "image_LeftPict" && sCurrentPos == "Left" && ZoomIsPress_Left)
            {
                BigPictIsPress_Left = true;
                BigPictPos0_Left = e.GetPosition(Canvas_LeftPict1);
                if (Pop_Detect_Left.IsOpen)
                {
                    fEle.CaptureMouse();
                    Detect_MouseLeftButtonDown("Left", sender, e);
                }
            }
            else if (fEle.Name == "image_RightPict" && sCurrentPos == "Right" && ZoomIsPress_Right)
            {
                BigPictIsPress_Right = true;
                BigPictPos0_Right = e.GetPosition(Canvas_RightPict1);
                if (Pop_Detect_Right.IsOpen)
                {
                    fEle.CaptureMouse();
                    Detect_MouseLeftButtonDown("Right", sender, e);
                }
            }
            else
            {
                if (Pop_Detect_Left.IsOpen || Pop_Detect_Right.IsOpen)
                {
                    Point P1 = e.GetPosition(Canvas_LeftPict1);
                    Point P2 = e.GetPosition(Canvas_RightPict1);
                    if (P1.X >= 0 && P1.X < Canvas_LeftPict1.ActualWidth && P1.Y >= 0 && P1.Y < Canvas_LeftPict1.ActualHeight)
                    {
                        fEle.CaptureMouse();
                        Detect_MouseLeftButtonDown("Left", sender, e);
                    }
                    else if (P2.X >= 0 && P2.X < Canvas_RightPict2.ActualWidth && P2.Y >= 0 && P2.Y < Canvas_RightPict1.ActualHeight)
                    {
                        fEle.CaptureMouse();
                        Detect_MouseLeftButtonDown("Right", sender, e);
                    }
                }
                //else      // 其他都不处理
                //{
                //}
                return;
            }
            fEle.CaptureMouse();
        }
        private void image_LeftPict_MouseMove(object sender, MouseEventArgs e)
        {
            if (Pop_Detect_Left.IsOpen && BigPictIsPress_Left && sCurrentPos == "Left") 
            {
                Detect_MouseLeftButtonMove("Left", sender, e);
            }
            else if (Pop_Detect_Right.IsOpen && BigPictIsPress_Right && sCurrentPos == "Right")
            {
                Detect_MouseLeftButtonMove("Right", sender, e);
            }
            else if ( (ZoomIsPress_Left && BigPictIsPress_Left && sCurrentPos == "Left" && !Pop_Detect_Left.IsOpen) || (ZoomIsPress_Right && BigPictIsPress_Right && sCurrentPos == "Right" && !Pop_Detect_Right.IsOpen) )         // 只有Zoom按键被设置，同时BigPict被指向的时候，本功能才启用
            {
                Image BigPict = (Image)sender;
                Point BigPictPos1;
                Point BigPictPos0;
                Canvas Canvas_BigPict1, Canvas_BigPict2;
                ScaleTransform sfr;
                TranslateTransform translater;

                if (BigPict.Name == "image_LeftPict" && sCurrentPos == "Left")
                {
                    BigPictPos1 = BigPictPos1_Left;
                    BigPictPos0 = BigPictPos0_Left;
                    Canvas_BigPict1 = Canvas_LeftPict1;
                    Canvas_BigPict2 = Canvas_LeftPict2;
                    sfr = LeftSfr;
                    translater = LeftTranslater;
                }
                else if (BigPict.Name == "image_RightPict" && sCurrentPos == "Right")
                {
                    BigPictPos1 = BigPictPos1_Right;
                    BigPictPos0 = BigPictPos0_Right;
                    Canvas_BigPict1 = Canvas_RightPict1;
                    Canvas_BigPict2 = Canvas_RightPict2;
                    sfr = RightSfr;
                    translater = RightTranslater;
                }
                else
                    return;

                // 当前鼠标位置
                BigPictPos1 = e.GetPosition(Canvas_BigPict1);

                // 移动值
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
                        if ((translater.Y + yPos) < (Canvas_BigPict1.ActualHeight - targetRect.Height))
                        {
                            translater.Y = Canvas_BigPict1.ActualHeight - targetRect.Height;
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
                        if ((translater.X + xPos) < (Canvas_BigPict1.ActualWidth - targetRect.Width))
                        {
                            translater.X = Canvas_BigPict1.ActualWidth - targetRect.Width;
                        }
                        else
                        {
                            translater.X += xPos;
                        }
                    }
                }
                if (sCurrentPos == "Left")
                {
                    BigPictPos0_Left = BigPictPos1;
                }
                else
                {
                    BigPictPos0_Right = BigPictPos1;
                }

                if (bIsLink)
                {
                    if (sCurrentPos == "Left")
                    {
                        RightSfr.ScaleX = LeftSfr.ScaleX;
                        RightSfr.ScaleY = LeftSfr.ScaleY;
                        RightTranslater.X = LeftTranslater.X;
                        RightTranslater.Y = LeftTranslater.Y;
                        BigPictPos0_Right = BigPictPos1_Right;
                    }
                    else
                    {
                        LeftSfr.ScaleX = RightSfr.ScaleX;
                        LeftSfr.ScaleY = RightSfr.ScaleY;
                        LeftTranslater.X = RightTranslater.X;
                        LeftTranslater.Y = RightTranslater.Y;
                        BigPictPos0_Left = BigPictPos1_Left;
                    }
                }
            }
        }
        private void image_LeftPict_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement ele = sender as FrameworkElement;
            if (Pop_Detect_Left.IsOpen && BigPictIsPress_Left)
            {
                Detect_MouseLeftButtonMove("Left", sender, e);
                BigPictIsPress_Left = false;
            }
            else if (Pop_Detect_Right.IsOpen && BigPictIsPress_Right)
            {
                Detect_MouseLeftButtonMove("Right", sender, e);
                BigPictIsPress_Right = false;
            }
            else   // 不是测量
            {
                image_LeftPict_MouseMove(sender, e);

                if (ele.Name == "image_LeftPict" && ZoomIsPress_Left && BigPictIsPress_Left)
                {
                    BigPictIsPress_Left = false;
                }
                else if (ele.Name == "image_RightPict" && ZoomIsPress_Right && BigPictIsPress_Right)
                {
                    BigPictIsPress_Right = false;
                }
            }
            ele.ReleaseMouseCapture();
        }

        /// <summary>
        /// Detect位置响应  ===================================
        /// </summary>
        private void button_Detect_Click(object sender, RoutedEventArgs e)
        {
            MyData.CheckRecord_Struct Record_Select;

            System.Windows.Controls.Primitives.Popup Pop_Detect;
            if (sCurrentPos == "Left")
            {
                Record_Select = Record_Left;
                Pop_Detect = Pop_Detect_Left;
            }
            else
            {
                Record_Select = Record_Right;
                Pop_Detect = Pop_Detect_Right;
            }

            if (Record_Select.BigPict == "" || Record_Select.BigPict == null)
            {
                //MyTools.ShowMsg("该区域没有检查记录，不能测量！", "");
            }
            else
            {
                Pop_Detect.IsOpen = true;
                Pop_Detect.StaysOpen = true;
                Detect_Clear(Pop_Detect.Name == "Pop_Detect_Left" ? "Left" : "Right");
            }
        }
        private void Detect_Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Grid Parent1 = VisualTreeHelper.GetParent(btn) as Grid;
            Border Parent2 = Parent1.Parent as Border;
            Control Parent3 = Parent2.Parent as Control;
            string sPos;

            System.Windows.Controls.Primitives.Popup Pop_Detect;
            if (Parent3.Name == "UserControl_Detect_Left")
            {
                Pop_Detect = Pop_Detect_Left;
                sPos = "Left";
            }
            else
            {
                Pop_Detect = Pop_Detect_Right;
                sPos = "Right";
            }

            Detect_Clear(sPos);
            Pop_Detect.IsOpen = false;
        }
        private void Detect_Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Grid Parent1 = VisualTreeHelper.GetParent(btn) as Grid;
            Border Parent2 = Parent1.Parent as Border;
            Control Parent3 = Parent2.Parent as Control;
            string sPos;

            System.Windows.Controls.Primitives.Popup Pop_Detect;
            if (Parent3.Name == "UserControl_Detect_Left")
            {
                Pop_Detect = Pop_Detect_Left;
                sPos = "Left";
            }
            else
            {
                Pop_Detect = Pop_Detect_Right;
                sPos = "Right";
            }

            Detect_Clear(sPos);
        }
        private void Detect_RadioButton_Line_Change(object sender, RoutedEventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            Grid Parent1 = VisualTreeHelper.GetParent(btn) as Grid;
            Border Parent2 = Parent1.Parent as Border;
            Control Parent3 = Parent2.Parent as Control;
            string sPos;

            System.Windows.Controls.Primitives.Popup Pop_Detect;
            if (Parent3.Name == "UserControl_Detect_Left")
            {
                Pop_Detect = Pop_Detect_Left;
                sPos = "Left";
            }
            else
            {
                Pop_Detect = Pop_Detect_Right;
                sPos = "Right";
            }

            Detect_Clear(sPos);
        }

        private void Detect_Clear(string sPos)
        {
            List<struct_Lines> DetectLine = null;
            UserControl_Detect userControl_Detect;
            Canvas Canvas_BigPict2;

            if (sPos == "Left")
            {
                DetectLine = DetectLine_Left;
                Canvas_BigPict2 = Canvas_LeftPict2;
                userControl_Detect = UserControl_Detect_Left;
                BigPictIsPress_Left = false;
            }
            else
            {
                DetectLine = DetectLine_Right;
                Canvas_BigPict2 = Canvas_RightPict2;
                userControl_Detect = UserControl_Detect_Right;
                BigPictIsPress_Right = false;
            }

            if (DetectLine != null)
            {
                for (int i = DetectLine.Count; i > 0; i--)
                {
                    Line im = Canvas_BigPict2.FindName("DetectLine" + (i - 1).ToString() + "_" + sPos) as Line;   //找到添加的线
                    if (im != null)//判断是否找到，以免在未添加前就误点了   
                    {
                        Canvas_BigPict2.Children.Remove(im);//移除对应按钮控件   
                        Canvas_BigPict2.UnregisterName("DetectLine" + (i - 1).ToString() + "_" + sPos);//还需要把对用的名字注销掉，否则再次点击Button_Add会报错   
                    };

                    Ellipse ie = Canvas_BigPict2.FindName("DetectBox0" + (i - 1).ToString() + "_" + sPos) as Ellipse;   //找到第一点的box
                    if (ie != null)//判断是否找到，以免在未添加前就误点了   
                    {
                        Canvas_BigPict2.Children.Remove(ie);//移除对应按钮控件   
                        Canvas_BigPict2.UnregisterName("DetectBox0" + (i - 1).ToString() + "_" + sPos);//还需要把对用的名字注销掉，否则再次点击Button_Add会报错   
                    };

                    ie = Canvas_BigPict2.FindName("DetectBox1" + (i - 1).ToString() + "_" + sPos) as Ellipse;   //找到第二点的box
                    if (ie != null)//判断是否找到，以免在未添加前就误点了   
                    {
                        Canvas_BigPict2.Children.Remove(ie);//移除对应按钮控件   
                        Canvas_BigPict2.UnregisterName("DetectBox1" + (i - 1).ToString() + "_" + sPos);//还需要把对用的名字注销掉，否则再次点击Button_Add会报错   
                    };

                    Rectangle ir = Canvas_BigPict2.FindName("DetectRect0" + (i - 1).ToString() + "_" + sPos) as Rectangle;   //找到第一点的box
                    if (ir != null)//判断是否找到，以免在未添加前就误点了   
                    {
                        Canvas_BigPict2.Children.Remove(ir);//移除对应按钮控件   
                        Canvas_BigPict2.UnregisterName("DetectRect0" + (i - 1).ToString() + "_" + sPos);//还需要把对用的名字注销掉，否则再次点击Button_Add会报错   
                    };
                }

            }
            DetectLine = null;

            userControl_Detect.TextBox_Length.Text = "";
            userControl_Detect.TextBox_Width.Text = "";
            userControl_Detect.TextBox_Perimeter.Text = "";
            userControl_Detect.TextBox_Area.Text = "";

            if (sPos == "Left")
            {
                DetectLine_Left = DetectLine;
            }
            else
            {
                DetectLine_Right = DetectLine;
            }
        }                                                           // 清除由测量动作所建立的东东
        private void Detect_MouseLeftButtonDown(string sPos, object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fEle = sender as FrameworkElement;
            fEle.CaptureMouse();
            //fEle.Cursor = Cursors.Hand;

            List<struct_Lines> DetectLine = null;
            UserControl_Detect userControl_Detect;
            Canvas Canvas_BigPict2;

            if (sPos == "Left")
            {
                DetectLine = DetectLine_Left;
                Canvas_BigPict2 = Canvas_LeftPict2;
                userControl_Detect = UserControl_Detect_Left;
                BigPictIsPress_Left = true;
            }
            else
            {
                DetectLine = DetectLine_Right;
                Canvas_BigPict2 = Canvas_RightPict2;
                userControl_Detect = UserControl_Detect_Right;
                BigPictIsPress_Right = true;
            }


            Point P0 = e.GetPosition(Canvas_BigPict2);       // 相对于Canvas_BigPict2的坐标

            if ((bool)userControl_Detect.RadioButton_Line.IsChecked)
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
                    NewOne.Line1.Name = "DetectLine0" + "_" + sPos;
                    NewOne.Line1.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.Line1.StrokeThickness = 1;

                    NewOne.C1 = new Ellipse();
                    NewOne.C1.Width = 3;
                    NewOne.C1.Height = 3;
                    NewOne.C1.Name = "DetectBox00" + "_" + sPos;
                    NewOne.C1.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.C1.StrokeThickness = 1;
                    NewOne.C1.Margin = new Thickness(NewOne.Line1.X1 - 1, NewOne.Line1.Y1 - 1, 0, 0);
                    NewOne.C1.MouseDown += new MouseButtonEventHandler(image_LeftPict_MouseLeftButtonDown);
                    NewOne.C1.MouseMove += new MouseEventHandler(image_LeftPict_MouseMove);
                    NewOne.C1.MouseUp += new MouseButtonEventHandler(image_LeftPict_MouseLeftButtonUp);

                    NewOne.C2 = new Ellipse();
                    NewOne.C2.Width = 3;
                    NewOne.C2.Height = 3;
                    NewOne.C2.Name = "DetectBox10" + "_" + sPos;
                    NewOne.C2.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.C2.StrokeThickness = 1;
                    NewOne.C2.Margin = new Thickness(NewOne.Line1.X2 - 1, NewOne.Line1.Y2 - 1, 0, 0);
                    NewOne.C2.MouseDown += new MouseButtonEventHandler(image_LeftPict_MouseLeftButtonDown);
                    NewOne.C2.MouseMove += new MouseEventHandler(image_LeftPict_MouseMove);
                    NewOne.C2.MouseUp += new MouseButtonEventHandler(image_LeftPict_MouseLeftButtonUp);

                    NewOne.SetLinePoint = 2;

                    DetectLine.Add(NewOne);
                    Canvas_BigPict2.Children.Add(DetectLine[0].Line1);
                    Canvas_BigPict2.RegisterName(DetectLine[0].Line1.Name, DetectLine[0].Line1);  //注册名字，以便以后使用
                    Canvas_BigPict2.Children.Add(DetectLine[0].C1);
                    Canvas_BigPict2.RegisterName(DetectLine[0].C1.Name, DetectLine[0].C1);        //注册名字，以便以后使用
                    Canvas_BigPict2.Children.Add(DetectLine[0].C2);
                    Canvas_BigPict2.RegisterName(DetectLine[0].C2.Name, DetectLine[0].C2);        //注册名字，以便以后使用

                    userControl_Detect.TextBox_Length.Text = "0.000";
                    userControl_Detect.TextBox_Width.Text = "";
                    userControl_Detect.TextBox_Perimeter.Text = "";
                    userControl_Detect.TextBox_Area.Text = "";
                }
                else       // 移动点
                {
                    struct_Lines OldOne = DetectLine[0];
                    if ((P0.X >= OldOne.Line1.X1 - 1) && (P0.X <= OldOne.Line1.X1 + 1) && (P0.Y >= OldOne.Line1.Y1 - 1) && (P0.Y <= OldOne.Line1.Y1 + 1))
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
            else if ((bool)userControl_Detect.RadioButton_Rectangle.IsChecked)
            {
                if (DetectLine == null)            // 第一次画点
                {
                    DetectLine = new List<struct_Lines>();
                    struct_Lines NewOne = new struct_Lines();

                    NewOne.R = new Rectangle();
                    NewOne.R.Width = 0;
                    NewOne.R.Height = 0;
                    NewOne.R.Margin = new Thickness(P0.X, P0.Y, P0.X, P0.Y);
                    NewOne.R.Name = "DetectRect00" + "_" + sPos;
                    NewOne.R.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.R.StrokeThickness = 1;
                    NewOne.R.Margin = new Thickness(P0.X, P0.Y, 0, 0);

                    NewOne.C1 = new Ellipse();
                    NewOne.C1.Width = 3;
                    NewOne.C1.Height = 3;
                    NewOne.C1.Name = "DetectBox00" + "_" + sPos;
                    NewOne.C1.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.C1.StrokeThickness = 1;
                    NewOne.C1.Margin = new Thickness(P0.X - 1, P0.Y - 1, 0, 0);
                    NewOne.C1.MouseDown += new MouseButtonEventHandler(image_LeftPict_MouseLeftButtonDown);
                    NewOne.C1.MouseMove += new MouseEventHandler(image_LeftPict_MouseMove);
                    NewOne.C1.MouseUp += new MouseButtonEventHandler(image_LeftPict_MouseLeftButtonUp);

                    NewOne.C2 = new Ellipse();
                    NewOne.C2.Width = 3;
                    NewOne.C2.Height = 3;
                    NewOne.C2.Name = "DetectBox10" + "_" + sPos;
                    NewOne.C2.Stroke = new SolidColorBrush(Colors.White);      // new SolidColorBrush(Color.FromArgb(0xff, 0x6e, 0x6e, 0x6e));
                    NewOne.C2.StrokeThickness = 1;
                    NewOne.C2.Margin = new Thickness(P0.X - 1, P0.Y - 1, 0, 0);
                    NewOne.C2.MouseDown += new MouseButtonEventHandler(image_LeftPict_MouseLeftButtonDown);
                    NewOne.C2.MouseMove += new MouseEventHandler(image_LeftPict_MouseMove);
                    NewOne.C2.MouseUp += new MouseButtonEventHandler(image_LeftPict_MouseLeftButtonUp);

                    NewOne.SetLinePoint = 2;

                    DetectLine.Add(NewOne);
                    Canvas_BigPict2.Children.Add(DetectLine[0].R);
                    Canvas_BigPict2.RegisterName(DetectLine[0].R.Name, DetectLine[0].R);  //注册名字，以便以后使用
                    Canvas_BigPict2.Children.Add(DetectLine[0].C1);
                    Canvas_BigPict2.RegisterName(DetectLine[0].C1.Name, DetectLine[0].C1);        //注册名字，以便以后使用
                    Canvas_BigPict2.Children.Add(DetectLine[0].C2);
                    Canvas_BigPict2.RegisterName(DetectLine[0].C2.Name, DetectLine[0].C2);        //注册名字，以便以后使用

                    userControl_Detect.TextBox_Length.Text = "0.0000";
                    userControl_Detect.TextBox_Width.Text = "0.0000";
                    userControl_Detect.TextBox_Perimeter.Text = "0.0000";
                    userControl_Detect.TextBox_Area.Text = "0.0000";
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

                if ((bool)userControl_Detect.RadioButton_Line.IsChecked)
                {
                    // 计算线长
                }
                else   // Any
                {
                }
            }

            if (sPos == "Left")
            {
                DetectLine_Left = DetectLine;
            }
            else
            {
                DetectLine_Right = DetectLine;
            }

        }
        private void Detect_MouseLeftButtonMove(string sPos, object sender, MouseEventArgs e)
        {
            // 前面已经判断是：Pop_Detect.IsOpen && BigPictIsPress
            Point P1;
            FrameworkElement fEle = sender as FrameworkElement;
            if (sPos == "Left")
            {
                P1 = e.GetPosition(Canvas_LeftPict2);
                Detect_MouseLeftButtonMoveAndUp("Left", "Move", P1);
            }
            else
            {
                P1 = e.GetPosition(Canvas_RightPict2);
                Detect_MouseLeftButtonMoveAndUp("Right", "Move", P1);
            }
        }
        private void Detect_MouseLeftButtonUp(string sPos, object sender, MouseButtonEventArgs e)
        {
            // 前面已经判断是：Pop_Detect.IsOpen && BigPictIsPress
            Point P1 ;
            FrameworkElement fEle = sender as FrameworkElement;
            if (sPos == "Left")
            {
                P1 = e.GetPosition(Canvas_LeftPict2);
                Detect_MouseLeftButtonMoveAndUp("Left", "Up", P1);
            }
            else
            {
                P1 = e.GetPosition(Canvas_RightPict2);
                Detect_MouseLeftButtonMoveAndUp("Right", "Up", P1);
            }
        }
        private void Detect_MouseLeftButtonMoveAndUp(string sPos, string sMode, Point P)
        {
            List<struct_Lines> DetectLine = null;
            UserControl_Detect userControl_Detect;
            Canvas Canvas_BigPict1;
            Canvas Canvas_BigPict2;

            if (sPos == "Left")
            {
                DetectLine = DetectLine_Left;
                Canvas_BigPict1 = Canvas_LeftPict1;
                Canvas_BigPict2 = Canvas_LeftPict2;
                userControl_Detect = UserControl_Detect_Left;
                BigPictIsPress_Left = true;
            }
            else
            {
                DetectLine = DetectLine_Right;
                Canvas_BigPict1 = Canvas_RightPict1;
                Canvas_BigPict2 = Canvas_RightPict2;
                userControl_Detect = UserControl_Detect_Right;
                BigPictIsPress_Right = true;
            }

            // 当前鼠标位置
            Point P1 = P;

            if ((bool)userControl_Detect.RadioButton_Line.IsChecked)           // Line
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
                if (sMode == "Up")
                {
                    OldOne.SetLinePoint = 0;
                }
                DetectLine[0] = OldOne;

                double LineLength_X = Math.Abs(DetectLine[0].Line1.X2 - DetectLine[0].Line1.X1);
                LineLength_X = (MyData.dBigPictWidth * LineLength_X) / (MyData.dAirRate * Canvas_BigPict1.ActualWidth);           //sfr.ScaleX * 

                double LineLength_Y = Math.Abs(DetectLine[0].Line1.Y2 - DetectLine[0].Line1.Y1);
                LineLength_Y = (MyData.dBigPictWidth * LineLength_Y) / (MyData.dAirRate * Canvas_BigPict1.ActualHeight);         // * sfr.ScaleY 

                double LineLength = Math.Sqrt(Math.Pow(LineLength_X, 2) + Math.Pow(LineLength_Y, 2));
                userControl_Detect.TextBox_Length.Text = LineLength.ToString("F4");
            }
            else if ((bool)userControl_Detect.RadioButton_Rectangle.IsChecked)
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
                if (sMode == "Up")
                {
                    OldOne.SetLinePoint = 0;
                }

                // 计算线长和面积
                DetectLine[0] = OldOne;

                P0 = new Point(OldOne.R.Margin.Left, OldOne.R.Margin.Top);
                P1 = new Point(OldOne.R.Margin.Left + OldOne.R.Width, OldOne.R.Margin.Top + OldOne.R.Height);

                double LineLength = Math.Abs(P1.X - P0.X);
                LineLength = (MyData.dBigPictWidth * LineLength) / (MyData.dAirRate * Canvas_BigPict1.ActualWidth);   // 毫米
                userControl_Detect.TextBox_Length.Text = LineLength.ToString("F4");

                double LineWidth = Math.Abs(P1.Y - P0.Y);
                LineWidth = (MyData.dBigPictWidth * LineWidth) / (MyData.dAirRate * Canvas_BigPict1.ActualHeight);   // 毫米
                userControl_Detect.TextBox_Width.Text = LineWidth.ToString("F4");

                double dd = 2 * (LineLength + LineWidth);
                userControl_Detect.TextBox_Perimeter.Text = dd.ToString("F4");

                dd = LineLength * LineWidth;
                userControl_Detect.TextBox_Area.Text = dd.ToString("F4");
            }
            else
            {
            }
            MyTools.DoEvents();
        }

        /// <summary>
        /// 播放速度控制  ===================================
        /// </summary>
        bool IsSpeedSlideMouseDown = false;
        public int iDelayMax = 5000;   // Max = 5 second
        public int iDelay = 100;
        private void button_Speed_Click(object sender, RoutedEventArgs e)
        {
            mod_GG gg1;
            int iDelay = 100;
            int iSelectID = -1;
            double dCutAngle = 0;

            if (sCurrentPos == "Left")
            {
                iSelectID = SelectID_Left;
                gg1 = gg1_Left;
                iDelay = iDelay_Left;
                dCutAngle = dCutAngle_Left;
            }
            else
            {
                iSelectID = SelectID_Right;
                gg1 = gg1_Right;
                iDelay = iDelay_Right;
                dCutAngle = dCutAngle_Right;
            }
            if (iSelectID == -1)    // 还没有选择病人
            {
                return;
            }

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
            mod_GG gg1;
            int iDelay = 100;
            double dCutAngle = 0;
            mod_ShowPict PB;
            Image image_Record;

            if (sCurrentPos == "Left")
            {
                gg1 = gg1_Left;
                iDelay = iDelay_Left;
                dCutAngle = dCutAngle_Left;
                PB = PB_Left;
                image_Record = image_Record_Left;
            }
            else
            {
                gg1 = gg1_Right;
                iDelay = iDelay_Right;
                dCutAngle = dCutAngle_Right;
                PB = PB_Right;
                image_Record = image_Record_Right;
            }

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
                    ShowFrame(sCurrentPos, gg1.iCurrentPos);
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

                    if (sCurrentPos == "Left")
                    {
                        iDelay_Left = iDelay;
                    }
                    else
                    {
                        iDelay_Right = iDelay;
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

                            DrawCurrentPos(sCurrentPos, gg1.iCurrentPos);
                            if (sCurrentPos == "Left")
                            {
                                iDelay_Left = iDelay;
                            }
                            else
                            {
                                iDelay_Right = iDelay;
                            }
                        }
                        else
                        {
                            MyTools.ShowMsg("读取帧数据出错！", sErrString);
                        }
                    }
                }
            }
        }
        private void button_FrameAdd_Click(object sender, RoutedEventArgs e)
        {
            double dPictWidthEachPoint;     // Bar上 单位长度的点数
            Button ThisButton = (Button)sender;
            Thickness thick;
            mod_GG gg1;
            int iDelay = 100;
            double dCutAngle = 0;
            mod_ShowPict PB;
            Image image_Record;

            if (sCurrentPos == "Left")
            {
                gg1 = gg1_Left;
                iDelay = iDelay_Left;
                dCutAngle = dCutAngle_Left;
                PB = PB_Left;
                image_Record = image_Record_Left;
            }
            else
            {
                gg1 = gg1_Right;
                iDelay = iDelay_Right;
                dCutAngle = dCutAngle_Right;
                PB = PB_Right;
                image_Record = image_Record_Right;
            }


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
                    ShowFrame(sCurrentPos, gg1.iCurrentPos);
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
                    if (sCurrentPos == "Left")
                    {
                        iDelay_Left = iDelay;
                    }
                    else
                    {
                        iDelay_Right = iDelay;
                    }
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

                        DrawCurrentPos(sCurrentPos,                                                         gg1.iCurrentPos);
                        if (sCurrentPos == "Left")
                        {
                            iDelay_Left = iDelay;
                        }
                        else
                        {
                            iDelay_Right = iDelay;
                        }
                    }
                    else
                    {
                        MyTools.ShowMsg("读取帧数据出错！", sErrString);
                    }
                }
            }
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
            mod_GG gg1;
            int iDelay = 100;
            double dCutAngle = 0;
            mod_ShowPict PB;
            Image image_Record;
            double dBrightLeft_New;
            double dPictWidthEachPoint = 0;     // Bar上 单位长度的点数

            if (IsSpeedSlideMouseDown)         // 只有按键被，本功能才启用
            {
                if (sCurrentPos == "Left")
                {
                    gg1 = gg1_Left;
                    iDelay = iDelay_Left;
                    dCutAngle = dCutAngle_Left;
                    PB = PB_Left;
                    image_Record = image_Record_Left;
                }
                else
                {
                    gg1 = gg1_Right;
                    iDelay = iDelay_Right;
                    dCutAngle = dCutAngle_Right;
                    PB = PB_Right;
                    image_Record = image_Record_Right;
                }

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
                    dPictWidthEachPoint = gg1.ggData.numberof_frames / (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1);     // Bar上 单位长度的点数

                    int iP = (int)(dBrightLeft_New * dPictWidthEachPoint);
                    if (iP < 0) iP = 0;
                    if (iP >= (int)gg1.ggData.numberof_frames) iP = (int)gg1.ggData.numberof_frames - 1;
                    gg1.iCurrentPos = iP;
                    UserControl_Speed.label_FrameValue.Content = "当前位置：" + gg1.iCurrentPos.ToString() + " 帧";
                    ShowFrame(sCurrentPos, gg1.iCurrentPos);
                }
                else if (ThisImageSlide.Name == "image_DelaySlide")
                {
                    dPictWidthEachPoint = 5000 / (UserControl_Speed.line_FrameBar.X2 - UserControl_Speed.line_FrameBar.X1);     // Bar上 单位长度的点数
                    iDelay = (int)(dBrightLeft_New * dPictWidthEachPoint);
                    UserControl_Speed.label_DelayValue.Content = "播放延时：" + iDelay.ToString() + " 毫秒";

                    if (sCurrentPos == "Left")
                    {
                        iDelay_Left = iDelay;
                    }
                    else
                    {
                        iDelay_Right = iDelay;
                    }
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

                        DrawCurrentPos(sCurrentPos, gg1.iCurrentPos);
                    }
                    else
                    {
                        MyTools.ShowMsg("读取帧数据出错！", sErrString);
                    }
                    if (sCurrentPos == "Left")
                    {
                        dCutAngle_Left = dCutAngle;
                    }
                    else
                    {
                        dCutAngle_Right = dCutAngle;
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
            IsSpeedSlideMouseDown = false;
            FrameworkElement ele = sender as FrameworkElement;
            ele.ReleaseMouseCapture();
        }


        /// <summary>
        /// 播放控制  ===================================
        /// </summary>
        public static bool IsPlayStop_Left = true;
        public static bool IsPlayStop_Right = true;
        private void button_Play_Click(object sender, RoutedEventArgs e)
        {
            string sPos = "";
            string sErr = "";
            Button button_Play = (Button)sender;

            Image image_SelectImage = null;
            BitmapImage PatientPict_Current;

            mod_ShowPict PB;
            mod_GG gg1;
            mod_ReadGen2.Gen2_Struct Gen2Current = new mod_ReadGen2.Gen2_Struct();

            if (button_Play.Name == "button_LeftPlay")
            {
                sPos = "Left";
                IsPlayStop_Left = false;
                //Gen2Current = Gen2Current_Left;
                gg1 = gg1_Left;
                PB = PB_Left;
                PatientPict_Current = PatientPict_Current_Left;
                image_SelectImage = image_LeftPict;
            }
            else      // button_RightPlay
            {
                sPos = "Right";
                IsPlayStop_Right = false;
                //Gen2Current = Gen2Current_Right;
                gg1 = gg1_Right;
                PB = PB_Right;
                PatientPict_Current = PatientPict_Current_Right;
                image_SelectImage = image_RightPict;
            }
            SetPlayKey(sPos, false);

            if (gg1.iCurrentPos == gg1.ggData.numberof_frames - 1)
            {
                gg1.iCurrentPos = 0;
            }
            for (int i = gg1.iCurrentPos; i < gg1.ggData.numberof_frames; i++)
            {
                if ( (sPos == "Left" && IsPlayStop_Left) || (sPos == "Right" && IsPlayStop_Right) )   // if pause-key is pressed
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
                image_SelectImage.Source = PatientPict_Current;
                DrawCurrentPos(sPos, i);
                MyTools.DoEvents();
                if (sPos == "Left")
                {
                    System.Threading.Thread.Sleep(iDelay_Left);
                }
                else      // button_RightPlay
                {
                    System.Threading.Thread.Sleep(iDelay_Right);
                }
            }

            if (sPos == "Left")
            {
                IsPlayStop_Left = true;
            }
            else      // button_RightPlay
            {
                IsPlayStop_Right = true;
            }
            SetPlayKey(sPos, true);
        }
        private void button_Pause_Click(object sender, RoutedEventArgs e)
        {
            Button button_Play = (Button)sender;
            if (button_Play.Name == "button_LeftPause")
            {
                IsPlayStop_Left = true;
            }
            else      // button_RightPause
            {
                IsPlayStop_Right = true;
            }
        }
        private void button_First_Click(object sender, RoutedEventArgs e)
        {
            Button button_First = (Button)sender;

            string sPos = "";
            mod_GG gg1;
            if (button_First.Name == "button_LeftFirst")
            {
                sPos = "Left";
                gg1 = gg1_Left;
            }
            else      // button_RightPlay
            {
                sPos = "Right";
                gg1 = gg1_Right;
            }
            gg1.iCurrentPos = 0;
            ShowFrame(sPos, gg1.iCurrentPos);
        }
        private void button_Back_Click(object sender, RoutedEventArgs e)
        {
            Button button_Back = (Button)sender;

            string sPos = "";
            mod_GG gg1;
            if (button_Back.Name == "button_LeftBack")
            {
                sPos = "Left";
                gg1 = gg1_Left;
            }
            else      // button_RightPlay
            {
                sPos = "Right";
                gg1 = gg1_Right;
            }
            if (gg1.iCurrentPos > 0)
            {
                gg1.iCurrentPos--;
                ShowFrame(sPos, gg1.iCurrentPos);
            }
        }
        private void button_Right_Click(object sender, RoutedEventArgs e)
        {
            Button button_Right = (Button)sender;

            string sPos = "";
            mod_GG gg1;
            if (button_Right.Name == "button_LeftRight")
            {
                sPos = "Left";
                gg1 = gg1_Left;
            }
            else      // button_RightPlay
            {
                sPos = "Right";
                gg1 = gg1_Right;
            }
            if (gg1.iCurrentPos < gg1.ggData.numberof_frames - 1)
            {
                gg1.iCurrentPos++;
                ShowFrame(sPos,gg1.iCurrentPos);
            }
        }
        private void button_End_Click(object sender, RoutedEventArgs e)
        {
            Button button_End = (Button)sender;

            string sPos = "";
            mod_GG gg1;
            if (button_End.Name == "button_LeftEnd")
            {
                sPos = "Left";
                gg1 = gg1_Left;
            }
            else      // button_RightPlay
            {
                sPos = "Right";
                gg1 = gg1_Right;
            }
            gg1.iCurrentPos = (int)(gg1.ggData.numberof_frames - 1);
            ShowFrame(sPos, gg1.iCurrentPos);
        }
        private void ShowFrame(string sPos, int iFrameID)
        {
            string sErr = "";
            Image image_SelectImage = null;
            BitmapImage PatientPict_Current;

            mod_ShowPict PB;
            mod_GG gg1;
            mod_ReadGen2.Gen2_Struct Gen2Current = new mod_ReadGen2.Gen2_Struct();

            if (sPos == "Left")
            {
                gg1 = gg1_Left;
                PB = PB_Left;
                PatientPict_Current = PatientPict_Current_Left;
                image_SelectImage = image_LeftPict;
            }
            else      // button_RightPlay
            {
                gg1 = gg1_Right;
                PB = PB_Right;
                PatientPict_Current = PatientPict_Current_Right;
                image_SelectImage = image_RightPict;
            }
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
            image_SelectImage.Source = PatientPict_Current;
            DrawCurrentPos(sPos, iFrameID);
        }
        private void SetPlayKey(string sPos, bool Mark)
        {
            Button button_Right;
            Button button_End;
            Button button_Back;
            Button button_First;
            Button button_Play;
            Button button_Pause;

            if (sPos == "Left")
            {
                button_Right = button_LeftRight;
                button_End = button_LeftEnd;
                button_Back = button_LeftBack;
                button_First = button_LeftFirst;
                button_Play = button_LeftPlay;
                button_Pause = button_LeftPause;
            }
            else
            {
                button_Right = button_RightRight;
                button_End = button_RightEnd;
                button_Back = button_RightBack;
                button_First = button_RightFirst;
                button_Play = button_RightPlay;
                button_Pause = button_RightPause;
            }

            //StackPanel_RightIcon.IsEnabled = Mark;
            button_Right.IsEnabled = Mark;
            button_End.IsEnabled = Mark;
            button_Back.IsEnabled = Mark;
            button_First.IsEnabled = Mark;

            if (Mark)
            {
                //StackPanel_RightIcon.IsEnabled = true;
                button_Play.Visibility = Visibility.Visible;
                button_Play.IsEnabled = true;
                button_Pause.Visibility = Visibility.Hidden;
                button_Pause.IsEnabled = false;
            }
            else
            {
                //StackPanel_RightIcon.IsEnabled = false;
                button_Play.Visibility = Visibility.Hidden;
                button_Play.IsEnabled = false;
                button_Pause.Visibility = Visibility.Visible;
                button_Pause.IsEnabled = true;
            }
        }


        /// <summary>
        /// Pos位置响应  ===================================
        /// </summary>
        public bool IsFramePosMouseDown_Left = false;
        public bool IsFramePosMouseDown_Right = false;
        public Point FramePosP0_Left, FramePosP1_Left, FramePosP0_Right, FramePosP1_Right;
        public int iFramePosLength = 100;
        private void DrawCurrentPos(string sPosition, int iPos)      // 画出当前帧的位置
        {
            Image image_FramePos;
            Line line_FramePos;
            Label FrameInfo;
            Button Button_Mark;
            Button Button_UnMark;
            Canvas canvas_RecordBottomRule;
            Canvas canvas_RecordLeftRule;
            mod_GG gg1;

            if (sPosition == "Left")
            {
                image_FramePos = image_FramePos_Left;
                line_FramePos = line_FramePos_Left;
                FrameInfo = LeftFrameInfo;
                Button_Mark = Button_LeftMark;
                Button_UnMark = Button_LeftUnMark;
                canvas_RecordBottomRule = canvas_RecordBottomRule_Left;
                canvas_RecordLeftRule = canvas_RecordLeftRule_Left;
                gg1 = gg1_Left;
            }
            else
            {
                image_FramePos = image_FramePos_Right;
                line_FramePos = line_FramePos_Right;
                FrameInfo = RightFrameInfo;
                Button_Mark = Button_RightMark;
                Button_UnMark = Button_RightUnMark;
                canvas_RecordBottomRule = canvas_RecordBottomRule_Right;
                canvas_RecordLeftRule = canvas_RecordLeftRule_Right;
                gg1 = gg1_Right;
            }

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
            Image image_FramePos = (Image)sender;
            string sPos = "";
            if (image_FramePos.Name == "image_FramePos_Left")
            {
                sPos = "第 " + gg1_Left.iCurrentPos.ToString() + " 帧/共 " + gg1_Left.ggData.numberof_frames.ToString() + " 帧";
            }
            else
            {
                sPos = "第 " + gg1_Right.iCurrentPos.ToString() + " 帧/共 " + gg1_Right.ggData.numberof_frames.ToString() + " 帧";
            }
            image_FramePos.ToolTip = sPos;
        }
        private void Line_FramePos_MouseEnter(object sender, MouseEventArgs e)
        {
            Line line_FramePos = (Line)sender;

            string sPos = "";
            if (line_FramePos.Name == "Line_FramePos_Left")
            {
                sPos = "第 " + gg1_Left.iCurrentPos.ToString() + " 帧/共 " + gg1_Left.ggData.numberof_frames.ToString() + " 帧";
            }
            else
            {
                sPos = "第 " + gg1_Right.iCurrentPos.ToString() + " 帧/共 " + gg1_Right.ggData.numberof_frames.ToString() + " 帧";
            }
            line_FramePos.ToolTip = sPos;
        }
        //private void image_FramePos_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    FrameworkElement fEle = sender as FrameworkElement;
        //    fEle.CaptureMouse();
        //    fEle.Cursor = Cursors.Hand;
        //    if (fEle.Name == "image_FramePos_Left")
        //    {
        //        IsFramePosMouseDown_Left = true;
        //        FramePosP0_Left = e.GetPosition(null);
        //    }
        //    else
        //    {
        //        IsFramePosMouseDown_Right = true;
        //        FramePosP0_Right = e.GetPosition(null);
        //    }
        //}
        //private void image_FramePos_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    image_FramePos_MouseMove(sender, e);
        //    FrameworkElement ele = sender as FrameworkElement;
        //    ele.ReleaseMouseCapture();

        //    if (ele.Name == "image_Record_Left" && sCurrentPos == "Left")
        //    {
        //        IsFramePosMouseDown_Left = false;
        //    }
        //    else if (ele.Name == "image_Record_Right" && sCurrentPos == "Right")
        //    {
        //        IsFramePosMouseDown_Right = false;
        //    }
        //}
        //private void image_FramePos_MouseMove(object sender, MouseEventArgs e)
        //{
        //    Grid Grid_FramePos;
        //    Canvas canvas_RecordBottomRule;
        //    Canvas canvas_RecordLeftRule;
        //    mod_GG gg1;
        //    string sPosition;
        //    bool IsFramePosMouseDown;

        //    Image image_FramePos = sender as Image;
        //    if (image_FramePos.Name == "image_Record_Left" && sCurrentPos == "Left")
        //    {
        //        sPosition = "Left";
        //        Grid_FramePos = Grid_Record_Left;
        //        canvas_RecordBottomRule = canvas_RecordBottomRule_Left;
        //        canvas_RecordLeftRule = canvas_RecordLeftRule_Left;
        //        gg1 = gg1_Left;
        //        IsFramePosMouseDown = IsFramePosMouseDown_Left;
        //    }
        //    else if (image_FramePos.Name == "image_Record_Right" && sCurrentPos == "Right")
        //    {
        //        sPosition = "Right";
        //        Grid_FramePos = Grid_Record_Right;
        //        canvas_RecordBottomRule = canvas_RecordBottomRule_Right;
        //        canvas_RecordLeftRule = canvas_RecordLeftRule_Right;
        //        gg1 = gg1_Right;
        //        IsFramePosMouseDown = IsFramePosMouseDown_Right;
        //    }
        //    else
        //        return;


        //    // 当前鼠标位置
        //    Point Pos1 = e.GetPosition(null);

        //    if (IsFramePosMouseDown)           // 只有按键被，本功能才启用
        //    {
        //        // 控件位置
        //        Window window = Window.GetWindow(Grid_FramePos);
        //        Point point = Grid_FramePos.TransformToAncestor(window).Transform(new Point(0, 0));
        //        double X1 = point.X;
        //        double X2 = X1 + Grid_FramePos.ActualWidth;
        //        X1 += 13;                 // 标尺起点

        //        if (Pos1.X < X1)
        //            Pos1.X = X1;
        //        else if (Pos1.X > X2)
        //            Pos1.X = X2;

        //        int iStepX = (int)((canvas_RecordBottomRule.ActualWidth - canvas_RecordLeftRule.ActualWidth) / gg1.ggData.numberof_frames);   // 每帧的宽度
        //        int iFrame = (int)((Pos1.X - X1) / iStepX);
        //        if (iFrame < 0)
        //        {
        //            iFrame = 0;
        //        }
        //        else if (iFrame >= gg1.ggData.numberof_frames)
        //        {
        //            iFrame = (int)(gg1.ggData.numberof_frames - 1);
        //        }

        //        if (gg1.iCurrentPos != iFrame)
        //        {
        //            gg1.iCurrentPos = iFrame;
        //            ShowFrame(sPosition, gg1.iCurrentPos);
        //        }
        //    }
        //}
        private void Grid_FramePos_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image ThisImage;
            Grid ThisGrid;

            Grid Grid_FramePos;
            Canvas canvas_RecordBottomRule;
            Canvas canvas_RecordLeftRule;
            mod_GG gg1;
            string sPosition;
            string ctlName = "";
            bool IsFramePosMouseDown;

            Type ty = sender.GetType();
            if (ty.Name == "Image")
            {
                ThisImage = sender as Image;
                ctlName = ThisImage.Name;
            }
            else
            {
                ThisGrid = sender as Grid;
                ctlName = ThisGrid.Name;
            }

            if ( (ctlName == "Grid_FramePos_Left" || ctlName == "image_Record_Left") && sCurrentPos == "Left")
            {
                sPosition = "Left";
                Grid_FramePos = Grid_FramePos_Left;
                canvas_RecordBottomRule = canvas_RecordBottomRule_Left;
                canvas_RecordLeftRule = canvas_RecordLeftRule_Left;
                gg1 = gg1_Left;
                IsFramePosMouseDown = IsFramePosMouseDown_Left;
            }
            else if ((ctlName == "Grid_FramePos_Right" || ctlName == "image_Record_Right") && sCurrentPos == "Right")
            {
                sPosition = "Right";
                Grid_FramePos = Grid_FramePos_Right;
                canvas_RecordBottomRule = canvas_RecordBottomRule_Right;
                canvas_RecordLeftRule = canvas_RecordLeftRule_Right;
                gg1 = gg1_Right;
                IsFramePosMouseDown = IsFramePosMouseDown_Right;
            }
            else
                return;


            //// 当前鼠标位置
            //Point Pos1 = e.GetPosition(null);
            Point Pos1 = e.GetPosition(Grid_FramePos);
            if (Pos1.X < 0)
                Pos1.X = 0;
            else if (Pos1.X > Grid_FramePos.ActualWidth)
                Pos1.X = Grid_FramePos.ActualWidth;

            //// 控件位置
            //Window window = Window.GetWindow(Grid_FramePos);
            //Point point = Grid_FramePos.TransformToAncestor(window).Transform(new Point(0, 0));
            //double X1 = point.X;
            //double X2 = X1 + Grid_FramePos.ActualWidth;
            //X1 += canvas_RecordLeftRule.ActualWidth;                 // 标尺起点

            //if (Pos1.X < X1)
            //    Pos1.X = X1;
            //else if (Pos1.X > X2)
            //    Pos1.X = X2;

            int iStepX = (int)((canvas_RecordBottomRule.ActualWidth - canvas_RecordLeftRule.ActualWidth) / gg1.ggData.numberof_frames);   // 每帧的宽度
            int iFrame = (int)((Pos1.X - canvas_RecordLeftRule.ActualWidth) / iStepX);
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
                ShowFrame(sPosition, gg1.iCurrentPos);
            }
        }
        private void image_Record_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid_FramePos_MouseDown(sender, e);
        }

        /// <summary>
        /// Mark位置响应  ===================================
        /// </summary>
        private void DrawMarkPict(string sPos, int iFrameID, string Mode)
        {
            // iFrameID = -1              : All Mark
            // iFrameID = 0 -- MaxFrameID : Single Frame
            // Mode = Clear or Draw

            mod_GG gg1;

            if (sPos == "Left")
            {
                gg1 = gg1_Left;
            }
            else
            {
                gg1 = gg1_Right;
            }

            if (iFrameID >= 0 && iFrameID < gg1.ggData.numberof_frames)   // 指定某一帧
            {
                if (Mode == "Draw")
                {
                    DrawOneMark(sPos, iFrameID, "Draw");
                }
                else
                {
                    DrawOneMark(sPos, iFrameID, "Clear");
                }
            }
            else  // 不是指定某一帧
            {
                // 先全部清掉，iFrameID参数不符合要求的也全部清掉
                ClearAllMark(sPos);

                // 如果是对全体进行Draw操作
                if (iFrameID == -1 && Mode == "Draw")       // 全部画出来
                {
                    for (int i = 0; i < gg1.ggData.numberof_frames; i++)
                    {

                        if (gg1.ggMark[i, 0] == "True")
                        {
                            DrawOneMark(sPos, i, "Draw");
                        }
                    }
                }
            }
        }
        private void ClearAllMark(string sPos)
        {
            Grid Grid_FramePos;
            if (sPos == "Left")
            {
                Grid_FramePos = Grid_FramePos_Left;
            }
            else
            {
                Grid_FramePos = Grid_FramePos_Right;
            }

            if (Grid_FramePos.Children.Count > 0)
            {
                int ImageNum = Grid_FramePos.Children.Count;
                for (int i = ImageNum; i > 0; i--)
                {
                    UIElement ctl = Grid_FramePos.Children[i - 1];
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
        private void DrawOneMark(string sPos, int iFrameID, string Mode)
        {
            mod_GG gg1;
            RecordMarkImage[] MarkPict;
            Image image_FramePos;
            Grid Grid_FramePos;
            Canvas canvas_RecordBottomRule;
            Canvas canvas_RecordLeftRule;

            if (sPos == "Left")
            {
                gg1 = gg1_Left;
                MarkPict = MarkPict_Left;
                image_FramePos = image_FramePos_Left;
                Grid_FramePos = Grid_FramePos_Left;
                canvas_RecordBottomRule = canvas_RecordBottomRule_Left;
                canvas_RecordLeftRule = canvas_RecordLeftRule_Left;
            }
            else
            {
                gg1 = gg1_Right;
                MarkPict = MarkPict_Right;
                image_FramePos = image_FramePos_Right;
                Grid_FramePos = Grid_FramePos_Right;
                canvas_RecordBottomRule = canvas_RecordBottomRule_Right;
                canvas_RecordLeftRule = canvas_RecordLeftRule_Right;
            }

            if (Mode == "Draw")
            {
                if (gg1.ggMark[iFrameID, 0] == "True" )
                {
                    if (MarkPict[iFrameID] == null)
                    {
                        MarkPict[iFrameID] = new RecordMarkImage();
                        MarkPict[iFrameID].Stretch = Stretch.Uniform;
                        MarkPict[iFrameID].HorizontalAlignment = HorizontalAlignment.Left;
                        MarkPict[iFrameID].VerticalAlignment = VerticalAlignment.Top;
                        MarkPict[iFrameID].Height = image_FramePos.ActualHeight;
                        MarkPict[iFrameID].Width = image_FramePos.ActualWidth;
                        MarkPict[iFrameID].Name = "MarkPict" + iFrameID.ToString() + "_" + sPos;
                        MarkPict[iFrameID].ToolTip = "第 " + iFrameID.ToString() + " 帧标记：" + System.Environment.NewLine + gg1.ggMark[iFrameID, 3];

                        Grid_FramePos.Children.Add(MarkPict[iFrameID]);
                        Grid_FramePos.RegisterName("MarkPict" + iFrameID.ToString() + "_" + sPos, MarkPict[iFrameID]);  //注册名字，以便以后使用
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
                    Image im = Grid_FramePos.FindName("MarkPict" + iFrameID.ToString() + "_" + sPos) as Image;   //找到添加的按钮
                    if (im != null)//判断是否找到，以免在未添加前就误点了   
                    {
                        Grid_FramePos.Children.Remove(im);//移除对应按钮控件   
                        Grid_FramePos.UnregisterName("MarkPict" + iFrameID.ToString() + "_" + sPos);//还需要把对用的名字注销掉，否则再次点击Button_Add会报错   
                    };
                    MarkPict[iFrameID] = null;
                }
            }
            if (sPos == "Left")
            {
                MarkPict_Left = MarkPict;
            }
            else
            {
                MarkPict_Right = MarkPict;
            }
        }
        private void Button_Mark_Click(object sender, RoutedEventArgs e)
        {
            mod_GG gg1;
            MyData.CheckRecord_Struct Record_Sele;
            string sPos;
            Button Button_Mark, Button_UnMark;

            Button btn = sender as Button;
            if (btn.Name == "Button_LeftMark" || btn.Name == "Button_LeftUnMark")
            {
                gg1 = gg1_Left;
                Record_Sele = Record_Left;
                sPos = "Left";
                Button_Mark = Button_LeftMark;
                Button_UnMark = Button_LeftUnMark;
            }
            else
            {
                gg1 = gg1_Right;
                Record_Sele = Record_Right;
                sPos = "Right";
                Button_Mark = Button_RightMark;
                Button_UnMark = Button_RightUnMark;
            }

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
            Frm_EditMark.sCheckTime = DateTime.Parse(Record_Sele.CheckTime).ToString("yyyy-MM-dd HH:mm:ss");

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
                            Record_Sele.FileID + "," +
                            Record_Sele.RecordID + "," +
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
                DrawMarkPict(sPos, gg1.iCurrentPos, "Draw");
                Button_Mark.Visibility = Visibility.Visible;
                Button_UnMark.Visibility = Visibility.Hidden;
            }
            else                                            // 没有标记
            {
                DrawMarkPict(sPos, gg1.iCurrentPos, "Clear");
                Button_Mark.Visibility = Visibility.Hidden;
                Button_UnMark.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// CutLine响应  ===================================
        /// </summary>
        bool IsCutLineDown_Left = false;
        bool IsCutLineDown_Right = false;
        Point CutLineP0, CutLineP1;
        private void DrawCutLine(string sPosition, int Mode)     // 切割线 -1=隐藏
        {
            if (Mode == -1)
            {
                if (sPosition == "Left")
                {
                    Line_LeftCutLine_0.Visibility = Visibility.Hidden;
                    Line_LeftCutLine_1.Visibility = Visibility.Hidden;
                }
                else
                {
                    Line_RightCutLine_0.Visibility = Visibility.Hidden;
                    Line_RightCutLine_1.Visibility = Visibility.Hidden;
                }
                return;
            }

            double R = 0;
            double P0x = 0;
            double P0y = 0;
            double P1x = 0;
            double P1y = 0;
            double CutAngle;
            Line Line_CutLine_0, Line_CutLine_1;
            if (sPosition == "Left")
            {
                R = image_LeftPict.ActualWidth / 2;
                Line_CutLine_0 = Line_LeftCutLine_0;
                Line_CutLine_1 = Line_LeftCutLine_1;
                CutAngle = dCutAngle_Left;
            }
            else
            {
                R = image_RightPict.ActualWidth / 2;
                Line_CutLine_0 = Line_RightCutLine_0;
                Line_CutLine_1 = Line_RightCutLine_1;
                CutAngle = dCutAngle_Right;
            }

            Line_CutLine_0.Visibility = Visibility.Hidden;
            Line_CutLine_1.Visibility = Visibility.Hidden;
            if (CutAngle >= MyData.PI_90 && CutAngle < MyData.PI_180)            // 第一象限
            {
                P0x = R * Math.Cos(CutAngle - MyData.PI_90);
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

            Line_CutLine_0.X1 = R;
            Line_CutLine_0.Y1 = R;
            Line_CutLine_0.X2 = R + P0x;
            Line_CutLine_0.Y2 = R + P0y;

            P1x = -P0x;
            P1y = -P0y;
            Line_CutLine_1.X1 = R;
            Line_CutLine_1.Y1 = R;
            Line_CutLine_1.X2 = R + P1x;
            Line_CutLine_1.Y2 = R + P1y;
        }
        private void Line_CutLine_0_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement fEle = sender as FrameworkElement;
            if (fEle.Name == "Line_LeftCutLine_0" && ZoomIsPress_Left && !Pop_Detect_Left.IsOpen)
            {
                fEle.Cursor = Cursors.SizeAll;
            }
            else if (fEle.Name == "Line_LeftCutLine_1" && ZoomIsPress_Left && !Pop_Detect_Left.IsOpen)
            {
                fEle.Cursor = Cursors.SizeAll;
            }
            else if (fEle.Name == "Line_RightCutLine_0" && ZoomIsPress_Right && !Pop_Detect_Right.IsOpen)
            {
                fEle.Cursor = Cursors.SizeAll;
            }
            else if (fEle.Name == "Line_RightCutLine_1" && ZoomIsPress_Right && !Pop_Detect_Right.IsOpen)
            {
                fEle.Cursor = Cursors.SizeAll;
            }
        }
        private void Line_CutLine_0_MouseLeave(object sender, MouseEventArgs e)
        {
            FrameworkElement fEle = sender as FrameworkElement;
            if (fEle.Name == "Line_LeftCutLine_0" && ZoomIsPress_Left && !Pop_Detect_Left.IsOpen)
            {
                fEle.Cursor = Cursors.Hand;
            }
            else if (fEle.Name == "Line_LeftCutLine_1" && ZoomIsPress_Left && !Pop_Detect_Left.IsOpen)
            {
                fEle.Cursor = Cursors.Hand;
            }
            else if (fEle.Name == "Line_RightCutLine_0" && ZoomIsPress_Right && !Pop_Detect_Right.IsOpen)
            {
                fEle.Cursor = Cursors.Hand;
            }
            else if (fEle.Name == "Line_RightCutLine_1" && ZoomIsPress_Right && !Pop_Detect_Right.IsOpen)
            {
                fEle.Cursor = Cursors.Hand;
            }
        }
        private void Line_CutLine_0_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fEle = sender as FrameworkElement;
            if (fEle.Name == "Line_LeftCutLine_0" && ZoomIsPress_Left && !Pop_Detect_Left.IsOpen && sCurrentPos == "Left")
            {
                fEle.CaptureMouse();                                           // 需要捕获鼠标
                fEle.Cursor = Cursors.SizeAll;
                IsCutLineDown_Left = true;
            }
            else if (fEle.Name == "Line_LeftCutLine_1" && ZoomIsPress_Left && !Pop_Detect_Left.IsOpen && sCurrentPos == "Left")
            {
                fEle.CaptureMouse();                                           // 需要捕获鼠标
                fEle.Cursor = Cursors.SizeAll;
                IsCutLineDown_Left = true;
            }
            else if (fEle.Name == "Line_RightCutLine_0" && ZoomIsPress_Right && !Pop_Detect_Right.IsOpen && sCurrentPos == "Right")
            {
                fEle.CaptureMouse();                                           // 需要捕获鼠标
                fEle.Cursor = Cursors.SizeAll;
                IsCutLineDown_Right = true;
            }
            else if (fEle.Name == "Line_RightCutLine_1" && ZoomIsPress_Right && !Pop_Detect_Right.IsOpen && sCurrentPos == "Right")
            {
                fEle.CaptureMouse();                                           // 需要捕获鼠标
                fEle.Cursor = Cursors.SizeAll;
                IsCutLineDown_Right = true;
            }
        }
        private void Line_CutLine_0_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fEle = sender as FrameworkElement;
            if ((fEle.Name == "Line_LeftCutLine_0" && ZoomIsPress_Left && !Pop_Detect_Left.IsOpen && sCurrentPos == "Left" && IsCutLineDown_Left) ||
                 (fEle.Name == "Line_LeftCutLine_1" && ZoomIsPress_Left && !Pop_Detect_Left.IsOpen && sCurrentPos == "Left" && IsCutLineDown_Left))
            {
                Line_CutLine_0_MouseMove(sender, e);

                // 调整RecordPict
                string sErrString = "";
                if (gg1_Left.ReadRecordBeltData(gg1_Left.ggAllRawData, ref gg1_Left.ggRecordBeltData, dCutAngle_Left, ref sErrString))
                {
                    PB_Left.GGRecordToPict(PB_Left.RecordPictWidth);
                    image_Record_Left.Source = PB_Left.RecordPict;

                    DrawCurrentPos("Left",gg1_Left.iCurrentPos);
                }
                else
                {
                    MyTools.ShowMsg("读取帧数据出错！", sErrString);
                }
                IsCutLineDown_Left = false;
            }
            else if ((fEle.Name == "Line_RightCutLine_0" && ZoomIsPress_Right && !Pop_Detect_Right.IsOpen && sCurrentPos == "Right" && IsCutLineDown_Right) ||  
                     (fEle.Name == "Line_RightCutLine_1" && ZoomIsPress_Right && !Pop_Detect_Right.IsOpen && sCurrentPos == "Right" && IsCutLineDown_Right))
            {
                Line_CutLine_0_MouseMove(sender, e);

                // 调整RecordPict
                string sErrString = "";
                if (gg1_Right.ReadRecordBeltData(gg1_Right.ggAllRawData, ref gg1_Right.ggRecordBeltData, dCutAngle_Right, ref sErrString))
                {
                    PB_Right.GGRecordToPict(PB_Right.RecordPictWidth);
                    image_Record_Right.Source = PB_Right.RecordPict;

                    DrawCurrentPos("Right", gg1_Right.iCurrentPos);
                }
                else
                {
                    MyTools.ShowMsg("读取帧数据出错！", sErrString);
                }
                IsCutLineDown_Right = false;
            }
            else
            {
                return;
            }

            fEle.ReleaseMouseCapture();
            fEle.Cursor = Cursors.Hand;
        }
        private void Line_CutLine_0_MouseMove(object sender, MouseEventArgs e)
        {
            Rect targetRect;
            Canvas Canvas_BigPict1, Canvas_BigPict2;
            Point CutLineP1;
            string sPos;

            FrameworkElement fEle = sender as FrameworkElement;
            if ((fEle.Name == "Line_LeftCutLine_0" && ZoomIsPress_Left && !Pop_Detect_Left.IsOpen && sCurrentPos == "Left" && IsCutLineDown_Left) ||
                 (fEle.Name == "Line_LeftCutLine_1" && ZoomIsPress_Left && !Pop_Detect_Left.IsOpen && sCurrentPos == "Left" && IsCutLineDown_Left))
            {
                Canvas_BigPict1 = Canvas_LeftPict1;
                Canvas_BigPict2 = Canvas_LeftPict2;
                sPos = "Left";
            }
            else if ((fEle.Name == "Line_RightCutLine_0" && ZoomIsPress_Right && !Pop_Detect_Right.IsOpen && sCurrentPos == "Right" && IsCutLineDown_Right) ||
                     (fEle.Name == "Line_RightCutLine_1" && ZoomIsPress_Right && !Pop_Detect_Right.IsOpen && sCurrentPos == "Right" && IsCutLineDown_Right))
            {
                Canvas_BigPict1 = Canvas_RightPict1;
                Canvas_BigPict2 = Canvas_RightPict2;
                sPos = "Right";
            }
            else
                return;


            // 当前鼠标位置
            CutLineP1 = e.GetPosition(Canvas_BigPict1);

            //获得目标经过变换之后的Rect
            targetRect =
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

            if (fEle.Name == "Line_LeftCutLine_0")       // 移动的是上半截
            {
                dCutAngle_Left = Angle_New;
            }
            else if (fEle.Name == "Line_LeftCutLine_1")       // 移动的是下半截
            {
                dCutAngle_Left = Angle_New + MyData.PI;
            }
            else if (fEle.Name == "Line_RightCutLine_0")
            {
                dCutAngle_Right = Angle_New;
            }
            else if (fEle.Name == "Line_RightCutLine_1")
            {
                dCutAngle_Right = Angle_New + MyData.PI;
            }

            if (dCutAngle_Left >= MyData.PI_360)
                dCutAngle_Left -= MyData.PI_360;

            if (dCutAngle_Right >= MyData.PI_360)
                dCutAngle_Right -= MyData.PI_360;

            DrawCutLine(sPos, 1);
        }

        private void button_Link_Click(object sender, RoutedEventArgs e)
        {
            if (button_Link.Visibility == Visibility.Visible)
            {
                if (bIsLink)        // 图像关联
                {
                    bIsLink = false;
                }
                else                // 图像没关联
                {
                    bIsLink = true;
                }
            }
        }

    }
}
