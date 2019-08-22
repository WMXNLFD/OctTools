using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace OctTools
{
    public class MyTools
    {
        public static string StrTimeToStrDateTime(string StrTime)
        {
            if (StrTime != null && StrTime.Length >= 16)
                return StrTime.Substring(0, 4) + "-" + StrTime.Substring(4, 2) + "-" +
                         StrTime.Substring(6, 2) + " " + StrTime.Substring(8, 2) + ":" +
                         StrTime.Substring(10, 2) + ":" + StrTime.Substring(12, 2) + "." + StrTime.Substring(14, 2);
            else if (StrTime != null && StrTime.Length == 14)
                return StrTime.Substring(0, 4) + "-" + StrTime.Substring(4, 2) + "-" +
                         StrTime.Substring(6, 2) + " " + StrTime.Substring(8, 2) + ":" +
                         StrTime.Substring(10, 2) + ":" + StrTime.Substring(12, 2);
            else
                return StrTime;
        }

        public static string StrDataTimeToStrTime(string StrDataTime)
        {
            if (StrDataTime != null && StrDataTime.Length == 21)
                return StrDataTime.Substring(0, 4) + StrDataTime.Substring(5, 2) + StrDataTime.Substring(8, 2) +
                       StrDataTime.Substring(11, 2) + StrDataTime.Substring(14, 2) + StrDataTime.Substring(17, 2) + StrDataTime.Substring(20, 2);
            else
                return StrDataTime;
        }

        public static string ByteToString(byte[] byteSourceArry, int iBeginPos, int iByteLen)
        {
            int iI,iJ;
            byte Ch;
            string sTargetStr="";
            string sAscII = "0123456789ABCDEF";

            for (iI = 0; iI < iByteLen; iI++)
            {
                Ch = byteSourceArry[iI + iBeginPos];
                iJ = (Ch & 0xf0)>>4;
                sTargetStr += sAscII[iJ];

                iJ = Ch & 0x0f;
                sTargetStr += sAscII[iJ];
            }
            return sTargetStr;
        }

        public static void StringToByte(string sStringSource, byte[] bByteTarget, int iDstPos, int iByteLen)
        {
            int iI, iJ;
            char cC1, cC2;

            for (iI = 0; iI < iByteLen; iI++)
            {
                cC1 = sStringSource[2 * iI];
                cC2 = sStringSource[2 * iI + 1];

                if (cC1 >= 0x30 && cC1 <= 0x39)
                    iJ = cC1 - 0x30;
                else if (cC1 >= 0x41 && cC1 <= 0x46)
                    iJ = 10 + cC1 - 0x41;
                else
                    iJ = 10 + cC1 - 0x61;

                if (cC2 >= 0x30 && cC2 <= 0x39)
                    iJ = (iJ << 4) + cC2 - 0x30;
                else if (cC2 >= 0x41 && cC2 <= 0x46)
                    iJ = (iJ << 4) + 10 + cC2 - 0x41;
                else
                    iJ = (iJ << 4) + 10 + cC2 - 0x61;

                bByteTarget[iI + iDstPos] = (byte)(iJ & 0xff);
            }
        }

        public static long StrToLong(string sStrBuff, int iStrLen)
        {
            int i;
            long li;

            li = 0;
            for (i = 0; i < iStrLen; i++)
            {
                li = li * 10 + (long)(sStrBuff[i]-0x30);
            }
            return li;
        }

        public static long BCDStringToLong(string sBCDString)
        {
            byte[] bTmp = new byte[4]; 
            long li = 0;

            StringToByte(sBCDString, bTmp, 0, 4);
            li = BytesToNum(bTmp, 0, 4);
            return li;
        }

        public static string StringToHexString(string szSrc)
        {//e.g "78"->"3738"
            string szOut = "";
            byte[] ucArr = System.Text.Encoding.ASCII.GetBytes(szSrc);
            int iLen = ucArr.Length;
            for (int i = 0; i < iLen; i++)
                szOut += ucArr[i].ToString("X2");// Convert.ToString(asciicode);//字符串ASCIIstr2 为对应的ASCII字符串
            return szOut;
        }

        public static string HexStringToString(string szSrc)
        {//e.g "78"->"3738"
            string szOut = "";
            int iLen = szSrc.Length / 2;
            byte[] ucBuf = new byte[iLen];
            MyTools.StringToByte(szSrc, ucBuf, 0, iLen);
            for (int i = 0; i < iLen; i++)
                szOut += Convert.ToChar(ucBuf[i]);
            return szOut;
        }

        public static string ByteToUnicode(ref byte[] ucSrc, int iLen)
        {
            char cSrc;
            string szDst = "";
            for (int i = 0; i < iLen; i++)
            {
                cSrc = (char)ucSrc[i * 2 + 1];
                if (cSrc == 0)
                    return szDst;

                cSrc <<= 8;
                cSrc += (char)ucSrc[i * 2];
                szDst = szDst + cSrc;
            }
            return szDst;
        }

        public static void UnicodeToByte(string szSrc, ref byte[] ucDst)
        {//string is UNICODE
            int iLen = szSrc.Length;
            char cSrc;
            for (int i = 0; i < iLen; i++)
            {
                cSrc = szSrc[i];
                ucDst[i * 2 + 1] = (byte)(cSrc >> 8);
                ucDst[i * 2] = (byte)cSrc;
            }
        }

        //比较两字串数组， 如果两个数组相同，返回0；如果数组1小于数组2，返回小于0的值；如果数组1大于数组2，返回大于0的值。
        public static int MemoryCompare(byte[] b1, byte[] b2)
        {
            int result = 0;
            int iLen = 0;

            if (b1.Length == 0 && b2.Length == 0)
            {
                return 0;
            }
            else if (b1.Length == 0 || b2.Length == 0)
            {
                if (b1.Length == 0)
                    return -1;
                else
                    return 1;
            }

            if (b1.Length <= b2.Length)
                iLen = b1.Length;
            else
                iLen = b2.Length;

            for (int i = 0; i < iLen; i++)
            {
                if (b1.Length < iLen)
                {
                    return -1;
                }
                else if (b2.Length < iLen)
                {
                    return 1;
                }

                if (b1[i] != b2[i])
                {
                    result = (int)(b1[i] - b2[i]);
                    break;
                }
            }
            return result;
        }

        public static int BytesComp(byte[] b1,int iBgnPos1, byte[] b2,int iBgnPos2,int iLen)
        {
            int result = 0;
            //if (b1.Length != b2.Length)
            //    result = b1.Length - b2.Length;
            //else
            //{
            for (int i = 0; i < iLen; i++)
                {
                    if (b1[i+iBgnPos1] != b2[i+iBgnPos2])
                    {
                        result = (int)(b1[i+iBgnPos1] - b2[i+iBgnPos2]);
                        break;
                    }
                }
            //}
            return result;
        }

        public static void NumToBytes(long Src, byte[] Dst, int iDstPos, int iLen)
        {
            long lTemp = Src;
            for (int index = iLen; index > 0; index--)
            {
                Dst[index - 1 + iDstPos] = Convert.ToByte(lTemp % 256);
                lTemp -= Dst[index - 1 + iDstPos];
                lTemp /= 256;
            }
        }

        public static long BytesToNum(byte[] Src, int iSrcPos, int iLen)
        {
            long lTemp = 0;
            for (int index = 0; index < iLen; index++)
            {
                lTemp += Src[index + iSrcPos];
                if (index + 1 != iLen)
                    lTemp *= 256;
            }
            return lTemp;
        }

        public static int StringToInt32(string DataStr)
        {
            int i = 0;
            try
            {
                i = Int32.Parse(DataStr);
            }
            catch
            {
                i = 0;
            }
            return i;
        }

        public static float StringToFloat(string DataStr)
        {
            float fi = 0;
            try
            {
                fi = float.Parse(DataStr);
            }
            catch
            {
                fi = -1;
            }
            return fi;
        }

        // 这是为打印机使用设计，打印机中一个中文=2个ASCII
        // 取字串中指定位置起的子字串，其长度不得大于iMaxLen(如果最后一个字节刚好碰到是汉字，则长度就会超过，此时须在后面补" ")
        public static string GetStringSubLen(string sString, int iStartPos, int iMaxLen)
        {
            int i = 0, j = 0;
            string sSub = "";

            if (iMaxLen <= 0)
                return "";

            if (sString.Length == 0)
            {
                sSub = sSub.PadRight(iMaxLen);
            }
            else
            {
                for (i = 0; i < iMaxLen;)
                {
                    if ((iStartPos + j) >= sString.Length)    // 超过了源的长度了，就在后面补空格
                    {
                        sSub = sSub.PadRight(iMaxLen - i);
                        i = iMaxLen;
                    }
                    else
                    {
                        if (sString[iStartPos + j] <= 127)
                        {
                            sSub += sString[iStartPos + j];
                            i++;
                            j++;
                        }
                        else   // HZ
                        {
                            if ((iMaxLen - i) >= 2)
                            {
                                sSub += sString[iStartPos + j];
                                i += 2;
                                j++;
                            }
                            else
                            {
                                sSub += " ";
                                i++;
                            }
                        }
                    }
                }
            }
            return sSub;
        }

        // pverride 1
        public static string IPValueToIPStr( int IP1, int IP2, int IP3, int IP4)
        {
            return IP1.ToString() + "." + IP2.ToString() + "." + IP3.ToString() + "." + IP4.ToString("000") ;
        }
        // pverride 2
        public static string IPValueToIPStr(int IP1, int IP2, int IP3, int IP4, int Port)
        {
            return IP1.ToString() + "." + IP2.ToString() + "." + IP3.ToString() + "." + IP4.ToString("000") + ":" + Port.ToString();
        }

        // pverride 1
        public static void IPStrToIPValue(string IPStr, out byte IP1, out byte IP2, out byte IP3, out byte IP4)
        {
            int i;
            string sStr, sStrOld;

            sStrOld = IPStr;
            i = sStrOld.IndexOf('.');
            sStr = sStrOld.Substring(0, i);
            IP1 = (byte)StrToLong(sStr, sStr.Length);

            sStrOld = sStrOld.Substring(i + 1, sStrOld.Length - i - 1);
            i = sStrOld.IndexOf('.');
            sStr = sStrOld.Substring(0, i);
            IP2 = (byte)StrToLong(sStr, sStr.Length);

            sStrOld = sStrOld.Substring(i + 1, sStrOld.Length - i - 1);
            i = sStrOld.IndexOf('.');
            sStr = sStrOld.Substring(0, i);
            IP3 = (byte)StrToLong(sStr, sStr.Length);

            sStrOld = sStrOld.Substring(i + 1, sStrOld.Length - i - 1);
            i = sStrOld.IndexOf(':');
            if (i > 0)
            {
                sStr = sStrOld.Substring(0, i);
                IP4 = (byte)StrToLong(sStr, sStr.Length);
            }
            else
            {
                IP4 = (byte)StrToLong(sStrOld, sStrOld.Length);
            }
        }

        // pverride 2
        public static void IPStrToIPValue(string IPStr, out byte IP1, out byte IP2, out byte IP3, out byte IP4, out int Port)
        {
            int i;
            string sStr, sStrOld;

            sStrOld = IPStr;
            i = sStrOld.IndexOf('.');
            sStr = sStrOld.Substring(0, i);
            IP1 = (byte)StrToLong(sStr, sStr.Length);

            sStrOld = sStrOld.Substring(i + 1, sStrOld.Length - i - 1);
            i = sStrOld.IndexOf('.');
            sStr = sStrOld.Substring(0, i);
            IP2 = (byte)StrToLong(sStr, sStr.Length);

            sStrOld = sStrOld.Substring(i + 1, sStrOld.Length - i - 1);
            i = sStrOld.IndexOf('.');
            sStr = sStrOld.Substring(0, i);
            IP3 = (byte)StrToLong(sStr, sStr.Length);

            sStrOld = sStrOld.Substring(i + 1, sStrOld.Length - i - 1);
            i = sStrOld.IndexOf(':');
            if (i > 0)
            {
                sStr = sStrOld.Substring(0, i);
                IP4 = (byte)StrToLong(sStr, sStr.Length);

                sStr = sStrOld.Substring(i + 1, sStrOld.Length - i - 1);
                Port = (int)StrToLong(sStr, sStr.Length);
            }
            else
            {
                IP4 = (byte)StrToLong(sStrOld, sStrOld.Length);
                Port = 0;
            }

        }


        public static bool StringIsHex(string sMMessage)
        {
            //判断字串是否为Hex16进制的字符串（0～9，A～B，a～b）
            //如果是空串，返回 false
            int i;

            if (sMMessage.Length == 0)
                return false;

            char[] cCS = sMMessage.ToCharArray(0, sMMessage.Length);

            for (i = 0; i < sMMessage.Length; i++)
            {
                if ((cCS[i] >= '0' && cCS[i] <= '9') || (cCS[i] >= 'A' && cCS[i] <= 'F') || (cCS[i] >= 'a' && cCS[i] <= 'f'))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        static byte[] Key1 = { 0x9d, 0x87, 0xfa, 0x6a, 0x80, 0x5b, 0xcc, 0x77, 0x49, 0x6c, 0xd7, 0x32, 0x40, 0xbc, 0x08, 0x68 };
        //把明文字符串(8字节） 加密 成密文（16进制字符串表示，16字节）
        public static string OpenString_To_HideString(string OpenString, string szKey)
        {
            byte[] bS1 = new byte[8];
            byte[] bT1 = new byte[8];
            byte[] bK1 = new byte[8];
            string bResult;

            if (OpenString.Length != 8)
            {
                bResult = "";
            }
            else
            {
                bS1 = System.Text.Encoding.Default.GetBytes(OpenString);
                if (szKey != "" && szKey.Length == 16)
                    bK1 = System.Text.Encoding.Default.GetBytes(szKey);
                else
                    Array.Copy(Key1, 0, bK1, 0, 8);

                ThreeDesEncryptByte(bK1, bS1, ref bT1);
                bResult = ByteToString(bT1, 0, 8);
            }
            return bResult;
        }


        //把密文（16进制字符串表示的16个字节）解密成 明文字符串（8字节）
        public static string HideString_To_OpenString(string HideString, string szKey)
        {
            byte[] bS1 = new byte[8];
            byte[] bT1 = new byte[8];
            byte[] bK1 = new byte[8];
            string bResult;

            if (HideString.Length != 16 || !StringIsHex(HideString))
            {
                bResult = "";
            }
            else
            {
                StringToByte(HideString, bS1, 0, 8);
                if (szKey != "" && szKey.Length == 16)
                    bK1 = System.Text.Encoding.Default.GetBytes(szKey);
                else
                    Array.Copy(Key1, 0, bK1, 0, 8);

                ThreeDesDecryptByte(bK1, bS1, ref bT1);
                bResult = "";
                for (int i = 0; i < bT1.Length; i++)
                {
                    bResult += Convert.ToChar(bT1[i]);
                }
            }
            return bResult;
        }

        private static bool TriDesEncrypt(byte[] ucKey, byte[] ucSrc, ref byte[] ucDst)
        {//key len=8,DES;key len=16,3DES
            try
            {
                ICryptoTransform ct;
                MemoryStream ms;
                CryptoStream cs;
                SymmetricAlgorithm mCSP;

                if (ucKey.Length == 8)
                    mCSP = new DESCryptoServiceProvider();
                else if (ucKey.Length == 16)
                    mCSP = new TripleDESCryptoServiceProvider();
                else
                    return false;

                byte[] byt = ucSrc;
                //指定初始化向量
                //mCSP.IV = Convert.FromBase64String(sIV);  原程序
                //string szIV = "\x0\x0\x0\x0\x0\x0\x0\x0";
                mCSP.IV = System.Text.Encoding.Default.GetBytes(szIV);

                //指定加密的运算模式--电子密码本 (ECB) 模式分别加密每个块
                mCSP.Mode = System.Security.Cryptography.CipherMode.ECB;

                //获取或设置加密算法的填充模式
                mCSP.Padding = System.Security.Cryptography.PaddingMode.Zeros;

                //mCSP.Key = ucKey;
                //ct = mCSP.CreateEncryptor(mCSP.Key, mCSP.IV);                      //创建加密对象
                //以下方法是绕过弱密钥的检测方法，先用反编译找到结点，再用反射绕过
                //详见网页http://www.cnblogs.com/jintianhu/archive/2011/11/26/2264375.html
                Type t = Type.GetType("System.Security.Cryptography.CryptoAPITransformMode");
                object obj = t.GetField("Encrypt", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).GetValue(t);
                MethodInfo mi = mCSP.GetType().GetMethod("_NewEncryptor", BindingFlags.Instance | BindingFlags.NonPublic);
                ct = (ICryptoTransform)mi.Invoke(mCSP, new object[] { ucKey, mCSP.Mode, null, 0, obj });

                ms = new MemoryStream();
                cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                cs.Close();

                //TargetValue = Convert.ToBase64String(ms.ToArray());  // 原程序
                ucDst = ms.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                string szMsg = ex.ToString();
                return false;
            }
        }
        private static bool TriDesDecrypt(byte[] ucKey, byte[] ucSrc, ref byte[] ucDst)
        {
            try
            {
                ICryptoTransform ct;            //加密转换运算
                MemoryStream ms;                //内存流
                CryptoStream cs;                //数据流连接到数据加密转换的流
                SymmetricAlgorithm mCSP;

                if (ucKey.Length == 8)
                    mCSP = new DESCryptoServiceProvider();
                else if (ucKey.Length == 16)
                    mCSP = new TripleDESCryptoServiceProvider();
                else
                    return false;

                byte[] byt = ucSrc;

                //指定初始化向量
                //mCSP.IV = Convert.FromBase64String(sIV);  原程序
                mCSP.IV = System.Text.Encoding.Default.GetBytes(szIV);

                //指定加密的运算模式--电子密码本 (ECB) 模式分别加密每个块
                mCSP.Mode = System.Security.Cryptography.CipherMode.ECB;
                //获取或设置加密算法的填充模式
                mCSP.Padding = System.Security.Cryptography.PaddingMode.Zeros;

                //mCSP.Key = ucKey;
                //ct = mCSP.CreateDecryptor(mCSP.Key, mCSP.IV);                     //创建对称解密对象
                //以下方法是绕过弱密钥的检测方法，先用反编译找到结点，再用反射绕过
                //详见网页http://www.cnblogs.com/jintianhu/archive/2011/11/26/2264375.html
                Type t = Type.GetType("System.Security.Cryptography.CryptoAPITransformMode");
                object obj = t.GetField("Decrypt", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).GetValue(t);
                MethodInfo mi = mCSP.GetType().GetMethod("_NewEncryptor", BindingFlags.Instance | BindingFlags.NonPublic);
                ct = (ICryptoTransform)mi.Invoke(mCSP, new object[] { ucKey, mCSP.Mode, null, 0, obj });

                ms = new MemoryStream();
                cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                cs.Close();

                ucDst = ms.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                String szMsg = ex.Message;
                return false;
            }
        }
        // 3Des功能说明：
        // 1、密钥 byte[] sKey：长度必须是16字节。
        // 2、数据源 byte[] SourceValue：必须是8的倍数，由调用者封装，总长度由系统性能决定。
        // 3、定义向量，必须是8个字符
        //const string sIV = "W&L-ZZZS";
        const string szIV = "\x0\x0\x0\x0\x0\x0\x0\x0";

        //加密
        public static string ThreeDesEncryptHex(string szSrc, string szKey)
        {
            if (!StringIsHex(szSrc))
                return "";

            int iLen = szSrc.Length;
            if (iLen % 16 != 0)
                return "";
            iLen = iLen / 2;

            int iKeyLen = szKey.Length;
            if (iKeyLen % 16 != 0)
                return "";
            iKeyLen = iKeyLen / 2;

            //StringBuilder szDst = new StringBuilder(iLen);
            //TriDesEncrypt_HEX(szKey, szSrc, szDst);
            //return szDst.ToString().Substring(0,iLen);
            byte[] ucSrc = new byte[iLen];
            byte[] ucKey = new byte[iKeyLen];
            byte[] ucDst = new byte[iLen];
            StringToByte(szSrc, ucSrc, 0, iLen);
            StringToByte(szKey, ucKey, 0, iKeyLen);
            if (!ThreeDesEncryptByte(ucKey, ucSrc, ref ucDst))
                return "";

            return ByteToString(ucDst, 0, iLen);
        }

        public static string ThreeDesDecryptHex(string szSrc, string szKey)
        {
            if (!StringIsHex(szSrc))
                return "";

            int iLen = szSrc.Length;
            if (iLen % 16 != 0)
                return "";
            iLen = iLen / 2;

            int iKeyLen = szKey.Length;
            if (iKeyLen % 16 != 0)
                return "";
            iKeyLen = iKeyLen / 2;

            //StringBuilder szDst = new StringBuilder(iLen);
            //TriDesDecrypt_HEX(szKey, szSrc, szDst);                    
            //return szDst.ToString();
            byte[] ucSrc = new byte[iLen];
            byte[] ucKey = new byte[iKeyLen];
            byte[] ucDst = new byte[iLen];
            StringToByte(szSrc, ucSrc, 0, iLen);
            StringToByte(szKey, ucKey, 0, iKeyLen);
            if (!ThreeDesDecryptByte(ucKey, ucSrc, ref ucDst))
                return "";

            return ByteToString(ucDst, 0, iLen);
        }


        public static bool ThreeDesEncryptByte(byte[] sKey, byte[] SourceValue, ref byte[] TargetValue)
        {//key len=8,DES;key len=16,3DES
            try
            {
                ICryptoTransform ct;
                MemoryStream ms;
                CryptoStream cs;
                SymmetricAlgorithm mCSP;

                if (sKey.Length == 8)
                    mCSP = new DESCryptoServiceProvider();
                else if (sKey.Length == 16)
                    mCSP = new TripleDESCryptoServiceProvider();
                else
                    return false;

                byte[] byt;
                //mCSP.KeySize = 128;
                mCSP.Key = sKey;

                //指定初始化向量
                //mCSP.IV = Convert.FromBase64String(sIV);  原程序
                string szIV = "\x0\x0\x0\x0\x0\x0\x0\x0";
                mCSP.IV = System.Text.Encoding.Default.GetBytes(szIV);

                //指定加密的运算模式--电子密码本 (ECB) 模式分别加密每个块
                mCSP.Mode = System.Security.Cryptography.CipherMode.ECB;

                //获取或设置加密算法的填充模式
                mCSP.Padding = System.Security.Cryptography.PaddingMode.Zeros;

                ct = mCSP.CreateEncryptor(mCSP.Key, mCSP.IV);                      //创建加密对象
                //byt = Encoding.UTF8.GetBytes(SourceValue);
                byt = SourceValue;

                ms = new MemoryStream();
                cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                cs.Close();

                //TargetValue = Convert.ToBase64String(ms.ToArray());  // 原程序
                TargetValue = ms.ToArray();
                return true;
            }
            catch //(Exception ex)
            {
                //MessageBox.Show("加密算法出错" + ex.Message, "出现异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        public static bool ThreeDesDecryptByte(byte[] sKey, byte[] SourceValue, ref byte[] TargetValue)
        {
            try
            {
                ICryptoTransform ct;            //加密转换运算
                MemoryStream ms;                //内存流
                CryptoStream cs;                //数据流连接到数据加密转换的流
                SymmetricAlgorithm mCSP;

                if (sKey.Length == 8)
                    mCSP = new DESCryptoServiceProvider();
                else if (sKey.Length == 16)
                    mCSP = new TripleDESCryptoServiceProvider();
                else
                    return false;

                byte[] byt;
                //mCSP.KeySize = 128;
                mCSP.Key = sKey;

                //指定初始化向量
                //mCSP.IV = Convert.FromBase64String(sIV);  原程序
                string szIV = "\x0\x0\x0\x0\x0\x0\x0\x0";
                mCSP.IV = System.Text.Encoding.Default.GetBytes(szIV);

                //指定加密的运算模式--电子密码本 (ECB) 模式分别加密每个块
                mCSP.Mode = System.Security.Cryptography.CipherMode.ECB;

                //获取或设置加密算法的填充模式
                mCSP.Padding = System.Security.Cryptography.PaddingMode.Zeros;

                ct = mCSP.CreateDecryptor(mCSP.Key, mCSP.IV);                     //创建对称解密对象
                //byt = Convert.FromBase64String(SourceValue);
                byt = SourceValue;

                ms = new MemoryStream();
                cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();
                cs.Close();

                TargetValue = ms.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                String szMsg = ex.Message;
                //MessageBox.Show(ex.Message, "出现异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        public static int TriDes_Hex(int iMode, string szSrc, string szKey, out string szDst)
        {//ucMode=1,encrypt;=2,decrypt
            Int16 iKeyLen = (Int16)(szKey.Length / 2);
            Int32 iSrcLen = szSrc.Length / 2;
            byte[] ucSrc = new byte[iSrcLen];
            byte[] ucKey = new byte[iKeyLen];
            byte[] ucDst = new byte[iSrcLen];
            StringToByte(szSrc, ucSrc, 0, iSrcLen);
            StringToByte(szKey, ucKey, 0, iKeyLen);
            szDst = "";
            switch (iMode)
            {
                case 1:
                    //TriDesEncrypt(ref ucKey[0], iKeyLen, ref ucSrc[0], iSrcLen, ref ucDst[0]);
                    TriDesEncrypt(ucKey, ucSrc, ref ucDst);
                    break;
                case 2:
                    //TriDesDecrypt(ref ucKey[0], iKeyLen, ref ucSrc[0], iSrcLen, ref ucDst[0]);
                    TriDesDecrypt(ucKey, ucSrc, ref ucDst);
                    break;
                default:
                    return 0;
            }
            szDst = ByteToString(ucDst, 0, iSrcLen);
            return iSrcLen;
        }


        /// 写入日志文件功能
        /// </summary>
        /// <param name="input"></param>
        private static object Log_ForLock = new object();
        static int TryLockTime = 5000;                      // ms
        public static bool WriteLogFile(string ModuleName, string sMsg)
        {
            //bool bLockResult = false;
            bool bResult = false;
            string sTmp = "";

            try
            {
                ///指定日志文件的目录
                string fname = Directory.GetCurrentDirectory() + "/Log/LogFile" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                Monitor.TryEnter(Log_ForLock, TryLockTime);

                //Monitor.TryEnter(syncRoot, TryLockTime, ref bLockResult);
                //if (bLockResult)
                {
                    ///定义文件信息对象
                    if (!File.Exists(fname))
                    {
                        FileStream fs = File.Create(fname);
                        fs.Close();
                    }
                    FileInfo finfo = new FileInfo(fname);
                    //if (!finfo.Exists)
                    //{
                    //    FileStream fs;
                    //    fs = File.Create(fname);
                    //    fs.Close();
                    //    finfo = new FileInfo(fname);
                    //}

                    ///判断文件是否存在以及是否大于2K
                    if (finfo.Length > 1024 * 1024 * 10)
                    {
                        ///文件超过10MB则重命名
                        File.Move(fname, Directory.GetCurrentDirectory() + "\\Log\\LogFile" + DateTime.Now.ToString("yyyyMMddHH") + ".txt");
                        finfo = new FileInfo(fname);
                    }
                    //finfo.AppendText();
                    ///创建只写文件流
                    using (FileStream fs = finfo.OpenWrite())
                    {
                        ///根据上面创建的文件流创建写数据流
                        StreamWriter w = new StreamWriter(fs);
                        ///设置写数据流的起始位置为文件流的末尾
                        w.BaseStream.Seek(0, SeekOrigin.End);

                        ///写入“Log Entry : ”
                        ///写入当前系统时间并换行
                        sTmp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " " + ModuleName + ":";
                        w.Write(sTmp + Environment.NewLine);

                        ///写入日志内容并换行
                        w.Write(sMsg + Environment.NewLine);

                        ///写入------------------------------------并换行
                        //w.Write("---------------------------------------------------" + Environment.NewLine);

                        ///清空缓冲区内容，并把缓冲区内容写入基础流
                        w.Flush();

                        ///关闭写数据流
                        w.Close();
                    }

                    bResult = true;
                }
                Monitor.Exit(Log_ForLock);
            }
            catch (Exception ex)
            {
            }
            return bResult;
        }

        /*
        public static bool ExportDataGridview(DataGridView gridView, bool isShowExcle)
        {
            if (gridView.Rows.Count == 0)
            {
                return false;
            }
            //建立Excel对象
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            excel.Application.Workbooks.Add(true);
            excel.Visible = isShowExcle;

            //生成字段名称
            for (int i = 0; i < gridView.ColumnCount; i++)
            {
                excel.Cells[1, i + 1] = gridView.Columns[i].HeaderText;
            }
            //填充数据
            for (int i = 0; i < gridView.RowCount - 1; i++)
            {
                for (int j = 0; j < gridView.ColumnCount; j++)
                {
                    if (gridView[j, i].ValueType == typeof(string))
                    {
                        excel.Cells[i + 2, j + 1] = "'" + gridView[j, i].Value.ToString();
                    }
                    else
                    {
                        excel.Cells[i + 2, j + 1] = gridView[j, i].Value.ToString();
                    }
                }
            }
            return true;
        }
        */


        public static void SetSystemTime(DateTime ThisTime)
        {
            SystemTime MySystemTime = new SystemTime();

            MySystemTime.vYear = (ushort)ThisTime.Year;
            MySystemTime.vMonth = (ushort)ThisTime.Month;
            MySystemTime.vDay = (ushort)ThisTime.Day;
            MySystemTime.vHour = (ushort)ThisTime.Hour;
            MySystemTime.vMinute = (ushort)ThisTime.Minute;
            MySystemTime.vSecond = (ushort)ThisTime.Second;

            SystemTime.SetLocalTime(MySystemTime);

        }
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public class SystemTime
        {
            [DllImportAttribute("Kernel32.dll")]
            public static extern void GetLocalTime(SystemTime st);

            [DllImportAttribute("Kernel32.dll")]
            public static extern void SetLocalTime(SystemTime st);

            public ushort vYear;
            public ushort vMonth;
            public ushort vDayOfWeek;
            public ushort vDay;
            public ushort vHour;
            public ushort vMinute;
            public ushort vSecond;
        }

        public static void ShowMsg(string sResult, string sCause)
        {
            string sTmp = sResult;

            if (sCause != "")
            {
                sTmp += "\r\n" + "错误原因/Error cause = " + sCause;
            }

            MessageBox.Show(sTmp, "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static int GetStringCode(string sString)
        {
            int iR = 0;
            if (sString.Length == 0)
                return iR;

            byte[] bTmp = System.Text.Encoding.Default.GetBytes(sString);
            for (int i = 0; i < bTmp.Length; i++)
            {
                iR += (sbyte)bTmp[i];
            }
            return iR;
        }

        public static string ReadFileLine(BinaryReader br)
        {
            byte[] Data = new byte[4096];
            int DataLen = 0;
            byte c = 0;
            byte d = 0;

            string ReturnString = "";

            while (true)
            {
                if (br.BaseStream.Position < (br.BaseStream.Length - 1))
                {
                    c = br.ReadByte();
                    if (c == 0x0A)
                    {
                        if (br.BaseStream.Position < (br.BaseStream.Length - 1))
                        {
                            d = br.ReadByte();
                            if (d == 0x0D)
                                break;
                            else
                                br.BaseStream.Seek(-1, SeekOrigin.Current);
                        }
                        break;
                    }
                    else if (c == 0x0D)
                    {
                        if (br.BaseStream.Position < (br.BaseStream.Length - 1))
                        {
                            d = br.ReadByte();
                            if (d == 0x0A)
                                break;
                            else
                                br.BaseStream.Seek(-1, SeekOrigin.Current);
                        }
                        break;
                    }
                    else
                    {
                        Data[DataLen++] = c;
                    }
                    if (DataLen >= Data.Length) break;
                }
                else
                {
                    break;
                }
            }

            if (DataLen > 0)
                ReturnString = System.Text.Encoding.Default.GetString(Data, 0, DataLen);

            return ReturnString;
        }

        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(delegate (object f)
                {
                    ((DispatcherFrame)f).Continue = false;

                    return null;
                }
                    ), frame);
            Dispatcher.PushFrame(frame);
        }

    }
}
