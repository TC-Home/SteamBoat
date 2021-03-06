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
using Microsoft.Extensions.Configuration;

namespace SteamBoat.Controllers
{
    public class HomeController : Controller
    {
        private readonly SteamBoatContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<HomeController> _logger;
        private readonly IContentGrabberDataService _ContentGrabberDataService;
        private readonly IContentGrabberService _ContentGrabberService;
        private readonly ISteamBoatService _SteamBoatService;

        public HomeController(IConfiguration Configuration, SteamBoatContext context, ISteamBoatService SteamBoatService, ILogger<HomeController> logger, IContentGrabberDataService ContentGrabberDataService, IContentGrabberService ContentGrabberService)
        {
            _logger = logger;
            _ContentGrabberDataService = ContentGrabberDataService;
            _ContentGrabberService = ContentGrabberService;
            _SteamBoatService = SteamBoatService;
            _context = context;
            _config = Configuration;
        }

        public IActionResult Domission(int id = 1004, bool grab = true, bool flip_order = false)
        {

            missionReportVM res = null;

            if (grab)
            {
                res = _SteamBoatService.doMission(id, Freshness.Fresh,flip_order);
            } else
            {
                res = _SteamBoatService.doMission(id, Freshness.AnyCached, flip_order);
            }
            return View("/views/home/default.cshtml", res);
        }
        public IActionResult Cancelbidsbygame(string game = "Rust")
        {

            var games = _context.Items.Where(b => b.Game == game && b.bid_price > 0).ToList();
            foreach (var mybid in games)
            {

                //mybid.bid_price = 0;
                //mybid.bid_price_in_pound = "";
                mybid.bid_quant = 0;
                mybid.IdealBidInt = 0;
                mybid.IdealBidStr = "";
                mybid.CancelCurrentBid = true;
                mybid.IdealBid_Notes = "cancelled by Cancelbidsbygame";

            }
            _context.SaveChanges();
            _SteamBoatService.PostBids(null, null, true);
            return Content("OK");

        }


        public IActionResult bidcheckall()
        {
            var myitems = _context.Items.ToList();
            foreach (var myItem in myitems)
            {
                _SteamBoatService.bidcheck(myItem);
            }
            _context.SaveChanges();
            return Content("OK");
        }
            public IActionResult Cancelbidsbyprice(int price = 200) 
        {

            var bids = _context.Items.Where(b => b.bid_price > price).ToList();
            foreach (var mybid in bids) 
            {
                
                //mybid.bid_price = 0;
                //mybid.bid_price_in_pound = "";
                mybid.bid_quant = 0;
                mybid.IdealBidInt = 0;
                mybid.IdealBidStr = "";
                mybid.CancelCurrentBid = true;
                mybid.IdealBid_Notes = "cancelled by Cancelbidsbyprice";

            }
            _context.SaveChanges();
            _SteamBoatService.PostBids(null, null, true);
            return Content("OK");
        
        }

        public IActionResult LHF(string game, string excludeGame, bool grab = false)
        {
            if (grab)
            {
                string res = _SteamBoatService.LHFandGaps(Freshness.Fresh, game, excludeGame);
            }
            else
            {
                string res = _SteamBoatService.LHFandGaps(Freshness.AnyCached, game, excludeGame);
            }

            var myLHfs = _SteamBoatService.GetLHFS(game);
            if (excludeGame != null) 
            {
                myLHfs = myLHfs.Where(x => x.Game != excludeGame).ToList();
            }

            myLHfs = myLHfs.Where(w => w.ThrottledOUT == false).ToList();

            return View(myLHfs);
        }
        public IActionResult sharktest() 
        {
            var sharkme = _context.Items.Where(i => i.hash_name_key == "Battle-Scarred AKS47").SingleOrDefault();
            return Content(sharkSingleItem(sharkme));
        }
        public IActionResult shark() 
        {
            var sharkitems = _context.Items.Where(w => w.StartingPrice < _config.GetValue<int>("SteamBoat:MinStartingPrice") || w.StartingPrice > _config.GetValue<int>("SteamBoat:MaxStartingPrice")).OrderBy(o => o.lastSharked).ToList();
            foreach (var sharkitem in sharkitems) 
            {
                sharkSingleItem(sharkitem);


            }
            
            return Content("OK");
        }

