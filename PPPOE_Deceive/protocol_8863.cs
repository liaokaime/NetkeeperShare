using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cn.softname2.PPPOE_Deceive
{
    class protocol8863
    {
        byte[] distinationMac = null;    //目的mac
        byte[] sourceMac = null;       //源mac
        byte[] packetType = null;      //报文协议
        byte[] versions = null;        //pppoe版本,note:一般必须为11,,此数据由该类自动填充
        byte[] discoveryStage = null;  //pppoe发现阶段
        byte[] sessionID = null;       //会话ID
        byte[] payloadLength = null;   //pppoe数据总长度(不包括以上字段),此数据由该类自动填充
        List<byte[]> payloadAnInfo = new List<byte[]>();    //挂载总数据的单条数据集合
        byte[] payloadData = null;     //挂载的具体数据

        support sup = new support();
        //获取报文的版本号
        public byte[] getVersions(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 14, 1);
            }
            catch
            {
                Console.WriteLine("getVersions() : 未知错误");
                return null;
            }
        }

        //获取报文的发现阶段
        public byte[] getDiscoveryStage(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 15, 1);
            }
            catch
            {
                Console.WriteLine("getDiscoveryStage() : 未知错误");
                return null;
            }
        }

        //获取报文的session
        public byte[] getSessionID(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 16, 2);
            }
            catch
            {
                Console.WriteLine("getSessionID() : 未知错误");
                return null;
            }
        }

        //获取pppoe搭载数据长度
        public byte[] getpayloadLength(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 18, 2);
            }
            catch
            {
                Console.WriteLine("getpayloadLength() : 未知错误");
                return null;
            }
        }

        //获取数据title为Host-Uniq（主机唯一标识）
        public byte[] getData_HostUniq(byte[] pac)
        {
            try
            {
                byte[] hostUniqToString = sup.toByte(new String[] { "01", "01", "00", "00", "01", "03" });
                int loca = sup.byteIndexOf(pac, hostUniqToString);
                if (loca == -1)
                    return null;
                byte[] hostUniqLengthByte = sup.byteSub(pac, loca + hostUniqToString.Length, 2);
                int hostUniqLength = hostUniqLengthByte[0] * 256 + hostUniqLengthByte[1];
                return sup.byteSub(pac, loca + hostUniqToString.Length + 2, hostUniqLength);
            }
            catch
            {
                Console.WriteLine("getData_HostUniq() : 未知错误");
                return null;
            }
        }



        //设置报文的目的mac
        public void setDistinationMac(byte[] bytes)
        {
            if (bytes.Length != 6)
            {
                Console.WriteLine("setDistinationMac() : Mac地址不合法");
                return;
            }
            distinationMac = bytes;
        }

        //设置报文源Mac地址
        public void setSourceMac(byte[] bytes)
        {
            if (bytes.Length != 6)
            {
                Console.WriteLine("setSourceMac() : Mac地址不合法");
                return;
            }
            sourceMac = bytes;
        }

        //设置报文协议
        private void setProtocol(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                Console.WriteLine("setsetProtocol() : 协议代码长度不合法");
                return;
            }
            packetType = bytes;
        }

        //设置PPPOE版本
        private void setVersions(byte[] bytes)
        {
            if (bytes.Length != 1)
            {
                Console.WriteLine("setversions() : 版本代码长度不合法");
                return;
            }
            versions = bytes;
        }

        //设置发现阶段        PADI/09, PADO/07, PADR/09, PADS/65 (16hexValue)
        public void setDiscoveryStage(byte[] bytes)
        {
            if (bytes.Length != 1)
            {
                Console.WriteLine("setDiscoveryStage() : 发现阶段代码长度不合法");
                return;
            }
            discoveryStage = bytes;
        }

        //设置sessionID
        public void setSessionID(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                Console.WriteLine("setSessionID : sessionID代码长度不合法");
                return;
            }
            sessionID = bytes;
        }

        //设置title为Host-Uniq（主机唯一标识）的单条数据  note:传入Host-Uniq的值即可
        public void setData_HostUniq(byte[] bytes)
        {
            try
            {
                byte[] title = sup.toByte(new String[] { "01", "01", "00", "00", "01", "03" });
                int infoLength = bytes.Length;
                byte[] lengthByte = new byte[2];
                lengthByte[1] = (byte)(infoLength % 256);
                lengthByte[0] = (byte)(infoLength / 256);
                payloadAnInfo.Add(sup.byteAppent(sup.byteAppent(title, lengthByte), bytes));
            }
            catch
            {
                Console.WriteLine("setData_HostUniq() : 未知错误");
            }
        }

        //设置其他Title + Length + Data组成的单条数据的byte[]形式
        public void setData_other(byte[] bytes)
        {
            try
            {
                payloadAnInfo.Add(bytes);
            }
            catch
            {
                Console.WriteLine("setData_other() : 未知错误");
            }
        }

        //将List<byte[]> payloadAnInfo排序 (顺便把排序完成的东西赋值到payloadData数组)
        public void sortPayloadAnInfo()
        {
            payloadAnInfo.Sort(sortRule);
            for (int tm = 0; tm < payloadAnInfo.Count; tm++)
            {
                payloadData = sup.byteAppent(payloadData, payloadAnInfo[tm]);
            }
        }

        //List排序方式,请测试升序或降序，要求升序
        private int sortRule(byte[] a, byte[] b)
        {
            if (a[0] > b[0])
                return 1;
            else if (a[0] < b[0])
                return -1;
            else
            {
                if (a[1] > b[1])
                    return 1;
                else if (a[1] < b[1])
                    return -1;
                else
                    return 0;
            }
        }

        //设置payload数据
        public void setPayloadData(byte[] bytes)
        {
            payloadData = bytes;
        }

        //获取总报文
        public byte[] getAllPacketData()
        {

            try
            {
                if (payloadData == null)        //当PPPOE Tags数据未被总数据填写时
                    sortPayloadAnInfo();    //将List<byte[]> payloadAnInfo排序 (顺便把排序完成的东西赋值到payloadData数组)

                if (distinationMac == null || sourceMac == null || discoveryStage == null || sessionID == null)
                {
                    Console.WriteLine("8863报文不完整");
                    return null;
                }
                versions = sup.toByte(new String[] { "11" });                   //pppoe版本为固定值
                int payloadLength_i = -1;
                if (payloadData == null)        //如果传入payloadData为null,则长度为0
                    payloadLength_i = 0;
                else
                    payloadLength_i = payloadData.Length;

                payloadLength = new byte[2];
                payloadLength[1] = (byte)(payloadLength_i % 256);
                payloadLength[0] = (byte)(payloadLength_i / 256);                     //写入pppoe挂载数据总长度
                byte[] append1 = sup.byteAppent(distinationMac, sourceMac);
                packetType = sup.toByte(new string[] { "88", "63" });             //自动设置包协议
                byte[] append2 = sup.byteAppent(packetType, versions);
                byte[] append3 = sup.byteAppent(discoveryStage, sessionID);
                byte[] append4 = sup.byteAppent(payloadLength, payloadData);
                byte[] append5 = sup.byteAppent(append1, append2);
                byte[] append6 = sup.byteAppent(append3, append4);
                return sup.byteAppent(append5, append6);
            }
            catch
            {
                Console.Write("getAllPacketData() : 未知错误");
                return null;
            }
        }

    } //end of class protocol8863
}
