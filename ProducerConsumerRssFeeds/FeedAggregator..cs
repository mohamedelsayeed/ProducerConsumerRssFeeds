using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ProducerConsumerRssFeeds
{
    public class FeedAggregator
    {
        private async Task QueueAllFeeds(BufferBlock<SyndicationItem> itemQueue)
        {
            Task feedTask1 = ProduceFeedItems(itemQueue, "https://devblogs.microsoft.com/dotnet/feed/");
            //Task feedTask2 = ProduceFeedItems(itemQueue, "https://blogs.windows.com/feed");
            //Task feedTask3 = ProduceFeedItems(itemQueue, "https://www.microsoft.com/microsoft-365/blog/feed/");
            await Task.WhenAll(feedTask1
                //,                feedTask2, feedTask3
                );
            itemQueue.Complete();
        }
        public async Task<IEnumerable<BlogPost>> GetAllMicrosoftBlogPosts()
        {
            //var mockPost = new List<BlogPost>()
            //{
            //    new BlogPost{ Categories = "ds", PostContent="fd", PostDate=DateTime.Now.ToString()},
            //    new BlogPost{ Categories = "ds1", PostContent="fd1", PostDate=DateTime.Now.ToString()},
            //    new BlogPost{ Categories = "ds2", PostContent="fd2", PostDate=DateTime.Now.ToString()},
            //};
            //return mockPost;
            var posts = new ConcurrentBag<BlogPost>();

            BufferBlock<SyndicationItem> itemQueue = new BufferBlock<SyndicationItem>(
                    new DataflowBlockOptions
                    {
                        BoundedCapacity = 10
                    });
            var consumerOptions = new ExecutionDataflowBlockOptions { BoundedCapacity = 1 };

            var consumerA = new ActionBlock<SyndicationItem>((i) => ConsumerFeedItem(i, posts), consumerOptions);
            //var consumerB = new ActionBlock<SyndicationItem>((i) => ConsumerFeedItem(i, posts), consumerOptions);
            //var consumerC = new ActionBlock<SyndicationItem>((i) => ConsumerFeedItem(i, posts), consumerOptions);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            itemQueue.LinkTo(consumerA, linkOptions);
            //itemQueue.LinkTo(consumerB, linkOptions);
            //itemQueue.LinkTo(consumerC, linkOptions);

            Task Producers = QueueAllFeeds(itemQueue);

            await Task.WhenAll(Producers,
                               consumerA.Completion
                               //, consumerB.Completion, consumerC.Completion
                               );

            return posts;
        }

        private async Task ProduceFeedItems(BufferBlock<SyndicationItem> itemQueue, string feedUrl)
        {
            IEnumerable<SyndicationItem> items = RssFeedService.GetFeedItems(feedUrl);
            foreach (var item in items)
            {
                await itemQueue.SendAsync(item);
            }
        }

        private void ConsumerFeedItem(SyndicationItem nextItem, ConcurrentBag<BlogPost> blogPosts)
        {
            if (nextItem != null && nextItem.Summary != null)
            {
                BlogPost newPost = new();
                newPost.PostContent = nextItem.Summary.Text.ToString();
                newPost.PostDate = nextItem.PublishDate.ToLocalTime().ToString("g");
                if (nextItem.Categories != null)
                {
                    newPost.Categories = string.Join(", ", nextItem.Categories.Select(c => c.Name));
                }
                blogPosts.Add(newPost);
            }
        }


    }
}
