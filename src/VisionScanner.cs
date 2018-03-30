using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using TweetSharp;
using TwitterVision.Models;

namespace TwitterVision.Twitter
{
    public static class VisionScanner
    {
        [FunctionName("VisionScanner")]
        public static async Task Run(
            [QueueTrigger("visionscanner")]string tweetQuery,
            [Table("visiontweet")] CloudTable visionTweetTable,
            [Queue("visionscanned")] IAsyncCollector<string> visionScannedQueue,
            TraceWriter log)
        {
            log.Info($"VisionScanner processing TweetId: {tweetQuery}");

            long tweetId = long.Parse(tweetQuery);
            long originalTweetId = tweetId;
            List<string> screenNamesToReplyTo = new List<string>();
            var tweetDetails = Helper.FetchTweetFromStorage(visionTweetTable, tweetQuery);
            if (tweetDetails != null) //already processed
            {
                log.Info($"VisionScanner Already Processed TweetId: {tweetQuery}");
                return; 
            }

            var service = Helper.TwitterService();
            TwitterStatus tweet = null;
            TweetEntity tweetEntity = null;

            while (tweet == null)
            {
                tweet = service.GetTweet(new GetTweetOptions
                {
                    Id = tweetId,
                    IncludeEntities = true,
                    TweetMode = TweetMode.Extended
                });

                foreach (var mention in tweet.Entities.Mentions)
                {
                    screenNamesToReplyTo.Add("@" + mention.ScreenName);
                }
                screenNamesToReplyTo.Add("@" + tweet.User.ScreenName);

                tweetEntity = new TweetEntity(
                      tweet.IdStr,
                      tweet.FullText,
                      tweet.InReplyToStatusId.ToString(),
                      JsonConvert.SerializeObject(tweet));

                //no media? try the parent
                if (!tweet.Entities.Media.Any())
                {
                    log.Info($"VisionScanner No media found in TweetId: {tweetQuery}");

                    // Documentation Link: Add an entity to a table - https://cda.ms/nn
                    visionTweetTable.Execute(TableOperation.Insert(tweetEntity));

                    if (tweet.InReplyToStatusId == null)
                    {
                        //walked up the whole chain - no media found
                        return;
                    }
                    else
                    {
                        tweetId = ((long)tweet.InReplyToStatusId);
                        tweet = null;
                    }
                }
            }

            var mediaDesc = new MediaDescription(tweet.Id);

            var photos = tweet.Entities.Media.Where(m => m.MediaType == TwitterMediaType.Photo);
            foreach (var media in photos)
            {
                log.Info($"VisionScanner FetchVisionDescriptionAsync TweetId/MediaId: {tweetQuery}/{media.Id}");
                var vision = await Helper.FetchVisionDescriptionAsync(tweet, media);
                mediaDesc.VisionDescription.Add(media.IdAsString, vision);
            }

            tweetEntity.VisionJson = JsonConvert.SerializeObject(mediaDesc);

            // Documentation Link: Add an entity to a table - https://cda.ms/nn
            visionTweetTable.Execute(TableOperation.Insert(tweetEntity));

            var dto = new TweetScannedDTO
            {
                TwitterStatus = tweetEntity.BuildTwitterStatus(),
                OriginalTweetId = originalTweetId,
                UsersToReplyTo = screenNamesToReplyTo.Distinct().ToArray(),
                MediaDescription = mediaDesc
            };

            log.Info($"VisionScanner Queuing VisionScanned TweetId: {tweetQuery}");
            await visionScannedQueue.AddAsync(JsonConvert.SerializeObject(dto));
            log.Info($"VisionScanner processed TweetId: {tweetQuery}");
        }
    }
}
