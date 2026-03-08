using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMusicMVC.Data;
using DemoMusicMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DemoMusicMVC.Controllers
{
    [Authorize(Roles = "Admin,Customer,PremiumCustomer")]
    public class PlaylistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PlaylistsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Playlists
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var playlists = await _context.Playlists
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
            return View(playlists);
        }

        // GET: Playlists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs!)
                    .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (playlist == null) return NotFound();

            return View(playlist);
        }

        // GET: Playlists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Playlists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Playlist playlist)
        {
            var userId = _userManager.GetUserId(User);
            playlist.UserId = userId!;
            playlist.CreatedDate = DateTime.UtcNow;
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Add(playlist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(playlist);
        }

        // GET: Playlists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (playlist == null) return NotFound();

            return View(playlist);
        }

        // POST: Playlists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (playlist == null) return NotFound();

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Playlists/AddSong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSong(int playlistId, int songId)
        {
            var userId = _userManager.GetUserId(User);
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId);

            if (playlist == null) return NotFound();

            var songExists = await _context.songs.AnyAsync(s => s.songId == songId);
            if (!songExists) return NotFound();

            var alreadyAdded = await _context.PlaylistSongs
                .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (!alreadyAdded)
            {
                _context.PlaylistSongs.Add(new PlaylistSong
                {
                    PlaylistId = playlistId,
                    SongId = songId
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = playlistId });
        }

        // POST: Playlists/RemoveSong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSong(int playlistSongId, int playlistId)
        {
            var userId = _userManager.GetUserId(User);
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId);

            if (playlist == null) return NotFound();

            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistSongId == playlistSongId && ps.PlaylistId == playlistId);

            if (playlistSong != null)
            {
                _context.PlaylistSongs.Remove(playlistSong);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = playlistId });
        }

        // GET: Playlists/SelectPlaylist/5 (select playlist to add song to)
        public async Task<IActionResult> SelectPlaylist(int songId)
        {
            var userId = _userManager.GetUserId(User);
            var playlists = await _context.Playlists
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            ViewData["SongId"] = songId;
            return View(playlists);
        }
    }
}
