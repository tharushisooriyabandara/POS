using System.Collections.ObjectModel;
using System.Windows.Input;
using POS_UI.Models;
using MaterialDesignThemes.Wpf;

namespace POS_UI.ViewModels
{
    public class TableSelectionDialogViewModel : BaseViewModel
    {
        public ObservableCollection<TableModel> Tables { get; set; }
        private TableModel _selectedTable;
        public TableModel SelectedTable
        {
            get => _selectedTable;
            set { _selectedTable = value; OnPropertyChanged(); }
        }
        public ICommand SelectTableCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public TableSelectionDialogViewModel(ObservableCollection<TableModel> tables, TableModel selectedTable)
        {
            Tables = tables;
            SelectedTable = selectedTable;
            SelectTableCommand = new RelayCommand<TableModel>(SelectTable);
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }
        private void SelectTable(TableModel table)
        {
            foreach (var t in Tables) t.IsSelected = false;
            table.IsSelected = true;
            SelectedTable = table;
        }
        private void Save() { DialogHost.CloseDialogCommand.Execute(SelectedTable, null); }
        private void Cancel() { DialogHost.CloseDialogCommand.Execute(null, null); }
    }
} 