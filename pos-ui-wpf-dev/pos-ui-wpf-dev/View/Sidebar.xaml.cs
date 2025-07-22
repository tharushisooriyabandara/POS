using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using POS_UI.Services;

namespace POS_UI
{
    public partial class Sidebar : UserControl
    {
        private Frame _parentFrame;
        private readonly TokenValidationService _tokenValidationService;

        public Sidebar()
        {
            InitializeComponent();
            this.Loaded += Sidebar_Loaded;
            _tokenValidationService = new TokenValidationService();
        }

        private void Sidebar_Loaded(object sender, RoutedEventArgs e)
        {
            _parentFrame = Window.GetWindow(this)?.Content as Frame;
        }

        private void CashierButton_Click(object sender, RoutedEventArgs e)
        {
            _parentFrame?.Navigate(new Uri("/View/CashierHomePage.xaml", UriKind.Relative));
        }

        private void TablesButton_Click(object sender, RoutedEventArgs e)
        {
            _parentFrame?.Navigate(new Uri("/View/TablesPage.xaml", UriKind.Relative));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear all tokens
                _tokenValidationService.ClearTokens();
                
                // Navigate to login page
                if (_parentFrame != null)
                {
                    _parentFrame.Navigate(new Uri("/View/LoginPage.xaml", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show("Navigation service is not available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Logout Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void KitchenButton_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext;
            var prop = dc?.GetType().GetProperty("CurrentPage");
            if (prop != null && prop.CanWrite)
                prop.SetValue(dc, "Kitchen");
            _parentFrame?.Navigate(new Uri("/View/KitchenPage.xaml", UriKind.Relative));
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext;
            var prop = dc?.GetType().GetProperty("CurrentPage");
            if (prop != null && prop.CanWrite)
                prop.SetValue(dc, "Settings");
            _parentFrame?.Navigate(new Uri("/View/SettingsPage.xaml", UriKind.Relative));
        }

        private void InventoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext;
            var prop = dc?.GetType().GetProperty("CurrentPage");
            if (prop != null && prop.CanWrite)
                prop.SetValue(dc, "Inventory");
            _parentFrame?.Navigate(new Uri("/View/InventoryPage.xaml", UriKind.Relative));
        }
    }
} 