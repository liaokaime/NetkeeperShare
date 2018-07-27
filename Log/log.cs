using cn.softname2.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using sunflower.RSAStandard;



/*
 * author:Liao.
 * 软件日志模块
 *      1.支持多线程、多实例同时进行日志写入操作
 *          支持该操作的机制为日志的队列模型
 *      2.日志默认路径偏向于softname软件定制方向
 *      
 * public API:
 *      static public enum msgType
 *      {
 *          debug=0,
 *          info,
 *          warning,
 *          error,
 *          fatal
 *      }
 *      static public void writeLog(String message,enum msgType);   //写入日志内容
 *      static public void openFolder();                            //打开文件夹并选中该类filePath字段的文件
 *      static public void encriptMode(bool encript);               //是否启用日志加密  //该方法可以随时调整加密模式，以使调用者可以控制单条日志的加密与否
 */

namespace cn.softname2.Log
{
    class log
    {
        static String filePath = ".\\softname.log";
        static Queue<String> log_Queue = new Queue<String>();
        static Thread t = new Thread(engine);
        static bool encript=true;       //默认日志加密模式为'是'
        //公钥
        static String publicKey = @"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmTJjJXOo/DGBeIbADIPLmspwLbBajTavuV2hWFE02U+7wOMtWbRpf3l5UIiV1E/ErVYW8gCr7sPQRTTkZAef7cw3A8nei3afvJcneGQuzfNlVGk2s8n++6G73o2nFavIn/PxnqBDhuviyv86z7CTE71W3eP9oA0cjjEM1RtmYUy4MXy473viK8MGLS7qwm8ubkbNE8Q8e54zJRufK3PygQmldo+5LxLfg3ewYDoehcZF5VbohiIBiFXVaCirtFaFGJu+uyTKnPBez4x/+gkjlCZescXji0jwnm3JWTR/MucdoVNrmt4sb/0OjjVXywgVwy63JecB2c5BU8Oh8EJJJwIDAQAB";
        //私钥
        //String privateKey = @"gHKy6uSvhBiqRXnPd/AUtXkZF0BqDAhDsmIi14+bfDjkgBK5LZZmaFFkGwBDscXcFF+6BQY5yvVTB9pTk9+ewC7IbzLfz4iQq16qF0Y5HQZ9+ZsHgC3zyTfa7MdujTg8GPuePuBYl4/OdCRb/mOByAfz3CrGQUHNDoxOlmUk04mBh3a7cGNR7fRIX/GdR/6WFLnPMDhgmJJiimWyDgLQOJ5VwkBW4HoVAiDzT3MsN42sOs3ACEL90FWvTNU4onB2r9V1WTAWhJUt1cMJ44/khqwdYoWIPwRBNvo0asviIzfU/VqoE34eeKyiYk7qtHH4neQL3iNa5bw3jXqPhUOHCKU=";

        static public void init(String path) {
            if (path == null)
            {
                filePath = ".\\softname.log";    //default path
            }  
            else {
                filePath = path;
            }
            if (!File.Exists(filePath))
            {
                FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs); // 创建写入流
                sw.Write("\n======================程序日志======================\n");   // 写入内容
                sw.Close();             //关闭写入流
                fs.Close();             //关闭文件
            }

            if (!t.IsAlive)
            {
                log.encript = encript;
                FileStream fs = new FileStream(filePath, FileMode.Append);
                StreamWriter sw = new StreamWriter(fs); // 创建写入流
                sw.Write($"\r\n\r\n\r\n/*\r\n * 日志时间：{DateTime.Now.ToLocalTime().ToString()}\r\n */");      //写入内容
                sw.Close();             //关闭写入流
                fs.Close();             //关闭文件
                t.Start();
            }
            
        }


        static public String[] splitStr(String str, int len)
        {
            if (len < 1)
                return null;
            if (str == null)
                return null;
            int strnum = str.Length / len + (str.Length % len == 0 ? 0 : 1);
            string[] result = new string[strnum];
            int end = 0;
            int p = 0;
            while (end < str.Length)
            {
                if (end + len <= str.Length - 1)
                {
                    result[p++] = str.Substring(end, len);
                }
                else
                {
                    result[p++] = str.Substring(end, str.Length - end);
                }
                end += len;
            }
            return result;
        }


