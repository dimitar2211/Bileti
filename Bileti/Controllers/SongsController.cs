using Bileti.Data;
using Bileti.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bileti.Controllers
{
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
            var tracks = _context.Songs
                .Select(s => s.SpotifyTrackId)
                .ToList();

            return View(tracks); // Връща List<string> с track IDs към View
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

            // Проверява дали вече съществува track с този ID
            if (!_context.Songs.Any(s => s.SpotifyTrackId == trackId))
            {
                _context.Songs.Add(new Song { SpotifyTrackId = trackId });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Извлича track ID от Spotify линка (например: "4uLU6hMCjMI75M1A2tKUQC")
        private string ExtractSpotifyTrackId(string url)
        {
            var match = Regex.Match(url, @"spotify\.com/track/([a-zA-Z0-9]+)");
            if (match.Success)
                return match.Groups[1].Value;

            return null;
        }


        [HttpPost]
        public async Task<IActionResult> DeleteSong(string spotifyTrackId)
        {
            if (string.IsNullOrEmpty(spotifyTrackId))
                return RedirectToAction("Index");

            var song = _context.Songs.FirstOrDefault(s => s.SpotifyTrackId == spotifyTrackId);
            if (song != null)
            {
                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

    }
}

