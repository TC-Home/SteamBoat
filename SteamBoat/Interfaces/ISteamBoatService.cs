using ContentGrabber.Models;
using ContentGrabber.Models.TypeSafeEnum;
using SteamBoat.Models;
using SteamBoat.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamBoat.Interfaces
{
    public interface ISteamBoatService
    {
        public missionReportVM doMission(int MissionId);
        public missionReportVM doMission(int MissionId, Freshness Freshness);

        public missionReportVM GetItemsfromJSON(GrabResult GrabbedJSON, Mission Mission, missionReportVM MissionReport, Freshness Freshness);

        public missionReportVM CreateUpdateItemPage(string hash_name, string itemUrl, missionReportVM MissionReport, int sellprice, string imageurl, string fullitemURL = null);

        public string LHFandGaps(Freshness freshness);

        public List<Item> GetLHFS(int lowest = 10);

        public List<Item> GetGaps(int lowest = 10);

        public int poundtocent(string pound, float? exrate = 1f);

        public string UpdateBidPrice(string hash_name, int bid_quant, int bid_price, string bid_price_in_pound);
   

        public string Clean(string Dirty);

        //remove all bids before updating them
        public string ClearAllBids();

        public List<Item> GetAllItems(); 
        

    }
}