        private static void engine()
        {
            while (true)
            {
                try
                {
                    String str = log_Queue.Dequeue();
                    try
                    {
                        String str2 = "";
                        if (log.encript)
                        {
                            String[] strArr = splitStr(str, 60);
                            foreach(String s in strArr)
                            {
                                str2 += "<encript>" + new RSAStandard().RSAEncryptByPub(s, log.publicKey) + "</encript>";
                            }
                            //str = "<encript>"+RSAHelper.EncryptString(str, log.publicKey)+ "</encript>";
                        }
                        FileStream fs = new FileStream(filePath, FileMode.Append);
                        StreamWriter sw = new StreamWriter(fs); // 创建写入流
                        sw.Write(str2);          //写入内容
                        sw.Flush();
                        sw.Close();             //关闭写入流
                        fs.Flush();
                        fs.Close();             //关闭文件
                    }
                    catch
                    {
                        Console.WriteLine("engine() : catch an unkonw error");
                    }
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }
        }

        static public void encriptMode(bool encript)
        {
            log.encript = encript;
        }


        #region
        //key标签包裹value
        private String addLogFileKey(String key,String value)
        {
            String start = $"<{key}>";
            String end   = $"</{key}>";
            return start + value + end;
        } 
        //从长字符串中找出被key包裹的value
        private String getLogFileValue(String key,String fileString)
        {
            String start = $"<{key}>";
            String end   = $"</{key}>";
            int startIndex = fileString.IndexOf(start);
            int endIndex   = fileString.IndexOf(end);
            if (startIndex!=-1 && endIndex!=-1)
            {
                String value = fileString.Substring(startIndex+ start.Length, endIndex-startIndex- start.Length);
                return value;
            }
            else
            {
                return null;
            }
        }
        #endregion


        public enum msgType
        {
            debug=0,
            info,
            warning,
            error,
            fatal
        }

        static public void writeLog(String message,log.msgType enumType) {
            int type = (int)enumType;
            if (Enum.GetNames(typeof(msgType)).Length < type)
            {
                Console.Write("writeLog() : parameter[type] value out of range!");
                return;
            }
            String time= DateTime.Now.ToLocalTime().ToString();
            String logType = Enum.GetNames(typeof(msgType))[type];
            String line = "\r\n----------------------------------------------------\r\n";
            String singleData = $"{line}{time,-20}日志级别:{logType,-5}\r\n{message}";
            log_Queue.Enqueue(singleData);
        }

        static public bool openFolder()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,{filePath}");
                    return true;
                }
                else
                {
                    log.writeLog("打开日志路径失败，日志文件不存在:", log.msgType.warning);
                    return false;
                }
            }
            catch
            {
                log.writeLog($"因未知异常打开日志路径失败，日志路径:{filePath}", log.msgType.warning);
                return false;
                //Console.WriteLine("openFolde() : 未知异常 || 打开日志路径失败");
            }
        }

        //public String decode(String src, String privateKey)
        //{
        //    for (int i = 0; true; i++)
        //    {
        //        int start = src.IndexOf("<encript>");
        //        int end = src.IndexOf("</encript>");
        //        if (start == -1 || end == -1)
        //            break;
        //        String singleEncode = src.Substring(start, end - start + "</encript>".Length);
        //        String singleEncode_pure = getLogFileValue("encript", singleEncode);
        //        String singleDecode = DecryptString(singleEncode_pure, privateKey);
        //        src = src.Replace(singleEncode, singleDecode);
        //    }
        //    return src;
        //}



        //static void Main(string[] args)
        //{
        //    //log lg =new log(".\\a.log");
        //    //Console.WriteLine("complete!");
        //    new log(null).writeLog("程序日志记录内容", (int)msgType.debug);
        //    new log(null).writeLog("程序日志记录内容", (int)msgType.debug);
        //    new log(null).writeLog("程序日志记录内容", (int)msgType.debug);
        //    new log(null).writeLog("程序日志记录内容", (int)msgType.debug);


        //}
    }
}
