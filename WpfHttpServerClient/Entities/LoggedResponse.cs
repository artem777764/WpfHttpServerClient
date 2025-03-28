using System.Net.Http;

namespace WpfHttpServerClient.Entities;

public class LoggedResponse
{
    public required int StatusCode {  get; set; }
    public string? Answer { get; set; }
}