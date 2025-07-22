using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using POS_UI.Models;
using System.Windows.Threading;
using POS_UI.Services;
using System.Security.Claims;
using POS_UI.Properties;

namespace POS_UI.ViewModels
{
    public class LoginViewModel : LoadingViewModelBase
    {
        private string _selectedUserType;
        private ObservableCollection<PinBoxViewModel> _pinBoxes;
        private ICommand _loginCommand;
        private ICommand _keypadCommand;
        private NavigationService _navigationService;
        private string _pin;
        private ICommand _deleteLastDigitCommand;
        private string _errorMessage;
        private bool _hasError;
        private ICommand _clearErrorCommand;
        private readonly TokenService _tokenService;
        private TokenModel _currentTokens;
        private ApiService _apiService;
        private UserModel _selectedUser;

        public string SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                if (_selectedUserType != value)
                {
                    _selectedUserType = value;
                    OnPropertyChanged();
                    UpdatePinBoxCount();
                }
            }
        }

        public ObservableCollection<PinBoxViewModel> PinBoxes
        {
            get => _pinBoxes;
            set
            {
                _pinBoxes = value;
                OnPropertyChanged();
            }
        }

        public string Pin
        {
            get => _pin;
            set { _pin = value; OnPropertyChanged(); }
        }

        public ObservableCollection<UserModel> Users { get; set; }

        public UserModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                    if (_selectedUser != null)
                    {
                        SelectedUserType = _selectedUser.Role;
                        UpdatePinBoxCount();
                    }
                }
            }
        }

        public ICommand LoginCommand => _loginCommand ??= new RelayCommand(ExecuteLogin);
        public ICommand KeypadCommand => _keypadCommand ??= new RelayCommand<string>(ExecuteKeypad);
        public ICommand DeleteLastDigitCommand => _deleteLastDigitCommand ??= new RelayCommand(DeleteLastDigit);
        public ICommand ClearErrorCommand => _clearErrorCommand ??= new RelayCommand(ClearError);

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(); }
        }

        public event Action LoginSucceeded;

        public LoginViewModel()
        {
            PinBoxes = new ObservableCollection<PinBoxViewModel>();
            //SelectedUserType = "Cashier"; // Default selection
            UpdatePinBoxCount();
            _tokenService = new TokenService();
            Users = new ObservableCollection<UserModel>();
            
            // Initialize API service with settings
            _apiService = new ApiService();
            FetchUsersFromApi();
        }

        public void SetNavigationService(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void RefreshApiService()
        {
            _apiService = new ApiService();
            FetchUsersFromApi();
        }

        private void UpdatePinBoxCount()
        {
            PinBoxes.Clear();
            int count = 4;
            if (SelectedUser != null)
            {
                if (SelectedUser.Role == "Outlet Admin")
                    count = 6;
                else if (SelectedUser.Role == "Cashier")
                    count = 4;
            }
            else if (SelectedUserType == "Admin" || SelectedUserType == "Outlet Admin")
            {
                count = 6;
            }
            for (int i = 0; i < count; i++)
            {
                var pinBox = new PinBoxViewModel();
                pinBox.PropertyChanged += PinBox_PropertyChanged;
                PinBoxes.Add(pinBox);
            }
            OnPropertyChanged(nameof(PinBoxes));
        }

        private void PinBox_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
            {
                if (PinBoxes.All(box => !string.IsNullOrEmpty(box.Text)))
                {
                    ExecuteLogin();
                }
            }
        }

        private async void ExecuteLogin()
        {
            string enteredPin = string.Join("", PinBoxes.Select(p => p.Text));
            var user = SelectedUser;
            if (user == null)
            {
                HasError = true;
                ErrorMessage = "Error: Please select a user";
                return;
            }
            try
            {
                var result = await _apiService.LoginAsync(user.Email, enteredPin);
                Properties.Settings.Default.AccessToken = result.accessToken;
                Properties.Settings.Default.RefreshToken = result.refreshToken;
                Properties.Settings.Default.AccessTokenExpiry = result.accessTokenExpiry;
                Properties.Settings.Default.RefreshTokenExpiry = result.refreshTokenExpiry;
                Properties.Settings.Default.Save();

                // Print the login API response to the console/terminal
                Console.WriteLine("Login API Response: SUCCESS");
                Console.WriteLine($"AccessToken: {result.accessToken}");
                Console.WriteLine($"RefreshToken: {result.refreshToken}");
                Console.WriteLine($"Token Expired: {_tokenService.IsTokenExpired(result.accessToken)}");

                _apiService.SetBearerToken(result.accessToken);

                HasError = false;
                ErrorMessage = "";
                // Raise event instead of navigating here
                LoginSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Login failed";
                Console.WriteLine("Login API Response: FAILED");
                Console.WriteLine($"Error: {ex.Message}");
                //MessageBox.Show($"Login failed: {ex.Message}", "Backend Error");
                ClearPinBoxes();
            }
        }

        private void TestTokenStorage()
        {
            try
            {
                // Verify tokens were stored correctly
                var storedAccessToken = Properties.Settings.Default.AccessToken;
                var storedRefreshToken = Properties.Settings.Default.RefreshToken;
                var storedAccessTokenExpiry = Properties.Settings.Default.AccessTokenExpiry;
                var storedRefreshTokenExpiry = Properties.Settings.Default.RefreshTokenExpiry;

                // Get time until expiration
                var timeUntilExpiry = _tokenService.GetTimeUntilExpiry(storedAccessToken);
                var isExpired = _tokenService.IsTokenExpired(storedAccessToken);

                // Create a message with token information
                var message = $"Token Storage Test Results:\n\n" +
                            $"Access Token: {(string.IsNullOrEmpty(storedAccessToken) ? "Not Stored" : "Stored Successfully")}\n" +
                            $"Refresh Token: {(string.IsNullOrEmpty(storedRefreshToken) ? "Not Stored" : "Stored Successfully")}\n" +
                            $"Access Token Expiry: {storedAccessTokenExpiry}\n" +
                            $"Refresh Token Expiry: {storedRefreshTokenExpiry}\n\n" +
                            $"Time Until Expiry: {timeUntilExpiry.TotalMinutes:F2} minutes\n" +
                            $"Is Token Expired: {isExpired}\n";

                MessageBox.Show(message, "Token Test Results", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing token storage: {ex.Message}", "Token Test Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteKeypad(string digit)
        {
            if (int.TryParse(digit, out _))
            {
                var emptyBox = PinBoxes.FirstOrDefault(box => string.IsNullOrEmpty(box.Text));
                if (emptyBox != null)
                {
                    emptyBox.Text = digit;
                }
            }
        }

        private void ClearPinBoxes()
        {
            foreach (var box in PinBoxes)
            {
                box.Text = string.Empty;
            }
        }

        private void DeleteLastDigit()
        {
            for (int i = PinBoxes.Count - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(PinBoxes[i].Text))
                {
                    PinBoxes[i].Text = string.Empty;
                    break;
                }
            }
        }

        private void ClearError()
        {
            ErrorMessage = "";
            HasError = false;
        }

        private async void FetchUsersFromApi()
        {
            await SetLoadingAsync(async () =>
            {
                try
                {
                    var usersFromApi = await _apiService.GetUsersAsync();
                    Users.Clear();
                    foreach (var user in usersFromApi)
                    {
                        Users.Add(user);
                    }
                    // Set the first user as default if available
                    if (Users.Count > 0)
                    {
                        SelectedUser = Users[0];
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to fetch users: {ex.Message}");
                }
            });
        }


    }

} 