        public string sharkSingleItem(Item sharkitem)
        {
            _SteamBoatService.UpdateStatsforItem(sharkitem, Freshness.Hour1);
            _SteamBoatService.ActivityUpdateSingle(sharkitem);
            sharkitem.lastSharked = DateTime.Now;
            Console.WriteLine(sharkitem.ItemPageURL + " price = " + sharkitem.StartingPrice + " shark = " + sharkitem.SharkMaxPrice);
            _context.SaveChanges();
            if (sharkitem.SharkMaxPrice > sharkitem.StartingPrice)
            {
                return "Shark";
            }
            return "NOShark";
        }

        public IActionResult updatestatssingleitem() 
        {
            var myitem = _context.Items.Where(x => x.hash_name_key == "Bats").SingleOrDefault();
            _SteamBoatService.ActivityUpdateSingle(myitem);
            _context.SaveChanges();
            return Content("OK");
        }
       
        
        public IActionResult randsharkdates()
        {
            var items = _context.Items.ToList();

            foreach (var sitem in items) 
            {
                Random rnd = new Random();
                int addtime = rnd.Next(1, 300);
                sitem.lastSharked = DateTime.Now.AddMinutes(addtime);
            
            }
            _context.SaveChanges();
            return Content("OK");

        }
            public IActionResult x()
        {

            for (int i = 200; i < 3000; i = i + 100)
            {
                Console.WriteLine(i);
                var feed = new FeederUrl();
                feed.MissionId = 2005;
                feed.isJSON = true;
                feed.Url = "https://steamcommunity.com/market/search/render/?q=&start=" + i.ToString() + "&count=100&appid=252490&sort_column=price&currency=2&norender=1";
                   _context.FeederUrl.Add(feed);
                   _context.SaveChanges();
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
                string res = _SteamBoatService.LHFandGaps(Freshness.Fresh, null, null);
            }

            var myGaps = _SteamBoatService.GetGaps();

            return View(myGaps);
        }

        public IActionResult Index()
        {

            //  var x = increaseintbypercent(123, 10);

            //return RedirectToAction("Hour24");
            //   var allItems = _context.Items.ToList();
            //    foreach (var myItem in allItems) 
            //    {
            //        _SteamBoatService.UpdateStatsforItem(myItem, Freshness.AnyCached);
            //     }

            // 
            //   _context.SaveChanges();

            //var lastsale = _context.Transactions.Where(w => w.Game_hash_name_key == "524220-2B (Foil Trading Card)" && w.type.ToString() == "-").OrderByDescending(o => o.DateT).Take(1).SingleOrDefault();
            //var lastbuy = _context.Transactions.Where(w => w.Game_hash_name_key == "524220-2B (Foil Trading Card)" && w.type.ToString() == "+").OrderByDescending(o => o.DateT).Take(1).SingleOrDefault();
            //var likelybuy = _context.Transactions.Where(t => t.DateT <= lastsale.DateT.AddDays(-6) && t.type.ToString() == "+" && t.Game_hash_name_key == "524220-2B (Foil Trading Card)").OrderByDescending(o => o.DateT).Take(1).SingleOrDefault();
            //var buys = _context.Transactions.Where(t => t.DateT < DateTime.Now.AddDays(-6) && t.type.ToString() == "+" && t.Game_hash_name_key == "524220-2B (Foil Trading Card)").OrderByDescending(o => o.DateT).ToList();



            return View();


        }

        public IActionResult PostBids(string game, string excludeGame, bool justCancells = false)
        {
            _SteamBoatService.PostBids(game, excludeGame, justCancells);

            return Content("OK");
        }
        public IActionResult PostSells()
        {

            _SteamBoatService.PostSells();


            return Content("OK");
        }

