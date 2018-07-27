using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using cn.softname2.Log;
using System.Drawing;
using cn.softname2.Support;
using cn.softname2.PPPOE_Deceive;
using cn.softname2.NK_Control;
using System.Threading;
using cn.softname2.HeartBeat;

/*
 *  下版本应做：
 *       win8蓝屏问题
 */



namespace cn.softname2.UI
{
    partial class Form
    {

        String easy_routerAcc = null;
        String easy_routerPwd = null;

        //中英混合文字 右对齐
        private string PadRightEx(string str, int totalByteCount, char c)
        {
            Encoding coding = Encoding.GetEncoding("gb2312");
            int dcount = 0;
            foreach (char ch in str.ToCharArray())
            {
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            string w = str.PadLeft(totalByteCount - dcount, c);
            return w;
        }
        //点击关于文字链接 ->   提示框
        private void exception_linkLabel_about_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            String aboutContent = PadRightEx("软件版本：", 16, ' ') + " nkshare_2.2.6_Alpha\n"
                                + PadRightEx("支持地区：", 16, ' ') + " 江西等\n"
                                + PadRightEx("更新频率：", 16, ' ') + " 不定期\n"
                                + PadRightEx("获取/授权：", 16, ' ') + " QQ群 241108736\n"
                                + PadRightEx("_Author：", 16, ' ') + " Liao、Tang\n"
                                + PadRightEx("___备注：", 16, ' ') + " 扫码客户端支持\n";
            DevExpress.XtraEditors.XtraMessageBox.Show(aboutContent, "关于", MessageBoxButtons.OK, MessageBoxIcon.None);
            log.writeLog("用户点开了'关于'对话框", log.msgType.info);
        }

        //点击日志文字链接  ->  打开日志文件路径
        private void exception_linkLabel_log_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (  log.openFolder()  )
            {
                pubFun_setText_simpleButton_showUser("已打开日志路径，如使用过程中有问题，请把日志提交给群主",Color.White);
                log.writeLog("用户点开了'日志'文件路径", log.msgType.info);
            }
            else
            {
                String warningText = "打开日志路径失败，可能是日志被损坏或不存在，已重建日志文件";
                pubFun_setText_simpleButton_showUser(warningText, Color.Orange);
            }
        }

        #region      //将前台数据实时映射到后台
        //改变Mac框文字内容时
        private void normal_comboBox_Mac_TextChanged(object sender, EventArgs e)
        {
            if (normal_comboBox_Mac.Text.Equals("本机MAC"))
            {
                cloneMAC = "pc";
            }
            else if (normal_comboBox_Mac.Text.Equals("路由MAC"))
            {
                cloneMAC = "router";
            }
            else
            {
                cloneMAC = normal_comboBox_Mac.Text; 
            }
        }
        //宽带密码实时映射
        private void normal_textBox_netPwd_TextChanged(object sender, EventArgs e)
        {
            netPwd = normal_textBox_netPwd.Text;
        }
        //路由管理员实时映射
        private void normal_textBox_routerAcc_TextChanged(object sender, EventArgs e)
        {
            routerAcc = normal_textBox_routerAcc.Text;
        }
        //路由密码实时映射
        private void normal_textBox_routerPwd_TextChanged(object sender, EventArgs e)
        {
            routerPwd = normal_textBox_routerPwd.Text;
        }
        //特殊拨号实时映射
        private void normal_comboBox_special_TextChanged(object sender, EventArgs e)
        {
            for (int i=1;i<=7;i++)
            {
                if (normal_comboBox_special.Text.IndexOf(i.ToString()) != -1)
                {
                    special = i;
                    return;
                }
            }
            special = 0;
        }
        //保存选项实时映射
        private void normal_checkBox_save_CheckedChanged(object sender, EventArgs e)
        {
            saveConfig = normal_checkBox_save.Checked;
        }
        //wifi账号实时映射
        private void normal_textBox_wifiAcc_TextChanged(object sender, EventArgs e)
        {
            wifiAcc = normal_textBox_wifiAcc.Text;
        }
        //wifi密码实时映射
        private void normal_textBox_wifiPwd_TextChanged(object sender, EventArgs e)
        {
            wifiPwd = normal_textBox_wifiPwd.Text;
        }
        #endregion


