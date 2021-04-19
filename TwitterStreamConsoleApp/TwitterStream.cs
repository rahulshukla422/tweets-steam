using log4net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using TwitterStreamConsoleApp.Models;

namespace TwitterStreamConsoleApp
{
    public class TwitterStream
    {
        public readonly ITwitterClient client;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public readonly IConfiguration _configuration;
        public List<ITweet> tweetList = new List<ITweet>();

        public TwitterStream(ITwitterClient _client, IConfiguration configuration)
        {
            client = _client;
            _configuration = configuration;
        }

        public async Task GetUserTweets(string user)
        {
            var count = 1;
            var timesToLoop = int.Parse(_configuration.GetSection("inMinutes").Value);
            var userTimeline = client.Timelines.GetUserTimelineIterator(user);
            while (!userTimeline.Completed && count <= timesToLoop)
            {
                var page = await userTimeline.NextPageAsync();
                tweetList.AddRange(page);
                log.Info($"\tReceived Set {count} with Total rows: {page.Count()}");
                LogTweets(page.ToList());
                Console.WriteLine($"\tReceived Set {count} with Total rows: {page.Count()}");
                Console.WriteLine("\tWaiting for 1 min...");
                log.Info("\tWaiting for 1 min...");
                count++;
                Thread.Sleep(60 * 1000);
            }
            Console.WriteLine($"\tStopping Streaming of Data after: {timesToLoop} mins");
            log.Info($"\tStopping Streaming of Data after: {timesToLoop} mins");
            TranformData();
        }

        public void TranformData()
        {
            var tweetAnalysis = new TweetAnalysis
            {
                CreatedDate = DateTime.Now,
                TotalNoOfTweets = TotalTweetCount(),
                AvgTweetsPerHour = AverageTweetsPerHour(),
                AvgTweetsPerMinute = AverageTweetsPerMinute(),
                AvgTweetsPerSecond = AverageTweetsPerSecond(),
                TopEmojis = GetTopEmojis(),
                PercentOfTweetsWithEmojis = GetPercentOfEmojis(),
                TopHastags = TopHashTag(),
                PercentOfTweetsWithUrl = GetPercentageOfURL(),
                PercentOfTweetsWithPhotoUrl = GetPercentageOfPhotoURL(),
                TopDomains = TopDomainsOfURL()
            };
            Console.WriteLine("\tBelow is the Data analysis\n");
            log.Info("\tBelow is the Data analysis\n");
            log.Info(JsonConvert.SerializeObject(tweetAnalysis));
            Console.WriteLine($"\tTotal Tweets analysis for {_configuration.GetSection("inMinutes").Value} minutes is as below");
            Console.WriteLine($"\tTotal number of tweets received: {tweetAnalysis.TotalNoOfTweets}");
            Console.WriteLine($"\tAverage tweets per Hour: {tweetAnalysis.AvgTweetsPerHour}");
            Console.WriteLine($"\tAverage tweets per Minute: {tweetAnalysis.AvgTweetsPerMinute}");
            Console.WriteLine($"\tAverage tweets per Second: {tweetAnalysis.AvgTweetsPerSecond}");
            Console.WriteLine($"\tTop Emojis in tweets: {JsonConvert.SerializeObject(tweetAnalysis.TopEmojis)}");
            Console.WriteLine($"\tPercent of tweets that contains Emojis: {tweetAnalysis.PercentOfTweetsWithEmojis} %");
            Console.WriteLine($"\tTop Hashtags: {JsonConvert.SerializeObject(tweetAnalysis.TopHastags)}");
            Console.WriteLine($"\tPercent of tweets that contain a url: {tweetAnalysis.PercentOfTweetsWithUrl} %");
            Console.WriteLine($"\tPercent of tweets that contain a photo url: {tweetAnalysis.PercentOfTweetsWithPhotoUrl} %");
            Console.WriteLine($"\tTop domains of urls in tweets: {JsonConvert.SerializeObject(tweetAnalysis.TopDomains)}");
            log.Info("\n***********************Task Completed**************************");
            Console.WriteLine("\n***********************Task Completed**************************");
        }

        public void LogTweets(List<ITweet> tweets)
        {
            if (tweets.Any())
            {
                var data = tweets.Select(x => new TweeterData { Tweet = x.FullText, CreatedAt = x.CreatedAt, CreatedBy = x.CreatedBy.Name, URI = x.Url });
                log.Info(JsonConvert.SerializeObject(data));
            }
        }

        public int TotalTweetCount()
        {
            return tweetList.Count();
        }

        public double AverageTweetsPerSecond()
        {
            var totalSeconds = double.Parse(_configuration.GetSection("inMinutes").Value) * 60;
            return tweetList.Count() / totalSeconds;
        }

        public double AverageTweetsPerMinute()
        {
            var totalMinutes = double.Parse(_configuration.GetSection("inMinutes").Value);
            return tweetList.Count() / totalMinutes;
        }

        public double AverageTweetsPerHour()
        {
            var totalHours = double.Parse(_configuration.GetSection("inMinutes").Value) / 60;
            return tweetList.Count() / totalHours;
        }

