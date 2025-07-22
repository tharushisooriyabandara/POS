using System.Windows;
using System.Windows.Controls;

namespace POS_UI.View
{
    public partial class SetDiscountDialog : UserControl
    {
        public SetDiscountDialog()
        {
            InitializeComponent();
        }

        private void QuickDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                CustomDiscountTextBox.Text = tag;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var discountValue = CustomDiscountTextBox.Text?.Trim();
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(discountValue, null);
        }
    }
} 