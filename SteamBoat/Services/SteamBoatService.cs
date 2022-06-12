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
using System.IO;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Web;

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
        public missionReportVM doMission(int MissionId, Freshness Freshness, bool flip_order = false) 
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
            List<FeederUrl> myUrls = new List<FeederUrl>();
            if (flip_order)
            {
                myUrls = _context.FeederUrl.Where(u => u.MissionId == mymission.MissionId).OrderBy(o => o.FeederId).ToList();
            }
            else 
            {
                myUrls = _context.FeederUrl.Where(u => u.MissionId == mymission.MissionId).OrderByDescending(o => o.FeederId).ToList();
            }
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
                Console.WriteLine("Checking we have thumb");
                var gotthmb = File.Exists("wwwroot/itemimages/" + hash_name.Replace(":", " ") + ".png");
                Console.WriteLine("got thumnb = " + gotthmb);

                if (gotthmb == false) 
                {
                    SaveThumb(imageurl, myItem.hash_name_key);
                }
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
             
                //old ACTIVITY
                //
                //var tradescount = trades.Split("2022").Count();               
                //myNewItem.Activity = tradescount;

                //NEW ACTIVITY
                ActivityUpdateSingle(myNewItem);


                myNewItem.ItemStatsURL = "https://steamcommunity.com/market/itemordershistogram?country=GB&language=english&currency=2&item_nameid=" + item_nameid + "&two_factor=0&norender=1";
                myNewItem.StartingPrice = sellprice;

                //GET THE THUMB

                SaveThumb(imageurl, hash_name);
                // try 
                // {
                //    using (var client = new WebClient())
                //    {
                //        client.DownloadFile("https://community.cloudflare.steamstatic.com/economy/image/" + imageurl + "/100fx116f", "wwwroot/itemimages/" + hash_name.Replace(":", " ") + ".png");
                //    } 
                // }
                // catch { }


                //UPDATE THE STATS ON THE NEW ITEM

                UpdateStatsforItem(myNewItem, Freshness.Fresh);
                _context.Items.Add(myNewItem);
                
                _context.SaveChanges();
                
                

                } 



            return MissionReport;
        }

        String SaveThumb(string imageurl,string hash_name ) 
        {
            var result = "";
            var THpath = "";
            try
            {
                using (var client = new WebClient())
                {
                    THpath = "https://community.cloudflare.steamstatic.com/economy/image/" + imageurl + "/100fx116f";
                    client.DownloadFile(THpath, "wwwroot/itemimages/" + hash_name.Replace(":", " ").Replace("\"", " ").Replace("|", " ") + ".png");
                    result = "OK";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GETTING THUMB : " + THpath);
                Console.WriteLine("ERROR : " + ex.Message);
                result = "FAIL";
            }
            return result;
        }
        public string LHFandGaps(Freshness freshness)
        {
        
            var myItems = _context.Items.Include("ItemsForSale").OrderByDescending(o => o.StartingPrice).ToList();
            int tot = myItems.Count();
            int cnt = 0;

            foreach (var myItem in myItems.ToList()) 
            {
                if (myItem.ItemsForSale.Count > 0) 
                {
                
                };
                UpdateStatsforItem(myItem, freshness);
                cnt++;
                Console.WriteLine(cnt.ToString() + "/" + tot.ToString());
                _context.SaveChanges();
            }
            return "OK";
        }


        public string ActivityUpdateAll2()
        {
            //do then all
            //check all items have an activity url
            //var allitems = _context.Items.ToList();
            var allitems = _context.Items.Where(w => w.Activity > 30 && w.StartingPrice > 75).ToList();
            var tot = allitems.Count();
            var cnt = 0;
           // var allitems = _context.Items.Where(w => w.Tip_Price10 == 0 && w.Activity > 20).ToList();

            //foreach (Item item in allitems.Where(w => w.ActivityHistory == 0).ToList())
            foreach (var item in allitems.ToList())
                {
                cnt++;
                Console.WriteLine(cnt + " of " + tot);
                //update activity for single item
                ActivityUpdateSingle2(item);
                    _context.SaveChanges();

                }
                Console.WriteLine("FINISHED");
               
           

            return "OK";

        }

        public string ActivityUpdateSingle2(Item item)
        {

            var itemGrab = _ContentGrabberService.GrabMe(item.ItemPageURL, Freshness.Hour72);

            for (int i = 0; i < 5; i++)
            {
                if (itemGrab.Fail == true)
                {
                    RandomWait(10, 100);
                    //try getting a fresh one
                    Console.WriteLine("Trying = " + i);
                    itemGrab = _ContentGrabberService.GrabMe(item.ItemPageURL, Freshness.Fresh);
                }
            }
            if (itemGrab.HTML == null)
            {
         
            }
            string trades = getBetween(itemGrab.HTML, "var line1=", "g_timePriceHistoryEarliest");

            trades = trades.Replace("]];", "");
            trades = trades.Replace("[[", "");
            string[] myArray = trades.Split("],[");

            var myModRecs = new List<Activityrec>();
            foreach (var myArrayRec in myArray)
            {
                LoadArraytoMod(myArrayRec, myModRecs);
            }

            //CREATE A GROUPED ONJ
            var myModRecsGroupedBYDATE = new List<Activityrec>();

            var today = DateTime.Now.Date;
            List<float> steps = new List<float>();
            //ACTIVITY ACTIVITY ACTIVITY ACTIVITY ACTIVITY ACTIVITY ACTIVITY 
            int Activity = 0;

            for (int i = 0; i < 30; i++)
            {
                int high = 0;
                int low = 10000000;
                var searchdate = today.AddDays(i * -1);
                var Allthisday = myModRecs.Where(w => w.myDate == searchdate).ToList();
                var Cdate = new DateTime();
                float CAmount = 0f;
                float CNumber = 0f;
                Cdate = searchdate;
                if (Allthisday.Count > 0)
                {
                  
                    foreach (var PartDate in Allthisday)
                    {

                        //STEPS THROUGH ALL ON SAME DAY
                        //CAMOUNT & myAmount IS number of items
                        //Cnumber & PartDate.myNumber is price!

                        if (PartDate.myNumber > high) 
                        {
                            high = (int)PartDate.myNumber;
                        }

                        if (PartDate.myNumber < low)
                        {
                            low = (int)PartDate.myNumber;
                        }

                        CAmount = CAmount + PartDate.myAmount;
                        CNumber = CNumber + (PartDate.myNumber * PartDate.myAmount);
                        Activity = Activity + (int)PartDate.myAmount;
                        Console.WriteLine(Cdate.ToShortDateString() + " Number Sold " + PartDate.myAmount + " Price" + PartDate.myNumber + " high = " + high + " low = " + low);
                    }
                    //Ave for day
                    CNumber = CNumber / CAmount;

                }
                myModRecsGroupedBYDATE.Add(new Activityrec() { myDate = Cdate, myAmount = CAmount, myNumber = CNumber, myHigh = high, myLow = low }) ;
            }
            //RECS ARE GROUPED BY DATE
            //TRY GROUP BY BATCH

            // BATCH BATCH BATCH BATCH BATCH BATCH BATCH BATCH BATCH BATCH BATCH

            //RECS ARE GROUPED BY DATE
            //TRY GROUP BY BATCH

            var batchsize = 3;
            var batchcnt = 0;

            float BNumber = 0f;
            int batchssofar = 0;

            int batch_days_with_sales = 0;
            var thisbatch = new Activityrec();
            var batchedRecs = new List<Activitybatch>();
            var AlldaysGrouped = myModRecs.ToList();
            int maxbatchsize = 0;
            float unitsize = 0;

            for (int y = 0; y < 30; y++)
            {
                batchcnt++;
                var searchdate = today.AddDays(y * -1);
                var singleday = myModRecsGroupedBYDATE.Where(w => w.myDate == searchdate).SingleOrDefault();

                if (singleday != null)
                {
                    if (singleday.myNumber != 0)
                    {
                        BNumber = BNumber + singleday.myHigh;
                        batch_days_with_sales++;
                    }

                }
                if (batchcnt == batchsize)
                {
                    batchssofar++;
                    if (BNumber > 0 && batch_days_with_sales > 0)
                    {
                        batchedRecs.Add(new Activitybatch() { BatchNumber = batchssofar, BatchAV = (int)(BNumber / batch_days_with_sales) });
                    }
                    else
                    {
                        batchedRecs.Add(new Activitybatch() { BatchNumber = batchssofar, BatchAV = 0 });

                    }
                    if ((int)(BNumber / batch_days_with_sales) > maxbatchsize)
                    {
                        maxbatchsize = (int)(BNumber / batch_days_with_sales);
                    }
                    batchcnt = 0;
                    BNumber = 0;
                    batch_days_with_sales = 0;
                }
            }

            if (maxbatchsize == 0)
            {
                item.Activity = 0;
                item.Tip_Price1 = 0;
                item.Tip_Price2 = 0;
                item.Tip_Price3 = 0;
                item.Tip_Price4 = 0;
                item.Tip_Price5 = 0;
                item.Tip_Price6 = 0;
                item.Tip_Price7 = 0;
                item.Tip_Price8 = 0;
                item.Tip_Price9 = 0;
                item.Tip_Price10 = 0;

            }
            else
            {
                unitsize = 100 / (float)maxbatchsize;
                unitsize = 1;

                item.Activity = Activity;
                item.Tip_Price1 = centtopenny((int)(batchedRecs[9].BatchAV * unitsize));
                item.Tip_Price2 = centtopenny((int)(batchedRecs[8].BatchAV * unitsize));
                item.Tip_Price3 = centtopenny((int)(batchedRecs[7].BatchAV * unitsize));
                item.Tip_Price4 = centtopenny((int)(batchedRecs[6].BatchAV * unitsize));
                item.Tip_Price5 = centtopenny((int)(batchedRecs[5].BatchAV * unitsize));
                item.Tip_Price6 = centtopenny((int)(batchedRecs[4].BatchAV * unitsize));
                item.Tip_Price7 = centtopenny((int)(batchedRecs[3].BatchAV * unitsize)); 
                item.Tip_Price8 = centtopenny((int)(batchedRecs[2].BatchAV * unitsize));
                item.Tip_Price9 = centtopenny((int)(batchedRecs[1].BatchAV * unitsize));
                item.Tip_Price10 = centtopenny((int)(batchedRecs[0].BatchAV * unitsize));

                if (item.Tip_Price10 > 0 && item.Tip_Price9 > 0 && item.Tip_Price8 > 0)
                {
                    item.Pred_Tip_Price = (int)((((((float)(item.Tip_Price10) + ((float)(item.Tip_Price10) - (float)(item.Tip_Price9))) + ((float)(item.Tip_Price10) + ((float)(item.Tip_Price10) - (float)(item.Tip_Price8))) + ((float)(item.Tip_Price10) + ((float)(item.Tip_Price10) - (float)(item.Tip_Price7)))) / 3) + (float)(item.Tip_Price10)) / 2);
                    item.SharkMaxPrice = increaseintbypercent(item.Pred_Tip_Price, -21);
                }
                
            }

            return "OK";
        }


        public string ActivityUpdateAll(bool usedayofweek) 
        {
            //loops through all items to update activity stats

            // use day of week, updates a seventh of the total items based on day of week

            //check all items have an activity url
            //var allitems = _context.Items.ToList();
            var allitems = _context.Items.Where(w=> w.Activity > 30 && w.StartingPrice > 75).ToList();
            var tot2 = allitems.Count();
            var cnt2 = 0;

            if (usedayofweek == false)
            {
                //do then all

                //foreach (Item item in allitems.Where(w => w.ActivityHistory == 0).ToList())
                foreach (var item in allitems.ToList())
                {
                    cnt2++;
                    Console.WriteLine(cnt2 + " of " + tot2);
                    //update activity for single item
                    ActivityUpdateSingle(item);
                    _context.SaveChanges();

                }
            }
            else 
            {
                //do a seveth of the total to avoid spamming
                //the seventh selectred are based on the day of tyhe week
                int dayNumber = (int)DateTime.Now.DayOfWeek;
                int cnt = 0;
                int mod = 7;
                int tot = 0;

                foreach (var my7item in allitems.Skip(dayNumber))
                {

                    if (cnt % mod == 0)
                    {
                       ActivityUpdateSingle(my7item);
                        tot = tot + 1;
                    }
                    cnt = cnt + 1;
                    

 _context.SaveChanges();
                }
                Console.WriteLine("FINISHED");
                Console.WriteLine("Used day of week filtering. did " + tot.ToString());
            }
           
            return "OK";
        
        }

        public string ActivityUpdateSingle(Item item) 
        {

            var itemGrab = _ContentGrabberService.GrabMe(item.ItemPageURL, Freshness.AnyCached);
            if (itemGrab.HTML == null) 
            {
            
            }
            string trades = getBetween(itemGrab.HTML, "var line1=", "g_timePriceHistoryEarliest");

            trades = trades.Replace("]];", "");
            trades = trades.Replace("[[", "");
            string[] myArray = trades.Split("],[");

            var myModRecs = new List<Activityrec>();
            foreach (var myArrayRec in myArray)
            {
                LoadArraytoMod(myArrayRec, myModRecs);
            }

            //CREATE A GROUPED ONJ
            var myModRecsGroupedBYDATE = new List<Activityrec>();

            var today = DateTime.Now.Date;
            List<float> steps = new List<float>();
            //ACTIVITY ACTIVITY ACTIVITY ACTIVITY ACTIVITY ACTIVITY ACTIVITY 
            int Activity = 0;

            for (int i = 0; i < 30; i++)
            {

                var searchdate = today.AddDays(i * -1);
                var Allthisday = myModRecs.Where(w => w.myDate == searchdate).ToList();
                var Cdate = new DateTime();
                float CAmount = 0f;
                float CNumber = 0f;
                Cdate = searchdate;
                if (Allthisday.Count > 0)
                {
     
                    foreach (var PartDate in Allthisday)
                    {
                        //STEPS THROUGH ALL ON SAME DAY
                        
                        CAmount = CAmount + PartDate.myAmount;
                        CNumber = CNumber + (PartDate.myNumber * PartDate.myAmount);
                        Activity = Activity + (int)PartDate.myAmount;
                        Console.WriteLine(Cdate.ToShortDateString() + " " + PartDate.myAmount + " " + PartDate.myNumber);
                    }
                    //Ave for day
                    CNumber = CNumber / CAmount;

                }
                myModRecsGroupedBYDATE.Add(new Activityrec() { myDate = Cdate, myAmount = CAmount, myNumber = CNumber });
            }
            //RECS ARE GROUPED BY DATE
            //TRY GROUP BY BATCH

            // BATCH BATCH BATCH BATCH BATCH BATCH BATCH BATCH BATCH BATCH BATCH

            //RECS ARE GROUPED BY DATE
            //TRY GROUP BY BATCH

            var batchsize = 3;
            var batchcnt = 0;
          
            float BNumber = 0f;
            int batchssofar = 0;

            int batch_days_with_sales = 0;
            var thisbatch = new Activityrec();
            var batchedRecs = new List<Activitybatch>();
            var AlldaysGrouped = myModRecs.ToList();
            int maxbatchsize = 0;
            float unitsize = 0;

            for (int y = 0; y < 30; y++)
            {
                batchcnt++;
                var searchdate = today.AddDays(y * -1);
                var singleday = myModRecsGroupedBYDATE.Where(w => w.myDate == searchdate).SingleOrDefault();

                if (singleday != null)
                {
                    if (singleday.myNumber != 0)
                    {
                        BNumber = BNumber + singleday.myNumber;
                        batch_days_with_sales++;
                    }

                }
                if (batchcnt == batchsize)
                {
                    batchssofar++;
                    if (BNumber > 0 && batch_days_with_sales > 0)
                    {
                        batchedRecs.Add(new Activitybatch() { BatchNumber = batchssofar, BatchAV = (int)(BNumber / batch_days_with_sales) });
                    }
                    else 
                    {
                        batchedRecs.Add(new Activitybatch() { BatchNumber = batchssofar, BatchAV = 0 });

                    }
                    if ((int)(BNumber / batch_days_with_sales) > maxbatchsize) 
                    {
                        maxbatchsize = (int)(BNumber / batch_days_with_sales);
                    }
                    batchcnt = 0;
                    BNumber = 0;
                    batch_days_with_sales = 0;
                }
            }

            if (maxbatchsize == 0)
            { 
                item.Activity = 0;
                item.AH1 = 0;
                item.AH2 = 0;
                item.AH3 = 0;
                item.AH4 = 0;
                item.AH5 = 0;
                item.AH6 = 0;
                item.AH7 = 0;
                item.AH8 = 0;
                item.AH9 = 0;
                item.AH10 = 0;

            }
            else
            {
                unitsize = 100 / (float)maxbatchsize;

              
                item.Activity = Activity;
                item.AH1 = (int)(batchedRecs[9].BatchAV * unitsize);
                item.AH2 = (int)(batchedRecs[8].BatchAV * unitsize);
                item.AH3 = (int)(batchedRecs[7].BatchAV * unitsize);
                item.AH4 = (int)(batchedRecs[6].BatchAV * unitsize);
                item.AH5 = (int)(batchedRecs[5].BatchAV * unitsize);
                item.AH6 = (int)(batchedRecs[4].BatchAV * unitsize);
                item.AH7 = (int)(batchedRecs[3].BatchAV * unitsize);
                item.AH8 = (int)(batchedRecs[2].BatchAV * unitsize);
                item.AH9 = (int)(batchedRecs[1].BatchAV * unitsize);
                item.AH10 = (int)(batchedRecs[0].BatchAV * unitsize);
            }

            return "OK";
        }

        void LoadArraytoMod(string myArrayRec, List<Activityrec> myModRecs)
        {
            try
            {
                Activityrec myRec = new Activityrec();
                string[] splits = myArrayRec.Split(",");
                splits[0] = splits[0].Replace("\"", "").Substring(0, 11);
                splits[2] = splits[2].Replace("\"", "");
                myRec.myDate = DateTime.Parse(splits[0]);
                decimal dec = Convert.ToDecimal(splits[1]);
                //dec = .5M;
                decimal dec2 = decimal.Round(dec * 100, 0, MidpointRounding.AwayFromZero);
                string dec3 = dec2.ToString();
                myRec.myAmount = int.Parse(splits[2]);
                myRec.myNumber = int.Parse(dec3);
                myModRecs.Add(myRec);
            }
            catch (Exception ex)
            {

                Console.WriteLine("ERROR: LoadArraytoMod = " + ex.Message);
            
            }

        }

        private string HighlightBuyOrder(JToken buysOTable, string bid_price, int adjuster) 
        {
            //tries to find the bid prioce in the table
            //the adjuster lets you tweak bu a penny to see if you can find a match
            //if adjuster = 99 then it gives up and returns the table wihotu a highlight
            bool found_bid = false;
            int cnt = 0;
            var buytable = "";
            foreach (var buyrows in buysOTable)
            {
                if (cnt < 5)
                {

                    var price = buyrows["price"];
                    var quant = buyrows["quantity"];

                    var matchme = (string)price;
                    matchme = matchme.Replace("£0.0", "").Replace("£0.", "").Replace("£", "").Replace(".", "");
                    string mybid = ((int.Parse(bid_price) + (int)adjuster)).ToString();
                    if (matchme == mybid)
                    {

                        price = "<span class=\"highlight" + cnt + "\">" + price + "</span>";
                        found_bid = true;
                    }

                    buytable = buytable + price + " - " + quant + "<br />";
                    cnt++;
                }

            }
            if (adjuster == -99) 
            {
                return buytable;
            }
            if (found_bid == false)
            {
                return "FAIL";
            }
            return buytable;
            
        }
        public string UpdateStatsforItem(Item myItem, Freshness freshness) 
        {
           // if (myItem.Name.Contains("Apex")) 
           // { 
          //  
          //  }

            var myStats = _ContentGrabberService.GrabMeJSON(myItem.ItemStatsURL, freshness, 0);
       

                if (myStats.Fail == true)
                {
                  
                    //try again:
                    myStats = _ContentGrabberService.GrabMeJSON(myItem.ItemStatsURL, freshness, 0);

                }
            

            if (myStats.JSON == null) 
            {
                Console.WriteLine(" ###### Couldnt get Json : " + myItem.hash_name_key + " " + myItem.Name);
                return "OK";
            }

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

            string buytable = "";

            if (myItem.bid_price != 0) {

                buytable = HighlightBuyOrder(buysOTable, myItem.bid_price.ToString(), 0);

                if (buytable == "FAIL") 
                {
                    buytable = HighlightBuyOrder(buysOTable, myItem.bid_price.ToString(), -1);
                }
                if (buytable == "FAIL")
                {
                    buytable = HighlightBuyOrder(buysOTable, myItem.bid_price.ToString(), 1);
                }
                if (buytable == "FAIL")
                {
                    buytable = HighlightBuyOrder(buysOTable, myItem.bid_price.ToString(), -99);
                }


            }
            else {

                buytable = HighlightBuyOrder(buysOTable, myItem.bid_price.ToString(), -99);
                    
                    }

            
            var sellsOTable = obj_linq.Where(k => k.Key == "sell_order_table").FirstOrDefault().Value;
            var selltable = "";
            var cnt = 0;
            myItem.bid1Price = 0;
            myItem.bid1Quant = 0;
            myItem.bid2Price = 0;
            myItem.bid2Quant = 0;
            myItem.bid3Price = 0;
            myItem.bid3Quant = 0;
            myItem.bid4Price = 0;
            myItem.bid5Quant = 0;
            myItem.bid5Price = 0;
            myItem.bid5Quant = 0;



            foreach (var sellrows in sellsOTable)
            {
                if (cnt < 5)
                {

                    var price = sellrows["price"];
                    int intprice = int.Parse(price.ToString().Replace("£0.0", "").Replace("£0.", "").Replace("£", "").Replace(".", ""));
                    var quant = sellrows["quantity"];
                    int intquant = int.Parse(quant.ToString().Replace(",",""));


                    if (cnt == 0)
                    {

                        myItem.sell1Price = intprice;
                        myItem.sell1Quant = intquant;
                    }

                    if (cnt == 1)
                    {

                        myItem.sell2Price = intprice;
                        myItem.sell2Quant = intquant;
                    }
                    if (cnt == 2)
                    {

                        myItem.sell3Price = intprice;
                        myItem.sell3Quant = intquant;
                    }
                    if (cnt == 3)
                    {

                        myItem.sell4Price = intprice;
                        myItem.sell4Quant = intquant;
                    }
                    if (cnt == 4)
                    {

                        myItem.sell5Price = intprice;
                        myItem.sell5Quant = intquant;
                    }





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

                        price = "<span class=\"highlight" + cnt + "\">" + price + "</span>";
                    }

                    selltable = selltable + price + " - " + quant + "<br />";
                    cnt++;
                }

            }

           cnt = 0;
            //get the bids
            foreach (var buyrows in buysOTable)
            {
                if (cnt < 5)
                {

                    var price = buyrows["price"];
                    int intprice = int.Parse(price.ToString().Replace("£0.0", "").Replace("£0.", "").Replace("£", "").Replace(".", ""));
                    var quant = buyrows["quantity"];
                    int intquant = int.Parse(quant.ToString().Replace(",",""));


                    if (cnt == 0)
                    {

                        myItem.bid1Price = intprice;
                        myItem.bid1Quant = intquant;
                    }

                    if (cnt == 1)
                    {

                        myItem.bid2Price = intprice;
                        myItem.bid2Quant = intquant;
                    }
                    if (cnt == 2)
                    {

                        myItem.bid3Price = intprice;
                        myItem.bid3Quant = intquant;
                    }
                    if (cnt == 3)
                    {

                        myItem.bid4Price = intprice;
                        myItem.bid4Quant = intquant;
                    }
                    if (cnt == 4)
                    {

                        myItem.bid5Price = intprice;
                        myItem.bid5Quant = intquant;
                    }

                 
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
            var hhf = ((double)myItem.bid1Price - (double)myItem.bid2Price) / (double)myItem.bid1Price * 100;
            var gap = ((double)int_min_sell_price - (double)int_max_buy_price) / (double)int_min_sell_price * 100;
            var mygap = ((double)int_min_sell_price - (double)myItem.bid_price) / (double)int_min_sell_price * 100;
            

            myItem.Fruit = (int)lhf;
            myItem.Fruit2 = (int)hhf;
            myItem.Gap = (int)gap;
            myItem.Gap_mybid = (int)mygap;
            myItem.sells_html = selltable;
            myItem.buys_html = buytable;
            return "OK";
        }

        public List<Item> GetLHFS(int lowest = 10) 
        {

            var myLHFs = _context.Items.Where(w => w.Fruit > lowest).Where(a => a.Activity > 15).OrderByDescending(o => o.Fruit).ToList();
            return myLHFs;
        
        }
        public List<Item> GetGaps(int lowest = 10)
        {

            var myGaps = _context.Items.Where(w => w.Gap > lowest).Where(a => a.Activity > 40).OrderByDescending(o => o.Gap).ToList();
            return myGaps;

        }

        public string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource == null) 
            {
                return "";
            }
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

        public int centtopenny(int cent, float? exrate = 0.81f)
        {
            float? penny = (float)cent;
            penny = penny * exrate;
            return Convert.ToInt32(penny);

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
                if (item.hash_name_key == "Under the Bridge") 
                { 
                }
                var sale_count = item.Transactions.Where(t => t.type.ToString() == "-").Count();
                var buy_count = item.Transactions.Where(t => t.type.ToString() == "+").Count();

                if (sale_count > buy_count) 
                {

                    Console.WriteLine("ERROR !!! SOLD MORE THAN BOUGHT!!!!! DELETING" + item.Name + " : " + item.Game + " buy: " + buy_count.ToString() + " | sale: " + sale_count.ToString());
                    var delthesetrans = _context.Transactions.Where(i => i.Game_hash_name_key == item.hash_name_key).ToList();
                    foreach(var t in delthesetrans) 
                    {
                        _context.Transactions.Remove(t);
                        _context.SaveChanges();
                    
                    }

                }

                //used for testing
                var filterdate = "18/04/2092";

                var all_buys = item.Transactions.Where(t => t.type == '+').Where(d => d.DateT < DateTime.Parse(filterdate)).OrderByDescending(o => o.DateT).ToList();
                var all_sells = item.Transactions.Where(t => t.type == '-').Where(d => d.DateT < DateTime.Parse(filterdate)).OrderByDescending(o => o.DateT).ToList();


                item.total_buys = all_buys.Count();
                item.total_sales = all_sells.Count();

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


                //works out profits on sales only. ignore unsold purchases

                int cnt = 0;
                item.total_profit_sales_only = 0;
                foreach (var sale in all_sells) 
                {
                    item.total_profit_sales_only = item.total_profit_sales_only + (sale.int_sale_price_after_fees - all_buys[cnt].int_sale_price_after_fees);
                    cnt++;
                
                }



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
                item.Gap_mybid = 0;
                item.bid_quant = 0;
                item.bid_price = 0;
                item.bid_price_in_pound = "";
            }
            _context.SaveChanges();
            return "OK";
        }
        public string Excluder() 
        {
            int cnt = 0;
            var gamestoexclude = _context.exclude.ToList();
            foreach (var excludeme in gamestoexclude) 
            {
                var matches = _context.Items.Where(g => g.Game == excludeme.Game).Include(i => i.ItemsForSale).Include(t => t.Transactions).ToList();
                foreach (var delme in matches) 
                {
                    cnt++;
                    if (delme.Transactions.Count() == 0 && delme.ItemsForSale.Count() == 0)
                    {
                        _context.Items.Remove(delme);
                    }

                }
            
            
            }
            _context.SaveChanges();
            return "Delted " + cnt + " Titles";
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

        public string PostSells() 
        {
            var options = new ChromeOptions();
            options.AddArgument(@"user-data-dir=C:\Users\Admin\AppData\Local\Google\Chrome\User Data\Profile 4\Profile 1\Default");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            List<string> markets = new List<string>
            {
                "https://steamcommunity.com/profiles/76561198024474411/inventory#753",
                "https://steamcommunity.com/profiles/76561198024474411/inventory?modal=1&market=1#232090",
                "https://steamcommunity.com/profiles/76561198024474411/inventory?modal=1&market=1#227300",
                "https://steamcommunity.com/profiles/76561198024474411/inventory?modal=1&market=1#250820",
                "https://steamcommunity.com/profiles/76561198024474411/inventory?modal=1&market=1#431240"
                


            };


            var lastgame = "";

            foreach (var MarketUrl in markets)
            {
                bool done = false;
               
                var driver = new ChromeDriver(options);
                driver.Url = MarketUrl;
                RandomWait(10, 20);
                var filter_button = driver.FindElement(By.Id("filter_tag_show"));
                RandomWait(10, 20);
                filter_button.Click();
                RandomWait(10, 20);

                //do we have any?
                try
                {
                    // var tick_marketable = driver.FindElement(By.Id("tag_filter_753_6_misc_marketable"));
                    var tick_marketable = driver.FindElement(By.CssSelector("[id*='_misc_marketable']"));

                    // var tick_marketable = driver.FindElement(By.Id("input[id*=_misc_marketable]"));
                    RandomWait(10, 40);
                    tick_marketable.Click();
                    do
                    {
                        try
                        {
                            var items = driver.FindElements(By.ClassName("itemHolder"));
                            RandomWait(5, 20);
                            int cnt = 0;
                            foreach (var singleItem in items)
                            {
                                
                                if (singleItem.Displayed == true)
                                {
                                    var myClass = singleItem.GetAttribute("class");
                                    if (!myClass.Contains("disabled"))
                                    {
                                        Console.WriteLine("----------------------------------");
                                        Console.WriteLine("ITEM: " + cnt.ToString());
                                        RandomWait(1, 5);
                                        singleItem.Click();
                                        RandomWait(5, 15);

                                        var myLink = driver.FindElement(By.LinkText("View in Community Market"));
                                        var url = myLink.GetAttribute("href");
                                        string[] split = url.Split("/");
                                        var hash = HttpUtility.UrlDecode(split[split.Length - 1]);

                                        var myItem = _context.Items.Where(h => h.hash_name_key == hash).SingleOrDefault();

                                        if (myItem == null)
                                        {
                                            throw new Exception("CaNT FIND ITEM  : " + hash);
                                        }
                                        Console.WriteLine(myItem.Name);
                                        //use 900 so zero is never used in error
                                        var myprice = "9.0";
                                        //We have an item
                                        myprice = myItem.IdealSellStr;
                                        if (!myItem.onHoldPriceToolLow && !myItem.onHold)
                                        {

                                            //price is good enough to make profit

                                            if (hash != lastgame)
                                        {
                                            //makes only on eof every game put up for sale a day
                                            //not same as last game
                                            lastgame = hash;
                                        
                                        var sellbuttons = driver.FindElements(By.ClassName("item_market_action_button_green"));
                                        RandomWait(10, 20);
                                        //Console.WriteLine(sellbuttons.Count().ToString() + " Sell buttons");
                                        foreach (var butt in sellbuttons)
                                        {
                                            if (butt.Displayed == true)
                                            {
                                                butt.Click();
                                            }


                                        }

                                        RandomWait(10, 20);
                                        var price_box = driver.FindElement(By.Id("market_sell_buyercurrency_input"));
                                        RandomWait(3, 10);
                                        price_box.SendKeys(Keys.Backspace);
                                        RandomWait(1, 6);
                             
                               
                                                if (myItem.IdealSellInt != 100 && myItem.IdealSellInt != 200 && myItem.IdealSellInt != 300 && myItem.IdealSellInt != 400 && myItem.IdealSellInt != 500 && myItem.IdealSellInt != 600 && myItem.IdealSellInt != 700 && myItem.IdealSellInt != 900)
                                                {
                                                    if (myItem.IdealSellInt > 900 || myItem.IdealSellStr.Substring(1, 1) != ".")
                                                    {

                                                        //somethings not right
                                                        throw new Exception("SELL amount looks wrong!");
                                                    }
                                                }
                                                RandomWait(8, 16);
                                                myprice = myItem.IdealSellStr;
                                                RandomWait(11, 23);
                                                price_box.SendKeys(myprice);
                                                var terms = driver.FindElement(By.Id("market_sell_dialog_accept_ssa"));
                                                RandomWait(5, 20);
                                                if (terms.Selected == false)
                                                {
                                                    terms.Click();
                                                }
                                                RandomWait(10, 20);




                                                var placeorder = driver.FindElements(By.Id("market_sell_dialog_accept"));
                                                RandomWait(5, 10);
                                                placeorder[placeorder.Count - 1].Click();
                                                RandomWait(40, 80);

                                                try
                                                {
                                                    //try twice
                                                    var placeorderConfirm = driver.FindElement(By.Id("market_sell_dialog_ok"));
                                                    RandomWait(5, 10);
                                                    placeorderConfirm.Click();
                                                    RandomWait(10, 15);
                                                }
                                                catch
                                                {
                                                    Console.WriteLine("SECOND GO AT SELL BUTTON!");
                                                    var placeorderConfirm = driver.FindElement(By.Id("market_sell_dialog_ok"));
                                                    RandomWait(25, 40);
                                                    placeorderConfirm.Click();
                                                    RandomWait(30, 50);

                                                }

                                                try
                                                {
                                                    Console.WriteLine("FIRST GO AT OK BUTTON!");
                                                    RandomWait(80, 90);
                                                    var OK = driver.FindElement(By.ClassName("btn_grey_steamui"));
                                                    OK.Click();

                                                    RandomWait(55, 79);
                                                }
                                                catch
                                                {
                                                    try
                                                    {
                                                        Console.WriteLine("SECOND GO AT OK BUTTON!");
                                                        RandomWait(55, 79);
                                                        var OK = driver.FindElement(By.ClassName("btn_grey_steamui"));
                                                        RandomWait(15, 22);
                                                        OK.Click();

                                                        RandomWait(15, 39);
                                                    }
                                                    catch
                                                    {
                                                        try
                                                        {
                                                            Console.WriteLine("THIRD GO AT OK BUTTON!");
                                                            RandomWait(90, 95);
                                                            RandomWait(90, 95);
                                                            var OK = driver.FindElement(By.ClassName("btn_grey_steamui"));
                                                            RandomWait(55, 77);
                                                            OK.Click();

                                                            RandomWait(15, 39);
                                                        }
                                                        catch
                                                        {

                                                            Console.WriteLine("NO DICE!");
                                                        }
                                                    }
                                                }

                                                //new bit to clse any erros
                                                try
                                                {
                                                    RandomWait(2, 5);
                                                    var cross = driver.FindElement(By.ClassName("newmodal_close"));
                                                    cross.Click();
                                                    Console.WriteLine("CLICKED THE CROSS");

                                                }
                                                catch { }

                                            }
                                            else 
                                            {
                                                //this game asme as last one
                                                Console.WriteLine("Multiple game ignored for today");

                                            }
                                        }
                                        else
                                        {
                                            //item is flagged as being on hold becasue the
                                            //calculated sel price would result in a loss
                                            Console.WriteLine("ITEM PRICE TOO LOW ON HOLD : " + myItem.hash_name_key);
                                            
                                        }

                                        cnt++;

                                    }
                                    else 
                                    {
                                        Console.WriteLine("Item Disabled");
                                    }

                                }
                            }
                            if (cnt == 25)
                            {
                                try
                                {
                                    var nextbutton = driver.FindElement(By.Id("pagebtn_next"));
                                    nextbutton.Click();
                                }
                                catch
                                {

                                    done = true;
                                }


                                //more to do
                            }
                            else
                            {
                                done = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            //dont have any, OK
                            Console.WriteLine("******* ERROR : " + ex.Message);
                        }
                    } while (done == false);
                }
                catch 
                {
                //NO market check box
                }
                driver.Close();
               
            }
            return "OK";
        }

        public int increaseintbypercent(int number, int percent)
        {
            //works for decrease too
            double dnumber = (double)number;
            var increase = dnumber / 100;
            double bigger = (double)number + (increase * percent);
            int returnint = (int)bigger;


            return returnint;
        }

        public string PostBids() 
        
        {
            var options = new ChromeOptions();
            options.AddArgument(@"user-data-dir=C:\Users\Admin\AppData\Local\Google\Chrome\User Data\Profile 4\Profile 1\Default");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            var driver = new ChromeDriver(options);

            //var allItems = _context.Items.Where(g => g.Game.Contains("outer")).ToList();
            var allItems = _context.Items.ToList();

            foreach (var myItem in allItems)
            {
                if (myItem.CancelCurrentBid == true)
                {
                    CancelBid(driver, myItem);
                    myItem.CancelCurrentBid = false;
                    myItem.IdealBidInt = 0;
                    myItem.IdealBidStr = "";
                    _context.SaveChanges();
                }
                else
                {
                    if (myItem.IdealBidInt != 0)
                    {
                        //do the bizzo
                        //cancel current bid?
                        //we got an ideal bid, cancel current if we have one
                        if (myItem.bid_price != 0)
                        {
                            
                            CancelBid(driver, myItem);
                         
                        }
                      
                        //PLACE BID
                            
                            PlaceBid(driver, myItem);
                            
                    }
                    else
                    {
                        //no auto bid set
                    }
                }
            
            }

            driver.Close();
                return ("OK");
        }
        string PlaceBid(ChromeDriver driver, Item myItem) 
        {
            Console.WriteLine("Placing bid for " + myItem.Name + " : Bid  = " + myItem.IdealBidStr);
            try
            {
                driver.Url = myItem.ItemPageURL;
                RandomWait(30, 40);
                var myBuyButton = driver.FindElement(By.ClassName("market_commodity_buy_button"));
                RandomWait(5, 15);
                myBuyButton.Click();
                RandomWait(10, 20);
                var price_box = driver.FindElement(By.Id("market_buy_commodity_input_price"));
                RandomWait(3, 10);
                price_box.SendKeys(Keys.Backspace);
                RandomWait(1, 6);
                price_box.SendKeys(Keys.Backspace);
                RandomWait(1, 8);
                price_box.SendKeys(Keys.Backspace);
                RandomWait(1, 10);
                price_box.SendKeys(Keys.Backspace);
                RandomWait(1, 6);
                price_box.SendKeys(Keys.Backspace);
                RandomWait(1, 10);
                price_box.SendKeys(Keys.Backspace);
                RandomWait(10, 28);
                if (myItem.IdealBidInt != 100 && myItem.IdealBidInt != 200 && myItem.IdealBidInt != 300 && myItem.IdealBidInt != 400 && myItem.IdealBidInt != 500 && myItem.IdealBidInt != 600 && myItem.IdealBidInt != 700 && myItem.IdealBidInt != 900)
                {
                    if (myItem.IdealBidInt > 900 || myItem.IdealBidStr.Substring(1, 1) != ".")
                    {
                      
                    //somethings not right
                    throw new Exception("Bid amount looks wrong!");
                    }
                }
                price_box.SendKeys(myItem.IdealBidStr);
                var terms = driver.FindElement(By.Id("market_buyorder_dialog_accept_ssa"));
                RandomWait(5, 20);
                terms.Click();
                RandomWait(10, 20);
                var placeorder = driver.FindElement(By.Id("market_buyorder_dialog_purchase"));
                RandomWait(5, 10);
                placeorder.Click();
                RandomWait(95, 99);
                //update the item rec
                //changes should be confirmed next time bids are inported
               
                myItem.bid_price = myItem.IdealBidInt;
                myItem.IdealBidInt = 0;
                myItem.IdealBidStr = "";
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("**************** FAIL PLACING BID!! ***********************" + ex.Message) ;
                RandomWait(99, 100);
                
            }
            return "FAIL";
        }
        string CancelBid(ChromeDriver driver,Item myItem) 
        {
            Console.WriteLine("Cancelling bid for " + myItem.Name + " : Bid was = " + myItem.bid_price_in_pound);
            try
            {
                driver.Url = myItem.ItemPageURL;
                RandomWait(20, 50);
                try
                {
                    var cancelbutton = driver.FindElements(By.ClassName("item_market_action_button"));

                    RandomWait(5, 10);
                    cancelbutton[cancelbutton.Count() - 1].Click();
                    RandomWait(5, 10);

                }
                catch
                {
                    Console.WriteLine("Couldnt Cancel, Already done?");
                }
            }
            catch 
            {
                Console.WriteLine("**************** Couldnt get page to cancel, Connection Error?");
                RandomWait(99,100);
                RandomWait(99, 100);
            }
    
            return "OK";
        }

        void RandomWait(int min100, int max100)
        {

            //Generate random sleeptime
            Random waitTime = new Random();
            var seconds = waitTime.Next(min100 * 72, max100 * 72);

            //Put the thread to sleep
            System.Threading.Thread.Sleep(seconds);
        }

      
    }
}
