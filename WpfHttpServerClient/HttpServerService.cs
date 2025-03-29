using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using WpfHttpServerClient.DTOs;
using WpfHttpServerClient.Entities;
using WpfHttpServerClient.ViewModels;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace WpfHttpServerClient
{
    public class HttpServerService
    {
        private int processedRequestCount = 0;
        private readonly HttpListener _listener = new HttpListener();
        private readonly DateTime _startTime = DateTime.Now;
        public ServerViewModel ViewModel { get; set; }
        private readonly DispatcherTimer _timer;

        public HttpServerService(int port)
        {
            _listener.Prefixes.Add($"http://localhost:{port}/");
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
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
            var sw = Stopwatch.StartNew();
            string method = context.Request.HttpMethod.ToUpper();
            string path = context.Request.Url.AbsolutePath.ToLower();
            string responseText = "";
            string? requestBody = null;
            if (method == "POST")
            {
                // Попробуем считать тело запроса, если оно ещё не было прочитано
                try
                {
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        requestBody = reader.ReadToEnd();
                    }
                }
                catch { }
            }
            AddMethodViewModel(method);
            try
            {
                if (path == "/count")
                {
                    if (method != "GET")
                    {
                        context.Response.StatusCode = 405;
                        responseText = "Method Not Allowed";
                        WriteResponse(context, responseText);
                        return;
                    }
                    context.Response.StatusCode = 200;
                    int answer = GetProcessedRequestCount();
                    responseText = answer.ToString();
                    context.Response.ContentType = "text/plain";
                    WriteResponse(context, responseText);
                }
                else if (path == "/time")
                {
                    if (method != "GET")
                    {
                        context.Response.StatusCode = 405;
                        responseText = "Method Not Allowed";
                        WriteResponse(context, responseText);
                        return;
                    }
                    context.Response.StatusCode = 200;
                    responseText = GetWorkTime();
                    context.Response.ContentType = "text/plain";
                    WriteResponse(context, responseText);
                }
                else if (path == "/add")
                {
                    if (method != "POST")
                    {
                        context.Response.StatusCode = 405;
                        responseText = "Method Not Allowed";
                        WriteResponse(context, responseText);
                        return;
                    }
                    context.Response.StatusCode = 201;
                    // При вызове /add тело уже прочитано выше
                    string json = requestBody ?? "";
                    MessageDTO messageDTO = JsonSerializer.Deserialize<MessageDTO>(json);
                    Message message = new Message
                    {
                        Id = ViewModel.Messages.Count == 0 ? 1 : ViewModel.Messages[ViewModel.Messages.Count - 1].Id + 1,
                        Text = messageDTO!.Text
                    };
                    responseText = message.Id.ToString();
                    context.Response.ContentType = "text/plain";
                    WriteResponse(context, responseText);
                    ViewModel.Messages.Add(message);
                }
                else
                {
                    context.Response.StatusCode = 404;
                    responseText = "Not Found";
                    WriteResponse(context, responseText);
                }
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                responseText = "Internal Server Error";
                WriteResponse(context, responseText);
            }
            finally
            {
                sw.Stop();
                var loggedRequest = new LoggedRequest
                {
                    URL = context.Request.Url.ToString(),
                    Method = method,
                    Headers = ConvertHeaders(context.Request.Headers),
                    Body = requestBody
                };
                var loggedResponse = new LoggedResponse
                {
                    StatusCode = context.Response.StatusCode,
                    Answer = responseText
                };
                var logEntry = new LoggedRequestResponse
                {
                    Request = loggedRequest,
                    Response = loggedResponse
                };
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ViewModel != null)
                    {
                        ViewModel.Logs.Add(logEntry);
                        ViewModel.FilterLogs();
                    }
                });
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ViewModel != null)
                        ViewModel.AddRequestTime(method, sw.Elapsed);
                });
                context.Response.Close();
            }
        }

        private Dictionary<string, string> ConvertHeaders(NameValueCollection headers)
        {
            var dict = new Dictionary<string, string>();
            foreach (string key in headers.AllKeys)
            {
                dict[key] = headers[key];
            }
            return dict;
        }

        private void WriteResponse(HttpListenerContext context, string responseText)
        {
            context.Response.ContentType = "text/plain";
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(responseText);
                writer.Flush();
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