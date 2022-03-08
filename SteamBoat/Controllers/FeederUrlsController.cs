using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SteamBoat.Data;
using SteamBoat.Models;

namespace SteamBoat.Controllers
{
    public class FeederUrlsController : Controller
    {
        private readonly SteamBoatContext _context;

        public FeederUrlsController(SteamBoatContext context)
        {
            _context = context;
        }

        // GET: FeederUrls
        public async Task<IActionResult> Index()
        {
            var steamBoatContext = _context.FeederUrl.Include(f => f.Mission);
            return View(await steamBoatContext.ToListAsync());
        }

        // GET: FeederUrls/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feederUrl = await _context.FeederUrl
                .Include(f => f.Mission)
                .FirstOrDefaultAsync(m => m.FeederId == id);
            if (feederUrl == null)
            {
                return NotFound();
            }

            return View(feederUrl);
        }

        // GET: FeederUrls/Create
        public IActionResult Create()
        {
            ViewData["MissionId"] = new SelectList(_context.Mission, "MissionId", "MissionId");
            return View();
        }

        // POST: FeederUrls/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FeederId,MissionId,Url,isJSON")] FeederUrl feederUrl)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feederUrl);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MissionId"] = new SelectList(_context.Mission, "MissionId", "MissionId", feederUrl.MissionId);
            return View(feederUrl);
        }

        // GET: FeederUrls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feederUrl = await _context.FeederUrl.FindAsync(id);
            if (feederUrl == null)
            {
                return NotFound();
            }
            ViewData["MissionId"] = new SelectList(_context.Mission, "MissionId", "MissionId", feederUrl.MissionId);
            return View(feederUrl);
        }

        // POST: FeederUrls/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FeederId,MissionId,Url,isJSON")] FeederUrl feederUrl)
        {
            if (id != feederUrl.FeederId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feederUrl);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeederUrlExists(feederUrl.FeederId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MissionId"] = new SelectList(_context.Mission, "MissionId", "MissionId", feederUrl.MissionId);
            return View(feederUrl);
        }

        // GET: FeederUrls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feederUrl = await _context.FeederUrl
                .Include(f => f.Mission)
                .FirstOrDefaultAsync(m => m.FeederId == id);
            if (feederUrl == null)
            {
                return NotFound();
            }

            return View(feederUrl);
        }

        // POST: FeederUrls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feederUrl = await _context.FeederUrl.FindAsync(id);
            _context.FeederUrl.Remove(feederUrl);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeederUrlExists(int id)
        {
            return _context.FeederUrl.Any(e => e.FeederId == id);
        }
    }
}
