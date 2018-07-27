using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using cn.softname2.Log;

namespace cn.softname2.HeartBeat
{
    class socketHeartBeat
    {
        static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);


        public byte[] sentUdpPac(String desIP,int desPort,byte[] pacData,int recTimeOut,int byteBufferSize)
        {
            EndPoint point = new IPEndPoint(IPAddress.Parse(desIP), desPort);
            log.writeLog($"sentUdpPac() : 开始发送心跳包数据\n目的IP:{desIP}\n目的端口:{desPort}\n心跳数据为:\n{System.Text.Encoding.Default.GetString(pacData)}\n", log.msgType.info);
            client.SendTo(pacData,pacData.Length, SocketFlags.None, point);
            client.ReceiveTimeout = recTimeOut;
            byte[] bytes = new byte[byteBufferSize];
            int recLength;
            try
            {
                recLength = client.Receive(bytes, byteBufferSize,SocketFlags.None);
            }
            catch(SocketException)
            {
                log.writeLog("sentUdpPac() : 远程UDP接收端未响应", log.msgType.warning);
                return null;
            }
            if (recLength == 0)
                return null;
            return bytes;
        }


        public byte[] getHeartBeatPayloadData(String netAcc, String IP, String Mac,String hearBeatKey)
        {
            String heartBeatText = "TYPE=HEARTBEAT&USER_NAME=" + netAcc + "&PASSWORD=NULL&IP=" + IP + "&MAC=" + Mac + "&DRIVER=NULLDRV&VERSION_NUMBER=4.8.0.609&PIN=" + "139" + new Random().Next(100, 999); //后面这几位数是无关紧要的毫秒时间戳，为了不重复我稍微改了点
            securityAES aes = new securityAES();
            byte[] encryptBytes = aes.Encrypt(heartBeatText, hearBeatKey);
            byte[] lengthBytes = new byte[] {0,0,0,0};
            int bp = -1;
            for(int ap= lengthBytes.Length-1; ap>=0 ; ap--)
            {
                bp++;
                if (BitConverter.GetBytes(encryptBytes.Length).Length > bp)
                    lengthBytes[ap] = BitConverter.GetBytes(encryptBytes.Length)[bp];
                else
                    break;
            }
            byte[] payload=  new ptcQUICsupport().byteAppent(lengthBytes, encryptBytes);
            return new ptcQUICsupport().byteAppent(new byte[] { 72, 82, 54, 48, 5, 0 }, payload);
        }

    }
}
