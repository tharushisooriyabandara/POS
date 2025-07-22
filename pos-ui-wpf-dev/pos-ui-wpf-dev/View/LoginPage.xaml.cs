using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using POS_UI.ViewModels;
using System.Text.RegularExpressions;
using POS_UI.Services;
using System;
using System.Windows.Threading;

namespace POS_UI
{
    public partial class LoginPage : Page
    {
        private LoginViewModel _viewModel;
        private readonly TokenValidationService _tokenValidationService;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            _tokenValidationService = new TokenValidationService();
            DataContext = _viewModel;
            _viewModel.LoginSucceeded += OnLoginSucceeded;
            Loaded += LoginPage_Loaded;
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.SetNavigationService(NavigationService);
            CheckTenantAndOutletCode();
            CheckExistingSession();
        }

        private void CheckTenantAndOutletCode()
        {
            var settingsService = new POS_UI.Services.SettingsService();
            var (tenantCode, outletCode) = settingsService.LoadSettings();
            if (string.IsNullOrWhiteSpace(tenantCode) || string.IsNullOrWhiteSpace(outletCode))
            {
                MessageBox.Show(
                    "Tenant Code and Outlet Code are missing. Tap the ⚙️ Settings icon to configure them before proceeding.",
                    "Configuration Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void CheckExistingSession()
        {
            try
            {
                if (_tokenValidationService.IsTokenValid())
                {
                    var currentUser = _tokenValidationService.GetCurrentUser();
                    if (currentUser != null)
                    {
                        var role = currentUser.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                        if (!string.IsNullOrEmpty(role))
                        {
                            // Navigate to appropriate page based on role
                            if (role == "Admin")
                            {
                                NavigationService?.Navigate(new Uri("/View/AdminHomePage.xaml", UriKind.Relative));
                            }
                            else if (role == "Cashier")
                            {
                                NavigationService?.Navigate(new Uri("/View/CashierHomePage.xaml", UriKind.Relative));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking session: {ex.Message}", "Session Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ForgotPinCode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SupportDialogHost.IsOpen = true;
        }

        private void ContactSupport_Click(object sender, RoutedEventArgs e)
        {
            SupportDialogHost.IsOpen = false;
        }

        //Allow only numbers in the textbox
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void OnLoginSucceeded()
        {
            Dispatcher.Invoke(() =>
            {
                NavigationService?.Navigate(new Uri("/View/CashierHomePage.xaml", UriKind.Relative));
            });
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new SettingsDialog();
            settingsDialog.Owner = Window.GetWindow(this);
            
            if (settingsDialog.ShowDialog() == true)
            {
                // Refresh the view model to use new settings
                _viewModel.RefreshApiService();
            }
        }
    }
} 