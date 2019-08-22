using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Threading;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.IO;

namespace OctTools
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public App()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);

            if (processes.GetLength(0) > 1)
            {
                System.Windows.MessageBox.Show("程序已经启动，不能重复启动！", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (File.Exists(Environment.CurrentDirectory + "//OctData.dat") != true)
            {
                System.Windows.MessageBox.Show("没有安装数据文件OctData.dat，不能运行！", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (File.Exists(Environment.CurrentDirectory + "//System.Data.SQLite.dll") != true)
            {
                System.Windows.MessageBox.Show("没有安装数据引擎文件System.Data.SQLite.dll，不能运行！", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Check Directory
            if (Directory.Exists(Environment.CurrentDirectory + "//Log") != true)
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "//Log");
            }

            if (Directory.Exists(Environment.CurrentDirectory + "//Data") != true)
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "//Data");
            }
        }
    }
}
