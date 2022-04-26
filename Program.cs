using dotnetCampus.Configurations;
using dotnetCampus.Configurations.Core;

namespace PayListener
{
    internal static class Program
    {
        public readonly static IAppConfigurator Configurator = ConfigurationFactory.FromFile(@".\config.coin").CreateAppConfigurator();
        public static StateConfiguration config = Configurator.Of<StateConfiguration>();
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        public static System.Data.DataTable wechat_DataTable = new System.Data.DataTable();
        public static System.Data.DataTable alipay_DataTable = new System.Data.DataTable();

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
        /// <summary>
        /// ��ȡ/����΢�Ű�װĿ¼
        /// </summary>
        internal string WeChatFolder
        {
            get => GetString();
            set => SetValue(value);
        }
        /// <summary>
        /// ��ȡ/����֧��������Ӽ��(��)
        /// </summary>
        internal int AlipayInterval
        {
            get => GetInt32() ?? 10;
            set => SetValue(value);
        }
    }
}