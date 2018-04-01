using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TwitterVision.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;
using TwitterVision.Models;

namespace TwitterVision
{
    internal static class Helper
    {
        internal static CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(GetEnvironmentVariable("AzureWebJobsStorage"));
        internal static CloudTableClient tableClient = StorageAccount.CreateCloudTableClient();

        internal static TwitterService TwitterService()
        {
            string TwitterTokenSecret = GetEnvironmentVariable("TwitterTokenSecret");
            string TwitterConsumerKey = GetEnvironmentVariable("TwitterConsumerKey");
            string TwitterConsumerSecret = GetEnvironmentVariable("TwitterConsumerSecret");
            string TwitterAuthenticationToken = GetEnvironmentVariable("TwitterAuthenticationToken");

            TwitterService service = new TwitterService(TwitterConsumerKey, TwitterConsumerSecret);
            service.AuthenticateWith(TwitterAuthenticationToken, TwitterTokenSecret);
            return service;
        }

        internal static TweetEntity FetchTweetFromStorage(string tweetId)
        {
            CloudTable table = tableClient.GetTableReference("visiontweet");

            if(string.IsNullOrEmpty(tweetId) || tweetId.Length<16)
            {
                return null;
            }

            var partitionKey = tweetId.Substring(0, 5);
            var rowKey = tweetId.Substring(5);

            TableOperation retrieveOperation = TableOperation.Retrieve<TweetEntity>(partitionKey, rowKey);
            return table.Execute(retrieveOperation).Result as TweetEntity;
        }

        internal static async Task<VisionDescription> FetchVisionDescriptionAsync(TwitterStatus tweet, TwitterMedia media)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", GetEnvironmentVariable("AzureVisionApiSubscriptionKey"));

            string requestParameters = "visualFeatures=Categories,Description,Color&language=en";
            string uri = GetEnvironmentVariable("AzureVisionUriBase") + "?" + requestParameters;

            var body = "{\"url\":\"" + media.MediaUrl + "\"}";
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage apiResponse = await client.PostAsync(uri, content);
            var result = await apiResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<VisionDescription>(result);
        }

        public static string GetEnvironmentVariable(string name)
        {
            // Documentation: Get an environment variable or an app setting value
            // https://cda.ms/nk

            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}


