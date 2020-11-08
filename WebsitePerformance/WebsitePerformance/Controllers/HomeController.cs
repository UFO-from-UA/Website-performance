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
            #region Stopwatch 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            Stopwatch timer1 = new Stopwatch();
            timer1.Start();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            timer1.Stop();
            TimeSpan timeTaken = timer1.Elapsed;
            ViewBag.page_speed = timeTaken.TotalMilliseconds.ToString() + " Milliseconds.";
            ViewBag.base_url = request.Host;
            _tempRequestInfo = new RequestInfo(url, timeTaken.TotalMilliseconds.ToString());
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
                httpWebRequest.Method = "POST";

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

        public ActionResult CreateMap(string url, int depth = 2)
        {
            RequestList.SiteMapTree.Clear();
            SimpleTreeNode.Refresh();
            CreateSitemap(url, depth);
            ViewBag.dataTree = RequestList.SiteMapTree;
            return PartialView("_SiteMapPartial");
        }


        private void CreateSitemap(string baseUrl, int depth)
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
                   
                    foreach (var i in res)
                    {
                        string tmpUrl = i.ToString();

                        //Skip useless
                        if (tmpUrl.Contains("\"/\""))
                        {
                            continue;
                        }

                        if (tmpUrl.Contains("\">"))
                        {
                            //Check what first
                            if (tmpUrl.IndexOf(" ") > tmpUrl.IndexOf("\">"))
                            {
                                var node = new SimpleTreeNode(tmpUrl.Substring(0, tmpUrl.IndexOf(@">") - 1).Replace("\"", "").Replace("href=", string.Empty), (nowDepth * 4).ToString() + "rem");
                                if (RequestList.SiteMapTree.Count(x => x.Title == node.Title) == 1) { continue; }
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
                            var node = new SimpleTreeNode(tmpUrl.Substring(0, tmpUrl.IndexOf(" ") - 1).Replace("\"", "").Replace("href=", string.Empty), (nowDepth * 4).ToString() + "rem");
                            if (RequestList.SiteMapTree.Count(x => x.Title == node.Title) == 1) { continue; }

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
                            var node = new SimpleTreeNode(tmpUrl.Replace("\"", "").Replace("href=", string.Empty), (nowDepth * 4).ToString() + "rem");
                            if (RequestList.SiteMapTree.Count(x => x.Title == node.Title) == 1) { continue; }
                            if (nowDepth == 0)
                            {
                                RequestList.SiteMapTree.Add(node);
                            }
                            else
                            {
                                TreeSort(getParebtId(url, tmpUrl), node);
                            }
                        }

                        if (nowDepth < depth-1)
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

        private void TreeSort(int ParentId, SimpleTreeNode node)
        {
            node.Parent_Id = ParentId;
            var ParentIndex = RequestList.SiteMapTree.IndexOf(RequestList.SiteMapTree.Where(x => x.Id == ParentId).FirstOrDefault());
            var sss = RequestList.SiteMapTree[ParentId];
            var sss2 = RequestList.SiteMapTree[ParentIndex];
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
    }
}