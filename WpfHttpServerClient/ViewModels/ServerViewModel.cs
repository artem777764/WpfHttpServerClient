using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using WpfHttpServerClient.Entities;

namespace WpfHttpServerClient.ViewModels
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        public int Port { get; set; }

        private int _getCount;
        public int GetCount
        {
            get => _getCount;
            set
            {
                _getCount = value;
                OnPropertyChanged("GetCount");
                OnPropertyChanged("TotalCount");
                OnPropertyChanged("GetAverageTime");
                OnPropertyChanged("TotalAverageTime");
            }
        }

        private int _postCount;
        public int PostCount
        {
            get => _postCount;
            set
            {
                _postCount = value;
                OnPropertyChanged("PostCount");
                OnPropertyChanged("TotalCount");
                OnPropertyChanged("PostAverageTime");
                OnPropertyChanged("TotalAverageTime");
            }
        }

        public int TotalCount => GetCount + PostCount;

        private string _workTime = "00:00:00";
        public string WorkTime
        {
            get => _workTime;
            set { _workTime = value; OnPropertyChanged("WorkTime"); }
        }

        public ObservableCollection<Message> Messages { get; } = new();

        private double _getTotalTime;
        private double _postTotalTime;
        public double GetAverageTime => GetCount > 0 ? _getTotalTime / GetCount : 0;
        public double PostAverageTime => PostCount > 0 ? _postTotalTime / PostCount : 0;
        public double TotalAverageTime => TotalCount > 0 ? (_getTotalTime + _postTotalTime) / TotalCount : 0;

        public void AddRequestTime(string method, TimeSpan elapsed)
        {
            if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                _getTotalTime += elapsed.TotalMilliseconds;
                OnPropertyChanged("GetAverageTime");
            }
            else if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                _postTotalTime += elapsed.TotalMilliseconds;
                OnPropertyChanged("PostAverageTime");
            }
            OnPropertyChanged("TotalAverageTime");
        }

        // Новая коллекция логов
        public ObservableCollection<LoggedRequestResponse> Logs { get; } = new();

        // Коллекция отфильтрованных логов, к которой привязан DataGrid
        public ObservableCollection<LoggedRequestResponse> FilteredLogs { get; } = new();

        private string _selectedMethodFilter = "All";
        public string SelectedMethodFilter
        {
            get => _selectedMethodFilter;
            set { _selectedMethodFilter = value; OnPropertyChanged("SelectedMethodFilter"); FilterLogs(); }
        }

        private string _statusCodeFilter = "";
        public string StatusCodeFilter
        {
            get => _statusCodeFilter;
            set { _statusCodeFilter = value; OnPropertyChanged("StatusCodeFilter"); FilterLogs(); }
        }

        public ICommand SaveLogsCommand { get; }

        public ServerViewModel()
        {
            SaveLogsCommand = new RelayCommand(_ => SaveLogs());
        }

        private void SaveLogs()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = System.IO.Path.Combine(desktopPath, "logs.json");

                var json = System.Text.Json.JsonSerializer.Serialize(Logs);
                System.IO.File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {

            }
        }

        public void FilterLogs()
        {
            FilteredLogs.Clear();
            foreach (var log in Logs)
            {
                bool methodMatches = SelectedMethodFilter == "All" || 
                    log.Request.Method.Equals(SelectedMethodFilter, StringComparison.OrdinalIgnoreCase);
                bool statusMatches = true;
                if (!string.IsNullOrWhiteSpace(StatusCodeFilter) && int.TryParse(StatusCodeFilter, out int code))
                {
                    statusMatches = log.Response.StatusCode == code;
                }
                if (methodMatches && statusMatches)
                    FilteredLogs.Add(log);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
