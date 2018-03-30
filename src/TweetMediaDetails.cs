using System;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using TwitterVision.Models;

namespace TwitterVision.Twitter
{
    public static class TweetMediaDetails
    {
        const string ErrorIdNotFound = "{\"Error\":\"No Tweet ID found, Expected Querystring ?Id= \"}";
        const string ErrorTweetNotFound = "{\"Error\":\"Tweet not found\"}";

        [FunctionName("TweetMediaDetails")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TweetMediaDetails/{tweetId}")]
            [Table("visiontweet")] CloudTable visionTweetStorageTable,
            HttpRequestMessage req,
            string tweetId,
            TraceWriter log)
        {
            if (String.IsNullOrEmpty(tweetId))
            {
                return BuildResponse(HttpStatusCode.OK, ErrorIdNotFound);
            }

            var tweetDetails = Helper.FetchTweetFromStorage(visionTweetStorageTable, tweetId);
            if (tweetDetails == null)
            {
                return BuildResponse(HttpStatusCode.OK, ErrorTweetNotFound);
            }
            
            return BuildResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(new TweetProjection(tweetDetails)));
        }
        
        internal static HttpResponseMessage BuildResponse(HttpStatusCode code, string content)
        {
            return new HttpResponseMessage(code)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }
    }
}
