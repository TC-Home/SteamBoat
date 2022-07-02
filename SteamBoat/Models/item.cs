using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SteamBoat.Models
{
    public class Item
    {

        public Item()
        {
            this.ItemsForSale = new HashSet<ItemForSale>();
            this.Transactions = new HashSet<Transaction>();
            this.ThrottledOUT = false;
                this.allowshark = false;
        }

        [Key]
        public string hash_name_key { get; set; }
        public string item_nameid { get; set; }

        public string ItemPageURL { get; set; }

        public string ItemStatsURL { get; set; }

       


        [MaxLength(255)]
        public string Game { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }

        public int NumForSale { get; set; }
        public int StartingPrice { get; set; }
        public int orders { get; set; }
        public int Activity { get; set; }
        public int Recent { get; set; }

        public int Median { get; set; }

        public int max_buy_price { get; set; }
        public int min_sell_price { get; set; }
        public int next_min_sell_price { get; set; }

        //low hanging
        public int Fruit { get; set; }

        //high hanging
        public int Fruit2 { get; set; }

        public int Gap { get; set; }

        public int Gap_mybid { get; set; }

        public string bid_price_in_pound { get; set; }

        public int bid_price { get; set; }
        public int bid_quant { get; set; }

        public string buys_html { get; set; }
        public string sells_html { get; set; }


        public int total_buys { get; set; }

        public int total_sales { get; set; }

        public int total_buys_sum_amount { get; set; }

        public int total_sales_sum_amount { get; set; }

        public int total_profit { get; set; }

        public int total_profit_including_stock { get; set; }

        public int total_profit_sales_only { get; set; }

        public int Ave_buy { get; set; }

        public int Ave_sell { get; set; }

        public int Ave_profit { get; set; }

        public int Ave_profic_pc { get; set; }

        

        public int AH1 { get; set; }
        public int AH2 { get; set; }
        public int AH3 { get; set; }
        public int AH4 { get; set; }
        public int AH5 { get; set; }
        public int AH6 { get; set; }
        public int AH7 { get; set; }
        public int AH8 { get; set; }
        public int AH9 { get; set; }

        public int AH10 { get; set; }

        public int bid1Price { get; set; }
        public int bid1Quant { get; set; }
        public int bid2Price { get; set; }
        public int bid2Quant { get; set; }
        public int bid3Price { get; set; }
        public int bid3Quant { get; set; }
        public int bid4Price { get; set; }
        public int bid4Quant { get; set; }
        public int bid5Price { get; set; }
        public int bid5Quant { get; set; }

        public int sell1Price { get; set; }
        public int sell1Quant { get; set; }
        public int sell2Price { get; set; }
        public int sell2Quant { get; set; }
        public int sell3Price { get; set; }
        public int sell3Quant { get; set; }
        public int sell4Price { get; set; }
        public int sell4Quant { get; set; }
        public int sell5Price { get; set; }
        public int sell5Quant { get; set; }

        public bool IncludeInAutoBid { get; set; }

        public bool CancelCurrentBid { get; set; }
        public int IdealBidInt { get; set; }
        public String IdealBidStr { get; set; }

        public string IdealBid_Notes { get; set; }

        public int IdealSellInt { get; set; }
        public String IdealSellStr { get; set; }

        public string IdealSell_Notes { get; set; }

        public DateTime LastSellDate { get; set; }
        public int LastSellInt { get; set; }

        public int LastProfitInt { get; set; }
        
        public int LastNumberSold { get; set; }

        public bool tempBuyHold { get; set; }
        public bool tempSellHold { get; set; }

        public int minSellPrice { get; set; }
        
        //likely buy price of last sale
        public int LastSaleBuyPrice { get; set; }
        public int LastBuyPrice { get; set; }

        public bool onHoldPriceToolLow { get; set; }
        public bool onHold { get; set; }

        public int Tip_Price1 { get; set; }
        public int Tip_Price2 { get; set; }
        public int Tip_Price3 { get; set; }
        public int Tip_Price4 { get; set; }
        public int Tip_Price5 { get; set; }
        public int Tip_Price6 { get; set; }
        public int Tip_Price7 { get; set; }
        public int Tip_Price8 { get; set; }
        public int Tip_Price9 { get; set; }

        public int Tip_Price10 { get; set; }

        public int Tip_PriceAVG { get; set; }

        public int Pred_Tip_Price { get; set; }

        public int SharkMaxPrice { get; set; }

        public bool ThrottledOUT { get; set; }

        public bool allowshark { get; set; }

        public DateTime lastSharked { get; set; }

        //bid check uses various systems to look for uhnuaul high bids
        //caused by freak price rises or manuipulation
        public int BidCheck1Score { get; set; }

        public int BidCheck2Score { get; set; }

        public int BidCheck3Score { get; set; }

        public bool BidCheck1Pass { get; set; }

        public bool BidCheck2Pass { get; set; }

        public bool BidCheck3Pass { get; set; }

        public string BidCheckNotes { get; set; }

        public int MyValuation { get; set; }

        [ForeignKey("Game_hash_name_key")]
        public ICollection<ItemForSale> ItemsForSale { get; set; }
        [ForeignKey("Game_hash_name_key")]
        public ICollection<Transaction> Transactions { get; set; }



    }


    public class ItemForSale
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Item")]
        public string Game_hash_name_key { get; set; }

        public int sale_price { get; set; }

        public int max_buy_bid { get; set; }

        public int max_buy_bid_diff { get; set; }

        public int sale_price_diff { get; set; }

        public int sale_price_after_fees { get; set; }

        public string sell_price_without_fees { get; set; }



    }


    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Tran_Id { get; set; }

        public char type { get; set; }

        [ForeignKey("Item")]
        public string Game_hash_name_key { get; set; }

        public int int_sale_price_after_fees { get; set; }

        public string sale_price_after_fees { get; set; }

        public DateTime DateT { get; set; }
    }

    public class exclude
    {
        [Key]
        [MaxLength(255)]
        public string Game { get; set; }


    }

    public class bids 
    {
        public int bid_order { get; set; }
        public int bid_int { get; set; }
        public int bid_quant { get; set; }

        public int bid_fruit { get; set; }

    }
}
