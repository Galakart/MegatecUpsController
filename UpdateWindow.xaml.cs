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
                result = webClient.DownloadString("https://raw.githubusercontent.com/Galakart/MegatecUpsController/master/CHANGELOG.md");
            }
            result = Encoding.UTF8.GetString(Encoding.Default.GetBytes(result));

            Tb_Changelog.Text = result;
        }
    }
}
