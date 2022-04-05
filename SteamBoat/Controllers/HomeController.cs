using ContentGrabber.DataAccess.Interfaces;
using ContentGrabber.Interfaces;
using ContentGrabber.Models.TypeSafeEnum;
using ContentGrabber.Models.ViewModels;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SteamBoat.Interfaces;
using SteamBoat.Models;
using SteamBoat.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

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

        public IActionResult Domission(int id, bool grab=false) 
        {
            missionReportVM res = null;

            if (grab)
            {
                res = _SteamBoatService.doMission(id, Freshness.Fresh);
            } else
            {
                res = _SteamBoatService.doMission(id, Freshness.AnyCached);
            }
            return View("/views/home/default.cshtml", res);
        }

        public IActionResult LHF(bool grab = false)
        {
            if (grab)
            {
                string res = _SteamBoatService.LHFandGaps(Freshness.Fresh);
            }
            else 
            {
                string res = _SteamBoatService.LHFandGaps(Freshness.AnyCached);
            }
            var myLHfs = _SteamBoatService.GetLHFS();
            return View(myLHfs);
        }

        public IActionResult CheckBids() 
        {

            var items = _SteamBoatService.GetAllItems();
            var bids = items.Where(b => b.bid_price != 0).ToList();
            return View("/views/home/items.cshtml", bids);
        
        }
        public IActionResult Gaps(bool grab = false)
        {
            if (grab)
            {
                string res = _SteamBoatService.LHFandGaps(Freshness.Fresh);
            }
            
            var myGaps = _SteamBoatService.GetGaps();
            return View(myGaps);
        }

        public IActionResult Index()
        {
            
            return View();


            }

        public IActionResult ReadBuyAndSellOrders() 
        {
            _SteamBoatService.ClearAllBids();

            //BUYS ** BUYS ** BUYS ** BUYS ** BUYS ** BUYS ** BUYS ** BUYS ** BUYS ** BUYS ** 
            var fileloc = Path.Combine(Directory.GetCurrentDirectory(), "downloads", "view-source_https___steamcommunity.com_market_.html");

            string BuyOrdersFile = System.IO.File.ReadAllText(fileloc);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(BuyOrdersFile);
            var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body");
            HtmlNodeCollection childnodes = htmlBody.ChildNodes;

            //var x = htmlBody.Descendants.("div").Where(d => d.Attributes["id"].Value.Contains("mybuyorder_")); 
            var buyorders = htmlBody.SelectNodes("//div[contains(@id, 'mybuyorder_')]");

            foreach (var buyorder in buyorders)
            {
                //var x = buyorder.Descendants("a").ToList();
                var market_listing_item_name = buyorder.Descendants("a").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_name_link").SingleOrDefault();
                var Name = market_listing_item_name.InnerText;
                var Link = market_listing_item_name.GetAttributeValue("href", "");
                var hash_name = HttpUtility.UrlDecode(Last_in_array(Link, "/"));
                var bid_and_quant = buyorder.Descendants("span").Where(c => c.GetAttributeValue("class", "") == "market_listing_price").ToList();
                var bid_price_in_pound = Last_in_array(_SteamBoatService.Clean(bid_and_quant[0].InnerText), "@");
                var bid_quant = int.Parse( _SteamBoatService.Clean(bid_and_quant[1].InnerText));
                var bid_price = _SteamBoatService.poundtocent(bid_price_in_pound);
                //var imageURL = buyorder.Descendants("img").Single();
                var imageURLFULL = buyorder.Descendants("img").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_img").SingleOrDefault();
                var imageURL = imageURLFULL.GetAttributeValue("src", "");
                imageURL = imageURL.Replace("/38fx38f", "").Replace("https://community.cloudflare.steamstatic.com/economy/image/", "");
                missionReportVM unused = new missionReportVM();
               var addResult = _SteamBoatService.CreateUpdateItemPage(hash_name, Link, unused, 0, imageURL, Link);
                var myitem = _SteamBoatService.UpdateBidPrice(hash_name, bid_quant, bid_price, bid_price_in_pound);


            }

            //* SELLS * SELLS * SELLS * SELLS * SELLS * SELLS * SELLS * SELLS * SELLS * SELLS * SELLS * SELLS 
            _SteamBoatService.ClearAllSales();
            var sellorders = htmlBody.SelectNodes("//div[contains(@id, 'mylisting_')]");
            foreach (var sellorder in sellorders)
            {
                //var x = buyorder.Descendants("a").ToList();
                var market_listing_item_name = sellorder.Descendants("a").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_name_link").SingleOrDefault();
                var Name = market_listing_item_name.InnerText;
                var Link = market_listing_item_name.GetAttributeValue("href", "");
                var hash_name = HttpUtility.UrlDecode(Last_in_array(Link, "/"));
                var bid_and_quant = sellorder.Descendants("span").Where(c => c.GetAttributeValue("class", "") == "market_listing_price").ToList();
                var sell_price_without_and_with_fees = Last_in_array(_SteamBoatService.Clean(bid_and_quant[0].InnerText), "@");
                var sell_price_after_fees = _SteamBoatService.getBetween(sell_price_without_and_with_fees, "(", ")");
                var int_sell_price_after_fees = _SteamBoatService.poundtocent(sell_price_after_fees);
                
                var imageURLFULL = sellorder.Descendants("img").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_img").SingleOrDefault();
                var imageURL = imageURLFULL.GetAttributeValue("src", "");
                imageURL = imageURL.Replace("/38fx38f", "").Replace("https://community.cloudflare.steamstatic.com/economy/image/", "");
                missionReportVM unused = new missionReportVM();
                var addResult = _SteamBoatService.CreateUpdateItemPage(hash_name, Link, unused, 0, imageURL, Link);
                var myitem = _SteamBoatService.AddSellListing(hash_name, int_sell_price_after_fees);


            }




            var items = _SteamBoatService.GetAllItems();
            var bids = items.Where(b => b.bid_price != 0).ToList();
            return View("/views/home/items.cshtml", bids);



        }

        public IActionResult agility()
        {
            //    var res = _ContentGrabberService.GrabMe(new GrabRequestVM() { Freshness = Freshness.Hour12.Id, Url = "https://feweek.co.uk/news/" });
            var res = _ContentGrabberService.GrabMe(new GrabRequestVM() { Freshness = Freshness.Hour12.Id, Url = "https://www.tameside.ac.uk/" });

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(res.HTML);
            var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body");
            HtmlNodeCollection childnodes = htmlBody.ChildNodes;

            //var x = htmlBody.Descendants("ul").Where(d => d.Attributes["class"].Value.Contains("pagination")); market_listing_row market_recent_listing_row
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

        public IActionResult LinksFromResults(missionReportVM res) 
        {



            return View();
        }

        public IActionResult Items() 
        {

            var items = _SteamBoatService.GetAllItems();
            return View(items);
        }

        public IActionResult ItemsGold()
        {

            var items = _SteamBoatService.GetAllItems().Where(sp => sp.StartingPrice > 50).Where(a => a.Activity > 70).Where(g => g.StartingPrice > 20);
            return View("/views/home/items.cshtml", items);



        }


        private string Last_in_array(string myString, string separator) 
        {

            string[] parts = myString.Split(separator);
            var hash_name = parts[parts.Length-1];
            return hash_name;
        }

    }
}
