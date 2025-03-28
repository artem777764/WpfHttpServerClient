using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfHttpServerClient.Entities;

public class LoggedRequestResponse
{
    public required LoggedRequest Request { get; set; }
    public required LoggedResponse Response { get; set; }
}
