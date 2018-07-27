
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace cn.softname2.Support
{
    class network
    {
        //获取本地当前网关
        public String getLocaGateway()
        {       //获取本地网关(非标准获取网关代码，经过了对适合softname工程的筛选，权宜之计，日后补全)
            string strGateway = "";
            //获取所有网卡
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            //遍历数组
            foreach (var netWork in nics)
            {
                if (netWork.Name.IndexOf("VM") != -1)   //从名字层面剔除虚拟网卡
                    continue;
                if (netWork.OperationalStatus.ToString().IndexOf("Down") != -1)   //从开启或未开启层面上剔除干扰判断的网卡
                {
                    continue;
                }
                //单个网卡的IP对象
                IPInterfaceProperties ip = netWork.GetIPProperties();
                //获取该IP对象的网关
                GatewayIPAddressInformationCollection gateways = ip.GatewayAddresses;
                foreach (var gateWay in gateways)
                {
                    //如果能够Ping通网关
                    if (PingIpOrDomainName(gateWay.Address.ToString()))
                    {
                        //得到网关地址
                        strGateway = gateWay.Address.ToString();
                        //跳出循环
                        break;
                    }
                }
                //如果已经得到网关地址
                if (strGateway.Length > 0)
                {
                    break;
                }
            }
            return strGateway;
        }

        //获取本机MAC
        public String getLocaMac()
        {           //获取本机MAC
            string macAddresses = string.Empty;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return processMac(macAddresses);
        }

        //判断传入参数是否为标准可用的MAC地址，间断符必须为: ' - '
        public bool IsMac(String Mac)
        {
            if (Mac == null)
            {
                Console.WriteLine("IsMac() : Mac传入参数为null");
                return false;
            }
            if (Mac.Length != 17)
                return false;
            byte[] bytes = Encoding.UTF8.GetBytes(Mac);
            foreach (byte b in bytes)
            {
                if (!(b == 45 || (b >= 48 && b <= 57) || (b >= 65 && b <= 70) || (b >= 97 && b <= 102)))
                {
                    return false;
                }
            }
            if (bytes[2] + bytes[5] + bytes[8] + bytes[11] + bytes[14] == (int)'-' * 5)
                return true;
            return false;
        }
        
        //将未间隔的MAC处理为每两个字符间加一个"-"号的MAC
        public String processMac(String Mac)
        { 
            if (Mac.Length == 2)
                return Mac;
            return Mac.Substring(0, 2) + "-" + processMac(Mac.Substring(2, Mac.Length - 2));
        }

        //将传入字符串url编码
        public string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str);
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }
            return (sb.ToString());
        }

        //使用socket发送应用层数据并返回接收回复的数据
        public String socketSentString(String IP, int port, String _string)
        {
            byte[] result = new byte[5000];
            StringBuilder returnString = new StringBuilder();
            //设定服务器IP地址
            IPAddress ip = IPAddress.Parse(IP);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.SendBufferSize = 5000;
            clientSocket.SendTimeout = 1000;     //发送数据超时限制
            clientSocket.ReceiveTimeout = 1000;     //接收数据超时限制
            try
            {
                //clientSocket.Connect(new IPEndPoint(ip, port)); //配置服务器IP与端口  
                IAsyncResult connResult = clientSocket.BeginConnect(IP, port, null, null);
                connResult.AsyncWaitHandle.WaitOne(1000, true);  //等待1秒
                //Console.WriteLine("socketSentString() : 正在与服务器建立tcp连接(1000ms超时限制)\n");
            }
            catch
            {
                Console.WriteLine("socketSentString() : 连接服务器失败\n");
                return null;
            }

            //通过 clientSocket 发送数据  
            try
            {
                string sendMessage = _string;
                clientSocket.Send(Encoding.ASCII.GetBytes(sendMessage));
                //Console.WriteLine("向服务器发送消息：\n" + sendMessage); 

            }
            catch
            {
                Console.WriteLine("socketSentString() : 发送数据失败，可能是建立tcp连接超时(关闭套接字)\n");
                clientSocket.Close();
                return null;
            }

            try
            {
                //通过clientSocket接收数据 并粘包
                int receiveLength = clientSocket.Receive(result);
                while (receiveLength != 0)
                {
                    returnString.Append(Encoding.GetEncoding("GB2312").GetString(result, 0, receiveLength));
                    receiveLength = clientSocket.Receive(result);
                }

                Console.WriteLine("mark");
                return returnString.ToString();
            }
            catch
            {
                Console.WriteLine("socketSentString() :接收数据和粘包时发生未知异常");
                return null;
            }

        }

        //获取本机IP
        public string GetLocalIP()
        {
            //得到计算机名
            string strPcName = Dns.GetHostName();
            //得到本机IP地址数组
            IPHostEntry ipEntry = Dns.GetHostEntry(strPcName);
            //遍历数组
            foreach (var IPadd in ipEntry.AddressList)
            {
                //判断当前字符串是否为正确IP地址
                if (true)
                {
                    String netSegment = getLocaGateway().Substring(0, 7);
                    if (IPadd.ToString().Substring(0, 7).Equals(netSegment))
                        return IPadd.ToString();
                }
            }

            return null;
        }

        //ping ip或域名
        public bool PingIpOrDomainName(string strIpOrDName)
        {
            try
            {
                Ping objPingSender = new Ping();
                PingOptions objPinOptions = new PingOptions();
                objPinOptions.DontFragment = false;
                string data = "";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int intTimeout = 120;
                PingReply objPinReply = objPingSender.Send(strIpOrDName, intTimeout, buffer, objPinOptions);
                string strInfo = objPinReply.Status.ToString();
                if (strInfo.Equals("Success"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        //判断是否为合法ip地址
        public bool ValidateIPAddress(string ipAddress)
        {
            Regex validipregex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
            return (ipAddress != "" && validipregex.IsMatch(ipAddress.Trim())) ? true : false;
        }


        #region socket获取本地及远程网络信息
        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 length);
        [DllImport("Ws2_32.dll")]
        private static extern Int32 inet_addr(string ip);

        ////获取本机的IP
        //public string getLocalIP_2()
        //{
        //    string strHostName = Dns.GetHostName(); //得到本机的主机名

        //    IPHostEntry ipEntry = Dns.GetHostByName(strHostName); //取得本机IP

        //    string strAddr = ipEntry.AddressList[0].ToString();
        //    return (strAddr);
        //}
        //获取本机的MAC

        //public string getLocalMac_2()
        //{
        //    string mac = null;
        //    ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
        //    ManagementObjectCollection queryCollection = query.Get();
        //    foreach (ManagementObject mo in queryCollection)
        //    {
        //        if (mo["IPEnabled"].ToString() == "True")
        //            mac = mo["MacAddress"].ToString();
        //    }
        //    return (mac);
        //}

        //获取远程主机IP
        public string[] getRemoteIP(string RemoteHostName)
        {
            IPHostEntry ipEntry = Dns.GetHostByName(RemoteHostName);
            IPAddress[] IpAddr = ipEntry.AddressList;
            string[] strAddr = new string[IpAddr.Length];
            for (int i = 0; i < IpAddr.Length; i++)
            {
                strAddr[i] = IpAddr[i].ToString();
            }
            return (strAddr);
        }
        //获取远程主机MAC

        public string getRemoteMac(string localIP, string remoteIP)
        {
            Int32 ldest = inet_addr(remoteIP); //目的ip 

            Int32 lhost = inet_addr(localIP); //本地ip


            try
            {
                Int64 macinfo = new Int64();
                Int32 len = 6;
                int res = SendARP(ldest, 0, ref macinfo, ref len);
                return Convert.ToString(macinfo, 16);
            }
            catch (Exception err)
            {
                Console.WriteLine("Error:{0}", err.Message);
            }
            return 0.ToString();
        }

        #endregion





        public List<String> getAdapterIPs()
        {
            List<String> list = new List<String>();
            //得到计算机名
            string strPcName = Dns.GetHostName();
            //得到本机IP地址数组
            IPHostEntry ipEntry = Dns.GetHostEntry(strPcName);
            //遍历数组
            foreach (var IPadd in ipEntry.AddressList)
            {
                if (ValidateIPAddress(IPadd.ToString()))
                {
                    list.Add(IPadd.ToString());
                }
            }
            return list;
        }


        public List<String> getGateways()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection nics = mc.GetInstances();
            List<String> list = new List<String>();
            foreach (ManagementObject nic in nics)
            {
                if (Convert.ToBoolean(nic["ipEnabled"]) == true)
                {
                    //string mac = nic["MacAddress"].ToString();//Mac地址
                    //string ip = (nic["IPAddress"] as String[])[0];//IP地址
                    //string ipsubnet = (nic["IPSubnet"] as String[])[0];//子网掩码
                    string ipgateway = (nic["DefaultIPGateway"] as String[])[0];//默认网关
                    //Console.WriteLine("网关：" + ipgateway);
                    list.Add(ipgateway);
                }
            }
            return list;
        }


        /// <summary>
        /// 尝试Ping指定IP是否能够Ping通
        /// </summary>
        /// <param name="strIP">指定IP</param>
        /// <returns>true 是 false 否</returns>
        public bool IsPingIP(string strIP)
        {
            try
            {
                //创建Ping对象
                Ping ping = new Ping();
                //接受Ping返回值
                PingReply reply = ping.Send(strIP, 1000);
                //Ping通
                return true;
            }
            catch
            {
                //Ping失败
                return false;
            }
        }





    }


}
