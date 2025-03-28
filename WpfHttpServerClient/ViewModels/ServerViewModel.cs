using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WpfHttpServerClient.Entities;

namespace WpfHttpServerClient.ViewModels
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        private int _getCount;
        public int GetCount
        {
            get => _getCount;
            set { _getCount = value; OnPropertyChanged("GetCount"); OnPropertyChanged("TotalCount"); }
        }

        private int _postCount;
        public int PostCount
        {
            get => _postCount;
            set { _postCount = value; OnPropertyChanged("PostCount"); OnPropertyChanged("TotalCount"); }
        }

        public int TotalCount => GetCount + PostCount;

        private string _workTime = "00:00:00";
        public string WorkTime
        {
            get => _workTime;
            set { _workTime = value; OnPropertyChanged("WorkTime"); }
        }

        public ObservableCollection<Message> Messages { get; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}