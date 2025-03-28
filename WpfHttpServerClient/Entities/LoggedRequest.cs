using System.Collections.Generic;
using System.Net.Http;

namespace WpfHttpServerClient.Entities;

public class LoggedRequest
{
    public required string URL {  get; set; }
    public required string Method { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
}
