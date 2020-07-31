using MegatecUpsController.Properties;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MegatecUpsController
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        RegistryKey rkAppStartup = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public SettingsWindow()
        {
            InitializeComponent();

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

            if (UsbOps.usb.SpecifiedDevice == null)
            {
                connectUps();
            }
            this.DialogResult = true;
        }


        private void Tb_Settings_VID_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_VID.Text.Length == 0)
                Tb_Settings_VID.Text = "0";
        }

        private void Tb_Settings_VID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsDigit(e.Text, 0));
        }

        private void Tb_Settings_PID_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_PID.Text.Length == 0)
                Tb_Settings_PID.Text = "0";
        }

        private void Tb_Settings_PID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsDigit(e.Text, 0));
        }

        private void Tb_Settings_BatteryVoltage_Min_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_BatteryVoltage_Min.Text.Length == 0)
                Tb_Settings_BatteryVoltage_Min.Text = "0";
        }

        private void Tb_Settings_BatteryVoltage_Min_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = checkTextBoxDoubleValue(sender, e);
        }

        private void Tb_Settings_BatteryVoltage_Max_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_BatteryVoltage_Max.Text.Length == 0)
                Tb_Settings_BatteryVoltage_Max.Text = "0";
        }

        private void Tb_Settings_BatteryVoltage_Max_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = checkTextBoxDoubleValue(sender, e);
        }

        private void Tb_Settings_BatteryVoltage_MaxOnLoad_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_BatteryVoltage_MaxOnLoad.Text.Length == 0)
                Tb_Settings_BatteryVoltage_MaxOnLoad.Text = "0";
        }

        private void Tb_Settings_BatteryVoltage_MaxOnLoad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = checkTextBoxDoubleValue(sender, e);
        }

        private void Tb_Settings_UpsVA_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_Settings_UpsVA.Text.Length == 0)
                Tb_Settings_UpsVA.Text = "0";
        }

        private void Tb_Settings_UpsVA_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = checkTextBoxDoubleValue(sender, e);
        }

        private void Tb_ShutdownVoltage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_ShutdownVoltage.Text.Length == 0)
                Tb_ShutdownVoltage.Text = "0";
        }

        private void Tb_ShutdownVoltage_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = checkTextBoxDoubleValue(sender, e);
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

        private bool checkTextBoxDoubleValue(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            if (regex.IsMatch(e.Text) && !(e.Text == "." && ((System.Windows.Controls.TextBox)sender).Text.Contains(e.Text)))
                return false;

            else
                return true;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private bool connectUps()
        {
            return UsbOps.SetupUsbDevice(int.Parse(Settings.Default.vid, NumberStyles.AllowHexSpecifier), int.Parse(Settings.Default.pid, NumberStyles.AllowHexSpecifier));
        }
    }
}
