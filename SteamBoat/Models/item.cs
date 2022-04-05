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
        public int max_buy_price { get; set; }
        public int min_sell_price { get; set; }
        public int next_min_sell_price { get; set; }

        public int Fruit { get; set; }

        public int Gap { get; set; }

        public string bid_price_in_pound { get; set; }

        public int bid_price { get; set; }
        public int bid_quant { get; set; }

        public ICollection<ItemForSale> ItemsForSale { get; set; }
    }


    public class ItemForSale 
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Item")]
        public string Game_hash_name_key { get; set; }

        public int sale_price_after_fees { get; set; }



    }
}
