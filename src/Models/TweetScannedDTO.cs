using TweetSharp;

namespace TwitterVision.Models
{
    //transfer object from Scanner Queue Results, into Scanned Queue
    internal class TweetScannedDTO
    {
        public TweetScannedDTO() { }


        public string[] UsersToReplyTo { get; set; }
        public long OriginalTweetId { get; set; }
        public TwitterStatus TwitterStatus { get; set; }
        public MediaDescription MediaDescription { get; set; }
    }
}