        public IActionResult Hour24()
        {


            var sells24 = _context.Transactions.Where(t => t.DateT > DateTime.Now.AddHours(-24) && t.type.ToString() == "-").ToList();
            //var sells24 = _context.Transactions.Where(t => t.DateT == DateTime.Today && t.type.ToString() == "-").ToList();

            //var trans24 = _context.Transactions.Where(t => t.type.ToString() == "-").ToList();
            int sale_cnt = 0;
            var intSumofSales = 0;
            var intSumofProf = 0;
            foreach (var sells in sells24)
            {
                var myItem = _context.Items.Where(i => i.hash_name_key == sells.Game_hash_name_key).SingleOrDefault();
                myItem.LastNumberSold = 0;
            }


                foreach (var sells in sells24)
            {

                //REMEMBER: SOME ITEMS MIGHT HAVE MULTIPLE SALES SO THE NUMBER OF CARDS
                //RETURNED MIGHT NOT MATCH COUNTS


                //find likely buy
                var buys = _context.Transactions.Where(t => t.DateT < DateTime.Now.AddDays(-6) && t.type.ToString() == "+" && t.Game_hash_name_key == sells.Game_hash_name_key).OrderByDescending(o => o.DateT).ToList();
                if (buys.Count() < 1)
                {

                    //ERROR!!!!
                    Console.WriteLine("** ERROR HAVE A SELL BUT NO BUY!");
                    throw new Exception("EROR");
                }

                var myItem = _context.Items.Where(i => i.hash_name_key == sells.Game_hash_name_key).SingleOrDefault();
                if (myItem.hash_name_key.Contains("1449850")) 
                { 
                
                }
                myItem.LastSellDate = sells.DateT;
                myItem.LastSellInt = sells.int_sale_price_after_fees;
                myItem.LastProfitInt = sells.int_sale_price_after_fees - buys[0].int_sale_price_after_fees;
                myItem.LastNumberSold = myItem.LastNumberSold + 1;
                Console.WriteLine("Item: " + sale_cnt.ToString());

                intSumofSales = intSumofSales + myItem.LastSellInt;
                intSumofProf = intSumofProf + myItem.LastProfitInt;
                sale_cnt++;
                _context.SaveChanges();
               // Console.WriteLine(buys.Count());
            }


            var myItems = _context.Items.Where(l => l.LastSellDate == DateTime.Today).ToList();

            ViewBag.NumberofSales = sale_cnt;

           

         
            decimal dec = Convert.ToDecimal(intSumofProf);
            dec = dec / 100;
            ViewBag.SumofProfStr = "£" + dec.ToString();
            ViewBag.SumofProfInt = intSumofProf;

            var AveProfit = (int)((double)intSumofProf / (double)myItems.Count());


            decimal dec2 = Convert.ToDecimal(AveProfit);
            dec2 = dec2 / 100;


            ViewBag.AveProfitStr = "£" + String.Format("{0:0.00}", dec2);

            ViewBag.AveProfitInt = AveProfit;



            return View(myItems);

        }


