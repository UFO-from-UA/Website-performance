using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebsitePerformance.Models;

namespace WebsitePerformance.Controllers
{
    public class HomeController : Controller
    {
        private RequestInfo _tempRequestInfo;

        public ActionResult Info()
        {
            ViewBag.dataPoints = RequestList.Requests;
            ViewBag.Baddest = RequestList._baddest;
            ViewBag.Best = RequestList._best;
            return View(RequestList.Requests);
        }

        public ActionResult Index()
        {
            return View("Info");
        }

        public ActionResult History()
        {
            ViewBag.dataPoints = RequestList.Requests;
            ViewBag.Baddest = RequestList._baddest;
            ViewBag.Best = RequestList._best;
            return View();
        }

        public ActionResult Refresh()
        {
            if (RequestList.SiteMapTree.Count!=0)
            {
                ViewBag.dataTree = RequestList.SiteMapTree.Where(x=>x.Ping != "");
                return PartialView("_modalPartial");
            }
            return Content("");
        }


        public ActionResult RefreshChart()
        {
            try
            {
                if (RequestList.ChartData.Count() != 0)
                {
               
                    RequestList.ChartDataMode = 1;
                    ViewBag.chartData = RequestList.ChartData;

                    return PartialView("_chartPartial", RequestList.ChartData);
                }
                
                return Content("");
            }
            catch
            {
                return Content("");
            }
        }

        public ActionResult CheckInformation(string url)
        {
            CheckRequest(url);

            ViewBag.dataPoints = RequestList.Requests;
            ViewBag.Baddest = RequestList._baddest;
            ViewBag.Best = RequestList._best;
            return View("Info", RequestList.Requests);
        }

        private void CheckRequest(string url)
        {
            #region FillPageData 
            var tmpTime = Stopwatch(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            ViewBag.page_speed = tmpTime.Replace("ms", "Milliseconds.");
            ViewBag.base_url = request.Host;
            _tempRequestInfo = new RequestInfo(url, tmpTime.Replace("ms", ""));
            RequestList.Add(_tempRequestInfo);
            #endregion
        }

        private MatchCollection ProcessResponse(string xmlData, string url, int nowDepth)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/xml";
                httpWebRequest.ContentLength = 0;
                //httpWebRequest.Method = "POST";
                httpWebRequest.Method = "GET";

                MatchCollection result = null;
                if (!string.IsNullOrEmpty(xmlData))
                {
                    byte[] data = Encoding.UTF8.GetBytes(xmlData);

                    using (var stream = httpWebRequest.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var text = streamReader.ReadToEnd();
                    Regex rx = new Regex("href=\"[^#].*(\"){1}",
                         RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.None);

                    //result = JsonConvert.DeserializeObject(streamReader.ReadToEnd());
                    result = rx.Matches(text);
                }
                return result;

            }
            catch (Exception)
            {
                return null;
            }
        }

        #region CheckLinkValid
        [HttpPost]
        public ActionResult GetURL(string URL)
        {
            bool validLink = true;
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
                return RedirectToAction("CheckInformation", "Home", new { URL = URL });
            }
            catch (Exception EX)
            {
                validLink = false;
            }

            return Content("Invalid link");
        }
        #endregion

        public ActionResult CreateMap(string url, int linkCountForParent = 10, int depth = 2 )
        {
            RequestList.SiteMapTree.Clear();
            SimpleTreeNode.Refresh();
            RequestList.TimeStamp = DateTime.Now;
            CreateSitemap(url, linkCountForParent, depth);
            ViewBag.dataTree = RequestList.SiteMapTree;
            return PartialView("_SiteMapPartial");
        }


