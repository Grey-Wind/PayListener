using dotnetCampus.Configurations;
using dotnetCampus.Configurations.Core;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Quartz.Impl;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Data;

namespace PayListener
{
    public partial class Form1 : Form
    {
        public delegate void updateHbListDelegate(Object[] data); // delegate type 
        public static updateHbListDelegate updateHbList; // delegate object
        public static Form1 form1;

        public Form1()
        {
            InitializeComponent();

            form1 = this;
            updateHbList = new updateHbListDelegate(updateHbtListMethod);

            Form_Resize(null,null);
            this.Resize += new EventHandler(Form_Resize);

            var config = Program.config;
            remoteHostInput.Text = config.CallbackHost;
            remoteKeyInput.Text = config.CallbackKey;
            wechat_add_input.Text = config.WeChatFolder;
            alipayIntervaltext.Text = config.AlipayInterval.ToString();
            //CheckForIllegalCrossThreadCalls = false; ������߳�

            DataColumn c1 = new DataColumn("ʱ��", typeof(string));
            DataColumn c2 = new DataColumn("���", typeof(string));
            DataColumn c3 = new DataColumn("��ע", typeof(string));
            DataColumn c4 = new DataColumn("�ϱ�", typeof(string));
            Program.wechat_DataTable.Columns.Add(c1);
            Program.wechat_DataTable.Columns.Add(c2);
            Program.wechat_DataTable.Columns.Add(c3);
            Program.wechat_DataTable.Columns.Add(c4);
            data_wechat_View.DataSource = Program.wechat_DataTable.DefaultView;  //DataGridView������Դ
            data_wechat_View.AllowUserToAddRows = false;		//ɾ������
            data_wechat_View.Columns[0].FillWeight = 35;
            data_wechat_View.Columns[1].FillWeight = 15;
            data_wechat_View.Columns[2].FillWeight = 20;
            data_wechat_View.Columns[3].FillWeight = 30;

            DataColumn d1 = new DataColumn("ʱ��", typeof(string));
            DataColumn d2 = new DataColumn("���", typeof(string));
            DataColumn d3 = new DataColumn("��ע", typeof(string));
            DataColumn d4 = new DataColumn("�ϱ�", typeof(string));
            DataColumn d5 = new DataColumn("���׺�", typeof(string));
            Program.alipay_DataTable.Columns.Add(d1);
            Program.alipay_DataTable.Columns.Add(d2);
            Program.alipay_DataTable.Columns.Add(d3);
            Program.alipay_DataTable.Columns.Add(d4);
            Program.alipay_DataTable.Columns.Add(d5);
            data_alipay_View.DataSource = Program.alipay_DataTable.DefaultView;  //DataGridView������Դ
            data_alipay_View.AllowUserToAddRows = false;		//ɾ������
            data_alipay_View.Columns[0].FillWeight = 35;
            data_alipay_View.Columns[1].FillWeight = 25;
            data_alipay_View.Columns[2].FillWeight = 20;
            data_alipay_View.Columns[3].FillWeight = 30;
            data_alipay_View.Columns[4].FillWeight = 40;


        }

        public void updateHbtListMethod(Object[] data)
        {
            //Console.WriteLine(data[0].ToString());
            //���ݸ��£�UI��ʱ����ֱ��EndUpdate���ƿؼ���������Ч������˸�������߼����ٶ�
            listView1.BeginUpdate();
            ListViewItem listViewItem = new ListViewItem();
            listViewItem.SubItems[0].Text = data[0].ToString();
            for (int i = 1; i < data.Length; i++)
            {
                listViewItem.SubItems.Add(data[i].ToString());
            }
            listView1.Items.Add(listViewItem);
            //�������ݴ���UI����һ���Ի��ơ�
            listView1.EndUpdate();
        }

