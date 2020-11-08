using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsitePerformance.Models
{
    public class RequestInfo
    {
        public DateTime timestamp { get; set; }
        public string url { get; set; }
        public string pageSpeed { get; set; }
        public double best { get; set; }
        public double worst { get; set; }

        public RequestInfo()        {        }

        public RequestInfo(string url,string pageSpeed)
        {
            timestamp = DateTime.Now;
            this.url = url;
            this.pageSpeed = pageSpeed;
        }

        public override string ToString()
        {
            return $"{timestamp} : {url} /n Load time : {pageSpeed}";
        }
    }
}