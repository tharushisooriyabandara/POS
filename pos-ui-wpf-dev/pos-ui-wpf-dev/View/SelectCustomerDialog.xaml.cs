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
using POS_UI.Models;
using System.Collections.ObjectModel;

namespace POS_UI.View
{
    /// <summary>
    /// Interaction logic for SelectCustomerDialog.xaml
    /// </summary>
    public partial class SelectCustomerDialog : UserControl
    {
        public SelectCustomerDialog()
        {
            InitializeComponent();
        }

        private void OnDialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter is CustomerModel newCustomer && newCustomer != null)
            {
                if (DataContext is POS_UI.ViewModels.SelectCustomerDialogViewModel vm)
                {
                    if (!vm.AllCustomers.Contains(newCustomer))
                        vm.AllCustomers.Add(newCustomer);
                    vm.FilteredCustomers = new ObservableCollection<CustomerModel>(vm.AllCustomers);
                    vm.SelectedCustomer = newCustomer;
                }
            }
        }
    }
}