        public IActionResult AutoSell()
        {

            //Calculate min sell price
            var myItems = _context.Items.ToList();
            {
                foreach (var saleItem in myItems)
                {
                    var lastsale = _context.Transactions.Where(w => w.Game_hash_name_key == saleItem.hash_name_key && w.type.ToString() == "-").OrderByDescending(o => o.DateT).Take(1).SingleOrDefault();
                    var lastbuy = _context.Transactions.Where(w => w.Game_hash_name_key == saleItem.hash_name_key && w.type.ToString() == "+").OrderByDescending(o => o.DateT).Take(1).SingleOrDefault();
                    saleItem.minSellPrice = 0;
                    
                    if (lastsale != null)
                    {
                        // var x = lastsale.DateT.AddDays(-6);
                        // var y = _context.Transactions.Where(t => t.DateT <= lastsale.DateT.AddDays(-6) && t.type.ToString() == "+" && t.Game_hash_name_key == lastsale.Game_hash_name_key).OrderByDescending(o => o.DateT).ToList();
                        saleItem.LastSellInt = lastsale.int_sale_price_after_fees;
                        var likelybuy = _context.Transactions.Where(t => t.DateT <= lastsale.DateT.AddDays(-6) && t.type.ToString() == "+" && t.Game_hash_name_key == lastsale.Game_hash_name_key).OrderByDescending(o => o.DateT).Take(1).SingleOrDefault();
                        saleItem.LastSellInt = lastsale.int_sale_price_after_fees;
                        saleItem.LastSaleBuyPrice = likelybuy.int_sale_price_after_fees;
                    }

                    if (lastbuy != null) 
                    {
                        //set min sale price
                        //let cheaper items go for a small loss
                        int minSell = 0;
                        if (saleItem.StartingPrice < 75)
                        {
                            //let cheper items go for a small loss -75p
                            minSell = increaseintbypercent(lastbuy.int_sale_price_after_fees, 7);
                        }
                        else 
                        {
                            //this should ensure a profit or break even on 75p + items
                            minSell = increaseintbypercent(lastbuy.int_sale_price_after_fees, 13);
                        }
                        saleItem.minSellPrice = minSell;
                        saleItem.LastBuyPrice = lastbuy.int_sale_price_after_fees;

                        Console.WriteLine(saleItem.hash_name_key + "last buy = " + lastbuy.DateT.ToShortDateString() + " " + lastbuy.sale_price_after_fees.ToString() + "min sale = " + saleItem.minSellPrice.ToString());
                    }
                   
                    




                }
            }





            _context.SaveChanges();






                var allItems = _context.Items.ToList();
            foreach (var myItem in allItems)
            {
                myItem.IdealSell_Notes = "";
                myItem.IdealSellInt = 0;
                myItem.IdealSellStr = "";
                var currentbids = new List<bids>();


                //GET BIDS AND WORK OUT LHF
                currentbids.Add(new bids() { bid_order = 1, bid_int = myItem.sell1Price, bid_quant = myItem.bid1Quant, bid_fruit = (int)(((double)myItem.sell2Price - (double)myItem.sell1Price) / (double)myItem.sell2Price * 100) });
                currentbids.Add(new bids() { bid_order = 2, bid_int = myItem.sell2Price, bid_quant = myItem.bid2Quant, bid_fruit = (int)(((double)myItem.sell3Price - (double)myItem.sell2Price) / (double)myItem.sell3Price * 100) });
                currentbids.Add(new bids() { bid_order = 3, bid_int = myItem.sell3Price, bid_quant = myItem.bid3Quant });
                currentbids.Add(new bids() { bid_order = 4, bid_int = myItem.sell4Price, bid_quant = myItem.bid4Quant });
                currentbids.Add(new bids() { bid_order = 5, bid_int = myItem.sell5Price, bid_quant = myItem.bid5Quant });



                Console.WriteLine(myItem.Name + "\t Sell : " + currentbids[0].bid_int + "(" + currentbids[0].bid_quant + ")," + currentbids[1].bid_int + "(" + currentbids[1].bid_quant + ")," + currentbids[2].bid_int + "(" + currentbids[2].bid_quant + ")," + currentbids[3].bid_int + "(" + currentbids[3].bid_quant + ")," + currentbids[4].bid_int + "(" + currentbids[4].bid_quant + ")," + "\t : Fruits = \t" + currentbids[0].bid_fruit + "," + currentbids[1].bid_fruit);


                //remove my sales
                var mySales = _context.ItemsForSale.Where(w => w.Game_hash_name_key == myItem.hash_name_key).OrderBy(o => o.sale_price).ToList();

                if (mySales.Count > 1)
                {
                    foreach (var mySale in mySales)
                    {

                        var myMatch = currentbids.Where(b => b.bid_int == mySale.sale_price).SingleOrDefault();
                        if (myMatch != null)
                        {
                            Console.WriteLine("Found a Sell(s) thats mine, removing ...");
                            myMatch.bid_quant = myMatch.bid_quant - 1;
                            Console.WriteLine(myItem.Name + "\t Sell : " + currentbids[0].bid_int + "(" + currentbids[0].bid_quant + ")," + currentbids[1].bid_int + "(" + currentbids[1].bid_quant + ")," + currentbids[2].bid_int + "(" + currentbids[2].bid_quant + ")," + currentbids[3].bid_int + "(" + currentbids[3].bid_quant + ")," + currentbids[4].bid_int + "(" + currentbids[4].bid_quant + ")," + "\t : Fruits = \t" + currentbids[0].bid_fruit + "," + currentbids[1].bid_fruit);

                        }



                    }

                }

                //Decide the price

                var myPrice = 0;
                int cnt = 0;



                do
                {
                    var tryThis = currentbids[cnt];

                    if (FruitGood(myItem, tryThis.bid_fruit))
                    {
                        //fruit is good
                        if (WallGood(myItem, tryThis.bid_quant))
                        {
                            //prepared to share top spot
                            myPrice = tryThis.bid_int;
                        }
                        else
                        {
                            //not prepared to share
                            myPrice = tryThis.bid_int - 1;
                        }

                    }
                    else
                    {
                        //failed fruiti!
                    }

                    cnt++;
                    if (cnt == 5)
                    {
                        //EEK no price!


                    }

                } while (myPrice == 0);


                //check price is not lower than min price
                if (myPrice < myItem.minSellPrice)
                {

                    if (!_config.GetValue<bool>("SteamBoat:AllowLosses"))
                    {
                        //will sell at a loss
                        //so dont sell dont sell at a loss
                        myItem.onHoldPriceToolLow = true;
                        Console.WriteLine("Price too low to sell : " + myItem.hash_name_key);
                    }
                    else {
                        myItem.onHoldPriceToolLow = false;
                        Console.WriteLine("!!!!!!!!!!!!!!!!!! Selling at a loss : " + myItem.hash_name_key);
                    }

                } else 
                {
                    //price good to sell
                    myItem.onHoldPriceToolLow = false;
                }

                    
                    myItem.IdealSellInt = myPrice;


                    //Got a sell make a str
                    decimal dec = Convert.ToDecimal(myItem.IdealSellInt);
                    dec = dec / 100;
                    myItem.IdealSellStr = dec.ToString();

            }
            _context.SaveChanges();
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

                if (myItem.tempBuyHold == false)
                {

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
                            myItem.MyValuation = myItem.IdealBidInt;

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
                                            myItem.MyValuation = myItem.IdealBidInt;
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
                                            myItem.MyValuation = myItem.IdealBidInt;
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
                                        myItem.MyValuation = myItem.IdealBidInt;
                                    }
                                    else
                                    {
                                        //DONT ALREADY HAVE A BID
                                        //NEW BID
                                        myItem.IdealBid_Notes += " NEW BID | ";
                                        myItem.IdealBidInt = myItem.bid1Price + 1;
                                        myItem.MyValuation = myItem.IdealBidInt;
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
                        if (myItem.IdealBidInt > 900 || myItem.IdealBidInt < 10)
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


                        //look for bid anonomlies or shenanagens
                      //  _SteamBoatService.bidcheck(myItem);



                        //has bid remnanined the same?
                        if (myItem.IdealBidInt == myItem.bid_price)
                        {
                            myItem.IdealBid_Notes += " BID PRICE IS SAME AS START | ";
                            myItem.IdealBidInt = 0;
                            myItem.IdealBidStr = "";

                        }

                        //get rid of excludes
                        foreach (var excl in _context.exclude.ToList())
                        {
                            if (myItem.Game == excl.Game)
                            {
                                myItem.IdealBid_Notes += " EXCLUDED GAME REMOVED | ";
                                myItem.IdealBidInt = 0;
                                myItem.IdealBidStr = "";

                            }


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
            else
                {
                    myItem.IdealBid_Notes += " EXCLUDED ON BUY HOLD | ";
                    myItem.IdealBidInt = 0;
                    myItem.IdealBidStr = "";

                }

            }





            _context.SaveChanges();

            //stop any bids on excluded games
            var gamestoexclude = _context.exclude.ToList();
            foreach (var excludeme in gamestoexclude)
            {
                var matches = _context.Items.Where(g => g.Game == excludeme.Game).ToList();
                foreach (var delme in matches)
                {

                    delme.IdealBidInt = 0;
                    delme.IdealBidStr = "";

                }


            }
            _context.SaveChanges();


            var allItems2 = _context.Items.Where(ip => ip.IdealBidInt > 0 || ip.CancelCurrentBid == true).ToList();
            return View(allItems2);
        }



        bool GapGood(int myGap, Item myItem)
        {
            int handicap = 0;
            if (myItem.Game == "Rust") 
            {
                handicap = 2;
            }

            // 30 - 70
            if (myItem.StartingPrice <= 70)
            {
                if (myGap >= 26 + handicap)
                {
                    return true;

                }
                else
                {
                    myItem.IdealBid_Notes += " GAP TOO SMALL | ";
                    return false;

                }
            }
            // 70 - 100
            if (myItem.StartingPrice <= 100)
            {
                if (myGap >= 25 + handicap)
                {
                    return true;

                }
                else
                {
                    myItem.IdealBid_Notes += " GAP TOO SMALL | ";
                    return false;

                }
            }
            // 100 - 250
            if (myItem.StartingPrice <= 250)
            {
                if (myGap >= 24 + handicap)
                {
                    return true;

                }
                else
                {
                    myItem.IdealBid_Notes += " GAP TOO SMALL | ";
                    return false;

                }
            }

            // 250 +
            if (myGap >= 23 + handicap)
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

            //foreach (var myOUTitem in myItems.Where(w => w.StartingPrice < _config.GetValue<int>("SteamBoat:MinStartingPrice") || w.StartingPrice > _config.GetValue<int>("SteamBoat:MaxStartingPrice")).ToList())
                if (myItem.StartingPrice > _config.GetValue<int>("SteamBoat:MinStartingPrice") && myItem.StartingPrice < _config.GetValue<int>("SteamBoat:MaxStartingPrice"))
            {
                return true;
            }
            else
            {
                myItem.IdealBid_Notes += " OUT OF PRICE RANGE | ";
                return false;
            }
        }



        bool WallGood(Item myItem, int quant)
        {

            if (quant == 1 && myItem.Activity > 29)
            {
                return true;

            }
            if (quant == 2 && myItem.Activity > 60)
            {
                return true;

            }
            if (quant == 3 && myItem.Activity > 120)
            {
                return true;

            }
            if (quant == 4 && myItem.Activity > 200)
            {
                return true;

            }
            if (quant < 8 && myItem.Activity > 300)
            {
                return true;

            }

            return false;

        }




        bool FruitGood(Item myItem, int sellfruit)
        {
            if (myItem.StartingPrice < 65)
            {
                //dont care about fruit at this level
                return true;
            }

            if (sellfruit < 10)
            {

                return true;
            }

            return false;

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
                Console.WriteLine("222only one page of sell listings (less than 100)");

            }
            //THIS is optiopnal third page
            try
            {
                var fileloc3 = Path.Combine(Directory.GetCurrentDirectory(), "downloads//Selling", "download3.json");
                string myHistory3 = System.IO.File.ReadAllText(fileloc3);
                var myJSOObject3 = JObject.Parse(myHistory3);
                var arrayofitems3 = myJSOObject3.SelectToken("results_html");
                combined = combined + arrayofitems3.ToString();
            }
            catch
            {
                Console.WriteLine("333only one page of sell listings (less than 100)");

            }
            //THIS is optiopnal fourth page
            try
            {
                var fileloc4 = Path.Combine(Directory.GetCurrentDirectory(), "downloads//Selling", "download4.json");
                string myHistory4 = System.IO.File.ReadAllText(fileloc4);
                var myJSOObject4 = JObject.Parse(myHistory4);
                var arrayofitems4 = myJSOObject4.SelectToken("results_html");
                combined = combined + arrayofitems4.ToString();
            }
            catch
            {
                Console.WriteLine("444only one page of sell listings (less than 100)");

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

            _SteamBoatService.ActivityUpdateAll();
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
            return RedirectToAction("Hour24");
            //return Content("OK");


        }

        public IActionResult UpdateTrans() {
            
            
            _SteamBoatService.UpdateTrans();
            return RedirectToAction("Hour24");

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
        private int increaseintbypercent(int number, int percent) 
        {
            double dnumber = (double)number;
            var increase = dnumber / 100;
            double bigger = (double)number + (increase * percent);
            int returnint = (int)bigger;


            return returnint;
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
