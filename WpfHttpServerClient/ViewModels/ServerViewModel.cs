using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WpfHttpServerClient.Entities;

namespace WpfHttpServerClient.ViewModels
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        // Новое свойство для хранения номера порта
        private int _port;
        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged("Port"); }
        }

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

        // Суммарное время обработки в миллисекундах для GET и POST
        private double _getTotalTime;
        private double _postTotalTime;

        // Свойства для среднего времени (в миллисекундах)
        public double GetAverageTime => GetCount > 0 ? _getTotalTime / GetCount : 0;
        public double PostAverageTime => PostCount > 0 ? _postTotalTime / PostCount : 0;
        public double TotalAverageTime => TotalCount > 0 ? (_getTotalTime + _postTotalTime) / TotalCount : 0;

        // Метод для накопления времени обработки запроса
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
