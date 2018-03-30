using TweetSharp;

namespace TwitterVision.Models
{
    internal class TweetProjection
    {
        public TweetProjection(TweetEntity tweet)
        {
            GetStarted = "https://cda.ms/n2";
            Description = "As of March 28, 2018, this is just an experiment. After all the bugs " +
                "are worked out, I'll open source the solution, and fully document how this works. " +
                "For questions or updates please contact @scottcate on twitter.";

            TweetId = tweet.TweetId;
            MediaDescription = tweet.BuildMediaDescription();
            TwitterStatus = tweet.BuildTwitterStatus();
        }

        public string GetStarted { get; }
        public string Description { get; }
        public string TweetId { get; }
        public MediaDescription MediaDescription { get; }
        public TwitterStatus TwitterStatus { get; }
    }
}
