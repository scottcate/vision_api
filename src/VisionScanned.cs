using System;
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using TweetSharp;
using TwitterVision.Models;

namespace TwitterVision.Twitter
{
    public static class VisionScanned
    {
        [FunctionName("VisionScanned")]
        public static void Run([QueueTrigger("visionscanned")]string tweetEntity, 
            TraceWriter log)
        {
            TweetScannedDTO dto = JsonConvert.DeserializeObject<TweetScannedDTO>(tweetEntity);
            log.Info($"VisionScanned function processing TweetId: {dto.TwitterStatus.Id}");
            var service = Helper.TwitterService();
            foreach (var vision in dto.MediaDescription.VisionDescription)
            {
                var jsonUrl = string.Format(Helper.GetEnvironmentVariable("JsonDetailsUrlFormat"), dto.OriginalTweetId);
                var confidence = $"{vision.Value.description.captions[0].confidence,0:P2}";
                var replyToNames = string.Join(" ", dto.UsersToReplyTo);
                var status = $"{replyToNames} Confidence: {confidence}" +
                         $"{Environment.NewLine}{Environment.NewLine}" +
                         $"Vision API: { vision.Value.description.captions[0].text}" +
                         $"{Environment.NewLine}{Environment.NewLine}" +
                         $"Full API Result: { jsonUrl }";

                log.Info($"VisionScanned function Sending Tweet: {dto.TwitterStatus.Id} - {status}");

                var options = new SendTweetOptions
                {
                    Status = status,
                    InReplyToStatusId = dto.OriginalTweetId,
                    TrimUser = false
                };

                //var sentStatus = service.SendTweet(options, (tweet, response) =>
                //{
                //    if (response.StatusCode != HttpStatusCode.OK)
                //    {
                //        throw new Exception(response.StatusCode.ToString());
                //    }
                //});
            }

            log.Info($"VisionScanned function processed TweetId: {dto.TwitterStatus.Id}");
        }
    }
}
