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
using System.ComponentModel;
using System.Reflection;
using System.Drawing.Drawing2D;

namespace PayListener
{
    public partial class Form1 : Form
    {
        public delegate void updateHbListDelegate(Object[] data); // delegate type 
        public static updateHbListDelegate updateHbList; // delegate object
        public static Form1 form1;
        private delegate void InvokeHandler();

        public Form1()
        {
            InitializeComponent();
            //this.BindWaterMark();
            form1 = this;
            updateHbList = new updateHbListDelegate(updateHbtListMethod);

            Form_Resize(null,null);
            this.Resize += new EventHandler(Form_Resize);

            var config = Program.config;
            remoteHostInput.Text = config.CallbackHost;
            remoteKeyInput.Text = config.CallbackKey;
            wechat_add_input.Text = config.WeChatFolder;
            alipayIntervaltext.Text = config.AlipayInterval.ToString();
            checkBox1.Checked = config.Callbackssl;
            check_console.Checked = config.DebugMode;
            //CheckForIllegalCrossThreadCalls = false; ������߳�
            LoadConsole();

            Shell.WriteLine("{0}|����{1}", "��Ϣ", "����汾 " + Assembly.GetExecutingAssembly().GetName().Version?.ToString());
            about_label.Text = about_label.Text.Replace("{version}", Assembly.GetExecutingAssembly().GetName().Version?.ToString());
            string? dllinfo;
            try
            {
                dllinfo = FileVersionInfo.GetVersionInfo(System.Windows.Forms.Application.StartupPath + "WeChatHook.dll")?.FileVersion;
                if(dllinfo != null)
                {
                    Shell.WriteLine("{0}|΢�ţ�{1}", "��Ϣ", "΢��ģ��汾: " + dllinfo);
                }
                else
                {
                    Shell.WriteLine("{0}|΢�ţ�{1}", "����", "΢��ģ��δ�ҵ�����Ч");
                }
            }
            catch (Exception)
            {
                Shell.WriteLine("{0}|΢�ţ�{1}", "����", "΢��ģ��δ�ҵ�����Ч");
            }

            if(System.Text.RegularExpressions.Regex.IsMatch(System.Windows.Forms.Application.StartupPath, "[\u4e00-\u9fbb]"))
                Shell.WriteLine("{0}|΢�ţ�{1}", "����", "����Ŀ¼ \""+ System.Windows.Forms.Application.StartupPath +"\" ��������, ΢�ż������޷�ʹ��");
        }

