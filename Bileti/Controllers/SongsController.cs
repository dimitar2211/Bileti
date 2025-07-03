using Bileti.Data;
using Bileti.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bileti.Controllers
{
    [Authorize]
    public class SongsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SongsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Songs
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tracks = _context.Songs
                .Where(s => s.UserId == userId)
                .Select(s => s.SpotifyTrackId)
                .ToList();

            return View(tracks);
        }

        // POST: /Songs/AddSong
        [HttpPost]
        public async Task<IActionResult> AddSong(string spotifyLink)
        {
            if (string.IsNullOrWhiteSpace(spotifyLink))
                return RedirectToAction("Index");

            var trackId = ExtractSpotifyTrackId(spotifyLink);
            if (trackId == null)
                return RedirectToAction("Index");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!_context.Songs.Any(s => s.SpotifyTrackId == trackId && s.UserId == userId))
            {
                _context.Songs.Add(new Song { SpotifyTrackId = trackId, UserId = userId });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // POST: /Songs/DeleteSong
        [HttpPost]
        public async Task<IActionResult> DeleteSong(string spotifyTrackId)
        {
            if (string.IsNullOrEmpty(spotifyTrackId))
                return RedirectToAction("Index");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var song = _context.Songs.FirstOrDefault(s => s.SpotifyTrackId == spotifyTrackId && s.UserId == userId);
            if (song != null)
            {
                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        private string ExtractSpotifyTrackId(string url)
        {
            var match = Regex.Match(url, @"spotify\.com/track/([a-zA-Z0-9]+)");
            if (match.Success)
                return match.Groups[1].Value;

            return null;
        }
    }
}

