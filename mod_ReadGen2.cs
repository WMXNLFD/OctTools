using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Data;


namespace OctTools
{
    public static class mod_ReadGen2
    {
        public struct Gen2_Struct
        {
            public string m_FileName;
            public int m_cbsize;
            public string m_lastname;
            public string m_firstname;
            public string m_patientid;
            public string m_sex;
            public string m_dateofbirth;
            public string m_physican1;
            public string m_physican2;
            public string m_image_datatime;
            public string m_Procedures;
            public string m_Note;

            public int m_NScanSave;                    // 有多少条扫描线数据被记录
            public int m_Angle2;
            public int m_Number_line_360;              // 每一个截面有多少扫描线组成
            public int m_SN;
            public int m_DD;
            public int m_G;
            public int m_Marker;
            public int m_NData;                        // 每一扫描上有效数据个数
            public double m_minValue;
            public double m_maxValue;
            public string m_CheckNote;

            public Int32[,] ampplot_sI;                // 维数 = m_NData * m_NScanSave
            public double[,] ampplot_s;                // 维数 = m_NData * m_NScanSave
            public double[,] PictData;

            // 补充
            public int m_FileID;
            public string m_Address;
            public string m_Tele;
            public string m_IdentifyID;
        }

        public const int iErr_Succ = 1;
        public const int iErr_UnknowErr = -1;
        public const int iErr_NoInit = -10;
        public const int iErr_NoFile = -11;
        public const int iErr_ValidData = -12;
        public const int iErr_Exception = -20;
        public const int iErr_Nofind = -21;

