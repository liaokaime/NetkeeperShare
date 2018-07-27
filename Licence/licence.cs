using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cn.softname2.Support;
using System.Runtime.InteropServices;
using cn.softname2.Log;
using sunflower.RSAStandard;

namespace cn.softname2.Licence {

    public class licence {
        private String publicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxMma44H5wOws1QBESeL05bfb0RMeOhleGUypK4z1kmLWLy2JKHs5dYiOqu99LVYC3vpXEu+XQRHnXvdIQWU7OBLfsvEI39hGUhTVihZf+FuHxc0iASa0fFKIl58NGiEkUJtooapGeta2Fp0BHoPMMrzB94V5RVPP/95dJwBhJIeaWgSwGg0QiORiYD9OQ4xr7JYNdZhapFcESd1q6oBpnKzh+o29y0aneVpNqy1IDfYaRHsxzBlJwoBdLnrAjidMnFbfo9f2aIjhGSE9TBTpKdnJX2ABr2G2mQmHaQRR9zUK2VaImDnaryeXSY7XaBeY33KhJiZFcsGtJxgCgEN98QIDAQAB";
        private String timeLable = "endTime";
        private String accLable = "netAcc";
        private String cause = "";
        //源字符串应为  "<endTime>结束时间</endTime><netAcc>宽带账号</netAcc>"
        //使用加密字符串获取真实账号
        public string getNetAcc(string licenceString) {
            try
            {
                string decryptResult = new RSAStandard().RSADecryptByPub(licenceString, publicKey);
                log.writeLog(decryptResult, log.msgType.warning);
                String endTime = getNodeContent(decryptResult,timeLable);
                String netAcc = getNodeContent(decryptResult, accLable);
                long endTimeLong = long.Parse(endTime);
                if (endTime!=null && netAcc!=null)
                {
                    if (endTimeLong < getJavaDateTime())
                    {
                        log.writeLog($"licence模块\n解析授权账号成功，但该授权文件已过授权日期", log.msgType.warning);
                        cause = "授权文件已过期";
                        return null;
                    }
                    //String catchAcc = decryptResult.Substring(markString.Length, decryptResult.Length - markString.Length);
                    log.writeLog($"licence模块\n解析到授权账号:{netAcc}", log.msgType.info);
                    cause = "成功";
                    return netAcc;
                }
                else
                {
                    log.writeLog($"licence模块\n解析授权账号出错，在授权信息中未找到endTime或netAcc节点信息", log.msgType.error);
                    cause = "授权文件信息内容错误";
                    return null;
                }

            }
            catch
            {
                log.writeLog($"licence模块\n解析授权账号出错，遇到未知错误(多为许可证被损坏)", log.msgType.error);
                cause = "许可证被损坏";
                return null;
            }
        }

        /// <summary>
        /// 获取解析授权文件失败的原因
        /// </summary>
        /// <returns></returns>
        public String getErrorInfo()
        {
            return cause;
        }


        /// <summary>
        /// 在不引入包的情况下，轻量，狭义地解析xml数据
        /// 狭义指该xml字符串无嵌套xml标签，每个xml节点只出现一次
        /// </summary>
        /// <param name="xmlStr">xml字符串</param>
        /// <param name="nodeName">节点名</param>
        /// <returns>节点内数据</returns>
        private String getNodeContent(String xmlStr, String nodeName)
        {
            if (xmlStr == null || nodeName == null)
                return null;
            String nodeStart = "<" + nodeName + ">";
            String nodeEnd = "</" + nodeName + ">";
            int startInt = xmlStr.IndexOf(nodeStart);
            int endInt = xmlStr.IndexOf(nodeEnd);
            if (startInt < 0 || endInt < 0)
                return null;
            return xmlStr.Substring(startInt + nodeStart.Length, endInt - startInt - nodeStart.Length);
        }

        /// <summary>
        /// 获取java标准的当前毫秒数
        /// </summary>
        /// <returns></returns>
        private long getJavaDateTime()
        {
            DateTime dateTime = DateTime.Now;
            DateTime windowsEpoch = new DateTime(1601, 1, 1, 0, 0, 0, 0);
            DateTime javaEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long epochDiff = (javaEpoch.ToFileTimeUtc() - windowsEpoch.ToFileTimeUtc()) / TimeSpan.TicksPerMillisecond;
            return (dateTime.ToFileTime() / TimeSpan.TicksPerMillisecond) - epochDiff;
        }



    }


  }

