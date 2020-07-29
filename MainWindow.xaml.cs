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
            UpsData.UpsVA = float.Parse(Settings.Default.upsVA, CultureInfo.InvariantCulture.NumberFormat);

            Tb_Settings_VID.Text = Settings.Default.vid;
            Tb_Settings_PID.Text = Settings.Default.pid;
            Tb_Settings_BatteryVoltage_Max.Text = Settings.Default.batteryVoltage_max;
            Tb_Settings_BatteryVoltage_Min.Text = Settings.Default.batteryVoltage_min;
            Tb_Settings_BatteryVoltage_MaxOnLoad.Text = Settings.Default.batteryVoltage_maxOnLoad;
            Tb_Settings_UpsVA.Text = Settings.Default.upsVA;
            Cb_ShutdownAction.SelectedIndex = Settings.Default.shutdownAction;
            Tb_ShutdownVoltage.Text = Settings.Default.shutdownVoltage;
            Chb_Settings_RunOnStartup.IsChecked = Settings.Default.runOnStartup;
            Chb_Settings_AlwaysOnTop.IsChecked = Settings.Default.alwaysOnTop;
            Chb_Settings_RunMinimized.IsChecked = Settings.Default.runMinimized;

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

                    Lbl_AvrStatus.Content = "AVR: ???";
                    Lbl_UpsSoundStatus.Content = "Звук ИБП: ???";

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

        private void Btn_SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            UpsData.ShutdownAction = Cb_ShutdownAction.SelectedIndex;
            UpsData.ShutdownVoltage = float.Parse(Tb_ShutdownVoltage.Text, CultureInfo.InvariantCulture.NumberFormat);
            UpsData.BatteryVoltageMax = float.Parse(Tb_Settings_BatteryVoltage_Max.Text, CultureInfo.InvariantCulture.NumberFormat);
            UpsData.BatteryVoltageMin = float.Parse(Tb_Settings_BatteryVoltage_Min.Text, CultureInfo.InvariantCulture.NumberFormat);
            UpsData.BatteryVoltageMaxOnLoad = float.Parse(Tb_Settings_BatteryVoltage_MaxOnLoad.Text, CultureInfo.InvariantCulture.NumberFormat);
            UpsData.UpsVA = float.Parse(Tb_Settings_UpsVA.Text, CultureInfo.InvariantCulture.NumberFormat);

            Settings.Default.vid = Tb_Settings_VID.Text;
            Settings.Default.pid = Tb_Settings_PID.Text;
            Settings.Default.batteryVoltage_max = Tb_Settings_BatteryVoltage_Max.Text;
            Settings.Default.batteryVoltage_min = Tb_Settings_BatteryVoltage_Min.Text;
            Settings.Default.batteryVoltage_maxOnLoad = Tb_Settings_BatteryVoltage_MaxOnLoad.Text;
            Settings.Default.upsVA = Tb_Settings_UpsVA.Text;
            Settings.Default.shutdownAction = Cb_ShutdownAction.SelectedIndex;
            Settings.Default.shutdownVoltage = Tb_ShutdownVoltage.Text;
            Settings.Default.runOnStartup = (bool)Chb_Settings_RunOnStartup.IsChecked;
            Settings.Default.alwaysOnTop = (bool)Chb_Settings_AlwaysOnTop.IsChecked;
            Settings.Default.runMinimized = (bool)Chb_Settings_RunMinimized.IsChecked;
            Settings.Default.Save();

            if ((bool)Chb_Settings_RunOnStartup.IsChecked)
            {
                rkAppStartup.SetValue("MegatecUPSController", Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                rkAppStartup.DeleteValue("MegatecUPSController", false);
            }

            if ((bool)Chb_Settings_AlwaysOnTop.IsChecked)
            {
                Topmost = true;
            }
            else
            {
                Topmost = false;
            }

            if (UsbOps.usb.SpecifiedDevice == null)
            {
                connectUps();
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


        private void Btn_Debug_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private bool connectUps()
        {
            return UsbOps.SetupUsbDevice(int.Parse(Tb_Settings_VID.Text, NumberStyles.AllowHexSpecifier), int.Parse(Tb_Settings_PID.Text, NumberStyles.AllowHexSpecifier));
        }

        private void Btn_SearchUps_Click(object sender, RoutedEventArgs e)
        {
            bool success = false;
            string tmpVID = Tb_Settings_VID.Text;
            string tmpPID = Tb_Settings_PID.Text;

            if (tmpVID.Length > 0 && tmpPID.Length > 0)
            {
                success = connectUps();
            }

            if (!success)
            {
                Tb_Settings_VID.Text = "0665";
                Tb_Settings_PID.Text = "5161";
                success = connectUps();
            }

            if (!success)
            {
                Tb_Settings_VID.Text = "06DA";
                Tb_Settings_PID.Text = "0003";
                success = connectUps();
            }

            if (!success)
            {
                Tb_Settings_VID.Text = "0F03";
                Tb_Settings_PID.Text = "0001";
                success = connectUps();
            }

            if (!success)
            {
                Tb_Settings_VID.Text = "05B8";
                Tb_Settings_PID.Text = "0000";
                success = connectUps();
            }

            if (!success)
            {
                Tb_Settings_VID.Text = tmpVID;
                Tb_Settings_PID.Text = tmpPID;
                System.Windows.MessageBox.Show("ИБП не найден!", "Провал", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                Settings.Default.vid = Tb_Settings_VID.Text;
                Settings.Default.pid = Tb_Settings_PID.Text;
                Settings.Default.Save();
                System.Windows.MessageBox.Show("ИБП найден и подключён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }


        }

        private void Tb_Settings_VID_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_VID.Text.Length == 0)
                Tb_Settings_VID.Text = "0";
        }

        private void Tb_Settings_PID_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_PID.Text.Length == 0)
                Tb_Settings_PID.Text = "0";
        }

        private void Tb_Settings_BatteryVoltage_Min_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_BatteryVoltage_Min.Text.Length == 0)
                Tb_Settings_BatteryVoltage_Min.Text = "0";
        }

        private void Tb_Settings_BatteryVoltage_Max_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_BatteryVoltage_Max.Text.Length == 0)
                Tb_Settings_BatteryVoltage_Max.Text = "0";
        }

        private void Tb_Settings_BatteryVoltage_MaxOnLoad_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_BatteryVoltage_MaxOnLoad.Text.Length == 0)
                Tb_Settings_BatteryVoltage_MaxOnLoad.Text = "0";
        }

        private void Tb_Settings_UpsVA_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_UpsVA.Text.Length == 0)
                Tb_Settings_UpsVA.Text = "0";
        }

        private void Tb_ShutdownVoltage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_ShutdownVoltage.Text.Length == 0)
                Tb_ShutdownVoltage.Text = "0";
        }

        private void Tb_Settings_UpsVA_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = checkTextBoxDoubleValue(sender, e);
        }

        private bool checkTextBoxDoubleValue(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            if (regex.IsMatch(e.Text) && !(e.Text == "." && ((System.Windows.Controls.TextBox)sender).Text.Contains(e.Text)))
                return false;

            else
                return true;
        }

        private void Tb_ShutdownVoltage_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = checkTextBoxDoubleValue(sender, e);
        }

        private void Tb_Settings_BatteryVoltage_Min_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = checkTextBoxDoubleValue(sender, e);
        }

        private void Tb_Settings_BatteryVoltage_Max_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = checkTextBoxDoubleValue(sender, e);
        }

        private void Tb_Settings_BatteryVoltage_MaxOnLoad_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = checkTextBoxDoubleValue(sender, e);
        }
    }
}
