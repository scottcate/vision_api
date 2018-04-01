using TweetSharp;

namespace TwitterVision.Models
{
    internal class TweetProjection
    {
        public TweetProjection(TweetEntity tweet)
        {
            GetStarted = "https://cda.ms/n2";
            SourceCodeRepo = "https://github.com/scottcate/vision_api";
            Description = "Questions/Comments? Please contact @scottcate on twitter.";
            TweetId = tweet.TweetId;
            MediaDescription = tweet.BuildMediaDescription();
            TwitterStatus = tweet.BuildTwitterStatus();
        }

        public string GetStarted { get; }
        public string SourceCodeRepo { get; }
        public string Description { get; }
        public string TweetId { get; }
        public MediaDescription MediaDescription { get; }
        public TwitterStatus TwitterStatus { get; }
    }
}
