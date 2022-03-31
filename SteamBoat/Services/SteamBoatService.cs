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

                                CreateUpdateItemPage(item.SelectToken("hash_name").Value<string>(),  Mission, MissionReport, Int32.Parse(sell_price));

                                //var itempage = _ContentGrabberService.GrabMe(Mission.ItemUrl + item.SelectToken("hash_name"),Freshness);
                                //ParseItem(item.SelectToken("hash_name").Value<string>(),itempage.HTML, Mission, MissionReport);

                            }                        
                        }



                    }



                }



            }
            



           
            return MissionReport;
        
        }


        public missionReportVM CreateUpdateItemPage(string hash_name, Mission Mission, missionReportVM MissionReport, int sellprice)
        {

            //do we have the item
            var myItem = _context.Items.Where(i => i.hash_name_key == hash_name).SingleOrDefault();
            if (myItem != null)
            {
                Console.WriteLine("Already got item_nameid - got grabbing page");
                //we have the item
                myItem.StartingPrice = sellprice;
          
                _context.SaveChanges();

            }
            else
            {
                //we dont have the item, grab it
                Console.WriteLine("Need item_nameid - getting page");
                //anycached becasue there defo shouldnt be one anyway
                var itemGrab = _ContentGrabberService.GrabMe(Mission.ItemUrl + hash_name, Freshness.AnyCached);


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
                myNewItem.Name = crumbItems[5].InnerText;

                //get number for sale
                string item_nameid = getBetween(itemGrab.HTML, "Market_LoadOrderSpread( ", " );	// initial load");
                myNewItem.item_nameid = item_nameid;

                string trades = getBetween(itemGrab.HTML, "var line1=", "g_timePriceHistoryEarliest");
                myNewItem.ItemPageURL = itemGrab.Url;
               var tradescount = trades.Split("2022").Count();
                myNewItem.Activity = tradescount;

                myNewItem.ItemStatsURL = "https://steamcommunity.com/market/itemordershistogram?country=GB&language=english&currency=2&item_nameid=" + item_nameid + "&two_factor=0&norender=1";
                myNewItem.StartingPrice = sellprice;

                _context.Items.Add(myNewItem);

                _context.SaveChanges();
            } 



            return MissionReport;
        }
        public string LHFandGaps(Freshness freshness)
        {
            ClearAllBids();
            var myItems = _context.Items.OrderByDescending(o => o.StartingPrice).ToList();
            foreach (var myItem in myItems.ToList()) 
            {
                var myStats = _ContentGrabberService.GrabMeJSON(myItem.ItemStatsURL, freshness, 0 );


                var myJSOObject = JObject.Parse(myStats.JSON);

                var obj_linq = myJSOObject.Cast<KeyValuePair<string, JToken>>();

                var buys =  obj_linq.Where(k => k.Key == "buy_order_graph").FirstOrDefault().Value;
                var sells = obj_linq.Where(k => k.Key == "sell_order_graph").FirstOrDefault().Value;
                var buy_order_count = obj_linq.Where(k => k.Key == "buy_order_count").FirstOrDefault().Value;
                var sell_order_count = obj_linq.Where(k => k.Key == "sell_order_count").FirstOrDefault().Value;

                var x = ((JValue)buys[0][0]).Value.GetType();

                double max_buy_price  = Convert.ToDouble(((JValue)buys[0][0]).Value);
             
                double next_max_buy_price = 0;
                double next_min_sell_price = 0;
                //double next_max_buy_price = 0;
                try
                {
                    next_max_buy_price = Convert.ToDouble(((JValue)buys[1][0]).Value);
                }
                catch { }
                double min_sell_price = Convert.ToDouble(((JValue)sells[0][0]).Value);
                try
                {
                    next_min_sell_price = Convert.ToDouble(((JValue)sells[1][0]).Value);
                }
                catch { }


                int int_buy_order_count = int.Parse(buy_order_count.ToString().Replace(",",""));
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


                //myItem.orders = 0;
                //myItem.max_buy_price = 0;
                //myItem.min_sell_price = 0;
                //myItem.next_min_sell_price = 0;



                var lhf = ((double)int_next_min_sell_price - (double)int_min_sell_price) / (double)int_next_min_sell_price * 100;
                var gap = ((double)int_min_sell_price - (double)int_max_buy_price) / (double)int_min_sell_price * 100;

                myItem.Fruit = (int)lhf;
                myItem.Gap = (int)gap;

                _context.SaveChanges();




            }
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

        string getBetween(string strSource, string strStart, string strEnd)
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
            
            }


            return ("OK");
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


    }
}
