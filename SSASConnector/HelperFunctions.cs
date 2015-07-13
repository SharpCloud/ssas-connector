using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSASConnector
{
    public static class HelperFunctions
    {
        public static void WriteToEventLog(string Message)
        {
            if (!EventLog.SourceExists("SSASSharpCloudConnector"))
                EventLog.CreateEventSource("SSASSharpCloudConnector", "Application");

            EventLog.WriteEntry("SSASSharpCloudConnector", Message);
        }
    }
}
