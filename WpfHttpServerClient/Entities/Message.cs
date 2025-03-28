using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfHttpServerClient.Entities;

public class Message
{
    public required int Id { get; set; }
    public required string Text { get; set; }
}
