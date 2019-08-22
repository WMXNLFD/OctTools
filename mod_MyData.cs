using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctTools
{
    public static class MyData
    {
        public static string sVer = " OCTIS Tools V1.02";

        // DB
        public static MySqliteClass MySqlite;

        // Data
        public struct PatientInfo_Struct
        {
            public string FileID;
            public string PatientID;
            public string Name;
            public string Birthday;
            public string Sex;
            public string Address;
            public string Tele;
            public string IdentifyID;
            public string Memo;
        }
        public struct CheckRecord_Struct
        {
            public string RecordID;
            public string FileID;
            public string CheckTime;
            public string Doctor;
            public string CheckInfo;
            public string SmallPict;
            public string BigPict;
            public string SelectPict;
        }
        public struct RecordMark_Struct
        {
            public int MarkID;
            public int FileID;
            public int RecordID;
            public int Mark;
        }

        //   RGB colors (for all color attributes): ***/
        public const int VAL_RED = 0xFF0000;                /* 16 standard colors */
        public const int VAL_GREEN = 0x00FF00;
        public const int VAL_BLUE = 0x0000FF;
        public const int VAL_CYAN = 0x00FFFF;
        public const int VAL_MAGENTA = 0xFF00FF;
        public const int VAL_YELLOW = 0xFFFF00;
        public const int VAL_DK_RED = 0x800000;
        public const int VAL_DK_BLUE = 0x000080;
        public const int VAL_DK_GREEN = 0x008000;
        public const int VAL_DK_CYAN = 0x008080;
        public const int VAL_DK_MAGENTA = 0x800080;
        public const int VAL_DK_YELLOW = 0x808000;
        public const int VAL_LT_GRAY = 0xC0C0C0;
        public const int VAL_DK_GRAY = 0x808080;
        public const int VAL_BLACK = 0x000000;
        public const int VAL_WHITE = 0xFFFFFF;

        public const int VAL_PANEL_GRAY = VAL_LT_GRAY;
        public const int VAL_GRAY = 0xA0A0A0;
        public const int VAL_OFFWHITE = 0xE0E0E0;
        public const int VAL_TRANSPARENT = 0x1000000;

        public static int display_flg = 2;    // display_flg=0 for false color display, display_flg=1 for gray scale display   0=伪彩，1=灰度图
        public const int DisplayMode_IronTable = 0;
        public const int DisplayMode_RainTable = 1;
        public const int DisplayMode_Orig = 2;             // MoreColor
        public const int DisplayMode_WhiteGray = 3;
        public const int DisplayMode_Yello = 4;            // 单色
        public const int DisplayMode_BW = 5;               // 反相
        public const int DisplayMode_8Bit = 10;            // 保留

        // Refe 默认值
        public static int BrightDefault = 10;
        public static int ContractDefault = 10;
        public static int SaturationDefault = 10;
        public static int ColorDefault = 4;
        public static int AngleDefault = 0;
        public static int DelayDefault = 100;
        // Refe 当前值
        public static int BrightCurrent = 10;
        public static int ContractCurrent = 10;
        public static int SaturationCurrent = 10;
        public static int ColorCurrent = 4;
        public static int AngleCurrent = 0;
        public static int DelayCurrent = 100;

        public static int IT = 1, AL = 2;               // for smoothing

        // 区域范围与折射率
        public static double dBigPictWidth = 10.0;      // 大图像的宽度是 5 mm，同时也用于 RecordBelt 的高度
        public static double dAirRate = 1.0;    //1.4;            // 空气折射率

        // 二值化阈值
        public static double dBinaryzation = 0.75;
        public static int iBinaryzationLineNum = 100;
        public static int iBinaryzationRingNum = 30;

        // OCTIS数据的径向位移，正数向外移，负数向圆心方向移动
        public static int AScan_Offset = 0;

        // 原始数据
        //public static double[,] ampplot_offset;                 // // 位移后的原始数据[5100][1000];
        //public static double[,] ampplot_s_R90;
        //public static double[,] avg;

        //public static int[,] D = new int[2000, 2000];
        //public static int[,] D90 = new int[2000, 2000];       //for polar table caculation
        //public static int[,] L = new int[5000, 2000];
        //public static int[,] L90 = new int[2000, 2000];      //for polar table caculation 

        // Database Cover Mode
        public static int iDataCoverMode_Cover = 1;
        public static int iDataCoverMode_NotCover = 2;
        public static int iDataCoverMode_New = 3;

        // PI
        public const double PI = 3.1415926535897932384626433832795028841971;
        public const double PI_45 = PI / 4;
        public const double PI_90 = PI / 2;
        public const double PI_180 = PI;                    // 180度的圆周值
        public const double PI_270 = 3 * PI / 2;            // 180度的圆周值
        public const double PI_360 = PI * 2;                // 360 度的圆周值
        public const double PI_360_Div = PI * 2 / 360.0;    // 细分每一度的圆周值


        // Err
        public const int iErr_Succ = 1;
        public const int iErr_UnknowErr = -1;
        public const int iErr_NoInit = -10;
        public const int iErr_NoFile = -11;
        public const int iErr_ValidData = -12;
        public const int iErr_Exception = -20;
    }

}
