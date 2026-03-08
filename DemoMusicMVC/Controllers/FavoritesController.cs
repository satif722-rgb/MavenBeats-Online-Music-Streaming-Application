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
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoritesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Favorites
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var favorites = await _context.Favorites
                .Include(f => f.Song)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.DateAdded)
                .ToListAsync();
            return View(favorites);
        }

        // POST: Favorites/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int songId, string? returnUrl)
        {
            var userId = _userManager.GetUserId(User);
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == songId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
            }
            else
            {
                var songExists = await _context.songs.AnyAsync(s => s.songId == songId);
                if (!songExists) return NotFound();

                _context.Favorites.Add(new Favorite
                {
                    UserId = userId!,
                    SongId = songId,
                    DateAdded = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("VisitorPage", "Songs");
        }
    }
}
