using System.Diagnostics;

namespace MegatecUpsController
{
    static class PowerOps
    {
        public static void ShutdownComputer()
        {
            Process.Start(new ProcessStartInfo("shutdown", "/s /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        public static void HibernateComputer()
        {
            Process.Start(new ProcessStartInfo("shutdown", "/h /f")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        public static void RestartComputer()
        {
            Process.Start(new ProcessStartInfo("shutdown", "/r /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
    }
}