        public List<string> GetTopEmojis()
        {
            var topEmojiList = new List<string>();

            foreach (var item in tweetList)
            {
                var em = Regex.Matches(item.FullText, @"(?:[\u2700-\u27bf]|(?:\ud83c[\udde6-\uddff]){2}|[\ud800-\udbff][\udc00-\udfff]|[\u0023-\u0039]\ufe0f?\u20e3|\u3299|\u3297|\u303d|\u3030|\u24c2|\ud83c[\udd70-\udd71]|\ud83c[\udd7e-\udd7f]|\ud83c\udd8e|\ud83c[\udd91-\udd9a]|\ud83c[\udde6-\uddff]|[\ud83c[\ude01-\ude02]|\ud83c\ude1a|\ud83c\ude2f|[\ud83c[\ude32-\ude3a]|[\ud83c[\ude50-\ude51]|\u203c|\u2049|[\u25aa-\u25ab]|\u25b6|\u25c0|[\u25fb-\u25fe]|\u00a9|\u00ae|\u2122|\u2139|\ud83c\udc04|[\u2600-\u26FF]|\u2b05|\u2b06|\u2b07|\u2b1b|\u2b1c|\u2b50|\u2b55|\u231a|\u231b|\u2328|\u23cf|[\u23e9-\u23f3]|[\u23f8-\u23fa]|\ud83c\udccf|\u2934|\u2935|[\u2190-\u21ff])");
                if (em.Count > 0)
                {
                    topEmojiList.AddRange(em.Select(x => x.Value));
                }
            }
            topEmojiList.RemoveAll(x => x == "["); // remove this char since it doesnt gets filtered in regex pattern
            topEmojiList.RemoveAll(x => x == "]"); // remove this char since it doesnt gets filtered in regex pattern

            var encodeList = new List<string>();
            foreach (var item in topEmojiList)
            {
                var str = item;
                //string res = BitConverter.ToString(Encoding.BigEndianUnicode.GetBytes(str)).Replace("-", "");
                string res = EncodeNonAsciiCharacters(str);
                encodeList.Add(res);
            }
            var data = encodeList.GroupBy(x => new { Name = x, Count = x.Count() }).OrderByDescending(x => x.Key.Count);
            var decodeList = new List<string>();
            foreach (var item in encodeList)
            {
                string res = DecodeEncodedNonAsciiCharacters(item);
                decodeList.Add(res);
            }
            return data.Select(x => x.Key.Name).Take(5).ToList();
        }

        static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m =>
                {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }

        public double GetPercentOfEmojis()
        {
            var topEmojiList = new List<string>();
            foreach (var item in tweetList)
            {
                var em = Regex.Matches(item.FullText, @"(?:[\u2700-\u27bf]|(?:\ud83c[\udde6-\uddff]){2}|[\ud800-\udbff][\udc00-\udfff]|[\u0023-\u0039]\ufe0f?\u20e3|\u3299|\u3297|\u303d|\u3030|\u24c2|\ud83c[\udd70-\udd71]|\ud83c[\udd7e-\udd7f]|\ud83c\udd8e|\ud83c[\udd91-\udd9a]|\ud83c[\udde6-\uddff]|[\ud83c[\ude01-\ude02]|\ud83c\ude1a|\ud83c\ude2f|[\ud83c[\ude32-\ude3a]|[\ud83c[\ude50-\ude51]|\u203c|\u2049|[\u25aa-\u25ab]|\u25b6|\u25c0|[\u25fb-\u25fe]|\u00a9|\u00ae|\u2122|\u2139|\ud83c\udc04|[\u2600-\u26FF]|\u2b05|\u2b06|\u2b07|\u2b1b|\u2b1c|\u2b50|\u2b55|\u231a|\u231b|\u2328|\u23cf|[\u23e9-\u23f3]|[\u23f8-\u23fa]|\ud83c\udccf|\u2934|\u2935|[\u2190-\u21ff])");
                if (em.Count > 0)
                {
                    topEmojiList.Add(item.Text);
                }
            }
            var count = topEmojiList.Count();
            return (count * 100) / tweetList.Count();
        }

        public List<string> TopHashTag()
        {
            var topHashTagList = new List<string>();

            foreach (var item in tweetList)
            {
                if (item.Hashtags.Count > 0)
                {
                    topHashTagList.AddRange(item.Hashtags.Select(x => x.Text));

                }
            }
            var data = topHashTagList.GroupBy(x => new { name = x, count = x.Count() }).OrderByDescending(x => x.Key.count).ToList();

            return data.Select(x => x.Key.name).ToList();
        }

        public double GetPercentageOfURL()
        {
            var urlList = new List<string>();
            foreach (var item in tweetList)
            {
                if (item.Urls.Count > 0)
                {
                    urlList.AddRange(item.Urls.Select(x => x.DisplayedURL).ToList());
                }
            }
            return (urlList.Count * 100) / tweetList.Count();
        }

        public double GetPercentageOfPhotoURL()
        {
            return (tweetList.Count(x => x.Media.Count(y => y.MediaType == "photo") > 0) * 100) / tweetList.Count();
        }

        public List<string> TopDomainsOfURL()
        {
            var topDomainURLList = new List<string>();
            var urlList = new List<string>();
            foreach (var item in tweetList)
            {
                if (item.Urls.Count > 0)
                {
                    topDomainURLList.AddRange(item.Urls.Select(x => x.DisplayedURL.Split('/')[0].ToString()));
                }
            }
            var topdomain = topDomainURLList.GroupBy(x => new { name = x, count = x.Count() }).OrderByDescending(x => x.Key.count);
            return topdomain.Select(x => x.Key.name).ToList();
        }
    }
}