        public static int ReadGen2FromFile(string sFileName, ref Gen2_Struct ThisGen1, ref string sErrString)
        {
            Gen2_Struct ThisGen2 = new Gen2_Struct();

            //StreamReader sr = null;
            BinaryReader br = null;
            FileStream fs = null;
            //FileStream ws = null;
            //StreamWriter sw = null;

            int len_lastname = 0;
            int len_firstname = 0;
            int len_patientid = 0;
            int len_sex = 0;
            int len_dateofbirth = 0;
            int len_physican1 = 0;
            int len_physican2 = 0;
            int len_image_datatime = 0;
            int len_Procedures = 0;
            int len_Note = 0;

            int iTmp = 0;
            double dTmp = 0;

            int SumData1 = 0;
            int iResult = iErr_UnknowErr;
            int i = 0, j = 0;

            if (sFileName == "")
            {
                sErrString = "没有文件名";
                return iErr_NoFile;
            }

            if (!File.Exists(sFileName))
            {
                sErrString = "文件不存在";
                return iErr_NoFile;
            }

            try
            {
                ThisGen2.m_FileName = sFileName;
                fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read);
                //sr = new StreamReader(fs);
                br = new BinaryReader(fs);

                string sTmp = "";
                string[] sData;
                for (i = 0; i < 10; i++)
                {
                    //sTmp = sr.ReadLine();
                    sTmp = MyTools.ReadFileLine(br);
                    sData = sTmp.Split(':');
                    sData[0] = sData[0].ToUpper().Trim();
                    if (sData[0] == "LAST NAME")
                    {
                        len_lastname = MyTools.GetStringCode(sData[1]);
                        ThisGen2.m_lastname = sData[1].Trim();
                    }
                    else if (sData[0] == "FIRST NAME")
                    {
                        len_firstname = MyTools.GetStringCode(sData[1]);
                        ThisGen2.m_firstname = sData[1].Trim();
                    }
                    else if (sData[0] == "PATIENT ID")
                    {
                        len_patientid = MyTools.GetStringCode(sData[1]);
                        ThisGen2.m_patientid = sData[1].Trim();
                    }
                    else if (sData[0] == "SEX")
                    {
                        len_sex = MyTools.GetStringCode(sData[1]);
                        ThisGen2.m_sex = sData[1].Trim();
                    }
                    else if (sData[0] == "DATE OF BIRTH")
                    {
                        len_dateofbirth = MyTools.GetStringCode(sData[1]);
                        sTmp = sData[1].Trim();
                        ThisGen2.m_dateofbirth = sTmp.Substring(6, 4) + "-" + sTmp.Substring(0, 2) + "-" + sTmp.Substring(3, 2) + " 00:00:00";
                    }
                    else if (sData[0] == "PHYSICAN1")
                    {
                        len_physican1 = MyTools.GetStringCode(sData[1]);
                        ThisGen2.m_physican1 = sData[1].Trim();
                    }
                    else if (sData[0] == "PHYSICAN2")
                    {
                        len_physican2 = MyTools.GetStringCode(sData[1]);
                        ThisGen2.m_physican2 = sData[1].Trim();
                    }
                    else if (sData[0] == "IMAGE DATE AND TIME")
                    {
                        len_image_datatime = MyTools.GetStringCode(sData[1]);

                        // 可能有多种格式：Image Date and Time:20170210-1154-41
                        bool DateIsOK = false;
                        DateTime dt = DateTime.Now;
                        sTmp = sData[1].Trim();

                        while (!DateIsOK)
                        {
                            for (int m = 0; m < 2; m++)
                            {
                                try
                                {
                                    switch (m)
                                    {
                                        case 0:
                                            dt = DateTime.Parse(sTmp);
                                            DateIsOK = true;
                                            break;
                                        case 1:
                                            dt = DateTime.Parse(sTmp.Substring(0, 4) + "-" + sTmp.Substring(4, 2) + "-" + sTmp.Substring(6, 2) + " " +
                                                                        sTmp.Substring(9, 2) + ":" + sTmp.Substring(11, 2) + ":" + sTmp.Substring(14, 2));
                                            DateIsOK = true;
                                            break;
                                    }
                                }
                                catch
                                {
                                }

                                if (DateIsOK)
                                    break;
                            }

                            if (DateIsOK)
                            {
                                break;
                            }
                            else
                            {
                                string sEditTopic = "注意：检查记录的'检查时间'不是标准的时间格式，请按照'yyyy-mm-dd hh:mm:ss'更正。";
                                string sEditFiled = "检查时间";
                                string sEditText = sTmp;
                                bool bEditOK = false;

                                frm_EditDateTime Frm_EditDateTime = new frm_EditDateTime();
                                Frm_EditDateTime.sEditTopic = sEditTopic;
                                Frm_EditDateTime.sEditFiled = sEditFiled;
                                Frm_EditDateTime.sEditText = sEditText;
                                Frm_EditDateTime.bEditOK = bEditOK;
                                Frm_EditDateTime.ShowDialog();

                                sEditText = Frm_EditDateTime.sEditText; 
                                bEditOK = Frm_EditDateTime.bEditOK;
                                Frm_EditDateTime.Close();

                                if (bEditOK)     // 修改过了
                                {
                                    sTmp = sEditText;
                                }
                                else            // 不要修改
                                {
                                    break;
                                }
                            }
                        }

                        if (!DateIsOK)
                        {
                            sErrString = "Gen2文件的'检查时间'数据无效";
                            iResult = iErr_ValidData;
                            goto Eend;

                        }
                        else
                        {
                            ThisGen2.m_image_datatime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }
                    else if (sData[0] == "PROCEDURES")
                    {
                        len_Procedures = MyTools.GetStringCode(sData[1]);
                        ThisGen2.m_Procedures = sData[1].Trim();
                    }
                    else if (sData[0] == "NOTES")
                    {
                        len_Note = MyTools.GetStringCode(sData[1]);
                        ThisGen2.m_Note = sData[1].Trim();
                    }
                }

                // 补充
                ThisGen2.m_FileID = 0;
                ThisGen2.m_Address = "";
                ThisGen2.m_Tele = "";
                ThisGen2.m_IdentifyID = "";
                // 补充 End

                ThisGen2.m_NScanSave = int.Parse(MyTools.ReadFileLine(br).Trim());
                ThisGen2.m_Angle2 = int.Parse(MyTools.ReadFileLine(br).Trim());
                ThisGen2.m_Number_line_360 = int.Parse(MyTools.ReadFileLine(br).Trim());
                ThisGen2.m_SN = int.Parse(MyTools.ReadFileLine(br).Trim());
                ThisGen2.m_DD = int.Parse(MyTools.ReadFileLine(br).Trim());
                ThisGen2.m_G = int.Parse(MyTools.ReadFileLine(br).Trim());

                //dTmp = double.Parse(MyTools.ReadFileLine(br).Trim());
                iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                while (true)
                {
                    if (iTmp != 1000)
                    {
                        break;
                    }
                    iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                }
                //iTmp = (int)(dTmp + 0.1);
                if (iTmp != len_lastname)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                if (iTmp != len_firstname)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                if (iTmp != len_patientid)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                if (iTmp != len_sex)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                if (iTmp != len_dateofbirth)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                if (iTmp != len_physican1)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                if (iTmp != len_physican2)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                if (iTmp != len_image_datatime)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                if (iTmp != len_Procedures)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                iTmp = int.Parse(MyTools.ReadFileLine(br).Trim());
                if (iTmp != len_Note)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                ThisGen2.m_NData = int.Parse(MyTools.ReadFileLine(br).Trim());
                ThisGen2.m_maxValue = double.Parse(MyTools.ReadFileLine(br).Trim());       // 记录这组Data中的最大值与最小值
                ThisGen2.m_minValue = double.Parse(MyTools.ReadFileLine(br).Trim());
                // fscanf(fpp, "%c", &cr);				 // 还要多读一个字符 = 10 0x0A

                ThisGen2.ampplot_sI = new Int32[ThisGen2.m_NScanSave, ThisGen2.m_NData];
                ThisGen2.ampplot_s = new double[ThisGen2.m_NScanSave, ThisGen2.m_NData];
                SumData1 = 0;
                sbyte d1 = 0;
                sbyte d2 = 0;
                byte c = 0;

                // For Print ========================================
                //FileStream FileForPrint = File.Open(Directory.GetCurrentDirectory() + "\\gen2_Ampplot_sI.txt", FileMode.Create);
                //StreamWriter w = new StreamWriter(FileForPrint);
                // ==================================================

                for (j = 0; j < ThisGen2.m_NScanSave; j++)
                {
                    for (i = 0; i < ThisGen2.m_NData; i++)
                    {
                        //d1 = (sbyte)br.ReadByte();
                        //d2 = (sbyte)br.ReadByte();

                        d1 = (sbyte)br.ReadByte();
                        if (d1 == 0x0D)
                        {
                            if (br.BaseStream.Position < (br.BaseStream.Length - 1))
                            {
                                c = br.ReadByte();
                                if (c == 0x0A)
                                {
                                    d1 = (sbyte)c;
                                }
                                else
                                {
                                    br.BaseStream.Seek(-1, SeekOrigin.Current);
                                }
                            }
                        }
                        d2 = (sbyte)br.ReadByte();
                        if (d2 == 0x0D)
                        {
                            if (br.BaseStream.Position < (br.BaseStream.Length - 1))
                            {
                                c = br.ReadByte();
                                if (c == 0x0A)
                                {
                                    d2 = (sbyte)c;
                                }
                                else
                                {
                                    br.BaseStream.Seek(-1, SeekOrigin.Current);
                                }
                            }
                        }

                        //while (true)
                        //{
                        //    d2 = (sbyte)br.ReadByte();
                        //    if (d2 == 0x0D)
                        //    {
                        //        continue;
                        //    }
                        //    break;
                        //}

                        iTmp = d1;
                        if (iTmp < 0)
                            iTmp += 256;
                        ThisGen2.ampplot_sI[j, i] = iTmp * 256;

                        iTmp = d2;
                        if (iTmp < 0)
                            iTmp += 256;
                        ThisGen2.ampplot_sI[j, i] += iTmp;

                        // For Print ========================
                        //w.Write("ThisGen2.ampplot_sI[" + j.ToString() + "," + i.ToString() + "] = " + ThisGen2.ampplot_sI[j, i].ToString() + Environment.NewLine);
                        // ==================================


                        ThisGen2.ampplot_s[j, i] = (ThisGen2.ampplot_sI[j, i] / 65535.0) * (ThisGen2.m_maxValue - ThisGen2.m_minValue) + ThisGen2.m_minValue;
                        //  temp save raw data for background
                        // U16_RawData.RawData[j][i] = (unsigned short) ampplot_sI[j][i];//ampplot_s[j][i];

                        SumData1 = SumData1 + d1 + d2;
                        //sTmp = "ampot(" + j.ToString("D3") + "," + i.ToString("D3") + ") = " + d1.ToString("X2") + "," + d2.ToString("X2") + "," + ThisGen2.ampplot_sI[j, i].ToString("D6") + ",P = " + br.BaseStream.Position.ToString("X");
                        //sw.WriteLine(sTmp);
                    }
                }

                // For Print ========================
                //w.Close();
                //FileForPrint.Close();
                // ==================================

                sTmp = MyTools.ReadFileLine(br).Trim();      //fscanf(fpp, "%c", &cr);

                sTmp = MyTools.ReadFileLine(br).Trim();
                if (sTmp.Length > 0)
                    iTmp = int.Parse(sTmp);
                else
                    iTmp = 0;

                if (iTmp != SumData1)
                {
                    sErrString = "Gen2文件数据无效";
                    iResult = iErr_ValidData;
                    goto Eend;
                }

                iTmp = 0;
                ThisGen2.m_CheckNote = "";
                while (true)
                {
                    if (br.BaseStream.Position >= (br.BaseStream.Length - 1))
                    {
                        break;
                    }
                    else
                    {
                        sTmp = MyTools.ReadFileLine(br).Trim();
                        ThisGen2.m_CheckNote += "\r\n" + sTmp;
                    }
                }

                ThisGen1 = ThisGen2;
                iResult = iErr_Succ;
                sErrString = "";

                br.Close();
                fs.Close();
                // 读完文件后，数据就存放在 FileStruct 中
            }
            catch (Exception ex)
            {
                sErrString = "读Gen文件出错（" + ex.Message + "） j = " + j.ToString() + ",i = " + i.ToString();
                iResult = iErr_Exception;
            }

            Eend:
            if (br != null)
            {
                br.Close();
            }
            if (fs != null)
            {
                fs.Close();
            }
            return iResult;
        }

