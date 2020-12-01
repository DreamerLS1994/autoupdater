using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace autoupdater
{
    public partial class Form1 : Form
    {
        private string g_upxmlname;
        private string g_localxmlname = "local.xml";
        private string g_zipname;
        private struct MyXml_S
        {
            public string version;
            public string url;
            public string md5;
        };

        MyXml_S g_upxml;
        MyXml_S g_loxml;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 字符串MD5加密
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                string txt = textBox2.Text;
                using (MD5 mi = MD5.Create())
                {
                    byte[] buffer = Encoding.Default.GetBytes(txt);
                    //开始加密
                    byte[] newBuffer = mi.ComputeHash(buffer);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < newBuffer.Length; i++)
                    {
                        sb.Append(newBuffer[i].ToString("x2"));
                    }
                    textBox4.Text = sb.ToString();
                }
            }
        }

        /// <summary>
        /// 打开文件对话框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "所有文件|*.*";
            if (op.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = op.FileName;
            }

        }

        /// <summary>
        /// 根据流数据获取md5
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private string getMd5byStream(Stream stream)
        {
            using (MD5 mi = MD5.Create())
            {
                //开始加密
                byte[] newBuffer = mi.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 文件加密对话框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            if (textBox3.Text != "")
            {
                FileStream fs = new FileStream(textBox3.Text, FileMode.Open);

                // 读取文件的 byte[] 
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                // 把 byte[] 转换成 Stream 
                Stream stream = new MemoryStream(bytes);

                textBox4.Text = getMd5byStream(stream);
            }
        }

        /// <summary>
        /// 复制MD5到粘贴板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox4.Text);
        }

        /// <summary>
        /// 下载xml文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string url = textBox1.Text;
            WebClient client = new WebClient();

            richTextBox1.AppendText("开始下载XML...");

            try
            {
                g_upxmlname = System.IO.Path.GetFileName(url);
                client.DownloadFile(url, System.Environment.CurrentDirectory + "\\" + g_upxmlname);
                richTextBox1.AppendText("\n下载XML完成！");
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("\n下载失败，错误" + ex.Message);
            }

        }

        /// <summary>
        /// XML 解析
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {

            //解析 下载 XML

            XmlDocument doc = new XmlDocument();
            doc.Load(System.Environment.CurrentDirectory + "\\" + g_upxmlname);

            XmlElement root = null;
            root = doc.DocumentElement;


            XmlNodeList nodeList = null;
            nodeList = root.SelectNodes("/Update/Soft");

            foreach (XmlElement el in nodeList)//读元素值 
            {
                List<String> strlist = new List<String>();
                richTextBox1.AppendText("\n下载的xml内容：");
                foreach (XmlNode node in el.ChildNodes)
                {
                    richTextBox1.AppendText("\n" + node.Name + "：" + node.InnerText);
                    strlist.Add(node.InnerText);
                }
                g_upxml.version = strlist[0];
                g_upxml.url = strlist[1];
                g_upxml.md5 = strlist[2];
            }

            //解析 本地XML

            doc.Load(System.Environment.CurrentDirectory + "\\" + g_localxmlname);

            root = doc.DocumentElement;
            nodeList = root.SelectNodes("/Local/Soft");

            foreach (XmlElement el in nodeList)//读元素值 
            {
                List<String> strlist = new List<String>();
                foreach (XmlNode node in el.ChildNodes)
                {
                    richTextBox1.AppendText("\n本地xml内容：\n" + node.Name + "：" + node.InnerText);
                    strlist.Add(node.InnerText);
                }
                g_loxml.version = strlist[0];
            }

        }

        /// <summary>
        /// 版本号比较
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox1.AppendText("\n本地版本号：" + g_loxml.version);
            richTextBox1.AppendText("\n待升级版本号：" + g_upxml.version);
            if (g_loxml.version == g_upxml.version)
            {
                richTextBox1.AppendText("\n版本号一致，无需升级！");
                return;
            }
            else
            {
                richTextBox1.AppendText("\n版本号校验完成，需要升级！");
                return;
            }

        }

        /// <summary>
        /// 升级包下载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            WebClient client = new WebClient();
            string url = g_upxml.url;
            richTextBox1.AppendText("\n开始下载升级文件...");

            try
            {
                g_zipname = System.IO.Path.GetFileName(url);
                client.DownloadFile(url, System.Environment.CurrentDirectory + "\\" + g_zipname);
                richTextBox1.AppendText("\n下载升级文件完成！");
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("\n下载失败，错误" + ex.Message);
            }
        }

        /// <summary>
        /// 解压文件到临时目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            //解压前先校验文件
            richTextBox1.AppendText("\n开始验证下载文件...");
            string zipname = System.Environment.CurrentDirectory + "\\" + g_zipname;
            //先验证Md5
            FileStream fs = new FileStream(zipname, FileMode.Open);

            // 读取文件的 byte[] 
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            // 把 byte[] 转换成 Stream 
            Stream stream = new MemoryStream(bytes);

            string md5 = getMd5byStream(stream);

            if (md5 != g_upxml.md5)
            {
                richTextBox1.AppendText("\n下载文件验证失败！Md5不一致！");
                return;
            }

            richTextBox1.AppendText("\n下载文件验证成功！");

            //临时解压路径
            string tmppath = System.Environment.CurrentDirectory + "\\tmp\\";

            richTextBox1.AppendText("\n开始解压升级文件...");

            try
            {
                ZipFile.ExtractToDirectory(zipname, tmppath);
            }
            catch { }

            richTextBox1.AppendText("\n解压升级文件完成！");

        }

        /// <summary>
        /// 复制文件及文件夹
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        private void copyDir(string srcPath, string destPath)
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            //获取目录下的文件和子目录
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)
                {
                    //判断是否文件夹
                    if (!Directory.Exists(destPath + "\\" + i.Name))
                    {
                        //目标目录下不存在此文件夹即创建子文件夹
                        Directory.CreateDirectory(destPath + "\\" + i.Name);
                    }
                    //递归调用复制子文件夹
                    copyDir(i.FullName, destPath + "\\" + i.Name);    
                }
                else
                {
                    //不是文件夹即复制文件，true表示可以覆盖同名文件
                    File.Copy(i.FullName, destPath + "\\" + i.Name, true);      
                }

            }
        }

        /// <summary>
        /// 替换文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            //临时解压路径
            string tmppath = System.Environment.CurrentDirectory + "\\tmp";
            string dstpath = System.Environment.CurrentDirectory;

            richTextBox1.AppendText("\n开始升级(复制)文件...");

            copyDir(tmppath, dstpath);

            richTextBox1.AppendText("\n升级完成！");

            richTextBox1.AppendText("\n清理升级文件中...！");

            DirectoryInfo di = new DirectoryInfo(tmppath);
            di.Delete(true);
            
            File.Delete(dstpath + "\\" + g_zipname);
            File.Delete(dstpath + "\\" + g_upxmlname);
            richTextBox1.AppendText("\n清理完成！");
        }
    }
}
