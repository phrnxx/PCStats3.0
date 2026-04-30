using System.Windows;
using System.Windows.Input;
using PCStats.UI.ViewModels;

namespace PCStats.UI.Views
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnChangeBg_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
            {
                vm.NextBackground();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void BtnGithub_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/phrnxx/PCStats3.0");
        }
        protected override void OnClosed(System.EventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
            {
                vm.Stop();
            }
            base.OnClosed(e);
        }
    }
}