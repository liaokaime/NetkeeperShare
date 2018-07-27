using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using cn.softname2.Log;
using System.Xml;

namespace cn.softname2.SaveConfig
{
    class saveConfig
    {
        String defaultPath = "config.xml";
        String path = null;
        XmlDataDocument XmlDoc = new XmlDataDocument();
        public saveConfig(String fileName) {

            if (fileName == null || fileName.Equals(""))
            {
                path = ".\\" + defaultPath;
            }
            else {
                path = ".\\" + fileName;
            }

            try
            {
                if (File.Exists(path))
                {
                    XmlDoc.Load(path);
                    XmlNode all = XmlDoc.SelectSingleNode("All");
                    Console.WriteLine(all.ChildNodes.Count);
                }else
                {
                    log.writeLog("saveConfig模块:\n配置文件不存在,正在新建文件", log.msgType.warning);
                    XmlElement all = XmlDoc.CreateElement("All");
                    XmlDoc.AppendChild(all);
                    XmlDoc.Save(path);
                }
            }
            catch
            {
                log.writeLog("saveConfig模块:\n未知错误，正在重建文件", log.msgType.warning);
                Delete();
                XmlDoc.DocumentElement.RemoveAllAttributes();
                XmlElement all = XmlDoc.CreateElement("All");
                try
                {
                    XmlDoc.AppendChild(all);
                }
                catch { }

                XmlDoc.Save(path);
            }
            //catch (XmlException)
            //{
            //    log.writeLog("saveConfig模块:\n配置文件损坏，正在重建文件", log.msgType.warning);
            //    XmlElement all = XmlDoc.CreateElement("All");
            //    XmlDoc.AppendChild(all);
            //    XmlDoc.Save(defaultPath);
            //}
            //catch (NullReferenceException)
            //{
            //    log.writeLog("saveConfig模块:\n配置文件被篡改导致读取出错,正在重建文件", log.msgType.warning);
            //    XmlDoc.DocumentElement.RemoveAllAttributes();
            //    XmlElement all= XmlDoc.CreateElement("All");
            //    XmlDoc.AppendChild(all);
            //    XmlDoc.Save(defaultPath);


            //}
            //catch {
            //    log.writeLog("saveConfig模块:\n未知错误,正在重建文件", log.msgType.warning);
            //    XmlElement all = XmlDoc.CreateElement("All");
            //    XmlDoc.AppendChild(all);
            //    XmlDoc.Save(defaultPath);
            //}
        }


        public void Write(String a,String b) {
            XmlNode all = XmlDoc.SelectSingleNode("All");

            bool exit = false;
            foreach (XmlNode allNode in all)
            {
                foreach (XmlNode infoNode in allNode)
                {
                    if (exit == true)
                    {
                        infoNode.InnerText= b;
                        XmlDoc.Save(path);
                        return;
                    }
                    if (infoNode.InnerText.Equals(a))
                        exit = true;
                }
            }
            XmlElement info = XmlDoc.CreateElement("info");
            XmlElement title = XmlDoc.CreateElement("title");
            XmlElement content = XmlDoc.CreateElement("content");
            title.InnerText = a;
            content.InnerText = b;
            info.AppendChild(title);
            info.AppendChild(content);
            all.AppendChild(info);
            XmlDoc.Save(path);
        }

        public String Read(String title) {
            XmlNode all = XmlDoc.SelectSingleNode("All");
            bool exit = false;
            foreach (XmlNode allNode in all)
            {
                foreach (XmlNode infoNode in allNode)
                {
                        if (exit == true) {
                            return infoNode.InnerText;
                        }
                        if (infoNode.InnerText.Equals(title))
                            exit = true;
                }
            }
            log.writeLog($"读取保存配置内容时，未找到title为'{title}'的节点",log.msgType.warning);
            return null;
        }


        public void Delete() {
            if (File.Exists(path))
            {
                File.Delete(path);
                log.writeLog($"删除保存配置完成\n路径为:'{path}'", log.msgType.info);
            }
            log.writeLog($"删除保存配置完成\n路径下没有该文件", log.msgType.info);
        }


        //static void Main(string[] args)
        //{
        //    saveConfig s = new saveConfig(null);
        //    s.Write("11", "22");
        //    s.Write("33", "24");
        //    s.Write("44", "82");
        //    s.Write("11", "22");
        //    // Console.WriteLine(s.Read("44"));
        //    //s.Delete();
        //    Console.ReadLine();
        //}

    }
}
