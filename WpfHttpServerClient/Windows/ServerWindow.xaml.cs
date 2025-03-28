using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfHttpServerClient.ViewModels;

namespace WpfHttpServerClient.Windows
{
    public partial class ServerWindow : Window
    {
        private readonly HttpServerService _server;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ServerViewModel _viewModel;

        public ServerWindow(int port)
        {
            InitializeComponent();

            // Создаем и устанавливаем ViewModel как DataContext
            _viewModel = new ServerViewModel
            {
                Port = port // Передаем порт в модель
            };
            DataContext = _viewModel;

            // Создаем сервер и передаем ему ViewModel для обновления статистики
            _server = new HttpServerService(port)
            {
                ViewModel = _viewModel
            };

            _cancellationTokenSource = new CancellationTokenSource();

            StartServer();
        }

        private async void StartServer()
        {
            try
            {
                await _server.StartAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске сервера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopServer()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopServer();
        }
    }
}
