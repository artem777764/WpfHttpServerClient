using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfHttpServerClient.Windows
{
    /// <summary>
    /// Логика взаимодействия для ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        private readonly HttpServerService _server;
        private CancellationTokenSource _cancellationTokenSource;

        public ServerWindow(int port)
        {
            InitializeComponent();

            TextBlock_Port.Text += port.ToString();

            _server = new HttpServerService(port);
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
