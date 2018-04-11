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

                         $"{captionPrefix}: {vision.Value.description.captions[0].text}" +
                         $"{Environment.NewLine}{Environment.NewLine}" +
                         
                         "Tags: {0}" +
                         $"{Environment.NewLine}{Environment.NewLine}" +
                         
                         "Full API Result: {1}";

                const int TCO_LENGTH = 22;
                const int MAX_LENGTH = 250;
                const int LINE_BREAKS_CHAR_COUNT = 12;
                //replyToNames are not counted in tweet length
                var spaceAvailable = MAX_LENGTH - status.Length - TCO_LENGTH - LINE_BREAKS_CHAR_COUNT + replyToNames.Length;

                var tags = new List<string>();
                var tagLength = 0;
                foreach (var tag in vision.Value.description.tags)
                {
                    tagLength += tag.Length + 2; //2 is for the ", " when joined
                    if (tagLength > spaceAvailable)
                        break;
                    
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
