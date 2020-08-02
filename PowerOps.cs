using log4net;
using System;
using System.Diagnostics;
using System.Reflection;

namespace MegatecUpsController
{
    static class PowerOps
    {

        private static readonly ILog applog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog eventlog = LogManager.GetLogger("eventlog");

        public static void ShutdownComputer()
        {
            eventlog.Info("Выключение компьютера");
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
            ProcessingPower("shutdown", "/r /t 0");
        }

        private static void ProcessingPower(string command, string args)
        {
            try
            {
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
