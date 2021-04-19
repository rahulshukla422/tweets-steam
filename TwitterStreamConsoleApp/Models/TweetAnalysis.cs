using System;
using System.Collections.Generic;

namespace TwitterStreamConsoleApp.Models
{
    public class TweetAnalysis
    {
        public DateTime CreatedDate { get; set; }

        public int TotalNoOfTweets { get; set; }

        public double AvgTweetsPerHour { get; set; }

        public double AvgTweetsPerMinute { get; set; }

        public double AvgTweetsPerSecond { get; set; }

        public List<string> TopEmojis { get; set; }

        public double PercentOfTweetsWithEmojis { get; set; }

        public List<string> TopHastags { get; set; }

        public double PercentOfTweetsWithUrl { get; set; }

        public double PercentOfTweetsWithPhotoUrl { get; set; }

        public List<string> TopDomains { get; set; }

    }
}
