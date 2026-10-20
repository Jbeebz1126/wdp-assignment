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

            ValidateUpload(ImageFile, required: true);

            if (!ModelState.IsValid)
            {
                return View(aIImage);
            }

            try
            {
                aIImage.Filename = await SaveUploadAsync(ImageFile!);
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

            var aIImage = await _context.AIImages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (aIImage == null)
            {
                return NotFound();
            }

            // Upload date is automatically set to now (shown read-only)
            aIImage.UploadDate = DateTime.Now;
            return View(aIImage);
        }

        // POST: AIImages/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Prompt,ImageGenerator")] AIImage input, IFormFile? ImageFile)
        {
            if (id != input.Id)
            {
                return NotFound();
            }

            var existing = await _context.AIImages.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var originalFilename = existing.Filename;
            ModelState.Remove(nameof(AIImage.Filename));

            // A new image is optional when editing
            ValidateUpload(ImageFile, required: false);

            if (!ModelState.IsValid)
            {
                input.Filename = originalFilename;
                input.Like = existing.Like;
                input.UploadDate = DateTime.Now;
                return View(input);
            }

            try
            {
                existing.Prompt = input.Prompt;
                existing.ImageGenerator = input.ImageGenerator;
                existing.UploadDate = DateTime.Now;

                bool replacedImage = false;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    existing.Filename = await SaveUploadAsync(ImageFile);
                    replacedImage = true;
                }

                await _context.SaveChangesAsync();

                // Only remove the old file once the database has been updated
                if (replacedImage)
                {
                    DeleteUploadFile(originalFilename);
                }

                return RedirectToAction(nameof(Index), null, "img-" + existing.Id);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Sorry, the changes could not be saved. Please try again.");
                input.Filename = originalFilename;
                input.Like = existing.Like;
                input.UploadDate = DateTime.Now;
                return View(input);
            }
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
            if (aIImage == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var filename = aIImage.Filename;
                _context.AIImages.Remove(aIImage);
                await _context.SaveChangesAsync();
                DeleteUploadFile(filename);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Sorry, this image could not be deleted. Please try again.");
                return View("Delete", aIImage);
            }
        }

        // ---------- Helpers ----------

        // Checks the uploaded file's type and size
        private void ValidateUpload(IFormFile? file, bool required)
        {
            if (file == null || file.Length == 0)
            {
                if (required)
                {
                    ModelState.AddModelError(string.Empty, "Please choose an image file to upload.");
                }
                return;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Only JPG, PNG, GIF or WEBP images are allowed.");
            }
            else if (file.Length > MaxFileSize)
            {
                ModelState.AddModelError(string.Empty, "The image must be 5 MB or smaller.");
            }
        }

        // Saves the file to wwwroot/uploads with a unique name and returns that name
        private async Task<string> SaveUploadAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLowerInvariant();
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return uniqueName;
        }

        // Removes a file from wwwroot/uploads; never crashes the site if it can't
        private void DeleteUploadFile(string? filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            try
            {
                var filePath = Path.Combine(_env.WebRootPath, "uploads", filename);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception)
            {
                // Ignore: the record is already updated/removed
            }
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
    }
}
