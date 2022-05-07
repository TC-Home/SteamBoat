using ContentGrabber.DataAccess.Interfaces;
using ContentGrabber.Interfaces;
using ContentGrabber.Models.TypeSafeEnum;
using ContentGrabber.Models.ViewModels;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SteamBoat.Data;
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
        private readonly SteamBoatContext _context;

        private readonly ILogger<HomeController> _logger;
        private readonly IContentGrabberDataService _ContentGrabberDataService;
        private readonly IContentGrabberService _ContentGrabberService;
        private readonly ISteamBoatService _SteamBoatService;

        public HomeController(SteamBoatContext context,ISteamBoatService SteamBoatService,ILogger<HomeController> logger, IContentGrabberDataService ContentGrabberDataService, IContentGrabberService ContentGrabberService)
        {
            _logger = logger;
            _ContentGrabberDataService = ContentGrabberDataService;
            _ContentGrabberService = ContentGrabberService;
            _SteamBoatService = SteamBoatService;
            _context = context;
        }

        public IActionResult Domission(int id = 1003, bool grab=false) 
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

        public IActionResult x() 
        {

            for (int i = 10000; i < 43100; i = i + 100)
            {
                Console.WriteLine(i);
                var feed = new FeederUrl();
                feed.MissionId = 1002;
                feed.isJSON = true;
                feed.Url = "https://steamcommunity.com/market/search/render/?q=&start=" + i.ToString() + "&count=100&category_753_Game%5B%5D=any&category_753_cardborder%5B%5D=tag_cardborder_0&category_753_cardborder%5B%5D=tag_cardborder_1&appid=753&sort_column=price&currency=2&norender=1";
             //   _context.FeederUrl.Add(feed);
             //   _context.SaveChanges();
            }
            





            return Ok();
        
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

            
     // return RedirectToAction("AutoBid");
            //   var allItems = _context.Items.ToList();
            //    foreach (var myItem in allItems) 
            //    {
            //        _SteamBoatService.UpdateStatsforItem(myItem, Freshness.AnyCached);
            //     }

            // 
            //   _context.SaveChanges();

            return View();


            }

        public IActionResult PostBids()
        {
            _SteamBoatService.PostBids();

            return Content("OK");
        }
        

        public IActionResult AutoBid()
        {
            var allItems = _context.Items.ToList();
            foreach (var myItem in allItems)
            {
                myItem.IdealBid_Notes = "";
                myItem.IdealBidInt = 0;
                myItem.IdealBidStr = "";
                myItem.CancelCurrentBid = false;

                if (myItem.Name == "Top Hat") 
                    
                {
                
                }

                //set ideal bid
                if (ActivityGood(myItem) || myItem.IncludeInAutoBid == true || myItem.bid_price > 0) 
                {
                    if (PriceGood(myItem) || myItem.IncludeInAutoBid == true || myItem.bid_price > 0)
                    {
                        if (myItem.IncludeInAutoBid) 
                        {
                            myItem.IdealBid_Notes += " FLAGGED TO INCLUDE | ";
                        }
                        if (myItem.bid_price > 0) 
                        {
                            myItem.IdealBid_Notes += " EXISTING BID | ";
                        }
                     
                        //CAlculate the GAP
                        if (myItem.bid_price == myItem.bid1Price)
                        {
                            //I already have top bid
                            myItem.IdealBidInt = myItem.bid1Price;
                        }
                        else
                        {
                            //Calculate likely bid to work out GAP
                            myItem.IdealBidInt = myItem.bid1Price + 1;
                        }


                        var myGap = ((double)myItem.StartingPrice - (double)myItem.IdealBidInt) / (double)myItem.StartingPrice * 100;
                        //is this gap OK
                        //Console.WriteLine("Gap = " + ((int)myGap).ToString() + " | " + GapGood((int)myGap, myItem).ToString());
                        if (GapGood((int)myGap, myItem))
                        {

                            //Gap is good
                            //Set ideal bid

                            //Do we already have top bid?
                            if (myItem.bid_price == myItem.bid1Price)
                            {

                                //we have top bid
                                //If we are the only one bidding this price
                                //check we cant reduce it
                                if (myItem.bid1Quant == 1)
                                {
                                    myItem.IdealBid_Notes += " WE HAVE SOLO TOP BID | ";
                                    if (myItem.bid_price > myItem.bid2Price + 1)
                                    {
                                        //room to reduce our top bid
                                        myItem.IdealBid_Notes += " REDUCING IDEAL BID | ";
                                        myItem.IdealBidInt = myItem.bid2Price + 1;
                                    }
                                    else 
                                    {
                                        //keep top bid
                                        myItem.IdealBid_Notes += " HOLD | ";
                                        myItem.IdealBidInt = 0;
                                        myItem.IdealBidStr = "";
                                    }

                                }
                                else
                                {
                                    myItem.IdealBid_Notes += " WE HAVE TOP BID WITH OTHERS | ";
                                    if (myItem.bid1Quant > 3)
                                    {
                                        myItem.IdealBid_Notes += " RAISING IDEAL BID | ";
                                        myItem.IdealBidInt++;
                                    }
                                    else 
                                    {
                                        myItem.IdealBid_Notes += " HOLD | ";
                                        myItem.IdealBidInt = 0;
                                        myItem.IdealBidStr = "";
                                    }

                                }



                            }
                            else
                            {

                                //DONT HAVE TOP BID

                                if (myItem.bid_price > 0)
                                {
                                    //ALREADY HAVE A BID
                                    //BUT NOT TOP BID

                                    myItem.IdealBid_Notes += " RAISING IDEAL BID | ";
                                    myItem.IdealBidInt = myItem.bid1Price + 1;
                                }
                                else
                                {
                                    //DONT ALREADY HAVE A BID
                                    //NEW BID
                                    myItem.IdealBid_Notes += " NEW BID | ";
                                    myItem.IdealBidInt = myItem.bid1Price + 1;
                                }
                            }








                        }
                        else
                        {
                            //GAp too Small
                            if (myItem.bid_price > 0)
                            {
                                myItem.IdealBid_Notes += " CANCEL CURRENT BID (GAP) | ";
                                myItem.CancelCurrentBid = true;
                            }
                            myItem.IdealBidInt = 0;
                            myItem.IdealBidStr = "";
                        }



                    }
                    else 
                    {
                        //Price out of bounds
                        if (myItem.bid_price > 0)
                        {
                            myItem.IdealBid_Notes += " CANCEL CURRENT BID (PRICE OOB) | ";
                            myItem.CancelCurrentBid = true;
                        }
                        myItem.IdealBidInt = 0;
                        myItem.IdealBidStr = "";

                    }

                }
                //FINAL CHECKS
                if (myItem.IdealBidInt > 0) 
                {
                    if (myItem.IdealBidInt > 800 || myItem.IdealBidInt < 10) 
                    {
                        myItem.IdealBid_Notes += " ** ERROR ** NUMBER OOB | " + myItem.IdealBidInt.ToString();
                        myItem.IdealBidInt = 0;
                        myItem.IdealBidStr = "";
                    }


                    var myGapFinal = ((double)myItem.StartingPrice - (double)myItem.IdealBidInt) / (double)myItem.StartingPrice * 100;
                    if ((int)myGapFinal < 20)
                    {
                        myItem.IdealBid_Notes += " ** ERROR ** GAP < 20 | " + myGapFinal.ToString();
                        myItem.IdealBidInt = 0;
                        myItem.IdealBidStr = "";
                    }



                    //Still got a value after checks?
                    if (myItem.IdealBidInt > 0)
                    {
                        //Got a bid make a str
                        decimal dec = Convert.ToDecimal(myItem.IdealBidInt);
                        dec = dec / 100;
                        myItem.IdealBidStr = dec.ToString();
                    }

                }


            }
            _context.SaveChanges();
            var allItems2 = _context.Items.ToList();
            return View(allItems2);
        }

      

        bool GapGood(int myGap, Item myItem)
        {
            // 30 - 40
            if (myItem.StartingPrice <= 40)
            {
                if (myGap >= 25)
                {
                    return true;

                }
                else
                {
                    myItem.IdealBid_Notes += " GAP TOO SMALL | ";
                    return false;

                }
            }
            // 41 - 75
            if (myItem.StartingPrice <= 75)
            {
                if (myGap >= 22)
                {
                    return true;

                }
                else
                {
                    myItem.IdealBid_Notes += " GAP TOO SMALL | ";
                    return false;

                }
            }

            // 76 +
            if (myGap >= 20)
                {
                    return true;

                }
                else
                {
                myItem.IdealBid_Notes += " GAP TOO SMALL | ";
                return false;

                }
            


        }

        bool PriceGood(Item myItem)
        {
            if (myItem.StartingPrice > 30 && myItem.StartingPrice < 800)
            {
                return true;
            }
            else 
            {
                myItem.IdealBid_Notes += " OUT OF PRICE RANGE | ";
                return false;
            }
        }


        bool ActivityGood(Item myItem) 
        {
            if (myItem.StartingPrice < 50)
            {

                //Low price item needs to have more activity
                if (myItem.Activity >= 20)
                {
                    //active enough
                    
                    return true;
                }
                else 
                {
                    //NOT active enough
                    myItem.IdealBid_Notes += " NOT ACTIVE ENOUGH | ";
                    return false;
                
                }

            }
            else 
            {
                //Higher price allowed less activity
                if (myItem.Activity >= 10)
                {
                    //active enough
                    return true;
                }
                else
                {
                    //NOT active enough
                    myItem.IdealBid_Notes += " NOT ACTIVE ENOUGH | ";
                    return false;

                }

            }

            
        }


            public IActionResult AutoBid2()
        {

            int MinGap = 23;
            int MinActivity = 16;
            int MinStartingPrice = 40;
            int MaxStartingPrice = 800;
            

            var allItems = _context.Items.ToList();
            foreach (var myItem in allItems)
            {
                myItem.autoBidint = 0;
                myItem.autoBidStr = "";
                myItem.autoBidNotes = "";
                var before = myItem.bid_price;

                //checker
                if (myItem.Name.Contains("Prison"))
                {

                }


              
                    //POTENTIAL BID
                    //we may want to bid
                    //check we are not already top bid
                 
                    if ((myItem.Activity > MinActivity && myItem.StartingPrice > MinStartingPrice) || (myItem.IncludeInAutoBid == true) || (myItem.bid_price > 0)) 
                    {
                        //we may want to bid
                        //check we are not already top bid

                        if (myItem.bid_price >= myItem.bid1Price)
                        {
                            myItem.autoBidNotes = myItem.autoBidNotes + "We Top Bid | ";
                            Console.WriteLine("Already top bid : " + myItem.Name + " = " + myItem.bid_price_in_pound);
                            Console.WriteLine("Check gap below my bid (bigger than 1?)");
                            var mybidshouldbe = myItem.bid2Price + 1;
                            if (mybidshouldbe != myItem.bid1Price) 
                            {
                                if (myItem.bid1Quant > 1)
                                {
                                    Console.WriteLine("Cant reduce becasue others are also bidding the gap");

                                }
                                else
                                {
                                    myItem.autoBidNotes = myItem.autoBidNotes + "Reduce bid | ";
                                    Console.WriteLine("Reduced bid from " + myItem.bid_price_in_pound);
                                    myItem.autoBidint = myItem.bid2Price + 1;
                                    decimal dec = Convert.ToDecimal(myItem.autoBidint);
                                    dec = dec / 100;
                                    myItem.autoBidStr = dec.ToString();
                                    Console.WriteLine("to ... " + myItem.autoBidStr);
                                }
                            }

                        } 
                        else
                        {

                            if (myItem.bid1Quant == 1)
                            {
                                myItem.autoBidint = myItem.bid1Price;
                            }
                            else
                            {
                                myItem.autoBidint = myItem.bid1Price + 1;
                            }


                            //Check that the top bid isnt too high and making us pay too much (high hanging fruit)
                            // gap over 8
                            // starting price over 80
                            // bid quant = 1

                            if (myItem.Fruit2 > 15 && myItem.StartingPrice > 99 && myItem.bid1Quant == 1)
                            {
                                myItem.autoBidNotes = myItem.autoBidNotes + "HHF | ";
                                //HHF!
                                Console.WriteLine("HHF!! Reducing bid ...");
                                myItem.autoBidint = myItem.bid2Price + 1;
                                Console.WriteLine("to ..." + myItem.autoBidint.ToString());

                            }
                            else 
                            {
                                if (myItem.Fruit2 > 10 && myItem.StartingPrice > 199 && myItem.bid1Quant == 1)
                                {
                                    myItem.autoBidNotes = myItem.autoBidNotes + "HHF2 | ";
                                    //HHF!
                                    Console.WriteLine("HHF2!! Reducing bid ...");
                                    myItem.autoBidint = myItem.bid2Price + 1;
                                    Console.WriteLine("to ..." + myItem.autoBidint.ToString());

                                }


                            }



                            
                            


                        }
                    }
              

               
                //FINAL CHECK
                    if (myItem.autoBidint != before)
                    {
                        Console.WriteLine("bid started " + before.ToString() + " is now " + myItem.autoBidint.ToString());
                        if (myItem.autoBidint > 10 && myItem.autoBidint < 700)
                        {

                            decimal dec = Convert.ToDecimal(myItem.autoBidint);
                            dec = dec / 100;
                            myItem.autoBidStr = dec.ToString();
                        }
                        else
                        {
                        if (myItem.autoBidint == 0) { 
                            Console.WriteLine("ero bid = " + myItem.autoBidint.ToString());
                            myItem.autoBidStr = "";
                            myItem.autoBidint = 0;
                        }
                        else
                        {
                            myItem.autoBidNotes = myItem.autoBidNotes + "INVALID BID = " + myItem.autoBidint.ToString() + " | ";
                            Console.WriteLine("INVALID BID! = " + myItem.autoBidint.ToString());
                            myItem.autoBidStr = "";
                            myItem.autoBidint = 0;
                        }

                        }

                    }
                    else
                    {
                        Console.WriteLine("Bid remained the same after changes, Ignored : " + myItem.autoBidint.ToString());
                        myItem.autoBidStr = "";
                        myItem.autoBidint = 0;


                    }



                if (myItem.autoBidint > 0)
                {
                    var mygap = ((double)myItem.min_sell_price - (double)myItem.autoBidint) / (double)myItem.min_sell_price * 100;
                    myItem.Gap_mybid = (int)mygap;
                    if (myItem.Gap_mybid < MinGap)
                    {
                        //main gap and my gap below min
                        //dont bid & cancell bids
                        myItem.autoBidNotes = myItem.autoBidNotes + "Cancelled becasue of low gap with my bid | ";
                        myItem.autoBidStr = "Cancel Bid";
                     //   myItem.autoBidStr = "";
                        myItem.autoBidint = 0;
                    }
                }
         


                if (myItem.autoBidint > before)
                    {
                        if (before == 0)
                        {
                            myItem.autoBidNotes = "NEW BID | " + before.ToString() + " to " + myItem.autoBidint.ToString() + " | " + myItem.autoBidNotes;
                        }
                        else
                        {
                            myItem.autoBidNotes = "INCREASED BID | " + before.ToString() + " to " + myItem.autoBidint.ToString() + " | " + myItem.autoBidNotes;
                        }
                    }
                    else if (myItem.autoBidint == before)
                    {
                        myItem.autoBidNotes = "BID KEPT | " + before.ToString() + " to " + myItem.autoBidint.ToString() + " | " + myItem.autoBidNotes  ;

                    }
                    else if (myItem.autoBidint < before)
                    {
                        if (myItem.autoBidint == 0)
                        {
                            myItem.autoBidNotes = "CANCELLED BID | " + before.ToString() + " to " + myItem.autoBidint.ToString() + " | " + myItem.autoBidNotes;
                        }
                        else
                        {
                            myItem.autoBidNotes = "DECREASED BID | " + before.ToString() + " to " + myItem.autoBidint.ToString() + " | " + myItem.autoBidNotes;
                        }

                    }

            }

            _context.SaveChanges();

            //_SteamBoatService.PostBids();



            return Content("OK");


        }    


                public IActionResult ReadBuyOrders() 
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

            if (buyorders != null)
            {
                foreach (var buyorder in buyorders)
                {
                    
                    //var x = buyorder.Descendants("a").ToList();
                    var market_listing_item_name = buyorder.Descendants("a").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_name_link").SingleOrDefault();
                    var Name = market_listing_item_name.InnerText;
                    var Link = market_listing_item_name.GetAttributeValue("href", "");
                    var hash_name = HttpUtility.UrlDecode(Last_in_array(Link, "/"));
                    var bid_and_quant = buyorder.Descendants("span").Where(c => c.GetAttributeValue("class", "") == "market_listing_price").ToList();
                    var bid_price_in_pound = Last_in_array(_SteamBoatService.Clean(bid_and_quant[0].InnerText), "@");
                    var bid_quant = int.Parse(_SteamBoatService.Clean(bid_and_quant[1].InnerText));
                    var bid_price = _SteamBoatService.poundtocent(bid_price_in_pound);
                    //var imageURL = buyorder.Descendants("img").Single();
                    var imageURLFULL = buyorder.Descendants("img").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_img").SingleOrDefault();
                    var imageURL = imageURLFULL.GetAttributeValue("src", "");
                    imageURL = imageURL.Replace("/38fx38f", "").Replace("https://community.cloudflare.steamstatic.com/economy/image/", "");
                    imageURL = imageURL.Replace("https://community.akamai.steamstatic.com/economy/image/", "");
                    if (imageURL.Contains("https:")) 
                    {
                        Console.WriteLine("ERROR : This should just be image code, no http : " + imageURL);
                    }
                    missionReportVM unused = new missionReportVM();
                    if (bid_price == 37) 
                    { 
                    }
                    var addResult = _SteamBoatService.CreateUpdateItemPage(hash_name, Link, unused, 0, imageURL, Link);
                    var myitem = _SteamBoatService.UpdateBidPrice(hash_name, bid_quant, bid_price, bid_price_in_pound);


                }
            }


            
            var items = _SteamBoatService.GetAllItems();
            var bids = items.Where(b => b.bid_price != 0).ToList();
            return View("/views/home/items.cshtml", bids);



        }


        public IActionResult ReadSellOrders() 
        {
            _SteamBoatService.ClearAllSales();
            var fileloc = Path.Combine(Directory.GetCurrentDirectory(), "downloads//Selling", "download.json");
            

            string myHistory = System.IO.File.ReadAllText(fileloc);
           

            var myJSOObject = JObject.Parse(myHistory);
            var arrayofitems = myJSOObject.SelectToken("results_html");



            var combined = arrayofitems.ToString();

            //THIS is optiopnal seconf page
            try {
                var fileloc2 = Path.Combine(Directory.GetCurrentDirectory(), "downloads//Selling", "download2.json");
                string myHistory2 = System.IO.File.ReadAllText(fileloc2);
                var myJSOObject2 = JObject.Parse(myHistory2);
                var arrayofitems2 = myJSOObject2.SelectToken("results_html");
                combined = combined + arrayofitems2.ToString(); }
            catch
            {
                Console.WriteLine("only one page of sell listings (less than 100)");

            }




            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(combined);

            //var mySelling = htmlDoc.DocumentNode.SelectNodes("//div[contains(@id, 'history_row_')]");


            var sellorders = htmlDoc.DocumentNode.SelectNodes("//div[contains(@id, 'mylisting_')]");
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

                var sell_price_without_fees = sell_price_without_and_with_fees.Split("(")[0];
                var int_sell_price_without_fees = _SteamBoatService.poundtocent(sell_price_without_fees);


                var imageURLFULL = sellorder.Descendants("img").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_img").SingleOrDefault();
                var imageURL = imageURLFULL.GetAttributeValue("src", "");
                imageURL = imageURL.Replace("/38fx38f", "").Replace("https://community.cloudflare.steamstatic.com/economy/image/", "");
                imageURL = imageURL.Replace("https://community.akamai.steamstatic.com/economy/image/", "");
                if (imageURL.Contains("https:"))
                {
                    Console.WriteLine("ERROR : This should just be image code, no http : " + imageURL);
                }
                missionReportVM unused = new missionReportVM();
                var addResult = _SteamBoatService.CreateUpdateItemPage(hash_name, Link, unused, 0, imageURL, Link);
                var myitem = _SteamBoatService.AddSellListing(hash_name, int_sell_price_after_fees, int_sell_price_without_fees, sell_price_without_fees);


            }

            return Content("OK");

            }

        public IActionResult UpdateActivity() 
        {

            _SteamBoatService.ActivityUpdateAll(true);
            return Content("OK");
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

        public IActionResult Excluder()
        {

            return Content(_SteamBoatService.Excluder());
            
        }


        public IActionResult ItemsGold()
        {

            var items = _SteamBoatService.GetAllItems().Where(sp => sp.Gap > 19).Where(a => a.Activity > 74).Where(g => g.StartingPrice > 20);
            return View("/views/home/items.cshtml", items);



        }


        public IActionResult CheckSalePrices()
        {

            
           var result = _SteamBoatService.CheckSalePrices();
            var sale_items = _SteamBoatService.GetAllItemsandSales();
            return View(sale_items);

           


        }

        public IActionResult ReadHistory()
        {

            var fileloc = Path.Combine(Directory.GetCurrentDirectory(), "downloads", "download.json");

            string myHistory = System.IO.File.ReadAllText(fileloc);

            var myJSOObject = JObject.Parse(myHistory);
            var arrayofitems = myJSOObject.SelectToken("results_html");
            
            
            
            //json
           //var assets = myJSOObject.SelectToken("assets");

            foreach (var assetgroup in myJSOObject)
            {
                
                if (assetgroup.Key == "assets") 
                {
                    foreach (var assets in assetgroup.Value)
                    {
                        foreach (var instances in assets)
                        {
                            foreach (var itemcount in instances)
                            {
                                foreach (var games in itemcount)
                                {
                                    foreach (var game in games) 
                                    {

                                        foreach (var props in game)
                                        {

                                            var assettype = assets.GetType();

                                           
                                            var myasset = (JProperty)assets;
                                            var myassetName = myasset.Name;



                                            

                                            var game_hash = props.SelectToken("market_hash_name").ToString();
                                            var icon = props.SelectToken("icon_url").ToString();


                                            if (ItemExists(game_hash) == false)
                                            {

                                                Console.WriteLine("Trans: Game *DOES NOT* Exsist ... ADDING : " + game_hash);

                                                var url = "https://steamcommunity.com/market/listings/" + myassetName + "/";
                                                var notused = new missionReportVM();
                                                var res = _SteamBoatService.CreateUpdateItemPage(game_hash, url, notused, 0, icon);





                                            }
                                            else
                                            {
                                                Console.WriteLine("Trans: Game Exsist");
                                            }

                                        }

                                }

                                }
                            }
                        }
                    }
                }
                //var group 2 = ((JValue)assetgroup).Value;
                //var x = arrayofitems.Count();
                // var sell_listings = item.SelectToken("sell_listings").Value<string>();
                // var sell_price = item.SelectToken("sell_price").Value<string>();
                //  var name = item.SelectToken("name").Value<string>();
                //  var sell_price_text = item.SelectToken("sell_price_text").Value<string>();
                // Console.WriteLine(name + " " + sell_price_text + " (" + sell_listings + ")");
                // var assets = item.SelectToken("asset_description");
                // var URL = assets.SelectToken("icon_url").Value<string>();

            }


            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(arrayofitems.ToString());


            //var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//");


            var transactions = htmlDoc.DocumentNode.SelectNodes("//div[contains(@id, 'history_row_')]");

            
            foreach (var transaction in transactions)
            {

                var id = transaction.GetAttributeValue("id", "");

                var Item = transaction.Descendants("span").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_name").SingleOrDefault().InnerText;
                var Game = transaction.Descendants("span").Where(c => c.GetAttributeValue("class", "") == "market_listing_game_name").SingleOrDefault().InnerText;
                var mydate = transaction.Descendants("div").Where(c => c.GetAttributeValue("class", "").Contains("market_listing_listed_date")).FirstOrDefault().InnerText;
                var buysell = transaction.Descendants("div").Where(c => c.GetAttributeValue("class", "").Contains("market_listing_gainorloss")).FirstOrDefault().InnerText;
                buysell = _SteamBoatService.CleanMe(buysell);

                //see if transaction exists
                if (TransactionExists(id) == false && (buysell == "+" || buysell == "-"))
                {

                    var myprice = transaction.Descendants("span").Where(c => c.GetAttributeValue("class", "") == "market_listing_price").SingleOrDefault().InnerText;
                    myprice = _SteamBoatService.Clean(myprice);
                    var intmyprice = _SteamBoatService.poundtocent(myprice);



                    //                 var market_listing_item_name = transaction.Descendants("a").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_name_link").SingleOrDefault();
                    //                   var Name = market_listing_item_name.InnerText;
                    //              var Link = market_listing_item_name.GetAttributeValue("href", "");
                    //              var hash_name = HttpUtility.UrlDecode(Last_in_array(Link, "/"));
                    //            var bid_and_quant = sellorder.Descendants("span").Where(c => c.GetAttributeValue("class", "") == "market_listing_price").ToList();
                    //          var sell_price_without_and_with_fees = Last_in_array(_SteamBoatService.Clean(bid_and_quant[0].InnerText), "@");
                    //        var sell_price_after_fees = _SteamBoatService.getBetween(sell_price_without_and_with_fees, "(", ")");
                    //      var int_sell_price_after_fees = _SteamBoatService.poundtocent(sell_price_after_fees);

                    //     var sell_price_without_fees = sell_price_without_and_with_fees.Split("(")[0];
                    //     var int_sell_price_without_fees = _SteamBoatService.poundtocent(sell_price_without_fees);


                    //   var imageURLFULL = transaction.Descendants("img").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_img").SingleOrDefault();
                    //    var imageURL = imageURLFULL.GetAttributeValue("src", "");
                    //   imageURL = imageURL.Replace("/38fx38f", "").Replace("https://community.cloudflare.steamstatic.com/economy/image/", "");

                    //     var myitem = _SteamBoatService.AddSellListing(hash_name, int_sell_price_after_fees, int_sell_price_without_fees, sell_price_without_fees);






                   









                    mydate = _SteamBoatService.Clean(mydate);
                    DateTime myparseddate = DateTime.Parse(mydate);
                  
                    var hash = _SteamBoatService.GetGameHashNamefromItemandGame(Item.ToString(), Game.ToString());
                    
                    //missionReportVM unused = new missionReportVM();
                   // var addResult = _SteamBoatService.CreateUpdateItemPage(hash, Link, unused, 0, imageURL, Link);





                    var error = false;
                    if (hash == null)
                    {
                        error = true;
                        Console.WriteLine("ERROR: " + Item.ToString() + " " + Game.ToString());
                    }

                    if (error == false)
                    {
                        var Mytrans = new Transaction();

                        Mytrans.Tran_Id = id;
                        Mytrans.Game_hash_name_key = hash;
                        Mytrans.int_sale_price_after_fees = intmyprice;
                        Mytrans.sale_price_after_fees = myprice;
                        Mytrans.DateT = myparseddate;
                        Mytrans.type = char.Parse(buysell);
                        _context.Transactions.Add(Mytrans);
                        _context.SaveChanges();
                    }
                } else 
                {
                    Console.WriteLine(id + " already logged or is not a buy or sell");
                }

                //var x = buyorder.Descendants("a").ToList();
                // var market_listing_item_name = sellorder.Descendants("a").Where(c => c.GetAttributeValue("class", "") == "market_listing_item_name_link").SingleOrDefault();
                //var Name = market_listing_item_name.InnerText;
                //var Link = market_listing_item_name.GetAttributeValue("href", "");
                // var hash_name = HttpUtility.UrlDecode(Last_in_array(Link, "/"));
                //  var bid_and_quant = sellorder.Descendants("span").Where(c => c.GetAttributeValue("class", "") == "market_listing_price").ToList();
                //  var sell_price_without_and_with_fees = Last_in_array(_SteamBoatService.Clean(bid_and_quant[0].InnerText), "@");
                //   var sell_price_after_fees = _SteamBoatService.getBetween(sell_price_without_and_with_fees, "(", ")");
                //   var int_sell_price_after_fees = _SteamBoatService.poundtocent(sell_price_after_fees);

                //                var sell_price_without_fees = sell_price_without_and_with_fees.Split("(")[0];
                //              var int_sell_price_without_fees = _SteamBoatService.poundtocent(sell_price_without_fees);

            }



            _SteamBoatService.UpdateTrans();
            return Content("OK");


        }


        private string Last_in_array(string myString, string separator) 
        {

            string[] parts = myString.Split(separator);
            var hash_name = parts[parts.Length-1];
            return hash_name;
        }


        private bool TransactionExists(string id)
        {
            return _context.Transactions.Any(e => e.Tran_Id == id);
        }

        private bool ItemExists(string id)
        {
            return _context.Items.Any(e => e.hash_name_key == id);
        }

        void RandomWait(int min100, int max100)
        {

            //Generate random sleeptime
            Random waitTime = new Random();
            var seconds = waitTime.Next(min100 * 75, max100 * 75);

            //Put the thread to sleep
            System.Threading.Thread.Sleep(seconds);
        }
    }
}
