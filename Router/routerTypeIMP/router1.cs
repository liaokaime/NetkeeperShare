using cn.softname2.Log;
using cn.softname2.Router.routerTypeDAO;
using cn.softname2.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;


namespace cn.softname2.routerControl.routerType
{
    /*
     * **************************************
     *          使用get方法I的路由器
     * **************************************
     * 该路由器使用get方式向路由器传输各项控制参数，并将密码信息写在http请求的头信息（Authorization）中
     * 密码信息的加密方式为base64
     */
    class router1: Interf_routerType
    {
        network networkSup = new network();
        String gateway = null;
        public int routerType() {
            return 1;
        }

        public router1(String gateway)
        {
            this.gateway = gateway;
        }


        public router1()    // 为使反射机制更容易获取此类实例
        {
        }

        public virtual String ToGet(String routerAcc, String routerPwd, String url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                byte[] credentialBuffer = new UTF8Encoding().GetBytes(routerAcc + ":" + routerPwd);
                string cook = "Basic " + Convert.ToBase64String(credentialBuffer);
                request.Headers.Add("Authorization", cook);
                request.Referer = "http://" + this.gateway + "/userRpm/PPPoECfgRpm.htm?&SecType=0";
                Stream s = request.GetResponse().GetResponseStream();
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                String readToEnd = reader.ReadToEnd();
                request.Abort();
                s.Close();
                reader.Close();
                return readToEnd;
            }
            catch
            {
                return null;
            }
        }

        public String getRouterMac(String routerAcc, String routerPwd)
        {
            String httpLinkInfo_getRouterMac = "http://" + this.gateway + "/userRpm/MacCloneCfgRpm.htm";
            String httpReceive_getRouterMac = ToGet(routerAcc, routerPwd, httpLinkInfo_getRouterMac);
            if (httpReceive_getRouterMac == null)
                return null;
            String[] fragment_getRouterMac = httpReceive_getRouterMac.Split('"');
            byte tm = 0;
            foreach (String a in fragment_getRouterMac)
            {
                if (tm == 1 && networkSup.IsMac(a))
                    return a;
                if (networkSup.IsMac(a))
                    tm++;
            }
            return null;
        }

        public int setInternetMode_AutoIP(
            String routerAcc,   //不需要账号的路由器routerAcc填"admin"或""
            String routerPwd
        )
        {
            log.writeLog("router1 : setInternetMode_AutoIP()函数 正在执行", log.msgType.info);
            String httpLinkInfo_setInternetMode_AutoIP = "http://" + this.gateway + "/userRpm/WanDynamicIpCfgRpm.htm?wantype=0&mtu=1500&hostName=MW300R&downBandwidth=0&upBandwidth=0&Save=%B1%A3+%B4%E6";
            String httpReceive_setInternetMode_AutoIP = ToGet(routerAcc, routerPwd, httpLinkInfo_setInternetMode_AutoIP);
            if (httpReceive_setInternetMode_AutoIP == null)
                return 0;
            return 1;
        }


        public int setInternetMode_PPPOE(
            String routerAcc,
            String routerPwd,
            String netAcc,
            String netPwd,
            int callMode,        //拨号模式，0为正常拨号，1234567...对应特殊拨号1234567...    
            int linkMode         //连接模式：1.自动连接，开机和断线后自动连接
                                 //          2.按需连接，有网络时自动连接
                                 //          3.手动连接，由用户手动连接
         )
        {
            log.writeLog("router1 : setInternetMode_PPPOE()函数 正在执行", log.msgType.info);
            String writeInLinkCallMode = null;
            if (callMode >= 0)
                writeInLinkCallMode = callMode.ToString();       //  100为自动拨号
            String writeInLinkLinkMode = null;
            switch (linkMode)
            {
                case 1: writeInLinkLinkMode = "2"; break;       //自动连接
                case 2: writeInLinkLinkMode = "1"; break;       //按需连接
                case 3: writeInLinkLinkMode = "4"; break;       //手动连接
            }
            if (writeInLinkLinkMode == null)
            {
                log.writeLog("setInternetMode_PPPOE函数 参数:linkMode(连接模式)出现错误", log.msgType.error);
                return 0;
            }

            String httpLinkInfo_setInternetMode_PPPOE = "http://" + this.gateway + "/userRpm/PPPoECfgRpm.htm?wan=0&wantype=2&acc=" + netAcc + "&psw=" + netPwd + "&confirm=" + netPwd + "&specialDial=" + writeInLinkCallMode + "&SecType=0&sta_ip=0.0.0.0&sta_mask=0.0.0.0&linktype=" + writeInLinkLinkMode + "&waittime=15&Connect=%C1%AC+%BD%D3";
            //                                                                                                              宽带账号          宽带密码              确认密码                          拨号模式                                                                      连接模式
            //                                                                                                                            使用路由之前储存的密码:Hello123World
            String httpReceive_setInternetMode_PPPOE = ToGet(routerAcc, routerPwd, httpLinkInfo_setInternetMode_PPPOE);
            if (httpReceive_setInternetMode_PPPOE == null)
                return 0;
            return 1;
        }


