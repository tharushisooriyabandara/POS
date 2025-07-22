using System;
using System.IO;
using System.Windows;

namespace POS_UI
{
    public partial class SettingsDialog : Window
    {
        private const string SettingsFileName = "settings.txt";
        private string SettingsFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), SettingsFileName);

        public string TenantCode { get; private set; }
        public string OutletCode { get; private set; }

        public SettingsDialog()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var lines = File.ReadAllLines(SettingsFilePath);
                    if (lines.Length >= 2)
                    {
                        TenantCodeTextBox.Text = lines[0].Replace("TenantCode=", "");
                        OutletCodeTextBox.Text = lines[1].Replace("OutletCode=", "");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TenantCode = TenantCodeTextBox.Text.Trim();
                OutletCode = OutletCodeTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(TenantCode))
                {
                    MessageBox.Show("Please enter a Tenant Code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                /*if (string.IsNullOrWhiteSpace(OutletCode))
                {
                    MessageBox.Show("Please enter an Outlet Code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }*/

                // Save to file
                var settingsContent = $"TenantCode={TenantCode}\nOutletCode={OutletCode}";
                File.WriteAllText(SettingsFilePath, settingsContent);

               // MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 