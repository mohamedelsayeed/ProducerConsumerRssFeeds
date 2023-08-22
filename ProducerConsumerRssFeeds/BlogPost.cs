using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerRssFeeds
{
    public class BlogPost
    {
        public string PostDate { get; set; } = "";
        public string? Categories { get; set; }
        public string? PostContent { get; set; }
    }
}
