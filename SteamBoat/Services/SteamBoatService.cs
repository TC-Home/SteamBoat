using ContentGrabber.DataAccess.Interfaces;
using ContentGrabber.Interfaces;
using ContentGrabber.Models;
using ContentGrabber.Models.TypeSafeEnum;
using SteamBoat.Data;
using SteamBoat.Interfaces;
using SteamBoat.Models;
using SteamBoat.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace SteamBoat.Services
{
    public class SteamBoatService : ISteamBoatService
    {


        private readonly IContentGrabberDataService _ContentGrabberDataService;
        private readonly IContentGrabberService _ContentGrabberService;
        private readonly SteamBoatContext _context;

        public SteamBoatService(SteamBoatContext context, IContentGrabberDataService ContentGrabberDataService, IContentGrabberService ContentGrabberService)
        {
            
            _ContentGrabberDataService = ContentGrabberDataService;
            _ContentGrabberService = ContentGrabberService;
            _context = context;

        }

        public missionReportVM doMission(int MissionId)
        {
            //default freshness is 24hr
            return doMission(MissionId, Freshness.Hour24);
        }
        public missionReportVM doMission(int MissionId, Freshness Freshness) 
        {
            //for report back after mission
            var mymissionReport = new missionReportVM();

            //get mission
            var mymission = _context.Mission.Where(m => m.MissionId == MissionId).SingleOrDefault();
            if (mymission == null) {
                //Couldnt find mission
                mymissionReport.Errors.Add("Couldnt Find Mission : " + MissionId.ToString());
                mymissionReport.Fail = true;
                return mymissionReport;
                };

            //we have a mission
            //Grap all the pages/jsons in the mission-feederurls
            var myUrls = _context.FeederUrl.Where(u => u.MissionId == mymission.MissionId);

            if (!myUrls.Any()) 
            {
                //got no feeder URLS
                //Couldnt find mission
                mymissionReport.Errors.Add("ERR 3: Found Mission but had no Feeder URLS : " + MissionId.ToString());
                mymissionReport.Fail = true;
                return mymissionReport;
            }
            //got mission and feeder URLs
            foreach (var myFeederUrl in myUrls.ToList()) 
            {
                if (myFeederUrl.isJSON)
                {
                    var GrabbedJSON = _ContentGrabberService.GrabMeJSON(myFeederUrl.Url, Freshness);
                    if (GrabbedJSON.Fail)
                    {
                        //problem grabbing url
                        mymissionReport.Errors.Add("ERR 4: Problem grabbing this URL : " + myFeederUrl.Url);
                        mymissionReport.Fail = true;
                    }
                    else
                    {
                        //grabbed url OK
                        GetItemsfromJSON(GrabbedJSON, mymission, mymissionReport, Freshness);

                    }
                }
                else 
                {
                    //HTTP
                    var Grabbed = _ContentGrabberService.GrabMe(myFeederUrl.Url, Freshness);
                    if (Grabbed.Fail)
                    {
                        //problem grabbing url
                        mymissionReport.Errors.Add("ERR 8: Problem grabbing this URL : " + myFeederUrl.Url);
                        mymissionReport.Fail = true;
                    }
                    else
                    {
                        //grabbed url OK
                        //GetItemsfromJSON(Grabbed, mymission, mymissionReport);

                    }

                }
            }
            Console.WriteLine("DONE ################################################################################");


            return mymissionReport;

        }

        public missionReportVM GetItemsfromJSON(GrabResult GrabbedJSON, Mission Mission, missionReportVM MissionReport, Freshness Freshness) 
        {
            //check we are logged in and not getting dollars
            if (GrabbedJSON.JSON.Contains("sale_price_text\":\"$")) 
            {
                //Warning: Prices are in Dollars
                
                MissionReport.Errors.Add("WARNING 1: Prices are in dollars.");
                
            }
            var myJSOObject = JObject.Parse(GrabbedJSON.JSON);
            var arrayofitems = myJSOObject.SelectToken("results");

            if (arrayofitems.Count() < 1) 
            {
                MissionReport.Fail = true;
                MissionReport.Errors.Add("ERR 2: There were no items found?");
                return MissionReport;
            }
            //Good to go
            foreach (var item in arrayofitems) 
            {
                var x = arrayofitems.Count();
                var sell_listings = item.SelectToken("sell_listings").Value<string>();
                var sell_price = item.SelectToken("sell_price").Value<string>();
                var name = item.SelectToken("name").Value<string>();
                var sell_price_text = item.SelectToken("sell_price_text").Value<string>();
                // Console.WriteLine(name + " " + sell_price_text + " (" + sell_listings + ")");
                var assets = item.SelectToken("asset_description");
                var URL  = assets.SelectToken("icon_url").Value<string>();



          


                if (Int32.Parse(sell_listings) > Mission.ListingsMin) 
                {
                    if (Int32.Parse(sell_listings) < Mission.ListingsMax) 
                    {
                        if (Int32.Parse(sell_price) > Mission.PriceMin) 
                        {

                            if (Int32.Parse(sell_price) < Mission.PriceMax)
                            {

                                
                                Console.WriteLine("Adding url to report : " + name + " " + sell_price_text + " ("+ sell_listings + ")");
                                MissionReport.urls.Add(Mission.ItemUrl + item.SelectToken("hash_name"));

                                CreateUpdateItemPage(item.SelectToken("hash_name").Value<string>(),  Mission.ItemUrl, MissionReport, Int32.Parse(sell_price), URL);

                                //var itempage = _ContentGrabberService.GrabMe(Mission.ItemUrl + item.SelectToken("hash_name"),Freshness);
                                //ParseItem(item.SelectToken("hash_name").Value<string>(),itempage.HTML, Mission, MissionReport);

                            }                        
                        }



                    }



                }



            }
            



           
            return MissionReport;
        
        }


        public missionReportVM CreateUpdateItemPage(string hash_name, string itemUrl, missionReportVM MissionReport, int sellprice, string imageurl, string fullitemURL = null)
        {

            //do we have the item
            var myItem = _context.Items.Where(i => i.hash_name_key == hash_name).SingleOrDefault();
            if (myItem != null)
            {
                Console.WriteLine("Already got item_nameid - got grabbing page");
                //we have the item
                //myItem.StartingPrice = sellprice;
          
                //_context.SaveChanges();

            }
            else
            {

                //we dont have the item, grab it from NET
                Console.WriteLine("Need item_nameid - getting page | ADDINg ITEM");
                //anycached becasue there defo shouldnt be one anyway

                GrabResult itemGrab = null;

                if (fullitemURL != null)
                {
                    itemGrab = _ContentGrabberService.GrabMe(fullitemURL, Freshness.AnyCached);
                }
                else
                {
                    itemGrab = _ContentGrabberService.GrabMe(itemUrl + hash_name.Replace("#", "%23"), Freshness.AnyCached);
                }


                //PARSE IT
                var myNewItem = new Item();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(itemGrab.HTML);
                var pagecontents = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='mainContents']");
                HtmlNodeCollection childnodes = pagecontents.ChildNodes;


                var ClassToGet = "market_listing_nav";
                var crumbs = pagecontents.SelectNodes("//div[@class='" + ClassToGet + "']").First();
                var crumbItems = crumbs.Descendants("#Text").ToArray();

                //Get name and game
                myNewItem.hash_name_key = hash_name;
                myNewItem.Game = crumbItems[3].InnerText;
                try
                {
                    myNewItem.Name = crumbItems[5].InnerText;
                }
                catch 
                {
                    //non steam game has differnt crumbs
                    myNewItem.Name = crumbItems[3].InnerText;
                    myNewItem.Game = crumbItems[1].InnerText;
                }

                //get number for sale
                string item_nameid = getBetween(itemGrab.HTML, "Market_LoadOrderSpread( ", " );	// initial load");
                myNewItem.item_nameid = item_nameid;

                string trades = getBetween(itemGrab.HTML, "var line1=", "g_timePriceHistoryEarliest");
                myNewItem.ItemPageURL = itemGrab.Url;
               var tradescount = trades.Split("2022").Count();
                myNewItem.Activity = tradescount;

                myNewItem.ItemStatsURL = "https://steamcommunity.com/market/itemordershistogram?country=GB&language=english&currency=2&item_nameid=" + item_nameid + "&two_factor=0&norender=1";
                myNewItem.StartingPrice = sellprice;

                //GET THE THUMB

                try 
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://community.cloudflare.steamstatic.com/economy/image/" + imageurl + "/100fx116f", "wwwroot/itemimages/" + hash_name.Replace(":", " ") + ".png");
                    } 
                }
                catch { }


                //UPDATE THE STATS ON THE NEW ITEM

                UpdateStatsforItem(myNewItem, Freshness.Fresh);
                _context.Items.Add(myNewItem);
                
                _context.SaveChanges();
                
                

                } 



            return MissionReport;
        }
        public string LHFandGaps(Freshness freshness)
        {
        
            var myItems = _context.Items.Include("ItemsForSale").OrderByDescending(o => o.StartingPrice).ToList();
            foreach (var myItem in myItems.ToList()) 
            {
                if (myItem.ItemsForSale.Count > 0) 
                {
                
                };
                UpdateStatsforItem(myItem, freshness);

                _context.SaveChanges();




            }
            return "OK";
        }

        string UpdateStatsforItem(Item myItem, Freshness freshness) 
        {
            
            var myStats = _ContentGrabberService.GrabMeJSON(myItem.ItemStatsURL, freshness, 0);


            var myJSOObject = JObject.Parse(myStats.JSON);

            var obj_linq = myJSOObject.Cast<KeyValuePair<string, JToken>>();

            if (obj_linq.Count() == 1)
            {
                Console.WriteLine(" ###### GOT A BAD READ : " + myItem.hash_name_key + " " + myItem.Name);
                return "OK";

            }

            var buys = obj_linq.Where(k => k.Key == "buy_order_graph").FirstOrDefault().Value;
            var sells = obj_linq.Where(k => k.Key == "sell_order_graph").FirstOrDefault().Value;
            var buy_order_count = obj_linq.Where(k => k.Key == "buy_order_count").FirstOrDefault().Value;
            var sell_order_count = obj_linq.Where(k => k.Key == "sell_order_count").FirstOrDefault().Value;



            var buysOTable = obj_linq.Where(k => k.Key == "buy_order_table").FirstOrDefault().Value;
            var buytable = "";
            int cnt = 0;
            foreach (var buyrows in buysOTable) 
            {
                if (cnt < 5)
                {
                    
                    var price = buyrows["price"];
                    var quant = buyrows["quantity"];

                    if ((string)price == myItem.bid_price_in_pound) 
                    {

                        price = "<span class=\"highlight\">" + price + "</span>";
                    }

                    buytable = buytable + price + " - " + quant + "<br />";
                    cnt++;
                }
            
            }

            var sellsOTable = obj_linq.Where(k => k.Key == "sell_order_table").FirstOrDefault().Value;
            var selltable = "";
            cnt = 0;
            foreach (var sellrows in sellsOTable)
            {
                if (cnt < 5)
                {

                    var price = sellrows["price"];
                    var quant = sellrows["quantity"];

                    var mysaleitemchecklist = "";

                    if (myItem.ItemsForSale.Count > 0)
                    {

                    };
                    foreach (var mysaleitem in myItem.ItemsForSale) 
                    {
                        mysaleitemchecklist = mysaleitemchecklist + mysaleitem.sell_price_without_fees;
                    }


                    if (mysaleitemchecklist.Contains((string)price))
                    {

                        price = "<span class=\"highlight\">" + price + "</span>";
                    }

                    selltable = selltable + price + " - " + quant + "<br />";
                    cnt++;
                }

            }



            //Console.WriteLine(buytable);
            //Console.WriteLine(selltable);
            //var x = ((JValue)buys[0][0]).Value.GetType();


            double next_max_buy_price = 0;
            double next_min_sell_price = 0;
            double max_buy_price = 0;
            double min_sell_price = 0;

            try
            {
                max_buy_price = Convert.ToDouble(((JValue)buys[0][0]).Value);
            } 
            catch { }
            try
            {
                next_max_buy_price = Convert.ToDouble(((JValue)buys[1][0]).Value);
            }
            catch  { }
            try
            {
              min_sell_price = Convert.ToDouble(((JValue)sells[0][0]).Value);
            }
            catch { }
            try
            {
                next_min_sell_price = Convert.ToDouble(((JValue)sells[1][0]).Value);
            }
            catch { }


            int int_buy_order_count = int.Parse(buy_order_count.ToString().Replace(",", ""));
            int int_sell_order_count = int.Parse(sell_order_count.ToString().Replace(",", ""));
            int int_max_buy_price = (int)(max_buy_price * 100);
            int int_next_max_buy_price = (int)(next_max_buy_price * 100);
            int int_min_sell_price = (int)(min_sell_price * 100);
            int int_next_min_sell_price = (int)(next_min_sell_price * 100);

            myItem.orders = int_buy_order_count + int_sell_order_count;
            myItem.max_buy_price = int_max_buy_price;
            myItem.min_sell_price = int_min_sell_price;
            myItem.next_min_sell_price = int_next_min_sell_price;
            myItem.StartingPrice = int_min_sell_price;

            var lhf = ((double)int_next_min_sell_price - (double)int_min_sell_price) / (double)int_next_min_sell_price * 100;
            var gap = ((double)int_min_sell_price - (double)int_max_buy_price) / (double)int_min_sell_price * 100;

            myItem.Fruit = (int)lhf;
            myItem.Gap = (int)gap;
            myItem.sells_html = selltable;
            myItem.buys_html = buytable;
            return "OK";
        }

        public List<Item> GetLHFS(int lowest = 10) 
        {

            var myLHFs = _context.Items.Where(w => w.Fruit > lowest).Where(a => a.Activity > 70).OrderByDescending(o => o.Fruit).ToList();
            return myLHFs;
        
        }
        public List<Item> GetGaps(int lowest = 10)
        {

            var myGaps = _context.Items.Where(w => w.Gap > lowest).Where(a => a.Activity > 70).OrderByDescending(o => o.Gap).ToList();
            return myGaps;

        }

        public string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "";
        }

        public int poundtocent(string pound, float? exrate) 

        {
            
            pound = pound.Replace("/n", "");
            pound = pound.Replace("/r", "");
            pound = pound.Replace("£", "");
            pound = pound.Replace(".","");
            pound = pound.Replace(",", "");
            float? cent = float.Parse(pound);
            cent = cent * exrate;
            return Convert.ToInt32(cent);







        }

        public string CleanMe(string cleanme)

        {

            cleanme = cleanme.Replace("\n", "");
            cleanme = cleanme.Replace("\r", "");
            cleanme = cleanme.Replace("\t", "");
            
            return cleanme;







        }

        public string Clean(string Dirty) 
        {


            return Dirty.Replace("\n", "").Replace("\r", "").Replace("\t", "");



        }

        public string UpdateBidPrice(string hash_name, int bid_quant, int bid_price, string bid_price_in_pound) 
        {
            var myitem = _context.Items.Where(w => w.hash_name_key == hash_name).SingleOrDefault();

            if (myitem != null)
            {
                myitem.bid_quant = bid_quant;
                myitem.bid_price = bid_price;
                myitem.bid_price_in_pound = bid_price_in_pound;
                _context.SaveChanges();

            }
            else 
            {
                Console.WriteLine("Couldnt find : " + hash_name);
            }


            return ("OK");
        }


        public List<Item> GetAllItems() 
        {
            var items = _context.Items.ToList();
            return items;
        
        }

        public List<ItemForSale> GetAllSaleItems() 
        {
            var Saleitems = _context.ItemsForSale.ToList();
            return Saleitems;

        }

        public string CheckSalePrices()
        {
            //Loop through all sale itemns and update stats from items.
            //Run after doing a fresh LHF

            var saleitems = _context.ItemsForSale.ToList();

            foreach (var si in saleitems) 
            {

                var myItem = _context.Items.Where(h => h.hash_name_key == si.Game_hash_name_key ).SingleOrDefault();
                if (myItem != null) 
                {


                    si.max_buy_bid = myItem.max_buy_price;


                    var diff = ((double)si.sale_price - (double)si.max_buy_bid) / (double)si.sale_price * 100;
                    si.max_buy_bid_diff = (int)diff;

                    var diff2 = ((double)si.sale_price - (double)myItem.StartingPrice) / (double)si.sale_price * 100;
                    si.sale_price_diff = (int)diff2;
                    _context.SaveChanges();
                }
            
            }

            return "OK";

        }

        public string UpdateTrans()
        
        {

            var items = _context.Items.Include("Transactions").ToList();

            foreach (var item in items) 
            {

                var sale_count = item.Transactions.Where(t => t.type.ToString() == "-").Count();
                var buy_count = item.Transactions.Where(t => t.type.ToString() == "+").Count();

                if (sale_count > buy_count) 
                {

                    Console.WriteLine("ERROR !!! SOLD MORE THAN BOUGHT!!!!! DELETING" + item.Game + " buy: " + buy_count.ToString() + " | sale: " + sale_count.ToString());
                    var delthesetrans = _context.Transactions.Where(i => i.Game_hash_name_key == item.hash_name_key).ToList();
                    foreach(var t in delthesetrans) 
                    {
                        _context.Transactions.Remove(t);
                        _context.SaveChanges();
                    
                    }

                }


                item.total_buys = buy_count;
                item.total_sales = sale_count;


                var all_buys = item.Transactions.Where(t => t.type == '+').OrderByDescending(o => o.DateT).ToList();
                var all_sells = item.Transactions.Where(t => t.type == '-').OrderByDescending(o => o.DateT).ToList();

                var all_buys_amount = 0;
                foreach (var buy in all_buys) 
                {
                    all_buys_amount = all_buys_amount + buy.int_sale_price_after_fees;
                }
                item.total_buys_sum_amount = all_buys_amount;
                if (all_buys.Count() > 0)
                {
                    item.Ave_buy = (int)((double)all_buys_amount / (double)all_buys.Count());
                }
                else 
                {
                    item.Ave_buy = 0;
                }

                var all_sells_amount = 0;
                foreach (var sale in all_sells)
                {
                    all_sells_amount = all_sells_amount + sale.int_sale_price_after_fees;
                }
                item.total_sales_sum_amount = all_sells_amount;
                if (all_sells.Count() > 0)
                {
                    item.Ave_sell = (int)((double)all_sells_amount / (double)all_sells.Count());
                }
                else
                {
                    item.Ave_sell = 0;
                }

                if (item.Ave_sell > 0)
                {
                    // PROFIT
                    item.Ave_profic_pc = (int)(((double)item.Ave_sell - (double)item.Ave_buy) / (double)item.Ave_sell * 100);
                    item.Ave_profit = item.Ave_sell - item.Ave_buy;

                }
                else 
                {
                    item.Ave_profit = 0;
                }

                if (all_sells.Count() > 0)
                {
                    item.total_profit = all_sells_amount - all_buys_amount;
                }
                else 
                {
                    item.total_profit = 0;
                }

               // if (item.item_nameid == "176293912")
               //     {
               // 
               // }

                //get stock at sell price to calculate profit with stock
                //DOESNT WORKJ BECASUE NOT ALL ITEMS ARE ON SALE YET
                //var stock = _context.ItemsForSale.Where(i => i.Game_hash_name_key == item.hash_name_key).ToList();
                //foreach(var myStock in stock)
                // {
                //
                //     item.total_profit_including_stock = item.total_profit_including_stock + myStock.sale_price_after_fees;
                // }

                    //USE AVERAGE PRICe INSTEAD
                item.total_profit_including_stock = item.total_profit;
                var total_in_stock = item.total_buys - item.total_sales;

                item.total_profit_including_stock = item.total_profit_including_stock + (total_in_stock * item.Ave_buy);




                //Console.WriteLine("buy: " + buy_count.ToString() + " | sale: " + sale_count.ToString());


            }

            _context.SaveChanges();

            return "OK";
        
        }

        //remove all bids before updating them
        public string ClearAllBids()
        {
            var items = _context.Items.ToList();
            foreach (var item in items) 
            {
                item.bid_quant = 0;
                item.bid_price = 0;
                item.bid_price_in_pound = "";
            }
            _context.SaveChanges();
            return "OK";
        }

        public string ClearAllSales() 
        {
            var items = _context.ItemsForSale.ToList();
            foreach (var item in items)
            {
                _context.ItemsForSale.Remove(item);
            }
            _context.SaveChanges();
            return "OK";

        }

        public List<Item> GetAllItemsandSales() 
        {
            var items = _context.Items.Include("ItemsForSale").Where(i => i.ItemsForSale.Count > 0).ToList();
            return items;

        }

        public string AddSellListing(string hash_name, int sell_price_after_fees, int int_sell_price_without_fees, string sell_price_without_fees) 
        {

            var newSaleitem = new ItemForSale();
            newSaleitem.Game_hash_name_key = hash_name;
            newSaleitem.sale_price_after_fees = sell_price_after_fees;
            newSaleitem.sale_price = int_sell_price_without_fees;
            newSaleitem.sell_price_without_fees = sell_price_without_fees;
            _context.ItemsForSale.Add(newSaleitem);
            _context.SaveChanges();
            return "OK";

        }

        public string GetGameHashNamefromItemandGame(string Item, string Game) 
        {

            Game = Game.Replace(" Foil Trading Card", "");
            Game = Game.Replace(" Rare Emoticon", "");
            Game = Game.Replace(" Trading Card", "");
            Game = Game.Replace(" (Trading Card)", "");
            Game = Game.Replace(": Definitive Edition", "");
            


            var item = _context.Items.Where(i => i.Name == Item).Where(g => g.Game.Contains(Game)).SingleOrDefault();
            if (item != null) 
            {
                return item.hash_name_key;
            }

            //attempt 2
            Game = Game.Replace(" & ", " &amp; ");
            item = _context.Items.Where(i => i.Name == Item).Where(g => g.Game.Contains(Game)).SingleOrDefault();
            if (item != null)
            {
                return item.hash_name_key;
            }



            return null;
        


        }
    }
}
