
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using cn.softname2.Support;
using cn.softname2.Router.routerTypeDAO;
using cn.softname2.Log;

namespace cn.softname2.routerControl.routerType
{
    /*
     * *********************************************
     *              使用stok(令牌)的路由器
     * *********************************************
     * 该路由器通过本地计算路由密码的加密值，将该值发送至路由器以获得其返回的临时令牌，
     * 将临时令牌加入到之后请求设置路由动作的url里（get），并在http包体里写明请求设置路由器的具体动作（json字符串）
     */
    class router2: Interf_routerType
    {
        network networkSup = new network();
        String gateway = null;
        public int routerType()
        {
            return 2;
        }

        public router2(String gateway)
        {
            this.gateway = gateway;
        }

        static private string a = "RDpbLfCPsJZ7fiv";
        static private string c = "yLwVl0zKqws7LgKPRQ84Mdt708T1qQ3Ha7xv3H7NyU84p21BriUWBU43odz3iP4rBL3cD02KZciXTysVXiV8ngg6vL48rPJyAUw0HurW20xqxv9aYb4M9wK1Ae0wlro510qXeU07kV57fQMc8L6aLgMLwygtc0F10a0Dg70TOoouyFhdysuRMO51yY5ZlOZZLEal1h0t9YQW0Ko7oBwmCAHoic4HYbUyVeU3sfQ1xtXcPcf1aT303wAQhv66qzW";


        //计算本地路由密码加密值
        public string getLocaStok(string a, string routerPwd, string c)
        {
            string e = "";
            int n = 187, l = 187;
            int g = a.Length;
            int h = routerPwd.Length;
            int k = c.Length;
            int f = g > h ? g : h;
            for (int p = 0; p < f; p++)
            {
                n = l = 187;
                if (p >= g)
                {
                    n = routerPwd.ToCharArray()[p];
                }
                else
                {
                    if (p >= h)
                    {
                        l = a.ToCharArray()[p];
                    }
                    else
                    {
                        l = a.ToCharArray()[p];
                        n = routerPwd.ToCharArray()[p];
                    }
                    e += c.ToCharArray()[(l ^ n) % k];
                }
            }
            return e;
        }

