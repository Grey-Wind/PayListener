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

    internal static class Shell
    {
        /// <summary>  
        /// �����Ϣ  
        /// </summary>  
        /// <param name="format"></param>  
        /// <param name="args"></param>  
        public static void WriteLine(string message, ConsoleColor GetConsoleColor)
        {
            try
            {
                Console.ForegroundColor = GetConsoleColor;
                Console.WriteLine(@"[{0}]{1}", DateTimeOffset.Now, message);
            }
            catch (Exception)
            {

                return;
            }
        }

        /// <summary>  
        /// �����Ϣ  
        /// </summary>  
        /// <param name="format"></param>  
        /// <param name="args"></param>  
        public static void WriteLine(string format, params object[] args)
        {
            try
            {
                WriteLine(string.Format(format, args));
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>  
        /// �����Ϣ  
        /// </summary>  
        /// <param name="output"></param>  
        public static void WriteLine(string output)
        {
            try
            {
                Console.ForegroundColor = GetConsoleColor(output);
                Console.WriteLine(@"[{0}]{1}", DateTimeOffset.Now, output);
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>  
        /// ��������ı�ѡ�����̨������ɫ  
        /// </summary>  
        /// <param name="output"></param>  
        /// <returns></returns>  
        private static ConsoleColor GetConsoleColor(string output)
        {
            if (output.StartsWith("����")) return ConsoleColor.Yellow;
            if (output.StartsWith("����")) return ConsoleColor.Red;
            if (output.StartsWith("ע��")) return ConsoleColor.Green;
            return ConsoleColor.Gray;
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
        /// <summary>
        /// ��ȡ/����ͨ����Կ��
        /// </summary>
        internal bool Callbackssl
        {
            get => GetBoolean() ?? false;
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
        /// <summary>
        /// ����ģʽ
        /// </summary>
        internal bool DebugMode
        {
            get => GetBoolean() ?? false;
            set => SetValue(value);
        }
    }
}