        private async void Form_Resize(object? sender, System.EventArgs? e)
        {
            float totalColumnWidth = 10.0F;  //1.0+2.0+1.0 = 4.0

            float colPercentage0 = 3 / totalColumnWidth;
            listView1.Columns[0].Width = (int)(colPercentage0 * listView1.ClientRectangle.Width);

            float colPercentage1 = 1 / totalColumnWidth;
            listView1.Columns[1].Width = (int)(colPercentage1 * listView1.ClientRectangle.Width);

            float colPercentage2 = 6 / totalColumnWidth;
            listView1.Columns[2].Width = (int)(colPercentage2 * listView1.ClientRectangle.Width);
        }

        private async void SetLabelText(Label txt, string value, Color color)
        {
            txt.ForeColor = color;
            txt.Text = value;
            new Task(async () =>
            {
                Task.Delay(3000).Wait();
                if (txt.Text == value)
                {
                    Action set = () => { txt.Text = ""; txt.ForeColor = Color.Black; };
                    txt.Invoke(set);
                }
            }).Start();
        }

        const int WM_COPYDATA = 0x004A;//WM_COPYDATA��Ϣ����ҪĿ���������ڽ��̼䴫��ֻ�����ݡ�
        //Windows��ͨ��WM_COPYDATA��Ϣ�����ڼ䣬���ṩ�̳�ͬ����ʽ��
        //����,WM_COPYDATA��Ӧ��ʮ��������Ϊ0x004A
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_COPYDATA:
                    COPYDATASTRUCT cdata = new COPYDATASTRUCT();
                    Type mytype = cdata.GetType();
                    cdata = (COPYDATASTRUCT)m.GetLParam(mytype);
                    string Data = cdata.lpData;
                    WeChatService.WechatCallback(Data);
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            if (remoteHostInput.Text == "" || remoteKeyInput.Text == "")
            {
                SetLabelText(remoteTipLabel, "����дԶ�˵�ַ/ͨ����Կ!", Color.Red);
                return;
            }
            SetLabelText(remoteTipLabel, "���ڷ�����������...", Color.Blue);
            string timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            string sign = MD5Encrypt32(timestamp + remoteKeyInput.Text);
            var data = new JsonObject();
            data.Add("t", timestamp);
            data.Add("sign", sign.ToLower());
            new Task(async () =>
            {
                try
                {
                    string res = await Post("http://" + remoteHostInput.Text + "/appHeart", data);
                    if (res == "" || res == null)
                    {
                        remoteTipLabel.Invoke(new Action<Label, string, Color>(SetLabelText),
                                                 remoteTipLabel, "��ַ��Ч���޷�����", Color.Red);
                        return;
                    }
                    
                    var json = JsonNode.Parse(res);
                    if (json?["code"]?.ToString() == "1")
                    {
                        remoteTipLabel.Invoke(new Action<Label, string, Color>(SetLabelText),
                                                 remoteTipLabel, "���������Գɹ�", Color.Green);
                    }
                    else
                    {
                        remoteTipLabel.Invoke(new Action<Label, string, Color>(SetLabelText),
                                                 remoteTipLabel, json["msg"], Color.Red);
                    }
                }
                catch (Exception a)
                {
                    remoteTipLabel.Invoke(new Action<Label, string, Color>(SetLabelText),
                                                 remoteTipLabel, "��������", Color.Red);
                    MessageBox.Show("����δԤ�ϵĴ���:\n" + a.Message);
                    //throw;
                }
                
                
            }).Start();
            

        }
        public static void Delay(int milliSecond)
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < milliSecond)
            {
                Application.DoEvents();
            }
        }

        public static string MD5Encrypt32(string txt)
        {
            byte[] sor = Encoding.UTF8.GetBytes(txt);
            MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(sor);
            StringBuilder strbul = new StringBuilder(40);
            for (int i = 0; i < result.Length; i++)
            {
                //���ܽ��"x2"���Ϊ32λ,"x3"���Ϊ48λ,"x4"���Ϊ64λ
                strbul.Append(result[i].ToString("x2"));
            }
            return strbul.ToString();
        }

        public async Task<string> Post(string url, JsonObject data)
        {
            HttpClient httpClient = new HttpClient();
            var httpContent = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, httpContent);
            var result = await response.Content.ReadAsStringAsync();
            httpContent.Dispose();
            return result;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            var config = Program.config;
            config.CallbackHost = remoteHostInput.Text;
            config.CallbackKey = remoteKeyInput.Text;
            SetLabelText(remoteTipLabel, "�����ѱ���", Color.Green);
        }

        private async void button_startbeat_Click(object sender, EventArgs e)
        {
            var config = Program.config;
            if (config.CallbackHost == "" || config.CallbackKey == "")
            {
                SetLabelText(remoteTipLabel, "��������Զ�˵�ַ/ͨ����Կ", Color.Red);
                return;
            }
            if (button_startbeat.Text == "���������ϱ�")
            {
                if(await RemoteService.HeartBeatManagerAsync(true))
                {
                    SetLabelText(remoteTipLabel, "�����ϱ������ɹ�", Color.Green);
                    label_heartbeat_1.ForeColor = Color.Green;
                    label_heartbeat_1.Text = "�����ϱ�������";
                    button_startbeat.Text = "ֹͣ�����ϱ�";
                }
                else
                {
                    SetLabelText(remoteTipLabel, "�����ϱ�����ʧ��", Color.Red);
                }
            }
            else
            {
                if(await RemoteService.HeartBeatManagerAsync(false))
                {
                    SetLabelText(remoteTipLabel, "�����ϱ���ֹͣ", Color.Green);
                    label_heartbeat_1.ForeColor = Color.CornflowerBlue;
                    label_heartbeat_1.Text = "�����ϱ�δ����";
                    button_startbeat.Text = "���������ϱ�";
                }
                else
                {
                    SetLabelText(remoteTipLabel, "�����ϱ�ֹͣʧ��", Color.Red);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                if (checkWeChatVersion(fbd.SelectedPath))
                {
                    wechat_add_input.Text = fbd.SelectedPath;
                }
                else
                {
                    MessageBox.Show("΢�Ű汾����Ϊ 3.4.5.27 ��΢��·����Ч");
                }
            }
        }

        private bool checkWeChatVersion(string path)
        {
            try
            {
                FileVersionInfo info = FileVersionInfo.GetVersionInfo(path + "\\WeChatWin.dll");
                return info.FileVersion == "3.4.5.27";
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://www.123pan.com/s/XfT9-YxPod");
        }

        [DllImport("kernel32.dll")]
        public static extern int VirtualAllocEx(IntPtr hwnd, int lpaddress, int size, int type, int tect);

        [DllImport("kernel32.dll")]
        public static extern int WriteProcessMemory(IntPtr hwnd, int baseaddress, string buffer, int nsize, int filewriten);

        [DllImport("kernel32.dll")]
        public static extern int GetProcAddress(int hwnd, string lpname);

        [DllImport("kernel32.dll")]
        public static extern int GetModuleHandleA(string name);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hwnd, int attrib, int size, int address, int par, int flags, int threadid);
        [DllImport("KERNEL32.DLL ")]
        public static extern int CloseHandle(IntPtr handle);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (wechat_add_input.Text == "")//|| wechat_port_input.Text == "")
                {
                    SetLabelText(label_wechat_tip, "����д ΢��Ŀ¼", Color.Red);
                    return;
                }
                var config = Program.config;
                config.WeChatFolder = wechat_add_input.Text;
                Process[] avalible_p = Process.GetProcessesByName("WeChat");
                foreach (Process win_yg in avalible_p)
                {
                    win_yg.Kill();
                }
                Process myProcess = new Process();
                ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(wechat_add_input.Text + "\\WeChat.exe");
                myProcess.StartInfo = myProcessStartInfo;
                myProcess.Start();
                while (FindWindow("WeChatLoginWndForPC", null) == IntPtr.Zero)
                {
                    System.Threading.Thread.Sleep(500);
                }
                if (InjectDll(myProcess) == 0) return;
                if (true)
                {
                    SetLabelText(label_wechat_tip, "�����ɹ�", Color.Green);
                    return;
                }
                else
                {
                    SetLabelText(label_wechat_tip, "����ʧ��", Color.Red);
                    return;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            
        }
        private int InjectDll(Process myProcess)
        {
            //��ȡ��ǰ����Ŀ¼�µ�dll
            string dllfile = System.Windows.Forms.Application.StartupPath + "WeChatHook.dll";
            //MessageBox.Show(dllfile);
            if (!File.Exists(dllfile))
            {
                SetLabelText(label_wechat_tip, "����: DLL�ļ���ʧ", Color.Red);
                return 0;
            }

            //���dll�Ƿ��Ѿ�ע��
            if (!CheckIsInject(myProcess.Id))
            {
                //��΢�Ž����������ڴ�
                Int32 AllocBaseAddress = VirtualAllocEx(myProcess.Handle, 0, dllfile.Length + 1, 4096, 4);
                if (AllocBaseAddress == 0)
                {
                    SetLabelText(label_wechat_tip, "����: �ڴ����ʧ��", Color.Red);
                    return 0;
                }
                //д��dll·����΢�Ž���
                if (WriteProcessMemory(myProcess.Handle, AllocBaseAddress, dllfile, dllfile.Length + 1, 0) == 0)
                {
                    SetLabelText(label_wechat_tip, "����: DLLд��ʧ��", Color.Red);
                    return 0;
                }
                Int32 loadaddr = GetProcAddress(GetModuleHandleA("kernel32.dll"), "LoadLibraryA");
                if (loadaddr == 0)
                {
                    SetLabelText(label_wechat_tip, "����: ȡ�� LoadLibraryA �ĵ�ַʧ��", Color.Red);
                    return 0;
                }
                IntPtr ThreadHwnd = CreateRemoteThread(myProcess.Handle, 0, 0, loadaddr, AllocBaseAddress, 0, 0);
                if (ThreadHwnd == IntPtr.Zero)
                {
                    SetLabelText(label_wechat_tip, "����: ����Զ���߳�ʧ��", Color.Red);
                    return 0;
                }
                CloseHandle(ThreadHwnd);
            }
            else
            {
                SetLabelText(label_wechat_tip, "��ʾ: ��ǰ΢�Ŵ��ڼ����ؼ�, ��������΢��", Color.Red);
                return 0;
            }
            return myProcess.Id;
        }
        private bool CheckIsInject(int wxProcessid)
        {
            Process[] mProcessList = Process.GetProcesses(); //ȡ�����н���

            foreach (Process mProcess in mProcessList) //��������
            {
                if (mProcess.Id == wxProcessid)
                {

                    ProcessModuleCollection myProcessModuleCollection = mProcess.Modules;
                    ProcessModule myProcessModule;
                    for (int i = 0; i < myProcessModuleCollection.Count; i++)
                    {
                        myProcessModule = myProcessModuleCollection[i];
                        if (myProcessModule.ModuleName == "WeChatHook.dll")
                        {
                            return true;
                        }
                    }
                }

            }
            return false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if(!int.TryParse(alipayIntervaltext.Text, out var time)) { MessageBox.Show("���ʱ����Ч"); return; }

                var config = Program.config;
                config.AlipayInterval = time;

                if (button_startAlipay.Text == "ֹͣ")
                {
                    AliPayService.AliPayManagerAsync(false);
                    button_startAlipay.Text = "����";
                    button_alipayFreshCookie.Enabled = false;
                    return;
                }
                var cookie = AliPayService.GetCookie();
                if (cookie == "")
                {
                    MessageBox.Show("��¼ʧ��");
                    return;
                }
                Console.WriteLine(cookie);
                AliPayService.Init(cookie);
                AliPayService.UserInit();
                AliPayJob.LastUpdateTime = DateTime.Now;
                if (AliPayService.AliPayManagerAsync(true).Result)
                {
                    button_startAlipay.Text = "ֹͣ";
                    button_alipayFreshCookie.Enabled = true;
                    //MessageBox.Show("�����ɹ�");
                }
                else
                {
                    MessageBox.Show("����ʧ��");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void button_alipayFreshCookie_Click(object sender, EventArgs e)
        {
            AliPayService.UserInit();
        }
        
    }

}