using cn.softname2.Licence;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using cn.softname2.Log;
using cn.softname2.routerControl;
using cn.softname2.SaveConfig;
using cn.softname2.Support;
using cn.softname2.PPPOE_Deceive;
using cn.softname2.NK_Control;


/*
 * 注意：
 * OK 控件添加预留
 * 引导系统
 * OK 异线程操作模块
 * OK easy & normal
 * OK 防止金山游侠
 * 
 * 
 */


namespace cn.softname2.UI
{
    public partial class Form : DevExpress.XtraEditors.XtraForm
    {
        public Form()
        {
            log.init(null);                             //log类初始化
            InitializeComponent();
        }


        static public String netAccBase64 = null;
        static public String netAcc()
        {
            try
            {
                byte[] Base64Bytes = Convert.FromBase64String(netAccBase64);
                return Encoding.Default.GetString(Base64Bytes);
            }
            catch
            {
                return null;
            }
            
        }
        static public String netPwd = null;
        static public String gateway = null;
        static public String routerAcc = null;
        static public String routerPwd = null;
        static public String cloneMAC = "pc";
        static public int special = 0;
        static public bool saveConfig = false;
        static public String wifiAcc = null;
        static public String wifiPwd = null;
        

        routerP r = null;
        network netSup = new network();

