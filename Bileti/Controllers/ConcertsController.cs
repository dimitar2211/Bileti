using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bileti.Data;
using Bileti.Models;

namespace Bileti.Controllers
{
    public class ConcertsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConcertsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Concerts
        public async Task<IActionResult> Index()
        {
            var concerts = await _context.Concerts.ToListAsync();

            var lastPurchase = concerts
                .Where(c => c.LastPurchaseAt.HasValue)
                .OrderByDescending(c => c.LastPurchaseAt)
                .FirstOrDefault();

            if (lastPurchase != null && lastPurchase.LastPurchaseAt.HasValue)
            {
                var timePassed = DateTime.UtcNow - lastPurchase.LastPurchaseAt.Value;

                string message = timePassed.TotalSeconds < 60
                    ? $"Last ticket was bought {timePassed.Seconds} seconds ago."
                    : timePassed.TotalMinutes < 60
                        ? $"Last ticket was bought {timePassed.Minutes} minutes ago."
                        : $"Last ticket was bought {timePassed.Hours} hours ago.";

                ViewData["LastPurchaseMessage"] = message;
            }
            else
            {
                ViewData["LastPurchaseMessage"] = "Nobody has purchased a ticket yet.";
            }

            return View(concerts);
        }

        // GET: Concerts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var concert = await _context.Concerts.FirstOrDefaultAsync(m => m.Id == id);
            if (concert == null) return NotFound();

            return View(concert);
        }

        // GET: Concerts/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Concerts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Location,StartDate,EndDate,AvailableTickets")] Concert concert)
        {
            if (ModelState.IsValid)
            {
                _context.Add(concert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(concert);
        }

        // GET: Concerts/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var concert = await _context.Concerts.FindAsync(id);
            if (concert == null) return NotFound();

            return View(concert);
        }

        // POST: Concerts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Location,StartDate,EndDate,AvailableTickets")] Concert concert)
        {
            if (id != concert.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(concert);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConcertExists(concert.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(concert);
        }

        // GET: Concerts/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var concert = await _context.Concerts.FirstOrDefaultAsync(m => m.Id == id);
            if (concert == null) return NotFound();

            return View(concert);
        }

        // POST: Concerts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var concert = await _context.Concerts.FindAsync(id);
            if (concert != null)
            {
                _context.Concerts.Remove(concert);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Concerts/Buy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int id)
        {
            var concert = await _context.Concerts.FindAsync(id);
            if (concert == null) return NotFound();

            if (concert.AvailableTickets > 0)
            {
                concert.AvailableTickets--;
                concert.LastPurchaseAt = DateTime.UtcNow;

                if (concert.AvailableTickets == 0)
                {
                    _context.Concerts.Remove(concert);
                }
                else
                {
                    _context.Concerts.Update(concert);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "You bought a ticket! :)";
            }
            else
            {
                TempData["Error"] = "No available ticket :(";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ConcertExists(int id)
        {
            return _context.Concerts.Any(e => e.Id == id);
        }
    }
}
