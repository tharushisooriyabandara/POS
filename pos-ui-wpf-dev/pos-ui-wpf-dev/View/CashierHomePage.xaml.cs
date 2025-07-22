using System;
using System.Windows;
using System.Windows.Controls;
using POS_UI.ViewModels;
using POS_UI.Services;

namespace POS_UI
{
    public partial class CashierHomePage : Page
    {
        private readonly TokenValidationService _tokenValidationService;

        public CashierHomePage()
        {
            InitializeComponent();
            this.DataContext = new CashierHomeViewModel();
            _tokenValidationService = new TokenValidationService();
            Loaded += CashierHomePage_Loaded;
        }

        private void CashierHomePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_tokenValidationService.IsTokenValid())
            {
                MessageBox.Show("Your session has expired. Please login again.", "Session Expired", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigateToLogin();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear all tokens
                _tokenValidationService.ClearTokens();
                
                // Navigate to login page
                NavigateToLogin();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Logout Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToLogin()
        {
            try
            {
                if (NavigationService != null)
                {
                    NavigationService.Navigate(new Uri("/View/LoginPage.xaml", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show("Navigation service is not available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Navigation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TablesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("/View/TablesPage.xaml", UriKind.Relative));
        }

        private void Button_Click()
        {

        }
    }
} 