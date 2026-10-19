using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WDP_Assignment.Data;
using WDP_Assignment.Models;
using Microsoft.AspNetCore.Authorization;


namespace WDP_Assignment.Controllers
{
    public class AIImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AIImagesController(ApplicationDbContext context)
        {
            _context = context;
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

            var aIImage = await _context.AIImages
                .FirstOrDefaultAsync(m => m.Id == id);
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
            return View();
        }

        // POST: AIImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Prompt,ImageGenerator,UploadDate,Filename,Like,canIncreaseLike")] AIImage aIImage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aIImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aIImage);
        }

        // GET: AIImages/Edit/5
        [Authorize(Roles= "Admin")]
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles="Admin")]
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

            var aIImage = await _context.AIImages
                .FirstOrDefaultAsync(m => m.Id == id);
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
                _context.AIImages.Remove(aIImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AIImageExists(int id)
        {
            return _context.AIImages.Any(e => e.Id == id);
        }
    }
}
