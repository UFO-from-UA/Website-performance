using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsitePerformance.Models
{
    public class RequestList
    {

        public static double _baddest;
        public static double _best = 100000;
        public static List<RequestInfo> Requests { get; set; }
        public static List<SimpleTreeNode> SiteMapTree { get; set; }
        public static List<string> urls { get; set; }
        public static DateTime TimeStamp { get; set; }

        static RequestList()
        {
            Requests = new List<RequestInfo>();
            SiteMapTree = new List<SimpleTreeNode>();
            urls = new List<string>();
        }

        public static void Add(RequestInfo item)
        {
            _baddest = _baddest < Convert.ToDouble(item.pageSpeed) ? Convert.ToDouble(item.pageSpeed) : _baddest;
            _best = _best > Convert.ToDouble(item.pageSpeed) ? Convert.ToDouble(item.pageSpeed) : _best;

            item = Check(item);

            Requests.Insert(0, item);
        }

        private static RequestInfo Check(RequestInfo item)
        {
            List<RequestInfo> temp = Requests.Where(x => x.url == item.url).ToList<RequestInfo>();
            if (temp.Count > 0)
            {
                if (temp[0].best > Convert.ToDouble(item.pageSpeed))
                {
                    foreach (var it in Requests.Where(x => x.url == item.url).ToList<RequestInfo>())
                    {
                        it.best = Convert.ToDouble(item.pageSpeed);
                    }
                    item.best = Convert.ToDouble(item.pageSpeed);
                }
                else
                {
                    item.best = temp[0].best;
                }

                if (temp[0].worst < Convert.ToDouble(item.pageSpeed))
                {
                    foreach (var it in Requests.Where(x => x.url == item.url).ToList<RequestInfo>())
                    {
                        it.worst = Convert.ToDouble(item.pageSpeed);
                    }
                    item.worst = Convert.ToDouble(item.pageSpeed);
                }
                else
                {
                    item.worst = temp[0].worst;
                }
            }
            else
            {
                item.best = Convert.ToDouble(item.pageSpeed);
                item.worst = Convert.ToDouble(item.pageSpeed);
            }
            return item;
        }
    }
}