        #region     //普通start按钮集
        //normal_button_Start 点击事件
        private void normal_button_Start_Click(object sender, EventArgs e)
        {
            if (!UI_verifyControlsLegal())
                return;
            log.writeLog("用户点击了normal Start按钮",log.msgType.info);
            if ((t_dispEasyStart == null || !t_dispEasyStart.IsAlive) && (t_dispNormalStart == null || !t_dispNormalStart.IsAlive))
            {
                t_dispNormalStart = new Thread(dispNormalStart);
                t_dispNormalStart.Start();
            }
        }
        //线程，处理normal_button_Start按钮事件
        Thread t_dispNormalStart = null;
        private void dispNormalStart()
        {
            if (r.setInternetMode_AutoIP(easy_routerAcc, easy_routerPwd) != 1)//先设成自动ip以过二维码
            {
                pubFun_setText_simpleButton_showUser("路由账号或密码错误", Color.Red);
            }
            PPPOEDiceive pppoeDic = new PPPOEDiceive();     //实例化PPPOE模块
            startNK2 NK = new startNK2();                     //实例化NK_Control模块
            network netSup = new network();

            pppoeDic.Start();
            pubFun_setText_simpleButton_showUser("请在netkeeper上登录", Color.White);
            NK.startNk(NK.findNk());     //启动NK
            //Thread.Sleep(800);                              //等待NK客户端启动，以免客户端未启动就被检测到了NK已退出
            while (true)
            {
                //if (!NK.isAlive())
                //{
                //    pppoeDic.Close();   //结束抓包循环
                //    pubFun_setText_simpleButton_showUser("客户端已被关闭", Color.Red);
                //    log.writeLog("NK客户端关闭", log.msgType.warning);
                //    return;
                //}
                if (pppoeDic.netAcc != null)    //如果pppoe模块抓取到了账号
                {
                    //NK.killNk();   //结束NK
                    pubFun_setText_simpleButton_showUser("正在处理...", Color.White);
                    if (pppoeDic.netAcc.IndexOf(netAcc()) != -1 && netAcc().Length >= 11)    //如果pppoeDic模块抓取的账号和已授权账号一致
                    {
                        int callPPPOEResult = r.setInternetMode_PPPOE(routerAcc, routerPwd, pppoeDic.netAcc, netPwd, special, 3);   //路由PPPOE拨号,手动连接
                        if (callPPPOEResult == 1)   //如果路由PPPOE拨号成功
                        {
                            pubFun_setText_simpleButton_showUser("正在检测网络连接", Color.White);
                            heartBeat hb = new heartBeat(1, 1000 * 60 * 5, netAcc(), netSup.getLocaMac());    //初始化heartbeat对象

                            if (netSup.PingIpOrDomainName("www.baidu.com"))      //测试网络连接状态
                            {
                                hb.start(); //启动心跳
                                pubFun_setText_simpleButton_showUser("网络已连接,心跳已启动", Color.White);
                                return;
                            }
                            else                                                //没连接的话
                            {
                                if (netSup.PingIpOrDomainName("www.baidu.com")) //再试一遍是否联网
                                {
                                    hb.start(); //启动心跳
                                    pubFun_setText_simpleButton_showUser("网络已连接,心跳已启动", Color.White);
                                    return;
                                }
                                else
                                    pubFun_setText_simpleButton_showUser("网络未连接，请注意克隆MAC可用性和宽带密码正确性", Color.Red);
                            }
                        }
                        else
                        {
                            pubFun_setText_simpleButton_showUser("拨号失败，请检查路由的账号或密码", Color.Red);
                        }
                    }
                    else
                    {
                        pubFun_setText_simpleButton_showUser("客户端输入了不合法账号", Color.Red);
                    }
                    break;
                }
                Thread.Sleep(200);
            }   //end of while(true) loop
        }
        #endregion

