using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace cn.softname2.NK_Control
{
    class startNK
    {
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, uint wParam, uint lParam);
        const int WM_QUIT = 0x12;
        public IntPtr nkotr;
        private static string nkclassname = "CnetkeeperUIDlg";
        private static string nkname = "NetKeeper.exe";


        public bool start()
        {
            string path = findnk();
            Console.WriteLine(path + " 1");
            if (path == "")
                return false;
            ProcessStartInfo nkinfo = new ProcessStartInfo();
            nkinfo.WorkingDirectory = path;  //设置 工作环境 也就是 安装路径
            nkinfo.FileName = path + @"\" + nkname;  //设置启动的路径
            Process.Start(nkinfo);
            bool flag = false;
            IntPtr nkhandle = IntPtr.Zero;
            int timenum = 15;   //   timenum =  15000 / 500  计时 15s
            int sleeptime = 1000;  //设置每次延时多久,毫秒为单位
            while (nkhandle == IntPtr.Zero && (timenum-- > 0))
            {
                Thread.Sleep(sleeptime);
                nkhandle = FindWindow(nkclassname, null);
                flag = true;
            }
            nkotr = nkhandle;
            return flag;
        }

        public void killnk()
        {
            Process[] vProcesses = Process.GetProcesses();
            foreach (Process vProcess in vProcesses)
            {
                if (nkotr.ToString() == vProcess.MainWindowHandle.ToString())
                {
                    vProcess.Kill();
                }
            }
        }

        public bool IsAlive()
        {
            Process[] vProcesses = Process.GetProcesses();
            foreach (Process vProcess in vProcesses)
            {
                if (nkotr.ToString() == vProcess.MainWindowHandle.ToString())
                {
                    return true;
                }
            }
            return false;
        }

        public string findnk()
        {

            string nkpathss = "";
            RegistryKey currentKey = null;
            Console.WriteLine(2);
            RegistryKey pregkey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths");//获取指定路径下的键
            Console.WriteLine(3);
            foreach (string item in pregkey.GetSubKeyNames())               //循环所有子键
            {
                if (item.Equals("NetKeeper.exe"))
                {
                    currentKey = pregkey.OpenSubKey(item);
                    string paths = (string)currentKey.GetValue("INSTDIR");
                    if (paths != null)
                    {
                        nkpathss = paths;
                    }
                    currentKey.Close();
                }
            }
            pregkey.Close();
            return nkpathss;
        }
    }

}
