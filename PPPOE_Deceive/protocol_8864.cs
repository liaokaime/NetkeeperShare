using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cn.softname2.PPPOE_Deceive
{

    class protocol8864
    {
        byte[] distinationMac = null;   //目的mac
        byte[] sourceMac = null;        //源mac
        byte[] packetType = null;       //报文协议,自动填充
        byte[] versions = null;         //pppoe版本,note:一般必须为11,此数据由该类自动填充
        byte[] sessionData = null;      //一个固定参数,0x00
        byte[] sessionID = null;        //会话ID
        byte[] payloadLength = null;    //pppoe数据总长度(不包括以上字段),此数据由该类自动填充


        byte[] PPP_allData = null;
        byte[] PPP = null;              //Point to Point Protlcol   note:PPP-LCP代号0xc021    /PPP-CHAP代号0xc223    /PPP-PAP代号0xc023   /其它还有
        byte[] PPPLCP_config = null;    //PPP-LCP形态 01/request, 02/Ask, 04/Reject, 12/Identification
        byte[] PPPLCP_identifier = null;    //对话标识符
        byte[] PPPLCP_length = null;        //PPPLCP层数据长度
        List<byte[]> optionsAnInfo = new List<byte[]>();    //PPPLCP的options挂载的单条数据的集合
        byte[] PPPLCP_optionsData = null;   //PPPLC层搭载的数据


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

        //获取报文的sessionData
        public byte[] getSessionData(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 15, 1);
            }
            catch
            {
                Console.WriteLine("getSessionData() : 未知错误");
                return null;
            }
        }

        //获取报文的sessionID
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

        //获取报文搭载的总数据长度
        public byte[] getPayloadLength(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 18, 2);
            }
            catch
            {
                Console.WriteLine("getPayloadLength() : 未知错误");
                return null;
            }
        }

        //获取PPP类型
        public byte[] getPPP(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 20, 2);
            }
            catch
            {
                Console.WriteLine("getPPP() : 未知错误");
                return null;
            }
        }

        //获取PPPLCP形态    01/request, 02/Ask, 04/Reject, 12/Identification
        public byte[] getPPPLCP_config(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 22, 1);
            }
            catch
            {
                Console.WriteLine("getPPPLCP_config() : 未知错误");
                return null;
            }
        }

        //获取PPPLCP对话标识符
        public byte[] getPPPLCP_identifier(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 23, 1);
            }
            catch
            {
                Console.WriteLine("getPPPLCP_identifier() : 未知错误");
                return null;
            }
        }

        //获取PPPLCP层数据长度
        public byte[] getPPPLCP_length(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 24, 2);
            }
            catch
            {
                Console.WriteLine("getPPPLCP_length() : 未知错误");
                return null;
            }
        }

        //获取PPPLCP_Options数据
        public byte[] getPPPLCP_optionsData(byte[] pac)
        {
            try
            {
                return sup.byteSub(pac, 26, pac.Length - 26);
            }
            catch
            {
                Console.WriteLine("getPPPLCP_optionsData() : 未知错误");
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
        public void setVersions(byte[] bytes)
        {
            if (bytes.Length != 1)
            {
                Console.WriteLine("setversions() : 版本代码长度不合法");
                return;
            }
            versions = bytes;
        }

        //自动设置Session Code 0x00

        //设置SessionID
        public void setSessionID(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                Console.WriteLine("setSessionID() : SessionID代码长度不合法");
                return;
            }
            sessionID = bytes;
        }

        //自动设置payloadLength




        //设置PPP;    PPP-LCP代号0xc021    /PPP-CHAP代号0xc223    /PPP-PAP代号0xc023   /其它还有
        public void setPPP(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                Console.WriteLine("setPPP() : PPP代码长度不合法");
                return;
            }
            PPP = bytes;
        }

        //设置PPP_allData
        public void setPPP_allData(byte[] bytes)
        {
            PPP_allData = bytes;
        }

        //设置PPP_LCP_config（//PPP-LCP形态 01/request, 02/Ask, 04/Reject, 12/Identification）
        public void setPPPLCP_config(byte[] bytes)
        {
            if (bytes.Length != 1)
            {
                Console.WriteLine("setPPPLCP_config() : PPPLCP_config代码长度不合法");
                return;
            }
            PPPLCP_config = bytes;
        }

        //设置PPPLCP_identifier(对话标识符)
        public void setPPPLCP_identifier(byte[] bytes)
        {
            if (bytes.Length != 1)
            {
                Console.WriteLine("setPPPLCP_identifier() : PPPLCP_identifier代码长度不合法");
                return;
            }
            PPPLCP_identifier = bytes;
        }

        //自动设置byte[] PPPLCP_length

        //设置PPPLCP_optionsData总数据（设置optionsData总数据函数优先级比设置单条option集合更高，一旦设置了option总数据，单条数据便不会生效）
        public void setPPPLCP_optionsData(byte[] bytes)
        {
            PPPLCP_optionsData = bytes;
        }

        //设置单条PPPLCP_Options数据（设置optionsData总数据函数优先级比该函数优先级高，一旦设置了option总数据，此函数做的所有操作便不会生效）
        public void setPPPLCP_optionsAnInfo(byte[] bytes)
        {
            optionsAnInfo.Add(bytes);
        }






        //获取PPP_Data块数据为用户自定义的8864包
        public byte[] getAllPacketData_custom()
        {
            if (distinationMac == null || sourceMac == null || sessionID == null || PPP_allData == null)
            {
                Console.WriteLine("需要构造的自定义PPP_X的数据包的参数还没有赋值完");
                return null;
            }

            sortPayloadAnInfo();    //将List<byte[]> 排序 (顺便把排序完成的东西赋值到PPPLCP_optionsData数组)
            byte[] append1 = sup.byteAppent(distinationMac, sourceMac);
            versions = sup.toByte(new string[] { "11" });           //自动填充pppoe版本为0x11
            packetType = sup.toByte(new string[] { "88", "64" });     //自动设置包协议
            byte[] append2 = sup.byteAppent(packetType, versions);
            sessionData = sup.toByte(new string[] { "00" });         //自动设置sessionData
            byte[] append3 = sup.byteAppent(sessionData, sessionID);

            int payloadLength_i = PPP_allData.Length;
            payloadLength = new byte[2];
            payloadLength[0] = (byte)(payloadLength_i / 256);
            payloadLength[1] = (byte)(payloadLength_i % 256);


            byte[] append4 = sup.byteAppent(payloadLength, PPP_allData);
            byte[] append5 = sup.byteAppent(append1, append2);
            byte[] append6 = sup.byteAppent(append3, append4);


            return sup.byteAppent(append5, append6);

        }



        //获取标准的PPPLCP数据包
        public byte[] getAllPacketData_PPPLCP()
        {
            if (!(PPP[0] == Convert.ToByte("c0", 16) && PPP[1] == Convert.ToByte("21", 16)))
            {
                Console.WriteLine("getAllPacket_custom : 你构造的数据包不是标准PPPLCP数据包，如需构造自定义包，请使用getAllPacket_custom()函数");
                return null;
            }
            if (PPP_allData != null)
            {
                Console.WriteLine("PPP_allData字段(PPP_Data块数据已经被自定义),你应该使用使用getAllPacket_custom()获取本数据包");
                return null;
            }
            if (distinationMac == null || sourceMac == null || sessionID == null || PPP == null || PPPLCP_config == null || PPPLCP_identifier == null)
            {
                Console.WriteLine("需要构造标准的PPPLCP数据包的参数还没有赋值完");
                return null;
            }

            sortPayloadAnInfo();    //将List<byte[]> 排序 (顺便把排序完成的东西赋值到PPPLCP_optionsData数组)
            byte[] append1 = sup.byteAppent(distinationMac, sourceMac);
            versions = sup.toByte(new string[] { "11" });           //自动填充pppoe版本为0x11
            packetType = sup.toByte(new string[] { "88", "64" });     //自动设置包协议
            byte[] append2 = sup.byteAppent(packetType, versions);
            sessionData = sup.toByte(new string[] { "00" });         //自动设置sessionData
            byte[] append3 = sup.byteAppent(sessionData, sessionID);

            int payloadLength_i = PPP.Length + PPPLCP_config.Length + PPPLCP_identifier.Length + PPPLCP_optionsData.Length + 2;
            payloadLength = new byte[2];
            payloadLength[0] = (byte)(payloadLength_i / 256);
            payloadLength[1] = (byte)(payloadLength_i % 256);

            int PPPLCP_length_i = PPPLCP_optionsData.Length + PPPLCP_config.Length + PPPLCP_identifier.Length + 2;
            PPPLCP_length = new byte[2];
            PPPLCP_length[0] = (byte)(PPPLCP_length_i / 256);
            PPPLCP_length[1] = (byte)(PPPLCP_length_i % 256);
            byte[] PPP_allData1 = sup.byteAppent(PPP, PPPLCP_config);
            byte[] PPP_allData2 = sup.byteAppent(PPPLCP_identifier, PPPLCP_length);
            PPP_allData = sup.byteAppent(PPP_allData1, PPP_allData2);
            PPP_allData = sup.byteAppent(PPP_allData, PPPLCP_optionsData);

            byte[] append4 = sup.byteAppent(payloadLength, PPP_allData);
            byte[] append5 = sup.byteAppent(append1, append2);
            byte[] append6 = sup.byteAppent(append3, append4);


            return sup.byteAppent(append5, append6);
        }

        //将List<byte[]> payloadAnInfo排序 (顺便把排序完成的东西赋值到PPPLCP_optionsData数组)
        public void sortPayloadAnInfo()
        {
            optionsAnInfo.Sort(sortRule);
            for (int tm = 0; tm < optionsAnInfo.Count; tm++)
            {
                PPPLCP_optionsData = sup.byteAppent(PPPLCP_optionsData, optionsAnInfo[tm]);
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



    }

}
