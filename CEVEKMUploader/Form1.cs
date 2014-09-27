using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Resources;
using EveCacheParser;
using Newtonsoft.Json;

namespace CEVEKMUploader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static List<Dictionary<object, object>> killmails;
        private FileInfo[] cachedFiles = new FileInfo[0];
        private void button1_Click(object sender, EventArgs e)
        {
            killmails = new List<Dictionary<object, object>>();
            button1.Enabled = false;
            Thread thread = new Thread(start_upload);
            thread.IsBackground = true;
            thread.Start();
        }

        public static List<List<Dictionary<object, object>>> splitList(List<Dictionary<object, object>> locations, int nSize = 30)
        {
            List<List<Dictionary<object, object>>> list = new List<List<Dictionary<object, object>>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }
            return list;
        }

        /// <summary>
        /// 开始上传
        /// </summary>
        private void start_upload()
        {
            if (cachedFiles.Count() == 0)
            {
                Parser.SetCachedFilesFolders("CachedMethodCalls");
                Parser.SetIncludeMethodsFilter("GetRecentKillsAndLosses");
                Parser.SetIncludeMethodsFilter("GetRecentShipKillsAndLosses");
                Parser.SetIncludeMethodsFilter("GetKillMail");

                cachedFiles = Parser.GetMachoNetCachedFiles();
            }
            foreach (FileInfo cachedFile in cachedFiles /*.Where(x => x.Name == "9d34.cache")*/)
            {
                try
                {
                    KeyValuePair<object, object> result = Parser.Parse(cachedFile);
                    CheckResult(result);
                }
                catch (ParserException ex) { }
                catch (Exception ex) { }
            }
            //界面更新
            this.Invoke((MethodInvoker)delegate
            {
                textBox1.AppendText("读取完成, 总共" + killmails.Count + "封 KillMail." + Environment.NewLine);
                textBox1.AppendText("正在上传...." + Environment.NewLine);
                progressBar1.Value = 0;
                progressBar1.Maximum = killmails.Count;
            });

            var kmlists = splitList(killmails);
            try
            {
                //上传内容
                foreach (var kmlist in kmlists)
                {
                    var output = JsonConvert.SerializeObject(kmlist);    //生成Json
                    
                    //使用httpWebRequest进行上传（需要获取Cookie?）
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://kb.ceve-market.org/uploadjson/");
                    httpWebRequest.ContentType = "text/json";
                    httpWebRequest.Method = "POST";

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = output;

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                    }
                    List<Dictionary<object, object>> kmlist1 = kmlist;
                    this.Invoke((MethodInvoker)(() => { progressBar1.Value += kmlist1.Count; }));

                }
                this.Invoke((MethodInvoker)(() => { textBox1.AppendText("上传完成" + Environment.NewLine); }));
            }
            catch
            {
                this.Invoke((MethodInvoker)(() => { textBox1.AppendText("上传失败" + Environment.NewLine); button1.Enabled = true; }));
            }
            //this.Invoke((MethodInvoker)(() => {  button1.Enabled = true; }));
        }

        /// <summary>
        /// 检查返回值
        /// </summary>
        /// <param name="result">钥匙对</param>
        private static void CheckResult(KeyValuePair<object, object> result)
        {
            if (result.Key == null || result.Value == null)
            {
                Console.WriteLine("Parsing failed: Yielded no result");
                return;
            }

            object value;
            object id = result.Key as string ??
                        ((List<object>)((Tuple<object>)result.Key).Item1).First() as string ??
                        ((List<object>)
                            ((Tuple<object>)((List<object>)((Tuple<object>)result.Key).Item1).First()).Item1)
                            .First() as string;

            Dictionary<object, object> resultValue = result.Value as Dictionary<object, object>;
            if (resultValue == null)
            {
                value = ((List<object>)Parser.GetObject(((List<object>)result.Value).First())).First();
                return;
            }

            object lret = resultValue["lret"];
            object method = ((List<object>)((Tuple<object>)result.Key).Item1).Skip(1).First() as string;

            switch ((string)method)
            {
                case "GetRecentKillsAndLosses":
                case "GetRecentShipKillsAndLosses":
                    {
                        try
                        {
                            var methodresult = (List<object>)lret;
                            foreach (var o in methodresult)
                            {
                                if (o is Dictionary<object, object>)
                                    killmails.Add((Dictionary<object, object>)o);
                            }

                        }
                        catch (Exception) { }
                        break;

                    }
                case "GetKillMail":
                    {
                        try
                        {

                            var methodresult = (List<object>)((Tuple<object>)lret).Item1;
                            foreach (var o in methodresult)
                            {
                                if (o is Dictionary<object, object>)
                                    killmails.Add((Dictionary<object, object>)o);
                            }
                        }
                        catch (Exception) { }
                        break;
                    }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://kb.ceve-market.org");
            }
            catch { }
        }

        /// <summary>
        /// 托盘处理
        /// </summary>
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
            }
            else
            {
                if (this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Minimized;
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
            }
            else
            {
                if (this.WindowState == FormWindowState.Normal) this.WindowState = FormWindowState.Minimized;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try { Process.Start("http://ceve-market.org/"); }
            catch { }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try { Process.Start("http://kb.ceve-market.org"); }
            catch { }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            button1.PerformClick();
            timer1.Start();
        }

        /// <summary>
        /// 图标使用资源图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 3600 * 1000;
            timer1.Start();
            this.Icon = global::CEVEKMUploader.Properties.Resources.KM_logo;
            notifyIcon1.Icon = global::CEVEKMUploader.Properties.Resources.KM_logo;
        }
    }
}
