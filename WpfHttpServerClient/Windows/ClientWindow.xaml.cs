using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using WpfHttpServerClient.DTOs;
using WpfHttpServerClient.Entities;

namespace WpfHttpServerClient.Windows
{
    /// <summary>
    /// Логика взаимодействия для ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        HttpClientService _client;

        public ClientWindow()
        {
            InitializeComponent();

            _client = new HttpClientService();
        }

        private async void Button_SendRequest_Click(object sender, RoutedEventArgs e)
        {
            string url = TextBox_RequestURL.Text;
            if (string.IsNullOrWhiteSpace(url))
            {
                TextBlock_Response.Text = "Пустой URL";
                return;
            }

            string method = (ComboBox_Method.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(method))
            {
                TextBlock_Response.Text = "Тип метода не выбран";
                return;
            }

            if (method == "GET")
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                TextBlock_Response.Text = await _client.SendGetRequestAsync(request);
            }
            else if (method == "POST")
            {
                string jsonInput = TextBox_JSONInput.Text.Trim();

                if (string.IsNullOrEmpty(jsonInput))
                {
                    TextBlock_Response.Text = "Поле JSON пустое.";
                    return;
                }

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonInput, Encoding.UTF8, "application/json")
                };

                TextBlock_Response.Text = await _client.SendPostRequestAsync(request);
            }
            else return;
        }

        private void Button_ClearResponse_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_Response.Text = string.Empty;
        }
    }
}
