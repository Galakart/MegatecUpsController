using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;

namespace MegatecUpsController
{
    /// <summary>
    /// Логика взаимодействия для UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            InitializeComponent();
            PutCurVersion();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            getLatestVersionAndChangelog();
        }

        private void Btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Galakart/MegatecUpsController/releases/latest");
            DialogResult = true;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void PutCurVersion()
        {
            var curVersion = Assembly.GetEntryAssembly().GetName().Version;
            Tb_CurVersion.Text = string.Format("v{0}.{1}.{2}", curVersion.Major, curVersion.Minor, curVersion.Build);
        }

        private void getLatestVersionAndChangelog()
        {
            var result = string.Empty;
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                try
                {
                    result = webClient.DownloadString("https://raw.githubusercontent.com/Galakart/MegatecUpsController/master/CHANGELOG");
                    string latestVersion = result.Substring(0, result.IndexOf("\n"));
                    Tb_LatestVersion.Text = latestVersion;
                    Tb_Changelog.Text = result;
                }
                catch
                {
                    Tb_Changelog.Text = "Не удалось загрузить список изменений";
                }                
            }
        }
        
    }
}
