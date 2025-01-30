using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NewWebApplicationProject.Models;
using PharmacyApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace PharmacyApp.Controllers
{
    public class InventoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public InventoriesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Inventories
        public async Task<IActionResult> Index()
        {
            var inventories = await _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.Pharmacy)
                .OrderBy(i => i.Pharmacy.PharmacyName)
                .ThenBy(i => i.Medicine.Name)
                .Select(i => new Inventory
                {
                    InventoryId = i.InventoryId,
                    PharmacyId = i.PharmacyId,
                    MedicineId = i.MedicineId,
                    Quantity = i.Quantity,
                    LastUpdated = i.LastUpdated,
                    Pharmacy = new Pharmacy
                    {
                        PharmacyName = i.Pharmacy.PharmacyName
                    },
                    Medicine = new Medicine
                    {
                        Name = i.Medicine.Name
                    }
                })
                .ToListAsync();

            return View(inventories);
        }

        // GET: Inventories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.Pharmacy)
                .FirstOrDefaultAsync(m => m.InventoryId == id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        // GET: Inventories/Create
        public IActionResult Create()
        {
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "MedicineId");
            ViewData["PharmacyId"] = new SelectList(_context.Pharmacies, "PharmacyId", "PharmacyId");
            return View();
        }

        // POST: Inventories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InventoryId,PharmacyId,MedicineId,Quantity,LastUpdated")] Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inventory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "MedicineId", inventory.MedicineId);
            ViewData["PharmacyId"] = new SelectList(_context.Pharmacies, "PharmacyId", "PharmacyId", inventory.PharmacyId);
            return View(inventory);
        }

        [HttpGet]
        [Authorize(Roles = "Pharmacy")]
        public async Task<IActionResult> PharmacyInventory()
        {
            var user = await _userManager.GetUserAsync(User);
            var pharmacy = await _context.Pharmacies
                .FirstOrDefaultAsync(p => p.Email == user.Email);

            if (pharmacy != null)
            {
                var inventories = await _context.Inventories
                    .Include(i => i.Medicine)
                    .Include(i => i.Pharmacy)
                    .Where(i => i.PharmacyId == pharmacy.PharmacyId)
                    .ToListAsync();
                return View(inventories);
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Pharmacy")]
        public IActionResult CreateForPharmacy()
        {
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pharmacy")]
        public async Task<IActionResult> CreateForPharmacy([Bind("MedicineId,Quantity")] Inventory inventory)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }

                ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "Name");
                return View(inventory);
            }

            // Get the logged-in user's email
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("", "Logged-in user not found.");
                ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "Name");
                return View(inventory);
            }

            // Find the pharmacy associated with the logged-in user's email
            var pharmacy = await _context.Pharmacies
                .FirstOrDefaultAsync(p => p.Email == user.Email);

            if (pharmacy == null)
            {
                Console.WriteLine($"No pharmacy found for email: {user.Email}");
                ModelState.AddModelError("", $"No pharmacy found for email {user.Email}");
                ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "Name");
                return View(inventory);
            }

            try
            {
                // Assign the pharmacy ID to the inventory
                inventory.PharmacyId = pharmacy.PharmacyId;
                inventory.LastUpdated = DateTime.Now;

                Console.WriteLine($"PharmacyId set to: {inventory.PharmacyId}");

                // Add the inventory to the database
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();

                // Redirect to the pharmacy's inventory list
                return RedirectToAction(nameof(PharmacyInventory));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving inventory: {ex.Message}");
                ModelState.AddModelError("", $"Error saving inventory: {ex.Message}");
                ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "Name");
                return View(inventory);
            }
        }





        // GET: Inventories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "MedicineId", inventory.MedicineId);
            ViewData["PharmacyId"] = new SelectList(_context.Pharmacies, "PharmacyId", "PharmacyId", inventory.PharmacyId);
            return View(inventory);
        }

        // POST: Inventories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InventoryId,PharmacyId,MedicineId,Quantity,LastUpdated")] Inventory inventory)
        {
            if (id != inventory.InventoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryExists(inventory.InventoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(PharmacyInventory));
            }
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "MedicineId", inventory.MedicineId);
            ViewData["PharmacyId"] = new SelectList(_context.Pharmacies, "PharmacyId", "PharmacyId", inventory.PharmacyId);
            return View(inventory);
        }

        // GET: Inventories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.Pharmacy)
                .FirstOrDefaultAsync(m => m.InventoryId == id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        // POST: Inventories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PharmacyInventory));
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventories.Any(e => e.InventoryId == id);
        }
    }
}