        //快捷使用post请求（url/链接，body/负载的包体）
        public virtual string ToPost(string url, string body)
        {
            //url = "http://192.168.1.1/";
            try
            {
                byte[] pass = Encoding.UTF8.GetBytes(body);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Proxy = null;
                request.KeepAlive = false;
                request.ServicePoint.Expect100Continue = false;
                request.ContentLength = body.Length;
                request.ReadWriteTimeout = 1000;        //这里设置超时(事实证明不稳定),应该得用异步解决
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


        //通过路由密码，计算出立即可用的url链接（不包括包体）
        public virtual String getAbleLink(String routerPwd)
        {
            String httpBody_getLinkStok = "{\"method\":\"do\",\"login\":{\"password\":\"" + getLocaStok(a, routerPwd, c) + "\"}}";

            try
            {
                String httpReceive_getLinkStok = ToPost("http://" + this.gateway, httpBody_getLinkStok);
                String LinkStok = new string(httpReceive_getLinkStok.Split(':')[2].ToCharArray(), 1, httpReceive_getLinkStok.Split(':')[2].LastIndexOf("\"") - 1);
                return "http://" + this.gateway + "/stok=" + LinkStok + "/ds";
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("getAbleLink() : 报无返回异常");
                return null;
            }
            catch (Exception)
            {
                Console.WriteLine("getAbleLink() : 报未知异常");
                return null;
            }
            return null;
        }




        public virtual int setInternetMode_AutoIP(String routerAcc, String routerPwd)
        {
            log.writeLog("router2 : setInternetMode_AutoIP 正在执行", log.msgType.info);
            String httpBody_setAutoIP = "{\"protocol\":{\"wan\":{\"wan_type\":\"dhcp\"}},\"method\":\"set\"}";
            String httpReceive_setAutoIp = ToPost(getAbleLink(routerPwd), httpBody_setAutoIP);
            if (httpReceive_setAutoIp!=null  &&  httpReceive_setAutoIp.IndexOf("\"error_code\": 0") != -1)
                return 1;
            else
            {
                log.writeLog("router2 : setInternetMode_AutoIP 执行失败，服务器返回非成功内容", log.msgType.info);
                return 0;
            }
        }

        public virtual int setInternetMode_PPPOE(
            String routerAcc,
            String routerPwd,
            String netAcc,
            String netPwd,
            int callMode,        //拨号模式，0为正常拨号，1234567...对应特殊拨号1234567...    
            int linkMode         //连接模式：1.自动连接，开机和断线后自动连接
                                 //          2.按需连接，有访问时自动连接
                                 //          3.手动连接，由用户手动连接
         )
        {
            String httpBody_setPPPOEConnect = "{\"protocol\":{\"wan\":{\"wan_type\":\"pppoe\"},\"pppoe\":{\"username\":\"" + netAcc + "\",\"password\":\"" + netPwd + "\"}},\"method\":\"set\"}";
            String httpBody_doPPPOEConnect = "{\"network\":{\"change_wan_status\":{\"proto\":\"pppoe\",\"operate\":\"connect\"}},\"method\":\"do\"}";
            String httpBody_getPPPOEOption = "{\"protocol\":{\"name\":[\"wan\",\"pppoe\"]},\"network\":{\"name\":[\"wan_status\",\"iface_mac\"]},\"method\":\"get\"}";
            String httpReceive_getPPPOEOption = ToPost(getAbleLink(routerPwd), httpBody_getPPPOEOption);
            if (httpReceive_getPPPOEOption == null)
            {
                log.writeLog("router2 : setInternetMode_PPPOE() 获取pppoe选项卡信息失败", log.msgType.error);
                return 0;
            }
            String writeInLinkMac = getJsonValue(httpReceive_getPPPOEOption, "factory");    //MAC自动使用路由的MAC作为WAN口MAC,如需更改请使用setCloneMAC()函数

            String writeInLinkCallMode = null;
            if (callMode == 0)
                writeInLinkCallMode = "normal";
            else if (callMode > 0)
            {
                writeInLinkCallMode = "special" + callMode.ToString();
            }
            if (writeInLinkCallMode == null)
            {
                log.writeLog("router2 : setInternetMode_PPPOE() 拨号模式错误(为null)", log.msgType.error);
                return 0;
            }

            String writeInLinkLinkMode = null;
            switch (linkMode)
            {
                case 1: writeInLinkLinkMode = "auto"; break;
                case 2: writeInLinkLinkMode = "demand"; break;
                case 3: writeInLinkLinkMode = "manual"; break;
            }
            if (writeInLinkLinkMode == null)
            {
                log.writeLog("router2 : setInternetMode_PPPOE() 连接模式错误(为null)", log.msgType.error);
                return 0;
            }


            String httpBody_setPPPOEOption = "{\"protocol\":{\"wan\":{\"macaddr\":\"" + writeInLinkMac + "\",\"wan_rate\":\"auto\"},\"pppoe\":{\"dial_mode\":\"" + writeInLinkCallMode + "\",\"conn_mode\":\"" + writeInLinkLinkMode + "\",\"mtu\":\"1480\",\"access\":\"\",\"server\":\"\",\"ip_mode\":\"dynamic\",\"dns_mode\":\"dynamic\",\"proto\":\"none\"}},\"method\":\"set\"}";
            //                                                                          mac地址                                                                   特殊模式3/normal正常               自动拨号/manual手动模式/demand按需连接 
            String httpReceive_setPPPOEOption = ToPost(getAbleLink(routerPwd), httpBody_setPPPOEOption);        //设置mac地址、特殊拨号、连接模式等
            String httpReceive_setPPPOEconnect = ToPost(getAbleLink(routerPwd), httpBody_setPPPOEConnect);      //设置pppoe拨号信息
            String httpReceive_doPPPOEConnect = ToPost(getAbleLink(routerPwd), httpBody_doPPPOEConnect);        //执行pppoe拨号命令
            

            if (httpReceive_setPPPOEOption!= null && httpReceive_setPPPOEOption.IndexOf("\"error_code\": 0") != -1)
            {
                Console.WriteLine("pppoe设置动作成功，函数直接返回成功");
                return 1;

            }
            else
            {
                log.writeLog("router2 : setInternetMode_PPPOE() 函数报:pppoe设置动作失败", log.msgType.error);
                return 0;
            }

        }



        public virtual int setCloneMAC(
            String routerAcc,
            String routerPwd,
            String Mac         //传入"router"为使用路由器地址
                               //    "pc"    为使用本机MAC地址
                               //其余为指定MAC地址，格式用XX-XX-XX-XX-XX-XX
                               //最后有个校验，如果不是这种格式，此函数将返回失败代码0
        )
        {

            String writeInLinkMac = null;
            String httpBody_getPPPOEOption = "{\"protocol\":{\"name\":[\"wan\",\"pppoe\"]},\"network\":{\"name\":[\"wan_status\",\"iface_mac\"]},\"method\":\"get\"}";
            String httpReceive_getPPPOEOption = ToPost(getAbleLink(routerPwd), httpBody_getPPPOEOption);
            if (httpReceive_getPPPOEOption == null)     //如果获取路由原有信息失败
            {
                log.writeLog("router2 : setCloneMAC（） 获取路由原有信息失败", log.msgType.error);
                return 0;
            }

            
            if (Mac.Equals("router"))                   //如果传入参数为"router"
                writeInLinkMac = getJsonValue(httpReceive_getPPPOEOption, "factory");
            else if (Mac.Equals("pc"))                  //如果传入参数为"pc"
            {
                writeInLinkMac = getJsonValue(httpReceive_getPPPOEOption, "host");
            }
            else
            {
                if (!networkSup.IsMac(Mac))
                {                      //如果传入参数不是标准的Mac地址
                    log.writeLog("router2 : setCloneMAC（）  传入了非标准mac", log.msgType.error);
                    return 0;
                }
            }

            String httpbody_setMac = "{\"protocol\":{\"wan\":{\"macaddr\":\"" + writeInLinkMac + "\"}},\"method\":\"set\"}";
            String httpReceive_setMac = ToPost(getAbleLink(routerPwd), httpbody_setMac);
            if (httpReceive_setMac !=null && httpReceive_setMac.IndexOf("\"error_code\": 0") != -1)
                return 1;
            else
            {
                log.writeLog("router2 : setCloneMAC（） 服务器返回未成功字段", log.msgType.error);
                return 0;
            }
                
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
                log.writeLog("router2 : setWIFI（） wifiSSID错误", log.msgType.error);
                return 0;
            }
            if (wifiPwd.Length < 8)
            {
                log.writeLog("router2 : setWIFI（） wifi密码长度不够", log.msgType.error);
                return 0;
            }
            String writeInLinkStart = null;
            if (Start)
                writeInLinkStart = "1";
            else
                writeInLinkStart = "0";
            String httpBody_setWifi = "{\"method\":\"set\",\"wireless\":{\"wlan_host_2g\":{\"enable\":\"" + writeInLinkStart + "\",\"ssid\":\"" + wifiName + "\",\"key\":\"" + wifiPwd + "\"}}}";
            String httpReceive_setWifi = ToPost(getAbleLink(routerPwd), httpBody_setWifi);
            if (httpReceive_setWifi !=null && httpReceive_setWifi.IndexOf("\"error_code\": 0") != -1)
                return 1;
            else
            {
                log.writeLog("router2 : setWIFI（） 服务器返回未成功字段", log.msgType.error);
                return 0;
            }
               
        }


        public virtual int setReboot(
            String routerAcc,
            String routerPwd
        )
        {
            String httpBody_reboot = "{\"system\":{\"reboot\":null},\"method\":\"do\"}";
            String httpReceive_reboot = ToPost(getAbleLink(routerPwd), httpBody_reboot);
            Console.WriteLine(httpReceive_reboot);
            if (httpReceive_reboot!=null && httpReceive_reboot.IndexOf("\"error_code\": 0") != -1)
                return 1;
            else
            {
                log.writeLog("router2 : setReboot（） 服务器返回未成功字段", log.msgType.error);
                return 0;
            }
                
        }



        //获取字符串的"inputParameter"后的下一个双引号内的内容
        public String getJsonValue(String jsonString, String jsonParameter)
        {
            int firstFind = jsonString.IndexOf("\"" + jsonParameter + "\"");
            if (firstFind == -1)
                return null;
            String firstFindAfterString = jsonString.Substring(firstFind + jsonParameter.Length + 2, jsonString.Length - firstFind - jsonParameter.Length - 2);
            int secondStringFirstFind = firstFindAfterString.IndexOf("\"");
            if (secondStringFirstFind == -1)
            {
                return null;
            }
            String secondFindAfterString = firstFindAfterString.Substring(secondStringFirstFind + 1, firstFindAfterString.Length - secondStringFirstFind - 1);
            return secondFindAfterString.Substring(0, secondFindAfterString.IndexOf("\""));
        }

        


        //判断是否为该类类型的路由器
        public virtual bool IsRouterClassType()
        {
            try
            {
                String httpLinkInfo_getSLVerCode = "http://" + this.gateway + "/pc/Content.htm";
                HttpWebRequest request = WebRequest.Create(httpLinkInfo_getSLVerCode) as HttpWebRequest;
                request.Method = "GET";
                request.KeepAlive = true;
                Stream s = request.GetResponse().GetResponseStream();
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                String readToEnd = reader.ReadToEnd();
                request.Abort();
                s.Close();
                reader.Close();
                if (readToEnd!=null && readToEnd.IndexOf("error_code") != -1)
                {
                    return true;
                }
                return false;
            }
            catch (System.Net.WebException)
            {
                Console.WriteLine("IsRouterClassType() 未连接到服务器(WebException异常)");
                return false;
            }
            catch
            {
                Console.WriteLine("IsRouterClassType() 未知异常");
                return false;
            }
            return false;
        }


    }

}
