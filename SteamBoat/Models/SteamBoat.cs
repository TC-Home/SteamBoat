using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SteamBoat.Models
{
    public class Mission
    {
        [Key]
        public int MissionId { get; set; }
        [MaxLength(255)]
        public string   Name { get; set; }

        

        public int TradesMin { get; set; }
        public int TradesMax { get; set; }

        public int PriceMin { get; set; }
        public int PriceMax { get; set; }

        public virtual ICollection<FeederUrl> FeederUrls { get; set; }

    }

    
    public class FeederUrl 
    {
        [Key]
        public int FeederId { get; set; }
        [ForeignKey("Mission")]
        public int MissionId { get; set; }

        [MaxLength(255)]
        public string Url { get; set; }

        public bool isJSON { get; set; }

        public virtual Mission Mission { get; set; }

    }
    //listing is an individual item
    public class Listing
    {
       
        //steam hash_name
        [MaxLength(255)]
        [Key]
        public string Id { get; set; }

        [ForeignKey("FeederUrl")]
        public int FeederId { get; set; }
        [MaxLength(255)]
        public string name { get; set; }
       
        
        public int sell_listings { get; set; }

        public int sell_price { get; set; }
        [MaxLength(255)]
        public string sell_price_text { get; set; }
        [MaxLength(255)]
        public string app_icon { get; set; }
        [MaxLength(255)]
        public string app_name { get; set; }

        public int asset_appid { get; set; }
        [MaxLength(255)]
        public string asset_classid { get; set; }
        [MaxLength(255)]
        public string asset_instanceid { get; set; }
        [MaxLength(255)]
        public string asset_background_color { get; set; }
        [MaxLength(255)]
        public string asset_icon_url { get; set; }
        public int asset_tradable { get; set; }
        [MaxLength(255)]
        public string asset_name { get; set; }
        [MaxLength(255)]
        public string asset_name_color { get; set; }
        [MaxLength(255)]
        public string asset_type { get; set; }
        [MaxLength(255)]
        public string asset_market_name { get; set; }
        [MaxLength(255)]
        public string asset_market_hash_name { get; set; }

        public int asset_commodity { get; set; }
        [MaxLength(255)]
        public string sale_price_text { get; set; }

        public virtual FeederUrl FeederUrl { get; set; }


    }
}
