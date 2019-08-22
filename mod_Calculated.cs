using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OctTools
{
    class mod_Calculated
    {
        const double PI = 3.1415926535897932384626433832795028841971;

        const double PI_180 = PI;                    // 180度的圆周值
        const double PI_360 = PI * 2;                // 360 度的圆周值
        const double PI_360_Div = PI * 2 / 360.0;    // 细分每一度的圆周值

        //===========================================================================
        //
        //   Polar transfer for 360 degree display
        //
        //============================================================================
        //  计算屏幕显示每一个像素的显示值对应于采样数据阵列中的行与列的位置
        // 700 * 700		 距阵的屏幕显示范围
        // Number_line_360   旋转一周有多少条扫描线
        // ft_Angle				图形旋转多少角度
        // iW					计算显示区域的宽度   700
        // iH					计算显示区域的高度   700
        // Distance				屏幕上每一个点对应扫描线上的那一个点
        // iLine				屏幕上每一个点对应扫描数据中那一条线上数据
        // 原装定义 void PolarTables_Ex(int Number_line_360, float ft_Angle, int iW, int iH, int Distance[2000][2000], int iLine[5000][2000])
        public static void PolarTables_Ex(int Number_line_360, float ft_Angle, int iW, int iH, int[,] Distance, int[,] iLine)
        {
            int x, y;
            int iWCenter, iHCenter;         //显示区域的中心位置
            double a = 0, xx = 0, yy = 0, dd = 0;
            double dba;

            iWCenter = iW >> 1;             //除以2 = 宽度的中心位置
            iHCenter = iH >> 1;             //除以2 = 高度的中心位置
            dd = ft_Angle * PI_360_Div;             //显示的偏转角度
            dba = PI_360 / Number_line_360;         //每一扫描线的圆周率

            Distance.Initialize();
            iLine.Initialize();

            //  计算屏幕显示的每一个点对应的位置 700 表示屏幕显示使用的是700宽700高的显示空间
            //for (x=0; x<700; x++)
            for (x = 1; x <= iW; x++)
            {
                xx = (double)x;
                xx -= iWCenter;
                //for (y=0; y < 700; y++)
                for (y = 1; y <= iH; y++)
                {
                    yy = (double)y;
                    yy -= iHCenter;

                    //D[y][x]=sqrt((x - iWCenter) * (x - iWCenter) + (y - iHCenter) * (y - iHCenter));			//计算每个像素的与中心的距离，这个距离可以直接代入采样的数据的列值中
                    Distance[y,x] = (int)Math.Sqrt(xx * xx + yy * yy);           //计算每个像素的与中心的距离，这个距离可以直接代入采样的数据的列值中

                    //计算每一个像素的在平面坐标系的弧度
                    //计算反余切
                    //if( (xx-350.0)>=0.0)
                    if (xx >= 0.0)                        // 在中心点的右侧
                    {
                        a = Math.Atan2(xx, yy) - dd;
                    }
                    if (xx < 0.0)                         //在中心点的左侧
                    {
                        a = PI_360 + Math.Atan2(xx, yy) - dd;
                    }
                    while (true)
                    {
                        if (a < 0)            // 角度小于开始的角度
                        {
                            a = PI_360 + a;
                        }
                        else
                            break;
                    }

                    while (true)
                    {
                        if (a > PI_360)
                        {
                            a = a - PI_360;
                        }
                        else
                            break;
                    }
                    //  通过弧度计算数据阵列中的行
                    //  Number_line_360 每一个采样阵列中的行总数
                    iLine[y,x] = (int)(a / dba);//(a/6.283)*Number_line_360;      // 属于那条 射线

                }
            }
        }

        /*-----------------------------------------------------------------
        void PolarTables_Ex(int Number_line_360, float ft_Angle, int iW, int iH, int Distance[2000][2000], int iLine[5000][2000])
        {
            int x, y;
            int iWCenter, iHCenter;			//显示区域的中心位置
            double a, xx, yy, dd;

            iWCenter = iW >> 1;				//宽度的中心位置
            iHCenter = iH >> 1;				//高度的中心位置
            dd = ft_Angle/56.0; 			//显示的偏转角度

            //  计算屏幕显示的每一个点对应的位置 700 表示屏幕显示使用的是700宽700高的显示空间
            //for (x=0; x<700; x++)
            for (x=0; x < iW; x++)
            {
                xx=(double)x;
                xx -= iWCenter;
                //for (y=0; y < 700; y++)
                for (y=0; y < iH; y++)
                {
                    yy=(double)y;
                    yy -= iHCenter;

                    //D[y][x]=sqrt((x - iWCenter) * (x - iWCenter) + (y - iHCenter) * (y - iHCenter));			//计算每个像素的与中心的距离，这个距离可以直接代入采样的数据的列值中
                    Distance[y][x]=sqrt(xx * xx + yy * yy);			//计算每个像素的与中心的距离，这个距离可以直接代入采样的数据的列值中

                     //计算每一个像素的在平面坐标系的弧度
                    //计算反余切
                    //if( (xx-350.0)>=0.0)
                    if(xx>=0.0)
                    {
                        a=atan2(xx, yy)-dd;
                    }
                    if(xx < 0.0)
                    {
                        a=6.283+atan2(xx, yy)-dd;
                    }
                    while(1)
                    {
                        if ( a<0 )
                        {
                            a=6.283+a;
                        }
                        else
                            break;
                    }

                    while(1)
                    {
                        if ( a>6.283 )
                        {
                            a=a-6.283;
                        }
                        else
                            break;
                    }
                    //  通过弧度计算数据阵列中的行
                    //  Number_line_360 每一个采样阵列中的行总数
                    iLine[y][x]=(a/6.283)*Number_line_360;
                } 
            }
        }
        ---------------------------------------------------------------------------*/

        //---------------------------------------------------------------------------------
        //  计算90度角的显示区域
        //  计算屏幕显示每一个像素的显示值对应于采样数据阵列中的行与列的位置
        // 900 * 900		距阵的屏幕显示范围
        // Number_line_360   旋转一周有多少条扫描线
        // ft_Angle				图形旋转多少角度
        // iW					计算显示区域的宽度   900
        // iH					计算显示区域的高度   900
        // Distance				屏幕上每一个点对应扫描线上的那一个点
        // iLine				屏幕上每一个点对应扫描数据中那一条线上数据         // 注：张工介绍：此程序没用
        //---------------------------------------------------------------------------------
        //void PolarTables90_Ex(int Number_line_360, float ft_Angle, int iW, int iH, int Distance[2000][2000], int iLine[2000][2000])
        public static void PolarTables90_Ex(int Number_line_360, float ft_Angle, int iW, int iH, int[,] Distance, int[,] iLine)
        {
            int x, y;
            double a, xx, yy, dd;

            dd = ft_Angle / 56.0;

            for (x = 0; x < iW; x++)
            {
                xx = (double)x;

                for (y = 0; y < iH; y++)
                {
                    yy = (double)y;

                    Distance[y,x] = (int)(Math.Sqrt(xx * xx + yy * yy) + 0.1);

                    a = Math.Atan2(xx, yy);

                    while (true)
                    {
                        if (a < 0)
                        {
                            a = 6.283 + a;
                        }
                        else
                            break;
                    }

                    while (true)
                    {
                        if (a > 6.283)
                        {
                            a = a - 6.283;
                        }
                        else
                            break;
                    }

                    iLine[y,x] = (int)( (a / 6.283) * Number_line_360 + 0.1);
                }
            }
        }

        //==============================================================================
        //
        // 	Log look up table
        //  int ASDPTS=1024;   //A_scan data points;
        //  double CT[3000];   // windown look up table
        //  LogTable
        //==============================================================================
        // void LogTables_Ex(double LogTable[200010], double CT[3000], int ASDPTS)
        void LogTables_Ex(double[] LogTable, double[] CT, int ASDPTS)
        {
            long i;

            LogTable[0] = Math.Log(1.0);

            for (i = 1; i < 200010; i++)
            {
                LogTable[i] = Math.Log(i * 1.0);
            }
            for (i = 0; i < ASDPTS; i++)
            {
                CT[i] = Math.Cos(2 * 3.14 * i / ASDPTS);
            }
        }

        // 从CVI移植是，需要的union
        //typedef struct
        //{
        //    union
        //    {
        //        char               valChar;
        //        int                valInt;
        //        __int64            valInt64;
        //        short              valShort;
        //        float              valFloat;
        //        double             valDouble;
        //        unsigned char      valUChar;
        //        unsigned long      valULong;
        //        unsigned __int64   valUInt64;
        //        unsigned short     valUShort;
        //    } dataValue;
        //    int color;
        //} ColorMapEntry;

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct dataValue_Struct
        {
            [FieldOffset(0)]
            public char valChar;

            [FieldOffset(0)]
            public int valInt;

            [FieldOffset(0)]
            public Int64 valInt64;

            [FieldOffset(0)]
            public short valShort;

            [FieldOffset(0)]
            public float valFloat;

            [FieldOffset(0)]
            public double valDouble;

            [FieldOffset(0)]
            public byte valUChar;

            [FieldOffset(0)]
            public ulong valULong;

            [FieldOffset(0)]
            public UInt64 valUInt64;

            [FieldOffset(0)]
            public ushort valUShort;
        }

        public struct ColorMapEntry
        {
            public dataValue_Struct dataValue;
            public int color;
        }
        //==============================================================================
        // 计算调色板，用于在显示区域显示OCT的采样数据对应的颜色
        //==============================================================================
        // void Calculate_Palette_Ex(int palette[256], int paletteC[256], ColorMapEntry colorArray[255], ColorMapEntry colorArrayC[255])
        void Calculate_Palette_Ex(int[] palette, int[] paletteC, ColorMapEntry[] colorArray, ColorMapEntry[] colorArrayC)
        {
            int i;

            for (i = 0; i < 255; i++)
            {
                colorArray[i].dataValue.valInt = i;
                //colorArray[i].color = MakeColor (i * 255 / (float)255, i * 255 / (float)255, i * 255 / (float)255);
                colorArray[i].color = System.Drawing.Color.FromArgb(i, i, i).ToArgb();
                colorArrayC[i].dataValue.valInt = i;
                colorArrayC[i].color = System.Drawing.Color.FromArgb((int)((Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14), 
                                                                     (int)(250 / (0.0005 * (i - 100) * (i - 100) + 1)),
                                                                     (int)(200 / (0.005 * (i - 40) * (i - 40) + 1)) ).ToArgb();
            }
            colorArray[254].color = MyData.VAL_WHITE;
            colorArrayC[254].color = MyData.VAL_RED;

            for (i = 0; i < 256; i++)
                paletteC[i] = System.Drawing.Color.FromArgb((int)((Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14),
                                                            (int)(250 / (0.0005 * (i - 100) * (i - 100) + 1)),
                                                            (int)(200 / (0.005 * (i - 40) * (i - 40) + 1))).ToArgb();
            for (i = 0; i < 256; i++)
                //palette[i] = MakeColor (i * 255 / (float)255, i * 255 / (float)255, i * 255 / (float)255);
                palette[i] = System.Drawing.Color.FromArgb(i, i, i).ToArgb();

            palette[254] = MyData.VAL_WHITE;
            paletteC[254] = MyData.VAL_RED;
            palette[255] = MyData.VAL_WHITE;
            paletteC[255] = MyData.VAL_RED;
        }
    }
}
