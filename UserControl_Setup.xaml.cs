﻿using System;
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

namespace OctTools
{
    /// <summary>
    /// UserControl_Setup.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_Setup : UserControl
    {
        public UserControl_Setup()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            textBox_Bright_ForSetup.Text = MyData.BrightCurrent.ToString();
            textBox_Contract_ForSetup.Text = MyData.ColorCurrent.ToString();
            textBox_Saturation_ForSetup.Text = MyData.SaturationCurrent.ToString();
            combo_Box_Color_ForSetup.SelectedIndex = MyData.ColorCurrent;
            textBox_Angle_ForSetup.Text = MyData.AngleCurrent.ToString();
            textBox_Delay_ForSetup.Text = MyData.DelayCurrent.ToString();
            button_Exit_ForSetup.Focus();
        }

        private void Button_Save_ForSetup_Click(object sender, RoutedEventArgs e)
        {
            int i = -1;
            int iBright = -1, iContract = -1, iSaturation = -1, iColor = -1, iAngle = -1, iDelay = -1;
            try
            {
                for ( i = 0; i < 6; i++)
                {
                    switch (i)
                    {
                        case 0:
                            iBright = int.Parse(textBox_Bright_ForSetup.Text.Trim());
                            if (iBright < -100 || iBright > 100)
                            {
                                throw new Exception("参数值错误");
                            }
                            break;
                        case 1:
                            iContract = int.Parse(textBox_Contract_ForSetup.Text.Trim());
                            if (iContract < -100 || iContract > 100)
                            {
                                throw new Exception("参数值错误");
                            }
                            break;
                        case 2:
                            iSaturation = int.Parse(textBox_Saturation_ForSetup.Text.Trim());
                            if (iSaturation < -100 || iSaturation > 100)
                            {
                                throw new Exception("参数值错误");
                            }
                            break;
                        case 3:
                            iColor = combo_Box_Color_ForSetup.SelectedIndex;
                            break;
                        case 4:
                            iAngle = int.Parse(textBox_Angle_ForSetup.Text.Trim());
                            if (iAngle < 0 || iAngle > 360)
                            {
                                throw new Exception("参数值错误");
                            }
                            break;
                        case 5:
                            iDelay = int.Parse(textBox_Delay_ForSetup.Text.Trim());
                            if (iDelay < 0 || iDelay > 5000)
                            {
                                throw new Exception("参数值错误");
                            }
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                string sField = "图像亮度,图像对比,图像平滑,图像颜色,截面角度,播放间隔,".Substring(i * 5, 4);
                MyTools.ShowMsg("“" + sField + "”参数错误。", ex.Message);
                return;
            }

            MessageBoxResult rt = MessageBox.Show("把当前参数保存为默认配置吗？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (rt == MessageBoxResult.Yes)
            {
                MyData.BrightDefault = iBright;
                MyData.ContractDefault = iContract;
                MyData.SaturationDefault = iSaturation;
                MyData.ColorDefault = iColor;
                MyData.AngleDefault = iAngle;
                MyData.DelayDefault = iDelay;

                MyData.BrightCurrent = iBright;
                MyData.ContractCurrent = iContract;
                MyData.SaturationCurrent = iSaturation;
                MyData.ColorCurrent = iColor;
                MyData.AngleCurrent = iAngle;
                MyData.DelayCurrent = iDelay;

                string sErr = "";
                if (!MyData.MySqlite.SetRefe(ref sErr))
                {
                    MyTools.ShowMsg("把参数写入文件时出错", sErr);
                }
                else
                {
                    MyTools.ShowMsg("默认参数保存成功", sErr);
                }
            }
        }

        // 定义委托
        public delegate void ExitForSetupClick(object sender, RoutedEventArgs e);
        // 定义事件
        public event ExitForSetupClick Exit_ForSetup_Click;
        private void Button_Exit_ForSetup_Click(object sender, RoutedEventArgs e)
        {
            if (Exit_ForSetup_Click != null)
                Exit_ForSetup_Click(sender, e);
        }

        private void Button_Restore_ForSetup_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult rt = MessageBox.Show("使用默认参数替换当前参数吗？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (rt == MessageBoxResult.Yes)
            {
                MyData.BrightCurrent = MyData.BrightDefault;
                MyData.ContractCurrent = MyData.ContractDefault;
                MyData.SaturationCurrent = MyData.SaturationDefault;
                MyData.ColorCurrent = MyData.ColorDefault;
                MyData.AngleCurrent = MyData.AngleDefault;
                MyData.DelayCurrent = MyData.DelayDefault;

                string sErr = "";
                if (!MyData.MySqlite.SetRefe(ref sErr))
                {
                    MyTools.ShowMsg("把参数写入文件时出错", sErr);
                }
                else
                {
                    MyTools.ShowMsg("当前参数保存成功", sErr);
                }
            }
        }

    }
}
