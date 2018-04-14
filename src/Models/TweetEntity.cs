using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using TweetSharp;

namespace TwitterVision.Models
{
    public class FailedTweetEntity : TweetEntity
    {
        public FailedTweetEntity(TwitterStatus status) : base(status)
        {
            PartitionKey = "Failed";
            RowKey = status.IdStr;
        }
    }
    public class TweetEntity : TableEntity
    {
        public TweetEntity() { }

        public TweetEntity(TwitterStatus status)
            : this(status.IdStr, status.FullText, status.InReplyToStatusId.ToString())
        {
            TweetJson = JsonConvert.SerializeObject(status);
        }

        public TweetEntity(string tweetId, string text, string replyToTweetId = "", string tweetJson = "", string visionJson = "")
        {
            TweetId = tweetId;
            PartitionKey = tweetId.Substring(0,5);
            RowKey = tweetId.Substring(5);
            ReplyToTweetId = replyToTweetId;
            Text = text;
            TweetJson = tweetJson;
            VisionJson = visionJson;
        }

        public string TweetId { get; set; }
        public string Text { get; set; }
        public string TweetJson { get; set; }
        public string VisionJson { get; set; }
        public string ReplyToTweetId { get; set; }

        public TwitterStatus BuildTwitterStatus()
        {
            TwitterStatus status = null;
            if (!string.IsNullOrEmpty(TweetJson))
            {
                status = JsonConvert.DeserializeObject<TwitterStatus>(TweetJson);
            }
            return status;
        }

        public MediaDescription BuildMediaDescription()
        {
            MediaDescription media = null;
            if (!string.IsNullOrEmpty(VisionJson))
            {
                media = JsonConvert.DeserializeObject<MediaDescription>(VisionJson);
            }
            return media;
        }
    }
}
