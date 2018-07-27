using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpPcap;
using System.Collections;
using System.Threading;
using System.Net.NetworkInformation;
using System.Management;
using cn.softname2.Log;

namespace cn.softname2.PPPOE_Deceive
{
    class PPPOEDiceive
    {

        #region     //调试时的main方法以及一些调试方法
        //protocol8863 Test
        //static void Main(string[] args)
        //{
        //    support sup = new support();
        //    protocol8863 p3 = new protocol8863();
        //    p3.setDistinationMac(sup.toByte(new string[] {"11", "11", "11", "11", "11", "11", }));
        //    p3.setSourceMac(sup.toByte(new string[] { "22", "22", "22", "22", "22", "22", }));
        //    p3.setProtocol(sup.toByte(new string[] {"88","63"}));
        //    p3.setDiscoveryStage(sup.toByte(new string[] { "09" }));
        //    p3.setSessionID(sup.toByte(new string[] { "00", "00" }));
        //    p3.setData_HostUniq(sup.toByte(new string[] { "99", "99", "99", "99", "99", "99", "99", "99", "99", "99", "99", "99", }));
        //    p3.setData_other(sup.toByte(new string[] { "55", "55", "55", "55", }));
        //    p3.setData_other(sup.toByte(new string[] { "44", "44", "44", "44", }));
        //    p3.setData_other(sup.toByte(new string[] { "33", "33", "33", "33", }));
        //    showByte(p3.getAllPacketData());
        //    Console.Read();
        //}


        //protocol8864 Test
        //static void Main(string[] args)
        //{
        //    support sup = new support();
        //    protocol8864 p4 = new protocol8864();
        //    p4.setDistinationMac(sup.toByte(new string[] { "11", "11", "11", "11", "11", "11", }));
        //    p4.setSourceMac(sup.toByte(new string[] { "22", "22", "22", "22", "22", "22", }));
        //    p4.setSessionID(sup.toByte(new string[] { "00", "01" }));
        //    p4.setPPP(sup.toByte(new string[] { "c0", "21" }));
        //    p4.setPPPLCP_config(sup.toByte(new string[] { "01"}));
        //    p4.setPPPLCP_identifier(sup.toByte(new string[] { "00" }));
        //    p4.setPPPLCP_optionsAnInfo(sup.toByte(new string[] { "99", "99", "99", }));
        //    //p4.setPPP_allData(sup.toByte(new string[] { "88", "88", "88", }));    //自定义ppp数据
        //    showByte(p4.getAllPacketData_PPPLCP());
        //    //showByte(p4.getAllPacketData_custom());                               //显示自定义ppp数据
        //    Console.Read();


        //}


        //Main test
        //static void Main(string[] args)
        //{
        //    new PPPOEDiceive().catchPacketFun();
        //    new PPPOEDiceive().getActiveNetAdapte();
        //    Console.Read();
        //}
        #endregion
        
        private byte[] selfMac = { 17, 34, 51, 68, 85, 102, };  //伪装Mac字段
        private bool colseNetAdapte = false;            		//是否关闭抓包
        public String netAcc = null;                    		//宽带账号，该值为外部需要的值
        public String netPwd = null;                    		//宽带密码，该值为外部需要的值


        public void Start()
        {
            colseNetAdapte = false;
			netAcc=null;
			netPwd=null;
            try
            {
                Thread t = new Thread(new ThreadStart(catchPacketFun));
                t.Start();
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }

        public void Close()
        {
            this.colseNetAdapte = true;
        }



        //获取当前活动的网卡
        public ICaptureDevice getActiveNetAdapte()
        {
            support sup = new support();
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            foreach (ICaptureDevice dev in devices)
            {
                dev.Open();
                if (sup.processMac(dev.MacAddress.ToString()).Equals(sup.GetMacAddress()))
                {
                    return dev;
                }
            }
            log.writeLog("PPPOE欺骗模块 ： 获取活动网卡失败",log.msgType.error);
            return null;
        }




        /*
         *  抓包函数
         */
        public void catchPacketFun()
        {
            //选择活动网卡
            ICaptureDevice device = getActiveNetAdapte();


            //将处理函数注册到"包到达"事件
            //我理解为当device网卡(上面选择的网卡)抓到包时，调用"处理函数"，让"处理函数"处理包(显示/读取/修改等)
            device.OnPacketArrival +=
                new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);

            // Open the device for capturing
            //我理解为被抓包在1000ms时间内被读取的包，而不是只抓1000ms
            int readTimeoutMilliseconds = 1000;
            //打开网口，准备调用StartCapture()（阻塞函数/Capture(int packetCount)为非阻塞函数，使用方法再查询）开始抓包，抓到的包交由"处理函数"执行
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

            // 开始抓包
            device.StartCapture();
            log.writeLog("正在抓包...", log.msgType.info);

            //当colseNetAdapte参数被已经抓获pppoe账号的函数改写时停止抓包
            while (!colseNetAdapte)
            {
                Console.WriteLine("抓包循环...");
                Thread.Sleep(1000);
            }

            // 停止抓包
            device.StopCapture();
            // 关闭网口
            device.Close();
            log.writeLog("抓包终止...", log.msgType.info);
        }

