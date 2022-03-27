using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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


    }
}
