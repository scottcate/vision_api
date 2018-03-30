
using System.Collections.Generic;

namespace TwitterVision.Models
{
    public class MediaDescription
    {
        public long TweetId { get; set; }
        public IDictionary<string, VisionDescription> VisionDescription { get; set; }

        public MediaDescription(long tweetId = 0)
        {
            TweetId = tweetId;
            VisionDescription = new Dictionary<string, VisionDescription>();
        }
    }
}
