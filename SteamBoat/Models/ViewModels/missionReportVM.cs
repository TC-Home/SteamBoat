using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamBoat.Models.ViewModels
{
    public class missionReportVM
    {

        public missionReportVM()
        {
            this.Errors = new List<string>();
            this.Warnings = new List<string>();
            this.urls = new List<string>();
        }
        public string Status { get; set; }

        public bool Fail { get; set; }

        public List<String> Errors { get; set; }
        public List<String> Warnings { get; set; }

        public List<String> urls { get; set; }
    }
}
