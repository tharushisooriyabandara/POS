using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using POS_UI.Models;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Linq;
using System.Windows.Threading;

namespace POS_UI.ViewModels
{
    public class DraftsDialogViewModel
    {
        public ObservableCollection<DraftOrderModel> DraftOrders { get; }
        public ObservableCollection<DraftOrderModel> TicketsDrafts { get; }
        public ObservableCollection<DraftOrderModel> TablesDrafts { get; }
        public ICommand LoadDraftCommand { get; }
        public ICommand DeleteDraftCommand { get; }

        private DispatcherTimer _timer;

        public DraftsDialogViewModel(ObservableCollection<DraftOrderModel> draftOrders)
        {
            try
            {
                DraftOrders = draftOrders;
                // Add sample data for demonstration
                if (DraftOrders.Count == 0)
                {
                    for (int i = 0; i < 5; i++)
                        DraftOrders.Add(new DraftOrderModel { CustomerName = "Mike", Amount = 82.7m, CreatedAt = DateTime.Now.AddMinutes(-5), OrderType = "Take Away" });
                    for (int i = 0; i < 3; i++)
                        DraftOrders.Add(new DraftOrderModel { TableName = "Table 1", Amount = 82.7m, CreatedAt = DateTime.Now.AddMinutes(-5), OrderType = "Dine In" });
                }
                TicketsDrafts = new ObservableCollection<DraftOrderModel>(
                    draftOrders.Where(d => d.OrderType == "Take Away" || d.OrderType == "Delivery")
                );
                TablesDrafts = new ObservableCollection<DraftOrderModel>(
                    draftOrders.Where(d => d.OrderType == "Dine In")
                );
                
                LoadDraftCommand = new CashierRelayCommand<DraftOrderModel>(LoadDraft);
                DeleteDraftCommand = new CashierRelayCommand<DraftOrderModel>(DeleteDraft);

                // Start timer to update elapsed time
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMinutes(1);
                _timer.Tick += (s, e) =>
                {
                    foreach (var draft in DraftOrders)
                        draft.OnElapsedTimeChanged();
                };
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in DraftsDialogViewModel: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDraft(DraftOrderModel draft)
        {
            // Close the dialog and return the selected draft
            DialogHost.CloseDialogCommand.Execute(draft, null);
        }

        private void DeleteDraft(DraftOrderModel draft)
        {
            DraftOrders.Remove(draft);
            if (draft.OrderType == "Dine In")
            {
                TablesDrafts.Remove(draft);
            }
            else
            {
                TicketsDrafts.Remove(draft);
            }
        }
    }
} 