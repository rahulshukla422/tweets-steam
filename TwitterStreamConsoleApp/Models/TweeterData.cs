using System;

namespace TwitterStreamConsoleApp.Models
{
    public class TweeterData
    {
        public string Tweet { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string URI { get; set; }
    }
}
