using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using WpfHttpServerClient.DTOs;
using WpfHttpServerClient.Entities;

namespace WpfHttpServerClient;

public class HttpServerService
{
    private int processedRequestCount = 0;
    private readonly HttpListener _listener = new HttpListener();
    private readonly DateTime _startTime = DateTime.Now;
    private List<Message> messages = new List<Message>();

    public HttpServerService(int port)
    {
        _listener.Prefixes.Add($"http://localhost:{port}/");
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
        Interlocked.Increment(ref processedRequestCount);

        string method = context.Request.HttpMethod.ToUpper();
        string path = context.Request.Url.AbsolutePath.ToLower();

        if (path == "/count" && method == "GET")
        {
            context.Response.StatusCode = 200;
            int answer = GetProsessedRequestCount();

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
            context.Response.StatusCode = 201;

            string json;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                json = reader.ReadToEnd();
            }

            MessageDTO messageDTO = JsonSerializer.Deserialize<MessageDTO>(json);
            Message message = new()
            {
                Id = messages.Count == 0 ? 1 : messages[messages.Count - 1].Id + 1,
                Text = messageDTO!.Text
            };
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(message.Id);
                writer.Flush();
            }

            messages.Add(message);
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

    private int GetProsessedRequestCount()
    {
        return processedRequestCount;
    }

    private string GetWorkTime()
    {
        return (DateTime.Now - _startTime).ToString();
    }
}
