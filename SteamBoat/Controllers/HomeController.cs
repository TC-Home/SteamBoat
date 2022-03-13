using ContentGrabber.DataAccess.Interfaces;
using ContentGrabber.Interfaces;
using ContentGrabber.Models.TypeSafeEnum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SteamBoat.Interfaces;
using SteamBoat.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SteamBoat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IContentGrabberDataService _ContentGrabberDataService;
        private readonly IContentGrabberService _ContentGrabberService;
        private readonly ISteamBoatService _SteamBoatService;

        public HomeController(ISteamBoatService SteamBoatService,ILogger<HomeController> logger, IContentGrabberDataService ContentGrabberDataService, IContentGrabberService ContentGrabberService)
        {
            _logger = logger;
            _ContentGrabberDataService = ContentGrabberDataService;
            _ContentGrabberService = ContentGrabberService;
            _SteamBoatService = SteamBoatService;
        }

        public IActionResult Index()
        {

            var res = _SteamBoatService.doMission(1, Freshness.Hour24);

            //var res = _ContentGrabberService.GrabMeJSON("https://steamcommunity.com/market/search/render/?q=&start=0&count=500&category_753_Game%5B%5D=any&category_753_cardborder%5B%5D=tag_cardborder_1&category_753_item_class%5B%5D=tag_item_class_2&appid=753&sort_column=quantity&norender=1");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
