
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using cn.softname2.Support;
using cn.softname2.Router.routerTypeDAO;
using cn.softname2.Log;
using System.Threading;

namespace cn.softname2.routerControl.routerType
{
    /*
    * ************************************
    *       使用长短验证码的路由器
    * ************************************
    * 该类型的路由器以无状态连接路由器，获取带有服务器长短验证码的字符串
    * 将本地预储存好的长短验证码与路由密码加密后获取加密字符串①，
    * 再用加密字符串①与从服务器获取的长短验证码再次加密获取加密字符串②，
    * 加密字符串②即为写在url请求链接的后缀stok，
    * 再负载具体的路由请求动作的字符串（json格式）以post请求发送至路由80端口即可完成一次对路由的设置动作
    */
    class router3: Interf_routerType
    {
        network networkSup = new network();
        String gateway = null;
        public int routerType()
        {
            return 3;
        }

        public router3(String gateway)
        {
            this.gateway = gateway;
        }

        //获取带有长短验证码的字符串
        public String getSLVerCode()
        {
            try
            {
                String httpLinkInfo_getSLVerCode = "http://" + this.gateway + "/common/Content.htm";
                HttpWebRequest request = WebRequest.Create(httpLinkInfo_getSLVerCode) as HttpWebRequest;
                request.Method = "GET";
                request.KeepAlive = true;
                Stream s = request.GetResponse().GetResponseStream();
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                String readToEnd = reader.ReadToEnd();
                request.Abort();
                s.Close();
                reader.Close();
                return readToEnd;
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("getSLVerCode() : 未连接到服务器(WebException异常)");
                return null;
            }
            catch
            {
                Console.WriteLine("getSLVerCode() 未知异常");
                return null;
            }

        }

        //验证路由器返回的带有长短验证码的字符串是否正常
        public bool IsSLVerCode(String SLVerCode)
        {
            try
            {
                if (SLVerCode.Substring(SLVerCode.Length - 7, 5) == "00000" && SLVerCode.Length == 303)
                    return true;
            }
            catch
            {
                Console.WriteLine("IsSLVerCode() subString捕捉到异常");
                return false;
            }
            return false;
        }

        //从带有长短验证码的字符串中提取出短验证码
        private String getNetShort(String SLVerCode)
        {
            if (IsSLVerCode(SLVerCode))
                return SLVerCode.Substring(21, 16);
            else
            {
                Console.WriteLine("getNetShort() : 获取短验证码失败");
                return null;
            }
            
        }
        
        //从带有长短验证码的字符串中提取出长验证码
        private String getNetLong(String SLVerCode)
        {
            if (IsSLVerCode(SLVerCode))
                return SLVerCode.Substring(39, 255);
            else
            {
                Console.WriteLine("getNetLong() : 获取长验证码失败");
                return null;
            }
        }

        //该型路由的加密算法
        private String encrypt(String a, String b, String c)
        {
            String d = "";
            int e, k = 187, l = 187, f = a.Length, h = b.Length, m = c.Length;
            e = f > h ? f : h;
            for (int g = 0; g < e; g++)
            {
                l = k = 187;
                if (g >= f)
                {
                    l = b.Substring(g, 1).ToCharArray()[0];
                }
                else
                {
                    if (g >= h)
                    {
                        k = a.Substring(g, 1).ToCharArray()[0];
                    }
                    else
                    {
                        k = a.Substring(g, 1).ToCharArray()[0]; l = b.Substring(g, 1).ToCharArray()[0];
                    }
                }
                d += c.Substring((k ^ l) % m, 1).ToCharArray()[0].ToString();
            }
            return d;
        }

        //本地预储存的长短验证码
        static private String locaShort = "RDpbLfCPsJZ7fiv";
        static private String locaLong = "yLwVl0zKqws7LgKPRQ84Mdt708T1qQ3Ha7xv3H7NyU84p21BriUWBU43odz3iP4rBL3cD02KZciXTysVXiV8ngg6vL48rPJyAUw0HurW20xqxv9aYb4M9wK1Ae0wlro510qXeU07kV57fQMc8L6aLgMLwygtc0F10a0Dg70TOoouyFhdysuRMO51yY5ZlOZZLEal1h0t9YQW0Ko7oBwmCAHoic4HYbUyVeU3sfQ1xtXcPcf1aT303wAQhv66qzW";

