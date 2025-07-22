using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using POS_UI.ViewModels;

namespace POS_UI
{
    public partial class TablesPage : Page
    {
        public TablesPage()
        {
            InitializeComponent();
            this.DataContext = new TablesViewModel();
        }

        private void CashierButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("/View/CashierHomePage.xaml", UriKind.Relative));
        }
    }
} 