        public static int ReadGen2FromGGClass(mod_GG GG, ref Gen2_Struct ThisGen2, ref string sErrString)
        {
            int iResult = iErr_UnknowErr;

            // 用GG当前帧的数据构建 Gen2
            ThisGen2 = new Gen2_Struct();

            try
            {
                ThisGen2.m_FileName = GG.ggData.m_szFileName;
                ThisGen2.m_cbsize = 0;
                ThisGen2.m_lastname = GG.ggData.m_ggheader.szLastname;
                ThisGen2.m_firstname = GG.ggData.m_ggheader.szFirstname;
                ThisGen2.m_patientid = GG.ggData.m_ggheader.szPatientid;
                ThisGen2.m_sex = GG.ggData.m_ggheader.szSex;
                ThisGen2.m_dateofbirth = GG.ggData.m_ggheader.szDateofBirth;
                ThisGen2.m_physican1 = GG.ggData.m_ggheader.szPhysican1;
                ThisGen2.m_physican2 = GG.ggData.m_ggheader.szPhysican2;
                ThisGen2.m_image_datatime = DateTime.Parse(GG.ggData.m_ggheader.szDate).ToString("yyyy-MM-dd ") +
                                            DateTime.Parse(GG.ggData.m_ggheader.szTime).ToString("HH:mm:ss");

                // 补充
                ThisGen2.m_FileID = 0;
                ThisGen2.m_Address = "";
                ThisGen2.m_Tele = "";
                ThisGen2.m_IdentifyID = "";
                // 补充 End

                //ThisGen2.m_Procedures;
                ThisGen2.m_Note = GG.ggData.m_ggheader.szNote;

                ThisGen2.m_NScanSave = GG.ggData.m_ggheader.numberofline;
                ThisGen2.m_Angle2 = (int)GG.ggData.m_ggheader.Angle2;
                ThisGen2.m_Number_line_360 = GG.ggData.m_ggheader.NumberOfline_360;
                ThisGen2.m_SN = GG.ggData.m_ggheader.SN;
                ThisGen2.m_DD = GG.ggData.m_ggheader.DD;
                ThisGen2.m_G = GG.ggData.m_ggheader.G;

                ThisGen2.m_NData = GG.ggData.m_ggheader.numberofpointofline;
                ThisGen2.m_maxValue = GG.ggData.m_ggframe_info.raw_maxvalue;       // 记录这组Data中的最大值与最小值
                ThisGen2.m_minValue = GG.ggData.m_ggframe_info.raw_minvalue;

                ThisGen2.ampplot_sI = new Int32[ThisGen2.m_NScanSave, ThisGen2.m_NData];
                ThisGen2.ampplot_s = new double[ThisGen2.m_NScanSave, ThisGen2.m_NData];

                sbyte d1 = 0;
                sbyte d2 = 0;
                int i = 0, j = 0;
                int iTmp = 0;
                ushort uSi = 0;

                double Max = 0, Min = 0;

                // For Print ========================================
                //FileStream FileForPrint = File.Open(Directory.GetCurrentDirectory() + "\\gg_Ampplot_sI.txt", FileMode.Create);
                //StreamWriter w = new StreamWriter(FileForPrint);
                // ==================================================

                for (j = 0; j < ThisGen2.m_NScanSave; j++)
                {
                    for (i = 0; i < ThisGen2.m_NData; i++)
                    {
                        ////d1 = (sbyte)br.ReadByte();
                        ////d2 = (sbyte)br.ReadByte();

                        //d1 = (sbyte)GG.ggData.m_lprawdata[j * ThisGen2.m_NData + 2 * i];
                        //d2 = (sbyte)GG.ggData.m_lprawdata[j * ThisGen2.m_NData + 2 * i + 1];
                        //iTmp = d1;
                        //if (iTmp < 0)
                        //    iTmp += 256;
                        //ThisGen2.ampplot_sI[j, i] = iTmp * 256;

                        //iTmp = d2;
                        //if (iTmp < 0)
                        //    iTmp += 256;
                        //ThisGen2.ampplot_sI[j, i] += iTmp;

                        uSi = BitConverter.ToUInt16(GG.ggData.m_lprawdata, j * 2 * ThisGen2.m_NData + 2 * i);
                        ThisGen2.ampplot_sI[j, i] = uSi;

                        // For Print ========================
                        //w.Write("ThisGen2.ampplot_sI[" + j.ToString() + "," + i.ToString() + "] = " + ThisGen2.ampplot_sI[j, i].ToString() + Environment.NewLine);
                        // ==================================

                        //if (j < 10)
                            ThisGen2.ampplot_s[j, i] = (ThisGen2.ampplot_sI[j, i] / 65535.0) * (ThisGen2.m_maxValue - ThisGen2.m_minValue) + ThisGen2.m_minValue;
                        //else
                        //    ThisGen2.ampplot_s[j, i] = 0;

                        if (ThisGen2.ampplot_s[j, i] < Min)
                            Min = ThisGen2.ampplot_s[j, i];

                        if (ThisGen2.ampplot_s[j, i] > Max)
                            Max = ThisGen2.ampplot_s[j, i];
                    }
                }

                // For Print ========================
                //w.Close();
                //FileForPrint.Close();
                // ==================================

                iResult = iErr_Succ;
                sErrString = "";
            }
            catch (Exception ex)
            {
                iResult = iErr_Exception;
                sErrString = "GG转换成Gen2时意外错误（" + ex.Message + "）";
            }

            return iResult;
        }