        #region     //简易start按钮集
        //easy_button_Start 点击事件
        Thread t_dispEasyStart = null;
        private void easy_button_Start_Click(object sender, EventArgs e)
        {
            log.writeLog("用户点击了'简易Start'按钮", log.msgType.info);
            if (netAccBase64 == null || netAccBase64.Equals("") || netAcc() == null || netAcc().Equals(""))
            {
                pubFun_setText_simpleButton_showUser("请激活软件", Color.Red);
                return;
            }
            if (easy_textBox_routerAcc.Visible && (easy_textBox_routerAcc.Text==null || easy_textBox_routerAcc.Text.Equals("")))
            {
                pubFun_setText_simpleButton_showUser("请输入路由管理员", Color.Red);
                return;
            }
            if (easy_textBox_routerPwd.Text == null || easy_textBox_routerPwd.Text.Equals(""))
            {
                pubFun_setText_simpleButton_showUser("请输入路由密码", Color.Red);
                return;
            }
            
            if (!easy_textBox_routerAcc.Visible)
                easy_routerAcc = "admin";
            else
                easy_routerAcc = easy_textBox_routerAcc.Text;
            easy_routerPwd = easy_textBox_routerPwd.Text;
            //以上，前台限制处理完成
            
            //启动easy_start处理线程
            if ((t_dispEasyStart == null || !t_dispEasyStart.IsAlive) && (t_dispNormalStart==null || !t_dispNormalStart.IsAlive))
            {
                t_dispEasyStart = new Thread(dispEasyStart);
                t_dispEasyStart.Start();
            }
        }
        //线程，处理easy_start按钮事件
        private void dispEasyStart()
        {

            //if (r.setInternetMode_AutoIP(easy_routerAcc, easy_routerPwd)!=1)//先设成自动ip以过二维码
            //{
            //    pubFun_setText_simpleButton_showUser("路由账号或密码错误", Color.Red);
            //}
            PPPOEDiceive pppoeDic = new PPPOEDiceive();     //实例化PPPOE模块
            //NKControl NK = new NKControl();                 //实例化NK_Control模块
            startNK2 NK = new startNK2();                     //实例化NK_Control模块
            network netSup = new network();
            pppoeDic.Start();
            pubFun_setText_simpleButton_showUser("请在netkeeper上登录", Color.White);
            //NK.runNK();     //启动NK
            NK.startNk(NK.findNk());     //启动NK
            while (true)
            {
                //if (!NK.isAlive())
                //{
                //    pppoeDic.Close();   //结束抓包循环
                //    pubFun_setText_simpleButton_showUser("客户端已被关闭", Color.Red);
                //    return;
                //}
                if (pppoeDic.netAcc != null)    //如果pppoe模块抓取到了账号
                {
                    //NK.closeNK();   //结束NK
                    //NK.killNk();   //结束NK
                    pubFun_setText_simpleButton_showUser("正在处理...", Color.White);
                    if (pppoeDic.netAcc.IndexOf(netAcc())!=-1 && netAcc().Length>=11) {
                        if (r.routerType != 1)
                            r.setCloneMAC(easy_routerAcc, easy_routerPwd, "pc");  //先试pc的mac

                        int callPPPOEResult = r.setInternetMode_PPPOE(easy_routerAcc, easy_routerPwd, pppoeDic.netAcc, pppoeDic.netPwd, 0, 1);   //路由PPPOE拨号
                        if (callPPPOEResult == 1)   //如果路由PPPOE拨号成功
                        {
                            pubFun_setText_simpleButton_showUser("正在检测网络连接", Color.White);
                            heartBeat hb = new heartBeat(1, 1000 * 60 * 5, netAcc(), netSup.getLocaMac());    //初始化heartbeat对象

                            if (netSup.PingIpOrDomainName("www.baidu.com"))      //测试网络连接状态
                            {
                                hb.start(); //启动心跳
                                pubFun_setText_simpleButton_showUser("网络已连接,心跳已启动", Color.White);
                                return;
                            }
                            else                                                //没连接的话尝试改变mac
                            {
                                if (r.routerType != 1)
                                    r.setCloneMAC(easy_routerAcc, easy_routerPwd, "router");        //如果没连好网尝试改变路由的克隆mac为路由的mac
                            }
                            for(int ap = 0; ap < 3; ap++)                       //最后试三遍有没有联网
                            {
                                if (netSup.PingIpOrDomainName("www.baidu.com"))
                                {
                                    hb.start(); //启动心跳
                                    pubFun_setText_simpleButton_showUser("网络已连接,心跳已启动", Color.White);
                                    return;
                                }
                                else
                                    pubFun_setText_simpleButton_showUser("网络未连接，请注意克隆MAC可用性和宽带密码正确性", Color.Red);
                            }
                        }
                        else
                        {
                            pubFun_setText_simpleButton_showUser("拨号失败，请检查路由的账号或密码", Color.Red);
                        }
                    }
                    else
                    {
                        pubFun_setText_simpleButton_showUser("客户端输入了不合法账号", Color.Red);
                    }
                    break;
                }
                Thread.Sleep(200);
            }   //end of while(true) loop
        }   //end of thread method
        #endregion