        //catchPacketFun()的处理函数
        //处理方式是pppoe欺骗
        //note(maybe): packet对象是被抓获的包(单包)
        private void device_OnPacketArrival(object sender, CaptureEventArgs packet)
        {

            if(packet.Packet.Data[12] == 136)
            {
                if (packet.Packet.Data[13] == 99)        //过滤非pppoe包,是 8863 包时
                {
                    byte[] pac = packet.Packet.Data;
                    protocol8863 p8863 = new protocol8863();
                    support sup = new support();
                    switch (p8863.getDiscoveryStage(pac)[0])
                    {
                        //PADI 0x09
                        case 9:
                            p8863.setDistinationMac(sup.getSourceMac(pac));
                            p8863.setSourceMac( selfMac );
                            p8863.setDiscoveryStage(new byte[] { 7});
                            p8863.setSessionID(p8863.getSessionID(pac));
                            p8863.setData_HostUniq(p8863.getData_HostUniq(pac));
                            p8863.setData_other(sup.toByte(new string[] { "01", "02", "00", "08", "50", "50", "50", "4f", "45", "53", "52", "56" }));   //PPPOESRV
                            sendPacket(p8863.getAllPacketData());
                            log.writeLog("正在进行PPPOE协议交互...", log.msgType.info);
                            return;
                        //PADR 0x19
                        case 25:
                            p8863.setDistinationMac(sup.getSourceMac(pac));
                            p8863.setSourceMac( selfMac);
                            p8863.setDiscoveryStage(new byte[] { 101 });
                            byte[] sessionID = p8863.getSessionID(pac);
                            sessionID[1]++;
                            p8863.setSessionID(sessionID);
                            p8863.setData_HostUniq(p8863.getData_HostUniq(pac));
                            sendPacket(p8863.getAllPacketData());
                            return;
                    }
                }

                if (packet.Packet.Data[13] == 100)       //过滤非pppoe包,是 8864 包时
                {
                    byte[] pac = packet.Packet.Data;
                    protocol8864 p8864 = new protocol8864();
                    support sup = new support();
                    if (p8864.getPPP(pac)[0]== 192 && p8864.getPPP(pac)[1] == 35)   //如果PPP为PAP
                    {
                        #region //目前的方式，使用 691 Message 终结连接
                        if (pac[22] == (byte)1) //如果是PAP request包
                        {
                            //Console.WriteLine("终结包（691 message packet）已发送");
                            byte[] papContent = p8864.getPPPLCP_optionsData(pac);   //获取PAP内容
                            byte[] papNetAcc = sup.byteSub(papContent, 1, papContent[0]);
                            byte[] papNetPwd = sup.byteSub(papContent, papContent[0] + 2, papContent[papContent[0] + 1]);
                            log.writeLog($"PPPOE 欺骗模块\n截取到账号: {Encoding.Default.GetString(papNetAcc)}\n截取到密码:{Encoding.Default.GetString(papNetPwd)}", log.msgType.info);
                            netAcc = Encoding.Default.GetString(papNetAcc);     //将截取到的 账号 赋值到类变量
                            netPwd = Encoding.Default.GetString(papNetPwd);     //将截取到的 密码 赋值到类变量
                            colseNetAdapte = true;                              //停止抓包

                            //691 Msg
                            String msg691 = "\r\n拦截成功，这并非错误提示。\r\n\r\n你现在可以关闭Netkeeper\r\n\r\n并注意nkshare的提示##Error";
                            byte[] msg691Bytes = Encoding.GetEncoding("GB2312").GetBytes(msg691);
                            protocol8864 p8864Show = new protocol8864();
                            p8864Show.setDistinationMac(sup.getSourceMac(pac));
                            p8864Show.setSourceMac(selfMac);
                            p8864Show.setSessionID(p8864Show.getSessionID(pac));
                            byte[] msg691BytesPac = sup.byteAppent(new byte[] { (byte)msg691Bytes.Length }, msg691Bytes);
                            byte[] papCodeBytesPac = sup.byteAppent(new byte[] { 192, 35, 3, ++pac[23] }, new byte[] { (byte)((msg691BytesPac.Length + 4) / 256), (byte)((msg691BytesPac.Length + 4) % 256) });
                            byte[] papAll = sup.byteAppent(papCodeBytesPac, msg691BytesPac);
                            p8864Show.setPPP_allData(papAll);
                            sendPacket(p8864Show.getAllPacketData_custom());
                            //691 Msg
                            protocol8863 p8863 = new protocol8863();
                            p8863.setDistinationMac(sup.getSourceMac(pac));
                            p8863.setSourceMac(sup.getDistinationMac(pac));
                            p8863.setDiscoveryStage(new byte[] { 167 });    //PADT 终结包
                            p8863.setSessionID(p8863.getSessionID(pac));
                            p8863.setData_other(null);
                            sendPacket(p8863.getAllPacketData());
                            return;
                        }
                        #endregion

                        #region //原来直接方式终结包
                        //protocol8863 p8863 = new protocol8863();
                        //p8863.setDistinationMac(sup.getSourceMac(pac));
                        //p8863.setSourceMac(sup.getDistinationMac(pac));
                        //p8863.setDiscoveryStage(new byte[] { 167 });    //PADT 终结包
                        //p8863.setSessionID(p8863.getSessionID(pac));
                        //p8863.setData_other(null);
                        //sendPacket(p8863.getAllPacketData());
                        //Console.WriteLine("终结包已发送");
                        //byte[] papContent = p8864.getPPPLCP_optionsData(pac);   //获取PAP内容
                        //byte[] papNetAcc = sup.byteSub(papContent, 1, papContent[0]);
                        //byte[] papNetPwd = sup.byteSub(papContent, papContent[0] + 2, papContent[papContent[0] + 1]);
                        //log.writeLog($"PPPOE 欺骗模块\n截取到账号: {Encoding.Default.GetString(papNetAcc)}\n截取到密码:{Encoding.Default.GetString(papNetPwd)}",log.msgType.info);
                        //netAcc = Encoding.Default.GetString(papNetAcc);     //将截取到的 账号 赋值到类变量
                        //netPwd = Encoding.Default.GetString(papNetPwd);     //将截取到的 密码 赋值到类变量
                        //colseNetAdapte = true;                              //停止抓包
                        #endregion


                    }


                    if (sup.equalsByte(sup.getDistinationMac(pac),selfMac) && p8864.getPPPLCP_config(pac)[0]==1   )  //如果是客户机Requuest本机（PPP-LCP/虽然没加上这个默认条件）
                    {
                        p8864.setDistinationMac(sup.getSourceMac(pac));
                        p8864.setSourceMac(selfMac);
                        p8864.setSessionID(p8864.getSessionID(pac));
                        p8864.setPPP(p8864.getPPP(pac));
                        p8864.setPPPLCP_config(new byte[] { 2 });   //ACK
                        p8864.setPPPLCP_identifier(p8864.getPPPLCP_identifier(pac));

                        byte[] optionDataModif = p8864.getPPPLCP_optionsData(pac);
                        p8864.setPPPLCP_optionsData(optionDataModif);    //轻冗余
                        sendPacket(p8864.getAllPacketData_PPPLCP());

                        p8864 = new protocol8864();
                        p8864.setDistinationMac(sup.getSourceMac(pac));
                        p8864.setSourceMac(selfMac);
                        p8864.setSessionID(p8864.getSessionID(pac));
                        p8864.setPPP(p8864.getPPP(pac));
                        p8864.setPPPLCP_config(new byte[] { 1 });   //Req

                        p8864.setPPPLCP_identifier(new byte[] { 1 });
                        p8864.setPPPLCP_optionsData(sup.toByte(new string[] { "03", "04", "c0", "23", }));
                        sendPacket(p8864.getAllPacketData_PPPLCP());
                        return; 
                    }
                    


                }
            }
            

        }


        //发送byte[]数据到当前活动网卡
        private void sendPacket(byte[] bytes)
        {
            //选择一个网卡
            ICaptureDevice device = getActiveNetAdapte();
            device.Open();

            try
            {
                // Send the packet out the network device
                device.SendPacket(bytes);
            }
            catch (Exception e)
            {
                log.writeLog("发送原始包出现异常:", log.msgType.error);
            }
            // Close the pcap device
            //device.Close();
        }


        private void adapterInfo()  //查看所有网卡信息，本函数暂留测试之用
        {
            // Print SharpPcap version 
            string ver = SharpPcap.Version.VersionString;
            Console.WriteLine("SharpPcap {0}, Example1.IfList.cs", ver);

            // Retrieve the device list
            CaptureDeviceList devices = CaptureDeviceList.Instance;

            // If no devices were found print an error
            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine");
                return;
            }

            Console.WriteLine("\nThe following devices are available on this machine:");
            Console.WriteLine("----------------------------------------------------\n");

            // Print out the available network devices
            foreach (ICaptureDevice dev in devices)
                Console.WriteLine("{0}\n", dev.ToString());

            Console.Write("Hit 'Enter' to exit...");
            Console.ReadLine();
        }

    }




}