        #region ����̨��־
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool AllocConsole();//����

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        public static extern void FreeConsole();//�ر�

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "FindWindow")]
        extern static IntPtr FindWindow(string lpClassName, string lpWindowName);//�ҳ����еĴ���   

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        extern static IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert); //ȡ���������еĲ˵�   

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        extern static IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags); //�ҵ���ť

        [System.Runtime.InteropServices.DllImport("User32.dll ", EntryPoint = "SetParent")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);//���ø�����

        [System.Runtime.InteropServices.DllImport("user32.dll ", EntryPoint = "ShowWindow")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);//��ʾ����


        private void LoadConsole()
        {
            if (AllocConsole())
            {
                Console.Title = "֧�������ص� - ��־";//�ȶ��崰�ڱ��⣬��ֹδ�ҵ�
                Thread.Sleep(100);
                var windowHandle = FindWindow(null, "֧�������ص� - ��־");
                if (Program.config.DebugMode)
                {
                    ShowWindow(windowHandle, 1);
                }
                else
                {
                    ShowWindow(windowHandle, 0);
                }
            }
        }


        private void check_console_CheckedChanged(object sender, EventArgs e)
        {
            Program.config.DebugMode = check_console.Checked;
            var windowHandle = FindWindow(null, "֧�������ص� - ��־");
            ShowWindow(windowHandle, Program.config.DebugMode ? 1 : 0);
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

            this.Invoke(new Action(delegate {

                DataColumn c1 = new DataColumn("ʱ��", typeof(string));
                DataColumn c2 = new DataColumn("���", typeof(string));
                DataColumn c3 = new DataColumn("��ע", typeof(string));
                DataColumn c4 = new DataColumn("�ϱ�", typeof(string));
                Program.wechat_DataTable.Columns.Add(c1);
                Program.wechat_DataTable.Columns.Add(c2);
                Program.wechat_DataTable.Columns.Add(c3);
                Program.wechat_DataTable.Columns.Add(c4);
                data_wechat_View.DataSource = Program.wechat_DataTable.DefaultView;  //DataGridView������Դ
                data_wechat_View.AllowUserToAddRows = false;        //ɾ������
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
                data_alipay_View.AllowUserToAddRows = false;        //ɾ������
                data_alipay_View.Columns[0].FillWeight = 35;
                data_alipay_View.Columns[1].FillWeight = 25;
                data_alipay_View.Columns[2].FillWeight = 20;
                data_alipay_View.Columns[3].FillWeight = 30;
                data_alipay_View.Columns[4].FillWeight = 40;

            }));
            
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
            Shell.WriteLine("{0}|�ϱ���{1}", "��Ϣ", "���ڷ�����������: " + (Program.config.Callbackssl ? "https://" : "http://") + remoteHostInput.Text + "/appHeart");
            string timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            string sign = MD5Encrypt32(timestamp + remoteKeyInput.Text);
            var data = new JsonObject();
            data.Add("t", timestamp);
            data.Add("sign", sign.ToLower());
            new Task(async () =>
            {
                try
                {
                    string res = await Post((Program.config.Callbackssl ? "https://" : "http://") + remoteHostInput.Text + "/appHeart", data);
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
                        Shell.WriteLine("{0}|�ϱ���{1}", "��Ϣ", "���������Գɹ�: " + json.ToJsonString());
                    }
                    else
                    {
                        remoteTipLabel.Invoke(new Action<Label, string, Color>(SetLabelText),
                                                 remoteTipLabel, json?["msg"], Color.Red);
                        Shell.WriteLine("{0}|�ϱ���{1}", "����", "�ϱ�ʧ��, Զ�˷���: " + json?["msg"]);
                    }
                }
                catch (Exception a)
                {
                    remoteTipLabel.Invoke(new Action<Label, string, Color>(SetLabelText),
                                                 remoteTipLabel, "��������", Color.Red);
                    Shell.WriteLine("{0}|�ϱ���{1}", "����", "����δԤ�ϵĴ���: " + a.Message);
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
            Shell.WriteLine("{0}��{1}", "��Ϣ", "�����ļ��ѱ���");
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
                    Shell.WriteLine("{0}|�ϱ���{1}", "��Ϣ", "�����ϱ�������");
                    label_heartbeat_1.ForeColor = Color.Green;
                    label_heartbeat_1.Text = "�����ϱ�������";
                    button_startbeat.Text = "ֹͣ�����ϱ�";
                }
                else
                {
                    Shell.WriteLine("{0}|�ϱ���{1}", "����", "�����ϱ�����ʧ��");
                    SetLabelText(remoteTipLabel, "�����ϱ�����ʧ��", Color.Red);
                }
            }
            else
            {
                if(await RemoteService.HeartBeatManagerAsync(false))
                {
                    SetLabelText(remoteTipLabel, "�����ϱ���ֹͣ", Color.Green);
                    Shell.WriteLine("{0}|�ϱ���{1}", "��Ϣ", "�����ϱ���ֹͣ");
                    label_heartbeat_1.ForeColor = Color.CornflowerBlue;
                    label_heartbeat_1.Text = "�����ϱ�δ����";
                    button_startbeat.Text = "���������ϱ�";
                }
                else
                {
                    SetLabelText(remoteTipLabel, "�����ϱ�ֹͣʧ��", Color.Red);
                    Shell.WriteLine("{0}|�ϱ���{1}", "����", "�����ϱ�ֹͣʧ��");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                var (a,b) = checkWeChatVersion(fbd.SelectedPath);
                if (a)
                {
                    wechat_add_input.Text = fbd.SelectedPath;
                }
                else
                {
                    MessageBox.Show(b);
                }
            }
        }

        private (bool,string) checkWeChatVersion(string path)
        {
            try
            {
                FileVersionInfo hookdll_info = FileVersionInfo.GetVersionInfo(System.Windows.Forms.Application.StartupPath + "WeChatHook.dll");
                FileVersionInfo wechat_info = FileVersionInfo.GetVersionInfo(path + "\\WeChatWin.dll");
                return wechat_info.FileVersion == hookdll_info.FileVersion ? (true, "") : (false, "΢�Ű汾����Ϊ "+hookdll_info.FileVersion);
            }
            catch (Exception e)
            {
                return (false,"΢��Ŀ¼��Ч���޷��ҵ�΢��ģ��");
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

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(System.Windows.Forms.Application.StartupPath, "[\u4e00-\u9fbb]"))
                {
                    Shell.WriteLine("{0}|΢�ţ�{1}", "����", "����Ŀ¼ \"" + System.Windows.Forms.Application.StartupPath + "\" ��������, ΢�ż����޷�ʹ��");
                    MessageBox.Show("����Ŀ¼ \"" + System.Windows.Forms.Application.StartupPath + "\" ��������, ΢�ż����޷�ʹ��");
                    return;

                }
                if (wechat_add_input.Text == "")//|| wechat_port_input.Text == "")
                {
                    SetLabelText(label_wechat_tip, "����д ΢��Ŀ¼", Color.Red);
                    return;
                }
                var version_check = checkWeChatVersion(wechat_add_input.Text);
                if (!version_check.Item1)//|| wechat_port_input.Text == "")
                {
                    SetLabelText(label_wechat_tip, version_check.Item2, Color.Red);
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
                    Shell.WriteLine("{0}|΢�ţ�{1}", "��Ϣ", "΢��������");
                    return;
                }
                else
                {
                    SetLabelText(label_wechat_tip, "����ʧ��", Color.Red);
                    Shell.WriteLine("{0}|΢�ţ�{1}", "����", "΢������ʧ��");
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
                Shell.WriteLine("{0}|΢�ţ�{1}", "����", "DLL�ļ���ʧ: " + "WeChatHook.dll");
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
                    Shell.WriteLine("{0}|΢�ţ�{1}", "����", "�ڴ����ʧ��");
                    return 0;
                }
                //д��dll·����΢�Ž���
                if (WriteProcessMemory(myProcess.Handle, AllocBaseAddress, dllfile, dllfile.Length + 1, 0) == 0)
                {
                    SetLabelText(label_wechat_tip, "����: DLLд��ʧ��", Color.Red);
                    Shell.WriteLine("{0}|΢�ţ�{1}", "����", "DLLд��ʧ��");
                    return 0;
                }
                Int32 loadaddr = GetProcAddress(GetModuleHandleA("kernel32.dll"), "LoadLibraryA");
                if (loadaddr == 0)
                {
                    SetLabelText(label_wechat_tip, "����: ȡ�� LoadLibraryA �ĵ�ַʧ��", Color.Red);
                    Shell.WriteLine("{0}|΢�ţ�{1}", "����", "ȡ�� LoadLibraryA �ĵ�ַʧ��");
                    return 0;
                }
                IntPtr ThreadHwnd = CreateRemoteThread(myProcess.Handle, 0, 0, loadaddr, AllocBaseAddress, 0, 0);
                if (ThreadHwnd == IntPtr.Zero)
                {
                    SetLabelText(label_wechat_tip, "����: ����Զ���߳�ʧ��", Color.Red);
                    Shell.WriteLine("{0}|΢�ţ�{1}", "����", "����Զ���߳�ʧ��");
                    return 0;
                }
                CloseHandle(ThreadHwnd);
            }
            else
            {
                SetLabelText(label_wechat_tip, "��ʾ: ��ǰ΢�Ŵ��ڼ����ؼ�, ��������΢��", Color.Red);
                Shell.WriteLine("{0}|΢�ţ�{1}", "����", "��ǰ΢�Ŵ��ڼ����ؼ�, ��������΢��");
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
                    Shell.WriteLine("{0}|֧������{1}", "��Ϣ", "֧��������������ֹͣ");
                    button_startAlipay.Text = "����";
                    button_alipayFreshCookie.Enabled = false;
                    return;
                }
                var cookie = AliPayService.GetCookie();
                if (cookie == "")
                {
                    Shell.WriteLine("{0}|֧������{1}", "����", "֧������¼ʧ��");
                    MessageBox.Show("��¼ʧ��");
                    return;
                }
                //Console.WriteLine(cookie);
                AliPayService.Init(cookie);
                AliPayService.UserInit();
                AliPayJob.LastUpdateTime = DateTime.Now;
                if (AliPayService.AliPayManagerAsync(true).Result)
                {
                    button_startAlipay.Text = "ֹͣ";
                    button_alipayFreshCookie.Enabled = true;
                    //MessageBox.Show("�����ɹ�");
                    Shell.WriteLine("{0}|֧������{1}", "��Ϣ", "֧�����������������ɹ�");
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Program.config.Callbackssl = checkBox1.Checked;
        }


        private void remoteHostInput_TextChanged(object sender, EventArgs e)
        {
            if (remoteHostInput.Text.StartsWith("http://"))
            {
                checkBox1.Checked = false;
                remoteHostInput.Text = remoteHostInput.Text.Replace("http://", "");
            }
            else if (remoteHostInput.Text.StartsWith("https://"))
            {
                checkBox1.Checked = true;
                remoteHostInput.Text = remoteHostInput.Text.Replace("https://", "");
            }
        }

    }

    public static class ControlExtension
    {
        const float cos30 = 0.866f;
        const float sin30 = 0.5f;
        public static void BindWaterMark(this Control ctrl)
        {
            if (ctrl == null || ctrl.IsDisposed)
                return;
            // ����ˮӡ
            if (ctrl.HaveEventHandler("Paint", "BindWaterMark"))
                return;
            ctrl.Paint += (sender, e) =>
            {
                System.Windows.Forms.Control paintCtrl = sender as System.Windows.Forms.Control;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // ����ؼ�λ��
                int offsetX = 0;
                int offsetY = 0;
                while (paintCtrl.Parent != null)
                {
                    offsetX += paintCtrl.Location.X;
                    offsetY += paintCtrl.Location.Y;
                    paintCtrl = paintCtrl.Parent;
                }

                // ƽ�ƻ������������Ͻ�
                g.TranslateTransform(0 - offsetX, 0 - offsetY + 32);

                //��ʱ����ת30��
                g.RotateTransform(-30);

                for (int x = 0; x < e.ClipRectangle.Right + 64 + offsetX; x += 128)
                {
                    for (int y = 0; y < e.ClipRectangle.Bottom + 64 + offsetY; y += 128)
                    {
                        // �����������λ��
                        float x1 = cos30 * x - sin30 * y;
                        float y1 = sin30 * x + cos30 * y;

                        //��������
                        g.DrawString("���԰汾 0.1.0.5", new Font("΢���ź�", 16, FontStyle.Regular),
                            new SolidBrush(Color.FromArgb(50, 100, 100, 100)), x1, y1);
                        g.DrawString("QQȺ: 312558935", new Font("΢���ź�", 10, FontStyle.Regular),
                            new SolidBrush(Color.FromArgb(50, 100, 100, 100)), x1, y1-10);
                    }
                }
            };
            // �ӿؼ��󶨻����¼�
            foreach (System.Windows.Forms.Control child in ctrl.Controls)
                BindWaterMark(child);
        }

        public static bool HaveEventHandler(this Control control, string eventName, string methodName)
        {
            //��ȡControl�ඨ��������¼�����Ϣ
            PropertyInfo pi = (control.GetType()).GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic);
            //��ȡControl����control���¼���������б�
            EventHandlerList ehl = (EventHandlerList)pi.GetValue(control, null);

            //��ȡControl�� eventName �¼����ֶ���Ϣ
            FieldInfo fieldInfo = (typeof(Control)).GetField(string.Format("Event{0}", eventName), BindingFlags.Static | BindingFlags.NonPublic);
            //�û�ȡ�� eventName �¼����ֶ���Ϣ��ȥƥ�� control ������¼���������б���ȡcontrol���� eventName �¼���ί�ж���
            //�¼�ʹ��ί�ж���ģ�C#�е�ί��ʱ�ಥί�У����԰󶨶���¼�������򣬵��¼�����ʱ����Щ�¼������������ִ��
            //���Delegate������һ��GetInvocationList������������ȡ���ί���Ѿ��󶨵������¼��������
            Delegate d = ehl?[fieldInfo?.GetValue(null)];

            if (d == null)
                return false;

            foreach (Delegate del in d.GetInvocationList())
            {
                string anonymous = string.Format("<{0}>", methodName);
                //�ж�һ��ĳ���¼���������Ƿ��Ѿ����󶨵� eventName �¼���
                if (del.Method.Name == methodName || del.Method.Name.StartsWith(anonymous))
                {
                    return true;
                }
            }

            return false;
        }
    }
}