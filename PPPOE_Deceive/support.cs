using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace cn.softname2.PPPOE_Deceive
{

    class support
    {
        //两个byte数组叠加
        public byte[] byteAppent(byte[] a, byte[] b)
        {
            if (a == null && b == null)
                return null;
            else if (a == null)
                return b;
            else if (b == null)
                return a;
            byte[] bytes = new byte[a.Length + b.Length];
            int pointLoca = 0;
            foreach (byte tm in a)
            {
                bytes[pointLoca] = tm;
                pointLoca++;
            }
            foreach (byte tm2 in b)
            {
                bytes[pointLoca] = tm2;
                pointLoca++;
            }
            return bytes;
        }



        //截取byte数组的子数组
        public byte[] byteSub(byte[] a, int start, int length)
        {
            byte[] bytes = new byte[length];
            int pointLoca = 0;
            for (int tm = start; tm < start + length; tm++)
            {
                bytes[pointLoca] = a[tm];
                pointLoca++;
            }
            return bytes;
        }


        //将16进制 字符串数组 转换为byte
        public byte[] toByte(String[] a)
        {
            byte[] btm = new byte[a.Length];
            for (int i = 0; i < btm.Length; i++)
            {
                btm[i] = Convert.ToByte(a[i], 16);
            }
            return btm;
        }


        //byte[] 数组内容对比
        public bool equalsByte(byte[] a, byte[] b)
        {
            if (a.Length == b.Length)
            {
                for (int c = 0; c < a.Length; c++)
                {
                    if (a[c] != b[c])
                        return false;
                }
                return true;
            }
            return false;
        }



        //byteIndexOf(已优化算法)
        public int byteIndexOf(byte[] source, byte[] search)
        {
            byte c = 0;
            for (int d = 0; d < source.Length; d++)
            {
                if (search[c] == source[d])
                {
                    c++;
                }
                else
                {
                    c = 0;
                    if (search[c] == source[d])
                    {
                        c++;
                    }
                }
                if (c == search.Length)
                    return d - c + 1;
            }
            return -1;
        }



        //打印byte数组的String形式
        public void showByte(byte[] a)
        {
            int c = 0;
            foreach (byte b in a)
            {
                Console.Write(Convert.ToString(b, 16) + " ");
                c++;
                if (c % 16 == 0)
                    Console.WriteLine();
            }
        }

        //获取当前活动网卡的Mac
        public string GetMacAddress()
        {
            try
            {
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return null;
            }
            finally
            {
            }
        }

        //将未隔阂的Mac地址每两个字符之间加一个 '-' 号
        public String processMac(String Mac)
        {
            if (Mac == null)
            {
                Console.WriteLine("processMac() : Mac is NULL");
                return null;
            }
            //每两个字符间加一个"-"号
            if (Mac.Length == 2)
                return Mac;
            return Mac.Substring(0, 2) + ":" + processMac(Mac.Substring(2, Mac.Length - 2));
        }




        //获取报文的目的Mac (8863 || 8864协议的byte[])
        public byte[] getDistinationMac(byte[] pac)
        {
            try
            {
                return byteSub(pac, 0, 6);
            }
            catch
            {
                return null;
            }
        }

        //获取报文的源Mac (8863 || 8864协议的byte[])
        public byte[] getSourceMac(byte[] pac)
        {
            try
            {
                return byteSub(pac, 6, 6);
            }
            catch
            {
                return null;
            }
        }

        //获取报文的协议 (8863 || 8864协议的byte[])
        public byte[] getPacketType(byte[] pac)
        {
            try
            {
                return byteSub(pac, 12, 2);
            }
            catch
            {
                return null;
            }
        }




    }

}
