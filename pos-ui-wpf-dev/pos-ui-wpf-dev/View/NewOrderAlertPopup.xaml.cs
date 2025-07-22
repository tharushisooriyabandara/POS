using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS_UI.View
{
    /// <summary>
    /// Interaction logic for NewOrderAlertPopup.xaml
    /// </summary>
    public partial class NewOrderAlertPopup : UserControl
    {
        public NewOrderAlertPopup()
        {
            InitializeComponent();
        }

        public async Task ShowOrderDetailsDialog()
        {
            // Hide the alert popup
            var vm = this.DataContext as POS_UI.ViewModels.CashierHomeViewModel;
            if (vm != null)
                vm.IsOrderAlertVisible = false;

            // Create a sample order details view model
            var orderVm = POS_UI.ViewModels.OrderDetailsDialogViewModel.CreateSample();
            orderVm.DialogClosed += () => MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);

            var dialog = new POS_UI.View.OrderDetailsDialog { DataContext = orderVm };
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, "AddItemDialogHost");
        }

        private async void ViewOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            await ShowOrderDetailsDialog();
        }
    }
}
