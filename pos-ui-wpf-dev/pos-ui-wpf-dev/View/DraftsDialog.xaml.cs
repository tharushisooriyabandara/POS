using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS_UI.View
{
    /// <summary>
    /// Interaction logic for DraftsDialog.xaml
    /// </summary>
    public partial class DraftsDialog : UserControl
    {
        public DraftsDialog()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border border && border.DataContext is POS_UI.Models.DraftOrderModel draft)
                {
                    var viewModel = DataContext as POS_UI.ViewModels.DraftsDialogViewModel;
                    viewModel?.LoadDraftCommand.Execute(draft);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading draft: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