        public int setCloneMAC(
            String routerAcc,
            String routerPwd,
            String Mac         //传入"router"为使用路由器地址
                               //    "pc"    为使用本机MAC地址
                               //其余为指定MAC地址，格式用XX-XX-XX-XX-XX-XX
                               //最后有个校验，如果不是这种格式，此函数将返回失败代码0
        )
        {

            String writeInLinkMac = Mac;
            if (Mac.Equals("router"))
                writeInLinkMac = getRouterMac(routerAcc, routerPwd);
            else if (Mac.Equals("pc"))
                writeInLinkMac = networkSup.getLocaMac();
            if (!networkSup.IsMac(writeInLinkMac))
            {
                log.writeLog($"router1 : setCloneMAC() : 传入mac或快捷获取的mac并非标准mac //writeInLinkMac:{writeInLinkMac}", log.msgType.error);
                return 0;
            }

            String writeInLinkDefaultMac = null;
            if (networkSup.IsMac(getRouterMac(routerAcc, routerPwd)))
            {
                writeInLinkDefaultMac = getRouterMac(routerAcc, routerPwd);
            }
            else
            {
                writeInLinkDefaultMac = Mac;
            }

            String httpLinkInfo_setCloneMAC = "http://" + this.gateway + "/userRpm/MacCloneCfgRpm.htm?mac1=" + writeInLinkMac + "&defaultMac1=" + writeInLinkDefaultMac + "&Save=%B1%A3+%B4%E6";
            String httpReceive_setCloneMAC = ToGet(routerAcc, routerPwd, httpLinkInfo_setCloneMAC);
            if (httpReceive_setCloneMAC == null)
                return 0;
            return 1;
        }


        public virtual int setWIFI(
            String routerAcc,
            String routerPwd,
            String wifiName,
            String wifiPwd,
            Boolean Start       //true 启动wifi;  false 关闭wifi;
        )
        {
            if (wifiName == null && wifiName.Equals(""))
            {
                log.writeLog("router1 : setWIFI() : wifiSSID错误", log.msgType.error);
                return 0;
            }
            if (wifiPwd.Length < 8)
            {
                log.writeLog("router1 : setWIFI() : wifi密码长度不够", log.msgType.error);
                return 0;
            }
            String writeInLinkStart = null;
            if (Start)
            {
                writeInLinkStart = "ap=1&";
            }
            else
            {
                writeInLinkStart = "";
            }
            String httpLinkInfo_setWIFINameAndStart = "http://" + this.gateway + "/userRpm/WlanNetworkRpm.htm?ssid1=" + wifiName + "&wlMode=2&channel=0&mode=5&chanWidth=2&" + writeInLinkStart + "broadcast=2&brlssid=&brlbssid=&keytype=1&wepindex=1&authtype=1&keytext=&Save=%B1%A3+%B4%E6";
            String httpReceive_setWIFINameAndStart = ToGet(routerAcc, routerPwd, httpLinkInfo_setWIFINameAndStart);
            if (httpReceive_setWIFINameAndStart == null)
            {
                log.writeLog("router1 : setWIFI() : 设置 wifi名称与启动 命令 服务器无返回", log.msgType.error);
                return 0;
            }

            String httpLinkInfo_setWIFIPwd = "http://" + this.gateway + "/userRpm/WlanSecurityRpm.htm?vapIdx=1&secType=3&pskSecOpt=2&pskCipher=3&pskSecret=" + wifiPwd + "&interval=86400&wpaSecOpt=3&wpaCipher=1&radiusIp=&radiusPort=1812&radiusSecret=&intervalWpa=86400&wepSecOpt=3&keytype=1&keynum=1&key1=&length1=0&key2=&length2=0&key3=&length3=0&key4=&length4=0&Save=%B1%A3+%B4%E6";
            String httpReceive_setWIFIPwd = ToGet(routerAcc, routerPwd, httpLinkInfo_setWIFIPwd);
            if (httpReceive_setWIFIPwd == null)
            {
                log.writeLog("router1 : setWIFI() : 设置 wifi密码 命令 服务器无返回", log.msgType.error);
                return 0;
            }
            else
            {
                int rebootState = setReboot(routerAcc, routerPwd);
                if (rebootState == 0)
                {
                    log.writeLog("router1 : setWIFI() : 设置wifi后重启失败", log.msgType.error);
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            log.writeLog("router1 : setWIFI() : 未知原因出错", log.msgType.error);
            return 0;
        }


        public int setReboot(
            String routerAcc,
            String routerPwd
        )
        {
            
            String httpLinkInfo_Reboot = "http://" + this.gateway + "/userRpm/SysRebootRpm.htm?Reboot=%D6%D8%C6%F4%C2%B7%D3%C9%C6%F7";
            String httpReceive_Reboot = ToGet(routerAcc, routerPwd, httpLinkInfo_Reboot);
            if (httpReceive_Reboot == null)
            {
                log.writeLog("router1 : setReboot() 重启出错，服务器返回值为null",log.msgType.error);
                return 0;
            }
            return 1;
        }


        public virtual bool IsRouterClassType()
        {
            String socketData = @"GET /userRpm/SysRebootRpm.htm?Reboot=%D6%D8%C6%F4%C2%B7%D3%C9%C6%F7 HTTP/1.1
Host: " + this.gateway + @"
Connection: Keep-Alive

";

            String verCode0 = "用户名或密码有误";
            String socketReceive;
            try
            {
                socketReceive = networkSup.socketSentString(this.gateway, 80, socketData);
                if (socketReceive.IndexOf(verCode0) != -1)
                    return true;
                return false;
            }
            catch
            {
                log.writeLog("router1 : IsRouterClassType() : 捕捉到未知异常", log.msgType.warning);
                return false;
            }
        }


    }
}