        private void CreateSitemap(string baseUrl, int linkCountForParent , int depth)
        {
            RequestList.urls.Clear();
            RequestList.urls.Add(baseUrl);
            for (int nowDepth = 0; nowDepth < depth; nowDepth++)
            {
                var tmpUrlsList = new List<string>();

                foreach (var url in RequestList.urls)
                {

                    var res = ProcessResponse("", url, nowDepth);
                    if (res == null) { continue; }
                    int childsCounter = 0;

                    foreach (var i in res)
                    {
                        string tmpUrl = i.ToString();

                        //Skip useless
                        if (tmpUrl.Contains("\"/\"")) { continue; }
                        if (nowDepth != 0)
                        {
                            if (url.Contains("https") || url.Contains("http"))
                            {
                                if (childsCounter >= linkCountForParent)
                                {
                                    break;
                                }
                                childsCounter++;
                            }
                        }

                        if (tmpUrl.Contains("\">"))
                        {
                            //Check what first
                            if (tmpUrl.IndexOf(" ") > tmpUrl.IndexOf("\">"))
                            {
                                var urlString = tmpUrl.Substring(0, tmpUrl.IndexOf(@">") - 1).Replace("\"", "").Replace("href=", string.Empty);

                                var node = new SimpleTreeNode(urlString, (nowDepth * 4).ToString() + "rem") { Ping = Stopwatch(urlString) };
                                if (RequestList.SiteMapTree.Count(x => x.Title == node.Title) == 1) { continue; }
                                AddChartData(node);                               
                                if (nowDepth == 0)
                                {
                                    RequestList.SiteMapTree.Add(node);
                                }
                                else
                                {
                                    TreeSort(getParebtId(url, tmpUrl), node);
                                }
                                continue;
                            }
                        }

                        if (tmpUrl.Contains(" "))
                        {
                            var urlString = tmpUrl.Substring(0, tmpUrl.IndexOf(" ") - 1).Replace("\"", "").Replace("href=", string.Empty);
                            if (urlString.Contains(".css")) { continue; }
                            var node = new SimpleTreeNode(urlString, (nowDepth * 4).ToString() + "rem") { Ping = Stopwatch(urlString) };
                            if (RequestList.SiteMapTree.Count(x => x.Title == node.Title) == 1) { continue; }
                            AddChartData(node);
                            if (nowDepth == 0)
                            {
                                RequestList.SiteMapTree.Add(node);
                            }
                            else
                            {
                                TreeSort(getParebtId(url, tmpUrl), node);
                            }
                        }
                        else
                        {
                            var urlString = tmpUrl.Replace("\"", "").Replace("href=", string.Empty);
                            if (urlString.Contains(".css")) { continue; }
                            var node = new SimpleTreeNode(urlString, (nowDepth * 4).ToString() + "rem") { Ping = Stopwatch(urlString) };
                            if (RequestList.SiteMapTree.Count(x => x.Title == node.Title) == 1) { continue; }
                            AddChartData(node);
                            if (nowDepth == 0)
                            {
                                RequestList.SiteMapTree.Add(node);
                            }
                            else
                            {
                                TreeSort(getParebtId(url, tmpUrl), node);
                            }
                        }

                        if (CheckLoadingTime())
                        {
                            return;
                        }

                        if (nowDepth < depth - 1)
                        {
                            if (RequestList.SiteMapTree[RequestList.SiteMapTree.Count - 1].Title.Contains(@"https://") || RequestList.SiteMapTree[RequestList.SiteMapTree.Count - 1].Title.Contains(@"http://"))
                            {
                                tmpUrlsList.Add(RequestList.SiteMapTree[RequestList.SiteMapTree.Count - 1].Title);
                            }
                        }
                    }
                }
                RequestList.urls.Clear();
                RequestList.urls = tmpUrlsList;
            }
        }

      
        #region Helpers
        private bool CheckLoadingTime()
        {
            if ((DateTime.Now - RequestList.TimeStamp).Ticks > new TimeSpan(0, 3, 0).Ticks)
            {
                RequestList.urls.Clear();
                return true;
            }
            return false;
        }

        private void TreeSort(int ParentId, SimpleTreeNode node)
        {
            node.Parent_Id = ParentId;
            var ParentIndex = RequestList.SiteMapTree.IndexOf(RequestList.SiteMapTree.Where(x => x.Id == ParentId).FirstOrDefault());
            if (ParentIndex + 1 < RequestList.SiteMapTree.Count)
            {
                RequestList.SiteMapTree.Insert(1 + ParentIndex, node);
            }
            else
            {
                RequestList.SiteMapTree.Add(node);
            }

        }

        private int getParebtId(string parentUrl, string url)
        {
            return RequestList.SiteMapTree.Where(x => x.Title == parentUrl).FirstOrDefault().Id;
        }

        private string Stopwatch(string url)
        {
            #region Stopwatch 

            try
            {
                if (url.Contains("https") || url.Contains("http"))
                {

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    timer.Stop();
                    return timer.Elapsed.TotalMilliseconds.ToString() + " ms ";
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
            #endregion
        }

        private void AddChartData(SimpleTreeNode node)
        {
            if (node.Title.Contains("https") || node.Title.Contains("http"))
            {
                RequestList.AddChartData(node);
            }
        }
        #endregion
    }
}