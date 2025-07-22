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
using MaterialDesignThemes.Wpf;

namespace POS_UI.View
{
    /// <summary>
    /// Interaction logic for KitchenPage.xaml
    /// </summary>
    public partial class KitchenPage : Page
    {
        public KitchenPage()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void OrderItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is POS_UI.Models.OrderModel order)
            {
                try
                {
                    var dialog = new KitchenOrderDetailsDialog
                    {
                        DataContext = new POS_UI.ViewModels.KitchenOrderDetailsDialogViewModel(order)
                    };
                    await DialogHost.Show(dialog, "RootDialog");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error showing order details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