        //通过路由密码计算出当前可用的stok的url编码
        public String getLinkStok(String routerPwd)
        {
            String firstEncrypt = encrypt(routerPwd, locaShort, locaLong);

            String SLVerCode = getSLVerCode();
            String netShort = getNetShort(SLVerCode);
            if (netShort == null)
            {
                Console.WriteLine("getLinkStok() : getNetShort fail");
                return null;
            }
            String netLong = getNetLong(SLVerCode);
            if (netLong == null)
            {
                Console.WriteLine("getLinkStok() : getNetLong fail");
                return null;
            }

            String noTranscodingStok = encrypt(netShort, firstEncrypt, netLong);
            return networkSup.UrlEncode(noTranscodingStok);
        }
        
        //快速发送post请求
        private String ToPost(String url, String body)
        {
            try
            {
                byte[] pass = Encoding.UTF8.GetBytes(body);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Proxy = null;
                request.KeepAlive = true;
                request.Accept = "*/*";
                request.Referer = "http://"+this.gateway+"/"; 
                request.ServicePoint.Expect100Continue = false;
                request.ContentLength = body.Length;
                request.ContentType = "text/plain;charset=UTF-8";
                request.ReadWriteTimeout = 1000;        
                Stream mystream = request.GetRequestStream();
                mystream.Write(pass, 0, pass.Length);
                mystream.Close();
                HttpWebResponse respone = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(respone.GetResponseStream());
                string result = sr.ReadToEnd();
                sr.Close();
                respone.Close();
                request.Abort();
                return result;
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("ToPost() ： 连接超时！");
            }
            catch (System.UriFormatException)
            {
                Console.WriteLine("ToPost() ： url格式错误！");
            }
            catch
            {
                Console.WriteLine("ToPost() ： 未知异常");
            }
            return null;
        }



        public int setInternetMode_AutoIP(
            String routerAcc,   //不需要账号的路由器routerAcc填"admin"或""
            String routerPwd
        )
        {
            String url = "http://" + this.gateway + "/?code=1&asyn=0&id=" + getLinkStok(routerPwd);
            String body = @"id 22
linkMode 0
linkType 0";
            try
            {
                String receive = ToPost(url, body);
                if (receive.IndexOf("00000") != -1)
                    return 1;
                log.writeLog("router3 setInternetMode_AutoIP() 服务器返回未成功字段", log.msgType.error);
                return 0;
            }
            catch
            {
                log.writeLog("router3 : setInternetMode_AutoIP() 出现未知异常",log.msgType.error);
                Console.WriteLine("setInternetMode_AutoIP() : 未知异常");
                return 0;
            }
        }




        public int setInternetMode_PPPOE(
            String routerAcc,
            String routerPwd,
            String netAcc,
            String netPwd,
            int callMode,        //拨号模式，0为正常拨号，1234567...对应特殊拨号1234567...    100为自动拨号
            int linkMode         //连接模式：1.自动连接，开机和断线后自动连接
                                 //          2.按需连接，有访问时自动连接
                                 //          3.手动连接，由用户手动连接
         )
        {
            try
            {
                String url = "http://" + this.gateway + "/?code=1&asyn=0&id=" + getLinkStok(routerPwd);
                String writeInbody_dialMode = null;
                if (callMode >= 0)
                    writeInbody_dialMode = callMode.ToString();

                String writeInbody_linkMode = null;
                switch (linkMode)
                {
                    case 1: writeInbody_linkMode = "1"; break;
                    case 2: writeInbody_linkMode = "0"; break;
                    case 3: writeInbody_linkMode = "-1"; break;
                }
                if (writeInbody_linkMode == null)
                {
                    log.writeLog("router3 : setInternetMode_PPPOE() 连接模式参数为null", log.msgType.error);
                    return 0;
                }

                String body_dialMode = //"id 26\r\nsvName \r\nacName \r\nname " + this.urlEncode(netAcc) + "\r\npaswd " + this.urlEncode(netPwd) + "\r\nfixipEnb 0\r\nfixip 0.0.0.0\r\nmanualDns 0\r\ndns 0 0.0.0.0\r\ndns 1 0.0.0.0\r\nlcpMru 1480\r\n";

                    @"id 26
svName 
acName 
name " + this.urlEncode(netAcc) + @"
paswd " + netPwd + @"
fixipEnb 0
fixip 0.0.0.0
manualDns 0
dns 0 0.0.0.0
dns 1 0.0.0.0
lcpMru 1480
linkType " + writeInbody_linkMode + @"
dialMode " + writeInbody_dialMode + @"
maxIdleTime 15
id 22
linkMode 0
linkType 2";
                String receive = ToPost(url, body_dialMode);
                if (receive.IndexOf("00000") != -1)
                    return 1;
                log.writeLog("router3 : setInternetMode_PPPOE() pppoe连接时，服务器返回未成功字段", log.msgType.error);
                return 0;
            }
            catch
            {
                log.writeLog("router3 : setInternetMode_PPPOE() 未知错误", log.msgType.error);
                return 0;
            }

        }

