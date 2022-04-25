﻿using ContentGrabber.Models;
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

        public string ActivityUpdateAll(bool usedayofweek = false);

        public string ActivityUpdateSingle(Item item);

        public int poundtocent(string pound, float? exrate = 1f);

        public string UpdateBidPrice(string hash_name, int bid_quant, int bid_price, string bid_price_in_pound);
   

        public string Clean(string Dirty);

        //remove all bids before updating them
        public string ClearAllBids();

        public string ClearAllSales();

        public string CheckSalePrices();

        public string AddSellListing(string hash_name, int sell_price_after_fees, int int_sell_price_without_fees, string sell_price_without_fees);

        public List<Item> GetAllItems();

        public List<ItemForSale> GetAllSaleItems();

        
        public List<Item> GetAllItemsandSales();

        public string UpdateTrans();

        public string getBetween(string strSource, string strStart, string strEnd);

        public string GetGameHashNamefromItemandGame(string Item, string Game);


        public string CleanMe(string cleanme);
    }
}
