using System;
using System.Collections.Generic;
using System.Linq;
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
                var confidence = vision.Value.description.captions[0].confidence;

                string captionPrefix= "Caption";
                if (confidence < .2)
                    captionPrefix = "Guessing";
                else if (confidence < .4)
                    captionPrefix = "Unsure";
                else if (confidence < .6)
                    captionPrefix = "Maybe";
                else if (confidence < .8)
                    captionPrefix = "Probably";

                var replyToNames = string.Join(" ", dto.UsersToReplyTo);
                var status = $"{replyToNames} Confidence: {confidence,0:P2}" +
                         $"{Environment.NewLine}{Environment.NewLine}" +
                         "Tags: {0}" +
                         $"{Environment.NewLine}{Environment.NewLine}" +
                         $"{captionPrefix}: {vision.Value.description.captions[0].text}" +
                         $"{Environment.NewLine}{Environment.NewLine}" +
                         "Full API Result: {1}";

                //20 is for the t.co link space
                //replyToNames are not counted in tweet length
                var space = 280 - status.Length - 20 + replyToNames.Length;
                var tags = new List<string>();
                foreach (var tag in vision.Value.description.tags)
                {
                    int length = tag.Length;
                    if (length + space > 280)
                        break;

                    space += length + 2; //2 is for the ", " when joined
                    tags.Add(tag);
                }

                status = string.Format(status, string.Join(", ", tags), jsonUrl);

                log.Info($"VisionScanned function Sending Tweet: {dto.TwitterStatus.Id} - {status}");

                var options = new SendTweetOptions
                {
                    Status = status,
                    InReplyToStatusId = dto.OriginalTweetId,
                    TrimUser = false
                };

                var sentStatus = service.SendTweet(options, (tweet, response) =>
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(response.StatusCode.ToString());
                    }
                });
            }

            log.Info($"VisionScanned function processed TweetId: {dto.TwitterStatus.Id}");
        }
    }
}
