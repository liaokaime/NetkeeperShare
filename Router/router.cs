
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using cn.softname2.Log;
using cn.softname2.routerControl.routerType;

namespace cn.softname2.routerControl
{
    public class router
    {

        public int routerType;
        private router1 r1 = null;
        private router2 r2 = null;
        private router3 r3 = null;
        private router4 r4 = null;
        private router5 r5 = null;

        public router(String gateway)
        {
            r1 = new router1(gateway);
            r2 = new router2(gateway);
            r3 = new router3(gateway);
            r4 = new router4(gateway);
            r5 = new router5(gateway);

            routerType = 0;
            setRouterType();
            if(routerType==0)
                setRouterType();
            log.writeLog($"路由模块构造方法执行完毕，适配路由器种类为 routerType:{routerType}",log.msgType.info);
            //Thread t1 = new Thread(new ThreadStart(continuousDetection));
            //t1.IsBackground = true;
            //t1.Start();
        }

        //private void continuousDetection()
        //{    //若没有明确支持某个路由器，则此线程每3秒检测一次
        //    while (true)
        //    {
        //        if (this.routerType != 0)
        //            break;
        //        setRouterType();
        //        Thread.Sleep(3000);
        //    }
        //}

        private int setRouterType()
        {
            if (r2.IsRouterClassType())
            {
                this.routerType = 2;
                return 2;
            }
            else if (r3.IsRouterClassType())
            {
                this.routerType = 3;
                return 3;
            }
            else if (r1.IsRouterClassType())
            {
                this.routerType = 1;
                return 1;
            }
            else if (r4.IsRouterClassType())
            {
                this.routerType = 4;
                return 4;
            }
            else if (r5.IsRouterClassType())
            {
                this.routerType = 5;
                return 5;
            }
            else
            {
                this.routerType = 0;
                return 0;
            }
        }


        public bool IsSupportRouterType()
        {
            if (this.routerType > 0)
                return true;
            return false;
        }


        public int setInternetMode_AutoIP(string routerAcc, string routerPwd)
        {
            int ret = 0;
            switch (this.routerType)
            {
                case 0: ret= 0;break;
                case 1: ret = r1.setInternetMode_AutoIP(routerAcc, routerPwd); break;
                case 2: ret = r2.setInternetMode_AutoIP(routerAcc, routerPwd); break;
                case 3: ret = r3.setInternetMode_AutoIP(routerAcc, routerPwd); break;
                case 4: ret = r4.setInternetMode_AutoIP(routerAcc, routerPwd); break;
                case 5: ret = r5.setInternetMode_AutoIP(routerAcc, routerPwd); break;
            }
            log.writeLog($"路由Type{this.routerType} 已经执行【自动获取IP函数】((routerAcc)\"{routerAcc}\",(routerPwd)\"{routerPwd}\")\n返回值：{ret}", log.msgType.info);
            return ret;
        }

        public int setInternetMode_PPPOE(string routerAcc, string routerPwd, string netAcc, string netPwd, int callMode, int linkMode)
        {
            int ret = 0; 
            switch (this.routerType)
            {
                case 0: ret =  0;break;
                case 1: ret = r1.setInternetMode_PPPOE(routerAcc, routerPwd, netAcc, netPwd, callMode, linkMode); break;
                case 2: ret = r2.setInternetMode_PPPOE(routerAcc, routerPwd, netAcc, netPwd, callMode, linkMode); break;
                case 3: ret = r3.setInternetMode_PPPOE(routerAcc, routerPwd, netAcc, netPwd, callMode, linkMode); break;
                case 4: ret = r4.setInternetMode_PPPOE(routerAcc, routerPwd, netAcc, netPwd, callMode, linkMode); break;
                case 5: ret = r5.setInternetMode_PPPOE(routerAcc, routerPwd, netAcc, netPwd, callMode, linkMode); break;
            }
            log.writeLog($"路由Type{this.routerType} 已经执行【PPPOE拨号函数】routerAcc:{routerAcc}\nrouterPwd:{routerPwd}\nnetAcc:{netAcc}\nnetPwd:{netPwd}\ncallMode:{callMode}\nlinkMode:{linkMode}\n返回值:{ret}", log.msgType.info);
            return ret;
        }

        public int setCloneMAC(string routerAcc, string routerPwd, string Mac)
        {
            int ret = 0;
            switch (this.routerType)
            {
                case 0: ret = 0;break;
                case 1: ret = r1.setCloneMAC(routerAcc, routerPwd, Mac); break;
                case 2: ret = r2.setCloneMAC(routerAcc, routerPwd, Mac); break;
                case 3: ret = r3.setCloneMAC(routerAcc, routerPwd, Mac); break;
                case 4: ret = r4.setCloneMAC(routerAcc, routerPwd, Mac); break;
                case 5: ret = r5.setCloneMAC(routerAcc, routerPwd, Mac); break;
            }
            log.writeLog($"路由Type{this.routerType} 已经执行【克隆MAC函数】((routerAcc)\"{routerAcc}\",(routerPwd)\"{routerPwd}\"),(Mac)\"{Mac}\")\n返回值：{ret}", log.msgType.info);
            return ret;
        }

        public int setWIFI(string routerAcc, string routerPwd, string wifiName, string wifiPwd, bool Start)
        {
            int ret = 0;
            switch (this.routerType)
            {
                case 0: ret = 0;break;
                case 1: ret = r1.setWIFI(routerAcc, routerPwd, wifiName, wifiPwd, Start); break;
                case 2: ret = r2.setWIFI(routerAcc, routerPwd, wifiName, wifiPwd, Start); break;
                case 3: ret = r3.setWIFI(routerAcc, routerPwd, wifiName, wifiPwd, Start); break;
                case 4: ret = r4.setWIFI(routerAcc, routerPwd, wifiName, wifiPwd, Start); break;
                case 5: ret = r5.setWIFI(routerAcc, routerPwd, wifiName, wifiPwd, Start); break;
            }
            log.writeLog($"路由Type{this.routerType} 已经执行【设置wifi函数】((routerAcc)\"{routerAcc}\",(routerPwd)\"{routerPwd}\") ,(wifiName)\"{wifiName}\"),(wifiPwd)\"{wifiPwd}\") ,(Start)\"{Start}\") )   \n返回值：{ret}", log.msgType.info);
            return ret;
        }

        public int setReboot(string routerAcc, string routerPwd)
        {
            int ret = 0; 
            switch (this.routerType)
            {
                case 0: ret = 0;break;
                case 1: ret = r1.setReboot(routerAcc, routerPwd); break;
                case 2: ret = r2.setReboot(routerAcc, routerPwd); break;
                case 3: ret = r3.setReboot(routerAcc, routerPwd); break;
                case 4: ret = r4.setReboot(routerAcc, routerPwd); break;
                case 5: ret = r5.setReboot(routerAcc, routerPwd); break;
            }
            log.writeLog($"路由Type{this.routerType} 已经执行【重启路由函数】((routerAcc)\"{routerAcc}\",(routerPwd)\"{routerPwd}\")\n返回值：{ret}", log.msgType.info);
            return ret;
        }

    }

}
