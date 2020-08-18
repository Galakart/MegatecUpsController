using FontAwesome.WPF;
using log4net;
using MegatecUpsController.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
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
        
        private static readonly ILog applog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog eventlog = LogManager.GetLogger("eventlog");

        private readonly System.Threading.Timer timerUI;

        private readonly NotifyIcon trayIcon = new NotifyIcon();
        private readonly double[] xAxis = new double[60];

        public MainWindow()
        {
            InitializeComponent();            
            LoadSettings();
            SetupTrayIcon();

            TimerCallback tm = new TimerCallback(TimerActionRefreshUI);
            timerUI = new System.Threading.Timer(tm, null, 0, 1000);

            eventlog.Info("Приложение запущено");
        }

        private void LoadSettings()
        {
            log4net.Config.XmlConfigurator.Configure();

            for (int i = 0; i < xAxis.Length; i++) //заполняем ось X (время) графика от 0 до 59
                xAxis[i] = i;

            if (Settings.Default.alwaysOnTop)
            {
                Topmost = true;
            }
            else
            {
                Topmost = false;
            }
        }

        private void SetupTrayIcon()
        {
            MenuItem MainMenuItem = new MenuItem("Главное окно", new EventHandler(ShowMainWindow));
            MenuItem SettingsMenuItem = new MenuItem("Настройки", new EventHandler(ShowSettingsWindow));
            MenuItem AboutMenuItem = new MenuItem("О программе", new EventHandler(ShowAboutWindow));
            MenuItem ExitMenuItem = new MenuItem("Завершить", new EventHandler(ExitApp));

            trayIcon.Icon = new System.Drawing.Icon(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/AppIcon.ico")).Stream);
            trayIcon.Visible = true;
            trayIcon.ContextMenu = new ContextMenu(new MenuItem[] { MainMenuItem, SettingsMenuItem, AboutMenuItem, ExitMenuItem });
            trayIcon.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    Show();
                    WindowState = WindowState.Normal;
                };
        }

        private void TimerActionRefreshUI(object obj) //TODO переделать на Binding
        {
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                if ((DateTime.Now - UpsData.LastUpdated).TotalSeconds > 10) //иногда у ИБП случается затуп и он ничего не шлёт в ответ секунд 7. Больше уж да, что-то не то
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
                    Lbl_PowerInfo.Content = "Нет данных от ИБП";

                    UpsData.InputVoltageHistory.Enqueue(0);
                    UpsData.OutputVoltageHistory.Enqueue(0);

                    VoltageInputGraph.Plot(xAxis, UpsData.InputVoltageHistory);
                    VoltageOutputGraph.PlotBars(xAxis, UpsData.OutputVoltageHistory);
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
                        Btn_UpsSoundSwitch.ToolTip = "Пищалка ИБП (включёна)";
                    }
                    else
                    {
                        Fa_UpsSoundSwitch.Icon = FontAwesomeIcon.VolumeOff;
                        Btn_UpsSoundSwitch.ToolTip = "Пищалка ИБП (выключена)";
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
                        Lbl_PowerInfo.Content = "Питание: от розетки";
                    }

                    VoltageInputGraph.Plot(xAxis, UpsData.InputVoltageHistory);
                    VoltageOutputGraph.PlotBars(xAxis, UpsData.OutputVoltageHistory);
                }

                if (UpsData.ConnectStatus)
                {
                    El_UpsStatus.Fill = Brushes.LimeGreen;
                    Lbl_BottomStatus.Content = "ИБП работает";
                    Lbl_RawInputData.Content = UpsData.RawInputData;
                }
                else
                {
                    El_UpsStatus.Fill = Brushes.Red;
                    Lbl_BottomStatus.Content = "ИБП недоступен";
                    Lbl_RawInputData.Content = "";
                }

                trayIcon.Text = Lbl_BottomStatus.Content + ". " + Lbl_PowerInfo.Content + ". " + Lbl_AvrStatus.Content;

            });
        }

        private void Btn_UpsSoundSwitch_Click(object sender, RoutedEventArgs e)
        {
            UsbOps.SwitchUpsBeeper();
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

        private void MainForm_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle; //это и ещё две строки в MainForm_Loaded - для нормальной обработки потери связи по USB
            UsbOps.usb.RegisterHandle(handle);
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource src = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            src.AddHook(new HwndSourceHook(WndProc));
            UsbOps.SetupUsbDevice(int.Parse(Settings.Default.vid, NumberStyles.AllowHexSpecifier), int.Parse(Settings.Default.pid, NumberStyles.AllowHexSpecifier));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
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
            LoadSettings();
        }

        private void ShowAboutWindow(object sender, EventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void ExitApp(object sender, EventArgs e)
        {
            UsbOps.StopUsbTimer();
            timerUI.Dispose();
            trayIcon.Dispose();
            eventlog.Info("Приложение закрыто");
            App.Current.Shutdown();
        }

        private void Menu_About_Click(object sender, RoutedEventArgs e)
        {
            ShowAboutWindow(null, null);
        }

        private void Menu_Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsWindow(null, null);
        }

        private void Menu_Help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Galakart/MegatecUpsController/wiki");
        }

        private void Menu_Update_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindow updateWindow = new UpdateWindow();
            updateWindow.ShowDialog();
        }

        private void Menu_Close_Click(object sender, RoutedEventArgs e)
        {
            ExitApp(null, null);
        }
        
    }
}
