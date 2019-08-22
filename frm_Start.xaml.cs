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
    /// frm_Start.xaml 的交互逻辑
    /// </summary>
    public partial class frm_Start : Window
    {
        double WinWidth_Real = 1600;
        double WinHeight_Real = 900;

        public frm_Start()
        {
            InitializeComponent();
        }
        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            MyData.MySqlite.CloseDb();
            this.Close();
            Environment.Exit(0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindows();

            MyData.MySqlite = new MySqliteClass();
            MyData.MySqlite.DbFileName = Environment.CurrentDirectory + "//OctData.dat";
            if (!MyData.MySqlite.OpenDb())
            {
                MessageBox.Show("系统数据错误（" + MyData.MySqlite.ErrMsg + "），请更正！", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }

            string sErr = "";
            if (!MyData.MySqlite.GetRefe(ref sErr))
            {
                MyTools.ShowMsg("读参数表错误！", sErr);
                this.Close();
                return;
            }

            textBox_TopicString.Text = "请输入关键词检索（＊代表全部）";
            textBox_SearchString.Text = "";
            textBox_SearchString.Focus();
        }
        private void SetWindows()
        {
            label_Logo.Content = "软件工具" + MyData.sVer;
            this.Title = MyData.sVer;

            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            WinWidth_Real = System.Windows.SystemParameters.WorkArea.Width;
            WinHeight_Real = System.Windows.SystemParameters.WorkArea.Height;

            this.Top = 0;
            this.Left = 0;
            this.Width = WinWidth_Real;
            this.Height = WinHeight_Real;
        }
        private void Button_Mini_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            SetWindows();
        }
        private void TextBox_SearchString_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox_SearchString.Text.Trim().Length > 0)
            {
                textBox_TopicString.Text = "                             ";
            }
            else
            {
                textBox_TopicString.Text = "请输入关键词检索（＊代表全部）";
            }
        }
        private void TextBox_TopicString_GotFocus(object sender, RoutedEventArgs e)
        {
            textBox_SearchString.Focus();
        }
        private void TextBlock_Search_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock_Search.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Wheat"));
        }
        private void TextBlock_Search_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock_Search.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
        }
        private void TextBlock_Import_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock_Import.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightBlue"));
        }
        private void TextBlock_Import_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock_Import.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
        }

        private void TextBlock_Search_MouseDown(object sender, MouseButtonEventArgs e)
        {            
            if (textBox_SearchString.Text.Trim().Length > 0)
            {                                
                frm_SearchPatient Frm_SearchPatient = new frm_SearchPatient();
                Frm_SearchPatient.IsFirst = true;
                Frm_SearchPatient.sSearchString = textBox_SearchString.Text;
                Frm_SearchPatient.IsShowInput = false;
                Frm_SearchPatient.pfrm_Start = this;
                Frm_SearchPatient.Owner = this;
                this.ShowInTaskbar = false;
                this.IsEnabled = false;
                Frm_SearchPatient.ShowDialog();
            }
            else
            {
                MyTools.ShowMsg("没有输入检索关键词!", "");
            }
        }
        private void Button_Setup_Click(object sender, RoutedEventArgs e)
        {
            frm_Setup Frm_Setup = new frm_Setup();
            Frm_Setup.ShowDialog();
        }

        private void TextBlock_Import_MouseDown(object sender, MouseButtonEventArgs e)
        {
            frm_Import Frm_Import = new frm_Import();
            Frm_Import.pfrm_Start = this;
            Frm_Import.Owner = this;
            this.ShowInTaskbar = false;
            this.IsEnabled = false;
            Frm_Import.ShowDialog();
        }    

        //增加回车搜索
        private void TextBox_SearchString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (textBox_SearchString.Text.Trim().Length > 0)
                {
                    frm_SearchPatient Frm_SearchPatient = new frm_SearchPatient();
                    Frm_SearchPatient.IsFirst = true;
                    Frm_SearchPatient.sSearchString = textBox_SearchString.Text;
                    Frm_SearchPatient.IsShowInput = false;
                    Frm_SearchPatient.pfrm_Start = this;
                    Frm_SearchPatient.Owner = this;
                    this.ShowInTaskbar = false;
                    this.IsEnabled = false;
                    Frm_SearchPatient.ShowDialog();
                }
                else
                {
                    MyTools.ShowMsg("没有输入检索关键词!", "");
                }
            }
        }
    }

}
