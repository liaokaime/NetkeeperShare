using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace cn.softname2.UI
{

    partial class Form
    {
        #region     //本地操作控件方法
        //设置或获取 主窗口软件名，当传入参数为null时获取，非null时设置
        private String locaFun_form_setText(String text)
        {
            if(text != null)
                this.Text = text;
            return this.Text;
        }
        //设置或获取 普通模式宽带账号文本框，当传入参数为null时获取，非null时设置
        private String locaFun_setText_normal_textBox_netAcc(String text)
        {
            if (text != null)
                normal_textBox_netAcc.Text = text;
            return normal_textBox_netAcc.Text;
        }
        //设置或获取 简易模式宽带账号文本框，当传入参数为null时获取，非null时设置
        private String locaFun_setText_normal_textBox_netAccEasy(String text)
        {
            if (text != null)
                normal_textBox_netAccEasy.Text = text;
            return normal_textBox_netAccEasy.Text;
        }
        //本地设置easy_label_routerAcc可见与否
        private void locaFun_setVisible_easy_label_routerAcc(bool visible)
        {
            easy_label_routerAcc.Visible = visible;
        }
        //本地设置easy_textBox_routerAcc可见与否
        private void locaFun_setVisible_easy_textBox_routerAcc(bool visible)
        {
            easy_textBox_routerAcc.Visible = visible;
        }
        //本地设置simpleButton_showUser的文字与颜色
        private void locaFun_setText_simpleButton_showUser(String text, Color color)
        {
            simpleButton_showUser.Text = text;
            simpleButton_showUser.ForeColor = color;
        }
        //本地设置simpleButton_showUser的可用性
        private void locaFun_setEnable_simpleButton_showUser(bool enable)
        {
            simpleButton_showUser.Enabled = enable;
        }
        //设置normal_button_setWifi的可用性
        private void locaFun_setEnable_normal_button_setWifi(bool enable)
        {
            normal_button_setWifi.Enabled = enable;
        }
        //设置normal_button_Start的可用性
        private void locaFun_setEnable_normal_button_Start(bool enable)
        {
            normal_button_Start.Enabled = enable;
        }
        //设置easy_button_Start的可用性
        private void locaFun_setEnable_easy_button_Start(bool enable)
        {
            easy_button_Start.Enabled = enable;
        }
        //设置normal_button_cloneMac的可用性
        private void locaFun_setEnable_normal_button_cloneMac(bool enable)
        {
            normal_button_cloneMac.Enabled = enable;
        }
        //设置主界面（其中的一个小控件）为焦点
        private void locaFun_setFocus_exception_linkLabel_log()
        {
            exception_linkLabel_log.Focus();
        }

        #endregion

        #region     //控件委托声明
        delegate String del_setText_form(String text);
        delegate String del_setText_normal_textBox_netAcc(String text);
        delegate String del_setText_normal_textBox_netAccEasy(String text);
        delegate void del_setVisible_easy_label_routerAcc(bool visible);
        delegate void del_setVisible_easy_textBox_routerAcc(bool visible);
        delegate void del_setText_simpleButton_showUser(String text, Color color);
        delegate void del_setEnable_easy_textBox_routerAcc(bool enable);
        delegate void del_setEnable_normal_button_setWifi(bool enable);
        delegate void del_setEnable_normal_button_Start(bool enable);
        delegate void del_setEnable_easy_button_Start(bool enable);
        delegate void del_setEnable_normal_button_cloneMac(bool enable);
        delegate void del_setFocus_exception_linkLabel_log();
        

        #endregion

        #region     //异线程调用接口
        public String pubfun_setText_form(String text)
        {
            del_setText_form text_form = new del_setText_form(locaFun_form_setText);
            return this.Invoke(text_form, new object[] { text }).ToString() ;
        }
        //设置normal_textBox_netAcc文本框文本
        public String pubfun_setText_normal_textBox_netAcc(String text)
        {
            del_setText_normal_textBox_netAcc Text_normal_textBox_netAcc = new del_setText_normal_textBox_netAcc(locaFun_setText_normal_textBox_netAcc);
            return this.Invoke(Text_normal_textBox_netAcc, new object[] { text }).ToString();
        }
        //设置normal_textBox_netAccEasy文本框文本
        public String pubfun_setText_normal_textBox_netAccEasy(String text)
        {
            del_setText_normal_textBox_netAccEasy Text_normal_textBox_netAccEasy = new del_setText_normal_textBox_netAccEasy(locaFun_setText_normal_textBox_netAccEasy);
            return this.Invoke(Text_normal_textBox_netAccEasy, new object[] { text }).ToString();
        }
        //设置easy_label_routerAcc的可见性
        public void pubFun_setVisible_easy_label_routerAcc(bool visible)
        {
            del_setVisible_easy_label_routerAcc Visible_easy_label_routerAcc = new del_setVisible_easy_label_routerAcc(locaFun_setVisible_easy_label_routerAcc);
            easy_label_routerAcc.Invoke(Visible_easy_label_routerAcc, new object[] { visible });
        }
        //设置easy_textBox_routerAcc的可见性
        public void pubFun_setVisible_easy_textBox_routerAcc(bool visible)
        {
            del_setVisible_easy_textBox_routerAcc Visible_easy_textBox_routerAcc = new del_setVisible_easy_textBox_routerAcc(locaFun_setVisible_easy_textBox_routerAcc);
            easy_textBox_routerAcc.Invoke(Visible_easy_textBox_routerAcc, new object[] { visible });
        }
        //设置simpleButton_showUser的文字和颜色
        public void pubFun_setText_simpleButton_showUser(String text, Color color)
        {
            del_setText_simpleButton_showUser Text_simpleButton_showUser = new del_setText_simpleButton_showUser(locaFun_setText_simpleButton_showUser);
            simpleButton_showUser.Invoke(Text_simpleButton_showUser, new object[] { text, color });
        }
        //设置simpleButton_showUser的可用性
        public void pubFun_setEnable_simpleButton_showUser(bool enable)
        {
            del_setEnable_easy_textBox_routerAcc Enable_simpleButton_showUser = new del_setEnable_easy_textBox_routerAcc(locaFun_setEnable_simpleButton_showUser);
            simpleButton_showUser.Invoke(Enable_simpleButton_showUser, new object[] { enable });
        }
        //设置normal_button_setWifi的可用性
        public void pubFun_setEnable_normal_button_setWifi(bool enable)
        {
            del_setEnable_normal_button_setWifi Enable_normal_button_setWifi = new del_setEnable_normal_button_setWifi(locaFun_setEnable_normal_button_setWifi);
            normal_button_setWifi.Invoke(Enable_normal_button_setWifi, new object[] { enable });
        }
        //设置normal_button_Start的可用性
        public void pubFun_setEnable_normal_button_Start(bool enable)
        {
            del_setEnable_normal_button_Start Enable_normal_button_Start = new del_setEnable_normal_button_Start(locaFun_setEnable_normal_button_Start);
            normal_button_Start.Invoke(Enable_normal_button_Start, new object[] { enable });
        }
        //设置normal_button_Start的可用性
        public void pubFun_setEnable_easy_button_Start(bool enable)
        {
            del_setEnable_easy_button_Start Enable_easy_button_Start = new del_setEnable_easy_button_Start(locaFun_setEnable_easy_button_Start);
            easy_button_Start.Invoke(Enable_easy_button_Start, new object[] { enable });
        }
        //设置normal_button_cloneMac的可用性
        public void pubFun_setEnable_normal_button_cloneMac(bool enable)
        {
            del_setEnable_normal_button_cloneMac Enable_normal_button_cloneMac = new del_setEnable_normal_button_cloneMac(locaFun_setEnable_normal_button_cloneMac);
            normal_button_cloneMac.Invoke(Enable_normal_button_cloneMac, new object[] { enable });
        }
        //设置exception_linkLabel_log焦点
        public void pubFun_setFocus_exception_linkLabel_log()
        {
            del_setFocus_exception_linkLabel_log Focus_exception_linkLabel_log = new del_setFocus_exception_linkLabel_log(locaFun_setFocus_exception_linkLabel_log);
            exception_linkLabel_log.Invoke(Focus_exception_linkLabel_log, new object[] { });
        }


        #endregion


    }

}