using System.Windows;
using POS_UI.ViewModels;
using POS_UI.Models;

namespace POS_UI.View
{
    public partial class PrinterSettingsDialog : Window
    {
        public PrinterSettingsDialogViewModel ViewModel { get; set; }

        public PrinterSettingsDialog(PrinterModel printer)
        {
            InitializeComponent();
            ViewModel = new PrinterSettingsDialogViewModel(printer);
            DataContext = ViewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedStatus != null)
            {
                ViewModel.Printer.IsActive = ViewModel.SelectedStatus.Name == "Active";
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select a status.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
} 