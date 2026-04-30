using System;
using PCStats.Core;
using PCStats.Overlay;
using PCStats.UI.Views;

namespace PCStats3
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var core = new CoreService();
            core.Start();

            // Оверлей запускается здесь, но благодаря IsVisible=false он будет ждать Alt+P
            var overlay = new OverlayClient();
            overlay.Start();

            var app = new System.Windows.Application();
            app.Run(new DashboardWindow());

            overlay.Stop();
            core.Stop();
        }
    }
}