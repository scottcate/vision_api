using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using System.Threading.Tasks;
using TweetSharp;

namespace TwitterVision.Twitter
{
    public static class TwitterSearch
    {
        [FunctionName("TwitterSearch")]
        public static async Task Run([TimerTrigger("*/20 * * * * *")]TimerInfo myTimer, 
            [Queue("visionscanner")] IAsyncCollector<string> visionScannerQueue,
            TraceWriter log)
        {
            log.Info($"TwitterSearch function started");

            var searchResult = Helper.TwitterService().Search(new SearchOptions
            {
                Q = Helper.GetEnvironmentVariable("TwitterSearch"),
                Resulttype = TwitterSearchResultType.Recent,
                Count = int.Parse(Helper.GetEnvironmentVariable("TwitterSearchCount"))
            });

            log.Info($"TwitterSearch function found: {searchResult.Statuses.Count()}");
            
            foreach (var status in searchResult.Statuses)
            {
                await visionScannerQueue.AddAsync(status.IdStr);
            }
            log.Info($"TwitterSearch function ended");

        }
    }
}
