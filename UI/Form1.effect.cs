using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using cn.softname2.Log;
using cn.softname2.Support;
using System.Threading;
using cn.softname2.SaveConfig;
using cn.softname2.PPPOE_Deceive;
using cn.softname2.NK_Control;

namespace cn.softname2.UI
{
    partial class Form
    {

        //点击当做标签使用的图片按钮时，按钮不要有沉浸效果
        private void simpleButton_showUser_MouseDown(object sender, MouseEventArgs e)
        {
            exception_linkLabel_log.Focus();
        }

        //按钮移除沉浸效果
        private void easy_button_Start_MouseUp(object sender, MouseEventArgs e)
        {
            exception_linkLabel_log.Focus();
        }
        private void normal_button_setWifi_MouseUp(object sender, MouseEventArgs e)
        {
            exception_linkLabel_log.Focus();
        }
        private void normal_button_Start_MouseUp(object sender, MouseEventArgs e)
        {
            exception_linkLabel_log.Focus();
        }
        private void normal_button_cloneMac_MouseUp(object sender, MouseEventArgs e)
        {
            exception_linkLabel_log.Focus();
        }

        //当MAC框获取焦点时
        private void normal_comboBox_Mac_Enter(object sender, EventArgs e)
        {
            pubFun_setText_simpleButton_showUser("你也可以在这里输入自定义MAC，例如 'A1-B2-C3-D4-E5-F6' 格式", Color.White);
        }

        //窗口启动时
        private void Form_Load(object sender, EventArgs e)
        {
            log.writeLog("软件已启动", log.msgType.info);
            this.Text += " (启动中)";
            normal_comboBox_special.Text = "正常拨号";  //设置特殊拨号默认值
            UI_readConfig();
            Thread t_verifyLicence = new Thread(UI_verifyLicence);              //检测许可证
            t_verifyLicence.Start();
            Thread t_verifyRouterSupport = new Thread(UI_verifyRouterSupport);  //检测路由支持
            t_verifyRouterSupport.Start();
        }
        //窗口第一次显示(启动后)
        private void Form_Shown(object sender, EventArgs e)
        {
            UI_winpcapVerify(); //验证是否存在winpcap
            UI_NK_Verify();     //验证是否存在NK
            UI_NetworkAdapt_Verify();   //验证是否存在多网关
            pubfun_setText_form(pubfun_setText_form(null).Replace(" (启动中)","")); //当加载出页面后去掉"启动中"文字
        }

        //窗口关闭时
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            UI_writeConfig();   //将配置信息写入文件,(写还是要写的，加不加载另算)
            log.writeLog("用户关闭了'softname'软件,配置信息写入完成", log.msgType.info);
            Thread.Sleep(600);
        }

        //窗口关闭后
        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            //关闭程序和所有线程
            Environment.Exit(0);
        }

        

    }
}
