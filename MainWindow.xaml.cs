using MegatecUpsController.Properties;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Globalization;
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

        RegistryKey rkAppStartup = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        NotifyIcon ni = new NotifyIcon();
        double[] x = new double[60];

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();

            for (int i = 0; i < x.Length; i++)
                x[i] = i;

            MenuItem MainMenuItem = new MenuItem("Главное окно", new EventHandler(ShowMainWindow));
            MenuItem AboutMenuItem = new MenuItem("О программе", new EventHandler(ShowAboutWindow));
            MenuItem ExitMenuItem = new MenuItem("Выход", new EventHandler(ExitApp));

            ni.Icon = Properties.Resources.AppIcon;
            ni.Visible = true;
            ni.ContextMenu = new ContextMenu(new MenuItem[] { MainMenuItem, AboutMenuItem, ExitMenuItem });
            ni.Text = "ToolTipText";
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    Show();
                    WindowState = WindowState.Normal;
                };

            UsbOps.SetupUsbDevice(int.Parse(Tb_Settings_VID.Text, NumberStyles.AllowHexSpecifier), int.Parse(Tb_Settings_PID.Text, NumberStyles.AllowHexSpecifier));

            TimerCallback tm = new TimerCallback(TimerActionRefreshUI);
            timerUI = new System.Threading.Timer(tm, null, 0, 1000);


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


        private void LoadSettings()
        {
            UpsData.ShutdownAction = Settings.Default.shutdownAction;
            UpsData.ShutdownVoltage = float.Parse(Settings.Default.shutdownVoltage, CultureInfo.InvariantCulture.NumberFormat);
            UpsData.BatteryVoltageMax = float.Parse(Settings.Default.batteryVoltage_max, CultureInfo.InvariantCulture.NumberFormat);
            UpsData.BatteryVoltageMin = float.Parse(Settings.Default.batteryVoltage_min, CultureInfo.InvariantCulture.NumberFormat);
            UpsData.BatteryVoltageMaxOnLoad = float.Parse(Settings.Default.batteryVoltage_maxOnLoad, CultureInfo.InvariantCulture.NumberFormat);

            Tb_Settings_VID.Text = Settings.Default.vid;
            Tb_Settings_PID.Text = Settings.Default.pid;
            Tb_Settings_BatteryVoltage_Max.Text = Settings.Default.batteryVoltage_max;
            Tb_Settings_BatteryVoltage_Min.Text = Settings.Default.batteryVoltage_min;
            Tb_Settings_BatteryVoltage_MaxOnLoad.Text = Settings.Default.batteryVoltage_maxOnLoad;
            Cb_ShutdownAction.SelectedIndex = Settings.Default.shutdownAction;
            Tb_ShutdownVoltage.Text = Settings.Default.shutdownVoltage;
            Chb_Settings_RunOnStartup.IsChecked = Settings.Default.runOnStartup;

        }


        private void TimerActionRefreshUI(object obj)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((System.Action)delegate {
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
                //Pb_BatteryLevel.Foreground = new Brush();

                Lbl_RawInputData.Content = UpsData.RawInputData;
                Lbl_BottomStatus.Content = UpsData.StatusLine;

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
                    El_UpsSoundStatus.Fill = Brushes.LimeGreen;
                    Lbl_UpsSoundStatus.Content = "Звук ИБП: вкл.";
                }
                else
                {
                    El_UpsSoundStatus.Fill = Brushes.Gray;
                    Lbl_UpsSoundStatus.Content = "Звук ИБП: выкл.";
                }

                VoltageInputGraph.Plot(x, UpsData.InputVoltageHistory);
                VoltageOutputGraph.PlotBars(x, UpsData.OutputVoltageHistory);


            });
        }

        private void Btn_SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            UpsData.ShutdownAction = Cb_ShutdownAction.SelectedIndex;
            UpsData.ShutdownVoltage = float.Parse(Tb_ShutdownVoltage.Text, CultureInfo.InvariantCulture.NumberFormat);
            UpsData.BatteryVoltageMax = float.Parse(Tb_Settings_BatteryVoltage_Max.Text, CultureInfo.InvariantCulture.NumberFormat);
            UpsData.BatteryVoltageMin = float.Parse(Tb_Settings_BatteryVoltage_Min.Text, CultureInfo.InvariantCulture.NumberFormat);
            UpsData.BatteryVoltageMaxOnLoad = float.Parse(Tb_Settings_BatteryVoltage_MaxOnLoad.Text, CultureInfo.InvariantCulture.NumberFormat);

            Settings.Default.vid = Tb_Settings_VID.Text;
            Settings.Default.pid = Tb_Settings_PID.Text;
            Settings.Default.batteryVoltage_max = Tb_Settings_BatteryVoltage_Max.Text;
            Settings.Default.batteryVoltage_min = Tb_Settings_BatteryVoltage_Min.Text;
            Settings.Default.batteryVoltage_maxOnLoad = Tb_Settings_BatteryVoltage_MaxOnLoad.Text;
            Settings.Default.shutdownAction = Cb_ShutdownAction.SelectedIndex;
            Settings.Default.shutdownVoltage = Tb_ShutdownVoltage.Text;
            Settings.Default.runOnStartup = (bool)Chb_Settings_RunOnStartup.IsChecked;
            Settings.Default.Save();

            if ((bool)Chb_Settings_RunOnStartup.IsChecked)
            {
                rkAppStartup.SetValue("MegatecUPSController", System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                rkAppStartup.DeleteValue("MegatecUPSController", false);
            }
        }


        private void Tb_Settings_VID_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsDigit(e.Text, 0));
        }

        private void Tb_Settings_PID_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsDigit(e.Text, 0));
        }

        private void MainForm_StateChanged(object sender, EventArgs e)
        {
        }

        private void Btn_About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }

        private void ShowMainWindow(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void ShowAboutWindow(object sender, EventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }

        private void ExitApp(object sender, EventArgs e)
        {
            UsbOps.StopUsbTimer();
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
        }

        private void MainForm_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            UsbOps.usb.RegisterHandle(handle);
        }

    }
}
