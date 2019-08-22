using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Data;

namespace OctTools
{
    public class mod_GG
    {
        public const int RAWHEADER_SZ_SIZE = 101;
        public const int RAWHEAD_SZNOTE_SIZE = 512;

        public struct ggHeader_struct 
        {
            public Int32 cbsize;                  // raw header size of bytes
            public string szCompanyName;          // company name
            public string szVersion;              // version
            public string szDate;                 // stamp date
            public string szTime;                 // stamp time
            public string szPatientid;            // patient id
            public string szLastname;
            public string szFirstname;
            public string szSex;
            public string szDateofBirth;
            public string szPhysican1;
            public string szPhysican2;
            public string szNote;

            public int video_on;                           // capture video is enable
            public int video_cx;
            public int video_cy;
            public int video_bytesPerRow;
            public int video_pixelDepth;
            public int video_ColorTables,
                       video_BmpBytes,
                       video_MasksBytes;

            public int numberof_frames;                    //number of frame
            public int elapseof_frame;                     //elaspe time of even frame

            public int DD;                                 //DD start pix for full range OCT display
            public int G;                                  //start pix for select range OCT display
            public int SN;                                 //Start Number for select range OCT display
            public int NumberOfline_360;                   //nuber of scan ling of a round 
            public float Angle2;

            public int numberofline;                       //number of scan lines
            public int numberofpointofline;                //number of point for ever scan line
            public uint offsetframe;                       // number of byte for first raw block
            public uint cbsize_frame;                      // frame size of bytes 
        }

        public struct ggFrame_struct
        {
            public Int32 cb_size;
            public uint rawdatasize;                       //OCT 原始数据的大小
            public uint rawdataoffset;                     //number of bytes for raw data offset
            public uint videoimagesize;                    //视频捕捉图像的数据大小
            public uint imageoffset;                       //number of bytes for video capture image data offset

            public double raw_maxvalue;                    //OCT raw 数据的最大值
            public double raw_minvalue;                    //OCT raw 数据的最小值

            public Int32 m_savetickcount;                  // OCT 数据保存的时间间隔   原定义DWORD
            public double m_saveposition;                  // OCT pull back 的位置 
        }

        public struct ggData_struct
        {
            public string m_szFileName;
            public ggHeader_struct m_ggheader;
            public ggFrame_struct m_ggframe_info;

            public uint m_currpos;                         // current read data position for second thread

            // oct raw data information
            public int elapseof_frame;                     //elaspe time of even frame
            public uint numberof_frames;

            public int numberofline;                       //number of scan lines
            public int numberofpointofline;                //number of point for ever scan line

            public int DD;                                 //DD start pix for full range OCT display
            public int G;                                  //start pix for select range OCT display
            public int SN;                                 //Start Number for select range OCT display
            public int NumberOfline_360;                   //nuber of scan ling of a round
            public float Angle2;

            // other image data information
            public int video_on;                           // capture video is enable
            public int video_cx;
            public int video_cy;
            public int video_bytesPerRow;
            public int video_pixelDepth;
            public int video_ColorTables,
                       video_BmpBytes,
                       video_MasksBytes;

            public uint m_sizeofrawdata;
            public uint m_sizeofimagedata;

            public byte[] m_lpframedata;               //whole frame and oct's data and image's data buffer
            public byte[] m_lpframeinfo;               // frame infomation from file
            public byte[] m_lprawdata;                 //temp raw data buf for second thread
            public byte[] m_lpimagedata;               //temp image data buf for second thread
        }

        public ggData_struct ggData;
        public double[,] ggRecordBeltData;
        public byte[] ggAllFrameInfo;
        public byte[] ggAllRawData;
        //public mod_ReadGen2.Gen2_Struct[] Gen2Array;
        public string[,] ggMark;
            
        public int iCurrentPos = 0;

        //BinaryReader m_hfile;                 // save file handle
        BinaryReader br = null;
        FileStream fs = null;

        // 3、一些参数
        const double PI = 3.1415926535897932384626433832795028841971;
        const double PI_45 = PI / 4;
        const double PI_90 = PI / 2;
        const double PI_180 = PI;                    // 180度的圆周值
        const double PI_360 = PI * 2;                // 360 度的圆周值
        const double PI_360_Div = PI * 2 / 360.0;    // 细分每一度的圆周值

