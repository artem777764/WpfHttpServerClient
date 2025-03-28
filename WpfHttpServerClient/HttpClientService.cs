using System;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;

namespace WpfHttpServerClient;

public class HttpClientService
{
    private readonly HttpClient _client;

    public HttpClientService()
    {
        _client = new HttpClient();
    }

    public async Task<string> SendGetRequestAsync(HttpRequestMessage request)
    {
        try
        {
            HttpResponseMessage response = await _client.GetAsync(request.RequestUri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            return $"Ошибка HTTP: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }

    public async Task<string> SendPostRequestAsync(HttpRequestMessage request)
    {
        HttpResponseMessage response = await _client.PostAsync(request.RequestUri, request.Content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