        public String urlEncode(String s) {
            if (s != null) {
                return HttpUtility.UrlEncode(s).Replace("+", "%20");
            }
            return null;
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
            String url_getRouterMac = "http://" + this.gateway + "/?code=2&asyn=0&id=" + getLinkStok(routerPwd);
            String body_getRouterMac = "1";
            String receive_getRouterMac = ToPost(url_getRouterMac, body_getRouterMac);
            String lanMac = null;
            //Console.WriteLine("receive_getRouterMac:" + receive_getRouterMac);
            //Console.WriteLine("indexof: " + receive_getRouterMac.IndexOf("mac 0 "));
            if(receive_getRouterMac != null)
                lanMac = receive_getRouterMac.Substring(receive_getRouterMac.IndexOf("mac 0 ") + 6, 17);
            //Console.WriteLine("routerMac:" + routerMac);
            if (!networkSup.IsMac(lanMac))
            {
                log.writeLog("router3 : setCloneMAC() 获取路由MAC失败", log.msgType.error);
                return 0;
            }

            String url_getPcMac = "http://" + this.gateway + "/?code=8&asyn=0&id=" + getLinkStok(routerPwd);
            String body_getPcMac = "";
            String receive_getPcMac = ToPost(url_getPcMac, body_getPcMac);
            String PcMac = null;
            PcMac = receive_getPcMac.Substring(receive_getPcMac.IndexOf(@"00000
") + 7, 17);
            if (!networkSup.IsMac(PcMac))
            {
                log.writeLog("router3 : setCloneMAC() 获取PC-MAC失败", log.msgType.error);
                return 0;
            }


            String writeInLinkMac = null;
            if (Mac.Equals("router"))
            {
                writeInLinkMac = getWanMac(lanMac);
                if (writeInLinkMac == null)
                {
                    log.writeLog("router3 : setCloneMAC() writeInLinkMac错误", log.msgType.error);
                    return 0;
                }
            }

            else if (Mac.Equals("pc"))
                writeInLinkMac = PcMac;
            else if (networkSup.IsMac(Mac))
                writeInLinkMac = Mac;
            else
            {
                log.writeLog("router3 : setCloneMAC() Mac参数不合法", log.msgType.error);
                return 0;
            }

            String url_setCloneMac = "http://" + this.gateway + "/?code=1&asyn=0&id=" + getLinkStok(routerPwd);
            String body_setCloneMac = @"id 1
authKey " + encrypt(routerPwd, locaShort, locaLong) + @"
setWzd 1
mode 0
logLevel 3
fastpath 1
mac 0 " + lanMac + @"
mac 1 " + writeInLinkMac;
            String receive_setCloneMac = ToPost(url_setCloneMac, body_setCloneMac);
            Console.WriteLine("receive_setCloneMac:" + receive_setCloneMac);
            if (receive_setCloneMac.IndexOf("00000") != -1)
                return 1;
            log.writeLog("router3 : setCloneMAC() 服务器返回未成功字段", log.msgType.error);
            return 0;
        }

        //MAC地址最后一位+1传出
        public String getWanMac(String lanMac)
        {
            if (!networkSup.IsMac(lanMac))
            {
                Console.WriteLine("getWanMac() : lanMac参数不是标准MAC");
                return null;
            }

            lanMac = lanMac.ToUpper();   //转大写
            byte[] macBytes = Encoding.Default.GetBytes(lanMac);
            byte[] macBufferBytes = new byte[12];
            int tm = 0;
            foreach (byte a in macBytes)
            {
                if (a != 45)
                    macBufferBytes[tm++] = a;
            }
            macBufferBytes[11]++;
            for (int tm2 = 11; tm2 >= 0; tm2--)
            {
                if (macBufferBytes[0] > 70)
                {
                    Console.WriteLine("getWanMac() : MAC计算溢出");
                    return null;
                }
                if (macBufferBytes[tm2] > 70)
                {
                    macBufferBytes[tm2] = 48;
                    if ((macBufferBytes[tm2 - 1] >= 48 && macBufferBytes[tm2 - 1] <= 56) || (macBufferBytes[tm2 - 1] >= 65 && macBufferBytes[tm2 - 1] <= 69))
                    {
                        macBufferBytes[tm2 - 1] += 1;
                    }
                    else if (macBufferBytes[tm2 - 1] == 57)
                    {
                        macBufferBytes[tm2 - 1] = 65;
                    }
                }
            }
            return networkSup.processMac(Encoding.Default.GetString(macBufferBytes));
        }


        public int setWIFI(
            String routerAcc,
            String routerPwd,
            String wifiName,
            String wifiPwd,
            Boolean Start       //true 启动wifi;  false 关闭wifi;



        )
        {
            String url = "http://" + this.gateway + "/?code=1&asyn=0&id=" + getLinkStok(routerPwd);
            if (wifiName == null)
            {
                log.writeLog("router3 : setWIFI() wifiName参数错误", log.msgType.error);
                return 0;
            }
            if (wifiPwd.Length < 8)
            {
                log.writeLog("router3 : setWIFI() wifiPwd长度不够", log.msgType.error);
                return 0;
            }

            String body_startWIFI = @"id 34
bEnabled 0
uApMode 0
uRegionIndex 0
uChannel 0
uBgnMode 5
uChannelWidth 2
uRTSThreshold 2346
uFragThreashold 2346
uBeaconInterval 100
uPower 1
uDTIMInterval 1
bWMEEnabled 1
bIsolationEnabled 0
bShortPrmbleDisabled 0
bShortGI 1
bBridgeEnabled 0
cBridgedSsid 
cBridgedBssid 00-00-00-00-00-00
uWepIndex 1
uSecurityType 1
cPassWD 
uDetect 1
bTurboOn 0
id 35
uRadiusIp 0.0.0.0
uRadiusGKUpdateIntvl 0
uPskGKUpdateIntvl 0
uKeyLength 0 0
cKeyVal 0 
uKeyLength 1 0
cKeyVal 1 
uKeyLength 2 0
cKeyVal 2 
uKeyLength 3 0
cKeyVal 3 
uRadiusPort 1812
uKeyType 1
uDefaultKey 1
bEnable 1
bBcastSsid 1
cSsid " + wifiName + @"
bSecurityEnable 1
uAuthType 3
uWEPSecOpt 3
uRadiusSecOpt 3
uPSKSecOpt 3
uRadiusEncryptType 1
uPSKEncryptType 3
cRadiusSecret 
cPskSecret " + wifiPwd + @"
bSecCheck 0
bEnabled 1
cUsrPIN 33862170
bConfigured 0
bIsLocked 0";

            String body_offWIFI = @"id 35
uRadiusIp 0.0.0.0
uRadiusGKUpdateIntvl 0
uPskGKUpdateIntvl 0
uKeyLength 0 0
cKeyVal 0 
uKeyLength 1 0
cKeyVal 1 
uKeyLength 2 0
cKeyVal 2 
uKeyLength 3 0
cKeyVal 3 
uRadiusPort 1812
uKeyType 1
uDefaultKey 1
bEnable 0
bBcastSsid 1
cSsid " + wifiName + @"
bSecurityEnable 1
uAuthType 3
uWEPSecOpt 3
uRadiusSecOpt 3
uPSKSecOpt 3
uRadiusEncryptType 1
uPSKEncryptType 3
cRadiusSecret 
cPskSecret " + wifiPwd + @"
bSecCheck 0
bEnabled 1
cUsrPIN 33862170
bConfigured 0
bIsLocked 0";

            String body = Start ? body_startWIFI : body_offWIFI;
            String receive = ToPost(url, body);
            if (receive!=null &&  receive.IndexOf("00000") != -1)
                return 1;
            return 0;
        }



        public int setReboot(
            String routerAcc,
            String routerPwd
        )
        {
            String url = "http://" + this.gateway + "/?code=6&asyn=1&id=" + getLinkStok(routerPwd);
            String body = "";
            try
            {
                String receive = ToPost(url, body);
                if (receive.IndexOf("00000") != -1)
                    return 1;
                return 0;
            }
            catch
            {
                log.writeLog("router3 : setReboot() 未知原因出错", log.msgType.error);
                return 0;
            }
        }


        public bool IsRouterClassType()
        {
            String SLVerCode = getSLVerCode();
            if (SLVerCode == null)
                return false;
            if (IsSLVerCode(SLVerCode))
                return true;
            return false;
        }


    }

}
