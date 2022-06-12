using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamBoat.Models
{
    public class Activityrec
    {
        public DateTime myDate { get; set; }
        public float myNumber { get; set; }
        public float myAmount { get; set; }

        public int  myHigh { get; set; }
        public int myLow { get; set; }

    }


    public class Activitybatch 
    {
        public int BatchNumber;
        public int BatchAV { get; set; }

    }
}
