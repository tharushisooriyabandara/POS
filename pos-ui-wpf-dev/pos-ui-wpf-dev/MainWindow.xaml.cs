using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using POS_UI.Services;
using System.Security.Claims;

namespace POS_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TokenValidationService _tokenValidationService;

        public MainWindow()
        {
            try
            {
                //System.Windows.MessageBox.Show("MainWindow constructor starting...");
                InitializeComponent();
                //System.Windows.MessageBox.Show("InitializeComponent completed");
                _tokenValidationService = new TokenValidationService();
                //System.Windows.MessageBox.Show("TokenValidationService created");
                RestoreLastPage();
                //System.Windows.MessageBox.Show("RestoreLastPage completed");
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show("MainWindow constructor error: " + ex.Message + "\n\nStackTrace: " + ex.StackTrace);
                throw; // Re-throw to be caught by the App.xaml.cs
            }
        }

        private void RestoreLastPage()
        {
            try
            {
                //System.Windows.MessageBox.Show("RestoreLastPage starting...");
                var tokenService = new TokenValidationService();
                //System.Windows.MessageBox.Show("TokenService created in RestoreLastPage");
                
                if (tokenService.IsTokenValid())
                {
                    //System.Windows.MessageBox.Show("Token is valid, navigating to CashierHomePage...");
                    // User is still logged in (token valid or refreshed)
                    MainFrame.Navigate(new Uri("/View/CashierHomePage.xaml", UriKind.Relative));
                   // System.Windows.MessageBox.Show("Navigation to CashierHomePage completed");
                }
                else
                {
                    //System.Windows.MessageBox.Show("Token is invalid, navigating to LoginPage...");
                    // Tokens invalid/expired, prompt login
                    MainFrame.Navigate(new LoginPage());
                    //System.Windows.MessageBox.Show("Navigation to LoginPage completed");
                }
            }
            catch (Exception ex)
            {
               // System.Windows.MessageBox.Show("RestoreLastPage error: " + ex.Message + "\n\nStackTrace: " + ex.StackTrace);
                throw; // Re-throw to be caught by the constructor
            }
        }

        public void NavigateToCashierWithOrder(POS_UI.Models.OrderModel order)
        {
            var cashierPage = new POS_UI.CashierHomePage();
            var cashierVm = cashierPage.DataContext as POS_UI.ViewModels.CashierHomeViewModel;
            if (cashierVm != null)
            {
                cashierVm.LoadOrder(order);
            }
            this.MainFrame.Navigate(cashierPage);
        }
    }
}