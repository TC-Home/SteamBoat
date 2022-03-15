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
            foreach (var myFeederUrl in myUrls) 
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
                        GetItemsfromJSON(GrabbedJSON, mymission, mymissionReport);

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

                

            return mymissionReport;

        }

        public missionReportVM GetItemsfromJSON(GrabResult GrabbedJSON, Mission Mission, missionReportVM MissionReport) 
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

                                Console.WriteLine("GRABBING : " + name + " " + sell_price_text + " ("+ sell_listings + ")");
                                var itempage = _ContentGrabberService.GrabMe(Mission.ItemUrl + item.SelectToken("hash_name"));

                            }                        
                        }



                    }



                }



            }




            MissionReport.Errors.Add("THERE WAS AN ERROR");
            return MissionReport;
        
        } 

    }
}
