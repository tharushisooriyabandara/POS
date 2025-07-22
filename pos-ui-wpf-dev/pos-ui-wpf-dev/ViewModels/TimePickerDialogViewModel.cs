using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace POS_UI.ViewModels
{
    public class TimePickerDialogViewModel : INotifyPropertyChanged
    {
        private DateTime _minTime;
        private DateTime _maxTime;
        private DateTime _selectedTime;
        private int _startHour;
        private int _startMinute;
        private int _endHour;
        private int _endMinute;
        public int SelectedHour
        {
            get => _selectedTime.Hour % 12 == 0 ? 12 : _selectedTime.Hour % 12;
            set
            {
                int hour = value % 12;
                if (SelectedPeriod == "PM") hour += 12;
                if (hour < _startHour) hour = _startHour;
                if (hour > _endHour) hour = _endHour;
                int minute = _selectedTime.Minute;
                if (hour == _startHour && minute < _startMinute) minute = _startMinute;
                if (hour == _endHour && minute > _endMinute) minute = _endMinute;
                _selectedTime = new DateTime(_selectedTime.Year, _selectedTime.Month, _selectedTime.Day, hour, minute, 0);
                NotifyAllTimeProperties();
            }
        }
        public int SelectedMinute
        {
            get => _selectedTime.Minute;
            set
            {
                int hour = _selectedTime.Hour;
                if (hour == _startHour && value < _startMinute) value = _startMinute;
                if (hour == _endHour && value > _endMinute) value = _endMinute;
                if (value < 0) value = 0;
                if (value > 59) value = 59;
                _selectedTime = new DateTime(_selectedTime.Year, _selectedTime.Month, _selectedTime.Day, hour, value, 0);
                NotifyAllTimeProperties();
            }
        }
        public string SelectedPeriod
        {
            get => _selectedTime.Hour >= 12 ? "PM" : "AM";
            set
            {
                if (value == "AM" && _selectedTime.Hour >= 12)
                    _selectedTime = _selectedTime.AddHours(-12);
                else if (value == "PM" && _selectedTime.Hour < 12)
                    _selectedTime = _selectedTime.AddHours(12);
                OnPropertyChanged();
            }
        }
        public int PreviousHour => (SelectedHour == 1 ? 12 : SelectedHour - 1);
        public int NextHour => (SelectedHour == 12 ? 1 : SelectedHour + 1);
        public int PreviousMinute => (SelectedMinute == 0 ? 59 : SelectedMinute - 1);
        public int NextMinute => (SelectedMinute == 59 ? 0 : SelectedMinute + 1);
        public string NextPeriod => SelectedPeriod == "AM" ? "PM" : "AM";
        public string PreviousPeriod => SelectedPeriod == "AM" ? "PM" : "AM";
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncrementHourCommand { get; }
        public ICommand DecrementHourCommand { get; }
        public ICommand IncrementMinuteCommand { get; }
        public ICommand DecrementMinuteCommand { get; }
        public ICommand TogglePeriodCommand { get; }
        public TimePickerDialogViewModel()
        {
            _minTime = DateTime.Now;
            _maxTime = DateTime.Now.AddHours(1);
            _selectedTime = DateTime.Now;
            _startHour = _minTime.Hour;
            _startMinute = _minTime.Minute;
            _endHour = _maxTime.Hour;
            _endMinute = _maxTime.Minute;
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            IncrementHourCommand = new RelayCommand(IncrementHour);
            DecrementHourCommand = new RelayCommand(DecrementHour);
            IncrementMinuteCommand = new RelayCommand(IncrementMinute);
            DecrementMinuteCommand = new RelayCommand(DecrementMinute);
            TogglePeriodCommand = new RelayCommand(TogglePeriod);
        }
        private void Save()
        {
            if (_selectedTime < _minTime)
            {
                System.Windows.MessageBox.Show($"Selected time is more than one hour before the current time. Please select a valid time.", "Invalid Time", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            if (_selectedTime > _maxTime)
            {
                System.Windows.MessageBox.Show($"Selected time is more than one hour after the current time. Please select a valid time.", "Invalid Time", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            DialogHost.CloseDialogCommand.Execute(_selectedTime, null);
        }
        private bool CanSave()
        {
            return _selectedTime >= _minTime && _selectedTime <= _maxTime;
        }
        private void Cancel()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
        private void IncrementHour()
        {
            int hour = _selectedTime.Hour;
            if (hour < _endHour)
            {
                hour++;
                int minute = hour == _endHour ? _endMinute : 0;
                _selectedTime = new DateTime(_selectedTime.Year, _selectedTime.Month, _selectedTime.Day, hour, minute, 0);
                NotifyAllTimeProperties();
            }
        }
        private void DecrementHour()
        {
            int hour = _selectedTime.Hour;
            if (hour > _startHour)
            {
                hour--;
                int minute = hour == _startHour ? _startMinute : 0;
                _selectedTime = new DateTime(_selectedTime.Year, _selectedTime.Month, _selectedTime.Day, hour, minute, 0);
                NotifyAllTimeProperties();
            }
        }
        private void IncrementMinute()
        {
            int hour = _selectedTime.Hour;
            int minute = _selectedTime.Minute;
            if (hour == _endHour && minute < _endMinute)
            {
                minute++;
            }
            else if (minute < 59 && (hour < _endHour || (hour == _endHour && minute < _endMinute)))
            {
                minute++;
            }
            else if (hour < _endHour)
            {
                hour++;
                minute = 0;
            }
            // Clamp to max time if over
            var newTime = new DateTime(_selectedTime.Year, _selectedTime.Month, _selectedTime.Day, hour, minute, 0);
            if (newTime > _maxTime) newTime = _maxTime;
            _selectedTime = newTime;
            NotifyAllTimeProperties();
        }
        private void DecrementMinute()
        {
            int hour = _selectedTime.Hour;
            int minute = _selectedTime.Minute;
            if (hour == _startHour)
            {
                if (minute > _startMinute)
                {
                    minute--;
                }
                // Do not allow below start minute in start hour
            }
            else if (minute > 0)
            {
                minute--;
            }
            else if (hour > _startHour)
            {
                hour--;
                minute = 59;
                if (hour == _startHour && minute < _startMinute) minute = _startMinute;
            }
            // Clamp to min time if under
            var newTime = new DateTime(_selectedTime.Year, _selectedTime.Month, _selectedTime.Day, hour, minute, 0);
            if (newTime < _minTime) newTime = _minTime;
            _selectedTime = newTime;
            NotifyAllTimeProperties();
        }
        private void TogglePeriod()
        {
            var newTime = _selectedTime.AddHours(_selectedTime.Hour >= 12 ? -12 : 12);
            if (newTime >= _minTime && newTime <= _maxTime)
            {
                _selectedTime = newTime;
                NotifyAllTimeProperties();
            }
        }
        private void NotifyAllTimeProperties()
        {
            OnPropertyChanged(nameof(SelectedHour));
            OnPropertyChanged(nameof(SelectedMinute));
            OnPropertyChanged(nameof(SelectedPeriod));
            OnPropertyChanged(nameof(PreviousHour));
            OnPropertyChanged(nameof(NextHour));
            OnPropertyChanged(nameof(PreviousMinute));
            OnPropertyChanged(nameof(NextMinute));
            OnPropertyChanged(nameof(NextPeriod));
            OnPropertyChanged(nameof(PreviousPeriod));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
} 