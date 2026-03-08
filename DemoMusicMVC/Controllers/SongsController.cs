using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DemoMusicMVC.Data;
using DemoMusicMVC.Models;
using Microsoft.AspNetCore.Hosting;
using DemoMusicMVC.ViewModel;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DemoMusicMVC.Controllers
{
    public class SongsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public SongsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _context = context;
            this.hostingEnvironment = hostingEnvironment;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        [Authorize(Roles = "Admin,Customer,PremiumCustomer")]
        // GET: Songs
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            IList<string> role = await _userManager.GetRolesAsync(user!);
            if (role != null && (role[0] == "Admin" || role[0] == "PremiumCustomer"))
            {
                // Admins and PremiumCustomers only see their own songs
                var ownSongs = await _context.songs
                    .Where(s => s.UploadedByUserId == user!.Id)
                    .ToListAsync();
                return View(ownSongs);
            }
            return View("VisitorPage", await _context.songs.ToListAsync());
        }
        [Authorize(Roles = "Admin,Customer,PremiumCustomer")]
        // GET: Songs/VisitorPage
        public async Task<IActionResult> VisitorPage(string? searchString)
        {
            var songs = _context.songs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                songs = songs.Where(s => s.songName.Contains(searchString));
            }
            ViewData["CurrentFilter"] = searchString;
            return View(await songs.ToListAsync());
        }
        [Authorize(Roles = "Admin,Customer,PremiumCustomer")]
        // GET: Songs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            IList<string> role = await _userManager.GetRolesAsync(user!);
            if (role != null && (role[0] == "Admin" || role[0] == "PremiumCustomer"))
            {
                if (id == null)
                {
                    return NotFound();
                }

                var song = await _context.songs
                    .FirstOrDefaultAsync(m => m.songId == id);
                if (song == null)
                {
                    return NotFound();
                }

                return View(song);
            }
            else
            {
                if (id == null)
                {
                    return NotFound();
                }

                var song = await _context.songs
                    .FirstOrDefaultAsync(m => m.songId == id);
                if (song == null)
                {
                    return NotFound();
                }

                return View("PlaySong", song);
            }
        }
        [Authorize(Roles = "Admin,PremiumCustomer")]
        // GET: Songs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Songs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,PremiumCustomer")]
        public async Task<IActionResult> Create(SongViewModel uploadedSong)
        {
            string? pathForPhoto = null, pathForSong = null;
            if (ModelState.IsValid)
            {
                if (uploadedSong.song != null)
                {
                    string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath, "Songs");
                    pathForSong = Guid.NewGuid().ToString() + "_" + uploadedSong.song.FileName;
                    string filePath = Path.Combine(uploadFolder, pathForSong);
                    uploadedSong.song.CopyTo(new FileStream(filePath, FileMode.Create));
                }
                if (uploadedSong.photo != null)
                {
                    string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath, "Photos");
                    pathForPhoto = Guid.NewGuid().ToString() + "_" + uploadedSong.photo.FileName;
                    string filePath = Path.Combine(uploadFolder, pathForPhoto);
                    uploadedSong.photo.CopyTo(new FileStream(filePath, FileMode.Create));
                }
                if (uploadedSong.song != null && uploadedSong.photo != null)
                {
                    var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                    Song s = new Song
                    {
                        songName = uploadedSong.songName,
                        photoPath = pathForPhoto!,
                        songPath = pathForSong!,
                        UploadedByUserId = currentUser!.Id
                    };
                    _context.Add(s);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(uploadedSong);
        }
        [Authorize(Roles = "Admin,PremiumCustomer")]
        // GET: Songs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            // Ownership check
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (song.UploadedByUserId != null && song.UploadedByUserId != currentUser!.Id)
            {
                return Forbid();
            }

            string webRoot = hostingEnvironment.WebRootPath;
            if (System.IO.Directory.Exists(webRoot + "/Photos/") && System.IO.Directory.Exists(webRoot + "/Songs/"))
            {
                SongViewModel s = new SongViewModel()
                {
                    songId = song.songId,
                    songName = song.songName,
                    song = null,
                    photo = null
                };
                return View(s);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: Songs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,PremiumCustomer")]
        public async Task<IActionResult> Edit(int id, SongViewModel _song)
        {
            string? pathForPhoto = null, pathForSong = null;
            if (id != _song.songId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var song = await _context.songs.FirstOrDefaultAsync(m => m.songId == id);
                if (song == null)
                {
                    return NotFound();
                }

                // Ownership check
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                if (song.UploadedByUserId != null && song.UploadedByUserId != currentUser!.Id)
                {
                    return Forbid();
                }

                try
                {
                    if (_song.song != null)
                    {
                        string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath, "Songs");
                        pathForSong = Guid.NewGuid().ToString() + "_" + _song.song.FileName;
                        string filePath = Path.Combine(uploadFolder, pathForSong);
                        _song.song.CopyTo(new FileStream(filePath, FileMode.Create));
                    }
                    if (_song.photo != null)
                    {
                        string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath, "Photos");
                        pathForPhoto = Guid.NewGuid().ToString() + "_" + _song.photo.FileName;
                        string filePath = Path.Combine(uploadFolder, pathForPhoto);
                        _song.photo.CopyTo(new FileStream(filePath, FileMode.Create));
                    }

                    song.songName = _song.songName;
                    if (pathForSong != null) song.songPath = pathForSong;
                    if (pathForPhoto != null) song.photoPath = pathForPhoto;
                    _context.Update(song);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SongExists(_song.songId))
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
            return View(_song);
        }
        [Authorize(Roles = "Admin,PremiumCustomer")]
        // GET: Songs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.songs
                .FirstOrDefaultAsync(m => m.songId == id);
            if (song == null)
            {
                return NotFound();
            }

            // Ownership check
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (song.UploadedByUserId != null && song.UploadedByUserId != currentUser!.Id)
            {
                return Forbid();
            }

            return View(song);
        }

        // POST: Songs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,PremiumCustomer")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var song = await _context.songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            // Ownership check
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (song.UploadedByUserId != null && song.UploadedByUserId != currentUser!.Id)
            {
                return Forbid();
            }

            _context.songs.Remove(song);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> Convert()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _userManager.AddToRoleAsync(user!, "PremiumCustomer");
            await _userManager.RemoveFromRoleAsync(user!, "Customer");
            await _userManager.UpdateAsync(user!);
            return View();
        }
        private bool SongExists(int id)
        {
            return _context.songs.Any(e => e.songId == id);
        }
    }
}