        public mod_GG()
        {
            mod_GG_Init();
        }

        public void mod_GG_Init()
        {
            ggData = new ggData_struct();
            ggData.m_ggheader = new ggHeader_struct();
            ggData.m_ggframe_info = new ggFrame_struct();
            ggAllFrameInfo = null;
            ggAllRawData = null;
            ggRecordBeltData = null;
            ggMark = null;
            //Gen2Array = null;
            iCurrentPos = -1;
            br = null;
            fs = null;
        }

        public int ReadggHeader(string ggFilePath, ref string sErrString)
        {
            int iResult = MyData.iErr_UnknowErr;
            int iR = MyData.iErr_UnknowErr;
            int i = 0, j = 0;
            string sTmp = "";
            int DataLen = 0;
            float fData = 0.0f;

            if (ggFilePath == "")
            {
                sErrString = "没有文件名";
                return MyData.iErr_NoFile;
            }

            if (!File.Exists(ggFilePath))
            {
                sErrString = "文件不存在";
                return MyData.iErr_NoFile;
            }
            ggData.m_szFileName = ggFilePath;

            try
            {
                iResult = MyData.iErr_Succ;
                sErrString = "";

                fs = new FileStream(ggData.m_szFileName, FileMode.Open, FileAccess.Read);
                //sr = new StreamReader(fs);
                br = new BinaryReader(fs);

                ggData.m_ggheader.cbsize = br.ReadInt32();

                for (i = 0; i < 12; i++)
                {
                    if (i < 11)
                        DataLen = RAWHEADER_SZ_SIZE;
                    else
                        DataLen = RAWHEAD_SZNOTE_SIZE;

                    if (ReadFileString(br, DataLen, ref sTmp, ref sErrString) == MyData.iErr_Succ)
                    {
                        switch (i)
                        {
                            case 0:
                                ggData.m_ggheader.szCompanyName = sTmp;          // company name
                                break;
                            case 1:
                                ggData.m_ggheader.szVersion = sTmp;              // version
                                break;
                            case 2:
                                ggData.m_ggheader.szDate = sTmp;                 // stamp date
                                break;
                            case 3:
                                ggData.m_ggheader.szTime = sTmp;                 // stamp time
                                break;
                            case 4:
                                ggData.m_ggheader.szPatientid = sTmp;                // patient id
                                break;
                            case 5:
                                ggData.m_ggheader.szLastname = sTmp;
                                break;
                            case 6:
                                ggData.m_ggheader.szFirstname = sTmp;
                                break;
                            case 7:
                                ggData.m_ggheader.szSex = sTmp;
                                break;
                            case 8:
                                if (sTmp.Trim() == "")
                                {
                                    ggData.m_ggheader.szDateofBirth = "1900-01-01 12:00:00";
                                }
                                else
                                {
                                    ggData.m_ggheader.szDateofBirth = sTmp.Substring(6, 4) + "-" + sTmp.Substring(0, 5);
                                }
                                break;
                            case 9:
                                ggData.m_ggheader.szPhysican1 = sTmp;
                                break;
                            case 10:
                                ggData.m_ggheader.szPhysican2 = sTmp;
                                break;
                            case 11:
                                ggData.m_ggheader.szNote = sTmp;
                                break;
                        }
                    }
                    else
                    {
                        goto Eend;
                    }
                }

                // 跳过对齐字节
                br.BaseStream.Seek(1, SeekOrigin.Current);

                for (i = 0; i < 19; i++)
                {
                    if (i != 14)
                        iR = ReadFileInt(br, ref j, ref sErrString);
                    else
                        iR = ReadFileFloat(br, ref fData, ref sErrString);

                    if (iR == MyData.iErr_Succ)
                    {
                        switch (i)
                        {
                            case 0:
                                ggData.m_ggheader.video_on = j;          // company name
                                break;
                            case 1:
                                ggData.m_ggheader.video_cx = j;              // version
                                break;
                            case 2:
                                ggData.m_ggheader.video_cy = j;                 // stamp date
                                break;
                            case 3:
                                ggData.m_ggheader.video_bytesPerRow = j;                 // stamp time
                                break;
                            case 4:
                                ggData.m_ggheader.video_pixelDepth = j;                // patient id
                                break;
                            case 5:
                                ggData.m_ggheader.video_ColorTables = j;
                                break;
                            case 6:
                                ggData.m_ggheader.video_BmpBytes = j;
                                break;
                            case 7:
                                ggData.m_ggheader.video_MasksBytes = j;
                                break;
                            case 8:
                                ggData.m_ggheader.numberof_frames = j;
                                break;
                            case 9:
                                ggData.m_ggheader.elapseof_frame = j;
                                break;
                            case 10:
                                ggData.m_ggheader.DD = j;
                                break;
                            case 11:
                                ggData.m_ggheader.G = j;
                                break;
                            case 12:
                                ggData.m_ggheader.SN = j;
                                break;
                            case 13:
                                ggData.m_ggheader.NumberOfline_360 = j;
                                break;
                            case 14:
                                ggData.m_ggheader.Angle2 = fData;
                                break;
                            case 15:
                                ggData.m_ggheader.numberofline = j;
                                break;
                            case 16:
                                ggData.m_ggheader.numberofpointofline = j;
                                break;
                            case 17:
                                ggData.m_ggheader.offsetframe = (uint)j;
                                break;
                            case 18:
                                ggData.m_ggheader.cbsize_frame = (uint)j;
                                break;
                        }
                    }
                    else
                    {
                        goto Eend;
                    }
                }   // 至此应该到达：读完ThisggHeader.cbsize长度的位置

                ggData.m_currpos = 0;                  // current write data position for second thread

                // oct raw data information
                ggData.elapseof_frame = ggData.m_ggheader.elapseof_frame;                        //elaspe time of even frame
                ggData.numberof_frames = (uint)ggData.m_ggheader.numberof_frames;                // number of frames in the serial file

                ggData.numberofline = ggData.m_ggheader.numberofline;                            //number of scan lines
                                                                                                 //lpdata->numberofpointofline = lpdata->m_rawheader.numberofpointofline;				
                                                                                                 //number of point for ever scan line
                ggData.numberofpointofline = ggData.m_ggheader.numberofpointofline;              //number of point for ever scan line

                ggData.DD = ggData.m_ggheader.DD;                                                //DD start pix for full range OCT display
                ggData.G = ggData.m_ggheader.G;                                                  //start pix for select range OCT display
                ggData.SN = ggData.m_ggheader.SN;                                                //Start Number for select range OCT display
                ggData.NumberOfline_360 = ggData.m_ggheader.NumberOfline_360;                    //nuber of scan ling of a round
                ggData.Angle2 = ggData.m_ggheader.Angle2;

                // other image data information
                ggData.video_on = ggData.m_ggheader.video_on;                                    // capture video is enable
                ggData.video_cx = ggData.m_ggheader.video_cx;
                ggData.video_cy = ggData.m_ggheader.video_cy;
                ggData.video_bytesPerRow = ggData.m_ggheader.video_bytesPerRow;
                ggData.video_pixelDepth = ggData.m_ggheader.video_pixelDepth;
                ggData.video_ColorTables = ggData.m_ggheader.video_ColorTables;
                ggData.video_BmpBytes = ggData.m_ggheader.video_BmpBytes;
                ggData.video_MasksBytes = ggData.m_ggheader.video_MasksBytes;

                //lpdata->m_lpframedata = malloc(lpdata->m_rawheader.cbsize_frame);
                //if (lpdata->m_lpframedata)
                //{
                //    blResult = ReadRawFrameDataFromFile_FirstFrame(lpdata);
                //    //if(SetFilePointer(lpdata->m_hfile, (LONG)lpdata->m_rawheader.offsetframe, NULL, FILE_BEGIN) == lpdata->m_rawheader.offsetframe)
                //    //{
                //    // read first frame
                //    //	blResult = ReadRawFrameDataFromFile(lpdata);
                //    //}else
                //    //	blResult = FALSE;
                //}
                //else
                //blResult = FALSE;
                iResult = MyData.iErr_Succ;
            }
            catch (Exception ex)
            {
                sErrString = "读gg文件出错（" + ex.Message + "） j = " + j.ToString() + ",i = " + i.ToString();
                iResult = MyData.iErr_Exception;
            }

            Eend:
            return iResult;
        }

