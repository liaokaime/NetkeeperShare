using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cn.softname2.HeartBeat
{
    interface interAnalysisQUIC
    {
        byte[] getDesMac();
        byte[] getSrcMac();
        byte[] getPacketProtocolType(); //直接返回0x0800

        byte[] getVersion();            //不了解具体含义，一般为常量为0x45
        byte[] getDSF();                //给服务器的字段(differentiated services field)  0x00/默认  0x48保证转发
        byte[] getTotalLength();        //整个包除了源、目的Mac+包协议类型 字段外的所有字段的长度
        byte[] getIdentification();     //身份认证字段，不知道具体变化规则
        byte[] getFlags();              //0x02为不要传送时不要切割包
        byte[] getFrafmentOffset();     //切割大小,不切割直接为0x00
        byte[] getTimeToLive();         //TTL，服务器每转发一次减1，到0则丢包
        byte[] getProtocolOfLayer2();   //获取二层协议 0x11/UDP
        byte[] getHeaderChecksum();     //验证头部总长？   看做常量：0x00不验证    /验证头部完整性
        byte[] geSrcIP();               //本机IP
        byte[] getDisIP();              //将发送的目的IP

        byte[] getUDPSrcPort();         //UDP报文源端口
        byte[] getUDPDesPort();         //UDP报文目的端口
        byte[] getUDPLength();          //UDP报文长度  
        byte[] getUDPCheckSum();        //校验UDP报文完整性，通过与各项数据的计算得出值

        byte[] getPublicFlags();        //公共标识，意义不明,一般为0x48
        byte[] getCID();                //byte占4位，常变的量，意义不明
        byte[] getSeq();                //意义不明，一般为0x00
        byte[] getPayload();            //搭载的数据，前几位表示长度，解密时除
    }
}
