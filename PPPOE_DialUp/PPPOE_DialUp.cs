using DotRas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using cn.softname2.Log;

namespace cn.softname2.PPPOE_DialUp
{

    /*
     *  该类：
     *      public bool Connect(string PPPOEname, string username, string password);    连接
     *      public void Disconnect()                                                    断开
     *      void CreateOrUpdatePPPOE(string updatePPPOEname)                            创建连接接口
     *      
     */
    class locaPPPOEConnect
    {
        /// <summary>
        /// 创建或更新一个PPPOE连接(指定PPPOE名称)
        /// </summary>
        void CreateOrUpdatePPPOE(string updatePPPOEname)
        {
            RasDialer dialer = new RasDialer();
            RasPhoneBook allUsersPhoneBook = new RasPhoneBook();
            string path = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
            allUsersPhoneBook.Open(path);
            // 如果已经该名称的PPPOE已经存在，则更新这个PPPOE服务器地址
            if (allUsersPhoneBook.Entries.Contains(updatePPPOEname))
            {
                allUsersPhoneBook.Entries[updatePPPOEname].PhoneNumber = " ";
                // 不管当前PPPOE是否连接，服务器地址的更新总能成功，如果正在连接，则需要PPPOE重启后才能起作用
                allUsersPhoneBook.Entries[updatePPPOEname].Update();
            }
            // 创建一个新PPPOE
            else
            {
                string adds = string.Empty;
                ReadOnlyCollection<RasDevice> readOnlyCollection = RasDevice.GetDevices();
                //                foreach (var col in readOnlyCollection)
                //                {
                //                    adds += col.Name + ":" + col.DeviceType.ToString() + "|||";
                //                }
                //                _log.Info("Devices are : " + adds);
                // Find the device that will be used to dial the connection.
                RasDevice device = RasDevice.GetDevices().Where(o => o.DeviceType == RasDeviceType.PPPoE).First();
                RasEntry entry = RasEntry.CreateBroadbandEntry(updatePPPOEname, device);    //建立宽带连接Entry
                entry.PhoneNumber = " ";
                allUsersPhoneBook.Entries.Add(entry);
            }
        }

        /// <summary>
        /// 断开 宽带连接
        /// </summary>
        public void Disconnect()
        {
            ReadOnlyCollection<RasConnection> conList = RasConnection.GetActiveConnections();
            foreach (RasConnection con in conList)
            {
                con.HangUp();
            }
        }

        /// <summary>
        /// 宽带连接，成功返回true,失败返回 false
        /// </summary>
        /// <param name="PPPOEname">宽带连接名称</param>
        /// <param name="username">宽带账号</param>
        /// <param name="password">宽带密码</param>
        /// <returns></returns>
        public bool Connect(string PPPOEname, string username, string password)
        {
            log.writeLog("正在进行本地拨号", log.msgType.info);
            try
            {
                CreateOrUpdatePPPOE(PPPOEname);
                log.writeLog("创建PPPOE接口完成", log.msgType.info);
                using (RasDialer dialer = new RasDialer())
                {
                    dialer.EntryName = PPPOEname;
                    dialer.AllowUseStoredCredentials = true;
                    dialer.Timeout = 1000;
                    dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
                    dialer.Credentials = new NetworkCredential(username, password);
                    dialer.Dial();
                    log.writeLog($"本地拨号成功,账号：{username}\n密码：{password}", log.msgType.error);
                    return true;
                }
            }
            catch (RasException re)
            {
                log.writeLog("本地拨号捕获到未知异常，拨号失败！",log.msgType.error);
                return false;
            }
        }
    }
}
