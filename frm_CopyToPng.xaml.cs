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
using System.Windows.Forms;

namespace OctTools
{
    /// <summary>
    /// frm_CopyToPng.xaml 的交互逻辑
    /// </summary>
    public partial class frm_CopyToPng : Window
    {
        Image pImage = null;
        public frm_CopyToPng(Image pSaveImage)
        {
            InitializeComponent();
            pImage = pSaveImage;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PathName.Text = "";
            FileName.Text = "";
            ExtName.SelectedIndex = 0;
            button_Search.Focus();
        }

        private void button_Search_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "选择保存图像文件的文件夹";
            folderBrowserDialog.ShowNewFolderButton = true;    //是否显示对话框左下角 新建文件夹 按钮，默认为 true
            
            if (PathName.Text == "")
                folderBrowserDialog.SelectedPath = Environment.CurrentDirectory;
            else
                folderBrowserDialog.SelectedPath = PathName.Text;

            folderBrowserDialog.ShowDialog();
            if (folderBrowserDialog.SelectedPath == string.Empty)
            {
                return;
            }
            PathName.Text = folderBrowserDialog.SelectedPath;
        }

        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            string sFileName = "";
            try
            {
                PathName.Text = PathName.Text.Trim();
                FileName.Text = FileName.Text.Trim();
                if (PathName.Text == "" || FileName.Text == "" )
                {
                    MyTools.ShowMsg("请选择正确的保存路径和文件名。", "");
                    return;
                }

                sFileName = PathName.Text + "\\" + FileName.Text + ExtName.SelectionBoxItem.ToString().Substring(1);
                               
                if (File.Exists(sFileName))
                {                    
                    DialogResult dr = System.Windows.Forms.MessageBox.Show("已经存在同名文件，是否覆盖？", "温馨提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == System.Windows.Forms.DialogResult.No)
                        return;
                }

                BitmapSource BS = (BitmapSource)pImage.Source;
                if (ExtName.SelectedIndex == 0)       // png
                {
                    PngBitmapEncoder PBE = new PngBitmapEncoder();
                    PBE.Frames.Add(BitmapFrame.Create(BS));
                    using (Stream stream = File.Create(sFileName))
                    {
                        PBE.Save(stream);
                    }
                }
                else
                {
                    JpegBitmapEncoder JBE = new JpegBitmapEncoder();
                    JBE.Frames.Add(BitmapFrame.Create(BS));
                    using (Stream stream = File.Create(sFileName))
                    {
                        JBE.Save(stream);
                    }
                }
                MyTools.ShowMsg("保存成功！", "");
            }
            catch (Exception ex)
            {
                MyTools.ShowMsg("保存失败。", ex.Message);
            }
        }
        private void button_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
