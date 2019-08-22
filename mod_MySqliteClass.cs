using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.Threading;

namespace OctTools
{
    public class MySqliteClass
    {
        string MyDbName = System.Environment.CurrentDirectory + "\\OctData.dat";
        string MyErrMsg = "";
        SQLiteConnection MyConn;

        public string DbFileName
        {
            get
            {
                return MyDbName;
            }
            set
            {
                MyDbName = value;
            }
        }

        public bool IsOpen
        {
            get
            {
                return DbIsOpen();
            }
        }

        public string ErrMsg
        {
            get
            {
                return MyErrMsg;
            }
        }

        public bool OpenDb()
        {
            try
            {
                if (!DbIsOpen())
                {
                    MyConn = new SQLiteConnection();
                    MyConn.ConnectionString = "DataSource = " + MyDbName;
                    MyConn.Open();
                    MyErrMsg = "Open db succ.";
                }
            }
            catch (Exception ex)
            {
                MyErrMsg = "Open db fault = " + ex.Message;
                MyTools.WriteLogFile("MySqliteClass.OpenDb = Err.", ex.Message);
            }
            return DbIsOpen();
        }

        public void CloseDb()
        {
            if (DbIsOpen())
            {
                MyConn.Close();
            }
            MyTools.WriteLogFile("MySqliteClass.CloseDb", "sucesss.");
        }

        public bool DbIsOpen()
        {
            if (MyConn == null || MyConn.State != System.Data.ConnectionState.Open)
                return false;
            else
                return true;
        }

