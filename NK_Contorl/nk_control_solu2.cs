using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using cn.softname2.Log;


//控制台工程调试
//取得默认键
//总体修改
//路由器类log详细度增加

namespace cn.softname2.NK_Control
{
    

    class startNK2
    {
        private string nkname = "NetKeeper";

        //public bool startNk()
        //{
        //    ProcessStartInfo startProcess = new ProcessStartInfo();
        //    startProcess 
        //}

        public bool startNk(string path)
        {
            bool result = false;
            if (path == null || path.Equals(""))
                return false;
            ProcessStartInfo nkinfo = new ProcessStartInfo();
            nkinfo.WorkingDirectory = path;
            nkinfo.FileName = $@"{path}\{nkname}.exe";
            int timenum = 8;
            int sleeptime = 1000;
            Process.Start(nkinfo);
            while (result == false && (timenum-- > 0))
            {
                Thread.Sleep(sleeptime);
                Process[] vProvess = Process.GetProcesses();
                foreach (var item in vProvess)
                {
                    Console.WriteLine(item.MainWindowTitle);
                    if (item.MainWindowTitle == nkname)
                    {
                        Console.WriteLine(item.MainWindowTitle);
                        result = true;
                        break;
                    }
                }
            }
            if (result)
                log.writeLog("NK启动成功", log.msgType.info);
            else
                log.writeLog("NK启动失败", log.msgType.error);
            return result;
        }
        
        public void killNk()
        {
            try
            {
                Process[] vProcesses = Process.GetProcesses();
                foreach (Process vProcess in vProcesses)
                {
                    if (nkname == vProcess.MainWindowTitle)
                    {
                        vProcess.Kill();
                        log.writeLog("NK关闭操作完成", log.msgType.info);
                    }
                }
                log.writeLog("NK关闭操作失败", log.msgType.error);
            }
            catch
            {
                log.writeLog("NK关闭操作捕捉到异常", log.msgType.error);
            }
            
        }

        public bool isAlive()
        {
            bool result = false;
            Process[] vProcesses = Process.GetProcesses();
            foreach (Process vProcess in vProcesses)
            {
                if (nkname == vProcess.MainWindowTitle)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        //public String 

        public string findNk()
        {
            string nkpath = null;
            RegistryKey currentKey = null;
            RegistryKey pregkey = null;
            try
            {
                pregkey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths");//获取指定路径下的键
                foreach (string item in pregkey.GetSubKeyNames())
                {
                    if (item.Equals("NetKeeper.exe"))
                    {
                        currentKey = pregkey.OpenSubKey(item);
                        string paths = (string)currentKey.GetValue("INSTDIR");
                        if (paths != null)
                        {
                            nkpath = paths;
                        }
                        currentKey.Close();
                    }
                }
            }
            finally
            {
                if (currentKey != null)
                    currentKey.Close();
                if (pregkey != null)
                    pregkey.Close();
            }
            return nkpath;
        }

        public bool hasNk()
        {
            bool result = false;
            if (findNk() != null)
                result = true;
            return result;
        }

    }
}