        //设置Mac按钮事件
        private void normal_button_cloneMac_Click(object sender, EventArgs e)
        {
            if (UI_macVerify())
            {
                log.writeLog("用户点击了设置Mac按钮", log.msgType.info);
                if (r.routerType == 1)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show("该路由型号的MAC克隆功能将重启你的路由器", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
                }
                if (r.setCloneMAC(routerAcc, routerPwd, cloneMAC) == 0)
                {
                    pubFun_setText_simpleButton_showUser("MAC地址设置失败，请检查路由管理员/密码的正确性", Color.Red);
                    log.writeLog("设置Mac函数返回了0(失败)", log.msgType.warning);
                }
                else{
                    pubFun_setText_simpleButton_showUser("MAC地址设置成功", Color.White);
                    log.writeLog("设置Mac函数返回了1(成功)", log.msgType.info);
                }
            }
        }

        //normal_button_setWifi点击事件
        private void normal_button_setWifi_Click(object sender, EventArgs e)
        {
            if (!UI_wifiVerifyControlsLegal())
                return;
            log.writeLog("用户点击了设置wifi按钮", log.msgType.info);
            if (r.setWIFI(routerAcc, routerPwd, wifiAcc, wifiPwd, true) == 1 ? true : false)
            {
                pubFun_setText_simpleButton_showUser("设置wifi成功", Color.White);
                log.writeLog("设置wifi成功", log.msgType.info);
            }
            else
            {
                pubFun_setText_simpleButton_showUser("设置wifi失败,请检查路由管理员/密码 或 wifi名称/密码的正确性", Color.Red);
                log.writeLog("设置wifi失败", log.msgType.warning);
            }

            //String str = "netAccBase64:" + netAccBase64
            //    + "\n" + "netAcc:" + netAcc()
            //    + "\n" + "netPwd:" + netPwd
            //    + "\n" + "gateway:" + gateway
            //    + "\n" + "routerAcc:" + routerAcc
            //    + "\n" + "routerPwd:" + routerPwd
            //    + "\n" + "cloneMAC:" + cloneMAC
            //    + "\n" + "special:" + special
            //    + "\n" + "saveConfig:" + saveConfig
            //    + "\n" + "wifiAcc:" + wifiAcc
            //    + "\n" + "wifiPwd:" + wifiPwd;
            //    MessageBox.Show(str);
        }


        //当单击宽带账号时
        private void normal_textBox_netAcc_MouseDown(object sender, MouseEventArgs e)
        {
            if (normal_textBox_netAcc.Text.Equals("未激活"))
            {
                pubFun_setText_simpleButton_showUser("请从QQ群 241108736 获取授权文件", Color.Red);
            }
            else
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(normal_textBox_netAcc.Text);     //把账号复制到剪切板
                    pubFun_setText_simpleButton_showUser("\"" + normal_textBox_netAcc.Text + "\"" + "已复制到剪切板", Color.White);
                }
                catch
                {
                    System.Windows.Forms.Clipboard.SetText("ERROR");     //把空格复制到剪切板
                    pubFun_setText_simpleButton_showUser("该许可证内含非法字符，不可使用，请重新下载时注意填写内容", Color.Red);
                    log.writeLog("许可证内含非法字符", log.msgType.error);
                }
            }
        }


        private void normal_textBox_netAccEasy_MouseDown(object sender, MouseEventArgs e)
        {
            if (normal_textBox_netAccEasy.Text.Equals("未激活"))
            {
                pubFun_setText_simpleButton_showUser("请从QQ群 241108736 获取授权文件", Color.Red);
            }
            else
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(normal_textBox_netAccEasy.Text);     //把账号复制到剪切板
                    pubFun_setText_simpleButton_showUser("\"" + normal_textBox_netAccEasy.Text + "\"" + "已复制到剪切板", Color.White);
                }
                catch
                {
                    System.Windows.Forms.Clipboard.SetText("ERROR");     //把空格复制到剪切板
                    pubFun_setText_simpleButton_showUser("该许可证内含非法字符，不可使用，请重新下载时注意填写内容", Color.Red);
                    log.writeLog("许可证内含非法字符", log.msgType.error);
                }
            }
        }


    }
}
