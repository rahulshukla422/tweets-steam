using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using Tweetinvi;

namespace TwitterStreamConsoleApp
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IConfiguration configuration;
        private static TwitterClient client;

        static void Main(string[] args)
        {
            // set-up for log file
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true);
            configuration = builder.Build();

            client = new TwitterClient(
                    consumerKey: configuration.GetSection("consumerKey").Value,
                    consumerSecret: configuration.GetSection("consumerSecret").Value,
                    accessToken: configuration.GetSection("accessToken").Value,
                    accessSecret: configuration.GetSection("accessSecret").Value);
            client.Config.RateLimitTrackerMode = RateLimitTrackerMode.TrackOnly;

            var userName = configuration.GetSection("userName").Value;
            Console.WriteLine($"\tStarting Twitter Streams for @{userName}");
            Console.WriteLine($"\tfetching Twitter Streams, Please wait...");
            log.Info($"\tStarting Twitter Streams for @{userName}");
            log.Info($"\tfetching Twitter Streams, Please wait...");
            var twitterStream = new TwitterStream(client, configuration);

            twitterStream.GetUserTweets(userName).GetAwaiter().GetResult();
            Console.ReadLine();
        }
    }
}
