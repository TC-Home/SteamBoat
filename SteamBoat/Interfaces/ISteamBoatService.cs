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

        public missionReportVM CreateUpdateItemPage(string hash_name, Mission Mission, missionReportVM MissionReport, int sellprice);

        public string LHFandGaps(Freshness freshness);

        public List<Item> GetLHFS(int lowest = 10);

        public List<Item> GetGaps(int lowest = 10);

    }
}
