using System.Configuration;
using System.Data;
using System.Windows;
using System.Collections.ObjectModel;
using POS_UI.Models;
using POS_UI.Services;

namespace POS_UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load printers ONCE at app startup
            PrintersService.Instance.GetConnectedPrinters();

            // Load card machines ONCE at app startup
            CardMachineService.Instance.Refresh();
        }
    }
}
