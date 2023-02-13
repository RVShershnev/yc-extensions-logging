using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YandexCloud.Extensions.Logging.Demo
{
    internal static class AppLogEvents
    {
        internal static EventId Create = new(1000, "Created");
        internal static EventId Delete = new(1001, "Deleted");
        internal static EventId Update = new(1002, "Updated"); 
        internal static EventId Read = new(1003, "Read");

    }
}
