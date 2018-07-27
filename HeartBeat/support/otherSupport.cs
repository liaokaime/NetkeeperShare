using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace cn.softname2.HeartBeat
{
    class otherSupport
    {
        //获取公网IP
        public String getPublicIP()
        {
            try
            {
                Uri uri = new Uri("http://city.ip138.com/ip2city.asp");
                System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
                req.Method = "get";
                Stream s = req.GetResponse().GetResponseStream();
                StreamReader reader = new StreamReader(s);
                char[] ch = { '[', ']' };
                string str = reader.ReadToEnd();
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(str, @"\[(?<IP>[0-9\.]*)\]");
                return m.Value.Trim(ch);
            }
            catch
            {
                Console.WriteLine("getPublicIP() : 未知错误");
                return null;
            }
        }
    }
}