        public bool WriteData(string SqlString, bool ReturnID, ref int ID, ref string sErrMsg)
        {
            bool bResult = false;
            int iJ;
            SQLiteCommand cmd = null;
            SQLiteDataReader myRead = null;

            try
            {
                cmd = new SQLiteCommand();                                 //定义OleDbCommand对象cmd
                cmd.Connection = MyConn;
                cmd.CommandType = System.Data.CommandType.Text;           //指定SqlCommand类型
                cmd.CommandText = SqlString;

                iJ = cmd.ExecuteNonQuery();                                                 //执行
                if (iJ < 0)
                {
                    MyErrMsg = "写入记录时失败。";
                }
                else
                {
                    if (ReturnID)
                    {
                        if (iJ <= 0)
                        {
                            MyErrMsg = "写入记录时失败。";
                        }
                        else
                        {
                            cmd.CommandText = "select last_insert_rowid()";
                            myRead = cmd.ExecuteReader();                                 //执行
                            if (!myRead.Read())
                            {
                                sErrMsg = "插入新记录后读ID失败。";
                                ID = -1;
                            }
                            else
                            {
                                ID = myRead.GetInt32(0);
                                MyErrMsg = "写入记录成功。";
                            }
                            bResult = true;
                        }
                    }
                    else
                    {
                        bResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MyErrMsg = "写入记录时发生错误：" + ex.Message;
                MyTools.WriteLogFile("MySqliteClass.WriteData = Err.", "sql = " + SqlString + ", err = " + ex.Message);
            }

            if (myRead != null)
                myRead.Dispose();
            if (cmd != null)
                cmd.Dispose();
            sErrMsg = MyErrMsg;
            return bResult;
        }

        public bool ReadData(string SqlString, ref DataSet oDataSet, ref string sErrMsg)
        {
            bool bResult = false;
            SQLiteDataAdapter da = null;
            DataSet odt = null;
            //string sToday = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                da = new SQLiteDataAdapter();
                da.SelectCommand = new SQLiteCommand(SqlString, MyConn);
                odt = new DataSet();
                da.Fill(odt);

                if (odt.Tables[0].Rows.Count > 0)
                {
                    oDataSet = odt.Copy();
                }
                else
                {
                    oDataSet = null;
                }
                MyErrMsg = "读数据成功。";
                bResult = true;
            }
            catch (Exception ex)
            {
                MyErrMsg = "读出记录时发生错误：" + ex.Message;
                MyTools.WriteLogFile("MySqliteClass.ReadData = Err.", "sql = " + SqlString + ", err = " + ex.Message);
            }

            //Eend:
            if (odt != null)
                odt.Dispose();
            if (da != null)
                da.Dispose();
            sErrMsg = MyErrMsg;
            return bResult;
        }

        public bool GetAllPatient(ref MyData.PatientInfo_Struct[] pPatientInfo, ref string sErr)
        {
            bool bResult = false;
            DataSet DS = null;
            string sSql = "Select * from Patient";

            try
            {
                if (!ReadData(sSql, ref DS, ref sErr) )
                {
                    goto Eend;
                }

                if (DS != null && DS.Tables[0].Rows.Count > 0)
                {
                    pPatientInfo = new MyData.PatientInfo_Struct[DS.Tables[0].Rows.Count];
                    for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                    {
                        pPatientInfo[i].FileID = DS.Tables[0].Rows[i]["FileID"].ToString();
                        pPatientInfo[i].PatientID = DS.Tables[0].Rows[i]["PatientID"].ToString();
                        pPatientInfo[i].Name = DS.Tables[0].Rows[i]["Name"].ToString();
                        pPatientInfo[i].Birthday = DS.Tables[0].Rows[i]["Birthday"].ToString();
                        pPatientInfo[i].Sex = DS.Tables[0].Rows[i]["Sex"].ToString();
                        pPatientInfo[i].Address = DS.Tables[0].Rows[i]["Address"].ToString();
                        pPatientInfo[i].Tele = DS.Tables[0].Rows[i]["Tele"].ToString();
                        pPatientInfo[i].IdentifyID = DS.Tables[0].Rows[i]["IdentifyID"].ToString();
                        pPatientInfo[i].Memo = DS.Tables[0].Rows[i]["Memo"].ToString();
                    }
                }
                else
                {
                    pPatientInfo = null;
                }
                bResult = true;
                sErr = "";
            }
            catch  (Exception ex)
            {
                sErr = ex.Message;
                pPatientInfo = null;
            }

            Eend:
            if (DS != null)
                DS = null;
            return bResult;
        }

        public bool GetPatientRecord(string sFileID_Patient, ref MyData.CheckRecord_Struct[] Records, ref string sErrStr)
        {
            bool bResult = false;
            DataSet DS = null;
            string sSql = "Select * from Record Where FileID = " + sFileID_Patient + " Order by CheckTime Desc";

            try
            {
                if (!ReadData(sSql, ref DS, ref sErrStr))
                {
                    goto Eend;
                }

                if (DS != null && DS.Tables[0].Rows.Count > 0)
                {
                    Records = new MyData.CheckRecord_Struct[DS.Tables[0].Rows.Count];
                    for (int i = 0; i < DS.Tables[0].Rows.Count; i++)
                    {
                        Records[i].FileID = DS.Tables[0].Rows[i]["FileID"].ToString();
                        Records[i].RecordID = DS.Tables[0].Rows[i]["RecordID"].ToString();
                        Records[i].CheckTime = DateTime.Parse(DS.Tables[0].Rows[i]["CheckTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                        Records[i].Doctor = DS.Tables[0].Rows[i]["Doctor"].ToString();
                        Records[i].CheckInfo = DS.Tables[0].Rows[i]["CheckInfo"].ToString();
                        Records[i].SmallPict = DS.Tables[0].Rows[i]["SmallPict"].ToString();
                        Records[i].BigPict = DS.Tables[0].Rows[i]["BigPict"].ToString();
                        Records[i].SelectPict = DS.Tables[0].Rows[i]["SelectPict"].ToString();
                    }
                }
                else
                {
                    Records = null;
                }
                bResult = true;
                sErrStr = "";
            }
            catch (Exception ex)
            {
                sErrStr = ex.Message;
            }

            Eend:
            if (DS != null)
                DS = null;
            return bResult;
        }

        public bool GetRefe(ref string sMsg)
        {
            bool bResult = false;
            string sSql = "Select * from RefeInfo";
            string sFunction = "";
            DataSet oDataSet = new DataSet();

            try
            {
                if (!ReadData(sSql, ref oDataSet, ref MyErrMsg))
                {
                    goto ErrEnd;
                }

                if (oDataSet.Tables[0].Rows.Count <= 0)
                {
                    MyErrMsg = "打开数据库表错误。";
                    goto ErrEnd;
                }

                // DB Records
                for (int i = 0; i < oDataSet.Tables[0].Rows.Count; i++)
                {
                    sFunction = oDataSet.Tables[0].Rows[i]["FuncName"].ToString().Trim();
                    switch (sFunction)
                    {
                        case "Bright":
                            MyData.BrightDefault = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                        case "Contract":
                            MyData.ContractDefault = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                        case "Saturation":
                            MyData.SaturationDefault = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                        case "Color":
                            MyData.ColorDefault = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                        case "Angle":
                            MyData.AngleDefault = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                        case "Delay":
                            MyData.DelayDefault = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;

                        case "BrightCurrent":
                            MyData.BrightCurrent = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                        case "ContractCurrent":
                            MyData.ContractCurrent = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                        case "SaturationCurrent":
                            MyData.SaturationCurrent = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                        case "ColorCurrent":
                            MyData.ColorCurrent = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                        case "AngleCurrent":
                            MyData.AngleCurrent = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                        case "DelayCurrent":
                            MyData.DelayCurrent = int.Parse(oDataSet.Tables[0].Rows[i]["Data"].ToString().Trim());
                            break;
                    }
                }
                MyErrMsg = "ReadData RefeInfo table succ.";
                bResult = true;
            }
            catch (Exception ex)
            {
                //MyTools.WriteLogFile("MySqliteClass.GetRefe = Err.", "Err = " + ex.Message);
                MyErrMsg = "读取参数表意外错误 = " + ex.Message;
            }

            ErrEnd:
            if (oDataSet != null)
                oDataSet.Dispose();

            sMsg = MyErrMsg;
            return bResult;
        }

        public bool SetRefe(ref string sMsg)
        {
            bool bResult = false;
            string sSql = "";
            string sErrMsg = "";
            string sFunction = "";
            string sData = "";
            int i = -1;
            MyErrMsg = "";
            try
            {
                for (i = 0; i < 12; i++)
                {
                    switch (i)
                    {
                        case 0:
                            sFunction = "Bright";
                            sData = MyData.BrightDefault.ToString();
                            break;
                        case 1:
                            sFunction = "Contract";
                            sData = MyData.ContractDefault.ToString();
                            break;
                        case 2:
                            sFunction = "Saturation";
                            sData = MyData.SaturationDefault.ToString();
                            break;
                        case 3:
                            sFunction = "Color";
                            sData = MyData.ColorDefault.ToString();
                            break;
                        case 4:
                            sFunction = "Angle";
                            sData = MyData.AngleDefault.ToString();
                            break;
                        case 5:
                            sFunction = "Delay";
                            sData = MyData.DelayDefault.ToString();
                            break;

                        case 6:
                            sFunction = "BrightCurrent";
                            sData = MyData.BrightCurrent.ToString();
                            break;
                        case 7:
                            sFunction = "ContractCurrent";
                            sData = MyData.ContractCurrent.ToString();
                            break;
                        case 8:
                            sFunction = "SaturationCurrent";
                            sData = MyData.SaturationCurrent.ToString();
                            break;
                        case 9:
                            sFunction = "ColorCurrent";
                            sData = MyData.ColorCurrent.ToString();
                            break;
                        case 10:
                            sFunction = "AngleCurrent";
                            sData = MyData.AngleCurrent.ToString();
                            break;
                        case 11:
                            sFunction = "DelayCurrent";
                            sData = MyData.DelayCurrent.ToString();
                            break;
                    }

                    sSql = "Update RefeInfo set Data = '" + sData + "' where FuncName = '" + sFunction + "'";
                    if (!WriteData(sSql, false, ref i, ref sErrMsg))
                    {
                        MyErrMsg = "修改栏目(" + sFunction + ") 错误.";
                        goto Eend;
                    }
                }

                MyErrMsg = "修改参数表成功.";
                bResult = true;
            }
            catch (Exception ex)
            {
                //MyTools.WriteLogFile("MySqliteClass.SetRefe = Err.", "Err = " + ex.Message);
                MyErrMsg = "修改参数表意外错误(" + i.ToString() + ") = " + ex.Message;
            }

            Eend:
            sMsg = MyErrMsg;
            return bResult;
        }

        public bool FindPatient(string PATIENTID, ref bool IsExist, ref string sErrString)
        {
            bool bResult = false;
            string sSql = "Select * From Patient Where PATIENTID = '" + PATIENTID + "'";
            DataSet pDataSet = null;
            IsExist = false;
            sErrString = "";

            if (!ReadData(sSql, ref pDataSet, ref sErrString))
            {
            }
            else
            {
                // 找到
                if (pDataSet != null && pDataSet.Tables[0].Rows.Count > 0)
                {
                    IsExist = true;
                }
                else
                {
                    sErrString = "不存在";
                }
                bResult = true;
            }
            return bResult;
        }
        public bool FindRecord(string PATIENTID, string CheckTime, ref bool IsExist, ref string sErrString)
        {
            bool bResult = false;
            string sSql = "Select A.FILEID, B.RECORDID From Patient AS A, Record AS B Where A.PATIENTID = '" + PATIENTID + "' AND A.FILEID = B.FILEID AND CHECKTIME = '" + CheckTime + "' Group By A.FILEID";
            DataSet pDataSet = null;
            IsExist = false;
            sErrString = "";

            if (!ReadData(sSql, ref pDataSet, ref sErrString))
            {
            }
            else
            {
                // 找到
                if (pDataSet != null && pDataSet.Tables[0].Rows.Count > 0)
                {
                    IsExist = true;
                }
                else
                {
                    sErrString = "不存在";
                }
                bResult = true;
            }
            return bResult;
        }

    }
}
