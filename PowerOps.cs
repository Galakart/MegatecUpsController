using log4net;
using MegatecUpsController.Properties;
using Renci.SshNet;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace MegatecUpsController
{
    static class PowerOps
    {

        private static readonly ILog applog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog eventlog = LogManager.GetLogger("eventlog");
        private static readonly byte[] entropy = new byte[] { 1, 9, 3, 1 };

        public static void ShutdownComputer()
        {            
            eventlog.Info("Выключение компьютера");
            UsbOps.StopUsbTimer();
            ProcessingPower("shutdown", "/s /t 0");
        }

        public static void HibernateComputer()
        {
            eventlog.Info("Отправка компьютера в гибернацию");
            ProcessingPower("shutdown", "/h /f");
        }

        public static void RestartComputer()
        {            
            eventlog.Info("Перезагрузка компьютера");
            UsbOps.StopUsbTimer();
            ProcessingPower("shutdown", "/r /t 0");
        }

        private static void ProcessingPower(string command, string args)
        {
            try
            {
                if (Settings.Default.sshEnabled)
                {
                    byte[] protectedData = Convert.FromBase64String(Settings.Default.sshPassword);
                    byte[] clearData = ProtectedData.Unprotect(protectedData, entropy, DataProtectionScope.CurrentUser);
                    string sshPassword = Encoding.UTF8.GetString(clearData);

                    SshClient sshclient = new SshClient(Settings.Default.sshHost, Convert.ToInt32(Settings.Default.sshPort), Settings.Default.sshLogin, sshPassword);
                    sshclient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
                    sshclient.Connect();
                    SshCommand sc = sshclient.CreateCommand("sleep 1 && " + Settings.Default.sshCommand);
                    sc.Execute();
                    sshclient.Disconnect();
                    eventlog.Info("Команда по SSH отправлена");
                }                

                Process.Start(new ProcessStartInfo(command, args)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            catch (Exception e)
            {
                applog.Error("Processing power error. " + e.Message);
            }            
        }

    }
}
