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

        public AIImagesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: AIImages
        public async Task<IActionResult> Index()
        {
            return View(await _context.AIImages.ToListAsync());
        }

        // GET: AIImages/Details/5
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
            // Pre-fill the read-only fields
            var model = new AIImage { UploadDate = DateTime.Now, Like = 0 };
            return View(model);
        }

        // POST: AIImages/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Prompt,ImageGenerator")] AIImage aIImage, IFormFile? ImageFile)
        {
            // The server sets these, so users can't change them
            aIImage.UploadDate = DateTime.Now;
            aIImage.Like = 0;
            aIImage.canIncreaseLike = true;
            ModelState.Remove(nameof(AIImage.Filename));

            // Validate the uploaded file
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
                // Save the file to wwwroot/uploads with a unique name
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
                // POST: AIImages/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aIImage = await _context.AIImages.FindAsync(id);
            if (aIImage != null)
            {
                // Remove the uploaded file too, if it exists
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
    }
}
