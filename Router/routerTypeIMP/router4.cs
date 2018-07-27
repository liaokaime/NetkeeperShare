using cn.softname2.Router.routerTypeDAO;
using cn.softname2.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using cn.softname2.Log;

namespace cn.softname2.routerControl.routerType
{

    /*
     * ****************************************
     *          使用get方法II的路由器
     * ****************************************
     * 该型路由器的请求方式与getI型相似，不同的是密码信息不是写在Authorization头信息中，
     * 而是将其写在Cookie头信息中，并且稍作了修改，加密方式仍然为base64
     */
    class router4 : router1, Interf_routerType
    {
        network networkSup = new network();
        String gateway = null;
        public int routerType()
        {
            return 4;
        }

        public router4(String gateway):base(gateway)
        {
            this.gateway = gateway;
        }
        //重写router1的ToGet()方法，router1的ToGet之于此方法，主要是头字段cookie和Authorization的差别，和refresh有无的差别
        public override String ToGet(String routerAcc, String routerPwd, String url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                byte[] credentialBuffer = Encoding.ASCII.GetBytes(routerAcc + ":" + routerPwd);
                string cook = "Authorization=Basic " + Convert.ToBase64String(credentialBuffer) + "; ChgPwdSubTag=";
                request.Headers.Add("Cookie", cook);
                request.Referer = @"http://" + this.gateway + "/";
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
                Console.WriteLine("class router4 : ToGet request error");
                return null;
            }
        }

        public override bool IsRouterClassType()
        {
            String socketData = @"GET /userRpm/SysRebootRpm.htm?Reboot=%D6%D8%C6%F4%C2%B7%D3%C9%C6%F7 HTTP/1.1
Host: " + this.gateway + @"
Connection: Keep-Alive

";

            String verCode0 = "You have no authority to access this device";
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
                log.writeLog("router4 : IsRouterClassType() : 捕捉到未知异常", log.msgType.error);
                return false;
            }
        }

        //比起getI型路由，该getII型路由设置wifi时不需要重启路由
        public override int setWIFI(
            String routerAcc,
            String routerPwd,
            String wifiName,
            String wifiPwd,
            Boolean Start       //true 启动wifi;  false 关闭wifi;
        )
        {
            if (wifiName == null && wifiName.Equals(""))
            {
                log.writeLog("router4 : setWIFI() : wifiSSID错误", log.msgType.error);
                return 0;
            }
            if (wifiPwd.Length < 8)
            {
                log.writeLog("router4 : setWIFI() : wifi密码长度不够", log.msgType.error);
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
                log.writeLog("router4 : setWIFI() : 设置wifi名或开启wifi时出现错误", log.msgType.error);
                return 0;
            }

            String httpLinkInfo_setWIFIPwd = "http://" + this.gateway + "/userRpm/WlanSecurityRpm.htm?vapIdx=1&secType=3&pskSecOpt=2&pskCipher=3&pskSecret=" + wifiPwd + "&interval=86400&wpaSecOpt=3&wpaCipher=1&radiusIp=&radiusPort=1812&radiusSecret=&intervalWpa=86400&wepSecOpt=3&keytype=1&keynum=1&key1=&length1=0&key2=&length2=0&key3=&length3=0&key4=&length4=0&Save=%B1%A3+%B4%E6";
            String httpReceive_setWIFIPwd = ToGet(routerAcc, routerPwd, httpLinkInfo_setWIFIPwd);
            if (httpReceive_setWIFIPwd == null)
            {
                log.writeLog("router4 : setWIFI() : 设置wifi密码时出现错误", log.msgType.error);
                return 0;
            }
            else
            {
                int rebootState = setReboot(routerAcc, routerPwd);
                if (rebootState == 0)
                {
                    log.writeLog("router4 : setWIFI() : 设置完wifi后重启时发生错误", log.msgType.error);
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            log.writeLog("router4 : setWIFI() : 未知原因出错", log.msgType.error);
            return 0;
        }


    }

}
