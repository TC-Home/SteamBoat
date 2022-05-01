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
    public class excludesController : Controller
    {
        private readonly SteamBoatContext _context;

        public excludesController(SteamBoatContext context)
        {
            _context = context;
        }

        // GET: excludes
        public async Task<IActionResult> Index()
        {
            return View(await _context.exclude.ToListAsync());
        }

        // GET: excludes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exclude = await _context.exclude
                .FirstOrDefaultAsync(m => m.Game == id);
            if (exclude == null)
            {
                return NotFound();
            }

            return View(exclude);
        }

        // GET: excludes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: excludes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Game")] exclude exclude)
        {
            if (ModelState.IsValid)
            {
                _context.Add(exclude);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(exclude);
        }

        // GET: excludes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exclude = await _context.exclude.FindAsync(id);
            if (exclude == null)
            {
                return NotFound();
            }
            return View(exclude);
        }

        // POST: excludes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Game")] exclude exclude)
        {
            if (id != exclude.Game)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(exclude);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!excludeExists(exclude.Game))
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
            return View(exclude);
        }

        // GET: excludes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exclude = await _context.exclude
                .FirstOrDefaultAsync(m => m.Game == id);
            if (exclude == null)
            {
                return NotFound();
            }

            return View(exclude);
        }

        // POST: excludes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var exclude = await _context.exclude.FindAsync(id);
            _context.exclude.Remove(exclude);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool excludeExists(string id)
        {
            return _context.exclude.Any(e => e.Game == id);
        }
    }
}
