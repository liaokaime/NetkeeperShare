using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using cn.softname2.Log;
using cn.softname2.HeartBeat;

namespace cn.softname2.HeartBeat
{
    class heartBeat
    {
        String hearBeatKey = "nk4*hr39v~j4;7cj";
        String desIP = "117.21.209.186";
        int desPort = 443;
        String publicIP = null;
        int solution = 0;
        String netAcc = null;
        String locaMac = null;
        otherSupport otherSup = new otherSupport();
        bool keepHeartBeat = true;
        int frequency = 0;

        //  solution : 1/socketHeartBeat   2/winpcapHeartBeat
        //  frequency(心跳频率)/单位毫秒
        public heartBeat(int solution, int frequency, String netAcc ,String locaMac) 
        {
            this.solution = solution;
            this.netAcc = netAcc;
            this.locaMac = locaMac;
            this.frequency = frequency;
            log.writeLog($"心跳类构造方法执行完毕\nsolution:{solution,20}\t//solution : 1/socketHeartBeat   2/winpcapHeartBeat\nfrequency:{frequency,20}\nnetAcc:{netAcc,20}\nlocaMac:{locaMac,20}", log.msgType.info);
        }

        Thread t = null;
        public void start() //  frequency/频率，单位毫秒
        {
            if(t==null || !t.IsAlive)
            {
                Thread t = new Thread(startThread);
                t.Start();
                log.writeLog("心跳线程启动完毕",log.msgType.info);
            }
        }

        public void stop()
        {
            keepHeartBeat = false;
            log.writeLog("心跳线程关闭", log.msgType.info);
        }

        public void startThread()
        {
            while (keepHeartBeat)
            {
                this.publicIP = otherSup.getPublicIP();
                if (this.publicIP == null)
                    continue;
                switch (solution)
                {
                    case 1:
                        socketHeartBeat shb = new socketHeartBeat();
                        byte[] payload= shb.getHeartBeatPayloadData(this.netAcc, this.publicIP, this.locaMac, this.hearBeatKey);
                        byte[] rec_heartBeat= shb.sentUdpPac(desIP, desPort, payload,1000,1000);
                        Thread.Sleep(this.frequency);
                        break;
                    case 2:




                        break;
                }
            }
            
        }



        //static void Main(string[] args)
        //{

        //}

    }  
}
