using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Windows;

namespace MegatecUpsController
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(new[] { "megatecupscontroller" });
        }
    }
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        SingleInstanceApplication app;

        public SingleInstanceManager()
        {
            this.IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
        {
            app = new SingleInstanceApplication();
            app.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);
            app.Activate();
        }
    }

    public class SingleInstanceApplication : Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow window = new MainWindow();
            window.Show();
        }

        public void Activate()
        {
            this.MainWindow.WindowState = WindowState.Normal;
            this.MainWindow.Activate();
        }
    }
}
