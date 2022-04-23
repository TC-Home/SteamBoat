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
        }

        [Key]
        public string hash_name_key { get; set; }
        public string item_nameid { get; set; }

        public string ItemPageURL { get; set; }

        public string ItemStatsURL { get; set; }


        [MaxLength(255)]
        public string Game { get; set; }
        [MaxLength(255)]
        public string  Name { get; set; }

        public int NumForSale { get; set; }
        public int StartingPrice { get; set; }
        public int orders { get; set; }
        public int Activity { get; set; }
        public int Recent { get; set; }

        public int Median { get; set; }

        public int max_buy_price { get; set; }
        public int min_sell_price { get; set; }
        public int next_min_sell_price { get; set; }

        public int Fruit { get; set; }

        public int Gap { get; set; }

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
    }
