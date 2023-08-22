using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProducerConsumerRssFeeds;

public class RssFeedService
{
    public static IEnumerable<SyndicationItem> GetFeedItems(string feedUrl)
    {
        using var xmLReader = XmlReader.Create(feedUrl);
        SyndicationFeed rssFeed = SyndicationFeed.Load(xmLReader);
        return rssFeed.Items;
    }
}
