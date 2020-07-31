using FontAwesome.WPF;
using log4net;
using MegatecUpsController.Properties;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace MegatecUpsController
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private System.Threading.Timer timerUI;
        private static readonly ILog applog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog eventlog = LogManager.GetLogger("eventlog");


        NotifyIcon ni = new NotifyIcon();
        double[] x = new double[60];

        public MainWindow()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
            LoadUiSettings();

            for (int i = 0; i < x.Length; i++)
                x[i] = i;

            MenuItem MainMenuItem = new MenuItem("Главное окно", new EventHandler(ShowMainWindow));
            MenuItem SettingsMenuItem = new MenuItem("Настройки", new EventHandler(ShowSettingsWindow));
            MenuItem AboutMenuItem = new MenuItem("О программе", new EventHandler(ShowAboutWindow));
            MenuItem ExitMenuItem = new MenuItem("Выход", new EventHandler(ExitApp));

            ni.Icon = Properties.Resources.AppIcon;
            ni.Visible = true;
            ni.ContextMenu = new ContextMenu(new MenuItem[] { MainMenuItem, SettingsMenuItem, AboutMenuItem, ExitMenuItem });
            ni.Text = "ToolTipText";
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    Show();
                    WindowState = WindowState.Normal;
                };

            TimerCallback tm = new TimerCallback(TimerActionRefreshUI);
            timerUI = new System.Threading.Timer(tm, null, 0, 1000);

            applog.Info("app log normal");
            eventlog.Info("event log normal");

        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            Message m = new Message
            {
                HWnd = hwnd,
                Msg = msg,
                WParam = wParam,
                LParam = lParam
            };
            UsbOps.usb.ParseMessages(ref m);
            return IntPtr.Zero;
        }


        private void LoadUiSettings()
        {
            

            

            if (Settings.Default.alwaysOnTop)
            {
                Topmost = true;
            }
            else
            {
                Topmost = false;
            }

            

        }


        private void TimerActionRefreshUI(object obj)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate {
                if ((DateTime.Now - UpsData.LastUpdated).TotalSeconds > 10)
                {
                    Lbl_InputVoltage.Content = "???";
                    Lbl_OutputVoltage.Content = "???";
                    Lbl_Temperature.Content = "???";
                    Lbl_Hz.Content = "???";
                    Lbl_LoadPercent.Content = "???";
                    Lbl_CurAmper.Content = "???";
                    Lbl_CurWatt.Content = "???";
                    Lbl_CurVA.Content = "???";
                    Pb_BatteryLevel.Value = 0;
                    Lbl_BatteryVoltage.Content = "???";
                    Lbl_PowerInfo.Content = "";

                    UpsData.InputVoltageHistory.Enqueue(0);
                    UpsData.OutputVoltageHistory.Enqueue(0);

                    VoltageInputGraph.Plot(x, UpsData.InputVoltageHistory);
                    VoltageOutputGraph.PlotBars(x, UpsData.OutputVoltageHistory);
                }
                else
                {
                    Lbl_InputVoltage.Content = UpsData.InputVoltage + " В";
                    Lbl_OutputVoltage.Content = UpsData.OutputVoltage + " В";
                    Lbl_Temperature.Content = UpsData.Temperature + "°";
                    Lbl_Hz.Content = UpsData.Hz + " Гц";
                    Lbl_LoadPercent.Content = UpsData.LoadPercent + " %";
                    Lbl_CurAmper.Content = UpsData.CurAmper + " А";
                    Lbl_CurWatt.Content = UpsData.CurWatt + " ВТ";
                    Lbl_CurVA.Content = UpsData.CurVA + " ВА";
                    Pb_BatteryLevel.Value = UpsData.BatteryPercent;
                    Lbl_BatteryVoltage.Content = UpsData.BatteryVoltage + " В";

                    if (UpsData.IsActiveAVR)
                    {
                        El_AvrStatus.Fill = Brushes.Yellow;
                        Lbl_AvrStatus.Content = "AVR: вкл.";
                    }
                    else
                    {
                        El_AvrStatus.Fill = Brushes.Gray;
                        Lbl_AvrStatus.Content = "AVR: выкл.";
                    }

                    if (UpsData.IsBeeperOn)
                    {
                        Fa_UpsSoundSwitch.Icon = FontAwesomeIcon.VolumeUp;
                        Btn_UpsSoundSwitch.ToolTip = "Звук из ИБП (включён)";
                    }
                    else
                    {
                        Fa_UpsSoundSwitch.Icon = FontAwesomeIcon.VolumeOff;
                        Btn_UpsSoundSwitch.ToolTip = "Звук из ИБП (выключен)";
                    }

                    if (UpsData.IsUtilityFail)
                    {
                        Lbl_PowerInfo.FontWeight = FontWeights.Bold;
                        Lbl_PowerInfo.Foreground = Brushes.Red;
                        Lbl_PowerInfo.Content = "Питание: ОТ БАТАРЕИ";
                    }
                    else
                    {
                        Lbl_PowerInfo.FontWeight = FontWeights.Normal;
                        Lbl_PowerInfo.Foreground = Brushes.Black;
                        Lbl_PowerInfo.Content = "Питание: от сети";
                    }

                    VoltageInputGraph.Plot(x, UpsData.InputVoltageHistory);
                    VoltageOutputGraph.PlotBars(x, UpsData.OutputVoltageHistory);
                }

                if (UpsData.ConnectStatus)
                {
                    El_UpsStatus.Fill = Brushes.LimeGreen;
                    Lbl_BottomStatus.Content = "ИБП подключён";
                    Lbl_RawInputData.Content = UpsData.RawInputData;
                }
                else
                {
                    El_UpsStatus.Fill = Brushes.Red;
                    Lbl_BottomStatus.Content = "ИБП недоступен";
                    Lbl_RawInputData.Content = "";
                }

            });
        }



        private void MainForm_StateChanged(object sender, EventArgs e)
        {
        }


        private void ShowMainWindow(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void ShowSettingsWindow(object sender, EventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            LoadUiSettings();
        }

        private void ShowAboutWindow(object sender, EventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }

        private void ExitApp(object sender, EventArgs e)
        {
            UsbOps.StopUsbTimer();
            timerUI.Dispose();
            ni.Dispose();
            App.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }

        private void Btn_UpsSoundSwitch_Click(object sender, RoutedEventArgs e)
        {
            UsbOps.sendSwitchBeeper();
        }

        private void MainForm_Closing(object sender, CancelEventArgs e)
        {

        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource src = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            src.AddHook(new HwndSourceHook(WndProc));
            connectUps();
        }

        private void MainForm_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            UsbOps.usb.RegisterHandle(handle);
        }


        private bool connectUps()
        {
            return UsbOps.SetupUsbDevice(int.Parse(Settings.Default.vid, NumberStyles.AllowHexSpecifier), int.Parse(Settings.Default.pid, NumberStyles.AllowHexSpecifier));
        }

        

        private void Menu_About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }

        private void Menu_Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            LoadUiSettings();
        }

        private void Menu_Close_Click(object sender, RoutedEventArgs e)
        {
            ExitApp(null, null);
        }
    }
}
