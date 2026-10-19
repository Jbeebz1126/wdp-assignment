using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WDP_Assignment.Data;
using WDP_Assignment.Models;

namespace WDP_Assignment.Controllers
{
    public class AIImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        private const string LikedKey = "LikedImageIds";

        public AIImagesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: AIImages  (sorted by likes, highest first)
        public async Task<IActionResult> Index()
        {
            var images = await _context.AIImages
                .AsNoTracking()
                .OrderByDescending(i => i.Like)
                .ThenByDescending(i => i.UploadDate)
                .ToListAsync();

            // Mark which images this user can still like in this login
            var liked = GetLikedIds();
            foreach (var img in images)
            {
                img.canIncreaseLike = !liked.Contains(img.Id);
            }

            return View(images);
        }

        // POST: AIImages/Like/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var liked = GetLikedIds();
            if (!liked.Contains(id))
            {
                var image = await _context.AIImages.FindAsync(id);
                if (image != null)
                {
                    image.Like++;
                    await _context.SaveChangesAsync();

                    liked.Add(id);
                    HttpContext.Session.SetString(LikedKey, string.Join(",", liked));
                }
            }

            // Return to the list and jump to the liked image (#img-5)
            return RedirectToAction(nameof(Index), null, "img-" + id);
        }

        // GET: AIImages/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aIImage = await _context.AIImages.FirstOrDefaultAsync(m => m.Id == id);
            if (aIImage == null)
            {
                return NotFound();
            }

            return View(aIImage);
        }

        // GET: AIImages/Create
        [Authorize]
        public IActionResult Create()
        {
            var model = new AIImage { UploadDate = DateTime.Now, Like = 0 };
            return View(model);
        }

        // POST: AIImages/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Prompt,ImageGenerator")] AIImage aIImage, IFormFile? ImageFile)
        {
            aIImage.UploadDate = DateTime.Now;
            aIImage.Like = 0;
            aIImage.canIncreaseLike = true;
            ModelState.Remove(nameof(AIImage.Filename));

            if (ImageFile == null || ImageFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please choose an image file to upload.");
            }
            else
            {
                var checkExt = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(checkExt))
                {
                    ModelState.AddModelError(string.Empty, "Only JPG, PNG, GIF or WEBP images are allowed.");
                }
                else if (ImageFile.Length > MaxFileSize)
                {
                    ModelState.AddModelError(string.Empty, "The image must be 5 MB or smaller.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(aIImage);
            }

            try
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(ImageFile!.FileName).ToLowerInvariant();
                var uniqueName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                aIImage.Filename = uniqueName;
                _context.Add(aIImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Sorry, the image could not be saved. Please try again.");
                return View(aIImage);
            }
        }

        // GET: AIImages/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aIImage = await _context.AIImages.FindAsync(id);
            if (aIImage == null)
            {
                return NotFound();
            }
            return View(aIImage);
        }

        // POST: AIImages/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Prompt,ImageGenerator,UploadDate,Filename,Like,canIncreaseLike")] AIImage aIImage)
        {
            if (id != aIImage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aIImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AIImageExists(aIImage.Id))
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
            return View(aIImage);
        }

        // GET: AIImages/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aIImage = await _context.AIImages.FirstOrDefaultAsync(m => m.Id == id);
            if (aIImage == null)
            {
                return NotFound();
            }

            return View(aIImage);
        }

        // POST: AIImages/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aIImage = await _context.AIImages.FindAsync(id);
            if (aIImage != null)
            {
                if (!string.IsNullOrEmpty(aIImage.Filename))
                {
                    try
                    {
                        var filePath = Path.Combine(_env.WebRootPath, "uploads", aIImage.Filename);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    catch (Exception)
                    {
                        // If the file can't be removed, still delete the record
                    }
                }

                _context.AIImages.Remove(aIImage);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Reads the list of liked image IDs stored in the session
        private List<int> GetLikedIds()
        {
            var raw = HttpContext.Session.GetString(LikedKey);
            if (string.IsNullOrEmpty(raw))
            {
                return new List<int>();
            }

            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => int.TryParse(s, out var n) ? n : -1)
                      .Where(n => n > 0)
                      .ToList();
        }

        private bool AIImageExists(int id)
        {
            return _context.AIImages.Any(e => e.Id == id);
        }
    }
}