        //验证licence，并设置UI和本类下的netAcc字段      //本方法应另开线程，因实时检测许可
        private void UI_verifyLicence()
        {
            log.writeLog("开始循环检测许可", log.msgType.info);
            //---------当UI启动时：验证licence------------
            String licencePath = "C://ProgramData//licence";
            String locaLicencePath = "licence";
            bool firstActived = false;
            licence lic = new licence();
            for (;;) {
                if (File.Exists(licencePath))
                {
                    try
                    {
                        StreamReader sr = new StreamReader(licencePath);
                        String content = sr.ReadToEnd();
                        sr.Close();
                        String decryptString = lic.getNetAcc(content);
                        if (decryptString == null)
                        {
                            string cause = lic.getErrorInfo();
                            pubFun_setText_simpleButton_showUser(cause,Color.Red);
                            log.writeLog($"解析许可证时出现错误，原因：{cause}",log.msgType.error);
                        }
                        else
                        {
                            String base64encode=  Convert.ToBase64String(Encoding.Default.GetBytes(decryptString)); //将解密的宽带账号base64编码
                            netAccBase64 = base64encode;                //主缓存区的netAcc被赋值为base64加密后的值
                            pubfun_setText_normal_textBox_netAcc(decryptString);    //UI(普通模式)上显示正确的已授权账号
                            pubfun_setText_normal_textBox_netAccEasy(decryptString);    //UI(简易模式)上显示正确的已授权账号
                            //normal_textBox_netAcc.Text = decryptString; //UI上显示正确的已授权账号

                            //如果软件目录已经存在licence则删除它
                            if (File.Exists(locaLicencePath))
                                File.Delete(locaLicencePath);
                            // 创建文件
                            FileStream fs = new FileStream(locaLicencePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            StreamWriter sw = new StreamWriter(fs); // 创建写入流
                            sw.WriteLine(content); // 写入到软件目录
                            sw.Flush(); //清空缓冲区
                            sw.Close(); //关闭流
                            fs.Close(); //关闭文件
                            File.Delete(licencePath);   //删除临时licence文件
                            if (firstActived)   //如果是首次激活
                            {
                                DevExpress.XtraEditors.XtraMessageBox.Show($"软件激活完成！\n宽带账号:'{decryptString}'已授权", "许可证", MessageBoxButtons.OK, MessageBoxIcon.None);
                                pubFun_setText_simpleButton_showUser("软件已激活", Color.White);
                            }
                            return;
                        }
                    }
                    catch
                    {
                        pubFun_setText_simpleButton_showUser("软件读取临时许可证时发生未知错误", Color.Red);
                        log.writeLog("软件读取临时许可证时发生未知错误", log.msgType.error);
                    }
            }
                else if (File.Exists(locaLicencePath))
                {
                    try
                    {
                        StreamReader sr = new StreamReader(locaLicencePath);
                        String content = sr.ReadToEnd();
                        sr.Close();
                        String decryptString = lic.getNetAcc(content);
                        if (decryptString == null)
                        {
                            pubFun_setText_simpleButton_showUser("许可证被损坏", Color.Red);
                            log.writeLog("许可证被损坏", log.msgType.error);
                        }
                        else
                        {
                            String base64encode = Convert.ToBase64String(Encoding.Default.GetBytes(decryptString)); //将解密的宽带账号base64编码
                            netAccBase64 = base64encode;                //主缓存区的netAcc被赋值为base64加密后的值
                            pubfun_setText_normal_textBox_netAcc(decryptString);        //UI(普通模式)上显示正确的已授权账号
                            pubfun_setText_normal_textBox_netAccEasy(decryptString);    //UI(简易模式)上显示正确的已授权账号
                            if (firstActived)   //如果是首次激活
                            {
                                DevExpress.XtraEditors.XtraMessageBox.Show($"软件激活完成！\n宽带账号:'{decryptString}'已授权", "许可证", MessageBoxButtons.OK, MessageBoxIcon.None);
                                pubFun_setText_simpleButton_showUser("软件已激活", Color.White);
                            }
                            return;
                        }
                    }
                    catch
                    {
                        pubFun_setText_simpleButton_showUser("读取许可证时发生未知错误", Color.Red);
                        log.writeLog("读取许可证时发生未知错误", log.msgType.error);
                    }
                }
                Thread.Sleep(1000);
                firstActived = true;
            }//end of loop for(;;)
        }

        //检测路由支持，并设置UI相关        //本方法应另开线程，因其中有阻塞内容
        private void UI_verifyRouterSupport()
        {
            pubFun_setText_simpleButton_showUser("正在检测路由支持", Color.White);
            UI_lockButton(false);   //路由检测时不允许点击按钮
            foreach (String gate in netSup.getGateways())
            {
                r = new routerP(gate);
                if (r.routerType > 0)    //如果路由获得支持
                {

                    if (netAcc() == null)    //如果路由支持但无许可证
                    {
                        pubFun_setText_simpleButton_showUser("路由已支持，请从QQ群241108736获取许可证", Color.White);
                    }
                    else
                    {
                        String str = "路由已支持";
                        saveConfig saveCfg = new saveConfig(null);
                        if (saveCfg.Read("routerType") != null)
                        {
                            if (!saveCfg.Read("routerType").Equals(r.routerType.ToString()))
                                str += "，此路由器并非上次使用的设备，请注意路由密码正确";
                        }
                        pubFun_setText_simpleButton_showUser(str, Color.White);
                    }

                    if (r.routerType == 1)  //如果路由器时getI型
                    {
                        pubFun_setVisible_easy_label_routerAcc(true);
                        pubFun_setVisible_easy_textBox_routerAcc(true);
                    }
                    UI_lockButton(true);    //如果路由获得支持所有按键解放
                    break;
                }
            }
            if(r.routerType <= 0)
            {
                pubFun_setText_simpleButton_showUser("路由不支持", Color.Red);
            }

        }

        //读取软件上一次的保存配置,并映射到前台
        private void UI_readConfig()
        {
            saveConfig saveCfg = new saveConfig(null);  //参数null，使用默认文件名

            //如果上次UI退出前没点保存
            if (saveCfg.Read("save") != null)
            {
                if (!saveCfg.Read("save").Equals("True"))
                    return;
                else
                    normal_checkBox_save.Checked = true;
            }

            //宽带密码
            netPwd = saveCfg.Read("netPwd");
            if (netPwd != null && !netPwd.Equals(""))
                normal_textBox_netPwd.Text = netPwd;
            ////网关
            ////gateway = saveCfg.Read("gateway");
            ////if (gateway != null && !gateway.Equals(""))
            ////    normal_textBox_routerGateway.Text = gateway;
            //路由账号
            routerAcc = saveCfg.Read("routerAcc");
            if (routerAcc != null && !routerAcc.Equals(""))
                normal_textBox_routerAcc.Text = routerAcc;
            //路由密码
            routerPwd = saveCfg.Read("routerPwd");
            if (routerPwd != null && !routerPwd.Equals(""))
                normal_textBox_routerPwd.Text = routerPwd;
            //克隆MAC
            String cloneMacRead = saveCfg.Read("cloneMAC");
            switch (cloneMacRead)
            {
                case "本机MAC":
                    cloneMAC = "pc";
                    normal_comboBox_Mac.Text = "本机MAC";
                    break;
                case "路由MAC":
                    cloneMAC = "router";
                    normal_comboBox_Mac.Text = "路由MAC";
                    break;
                default:
                    if (cloneMacRead != null && !cloneMacRead.Equals(""))
                    {
                        cloneMAC = cloneMacRead;
                        normal_comboBox_Mac.Text = cloneMacRead;
                    }
                    else
                    {
                        cloneMAC = "pc";
                        normal_comboBox_Mac.Text = "本机MAC";
                    }
                    break;
            }
            //特殊拨号
            String specialRead = saveCfg.Read("special");
            if (specialRead != null)
            {
                for (int i = 1; i <= 7; i++)
                {
                    if (specialRead.IndexOf(i.ToString()) != -1)
                    {
                        special = i;
                        normal_comboBox_special.Text = "特殊拨号" + i.ToString();
                        break;
                    }
                    if (i == 7)
                    {
                        special = 0;
                        normal_comboBox_special.Text = "正常拨号";
                        break;
                    }
                }
            }

            //wifi账号
            wifiAcc = saveCfg.Read("wifiAcc");
            if (wifiAcc != null && !wifiAcc.Equals(""))
                normal_textBox_wifiAcc.Text = wifiAcc;
            //wifi密码
            wifiPwd = saveCfg.Read("wifiPwd");
            if (wifiPwd != null && !wifiPwd.Equals(""))
                normal_textBox_wifiPwd.Text = wifiPwd;

        }

        //写入软件本次配置保存
        private void UI_writeConfig()
        {
            saveConfig saveCfg = new saveConfig(null);  //参数null，使用默认文件名
            saveCfg.Write("netPwd",netPwd);
            //saveCfg.Write("gateway", gateway);
            saveCfg.Write("routerAcc", routerAcc);
            saveCfg.Write("routerPwd", routerPwd);
            saveCfg.Write("cloneMAC", normal_comboBox_Mac.Text);
            saveCfg.Write("special", normal_comboBox_special.Text);
            saveCfg.Write("wifiAcc", wifiAcc);
            saveCfg.Write("wifiPwd", wifiPwd);
            if(r!=null && r.routerType!=0)
                saveCfg.Write("routerType", r.routerType.ToString());
            saveCfg.Write("save", normal_checkBox_save.Checked.ToString());
        }

        //验证所有控件的合法性
        private bool UI_verifyControlsLegal()
        {
            network netSup = new network();
            if (netAccBase64 == null || netAccBase64.Equals("") || netAcc() == null || netAcc().Equals(""))
            {
                pubFun_setText_simpleButton_showUser("请激活软件", Color.Red);
                return false;
            }
            if (netPwd==null || netPwd.Equals(""))
            {
                pubFun_setText_simpleButton_showUser("宽带密码未填写", Color.Red);
                return false;
            }
            //if (gateway == null || gateway.Equals(""))
            //{
            //    pubFun_setText_simpleButton_showUser("网关未填写", Color.Red);
            //    return false;
            //}
            //if (!netSup.ValidateIPAddress(gateway))
            //{
            //    pubFun_setText_simpleButton_showUser("网关数值不合法", Color.Red);
            //    return false;
            //}
            if(routerAcc == null || routerAcc.Equals(""))
            {
                pubFun_setText_simpleButton_showUser("路由管理员未填写", Color.Red);
                return false;
            }
            if (routerPwd == null || routerPwd.Equals(""))
            {
                pubFun_setText_simpleButton_showUser("路由密码未填写", Color.Red);
                return false;
            }
            return true;
        }
        //Mac验证所需控件合法性
        private bool UI_macVerify()
        {

            if (!cloneMAC.Equals("pc") && !cloneMAC.Equals("router") && !netSup.IsMac(cloneMAC))
            {
                pubFun_setText_simpleButton_showUser("请输入正确的MAC地址", Color.Red);
                return false;
            }
            if (routerAcc == null || routerAcc.Equals(""))
            {
                pubFun_setText_simpleButton_showUser("路由管理员未填写", Color.Red);
                return false;
            }
            if (routerPwd == null || routerPwd.Equals(""))
            {
                pubFun_setText_simpleButton_showUser("路由密码未填写", Color.Red);
                return false;
            }
            return true;
        }
        //wifi验证所需控件的合法性
        private bool UI_wifiVerifyControlsLegal()
        {
            if (routerAcc == null || routerAcc.Equals(""))
            {
                pubFun_setText_simpleButton_showUser("路由管理员未填写", Color.Red);
                return false;
            }
            if (routerPwd == null || routerPwd.Equals(""))
            {
                pubFun_setText_simpleButton_showUser("路由密码未填写", Color.Red);
                return false;
            }
            if (wifiAcc == null || wifiAcc.Equals(""))
            {
                pubFun_setText_simpleButton_showUser("wifi名称未填写", Color.Red);
                return false;
            }
            if (wifiPwd == null || wifiPwd.Equals(""))
            {
                pubFun_setText_simpleButton_showUser("wifi密码未填写", Color.Red);
                return false;
            }
            return true;
        }
        //验证是否存在winpcap
        private bool UI_winpcapVerify()
        {
            try
            {
                new PPPOEDiceive().getActiveNetAdapte();
                return true;
            }
            catch
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("未安装winPCAP，请安装winPCAP后重启电脑", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                log.writeLog("未安装winPCAP", log.msgType.error);
                UI_lockButton(false);   //所有button不可用
                return false;
            }
        }

        //验证是否存在NK
        private bool UI_NK_Verify()
        {
            if (!new startNK2().hasNk())
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("未安装netkeeper，请安装netkeeper后重启电脑", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                log.writeLog("未安装netkeeper", log.msgType.error);
                UI_lockButton(false);   //所有button不可用
                return false;
            }
            return true;
        }

        //检测多网关
        private bool UI_NetworkAdapt_Verify()
        {
            List<String> list = netSup.getAdapterIPs();
            if(list.Count < 2)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("未检测到多网关存在，请至少使用双网卡同时连接到【路由器LAN口】和【Internet】(可为无线热点)", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                log.writeLog("未检测到多网关", log.msgType.error);
                UI_lockButton(false);   //所有button不可用
                return false;
            }
            return true;

        }

        //是否锁定界面所有按钮
        private void UI_lockButton(bool loc)
        {
            pubFun_setEnable_normal_button_setWifi(loc);
            pubFun_setEnable_normal_button_Start(loc);
            pubFun_setEnable_easy_button_Start(loc);
            pubFun_setEnable_normal_button_cloneMac(loc);
        }

    }
}
