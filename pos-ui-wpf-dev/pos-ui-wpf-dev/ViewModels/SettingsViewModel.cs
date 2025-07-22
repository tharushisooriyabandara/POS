using POS_UI.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using POS_UI.Models;
using System.Windows;
using POS_UI.Services;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Printing;
using System.Drawing;
using System.Runtime.InteropServices;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Management;
using POS_UI.View;

namespace POS_UI.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly TokenValidationService _tokenValidationService;
        public ObservableCollection<PlatformModel> Platforms { get; set; }
        public ObservableCollection<PrinterModel> Printers => PrintersService.Instance.Printers;
        public ObservableCollection<CardMachineModel> CardMachines => CardMachineService.Instance.CardMachines;
        public ICommand SwitchTabCommand { get; }
        private string _selectedTab;
        public string SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(); }
        }
        public UserModel LoggedInUser { get; set; }
        public string ShiftTimer { get; set; }
        public string OutletName { get; set; }
        public ICommand RequestPinChangeCommand { get; }
        private bool _isPinDialogOpen;
        public bool IsPinDialogOpen
        {
            get => _isPinDialogOpen;
            set { _isPinDialogOpen = value; OnPropertyChanged(); }
        }
        private bool _isPinChangeRequestedDialogOpen;
        public bool IsPinChangeRequestedDialogOpen
        {
            get => _isPinChangeRequestedDialogOpen;
            set { _isPinChangeRequestedDialogOpen = value; OnPropertyChanged(); }
        }
        public string NewPin { get; set; }
        public string ConfirmPin { get; set; }
        public ICommand SetNewPinCommand { get; }
        public bool IsAdmin => LoggedInUser != null &&
            !string.IsNullOrEmpty(LoggedInUser.Role) &&
            (LoggedInUser.Role.Replace(" ", "", System.StringComparison.OrdinalIgnoreCase)
                .Equals("OutletAdmin", System.StringComparison.OrdinalIgnoreCase));
        public ObservableCollection<SecuritySettingModel> SecuritySettings { get; set; }
        public SettingsViewModel()
        {
            try
            {
                _tokenValidationService = new TokenValidationService();
                Platforms = new ObservableCollection<PlatformModel>
                {
                    new PlatformModel { PlatformName = "Uber Eats", Branch = "Subway Galle", IsActive = true },
                    new PlatformModel { PlatformName = "Uber Eats", Branch = "Subway Matara", IsActive = false }
                };
                // CardMachines is now managed by CardMachineService
                SelectedTab = "Platform Settings";
                SwitchTabCommand = new RelayCommand<string>(tab => SelectedTab = tab);

                // Get logged in user information from token
                var currentUser = _tokenValidationService.GetCurrentUser();
                string userId = null;
                if (currentUser != null)
                {
                    // Show claims for debugging
                    var claims = string.Join("\n", currentUser.Claims.Select(c => $"{c.Type}: {c.Value}"));
                    //System.Windows.MessageBox.Show("Claims:\n" + claims);
                    userId = currentUser.FindFirst("sub")?.Value;
                    //System.Windows.MessageBox.Show($"userId from token: {userId}");
                }
                else
                {
                    System.Windows.MessageBox.Show("No current user found in token.");
                }
                FetchLoggedInUserFromApi(userId);
                ShiftTimer = "2h:48m:72s";
                OutletName = "SubWay Galle - POS";
                RequestPinChangeCommand = new RelayCommand(RequestPinChange);
                SetNewPinCommand = new RelayCommand(SetNewPin);
            } catch (Exception ex) { System.Windows.MessageBox.Show("Exception: " + ex.Message); }
        }

        private async void FetchLoggedInUserFromApi(string userId)
        {
            //System.Windows.MessageBox.Show("FetchLoggedInUserFromApi called");
            if (string.IsNullOrEmpty(userId))
                {
                LoggedInUser = new UserModel { FirstName = "Unknown", LastName = "User", Role = "Cashier" };
                SecuritySettings = new ObservableCollection<SecuritySettingModel>();
                OnPropertyChanged(nameof(LoggedInUser));
                OnPropertyChanged(nameof(IsAdmin));
                OnPropertyChanged(nameof(SecuritySettings));
                return;
            }
            try
            {
                var apiService = new ApiService();
                var user = await apiService.GetUserByIdAsync(userId);
                //System.Windows.MessageBox.Show(JsonConvert.SerializeObject(user));
                if (user != null)
                {
                    //System.Windows.MessageBox.Show($"API returned user: {user.FirstName} {user.LastName}");
                    LoggedInUser = user;
                }
                else
                {
                    //System.Windows.MessageBox.Show("API returned null user.");
                    LoggedInUser = new UserModel { FirstName = "gggg", LastName = "", Role = "Cashier" };
                }
                //System.Windows.MessageBox.Show($"Role: {LoggedInUser.Role}");

            if (IsAdmin)
            {
                SecuritySettings = new ObservableCollection<SecuritySettingModel>
                {
                    new SecuritySettingModel { UserName = "Cash 1", Status = "Checked IN", LastActive = DateTime.Now, IsActive = true, PinChangeRequested = true },
                    new SecuritySettingModel { UserName = "Cash 2", Status = "Checked OUT", LastActive = DateTime.Now.AddMinutes(-30), IsActive = false, PinChangeRequested = false }
                };
            }
            else
            {
                SecuritySettings = new ObservableCollection<SecuritySettingModel>();
            }
                OnPropertyChanged(nameof(LoggedInUser));
                OnPropertyChanged(nameof(IsAdmin));
                OnPropertyChanged(nameof(SecuritySettings));
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("Exception in FetchLoggedInUserFromApi: " + ex.Message);
                LoggedInUser = new UserModel { FirstName = "ffffff", LastName = "", Role = "Cashier" };
                SecuritySettings = new ObservableCollection<SecuritySettingModel>();
                OnPropertyChanged(nameof(LoggedInUser));
                OnPropertyChanged(nameof(IsAdmin));
                OnPropertyChanged(nameof(SecuritySettings));
            }
        }

        private void RequestPinChange()
        {
            IsPinDialogOpen = true;
        }
        private void SetNewPin()
        {
            if (!string.IsNullOrEmpty(NewPin) && NewPin == ConfirmPin)
            {
                IsPinDialogOpen = false;
                IsPinChangeRequestedDialogOpen = true;
            }
            else
            {
                MessageBox.Show("PINs do not match or are empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private RelayCommand addPrinterCommand;
        public ICommand AddPrinterCommand => addPrinterCommand ??= new RelayCommand(AddPrinter);

        private RelayCommand getConnectedPrinterCommand;
        public ICommand GetConnectedPrinterCommand => getConnectedPrinterCommand ??= new RelayCommand(RefreshPrinters);

        private RelayCommand<string> testPrintCommand;
        public ICommand TestPrintCommand => testPrintCommand ??= new RelayCommand<string>(TestPrint);

        private RelayCommand<PrinterModel> openPrinterSettingsCommand;
        public ICommand OpenPrinterSettingsCommand => openPrinterSettingsCommand ??= new RelayCommand<PrinterModel>(OpenPrinterSettings);

        private void RefreshPrinters()
        {
            PrintersService.Instance.GetConnectedPrinters();
        }

        private void AddPrinter()
        {
            var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            var posPrinterName = printers.FirstOrDefault(p => p.Contains("POS", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(posPrinterName))
            {
                MessageBox.Show("Printer named 'POS' not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool openDrawer = true;
            // ✅ Open cash drawer
            if (openDrawer)
            {
                byte[] openDrawerCommand = new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA };
                RawPrinterHelper.SendBytesToPrinter(posPrinterName, openDrawerCommand);
            }
            

            // ✅ Print test page
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrinterSettings.PrinterName = posPrinterName;

            printDoc.PrintPage += (s, e) =>
            {
                string testText = "=== TEST PRINT ===\nPrinter: POS\nTime: " + DateTime.Now;
                Font printFont = new Font("Consolas", 12);
                e.Graphics.DrawString(testText, printFont, Brushes.Black, new PointF(10, 10));
            };

            try
            {
                printDoc.Print();
                MessageBox.Show("Test print and cash drawer triggered.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestPrint(string printerName)
        {
            try
            {
                if (string.IsNullOrEmpty(printerName))
                {
                    MessageBox.Show("Printer name is required for test printing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create a simple test print content
                string testContent = $@"
==========================================
            TEST PRINT
==========================================
Printer: {printerName}
Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
==========================================
This is a test print to verify that the
printer '{printerName}' is working correctly.
==========================================
";

                // Convert the test content to bytes (assuming ESC/POS format)
                byte[] printData = System.Text.Encoding.ASCII.GetBytes(testContent);

                // Add cut command (ESC/POS cut command: GS V m)
                // 0x1D = GS (Group Separator)
                // 0x56 = V (Cut command)
                // 0x00 = Full cut (0x00 = full cut, 0x01 = partial cut)
                byte[] cutCommand = new byte[] { 0x1D, 0x56, 0x00 };

                // Combine print data with cut command
                byte[] combinedData = new byte[printData.Length + cutCommand.Length];
                Array.Copy(printData, 0, combinedData, 0, printData.Length);
                Array.Copy(cutCommand, 0, combinedData, printData.Length, cutCommand.Length);

                // Send to printer using the existing RawPrinterHelper
                bool success = RawPrinterHelper.SendBytesToPrinter(printerName, combinedData);

                if (success)
                {
                    MessageBox.Show($"Test print sent successfully to {printerName} with paper cut", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to send test print to {printerName}. Please check if the printer is connected and accessible.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during test print: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenPrinterSettings(PrinterModel printer)
        {
            try
            {
                var dialog = new View.PrinterSettingsDialog(printer);
                dialog.Owner = Application.Current.MainWindow;
                
                bool? result = dialog.ShowDialog();
                
                if (result == true)
                {
                    // The printer status has been updated in the dialog
                    // Trigger property change notification to update the UI
                    OnPropertyChanged(nameof(Printers));
                    
                    MessageBox.Show($"Printer '{printer.DeviceName}' status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening printer settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private RelayCommand addCardMachineCommand;
        public ICommand AddCardMachineCommand => addCardMachineCommand ??= new RelayCommand(AddCardMachine);

        private RelayCommand<CardMachineModel> editCardMachineCommand;
        public ICommand EditCardMachineCommand => editCardMachineCommand ??= new RelayCommand<CardMachineModel>(EditCardMachine);

        private RelayCommand<CardMachineModel> deleteCardMachineCommand;
        public ICommand DeleteCardMachineCommand => deleteCardMachineCommand ??= new RelayCommand<CardMachineModel>(DeleteCardMachine);

        private RelayCommand<CardMachineModel> activateCardMachineCommand;
        public ICommand ActivateCardMachineCommand => activateCardMachineCommand ??= new RelayCommand<CardMachineModel>(ActivateCardMachine);

        private RelayCommand<CardMachineModel> deactivateCardMachineCommand;
        public ICommand DeactivateCardMachineCommand => deactivateCardMachineCommand ??= new RelayCommand<CardMachineModel>(DeactivateCardMachine);

        private RelayCommand<CardMachineModel> pairCardMachineCommand;
        public ICommand PairCardMachineCommand => pairCardMachineCommand ??= new RelayCommand<CardMachineModel>(PairCardMachine);

        private RelayCommand<CardMachineModel> manageCardMachineUsersCommand;
        public ICommand ManageCardMachineUsersCommand => manageCardMachineUsersCommand ??= new RelayCommand<CardMachineModel>(ManageCardMachineUsers);

        private void AddCardMachine()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AddCardMachine method called");
                var dialog = new View.AddCardMachineDialog();
                dialog.Owner = Application.Current.MainWindow;
                
                System.Diagnostics.Debug.WriteLine("Showing dialog...");
                bool? result = dialog.ShowDialog();
                System.Diagnostics.Debug.WriteLine($"Dialog result: {result}");
                
                if (result == true)
                {
                    var newCardMachine = dialog.GetCardMachine();
                    CardMachineService.Instance.AddCardMachine(newCardMachine);
                    OnPropertyChanged(nameof(CardMachines));
                    
                    MessageBox.Show($"New card machine '{newCardMachine.DeviceName}' added and paired successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in AddCardMachine: {ex.Message}");
                MessageBox.Show($"Error adding card machine: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditCardMachine(CardMachineModel cardMachine)
        {
            if (cardMachine == null) return;

            try
            {
                var dialog = new View.AddCardMachineDialog(cardMachine);
                dialog.Owner = Application.Current.MainWindow;
                
                bool? result = dialog.ShowDialog();
                
                if (result == true)
                {
                    var updatedCardMachine = dialog.GetCardMachine();
                    
                    // Update the existing card machine with new values
                    cardMachine.DeviceName = updatedCardMachine.DeviceName;
                    cardMachine.DeviceType = updatedCardMachine.DeviceType;
                    cardMachine.IPAddress = updatedCardMachine.IPAddress;
                    cardMachine.Port = updatedCardMachine.Port;
                    cardMachine.DeviceId = updatedCardMachine.DeviceId;
                    cardMachine.ParingCode = updatedCardMachine.ParingCode;
                    cardMachine.ParingCodeTime = updatedCardMachine.ParingCodeTime;
                    cardMachine.IsActive = updatedCardMachine.IsActive;
                    cardMachine.APIEndpoint = updatedCardMachine.APIEndpoint;
                    cardMachine.AuthToken = updatedCardMachine.AuthToken;
                    
                    // Save to file using service
                    CardMachineService.Instance.UpdateCardMachine(cardMachine);
                    OnPropertyChanged(nameof(CardMachines));
                    
                    MessageBox.Show($"Card machine '{cardMachine.DeviceName}' updated and paired successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing card machine: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCardMachine(CardMachineModel cardMachine)
        {
            if (cardMachine == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{cardMachine.DeviceName}'?", 
                    "Confirm Delete", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CardMachineService.Instance.DeleteCardMachine(cardMachine);
                    OnPropertyChanged(nameof(CardMachines));
                    MessageBox.Show($"Card machine '{cardMachine.DeviceName}' deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting card machine: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActivateCardMachine(CardMachineModel cardMachine)
        {
            if (cardMachine == null) return;

            try
            {
                cardMachine.IsActive = true;
                CardMachineService.Instance.UpdateCardMachine(cardMachine);
                OnPropertyChanged(nameof(CardMachines));
                
                MessageBox.Show($"Card machine '{cardMachine.DeviceName}' activated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error activating card machine: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeactivateCardMachine(CardMachineModel cardMachine)
        {
            if (cardMachine == null) return;

            try
            {
                cardMachine.IsActive = false;
                CardMachineService.Instance.UpdateCardMachine(cardMachine);
                OnPropertyChanged(nameof(CardMachines));
                
                MessageBox.Show($"Card machine '{cardMachine.DeviceName}' deactivated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deactivating card machine: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PairCardMachine(CardMachineModel cardMachine)
        {
            if (cardMachine == null) return;

            try
            {
                var dialog = new View.PairCardMachineDialog(cardMachine);
                dialog.Owner = Application.Current.MainWindow;
                
                bool? result = dialog.ShowDialog();
                
                if (result == true && dialog.PairingSuccessful)
                {
                    // Update the card machine with new auth token and pairing info
                    cardMachine.AuthToken = dialog.AuthToken;
                    cardMachine.ParingCode = int.TryParse(dialog.PairingCode, out int code) ? code : 0;
                    cardMachine.ParingCodeTime = DateTime.Now;
                    
                    CardMachineService.Instance.UpdateCardMachine(cardMachine);
                    OnPropertyChanged(nameof(CardMachines));
                    
                    MessageBox.Show($"Card machine '{cardMachine.DeviceName}' paired successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pairing card machine: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageCardMachineUsers(CardMachineModel cardMachine)
        {
            if (cardMachine == null) return;

            try
            {
                var viewModel = new CardMachineUsersDialogViewModel(cardMachine);
                var dialog = new CardMachineUsersDialog(viewModel);
                
                dialog.Owner = Application.Current.MainWindow;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error managing card machine users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

public static class RawPrinterHelper
    {
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool StartDocPrinter(IntPtr hPrinter, int level, IntPtr pDocInfo);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
                IntPtr printerHandle;

                if (!OpenPrinter(printerName.Normalize(), out printerHandle, IntPtr.Zero))
                    return false;

                var docInfo = new DOCINFOA
                {
                    pDocName = "Raw Document",
                    pDataType = "RAW"
                };

                IntPtr pDocInfo = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DOCINFOA)));
                Marshal.StructureToPtr(docInfo, pDocInfo, false);

                bool success = false;

                if (StartDocPrinter(printerHandle, 1, pDocInfo) && StartPagePrinter(printerHandle))
                {
                    IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                    Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);
                    success = WritePrinter(printerHandle, pUnmanagedBytes, bytes.Length, out _);
                    EndPagePrinter(printerHandle);
                    Marshal.FreeCoTaskMem(pUnmanagedBytes);
                }

                EndDocPrinter(printerHandle);
                ClosePrinter(printerHandle);
                Marshal.FreeHGlobal(pDocInfo); // ❗ very important to clean up

                return success;
        }

            [StructLayout(LayoutKind.Sequential)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }
    }

}
} 