        public void ggClose()
        {
            if (br != null)
            {
                br.Close();
            }
            if (fs != null)
            {
                fs.Close();
            }
            ggData.m_ggheader = new ggHeader_struct();
            ggData.m_ggframe_info = new ggFrame_struct();
            ggData = new ggData_struct();
        }

        public void ggFileClose()
        {
            if (br != null)
            {
                br.Close();
            }
            if (fs != null)
            {
                fs.Close();
            }
        }

        // ------------------------------------------------------------------------------------------
        // 从已经打开的序列文件中读出一个帧的数据
        // ------------------------------------------------------------------------------------------
        public bool ReadRawFrameDataFromFile(ref ggFrame_struct FrameInfoBuff, ref byte[] FrameRawBuff, ref byte[] FrameImageBuff, ref string sErrString)
        {
            uint dwlen, dwsize, dwcpsize;
            byte[] Datatmp;
            IntPtr buffer = Marshal.AllocHGlobal(1); 

            bool bResult = false;
            try
            {
                dwlen = ggData.m_ggheader.cbsize_frame;
                Datatmp = br.ReadBytes((int)dwlen);
                if (Datatmp.Length == dwlen)
                {
                    dwcpsize = (uint)Datatmp.Length;
                    dwcpsize = dwcpsize < dwlen ? dwcpsize : dwlen;

                    //lpdata->m_lpframeinfo = (LPRAWFRAME_INFO)lpdata->m_lpframedata;
                    //CopyMemory(lpframeinfo, lpdata->m_lpframedata, sizeof(RAWFRAME_INFO));
                    int size = Marshal.SizeOf(typeof(ggFrame_struct));

                    Marshal.FreeHGlobal(buffer);                          // 释放以前分配的空间
                    buffer = Marshal.AllocHGlobal(size);        // 重新分配

                    Marshal.Copy(Datatmp, 0, buffer, size);
                    FrameInfoBuff = (ggFrame_struct)Marshal.PtrToStructure(buffer, typeof(ggFrame_struct));

                    if (FrameInfoBuff.cb_size >= size)
                    {
                        FrameRawBuff = new byte[FrameInfoBuff.rawdatasize];
                        Array.Copy(Datatmp, FrameInfoBuff.rawdataoffset, FrameRawBuff, 0, FrameInfoBuff.rawdatasize);

                        //FrameImageBuff = new byte[FrameInfoBuff.videoimagesize];
                        //Array.Copy(Datatmp, FrameInfoBuff.imageoffset, FrameImageBuff, 0, FrameInfoBuff.videoimagesize);
                    }
                    else
                    {
                        FrameImageBuff = null;
                    }
                    sErrString = "成功";
                    bResult = true;
                }
                else
                {
                    sErrString = "数据帧长度错误";
                    bResult = false;
                }
            }
            catch (Exception ex)
            {
                sErrString = "读帧数据错误（" + ex.Message + "）";
                bResult = false;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return bResult;
        }

        public bool ReadAllFramDataFromFile(ref byte[] AllFrameInfo, ref byte[] AllRaw, ref double[,] BeltData, double Angle0, UserControl_ReadFileBar RBar, ref string sErrString)
        {
            bool bResult = false;
            sErrString = "";
            double dBarStep = RBar.iBarValue;

            if (ggData.m_szFileName == "")   // 还没关联gg文件
            {
                sErrString = "尚未打开相关文件。";
                return bResult;
            }

            if (ggData.numberof_frames <= 0)
            {
                AllRaw = null;         // 没有数据
                BeltData = null;
                return bResult;
            }
            
            dBarStep += 5.0;
            RBar.iBarValue = (int)(dBarStep);
            MyTools.DoEvents();

            double Angle = 2 * PI_360 * Angle0 / 360;
            BeltData = null;
            IntPtr buffer = Marshal.AllocHGlobal(1);
            try
            {
                int iFrameInfoSize = Marshal.SizeOf(typeof(ggFrame_struct));

                AllFrameInfo = new byte[ggData.numberof_frames * iFrameInfoSize];
                mod_ReadGen2.Gen2_Struct Gen2Current = new mod_ReadGen2.Gen2_Struct();
                //Gen2Array = new mod_ReadGen2.Gen2_Struct[ggData.numberof_frames];
                mod_ShowPict PB = new mod_ShowPict();

                byte[] FirstLine = new byte[ggData.numberof_frames * ggData.numberofpointofline * 2];
                byte[] SecondLine = new byte[ggData.numberof_frames * ggData.numberofpointofline * 2];

                int iFirstLine = 0, iSeconLine = 0;   // 取出第几线
                iFirstLine = (int)((ggData.numberofline / (PI_360 * 2)) * (Angle + ggData.Angle2));
                if (iFirstLine > ggData.numberofline)
                    iFirstLine -= ggData.numberofline;

                iSeconLine = iFirstLine + ggData.numberofline / 2;
                if (iSeconLine > ggData.numberofline)
                    iSeconLine -= ggData.numberofline;

                for (int i = 0; i < ggData.numberof_frames; i++)
                {
                    if (ggFileMoveToFrame("Goto", i, ref sErrString))
                    {
                        // 计算从第几帧取数据

                        if (ReadRawFrameDataFromFile(ref ggData.m_ggframe_info, ref ggData.m_lprawdata, ref ggData.m_lpimagedata, ref sErrString))
                        {
                            if (i == 0)
                            {
                                ggData.m_sizeofrawdata = ggData.m_ggframe_info.rawdatasize;
                                AllRaw = new byte[ggData.numberof_frames * ggData.m_sizeofrawdata];
                            }

                            Marshal.FreeHGlobal(buffer);                          // 释放以前分配的空间
                            buffer = Marshal.AllocHGlobal(iFrameInfoSize);        // 重新分配
                            Marshal.StructureToPtr((ggFrame_struct)ggData.m_ggframe_info, buffer, false);
                            Marshal.Copy(buffer, AllFrameInfo, i * iFrameInfoSize, iFrameInfoSize);

                            Array.Copy(ggData.m_lprawdata, 0, AllRaw, i * ggData.m_sizeofrawdata, ggData.m_sizeofrawdata);
                            //Gen2Current = new mod_ReadGen2.Gen2_Struct();
                            //if (mod_ReadGen2.ReadGen2FromGGClass(this, ref Gen2Current, ref sErrString) != MyData.iErr_Succ)
                            //{
                            //    MyTools.ShowMsg("读取数据失败!", "从gg类读数据失败 = " + sErrString);
                            //    goto Eend;
                            //}
                            //PB.pGen2 = Gen2Current;
                            //PB.Gen2ToData();                        // 转变成 PictData
                            //Gen2Array[i] = PB.pGen2;

                            Array.Copy(ggData.m_lprawdata, iFirstLine * ggData.numberofpointofline * 2, FirstLine, i * ggData.numberofpointofline * 2, ggData.numberofpointofline * 2);       // To First Line
                            Array.Copy(ggData.m_lprawdata, iSeconLine * ggData.numberofpointofline * 2, SecondLine, i * ggData.numberofpointofline * 2, ggData.numberofpointofline * 2);       // To First Line

                            dBarStep += (double)(85.0 / ggData.numberof_frames);
                            RBar.iBarValue = (int)(dBarStep);
                            MyTools.DoEvents();
                        }
                        else
                        {
                            sErrString = "移动到第" + i.ToString() + "帧时出错";
                            goto Eend;
                        }
                    }
                    else
                    {
                        sErrString = "移动到第" + i.ToString() + "帧时出错";
                        goto Eend;
                    }
                }

                BeltData = new double[ggData.numberof_frames * 2, ggData.numberofpointofline];
                ushort uSi = 0;
                ushort uMax = 0, uMin = 0;

                for (int i = 0; i < ggData.numberof_frames; i++)
                {
                    for (int j = 0; j < ggData.numberofpointofline; j++)
                    {
                        uSi = BitConverter.ToUInt16(FirstLine, i * ggData.numberofpointofline * 2 + j * 2);
                        BeltData[i, j] = (double)uSi;
                        if (uMin > uSi)
                            uMin = uSi;

                        if (uMax < uSi)
                            uMax = uSi;

                        uSi = BitConverter.ToUInt16(SecondLine, i * ggData.numberofpointofline * 2 + j * 2);
                        BeltData[ggData.numberof_frames + i, j] = (double)uSi;
                        if (uMin > uSi)
                            uMin = uSi;

                        if (uMax < uSi)
                            uMax = uSi;
                    }
                }

                for (int i = 0; i < ggData.numberof_frames; i++)
                {
                    for (int j = 0; j < ggData.numberofpointofline; j++)
                    {
                        //BeltData[i, j] = (BeltData[i, j] / 65535.0) * (uMax - uMin) + uMin;
                        //BeltData[ggData.numberof_frames + i, j] = (BeltData[ggData.numberof_frames + i, j] / 65535.0) * (uMax - uMin) + uMin;

                        BeltData[i, j] = BeltData[i, j] * 255 / 65535.0;
                        BeltData[ggData.numberof_frames + i, j] = BeltData[ggData.numberof_frames + i, j] * 255 / 65535.0;
                    }
                }

                bResult = true;
                sErrString = "";
            }
            catch (OutOfMemoryException)
            {
                sErrString = "内存不足";
            }
            catch (Exception ex)
            {
                sErrString = ex.Message;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            Eend:
            return bResult;

        }

        public bool ReadFramDataFromMemo(int iFrameID, ref ggFrame_struct FrameInfoBuff, ref byte[] FrameRawBuff, ref byte[] FrameImageBuff, ref string sErrString)
        {
            bool bResult = false;
            IntPtr buffer = Marshal.AllocHGlobal(1);

            if (ggData.m_szFileName == "")   // 还没关联gg文件
            {
                sErrString = "尚未打开相关文件。";
                return bResult;
            }

            if (ggData.numberof_frames <= 0)
            {
                FrameInfoBuff = new ggFrame_struct();         // 没有数据
                FrameRawBuff = null;
                return bResult;
            }

            try
            {
                int size = Marshal.SizeOf(typeof(ggFrame_struct));
                Marshal.FreeHGlobal(buffer);                          // 释放以前分配的空间
                buffer = Marshal.AllocHGlobal(size);                  // 重新分配
                Marshal.Copy(ggAllFrameInfo, iFrameID * size, buffer, size);
                FrameInfoBuff = (ggFrame_struct)Marshal.PtrToStructure(buffer, typeof(ggFrame_struct));

                Array.Copy(ggAllRawData, iFrameID * ggData.m_sizeofrawdata, FrameRawBuff, 0, ggData.m_sizeofrawdata);
                bResult = true;
            }
            catch (Exception ex)
            {
                sErrString = "读取记录数据失败: " + ex.Message;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return bResult;
        }

        public bool ReadRecordBeltData(byte[] AllRaw, ref double[,] BeltData, double Angle0, ref string sErrString)
        {
            bool bResult = false;
            sErrString = "";
            BeltData = null;

            if (AllRaw == null )   // 还没关联gg文件
            {
                sErrString = "尚未打开相关文件。";
                return bResult;
            }

            if (ggData.numberof_frames <= 0)
            {
                return bResult;
            }

            double Angle = Angle0;
            try
            {
                byte[] FirstLine = new byte[ggData.numberof_frames * ggData.numberofpointofline * 2];
                byte[] SecondLine = new byte[ggData.numberof_frames * ggData.numberofpointofline * 2];

                int iFirstLine = 0, iSeconLine = 0;   // 取出第几线
                iFirstLine = (int)( (ggData.numberofline /PI_360) * (Angle + ggData.Angle2) );
                if (iFirstLine > ggData.numberofline)
                    iFirstLine -= ggData.numberofline;

                iSeconLine = iFirstLine + ggData.numberofline / 2;
                if (iSeconLine > ggData.numberofline)
                    iSeconLine -= ggData.numberofline;

                int iL = 0;
                for (int i = 0; i < ggData.numberof_frames; i++)
                {
                    // 计算从第几帧取数据
                    if (i == ggData.numberof_frames - 2)
                        iL = 1;
                    Array.Copy(AllRaw, i * ggData.m_sizeofrawdata + iFirstLine * ggData.numberofpointofline * 2, FirstLine, i * ggData.numberofpointofline * 2, ggData.numberofpointofline * 2);       // To First Line
                    Array.Copy(AllRaw, i * ggData.m_sizeofrawdata + iSeconLine * ggData.numberofpointofline * 2, SecondLine, i * ggData.numberofpointofline * 2, ggData.numberofpointofline * 2);       // To First Line
                }

                BeltData = new double[ggData.numberof_frames * 2, ggData.numberofpointofline];
                ushort uSi = 0;
                ushort uMax = 0, uMin = 0;

                for (int i = 0; i < ggData.numberof_frames; i++)
                {
                    for (int j = 0; j < ggData.numberofpointofline; j++)
                    {
                        uSi = BitConverter.ToUInt16(FirstLine, i * ggData.numberofpointofline * 2 + j * 2);
                        BeltData[i, j] = (double)uSi;
                        if (uMin > uSi)
                            uMin = uSi;

                        if (uMax < uSi)
                            uMax = uSi;

                        uSi = BitConverter.ToUInt16(SecondLine, i * ggData.numberofpointofline * 2 + j * 2);
                        BeltData[ggData.numberof_frames + i, j] = (double)uSi;
                        if (uMin > uSi)
                            uMin = uSi;

                        if (uMax < uSi)
                            uMax = uSi;
                    }
                }

                for (int i = 0; i < ggData.numberof_frames; i++)
                {
                    for (int j = 0; j < ggData.numberofpointofline; j++)
                    {
                        //BeltData[i, j] = (BeltData[i, j] / 65535.0) * (uMax - uMin) + uMin;
                        //BeltData[ggData.numberof_frames + i, j] = (BeltData[ggData.numberof_frames + i, j] / 65535.0) * (uMax - uMin) + uMin;

                        BeltData[i, j] = BeltData[i, j] * 255 / 65535.0;
                        BeltData[ggData.numberof_frames + i, j] = BeltData[ggData.numberof_frames + i, j] * 255 / 65535.0;
                    }
                }

                bResult = true;
                sErrString = "";
            }
            catch (Exception ex)
            {
                sErrString = "读取记录带数据失败: " + ex.Message;
            }

            Eend:
            return bResult;
        }

        public bool ReadMarkFromDB(int iFileID, int iRecordID, ref string[,] Marks, ref string sErrString)
        {
            bool bResult = false;
            string sSql = "";
            DataSet DS = null;
            sErrString = "";

            Marks = new string[ggData.numberof_frames, 4];
            for (int i = 0; i < ggData.numberof_frames; i++)
            {
                Marks[i, 0] = "False";            // 有无
                Marks[i, 1] = i.ToString();       // FrameID
                Marks[i, 2] = "-1";               // MarkID
                Marks[i, 3] = "";                 // MarkInfo
            }
            try
            {

                sSql = "Select * From Mark Where FileID = " + iFileID.ToString() + " AND RecordID = " + iRecordID;
                if (!MyData.MySqlite.ReadData(sSql, ref DS, ref sErrString))
                {
                    goto Eend;
                }

                if (DS != null && DS.Tables[0].Rows.Count > 0)
                {
                    int iFrameID = 0;
                    for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                    {
                        iFrameID = int.Parse(DS.Tables[0].Rows[i]["FRAMEID"].ToString());
                        Marks[iFrameID, 0] = "True";
                        Marks[iFrameID, 1] = iFrameID.ToString();
                        Marks[iFrameID, 2] = DS.Tables[0].Rows[i]["MARKID"].ToString();
                        Marks[iFrameID, 3] = DS.Tables[0].Rows[i]["MARK"].ToString();
                    }
                }
                sErrString = "";
                bResult = true;
            }
            catch (Exception ex)
            {
                sErrString = ex.Message;
            }

            Eend:
            if (DS != null)
                DS = null;

            return bResult;
        }

        /// <summary>
        ///   从已经打开的序列文件中，移动到指定的帧的起始位置 --------------------------------------
        ///   iFrameID = 指定的帧编号
        ///   MoveMode：移动方式， Top，Bottom， Last，Next，Goto
        ///   iFrameID：当MoveMode == Goto时，   iFrameID >= 0,  等于具体那一帧=0--Count-1
        ///             当MoveMode == 其他值时， iFrameID没有意义
        /// ------------------------------------------------------------------------------------------
        /// </summary>
        public bool ggFileMoveToFrame(string MoveMode, int iFrameID, ref string sErrString)
        {
            int dwlen = 0;
            int iMoveFramNum = 0;
            uint iCurrentFramID = ggData.m_currpos;

            long dwcode;
            long lpos;
            bool bResult = false;

            sErrString = "";
            if (MoveMode != "Top" && MoveMode != "Bottom" && MoveMode != "Last" && MoveMode != "Next" && MoveMode != "Goto")
            {
                sErrString = "参数错误（ggFileMoveToFrame-->MoveMode = '" + MoveMode + "')";
                return false;
            }
            try
            {
                if (MoveMode == "Top")
                {
                    iMoveFramNum = 0;
                }
                else if (MoveMode == "Bottom")
                {
                    iMoveFramNum = ggData.m_ggheader.numberof_frames - 1;
                }
                else if (MoveMode == "Last")
                {
                    if (ggData.m_currpos > 0)
                        iMoveFramNum = (int)ggData.m_currpos - 1;
                    else
                    {
                        sErrString = "已经移动到第一帧。";
                        return false;
                    }
                }
                else if (MoveMode == "Next")
                {
                    if (ggData.m_currpos < (int)ggData.m_currpos - 1)
                        iMoveFramNum = (int)ggData.m_currpos + 1;
                    else
                    {
                        sErrString = "已经移动到最后帧。";
                        return false;
                    }
                }
                else if (MoveMode == "Goto")
                {
                    if (iFrameID < 0)
                    {
                        sErrString = "移动帧数不能为负数。";
                        return false;
                    }
                    else if (iFrameID > (ggData.m_ggheader.numberof_frames -1 ) )
                    {
                        sErrString = "移动帧数不能超过总帧数。";
                        return false;
                    }
                    iMoveFramNum = iFrameID;
                }
                // else  只能有上面5种情况

                // 将文件的读写位置移动到 “第一帧 + 指定帧” 的开始位置
                dwlen = (int)ggData.m_ggheader.cbsize_frame;           //一帧的数据的数据长度
                lpos = ggData.m_ggheader.offsetframe;                  // 第一帧其实位置
                lpos += dwlen * iMoveFramNum;                          // 移动多少个帧，实际需要移动多少个数据长度
                dwcode = br.BaseStream.Seek(lpos, SeekOrigin.Begin);
                if (dwcode != lpos)
                {
                    sErrString = "图像不能移动到指定位置。";
                    bResult = false;
                }
                else
                {
                    ggData.m_currpos = (uint)iMoveFramNum;
                    bResult = true;
                }
            }
            catch (Exception ex)
            {
                sErrString = "移动帧数出现意外错误（" + ex.Message + "）";
                bResult = false;
            }
            return bResult;
        }

        static int ReadFileString(BinaryReader br, int DataLen, ref string ItemString, ref string ErrString)
        {
            byte[] FileData = new byte[DataLen];
            int iResult = MyData.iErr_UnknowErr;
            ItemString = "";

            try
            {
                FileData = br.ReadBytes(DataLen);
                ItemString = System.Text.Encoding.Default.GetString(FileData, 0, DataLen).TrimEnd('\0');
                iResult = MyData.iErr_Succ;
            }
            catch (Exception ex)
            {
                ErrString = "Read string from gg exception = " + ex.Message;
                iResult = MyData.iErr_Exception;
            }

            return iResult;
        }

        static int ReadFileInt(BinaryReader br, ref int ItemInt, ref string ErrString)
        {
            int iFileData = 0;
            int iResult = MyData.iErr_UnknowErr;

            try
            {
                iFileData = br.ReadInt32();
                ItemInt = iFileData;
                iResult = MyData.iErr_Succ;
            }
            catch (Exception ex)
            {
                ErrString = "Read int from gg exception = " + ex.Message;
                iResult = MyData.iErr_Exception;
            }

            return iResult;
        }

        static int ReadFileFloat(BinaryReader br, ref float ItemFloat, ref string ErrString)
        {
            float fFileData = 0;
            int iResult = MyData.iErr_UnknowErr;

            try
            {
                fFileData = br.ReadSingle();
                ItemFloat = fFileData;
                iResult = MyData.iErr_Succ;
            }
            catch (Exception ex)
            {
                ErrString = "Read int from gg exception = " + ex.Message;
                iResult = MyData.iErr_Exception;
            }

            return iResult;
        }
    }
}
