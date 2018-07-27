using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cn.softname2.HeartBeat
{
    class analysisQUIC
    {


        byte[] pac = null;
        ptcQUICsupport sup = new ptcQUICsupport();

        analysisQUIC(byte[] pac)    //构造方法传入要解析的包
        {
            pac = this.pac;
        }


        byte[] getDesMac()
        {
            byte[] desMac = sup.byteSub(this.pac, 0, 6);
            return desMac;
        }
        byte[] getSrcMac()
        {
            byte[] srcMac = sup.byteSub(this.pac, 6, 6);
            return srcMac;
        }
        byte[] getPacketProtocolType()
        {
            byte[] pacProtocolType = sup.byteSub(this.pac, 12, 2);
            return pacProtocolType;
        }

        byte[] getVersion()
        {
            byte[] version = sup.byteSub(this.pac, 14, 1);
            return version;
        }
        byte[] getDSF()
        {
            byte[] DFS = sup.byteSub(this.pac, 15, 1);
            return DFS;
        }
        byte[] getTotalLength()
        {
            byte[] totalLength = sup.byteSub(this.pac, 16, 2);
            return totalLength;
        }
        byte[] getIdentification()
        {
            byte[] identification = sup.byteSub(this.pac, 18, 2);
            return identification;
        }
        byte[] getFlags()
        {
            byte[] flags = sup.byteSub(this.pac, 20, 1);
            return flags;
        }
        byte[] getFrafmentOffset()
        {
            byte[] frafmentOffset = sup.byteSub(this.pac, 21, 1);
            return frafmentOffset;
        }
        byte[] getTimeToLive()
        {
            byte[] TTL = sup.byteSub(this.pac, 22, 1);
            return TTL;
        }
        byte[] getProtocolOfLayer2()
        {
            byte[] ProtocolOfLayer2 = sup.byteSub(this.pac, 23, 1);
            return ProtocolOfLayer2;
        }
        byte[] getHeaderChecksum()
        {
            byte[] headerChecksum = sup.byteSub(this.pac, 24, 2);
            return headerChecksum;
        }
        byte[] geSrcIP()
        {
            byte[] srcIP = sup.byteSub(this.pac, 26, 4);
            return srcIP;
        }
        byte[] getDisIP()
        {
            byte[] disIP = sup.byteSub(this.pac, 30, 4);
            return disIP;
        }


        byte[] getUDPSrcPort()
        {
            return null;
        }
        byte[] getUDPDesPort()
        {
            return null;
        }
        byte[] getUDPLength()
        {
            return null;
        }
        byte[] getUDPCheckSum()
        {
            return null;
        }


        byte[] getPublicFlags()
        {
            return null;
        }
        byte[] getCID()
        {
            return null;
        }
        byte[] getSeq()
        {
            return null;
        }
        byte[] getPayload()
        {
            return null;
        }
    }
}
