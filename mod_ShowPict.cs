using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace OctTools
{
    public class mod_ShowPict
    {
        public static double max = 0;
        public static double min = 0;

        //double[,] PictData = new double[PictWidth, PictWidth];
        double[,] ampplot_offset;
        double[,] ampplot_s_R90;
        double[,] avg;
        static mod_ReadGen2.Gen2_Struct Gen2;
        public mod_ReadGen2.Gen2_Struct pGen2
        {
            get { return Gen2; }
            set { Gen2 = value; }
        }

        public static int PictWidth = 700;
        public Bitmap BM;
        public BitmapImage Pict;

        public int RecordPictWidth = 0;
        public Bitmap RecordBM;
        public BitmapImage RecordPict;

        int brightness_inner = 0;
        public int PictBrightness                      //图像亮度调整值 (-255, 255)
        {
            get
            {
                return brightness_inner;
            }
            set
            {
                brightness_inner = value;
                RefreshPict();
            }
        }

        int contract_inner = 0;
        public int PictContract                        //图像对比系数调整 -100, 100 
        {
            get
            {
                return contract_inner;
            }
            set
            {
                contract_inner = value;
                RefreshPict();
            }
        }

        int displaymode = MyData.ColorDefault;
        public int DisplayMode
        {
            get { return displaymode; }
            set
            {
                displaymode = value;
                RefreshPict();
            }
        }

        int saturation_inner = 0;
        public int PictSaturation                         //图像色彩系数调整 -100, 100
        {
            get { return saturation_inner; }
            set
            {
                saturation_inner = value;
                RefreshPict();
            }
        }

        int Angle_inner = 0;
        public int Angle
        {
            get { return Angle_inner; }
            set
            {
                Angle_inner = value;
            }
        }


        public mod_GG gg1;
        //public mod_GG pgg1
        //{
        //    get { return gg1; }
        //    set
        //    {
        //        gg1 = value;
        //    }
        //}

        bool IsShowPict = false;
        public mod_ShowPict()
        {
            IsShowPict = false;
            Gen2 = new mod_ReadGen2.Gen2_Struct();
            Gen2.PictData = new double[PictWidth, PictWidth];

            gg1 = null;
            gg1 = new mod_GG();

            GetAllRefe();
        }

        public void GetAllRefe()
        {
            brightness_inner = MyData.BrightCurrent;
            contract_inner = MyData.ContractCurrent;
            displaymode = MyData.ColorCurrent;
            saturation_inner = MyData.SaturationCurrent;
            Angle_inner = MyData.AngleCurrent;
        }
        private void RefreshPict()
        {
            if (IsShowPict)
            {
                Gen2ToPict();
                if (gg1 != null)
                {
                    GGRecordToPict(RecordPictWidth);
                }
            }
            else
            {
                Pict = null;
                RecordPict = null;
            }
        }

        public void Gen2ToData()
        {
            IsShowPict = false;
            Gen2.PictData = new double[PictWidth, PictWidth];
            //PolarTables_Ex(Gen2.m_Number_line_360, Gen2.m_Angle2, 700, 700);
            PolarTables_Ex(Gen2.m_NScanSave, Gen2.m_Angle2, PictWidth, PictWidth);
            ReDoImage_avg(ref Gen2);
        }

        //============================================================================
        // Arrange the data array for 360 degree display and display the data as a
        // image in the panel for 360 degree view 
        //============================================================================
        //static void Panel4_Rescale_Graph(void);
        public void Gen2ToPict()                      // 把  Gen2.PictData --> BM 和 Pict
        {
            BM = DrawPict(Gen2.PictData, PictWidth, PictWidth, displaymode, brightness_inner, contract_inner);
            //pPB.Image = BM;

            using (MemoryStream stream = new MemoryStream())
            {
                BM.Save(stream, ImageFormat.Png); // 坑点：格式选Bmp时，不带透明度

                stream.Position = 0;
                Pict = new BitmapImage();
                Pict.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                Pict.CacheOption = BitmapCacheOption.OnLoad;
                Pict.StreamSource = stream;
                Pict.EndInit();
                Pict.Freeze();
            }
            IsShowPict = true;
        }

        public void GGRecordToPict(int iPWidth)       // 把  gg1.BeltData --> RecordBM 和 RecordPict
        {
            RecordBM = DrawRecordBelt(gg1, iPWidth, gg1.ggData.numberofpointofline*2, displaymode, brightness_inner, contract_inner);
            //pRecordPB = new PictureBox();
            //pRecordPB.Image = RecordBM;

            using (MemoryStream stream = new MemoryStream())
            {
                RecordBM.Save(stream, ImageFormat.Png); // 坑点：格式选Bmp时，不带透明度

                stream.Position = 0;
                RecordPict = new BitmapImage();
                RecordPict.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                RecordPict.CacheOption = BitmapCacheOption.OnLoad;
                RecordPict.StreamSource = stream;
                RecordPict.EndInit();
                RecordPict.Freeze();
            }
        }


        /// <summary>
        /// 铁红色带映射表
        /// 每一行代表一个彩色分类，存放顺序是RGB
        /// </summary>
        public static byte[,] ironTable = new byte[128, 3]
        {
            {0,   0,  0},
            {0,   0,  0},
            {0,   0,  36},
            {0,   0,  51},
            {0,   0,  66},
            {0,   0,  81},
            {2,   0,  90},
            {4,   0,  99},
            {7,   0, 106},
            {11,   0, 115},
            {14,   0, 119},
            {20,   0, 123},
            {27,   0, 128},
            {33,   0, 133},
            {41,   0, 137},
            {48,   0, 140},
            {55,   0, 143},
            {61,   0, 146},
            {66,   0, 149},
            {72,   0, 150},
            {78,   0, 151},
            {84,   0, 152},
            {91,   0, 153},
            {97,   0, 155},
            {104,   0, 155},
            {110,   0, 156},
            {115,   0, 157},
            {122,   0, 157},
            {128,   0, 157},
            {134,   0, 157},
            {139,   0, 157},
            {146,   0, 156},
            {152,   0, 155},
            {157,   0, 155},
            {162,   0, 155},
            {167,   0, 154},
            {171,   0, 153},
            {175,   1, 152},
            {178,   1, 151},
            {182,   2, 149},
            {185,   4, 149},
            {188,   5, 147},
            {191,   6, 146},
            {193,   8, 144},
            {195,  11, 142},
            {198,  13, 139},
            {201,  17, 135},
            {203,  20, 132},
            {206,  23, 127},
            {208,  26, 121},
            {210,  29, 116},
            {212,  33, 111},
            {214,  37, 103},
            {217,  41,  97},
            {219,  46,  89},
            {221,  49,  78},
            {223,  53,  66},
            {224,  56,  54},
            {226,  60,  42},
            {228,  64,  30},
            {229,  68,  25},
            {231,  72,  20},
            {232,  76,  16},
            {234,  78,  12},
            {235,  82,  10},
            {236,  86,   8},
            {237,  90,   7},
            {238,  93,   5},
            {239,  96,   4},
            {240, 100,   3},
            {241, 103,   3},
            {241, 106,   2},
            {242, 109,   1},
            {243, 113,   1},
            {244, 116,   0},
            {244, 120,   0},
            {245, 125,   0},
            {246, 129,   0},
            {247, 133,   0},
            {248, 136,   0},
            {248, 139,   0},
            {249, 142,   0},
            {249, 145,   0},
            {250, 149,   0},
            {251, 154,   0},
            {252, 159,   0},
            {253, 163,   0},
            {253, 168,   0},
            {253, 172,   0},
            {254, 176,   0},
            {254, 179,   0},
            {254, 184,   0},
            {254, 187,   0},
            {254, 191,   0},
            {254, 195,   0},
            {254, 199,   0},
            {254, 202,   1},
            {254, 205,   2},
            {254, 208,   5},
            {254, 212,   9},
            {254, 216,  12},
            {255, 219,  15},
            {255, 221,  23},
            {255, 224,  32},
            {255, 227,  39},
            {255, 229,  50},
            {255, 232,  63},
            {255, 235,  75},
            {255, 238,  88},
            {255, 239, 102},
            {255, 241, 116},
            {255, 242, 134},
            {255, 244, 149},
            {255, 245, 164},
            {255, 247, 179},
            {255, 248, 192},
            {255, 249, 203},
            {255, 251, 216},
            {255, 253, 228},
            {255, 254, 239},
            {255, 255, 249},
            {255, 255, 249},
            {255, 255, 249},
            {255, 255, 249},
            {255, 255, 249},
            {255, 255, 249},
            {255, 255, 249},
            {255, 255, 249}
        };

        /// <summary>
        /// 彩虹色带映射表
        /// </summary>
        public static byte[,] rainTable = new byte[128, 3]
        {
            {0,   0,   0},
            {0,   0,   0},
            {15,   0,  15},
            {31,   0,  31},
            {47,   0,  47},
            {63,   0,  63},
            {79,   0,  79},
            {95,   0,  95},
            {111,   0, 111},
            {127,   0, 127},
            {143,   0, 143},
            {159,   0, 159},
            {175,   0, 175},
            {191,   0, 191},
            {207,   0, 207},
            {223,   0, 223},
            {239,   0, 239},
            {255,   0, 255},
            {239,   0, 250},
            {223,   0, 245},
            {207,   0, 240},
            {191,   0, 236},
            {175,   0, 231},
            {159,   0, 226},
            {143,   0, 222},
            {127,   0, 217},
            {111,   0, 212},
            {95,   0, 208},
            {79,   0, 203},
            {63,   0, 198},
            {47,   0, 194},
            {31,   0, 189},
            {15,   0, 184},
            {0,   0, 180},
            {0,  15, 184},
            {0,  31, 189},
            {0,  47, 194},
            {0,  63, 198},
            {0,  79, 203},
            {0,  95, 208},
            {0, 111, 212},
            {0, 127, 217},
            {0, 143, 222},
            {0, 159, 226},
            {0, 175, 231},
            {0, 191, 236},
            {0, 207, 240},
            {0, 223, 245},
            {0, 239, 250},
            {0, 255, 255},
            {0, 245, 239},
            {0, 236, 223},
            {0, 227, 207},
            {0, 218, 191},
            {0, 209, 175},
            {0, 200, 159},
            {0, 191, 143},
            {0, 182, 127},
            {0, 173, 111},
            {0, 164,  95},
            {0, 155,  79},
            {0, 146,  63},
            {0, 137,  47},
            {0, 128,  31},
            {0, 119,  15},
            {0, 110,   0},
            {15, 118,   0},
            {30, 127,   0},
            {45, 135,   0},
            {60, 144,   0},
            {75, 152,   0},
            {90, 161,   0},
            {105, 169,  0},
            {120, 178,  0},
            {135, 186,  0},
            {150, 195,  0},
            {165, 203,  0},
            {180, 212,  0},
            {195, 220,  0},
            {210, 229,  0},
            {225, 237,  0},
            {240, 246,  0},
            {255, 255,  0},
            {251, 240,  0},
            {248, 225,  0},
            {245, 210,  0},
            {242, 195,  0},
            {238, 180,  0},
            {235, 165,  0},
            {232, 150,  0},
            {229, 135,  0},
            {225, 120,  0},
            {222, 105,  0},
            {219,  90,  0},
            {216,  75,  0},
            {212,  60,  0},
            {209,  45,  0},
            {206,  30,  0},
            {203,  15,  0},
            {200,   0,  0},
            {202,  11,  11},
            {205,  23,  23},
            {207,  34,  34},
            {210,  46,  46},
            {212,  57,  57},
            {215,  69,  69},
            {217,  81,  81},
            {220,  92,  92},
            {222, 104, 104},
            {225, 115, 115},
            {227, 127, 127},
            {230, 139, 139},
            {232, 150, 150},
            {235, 162, 162},
            {237, 173, 173},
            {240, 185, 185},
            {242, 197, 197},
            {245, 208, 208},
            {247, 220, 220},
            {250, 231, 231},
            {252, 243, 243},
            {252, 243, 243},
            {252, 243, 243},
            {252, 243, 243},
            {252, 243, 243},
            {252, 243, 243},
            {252, 243, 243},
            {252, 243, 243}
        };

        public static Bitmap DrawPict(double[,] PictData, int iWidth, int iHeight, int ColorModel, int iBright0, int iContract)
        {
            Bitmap img = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb); ;
            Rectangle rect = new Rectangle(0, 0, iWidth, iHeight);
            System.Drawing.Imaging.BitmapData bmpData = img.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            double PointData = 0;
            byte R = 0, G = 0, B = 0;
            int temp = 0;
            double[,] tempPalette = new double[256, 3];

            int iBright = (int)(iBright0 * 2.55);
            if (ColorModel == MyData.DisplayMode_Orig)
            {
                //tempPalette.Entries[i] = Color.FromArgb(255, (int)((Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14), (int)(250 / (0.0005 * (i - 100) * (i - 100) + 1)), (int)(200 / (0.005 * (i - 40) * (i - 40) + 1)));
                for (int i = 0; i < 255; i++)
                {
                    tempPalette[i, 2] = (Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14;
                    tempPalette[i, 1] = 250 / (0.0005 * (i - 100) * (i - 100) + 1);
                    tempPalette[i, 0] = 200 / (0.005 * (i - 40) * (i - 40) + 1);
                }
            }

            // 圆
            double CenX = bmpData.Width / 2;
            double CenY = bmpData.Height / 2;
            double RR = CenX;                        // 半径
            double rr = 0;                           // 当前点的距离

            // 点的值 = 原值*对比 + 亮度
            // 对比 = 斜率，  
            // 对比参数 = -100 -- +100
            double k = (iContract <= 80) ? iContract : 80;   // 斜率
            k = Math.Tan(PI_45 * (1 + k / 100));
            for (int i = 0; i < bmpData.Height; i++)
            {
                for (int j = 0; j < bmpData.Width; j++)
                {
                    //tempPalette.Entries[i] = Color.FromArgb(255, (int)((Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14), (int)(250 / (0.0005 * (i - 100) * (i - 100) + 1)), (int)(200 / (0.005 * (i - 40) * (i - 40) + 1)));
                    //colorTemp = rgbValues[i * bmpData.Stride + j + 2] * 0.299 + rgbValues[i * bmpData.Stride + j + 1] * 0.578 + rgbValues[i * bmpData.Stride + j] * 0.114;
                    rr = Math.Sqrt(Math.Pow(CenY - i, 2) + Math.Pow(CenX - j, 2));
                    if (rr > RR)                      // 圈外
                    {
                        PointData = 0;
                    }
                    else    // 圈内
                    {
                        PointData = PictData[i, j];

                        // 颜色
                        switch (ColorModel)
                        {
                            case MyData.DisplayMode_WhiteGray:
                                rgbValues[i * bmpData.Stride + j * 3] = (byte)PointData;
                                rgbValues[i * bmpData.Stride + j * 3 + 1] = (byte)PointData;
                                rgbValues[i * bmpData.Stride + j * 3 + 2] = (byte)PointData;
                                break;
                            case MyData.DisplayMode_IronTable:
                                temp = (int)(PointData / 2);
                                R = ironTable[temp, 0];
                                G = ironTable[temp, 1];
                                B = ironTable[temp, 2];

                                rgbValues[i * bmpData.Stride + j * 3] = B;
                                rgbValues[i * bmpData.Stride + j * 3 + 1] = G;
                                rgbValues[i * bmpData.Stride + j * 3 + 2] = R;
                                break;
                            case MyData.DisplayMode_RainTable:
                                temp = (int)(PointData / 2);
                                R = rainTable[temp, 0];
                                G = rainTable[temp, 1];
                                B = rainTable[temp, 2];

                                rgbValues[i * bmpData.Stride + j * 3] = B;
                                rgbValues[i * bmpData.Stride + j * 3 + 1] = G;
                                rgbValues[i * bmpData.Stride + j * 3 + 2] = R;
                                break;
                            case MyData.DisplayMode_Orig:
                                rgbValues[i * bmpData.Stride + j * 3] = (byte)tempPalette[(int)PointData, 0];
                                rgbValues[i * bmpData.Stride + j * 3 + 1] = (byte)tempPalette[(int)PointData, 1];
                                rgbValues[i * bmpData.Stride + j * 3 + 2] = (byte)tempPalette[(int)PointData, 2];
                                break;
                            case MyData.DisplayMode_BW:
                                rgbValues[i * bmpData.Stride + j * 3] = (byte)(255 - PointData);
                                rgbValues[i * bmpData.Stride + j * 3 + 1] = (byte)(255 - PointData);
                                rgbValues[i * bmpData.Stride + j * 3 + 2] = (byte)(255 - PointData);
                                break;
                            case MyData.DisplayMode_Yello:
                            default:
                                rgbValues[i * bmpData.Stride + j * 3] = (byte)(PointData * 0.2);                  // B
                                rgbValues[i * bmpData.Stride + j * 3 + 1] = (byte)(PointData * 0.4);              // G
                                rgbValues[i * bmpData.Stride + j * 3 + 2] = (byte)(PointData * 0.8);              // R

                                //G = (byte)(PointData * 0.393 + PointData * 0.349 + PointData * 272);
                                //R = (byte)(PointData * 0.769 + PointData * 0.686 + PointData * 534);
                                //B = (byte)(PointData * 0.189 + PointData * 0.168 + PointData * 131);
                                //rgbValues[i * bmpData.Stride + j * 3] = B;
                                //rgbValues[i * bmpData.Stride + j * 3 + 1] = G;
                                //rgbValues[i * bmpData.Stride + j * 3 + 2] = R;
                                break;
                        }


                        double dB = 0;
                        //========================
                        if (rgbValues[i * bmpData.Stride + j * 3] <= 127)
                            dB = rgbValues[i * bmpData.Stride + j * 3] * k + iBright;
                        else
                            dB = rgbValues[i * bmpData.Stride + j * 3] * k + iBright;

                        if (dB < 0)
                            rgbValues[i * bmpData.Stride + j * 3] = 0;
                        else if (dB > 255)
                            rgbValues[i * bmpData.Stride + j * 3] = 255;
                        else
                            rgbValues[i * bmpData.Stride + j * 3] = (byte)dB;

                        //=========================
                        if (rgbValues[i * bmpData.Stride + j * 3 + 1] <= 127)
                            dB = rgbValues[i * bmpData.Stride + j * 3 + 1] * k + iBright;
                        else
                            dB = rgbValues[i * bmpData.Stride + j * 3 + 1] * k + iBright;

                        if (dB < 0)
                            rgbValues[i * bmpData.Stride + j * 3 + 1] = 0;
                        else if (dB > 255)
                            rgbValues[i * bmpData.Stride + j * 3 + 1] = 255;
                        else
                            rgbValues[i * bmpData.Stride + j * 3 + 1] = (byte)dB;

                        //=========================
                        if (rgbValues[i * bmpData.Stride + j * 3 + 2] <= 127)
                            dB = rgbValues[i * bmpData.Stride + j * 3 + 2] * k + iBright;
                        else
                            dB = rgbValues[i * bmpData.Stride + j * 3 + 2] * k + iBright;

                        if (dB < 0)
                            rgbValues[i * bmpData.Stride + j * 3 + 2] = 0;
                        else if (dB > 255)
                            rgbValues[i * bmpData.Stride + j * 3 + 2] = 255;
                        else
                            rgbValues[i * bmpData.Stride + j * 3 + 2] = (byte)dB;
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            img.UnlockBits(bmpData);
            return img;
        }

        public static Bitmap DrawRecordBelt(mod_GG GG, int iWidth, int iHeight, int ColorModel, int iBright, int iContract)
        {
            int iStepX = (int)(iWidth / GG.ggData.numberof_frames);            // 按光标步长，取图像的宽度

            int iPictWidth = (int)Math.Round( (double)iStepX * GG.ggData.numberof_frames);
            int iPictHeight = iHeight;

            Bitmap img = new Bitmap(iPictWidth, iPictHeight, PixelFormat.Format24bppRgb); ;
            Rectangle rect = new Rectangle(0, 0, iPictWidth, iPictHeight);
            System.Drawing.Imaging.BitmapData bmpData = img.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            double PointData = 0;
            byte R = 0, G = 0, B = 0;
            int temp = 0;
            double[,] tempPalette = new double[256, 3];

            if (ColorModel == MyData.DisplayMode_Orig)
            {
                //tempPalette.Entries[i] = Color.FromArgb(255, (int)((Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14), (int)(250 / (0.0005 * (i - 100) * (i - 100) + 1)), (int)(200 / (0.005 * (i - 40) * (i - 40) + 1)));
                for (int i = 0; i < 255; i++)
                {
                    tempPalette[i, 2] = (Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14;
                    tempPalette[i, 1] = 250 / (0.0005 * (i - 100) * (i - 100) + 1);
                    tempPalette[i, 0] = 200 / (0.005 * (i - 40) * (i - 40) + 1);
                }
            }

            // 点的值 = 原值*对比 + 亮度
            // 对比 = 斜率，  
            // 对比参数 = -100 -- +100
            double k = (iContract <= 80) ? iContract : 80;   // 斜率
            k = Math.Tan(PI_45 * (1 + k / 100));

            int iRow = 0;
            int iCol = 0;

            for (int i = 0; i < iPictHeight; i++)
            {
                for (int j = 0; j < GG.ggData.numberof_frames; j++)      // 带图的宽，实际上是帧的数量 * iStepX
                {
                    if (i < GG.ggData.numberofpointofline )
                    {
                        iRow = GG.ggData.numberofpointofline - i - 1;
                        iCol = j;
                    }
                    else
                    {
                        iRow = i - GG.ggData.numberofpointofline;         // 下半部分图像
                        iCol = (int)GG.ggData.numberof_frames + j;
                    }
                    PointData = GG.ggRecordBeltData[iCol, iRow];

                    for (int m = 0; m < iStepX; m++)
                    {
                        // 颜色
                        switch (ColorModel)
                        {
                            case MyData.DisplayMode_WhiteGray:
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] = (byte)PointData;
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] = (byte)PointData;
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] = (byte)PointData;
                                break;
                            case MyData.DisplayMode_IronTable:
                                temp = (int)(PointData / 2);
                                R = ironTable[temp, 0];
                                G = ironTable[temp, 1];
                                B = ironTable[temp, 2];

                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] = B;
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] = G;
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] = R;
                                break;
                            case MyData.DisplayMode_RainTable:
                                temp = (int)(PointData / 2);
                                R = rainTable[temp, 0];
                                G = rainTable[temp, 1];
                                B = rainTable[temp, 2];

                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] = B;
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] = G;
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] = R;
                                break;
                            case MyData.DisplayMode_Orig:
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] = (byte)tempPalette[(int)PointData, 0];
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] = (byte)tempPalette[(int)PointData, 1];
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] = (byte)tempPalette[(int)PointData, 2];
                                break;
                            case MyData.DisplayMode_BW:
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] = (byte)(255 - PointData);
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] = (byte)(255 - PointData);
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] = (byte)(255 - PointData);
                                break;
                            case MyData.DisplayMode_Yello:
                            default:
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] = (byte)(PointData * 0.2);                  // B
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] = (byte)(PointData * 0.4);              // G
                                rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] = (byte)(PointData * 0.8);              // R

                                //G = (byte)(PointData * 0.393 + PointData * 0.349 + PointData * 272);
                                //R = (byte)(PointData * 0.769 + PointData * 0.686 + PointData * 534);
                                //B = (byte)(PointData * 0.189 + PointData * 0.168 + PointData * 131);
                                //rgbValues[i * bmpData.Stride + j * 3] = B;
                                //rgbValues[i * bmpData.Stride + j * 3 + 1] = G;
                                //rgbValues[i * bmpData.Stride + j * 3 + 2] = R;
                                break;
                        }


                        double dB = 0;
                        //========================
                        if (rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] <= 127)
                            dB = rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] * k + iBright;
                        else
                            dB = rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] * k + iBright;

                        if (dB < 0)
                            rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] = 0;
                        else if (dB > 255)
                            rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3] = 255;
                        else
                            rgbValues[i* bmpData.Stride + (j * iStepX + m) * 3] = (byte)dB;

                        //=========================
                        if (rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] <= 127)
                            dB = rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] * k + iBright;
                        else
                            dB = rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] * k + iBright;

                        if (dB < 0)
                            rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] = 0;
                        else if (dB > 255)
                            rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] = 255;
                        else
                            rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 1] = (byte)dB;

                        //=========================
                        if (rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] <= 127)
                            dB = rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] * k + iBright;
                        else
                            dB = rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] * k + iBright;

                        if (dB < 0)
                            rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] = 0;
                        else if (dB > 255)
                            rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] = 255;
                        else
                            rgbValues[i * bmpData.Stride + (j * iStepX + m) * 3 + 2] = (byte)dB;
                    }
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            img.UnlockBits(bmpData);
            return img;
        }


        /// <summary> 
        /// Polar transfer for 360 degree display
        /// 以下计算 平面坐标（如700*700）的每一个点，对应的扫描数据（极坐标）
        /// 例：知道了平面坐标的某个点x,y， 在 Distance[x,y]保存它的距离r， 在iLine[x,y]保存它属于第几线n
        ///     然后在PictData[n,r]取得数据
        /// 
        /// 计算屏幕显示每一个像素的显示值对应于采样数据阵列中的行与列的位置
        /// 700 * 700		    距阵的屏幕显示范围
        /// Number_line_360      旋转一周有多少条扫描线
        /// ft_Angle				图形旋转多少角度
        /// iW					计算显示区域的宽度   700
        /// iH					计算显示区域的高度   700
        /// Distance				屏幕上每一个点对应扫描线上的那一个点
        /// iLine				屏幕上每一个点对应扫描数据中那一条线上数据
        /// </summary>
        const double PI = 3.1415926535897932384626433832795028841971;
        const double PI_45 = PI / 4;
        const double PI_90 = PI / 2;
        const double PI_180 = PI;                    // 180度的圆周值
        const double PI_360 = PI * 2;                // 360 度的圆周值
        const double PI_360_Div = PI * 2 / 360.0;    // 细分每一度的圆周值
        static int[,] Distance;
        static int[,] iLine;
        // 原装定义 void PolarTables_Ex(int Number_line_360, float ft_Angle, int iW, int iH, int Distance[2000][2000], int iLine[5000][2000])
        public static void PolarTables_Ex(int Number_line_360, float ft_Angle, int iW, int iH)   // 生成到 Distance 和 iLine
        {
            int x, y;
            int iWCenter, iHCenter;         //显示区域的中心位置
            double a = 0, xx = 0, yy = 0, dd = 0;
            double dba;

            iWCenter = iW >> 1;                      //除以2 = 宽度的中心位置
            iHCenter = iH >> 1;                      //除以2 = 高度的中心位置
            dd = ft_Angle * PI_360_Div;              //显示的偏转角度
            dba = PI_360 / Number_line_360;          //每一扫描线的圆周率

            Distance = new int[PictWidth, PictWidth];
            iLine = new int[PictWidth, PictWidth];

            //  计算屏幕显示的每一个点对应的位置 700 表示屏幕显示使用的是700宽700高的显示空间
            //for (x=0; x<700; x++)
            for (x = 0; x < iW; x++)
            {
                xx = (double)x;
                xx -= iWCenter;
                //for (y=0; y < 700; y++)
                for (y = 0; y < iH; y++)
                {
                    yy = (double)y;
                    yy -= iHCenter;

                    //D[y][x]=sqrt((x - iWCenter) * (x - iWCenter) + (y - iHCenter) * (y - iHCenter));			//计算每个像素的与中心的距离，这个距离可以直接代入采样的数据的列值中
                    Distance[y, x] = (int)Math.Sqrt(xx * xx + yy * yy);           //计算每个像素的与中心的距离，这个距离可以直接代入采样的数据的列值中

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
                    iLine[y, x] = (int)(a / dba);//(a/6.283)*Number_line_360;      // 属于那条 射线

                }
            }

        }

        /// <summary>         
        /// Arrange the data array for 360 degree display and get the brightness, 
        ///   contrast, smoothness from the panel for display
        ///   将采样数据转换成360度旋转显示的图形数据
        ///   产生的数据是一张700*700的灰度图像
        /// </summary>         
        public void ReDoImage_avg(ref mod_ReadGen2.Gen2_Struct Gen2)
        {
            int x, y, n, j, k, N;
            double dd, d;
            int i, z, gg, jj;
            double a, c1, c2, nr, t1;
            float fldd;
            double dblmax_min;
            double dbmax, dbmin;

            //dbmax = max;
            dbmax = Gen2.m_maxValue;
            //dbmin = min;
            if (Gen2.m_minValue <= 0)
                dbmin = 1.2;
            else
                dbmin = Gen2.m_minValue;

            ampplot_offset = new double[Gen2.ampplot_sI.GetUpperBound(0) + 1, Gen2.ampplot_sI.GetUpperBound(1) + 1];
            ampplot_s_R90 = new double[Gen2.ampplot_sI.GetUpperBound(0) + 1, Gen2.ampplot_sI.GetUpperBound(1) + 1];
            avg = new double[Gen2.ampplot_sI.GetUpperBound(0) + 1, Gen2.ampplot_sI.GetUpperBound(1) + 1];

            for (x = 0; x < Gen2.ampplot_sI.GetUpperBound(0) + 1; x++)
                for (y = 0; y < Gen2.ampplot_sI.GetUpperBound(1) + 1; y++)
                    ampplot_offset[x, y] = Gen2.ampplot_s[x, y];

            if (MyData.AScan_Offset != 0)                                                          // 为什么要移动
            {
                // OffsetOCTIS_ScanLine(ampplot_offset, NScan, 512, mod_MyData.AScan_Offset);      // 原语句
                OffsetOCTIS_ScanLine(ampplot_offset, Gen2.m_NScanSave, Gen2.m_NData, MyData.AScan_Offset);
            }

            //	DD=10;    DD表示全范围
            //	G=93;	  G表示选择范围
            // imageID  表示要比较的两张图 1 为第一张图，2为第二张图
            //if (PanelID == ID_IMAGECOMPPANEL && imageID == 1)
            //    range = range1;
            //else if (PanelID == ID_IMAGECOMPPANEL && imageID == 2)
            //    range = range2;

            // WK：以下变量加进去以保持原程序的格式
            int range = 0;
            int DD = Gen2.m_DD;                               // 中心圆的半径
            int G = 0;
            int NScan = Gen2.m_NScanSave;
            int SN = 0;
            // Add End

            if (range == 1)  //显示放大后的图形
            {
                k = (int)((PictWidth / 2 - G) / 2.5);  // k 表示每毫米占用屏幕的多少个像素
                                                       // 256表示当前显示屏的数据深度是256个采样数据
                                                       // 350-G  表示用于显示的数据屏幕距离
                                                       // SN 表示用于移动放大后的显示数据
                fldd = (float)256 / (PictWidth / 2 - G);
                for (j = 0; j < NScan; j++)
                {
                    //if (PanelID == ID_IMAGECOMPPANEL)
                    //{
                    //    for (i = 0; i < G; i++)
                    //        ampplot_s_R90[j][i] = 0.0;
                    //}

                    for (i = G; i < PictWidth / 2; i++)
                        //ampplot_s_R90[j][i] = ampplot_s[j][(i-G)*256/(350-G)+SN];
                        //ampplot_s_R90[j][i] = ampplot_s[j][(int)((i-G)*fldd + SN)];
                        ampplot_s_R90[j, i] = ampplot_offset[j, (int)((i - G) * fldd + SN)];
                }
            }
            else            //显示完整图形
            {
                k = (PictWidth / 2 - DD) / 5;      // k 表示每毫米占用屏幕的多少个像素,                 WK注：512个像素映射出来的图像是对应 5个mm 的扫描空间，350 - DD = 实际显示空间的像素的大小
                                                   // 512表示当前显示屏的数据深度是256个采样数据
                                                   // 350-DD  表示用于显示的数据屏幕距离
                fldd = (float)512 / (PictWidth / 2 - DD);  // wk:512个采样数据 分布于 350 - DD 的空间中

                for (j = 0; j < NScan; j++)
                {
                    //if (PanelID == ID_IMAGECOMPPANEL)
                    //{
                    //    for (i = 0; i < DD; i++)
                    //        ampplot_s_R90[j][i] = 0.0;
                    //}

                    for (i = DD; i < PictWidth / 2; i++)
                        //ampplot_s_R90[j][i] = ampplot_s[j][((i-DD)*512)/(350-DD)];
                        //ampplot_s_R90[j][i] = ampplot_s[j][(int)((i-DD)*fldd)];
                        ampplot_s_R90[j, i] = ampplot_offset[j, (int)((i - DD) * fldd)];
                }
            }

            if (MyData.display_flg == 0)
            {  //问题可能会出现在这里
               //max=max+4.0;
               // 修改的部份
                dbmax = dbmax + 4.0;
            }

            if (MyData.IT > 0 && MyData.AL > 0)
            {
                for (j = 0; j < NScan; j++)
                {
                    for (i = 0; i < PictWidth / 2; i++)
                    {
                        avg[j, i] = ampplot_s_R90[j, i];
                    }
                }

                a = 0.5 * MyData.AL + 0.1;
                c1 = Math.Exp(-1 / (a * a));
                c2 = Math.Exp(-(Math.Sqrt(2) * Math.Sqrt(2)) / (a * a));

                nr = 1 / (1 + 4 * c1 + 4 * c2);

                for (gg = 0; gg < MyData.IT; gg++)
                {
                    for (j = 0; j < NScan; j++)
                    {
                        for (z = 0; z < PictWidth / 2; z++)
                        {
                            if (z == 0)
                            {
                                if (j == 0)
                                {
                                    avg[j, z] = (avg[j, z] + c1 * (avg[j, z + 1] + avg[j + 1, z]) + c2 * avg[j + 1, z + 1]) / (1 + 2 * c1 + c2);
                                }
                                else if (j == (NScan - 1))
                                {
                                    avg[j, z] = (avg[j, z] + c1 * (avg[j, z + 1] + avg[j - 1, z]) + c2 * avg[j - 1, z + 1]) / (1 + 2 * c1 + c2);
                                }
                                else
                                    avg[j, z] = (avg[j, z] + c1 * (avg[j - 1, z] + avg[j + 1, z] + avg[j, z + 1]) + c2 * (avg[j - 1, z + 1] + avg[j + 1, z + 1])) / (1 + 3 * c1 + 2 * c2);
                            }

                            else if (z == PictWidth / 2)
                            {
                                if (j == 0)
                                {
                                    avg[j, z] = (avg[j, z] + c1 * (avg[j, z - 1] + avg[j + 1, z]) + c2 * avg[j + 1, z - 1]) / (1 + 2 * c1 + c2);
                                }
                                else if (j == (NScan - 1))
                                {
                                    avg[j, z] = (avg[j, z] + c1 * (avg[j, z - 1] + avg[j - 1, z]) + c2 * avg[j - 1, z - 1]) / (1 + 2 * c1 + c2);
                                }
                                else
                                    avg[j, z] = (avg[j, z] + c1 * (avg[j - 1, z] + avg[j + 1, z] + avg[j, z - 1]) + c2 * (avg[j - 1, z - 1] + avg[j + 1, z - 1])) / (1 + 3 * c1 + 2 * c2);
                            }
                            else if (j == 0)
                            {
                                avg[j, z] = (avg[j, z] + c1 * (avg[j, z - 1] + avg[j + 1, z] + avg[j, z + 1]) + c2 * (avg[j + 1, z - 1] + avg[j + 1, z + 1])) / (1 + 3 * c1 + 2 * c2);
                            }
                            else if (j == (NScan - 1))
                            {
                                avg[j, z] = (avg[j, z] + c1 * (avg[j, z - 1] + avg[j - 1, z] + avg[j, z + 1]) + c2 * (avg[j - 1, z - 1] + avg[j - 1, z + 1])) / (1 + 3 * c1 + 2 * c2);
                            }
                            else
                            {
                                avg[j, z] = nr * (avg[j, z] + c1 * (avg[j, z - 1] + avg[j, z + 1] + avg[j - 1, z] + avg[j + 1, z])
                                            + c2 * (avg[j - 1, z - 1] + avg[j + 1, z + 1] + avg[j - 1, z + 1] + avg[j + 1, z - 1]));
                            }
                        }
                    }
                }

                n = 0;
                //dblmax_min = 255 / (max-min);
                dblmax_min = 255 / (dbmax - dbmin);
                for (y = 0; y < PictWidth; y++)
                {
                    for (x = 0; x < PictWidth; x++)
                    {
                        d = avg[iLine[y, x], Distance[y, x]];

                        //if (d>min &&  d<max)
                        if (d > dbmin && d < dbmax)
                        {
                            //ImaqBuffer[n] = ((d-min)/(max-min))*255;
                            //ImaqBuffer[n] = (d-min) * dblmax_min;//(/(max-min))*255;
                            Gen2.PictData[y, x] = (d - dbmin) * dblmax_min;//(/(max-min))*255;
                        }
                        //else if  ( d< min )
                        else if (d < dbmin)
                            Gen2.PictData[y, x] = 0;
                        //else if  ( d>max && d<50 )
                        else if (d > dbmax && d < 50)
                            Gen2.PictData[y, x] = 254;
                        else
                            Gen2.PictData[y, x] = 255;
                        //n++;
                    }
                }
            }

            else
            {
                n = 0;
                //dblmax_min = 255 / (max-min);
                dblmax_min = 255 / (dbmax - dbmin);
                for (y = 0; y < PictWidth; y++)
                {
                    for (x = 0; x < PictWidth; x++)
                    {
                        d = ampplot_s_R90[iLine[y, x], Distance[y, x]];
                        //if (d>min &&  d<max)
                        if (d > dbmin && d < dbmax)
                        {
                            //ImaqBuffer[n] = ((d-min)/(max-min))*255;
                            //ImaqBuffer[n] = (d-min) * dblmax_min;// (/(max-min))*255;
                            Gen2.PictData[y, x] = (d - dbmin) * dblmax_min;// (/(max-min))*255;
                        }
                        //else if  ( d<min )
                        else if (d < dbmin)
                            Gen2.PictData[y, x] = 0;
                        //else if  ( d>max && d<50 )
                        else if (d > dbmax && d < 50)
                            Gen2.PictData[y, x] = 254;
                        else
                            Gen2.PictData[y, x] = 255;
                        n++;
                    }
                }
            }
        }

        /*
        // 在回放或是显示gen2的图像，以单帧的形式显示
        static void Panel4_Rescale_Graph()
        {
            
            //if (!lpCtrlBar)                                                 // ??
                ReDoImage_avg();

            /*
            DeleteGraphPlot(analysispanel, PANEL_4_GRAPH, -1, VAL_DELAYED_DRAW);

            if (display_flg == 0)       //显示彩色的图形
                PlotScaledIntensity(analysispanel, PANEL_4_GRAPH, ImaqBuffer, 700, 700, VAL_UNSIGNED_CHAR,
                    1.0, 0.0, 1.0, 0.0, colorArrayC, VAL_RED, 255, 1, 1);
            else if (display_flg == -1)
                PlotScaledIntensity(analysispanel, PANEL_4_GRAPH, ImaqBuffer, 700, 700, VAL_UNSIGNED_CHAR,
                    1.0, 0.0, 1.0, 0.0, colorArrayCO, VAL_RED, 255, 1, 1);
            else                        //显示灰度图形式
                PlotScaledIntensity(analysispanel, PANEL_4_GRAPH, ImaqBuffer, 700, 700, VAL_UNSIGNED_CHAR,
                    1.0, 0.0, 1.0, 0.0, colorArray, VAL_WHITE, 255, 1, 1);

            //--------------------------------  在图像上画出切线位置 及中心点
            if (lpCtrlBar)
                DrawOCTIS_SliceLine_Graphs(analysispanel, lpCtrlBar, m_lightspectrum_startpos, Angle2);

            //  OCTIS切面图的中心画出中心点
            DrawOCTRawCenterToGraphs(analysispanel, PANEL_4_GRAPH);

            //Panel4_ShowAscanLine(analysispanel, 10, 512);

            if (markerFlg == 1)
            {
                marker();
            }
            

            return;
        }

        void ReDoImage_avgX()
        {
            int x, y, n, j, k, N;
            double dd, d;
            int i, z, gg, jj;
            double a, c1, c2, nr, t1;
            double dbmax, dbmin;
            double dblmax_min;
            /*
            dbmax = max;
            dbmin = min;
            //	DD=10;
            //	G=93;
            if (PanelID == ID_IMAGECOMPPANEL && imageID == 1)
                range = range1;
            else if (PanelID == ID_IMAGECOMPPANEL && imageID == 2)
                range = range2;

            if (range == 1)
            {
                k = (400 - G) / 2.5;
                for (j = 0; j < NScan; j++)
                {
                    if (PanelID == ID_IMAGECOMPPANEL)
                    {
                        for (i = 0; i < G; i++)
                            ampplot_s_R90[j][i] = 0.0;
                    }

                    for (i = G; i < 400; i++)
                        ampplot_s_R90[j][i] = ampplot_s[j][(i - G) * 256 / (400 - G) + SN];
                }
            }
            else
            {
                k = (400 - DD) / 5;
                for (j = 0; j < NScan; j++)
                {
                    if (PanelID == ID_IMAGECOMPPANEL)
                    {
                        for (i = 0; i < DD; i++)
                            ampplot_s_R90[j][i] = 0.0;
                    }

                    for (i = DD; i < 400; i++)
                        ampplot_s_R90[j][i] = ampplot_s[j][((i - DD) * 512) / (400 - DD)];
                }
            }

            if (display_flg == 0)
            {
                //max=max+4.0;
                dbmax += 4.0;
            }

            if (IT > 0 && AL > 0)
            {
                for (j = 0; j < NScan; j++)
                {
                    for (i = 0; i < 400; i++)
                    {
                        avg[j][i] = ampplot_s_R90[j][i];
                    }
                }

                a = 0.5 * AL + 0.1;
                c1 = exp(-1 / (a * a));
                c2 = exp(-(sqrt(2) * sqrt(2)) / (a * a));

                nr = 1 / (1 + 4 * c1 + 4 * c2);

                for (gg = 0; gg < IT; gg++)
                {
                    for (j = 0; j < NScan; j++)
                    {
                        for (z = 0; z < 400; z++)
                        {
                            if (z == 0)
                            {
                                if (j == 0)
                                {
                                    avg[j][z] = (avg[j][z] + c1 * (avg[j][z + 1] + avg[j + 1][z]) + c2 * avg[j + 1][z + 1]) / (1 + 2 * c1 + c2);
                                }
                                else if (j == NScan)
                                {
                                    avg[j][z] = (avg[j][z] + c1 * (avg[j][z + 1] + avg[j - 1][z]) + c2 * avg[j - 1][z + 1]) / (1 + 2 * c1 + c2);
                                }
                                else
                                    avg[j][z] = (avg[j][z] + c1 * (avg[j - 1][z] + avg[j + 1][z] + avg[j][z + 1]) + c2 * (avg[j - 1][z + 1] + avg[j + 1][z + 1])) / (1 + 3 * c1 + 2 * c2);
                            }

                            else if (z == 400)
                            {
                                if (j == 0)
                                {
                                    avg[j][z] = (avg[j][z] + c1 * (avg[j][z - 1] + avg[j + 1][z]) + c2 * avg[j + 1][z - 1]) / (1 + 2 * c1 + c2);
                                }
                                else if (j == NScan)
                                {
                                    avg[j][z] = (avg[j][z] + c1 * (avg[j][z - 1] + avg[j - 1][z]) + c2 * avg[j - 1][z - 1]) / (1 + 2 * c1 + c2);
                                }
                                else
                                    avg[j][z] = (avg[j][z] + c1 * (avg[j - 1][z] + avg[j + 1][z] + avg[j][z - 1]) + c2 * (avg[j - 1][z - 1] + avg[j + 1][z - 1])) / (1 + 3 * c1 + 2 * c2);
                            }
                            else if (j == 0)
                            {
                                avg[j][z] = (avg[j][z] + c1 * (avg[j][z - 1] + avg[j + 1][z] + avg[j][z + 1]) + c2 * (avg[j + 1][z - 1] + avg[j + 1][z + 1])) / (1 + 3 * c1 + 2 * c2);
                            }
                            else if (j == NScan)
                            {
                                avg[j][z] = (avg[j][z] + c1 * (avg[j][z - 1] + avg[j - 1][z] + avg[j][z + 1]) + c2 * (avg[j - 1][z - 1] + avg[j - 1][z + 1])) / (1 + 3 * c1 + 2 * c2);
                            }
                            else
                            {
                                avg[j][z] = nr * (avg[j][z] + c1 * (avg[j][z - 1] + avg[j][z + 1] + avg[j - 1][z] + avg[j + 1][z])
                                            + c2 * (avg[j - 1][z - 1] + avg[j + 1][z + 1] + avg[j - 1][z + 1] + avg[j + 1][z - 1]));
                            }
                        }
                    }
                }

                n = 0;
                dblmax_min = (dbmax - dbmin) / 255.0;
                for (y = 0; y < 800; y++)
                {
                    for (x = 0; x < 800; x++)
                    {
                        d = avg[L[y][x]][D[y][x]];

                        //if (d>min &&  d<max)
                        if (d > dbmin && d < dbmax)
                        {
                            //ImaqBuffer[n] = ((d-min)/(max-min))*255;
                            ImaqBuffer[n] = (d - dbmin) / dblmax_min;
                        }
                        //else if  ( d<min )
                        else if (d < dbmin)
                            ImaqBuffer[n] = 0;
                        //else if  ( d>max && d<50 )
                        else if (d > dbmax && d < 50)
                            ImaqBuffer[n] = 254;
                        else
                            ImaqBuffer[n] = 255;
                        n++;
                    }
                }
            }

            else
            {
                n = 0;
                dblmax_min = (dbmax - dbmin) / 255.0;
                for (y = 0; y < 800; y++)
                {
                    for (x = 0; x < 800; x++)
                    {
                        d = ampplot_s_R90[L[y][x]][D[y][x]];
                        if (d > dbmin && d < dbmax)
                        {
                            //ImaqBuffer[n] = ((d-min)/(max-min))*255;
                            ImaqBuffer[n] = (d - dbmin) / dblmax_min;
                        }
                        //else if  ( d<min )
                        else if (d < dbmin)
                            ImaqBuffer[n] = 0;
                        //else if  ( d>max && d<50 )
                        else if (d > dbmax && d < 50)
                            ImaqBuffer[n] = 254;
                        else
                            ImaqBuffer[n] = 255;
                        n++;
                    }
                }
            }  
        } */

        /// <summary>
        /// ---------------------------------------------------------------
        ///  平移扫描线上的每一个点数据
        ///  ampplot_s			需要处理的数据
        ///	iScanLines			有多少条要处理的数据
        ///  iScanPoints			每一条扫描线有多少扫描点
        ///  ioffset				位移少个点
        ///---------------------------------------------------------------
        /// static void OffsetOCTIS_ScanLine(double ampplot_s[5100][1000], int iScanLines, int iScanPoints, int ioffset)
        /// </summary>         
        static void OffsetOCTIS_ScanLine(double[,] ampplot_s, int iScanLines, int iScanPoints, int ioffset)
        {
            int il, id, iss;

            if (ioffset > 0)
            {   // 扫描线上的点向后移
                for (il = 0; il < iScanLines; il++)
                {
                    for (iss = iScanPoints - ioffset - 1, id = iScanPoints - 1; iss >= 0; id--, iss--)
                        ampplot_s[il, id] = ampplot_s[il, iss];

                    for (iss = 0; iss < ioffset; iss++)
                        ampplot_s[il, iss] = 0.0;
                }
            }
            else
            {
                // 扫描线上的点向前移
                for (il = 0; il < iScanLines; il++)
                {
                    for (iss = 0 - ioffset, id = 0; iss < iScanPoints; id++, iss++)
                        ampplot_s[il, id] = ampplot_s[il, iss];

                    for (iss = iScanPoints + ioffset; iss < iScanPoints; iss++)
                        ampplot_s[il, iss] = 0.0;
                }
            }
        }

        /*
        public static void DrawPictOnMemo(double[,] PictData, ref Bitmap bmp)
        {
            //Stopwatch st = new Stopwatch();    // 开始计时
            //st.Start();


            //DrawPict_WhiteGray(PictData, ref bmp);
            PGrayToPseudoColor2(PictData, ref bmp, 1);
        }

        ///<summary>
        ///采用8bit灰度映射画图
        ///</summary>
        public static void DrawPict_8Bit(double[,] PictData, ref Bitmap bmp)
        {
            // 8bit索引方式位图，设置灰度调色板
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            bmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format8bppIndexed);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];
            //System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int i = 0; i < bmpData.Height; i++)
            {
                for (int j = 0; j < bmpData.Width; j ++)
                {
                    rgbValues[i * bmpData.Stride + j] = (byte)PictData[i, j]; ;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            ColorPalette tempPalette;
            using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                tempPalette = tempBmp.Palette;
            }
            Calculate_Palette_Ex(ref tempPalette, 2);
            bmp.Palette = tempPalette;


            //ColorPalette tempPalette;
            //{
            //    using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            //    {
            //        tempPalette = tempBmp.Palette;
            //    }
            //    for (int i = 0; i < 256; i++)
            //    {
            //        tempPalette.Entries[i] = Color.FromArgb(255, i, i, i);
            //    }
            //    bmp.Palette = tempPalette;
            //}
            bmp.UnlockBits(bmpData);
            //st.Stop();
        }
        //----------------------------------------------------------------------------
        //计算调色板，用于在显示区域显示OCT的采样数据对应的颜色
        //----------------------------------------------------------------------------
        public static void Calculate_Palette_Ex(ref ColorPalette tempPalette, int Mode)
        {
            int i;

            for (i = 0; i < 255; i++)
            {
                if (Mode == 1)
                {
                    //tempPalette.Entries[i] = Color.FromArgb(255, i, i, i);             // White
                    tempPalette.Entries[i] = Color.FromArgb(255, i, i, i);
                }
                else
                {
                    tempPalette.Entries[i] = Color.FromArgb(255, (int)( (Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14),(int)( 250 / (0.0005 * (i - 100) * (i - 100) + 1)), (int)(200 / (0.005 * (i - 40) * (i - 40) + 1)));
                }
            }
        }

        ///<summary>
        ///白灰度映射画图
        ///</summary>
        public static void DrawPict_WhiteGray(double[,] PictData, ref Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            bmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = bmpData.Stride * bmpData.Height;
            byte[] rgbValues = new byte[bytes];
            //System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            //double colorTemp = 0;
            for (int i = 0; i < bmpData.Height; i++)
            {
                for (int j = 0; j < bmpData.Width; j++) 
                {
                    //colorTemp = rgbValues[i * bmpData.Stride + j + 2] * 0.299 + rgbValues[i * bmpData.Stride + j + 1] * 0.578 + rgbValues[i * bmpData.Stride + j] * 0.114;
                    rgbValues[i * bmpData.Stride + j * 3] = (byte)((PictData[i, j]) * 2 / 255);
                    rgbValues[i * bmpData.Stride + j * 3 + 1] = (byte)(PictData[i, j] * 164 / 255);
                    rgbValues[i * bmpData.Stride + j * 3 + 2] = (byte)(PictData[i, j] * 252 / 255);
                    
                    // for White
                    //rgbValues[i * bmpData.Stride + j * 3] = (byte)(PictData[i, j] * 0.114);
                    //rgbValues[i * bmpData.Stride + j * 3 + 1] = (byte)(PictData[i, j] * 0.578);
                    //rgbValues[i * bmpData.Stride + j * 3 + 2] = (byte)(PictData[i, j] * 0.299);
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            //ColorPalette tempPalette;
            //{
            //    using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            //    {
            //        tempPalette = tempBmp.Palette;
            //    }
            //    for (int i = 0; i < 256; i++)
            //    {
            //        tempPalette.Entries[i] = Color.FromArgb(255, i, i, i);
            //    }
            //    bmp.Palette = tempPalette;
            //}
            bmp.UnlockBits(bmpData);
        }

        /// <summary>
        /// 灰度图转伪彩色图像函数（通过映射规则计算的方法）
        /// </summary>
        /// <param name="src">24位灰度图</param>
        /// <returns>返回构造的伪彩色图像</returns>
        public static Bitmap PGrayToPseudoColor1(Bitmap src)
        {
            try
            {
                Bitmap a = new Bitmap(src);

                Rectangle rect = new Rectangle(0, 0, a.Width, a.Height);
                System.Drawing.Imaging.BitmapData bmpData = a.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                int stride = bmpData.Stride;
                unsafe
                {
                    byte* pIn = (byte*)bmpData.Scan0.ToPointer();
                    byte* P;
                    int R, G, B;
                    int temp = 0;

                    for (int y = 0; y < a.Height; y++)
                    {
                        for (int x = 0; x < a.Width; x++)
                        {
                            P = pIn;
                            B = P[0];
                            G = P[1];
                            R = P[2];

                            temp = (byte)(B * 0.114 + G * 0.587 + R * 0.299);
                            if (temp >= 0 && temp <= 63)
                            {
                                P[2] = 0;
                                P[1] = (byte)(254 - 4 * temp);
                                P[0] = (byte)255;
                            }
                            if (temp >= 64 && temp <= 127)
                            {
                                P[2] = 0;
                                P[1] = (byte)(4 * temp - 254);
                                P[0] = (byte)(510 - 4 * temp);
                            }
                            if (temp >= 128 && temp <= 191)
                            {
                                P[2] = (byte)(4 * temp - 510);
                                P[1] = (byte)(255);
                                P[0] = (byte)0;
                            }
                            if (temp >= 192 && temp <= 255)
                            {
                                P[2] = (byte)255;
                                P[1] = (byte)(1022 - 4 * temp);
                                P[0] = (byte)0;
                            }
                            pIn += 3;
                        }
                        pIn += stride - a.Width * 3;
                    }
                }
                a.UnlockBits(bmpData);
                return a;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                return null;
            }
        }


        /// <summary>
        /// 灰度图转伪彩色图像函数（通过查表的方法）
        /// </summary>
        /// <param name="src"></param>
        /// <param name="type">转换类型（1.使用铁红  2.使用彩虹）</param>
        /// <returns></returns>
        public static void PGrayToPseudoColor2(double[,] PictData, ref Bitmap a, int type)
        {
            int y = 0;
            int x = 0;
            try
            {
                if (type == 1)
                {
                    a = new Bitmap(a.Width, a.Height, PixelFormat.Format24bppRgb); ;
                    Rectangle rect = new Rectangle(0, 0, a.Width, a.Height);
                    System.Drawing.Imaging.BitmapData bmpData = a.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    IntPtr ptr = bmpData.Scan0;
                    int bytes = bmpData.Stride * bmpData.Height;
                    byte[] rgbValues = new byte[bytes];

                    byte R, G, B;
                    int temp = 0;

                    for (y = 0; y < a.Height; y++)
                    {
                        for (x = 0; x < a.Width; x++)
                        {
                            temp = (int)(PictData[y, x] / 2);
                            R = ironTable[temp, 0];
                            G = ironTable[temp, 1];
                            B = ironTable[temp, 2];

                            rgbValues[y * bmpData.Stride + x * 3] = B;
                            rgbValues[y * bmpData.Stride + x * 3 + 1] = G;
                            rgbValues[y * bmpData.Stride + x * 3 + 2] = R;
                        }
                    }
                    System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
                    a.UnlockBits(bmpData);
                }
                else if (type == 2)
                {
                    a = new Bitmap(a.Width, a.Height, PixelFormat.Format24bppRgb); ;
                    Rectangle rect = new Rectangle(0, 0, a.Width, a.Height);
                    System.Drawing.Imaging.BitmapData bmpData = a.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    IntPtr ptr = bmpData.Scan0;
                    int bytes = bmpData.Stride * bmpData.Height;
                    byte[] rgbValues = new byte[bytes];

                    byte R, G, B;
                    int temp = 0;

                    for (y = 0; y < a.Height; y++)
                    {
                        for (x = 0; x < a.Width; x++)
                        {
                            temp = (int)(PictData[y, x] / 2);
                            R = rainTable[temp, 0];
                            G = rainTable[temp, 1];
                            B = rainTable[temp, 2];

                            rgbValues[y * bmpData.Stride + x * 3] = B;
                            rgbValues[y * bmpData.Stride + x * 3 + 1] = G;
                            rgbValues[y * bmpData.Stride + x * 3 + 2] = R;
                        }
                    }
                    System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
                    a.UnlockBits(bmpData);
                }
                else if (type == 3)
                {
                    a = new Bitmap(a.Width, a.Height, PixelFormat.Format24bppRgb); ;
                    Rectangle rect = new Rectangle(0, 0, a.Width, a.Height);
                    System.Drawing.Imaging.BitmapData bmpData = a.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    IntPtr ptr = bmpData.Scan0;
                    int bytes = bmpData.Stride * bmpData.Height;
                    byte[] rgbValues = new byte[bytes];

                    double[,] tempPalette = new double[256, 3];

                    //tempPalette.Entries[i] = Color.FromArgb(255, (int)((Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14), (int)(250 / (0.0005 * (i - 100) * (i - 100) + 1)), (int)(200 / (0.005 * (i - 40) * (i - 40) + 1)));
                    for (int i = 0; i < 255; i++)
                    {
                        tempPalette[i, 2] = (Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14;
                        tempPalette[i, 1] = 250 / (0.0005 * (i - 100) * (i - 100) + 1);
                        tempPalette[i, 0] = 200 / (0.005 * (i - 40) * (i - 40) + 1);
                    }

                    int colorTemp = 0;
                    for (int i = 0; i < bmpData.Height; i++)
                    {
                        for (int j = 0; j < bmpData.Width; j++)
                        {
                            //tempPalette.Entries[i] = Color.FromArgb(255, (int)((Math.Atan(0.1 * (i - 90)) + 3.14 / 2) * 255 / 3.14), (int)(250 / (0.0005 * (i - 100) * (i - 100) + 1)), (int)(200 / (0.005 * (i - 40) * (i - 40) + 1)));
                            //colorTemp = rgbValues[i * bmpData.Stride + j + 2] * 0.299 + rgbValues[i * bmpData.Stride + j + 1] * 0.578 + rgbValues[i * bmpData.Stride + j] * 0.114;
                            colorTemp = (int)PictData[i, j];
                            rgbValues[i * bmpData.Stride + j * 3] = (byte)tempPalette[colorTemp,0];
                            rgbValues[i * bmpData.Stride + j * 3 + 1] = (byte)tempPalette[colorTemp, 1];
                            rgbValues[i * bmpData.Stride + j * 3 + 2] = (byte)tempPalette[colorTemp, 2];
                        }
                    }
                    System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
                    a.UnlockBits(bmpData);
                }
                else
                {
                    throw new Exception("type 参数不合法！");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
        }

        */


        public void DrawCircle(int Radio, Color ThisColor)
        {
            Bitmap BM = DrawPict(Gen2.PictData, PictWidth, PictWidth, displaymode, brightness_inner, contract_inner);
            Graphics g = Graphics.FromImage(BM);

            float r = Radio * ( BM.Width - Gen2.m_DD )/ (2 * Gen2.m_NData) + Gen2.m_DD / 2;
            Rectangle NewBox = new Rectangle((int)(BM.Width / 2 - r), (int)(BM.Width / 2 - r), (int)(2 * r), (int)(2 * r));
            g.DrawEllipse(new Pen(ThisColor), NewBox);

            //pPB.Image = BM;
            IsShowPict = true;
            return;
        }

    }
}
