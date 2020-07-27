using System.Reflection;
using System.Windows;

namespace MegatecUpsController
{
    /// <summary>
    /// Логика взаимодействия для AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            var version = Assembly.GetEntryAssembly().GetName().Version;
            Tb_AboutInfo.Text += string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        private void Btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
