using System.Windows;
using PCStats.UI.ViewModels;

namespace PCStats.UI.Views
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
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