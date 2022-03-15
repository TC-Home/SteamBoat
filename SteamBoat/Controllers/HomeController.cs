using ContentGrabber.DataAccess.Interfaces;
using ContentGrabber.Interfaces;
using ContentGrabber.Models.TypeSafeEnum;
using ContentGrabber.Models.ViewModels;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SteamBoat.Interfaces;
using SteamBoat.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SteamBoat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IContentGrabberDataService _ContentGrabberDataService;
        private readonly IContentGrabberService _ContentGrabberService;
        private readonly ISteamBoatService _SteamBoatService;

        public HomeController(ISteamBoatService SteamBoatService,ILogger<HomeController> logger, IContentGrabberDataService ContentGrabberDataService, IContentGrabberService ContentGrabberService)
        {
            _logger = logger;
            _ContentGrabberDataService = ContentGrabberDataService;
            _ContentGrabberService = ContentGrabberService;
            _SteamBoatService = SteamBoatService;
        }

        public IActionResult Index()
        {

            var res = _SteamBoatService.doMission(1, Freshness.Hour24);

            return View();
        }

        public IActionResult agility()
        {
            //    var res = _ContentGrabberService.GrabMe(new GrabRequestVM() { Freshness = Freshness.Hour12.Id, Url = "https://feweek.co.uk/news/" });
            var res = _ContentGrabberService.GrabMe(new GrabRequestVM() { Freshness = Freshness.Hour12.Id, Url = "https://www.tameside.ac.uk/" });

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(res.HTML);
            var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body");
            HtmlNodeCollection childnodes = htmlBody.ChildNodes;

            //var x = htmlBody.Descendants("ul").Where(d => d.Attributes["class"].Value.Contains("pagination")); ;
            var x = htmlBody.Descendants("ul");
            foreach (var nNode in x)
            {
                System.Diagnostics.Debug.WriteLine(nNode.Attributes["class"].Name);

                System.Diagnostics.Debug.WriteLine(nNode.Attributes["class"].Value);

            }
            var paginator = htmlBody.Descendants("ul").Where(x => x.Attributes["class"].Value.Contains("pagination")).SingleOrDefault();
            if (paginator != null)
            {
                foreach (var mypages in paginator.Descendants())
                {
                    System.Diagnostics.Debug.WriteLine("NAME: " + mypages.Name);
                    System.Diagnostics.Debug.WriteLine("INNER : " + mypages.InnerText);
                    if (mypages.Attributes["href"] != null)
                    {
                        System.Diagnostics.Debug.WriteLine("LINK : " + mypages.Attributes["href"].Value);
                    }

                }
            }

            foreach (var mypages in htmlBody.Descendants())
            {
                //System.Diagnostics.Debug.WriteLine("NAME: " + mypages.Name);
                //System.Diagnostics.Debug.WriteLine("INNER : " + mypages.InnerText);
                if (mypages.Attributes["href"] != null)
                {
                    System.Diagnostics.Debug.WriteLine("LINK : " + mypages.Attributes["href"].Value);
                }

            }

            System.Diagnostics.Debug.WriteLine("############################################");

            //QUICKER WAY
            foreach (HtmlNode link in htmlBody.SelectNodes("//a[@href]"))
            {

                HtmlAttribute att = link.Attributes["href"];
                if (att.Value.Contains("a"))
                {



                    var prev = link.PreviousSibling;
                    if (prev != null)
                    {
                        System.Diagnostics.Debug.WriteLine("PREV: " + prev.InnerText);
                    }

                    System.Diagnostics.Debug.WriteLine("LINK: " + att.Value);

                    var inner = link.InnerText;
                    if (inner != null)
                    {
                        System.Diagnostics.Debug.WriteLine("INNERText: " + inner);
                    }
                    var innerh = link.InnerHtml;
                    if (innerh != null)
                    {
                        System.Diagnostics.Debug.WriteLine("INNERhtml: " + innerh);
                    }

                    var next = link.NextSibling;
                    if (next != null)
                    {
                        System.Diagnostics.Debug.WriteLine("NEXT: " + next.InnerText);
                    }

                }

            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
