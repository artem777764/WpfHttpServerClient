using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;               // Для доступа к Application.Current.Dispatcher
using System.Windows.Threading;     // Для DispatcherTimer
using WpfHttpServerClient.DTOs;
using WpfHttpServerClient.Entities;
using WpfHttpServerClient.ViewModels;

namespace WpfHttpServerClient
{
    public class HttpServerService
    {
        private int processedRequestCount = 0;
        private readonly HttpListener _listener = new HttpListener();
        private readonly DateTime _startTime = DateTime.Now;

        // ViewModel для отображения статистики в UI
        public ServerViewModel ViewModel { get; set; }

        // Таймер для обновления времени работы
        private readonly DispatcherTimer _timer;

        public HttpServerService(int port)
        {
            _listener.Prefixes.Add($"http://localhost:{port}/");

            // Инициализируем таймер: обновляем каждую секунду
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                // Обновляем время работы сервера
                ViewModel.WorkTime = (DateTime.Now - _startTime).ToString(@"hh\:mm\:ss");
            }
        }

        public async Task StartAsync(CancellationToken token)
        {
            _listener.Start();
            while (!token.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync();
                ProcessRequest(context);
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string method = context.Request.HttpMethod.ToUpper();
            string path = context.Request.Url.AbsolutePath.ToLower();

            // Увеличиваем общий счётчик запросов
            Interlocked.Increment(ref processedRequestCount);

            // Обработка запросов по URL
            if (path == "/count" && method == "GET")
            {
                AddMethodViewModel(method);

                context.Response.StatusCode = 200;
                int answer = GetProcessedRequestCount();

                context.Response.ContentType = "text/plain";
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(answer);
                    writer.Flush();
                }

                context.Response.Close();
            }
            else if (path == "/time" && method == "GET")
            {
                AddMethodViewModel(method);

                context.Response.StatusCode = 200;
                string answer = GetWorkTime();

                context.Response.ContentType = "text/plain";
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(answer);
                    writer.Flush();
                }

                context.Response.Close();
            }
            else if (path == "/add" && method == "POST")
            {
                AddMethodViewModel(method);

                context.Response.StatusCode = 201;

                string json;
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    json = reader.ReadToEnd();
                }

                MessageDTO messageDTO = JsonSerializer.Deserialize<MessageDTO>(json);
                Message message = new()
                {
                    Id = ViewModel.Messages.Count == 0 ? 1 : ViewModel.Messages[ViewModel.Messages.Count - 1].Id + 1,
                    Text = messageDTO!.Text
                };
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(message.Id);
                    writer.Flush();
                }
                ViewModel.Messages.Add(message);

                context.Response.Close();
            }
            else
            {
                context.Response.StatusCode = 404;
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write("Not Found");
                    writer.Flush();
                }
                context.Response.Close();
            }
        }

        private void AddMethodViewModel(string method)
        {
            if (method == "GET")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ViewModel != null)
                        ViewModel.GetCount++;
                });
            }
            else if (method == "POST")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ViewModel != null)
                        ViewModel.PostCount++;
                });
            }
        }

        // Если ViewModel установлена, используем её для общего количества
        private int GetProcessedRequestCount()
        {
            return ViewModel != null ? ViewModel.TotalCount : processedRequestCount;
        }

        private string GetWorkTime()
        {
            return (DateTime.Now - _startTime).ToString(@"hh\:mm\:ss");
        }
    }
}
