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

namespace PayListener
{
    public partial class Form1 : Form
    {
        private readonly static IAppConfigurator Configurator = ConfigurationFactory.FromFile(@".\config.coin").CreateAppConfigurator();
        public Form1()
        {
            InitializeComponent();

            var config = Configurator.Of<StateConfiguration>();
            remoteHostInput.Text = config.CallbackHost;
            remoteKeyInput.Text = config.CallbackKey;
            //CheckForIllegalCrossThreadCalls = false; ������߳�


            new Task(async () =>
            {
                while (true)
                {
                    Delay(20000);
                    Action set = () => { label_lasthb.Text = HeartBeatJob.LastStatus;};
                    label_lasthb.Invoke(set);
                }
            }).Start();
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
                    if (json["code"]?.ToString() == "1")
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

        private async void SetLabelText(Label txt, string value, Color color)
        {
            txt.ForeColor = color;
            txt.Text = value;
            new Task(async () =>
            {
                Delay(2000);
                if (txt.Text == value)
                {
                    Action set = () => { txt.Text = ""; txt.ForeColor = Color.Black; };
                    txt.Invoke(set);
                }
            }).Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var config = Configurator.Of<StateConfiguration>();
            config.CallbackHost = remoteHostInput.Text;
            config.CallbackKey = remoteKeyInput.Text;
            SetLabelText(remoteTipLabel, "�����ѱ���", Color.Green);
        }
        /// <summary>
        /// ���������ϱ�
        ///     true: ���� 
        ///     false: ֹͣ
        /// </summary>
        public async Task<bool> HeartBeatManagerAsync(bool type)
        {
            try
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                IScheduler scheduler = await factory.GetScheduler();
                var gropName = "Tasks";
                var jobName = "HeartBeat_job";
                var tiggerName = "HeartBeat_tigger";
                if (type)
                {
                    IJobDetail job_heartbeat = JobBuilder.Create<HeartBeatJob>()
                                        .WithIdentity(jobName, gropName)
                                        //.UsingJobData("key", "value")//Ϊ����ľ������񴫵ݲ�������ֵ�ԣ��Ǳ��룩
                                        .Build();//����
                    ITrigger trigger_heartbeat = TriggerBuilder.Create()
                                           .WithIdentity(tiggerName, gropName) //Ϊ��������tiggerName��gropName��ֵ���൱�����һ�����
                                           .StartNow()                        //���ڿ�ʼ
                                           .WithSimpleSchedule(x => x
                                               .WithIntervalInSeconds(30)     //����ʱ�䣬30��һ�Ρ�
                                               .RepeatForever())              //������ظ�ִ��
                                           .Build();                          //���մ���

                    await scheduler.ScheduleJob(job_heartbeat, trigger_heartbeat);      //�����񣬴����������������

                    await scheduler.Start();
                }
                else
                {
                    TriggerKey triggerKey = new TriggerKey(tiggerName, gropName);
                    JobKey jobKey = new JobKey(jobName, gropName);
                    if (scheduler != null)
                    {
                        await scheduler.PauseTrigger(triggerKey);
                        await scheduler.UnscheduleJob(triggerKey);
                        await scheduler.DeleteJob(jobKey);
                        await scheduler.Shutdown();// �ر�
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        private void button_startbeat_Click(object sender, EventArgs e)
        {
            var config = Configurator.Of<StateConfiguration>();
            if (config.CallbackHost == null || config.CallbackKey == null)
            {
                SetLabelText(remoteTipLabel, "�������ûص���ַ/Key", Color.Red);
                return;
            }
            if (button_startbeat.Text == "���������ϱ�")
            {
                HeartBeatManagerAsync(true);
                SetLabelText(remoteTipLabel, "�����ϱ������ɹ�", Color.Green);
                label_heartbeat_1.ForeColor = Color.Green;
                label_heartbeat_1.Text = "�����ϱ�������";
                button_startbeat.Text = "ֹͣ�����ϱ�";
            }
            else
            {
                HeartBeatManagerAsync(false);
                SetLabelText(remoteTipLabel, "�����ϱ���ֹͣ", Color.Green);
                label_heartbeat_1.ForeColor = Color.CornflowerBlue;
                label_heartbeat_1.Text = "�����ϱ�δ����";
                button_startbeat.Text = "���������ϱ�";
            }
        }


    }


    internal class StateConfiguration : Configuration
    {
        /// <summary>
        /// ��ȡ/���ûص���ַ��
        /// </summary>
        internal string CallbackHost
        {
            get => GetString();
            set => SetValue(value);
        }
        /// <summary>
        /// ��ȡ/����ͨ����Կ��
        /// </summary>
        internal string CallbackKey
        {
            get => GetString();
            set => SetValue(value);
        }
    }


    public class HeartBeatJob : IJob
    {
        public static string LastStatus = "��δ�ϱ�"; 
        private readonly static IAppConfigurator Configurator = ConfigurationFactory.FromFile(@".\config.coin").CreateAppConfigurator();
        Task IJob.Execute(IJobExecutionContext context)
        {
            var config = Configurator.Of<StateConfiguration>();
            string timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            string sign = MD5Encrypt32(timestamp + config.CallbackKey);
            var data = new JsonObject();
            data.Add("t", timestamp);
            data.Add("sign", sign.ToLower());
            try
            {
                string res = Post("http://" + config.CallbackHost + "/appHeart", data).Result;
                //MessageBox.Show("�ϱ��ɹ�:\n" + res);
                LastStatus = res;
                
            }
            catch (Exception a)
            {
                MessageBox.Show("����δԤ�ϵĴ���:\n" + a.Message);
                //throw;
            }

            return Task.CompletedTask;
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
    }

}