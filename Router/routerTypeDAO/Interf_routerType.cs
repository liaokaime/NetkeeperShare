using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cn.softname2.Router.routerTypeDAO
{
    interface Interf_routerType
    {
        //充当字段，路由类型int代号
        int routerType();

        /// <summary>
        /// 设置自动获取IP
        /// </summary>
        int setInternetMode_AutoIP(
            String routerAcc,   //不需要账号的路由器routerAcc填"admin"或""
            String routerPwd
        );

        /// <summary>
        /// 设置PPPOE拨号
        /// </summary>
        int setInternetMode_PPPOE(
            String routerAcc,
            String routerPwd,
            String netAcc,
            String netPwd,
            int callMode,        //拨号模式，0为正常拨号，1234567...对应特殊拨号1234567...    
            int linkMode         //连接模式：1.自动连接，开机和断线后自动连接
                                 //          2.按需连接，有网络时自动连接
                                 //          3.手动连接，由用户手动连接
         );

        /// <summary>
        /// 设置克隆MAC
        /// </summary>
        int setCloneMAC(
            String routerAcc,
            String routerPwd,
            String Mac         //传入"router"为使用路由器地址
                               //    "pc"    为使用本机MAC地址
                               //其余为指定MAC地址，格式用XX-XX-XX-XX-XX-XX
                               //最后有个校验，如果不是这种格式，此函数将返回失败代码0
        );

        /// <summary>
        /// 设置WIFI
        /// </summary>
        int setWIFI(
            String routerAcc,
            String routerPwd,
            String wifiName,
            String wifiPwd,
            Boolean Start       //true 启动wifi;  false 关闭wifi;
        );

        /// <summary>
        /// 设置重启MAC
        /// </summary>
        int setReboot(
            String routerAcc,
            String routerPwd
        );

        /// <summary>
        /// 是否为这个类的类型的路由器
        /// </summary>
        bool IsRouterClassType();


    }
}