        public static int Gen2DataToDB(string sFileName, Gen2_Struct ThisGen2, bool IsCheckPatientID, bool IsCheckName, int iPatientMode, bool IsCheckRecord, int iRecordMode, ref string sErrString)
        {
            // PatientMode, RecordMode: See MyData.CoverMode;

            int iResult = MyData.iErr_UnknowErr;
            int iFileId = 0;
            int iId = -1;
            int ThisDataCoverMode = MyData.iDataCoverMode_New;
            string sSql = "";
            string sTmp = "";
            DataSet DS = null;
            sErrString = "";

            try
            {
                if (IsCheckPatientID || IsCheckName)
                {
                    // 是否检查已有相同 PatientID、 Name的档案
                    sSql = "";
                    if (IsCheckPatientID)
                    {
                        sSql = "Where PatientID = '" + ThisGen2.m_patientid + "'";
                    }
                    if (IsCheckName)
                    {
                        if (sSql == "")
                            sSql = "Where Name = '" + ThisGen2.m_firstname + ThisGen2.m_lastname + "'";
                        else
                            sSql += " OR Name = '" + ThisGen2.m_firstname + ThisGen2.m_lastname + "'";
                    }
                    sSql = "Select FileID, PatientID from Patient " + sSql;
                    if (!MyData.MySqlite.ReadData(sSql, ref DS, ref sErrString))
                    {
                        //MyTools.ShowMsg("导入失败！", "查询数据库失败: " + sErrString);
                        sErrString = "查询受检人数据表失败=" + sErrString;
                        goto Eend;
                    }

                    if (DS != null && DS.Tables[0].Rows.Count > 0)
                    {
                        if (iPatientMode == MyData.iDataCoverMode_Cover)   // 覆盖保存
                        {
                            iFileId = int.Parse(DS.Tables[0].Rows[0]["FileID"].ToString());
                            ThisDataCoverMode = MyData.iDataCoverMode_Cover;
                        }
                        else if (iPatientMode == MyData.iDataCoverMode_New)  // 新增
                        {
                            iFileId = -1;
                            ThisDataCoverMode = MyData.iDataCoverMode_New;
                        }
                        else if (iPatientMode == MyData.iDataCoverMode_NotCover)  // 不覆盖
                        {
                            iFileId = int.Parse(DS.Tables[0].Rows[0]["FileID"].ToString());
                            ThisDataCoverMode = MyData.iDataCoverMode_NotCover;
                        }
                    }
                    else   // 不存在
                    {
                        ThisDataCoverMode = MyData.iDataCoverMode_New;
                    }
                }
                else   // 不检查就是新增
                {
                    ThisDataCoverMode = MyData.iDataCoverMode_New;
                }

                if (ThisDataCoverMode == MyData.iDataCoverMode_New)
                {
                    // 新增
                    sSql = "Insert into Patient (PatientID, Name, Birthday, Sex, Address, Tele, IdentifyID, Memo ) Values(" +
                            "'" + ThisGen2.m_patientid + "'," +
                            "'" + ThisGen2.m_firstname + ThisGen2.m_lastname + "'," +
                            "'" + ThisGen2.m_dateofbirth + "'," +
                            "'" + (ThisGen2.m_sex == "男" ? "男" : "女") + "'," +
                            "'" + ThisGen2.m_Address + "'," +
                            "'" + ThisGen2.m_Tele + "'," +
                            "'" + ThisGen2.m_IdentifyID + "'," +
                            "'" + ThisGen2.m_Note + "')";
                    if (!MyData.MySqlite.WriteData(sSql, true, ref iFileId, ref sErrString))
                    {
                        //MyTools.ShowMsg("添加数据时出现错误", sErrString);
                        sErrString = "添加数据时出现错误=" + sErrString;
                        goto Eend;
                    }
                }
                else if (ThisDataCoverMode == MyData.iDataCoverMode_Cover)
                {
                    // 覆盖保存，FileID已读取
                    sSql = "Update Patient Set " +
                            "PatientID = '" + ThisGen2.m_patientid + "'," +
                            "Name = '" + ThisGen2.m_firstname + "'," +
                            "Birthday = '" + ThisGen2.m_dateofbirth + "'," +
                            "Sex = '" + ThisGen2.m_sex + "'," +
                            "Address = '" + ThisGen2.m_Address + "'," +
                            "Tele = '" + ThisGen2.m_Tele + "'," +
                            "IdentifyID = '" + ThisGen2.m_IdentifyID + "'," +
                            "Memo = '" + ThisGen2.m_Note + "' " +
                            "Where FileID = " + iFileId.ToString();
                    iId = -1;
                    if (!MyData.MySqlite.WriteData(sSql, false, ref iId, ref sErrString))
                    {
                        //MyTools.ShowMsg("覆盖数据时出现错误", sErrString);
                        sErrString = "覆盖数据时出现错误=" + sErrString;
                        goto Eend;
                    }
                }
                else if (ThisDataCoverMode == MyData.iDataCoverMode_NotCover)
                {
                    // 不做任何事情
                }

                // Save Check Record ==================================================================
                sTmp = System.Environment.CurrentDirectory + "\\Data\\" + iFileId.ToString();
                int iRecordID = -1;
                if (!Directory.Exists(sTmp))
                {
                    Directory.CreateDirectory(sTmp);
                }

                if (!Directory.Exists(sTmp + "\\Small"))
                {
                    Directory.CreateDirectory(sTmp + "\\Small");
                }

                if (!Directory.Exists(sTmp + "\\Big"))
                {
                    Directory.CreateDirectory(sTmp + "\\Big");
                }

                // 是否已有同 PatientID + Record 的档案
                if (IsCheckRecord)
                {
                    sSql = "Select FileID, RecordID, BIGPICT from Record Where FileId = '" + iFileId + "' AND CHECKTIME = '" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    DS = null;
                    sErrString = "";
                    if (!MyData.MySqlite.ReadData(sSql, ref DS, ref sErrString))
                    {
                        //MyTools.ShowMsg("导入失败！", "查询数据库失败: " + sErrString);
                        sErrString = "查询受检记录表失败=" + sErrString;
                        goto Eend;
                    }

                    if (DS != null && DS.Tables[0].Rows.Count > 0)
                    {
                        bool IsEqual = false;
                        string sFN1 = "";
                        string sFN2 = "";
                        for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                        {
                            sFN1 = System.IO.Path.GetExtension(DS.Tables[0].Rows[0]["BIGPICT"].ToString());
                            sFN2 = System.IO.Path.GetExtension(sFileName);
                            if (sFN1 == sFN2)
                            {
                                IsEqual = true;
                                break;
                            }
                        }

                        if (IsEqual)
                        {
                            if (iRecordMode == MyData.iDataCoverMode_Cover)   // 覆盖保存
                            {
                                iRecordID = int.Parse(DS.Tables[0].Rows[0]["RecordID"].ToString());
                                ThisDataCoverMode = MyData.iDataCoverMode_Cover;
                            }
                            else if (iRecordMode == MyData.iDataCoverMode_New)  // 新增
                            {
                                iRecordID = -1;
                                ThisDataCoverMode = MyData.iDataCoverMode_New;
                            }
                            else if (iRecordMode == MyData.iDataCoverMode_NotCover)  // 不覆盖
                            {
                                iRecordID = int.Parse(DS.Tables[0].Rows[0]["RecordID"].ToString());
                                ThisDataCoverMode = MyData.iDataCoverMode_NotCover;
                            }
                        }
                        else   // 不存在
                        {
                            iRecordID = -1;
                            ThisDataCoverMode = MyData.iDataCoverMode_New;
                        }
                    }
                    else   // 不存在
                    {
                        iRecordID = -1;
                        ThisDataCoverMode = MyData.iDataCoverMode_New;
                    }
                }
                else   // 不检查就是新增
                {
                    iRecordID = -1;
                    ThisDataCoverMode = MyData.iDataCoverMode_New;
                }

                string sExtendName = System.IO.Path.GetExtension(sFileName);
                if ( (ThisDataCoverMode == MyData.iDataCoverMode_New) || (ThisDataCoverMode == MyData.iDataCoverMode_Cover) )
                {
                    if (File.Exists(sTmp + "\\Big\\" + iFileId.ToString() + "BP_" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyyMMddHHmmss") + sExtendName))
                    {
                        File.Delete(sTmp + "\\Big\\" + iFileId.ToString() + "BP_" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyyMMddHHmmss") + sExtendName);
                    }
                    File.Copy(sFileName, sTmp + "\\Big\\" + iFileId.ToString() + "BP_" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyyMMddHHmmss") + sExtendName);


                    if (File.Exists(sTmp + "\\Small\\" + iFileId.ToString() + "SP_" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyyMMddHHmmss") + ".png"))
                    {
                        File.Delete(sTmp + "\\Small\\" + iFileId.ToString() + "SP_" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyyMMddHHmmss") + ".png");
                    }
                    if (SaveToSmallPict(sTmp + "\\Small\\" + iFileId.ToString() + "SP_" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyyMMddHHmmss") + ".png", ThisGen2, ref sErrString) != MyData.iErr_Succ)
                    {
                        sErrString = "复制Small Picture档案时出现错误 =" + sErrString;
                        goto Eend;
                    }

                    if (ThisDataCoverMode == MyData.iDataCoverMode_New)
                    {
                        // 新增
                        sSql = "Insert into Record ( FileID, CheckTime, Doctor, CheckInfo, SmallPict, BigPict ) Values (" +
                                iFileId.ToString() + "," +
                                "'" + ThisGen2.m_image_datatime + "'," +
                                "'" + ThisGen2.m_physican1 + "'," +
                                "'" + ThisGen2.m_CheckNote + "'," +
                                "'" + iFileId.ToString() + "SP_" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyyMMddHHmmss") + ".png" + "'," +
                                "'" + iFileId.ToString() + "BP_" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyyMMddHHmmss") + sExtendName + "')";
                        iId = -1;
                        if (!MyData.MySqlite.WriteData(sSql, false, ref iId, ref sErrString))
                        {
                            sErrString = "添加检验数据时出现错误=" + sErrString;
                            goto Eend;
                        }
                    }
                    else  // if (ThisDataCoverMode == MyData.iDataCoverMode_Cover)
                    {
                        // 覆盖保存，FileID，RecordID 已读取
                        sSql = "Update Record Set " +
                                "CHECKTIME = '" + ThisGen2.m_image_datatime + "'," +
                                "Doctor = '" + ThisGen2.m_physican1 + "'," +
                                "CheckInfo = '" + ThisGen2.m_CheckNote + "'," +
                                "SmallPict = '" + iFileId.ToString() + "SP_" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyyMMddHHmmss") + ".png'," +
                                "BigPict = '" + iFileId.ToString() + "BP_" + DateTime.Parse(ThisGen2.m_image_datatime).ToString("yyyyMMddHHmmss") + sExtendName + "' " +
                                "Where RecordID = " + iRecordID.ToString();
                        iId = -1;
                        if (!MyData.MySqlite.WriteData(sSql, false, ref iId, ref sErrString))
                        {
                            //MyTools.ShowMsg("覆盖数据时出现错误", sErrString);
                            sErrString = "覆盖Record数据时出现错误=" + sErrString;
                            goto Eend;
                        }
                    }
                }
                else if (ThisDataCoverMode == MyData.iDataCoverMode_NotCover)
                {
                    // 不做任何事情
                }

                //MyTools.ShowMsg("导入成功！", "");
                sErrString = "";
                iResult = MyData.iErr_Succ;
            }
            catch (Exception ex)
            {
                //MyTools.ShowMsg("复制档案时出现意外错误", "ex = " + ex.Message);
                sErrString = "意外错误=" + ex.Message;
                iResult = MyData.iErr_Exception;
            }

            Eend:
            if (DS != null)
                DS = null;
            return iResult;
        }

        private static int SaveToSmallPict(string sSmallPictFileName, Gen2_Struct ThisGen2, ref string sErrString)
        {
            int iResult = MyData.iErr_UnknowErr;
            sErrString = "";
            try
            {
                // if File exist,delete it.
                if (File.Exists(sSmallPictFileName))
                {
                    File.Delete(sSmallPictFileName);
                }

                // Create Big Picture
                mod_ShowPict PB = new mod_ShowPict();
                PB.pGen2 = ThisGen2;
                PB.DisplayMode = MyData.DisplayMode_Yello;
                PB.PictBrightness = 0;
                PB.PictContract = 0;

                PictureBox pbox = new PictureBox();
                pbox.Width = 700;
                pbox.Height = 700;
                PB.Gen2ToData();
                PB.Gen2ToPict();

                // 缩小保存
                Image img = PB.BM;
                int width = 60;
                Image newImg = new Bitmap(width, img.Height * width / img.Width);
                Graphics g = Graphics.FromImage(newImg);
                g.DrawImage(img, 0, 0, width, img.Height * width / img.Width);
                newImg.Save(sSmallPictFileName);
                iResult = MyData.iErr_Succ;
                sErrString = "";
            }
            catch (Exception ex)
            {
                iResult = MyData.iErr_Exception;
                sErrString = "生成索引图时出现意外错误：" + ex.Message;
            }

            return iResult;
        }

        // 1、计划观察12条射线，每条之间间隔30度
        // 2、在每条线上检查有多少个亮环，并记录下来，预计有20个
        struct Ring_Struct
        {
            public int Rmax;    // 亮环外径
            public int Rmin;    // 亮环内径
            public int RAvg;    // 亮环平均半径
            public int Target;  // 命中数
        }
        // 3、一些参数
        const double PI = 3.1415926535897932384626433832795028841971;
        const double PI_45 = PI / 4;
        const double PI_90 = PI / 2;
        const double PI_180 = PI;                    // 180度的圆周值
        const double PI_360 = PI * 2;                // 360 度的圆周值
        const double PI_360_Div = PI * 2 / 360.0;    // 细分每一度的圆周值

        // 对当前的gen2记录进行检查
        public static int FindTube(ref Gen2_Struct ThisGen2, ref int iRingNum, ref string sErrString)
        {
            int iResult = iErr_UnknowErr;
            int i = 0, j = 0;
            double[,] BW_Pict = new double[ThisGen2.ampplot_s.GetUpperBound(0) + 1, ThisGen2.ampplot_s.GetUpperBound(1) + 1];
            double[,] RingMark = new double[ThisGen2.m_NData, MyData.iBinaryzationLineNum];                                       // 保存有值的点：射线上总的扫描点数， 观察多少条线
            int[] RingMarkTotal = new int[ThisGen2.m_NData];                                                                          // 记录每个环有多少条线是有值的
            Ring_Struct[] Rings = new Ring_Struct[ThisGen2.m_NData];                                                                  // 最大就是512个

            try
            {
                // 二值化
                for (i = 0; i < ThisGen2.ampplot_s.GetUpperBound(0) + 1; i++)
                {
                    for (j = 0; j < ThisGen2.ampplot_s.GetUpperBound(1) + 1; j++)
                    {
                        if ( ThisGen2.ampplot_s[i, j] > (ThisGen2.m_maxValue * MyData.dBinaryzation))
                        {
                            BW_Pict[i, j] = 255;
                            ThisGen2.ampplot_s[i, j] = 255;
                        }
                        else
                        {
                            BW_Pict[i, j] = 0;
                            ThisGen2.ampplot_s[i, j] = 0;
                        }
                    }
                }

                // 获取各圆环上，在指定的n条射线上是否有亮点
                double ChangeLine = ThisGen2.m_NScanSave/ MyData.iBinaryzationLineNum;         // 平均两条线之间包含的扫描线数量 = 总扫描线/扫描次数
                for (i = 0; i < ThisGen2.m_NData; i++)        // i=环
                {
                    // 寻找该环上是否有亮点
                    for (j = 0; j < MyData.iBinaryzationLineNum; j++)
                    {
                        if (BW_Pict[(int)( ChangeLine * j),i] != 0)
                        {
                            RingMark[i, j] = 255;            // 第j线上，在第i环上有值
                            RingMarkTotal[i]++;              // 在第i环上命中点+1
                        }
                    }

                    // 顺便初始化Rings
                    Rings[i] = new Ring_Struct();
                }

                // 合拼亮点环
                bool FindWhite = false;                       // 前面已经找到第一个亮点
                int CombineRingNum = -1;                        // 合成环的编号
                for (i = 0; i < ThisGen2.m_NData; i++)        // i=环
                {
                    if (RingMarkTotal[i] > 0)
                    {
                        if (FindWhite)         // 不是新环
                        {
                        }
                        else                   // 新环
                        {
                            CombineRingNum++;
                            Rings[CombineRingNum].Rmin = i;
                            FindWhite = true;
                        }
                        Rings[CombineRingNum].Rmax = i;
                        Rings[CombineRingNum].Target += RingMarkTotal[i];      // 合并亮点数
                    }
                    else // RingMarkTotal[i] == 0
                    {
                        if (FindWhite)       // 已经有环，就是结束
                        {
                            FindWhite = false;
                        }
                        else     // 不是亮环,不用统计
                        {
                        }
                    }
                }

                double dMaxRing = 0;
                int SelectMax = -1;
                double dMinRing = 10000;
                int SelectMin = -1;
                double dTmp = 0;
                int SelectRing = -1;

                // 选取合适的环
                if (CombineRingNum == 0)         // 有一环
                {
                    SelectMin = 0;
                }
                else if (CombineRingNum > 0)     // 有多环
                {
                    for (i = 0; i <= CombineRingNum; i++)         // 观察所有环
                    {
                        dTmp = Rings[i].Target / (Rings[i].Rmax - Rings[i].Rmin + 1);             //    亮点数/占用线数

                        Rings[i].RAvg = (int)dTmp;
                        if (dTmp < dMinRing)             // 取最小环
                        {
                            dMinRing = dTmp;
                            SelectMin = i;
                        }

                        if (dTmp > dMaxRing)             // 取最大环
                        {
                            dMaxRing = dTmp;
                            SelectMax = i;
                        }
                    }
                }

                SelectRing = SelectMin;

                // 在找到的环上，选择最佳的环数
                double dDistanceAvg = 0;
                if (SelectRing >= 0)
                {
                    dDistanceAvg = Rings[SelectRing].Rmin + (Rings[SelectRing].Rmax - Rings[SelectRing].Rmin) / 2;        //    这个环的中心线
                    j = -1;                                                                    // 选择了那条线
                    dTmp = 0;
                    for (i = Rings[SelectRing].Rmin; i <= Rings[SelectRing].Rmax; i++)
                    {
                        if (RingMarkTotal[i] > dTmp)
                        {
                            j = i;
                            dTmp = RingMarkTotal[i];
                        }
                        else if (RingMarkTotal[i] == dTmp)   // 等于时，看看那条更靠近中心线
                        {
                            if (Math.Abs(i - dDistanceAvg) < Math.Abs(j - dDistanceAvg))
                            {
                                j = i;
                            }
                        }
                    }

                    // 返回
                    iRingNum = j;
                    iResult = iErr_Succ;
                    sErrString = "";
                }
                else
                {
                    iResult = iErr_Nofind;
                    sErrString = "没有符合条件的记录";
                }
            }
            catch (Exception ex)
            {
                sErrString = "计算时意外错误（FindTube = " + ex.Message + "）";
                iResult = iErr_Exception;
            }

            return iResult;
        }
    }
}

