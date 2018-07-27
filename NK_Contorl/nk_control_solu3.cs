using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using cn.softname2.Log;

namespace cn.softname2.NK_Control
{

    class appControl
    {
        private String appName = null;
        private String[] appNames = null;
        private bool caseSensitive = false;
        
        private String getAppName() {
            if (appName != null)
                return appName;
            else
            {
                String app = "";
                foreach(String s in appNames)
                {
                    app += s + "/";
                }
                return app;
            }
            log.writeLog("appControl模块的获取应用名失败", log.msgType.error);
        }

        public appControl(String appName, bool caseSensitive)
        {
            this.appName = appName;
            this.caseSensitive = caseSensitive;
        }

        public appControl(String[] appNames, bool caseSensitive)
        {
            this.appNames = appNames;
            this.caseSensitive = caseSensitive;
        }

        public String getAppPath()
        {
            RegistryKey currentKey = null;
            RegistryKey pregkey = null;
            try
            {
                pregkey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths");//获取指定路径下的键
                foreach (string item in pregkey.GetSubKeyNames())
                {
                    currentKey = pregkey.OpenSubKey(item);
                    String path = (string)currentKey.GetValue(null);    //默认键
                    if (matching(path, this.caseSensitive))   //false 不区分大小写
                    {
                        return path;     //try{return1} catch{return2} finally{return3}执行顺序,try和catch中的return在finally后执行，如果finally中有return，优先一切return
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
            return null;
        }
        public String getAppPathRegion()
        {
            String mainProPath = getAppPath();
            if (mainProPath == null)
                return null;
            int flag = mainProPath.LastIndexOf("\\");
            String mainProPathRegion = mainProPath.Substring(0, flag + 1);
            return mainProPathRegion;
        }

        private bool matching(String a, String b, bool caseSensitive)     //caseSensitive 是否区分大小写
        {
            if (a == null || b == null)
                return false;
            if (caseSensitive)
            {
                if (a.IndexOf(b) != -1)
                    return true;
                return false;
            }
            else
            {
                if (a.ToLower().IndexOf(b.ToLower()) >= 0)
                    return true;
                return false;
            }

        }
        private bool matching(String a, String[] b, bool caseSensitive)
        {
            if (a == null || b == null)
                return false;
            if (caseSensitive)
            {
                foreach (String c in b)
                {
                    if (a.IndexOf(c) != -1)
                        return true;
                }
                return false;
            }
            else
            {
                foreach (String c in b)
                {
                    if (a.ToLower().IndexOf(c.ToLower()) != -1)
                        return true;
                }
                return false;
            }

        }
        private bool matching(String a, bool caseSensitive)
        {
            if (a == null)
                return false;
            if (this.appName != null)
                if (matching(a, this.appName, caseSensitive))
                    return true;

            if (this.appNames != null)
                if (matching(a, this.appNames, caseSensitive))
                    return true;
            return false;
        }


        public void runApp()
        {
            try
            {
                String appPathRegion = getAppPathRegion();
                String appPath = getAppPath();
                
                if (appPathRegion == null || appPath == null)
                {
                    
                    log.writeLog($"启动应用 {getAppName()} 失败,程序内置注册表目录下不存在该应用",log.msgType.error);
                    return;
                }
                ProcessStartInfo appInfo = new ProcessStartInfo();
                appInfo.WorkingDirectory = appPathRegion;
                appInfo.FileName = appPath;
                Process pcs = Process.Start(appInfo);
                log.writeLog($"启动应用 {getAppName()} 完成", log.msgType.info);
            }
            catch
            {
                log.writeLog($"启动应用 {getAppName()} 时发生异常", log.msgType.error);
            }
        }


        public bool IsAlive()
        {
            Process[] vProcesses = Process.GetProcesses();
            foreach (Process p in vProcesses)
            {
                if (matching(p.MainWindowTitle, this.caseSensitive) || matching(p.ToString(), this.caseSensitive))
                {
                    log.writeLog($"上 搜索到", log.msgType.debug);
                    return true;
                }
                try
                {
                    if (matching(p.MainModule.FileVersionInfo.FileDescription, this.caseSensitive))
                    {
                        log.writeLog($"下 搜索到", log.msgType.debug);
                        return true;
                    }
                       
                }
                catch { }   //某些不可访问进程无法获取其描述
            }
            return false;
        }


        public void closeApp()
        {
            try
            {
                Process[] vProcesses = Process.GetProcesses();
                foreach (Process p in vProcesses)
                {
                    if (matching(p.MainWindowTitle, this.caseSensitive) || matching(p.ToString(), this.caseSensitive))
                    {
                        p.Kill();
                        log.writeLog($"关闭{getAppName()}完成", log.msgType.info);
                        return;
                    }
                }
            }
            catch
            {
                log.writeLog($"关闭{getAppName()}时发未知异常", log.msgType.warning);
            }
        }

    }

    class NKControl
    {
        appControl NKApp = new appControl("netkeeper", false);

        public void runNK()
        {
            NKApp.runApp();
        }

        public bool IsAlive()
        {
            return NKApp.IsAlive();
        }

        public void closeNK()
        {
            NKApp.closeApp();
        }

    }
}
