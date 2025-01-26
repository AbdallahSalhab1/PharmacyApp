using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NewWebApplicationProject.Models;
using PharmacyApp.Data;
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
    public class PharmaciesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PharmaciesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Pharmacies
        public async Task<IActionResult> Index()
        {   
            return View(await _context.Pharmacies.ToListAsync());
        }


        // GET: Pharmacies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pharmacy = await _context.Pharmacies
                .FirstOrDefaultAsync(m => m.PharmacyId == id);
            if (pharmacy == null)
            {
                return NotFound();
            }

            return View(pharmacy);
        }

        // GET: Pharmacies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pharmacies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PharmacyId,LicenseNumber,PharmacyName,IsVerified,Address,PhoneNumber,Email")] Pharmacy pharmacy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pharmacy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pharmacy);
        }


        [HttpGet]
        [Route("Pharmacies/ViewAnalytics/{id?}")]
        public async Task<IActionResult> ViewAnalytics(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pharmacy = await _context.Pharmacies
                .Include(p => p.Inventory)
                    .ThenInclude(i => i.Medicine)
                .Include(p => p.Purchases)
                    .ThenInclude(p => p.Medicine)
                .FirstOrDefaultAsync(m => m.PharmacyId == id);

            if (pharmacy == null)
            {
                return NotFound();
            }

            // Calculate analytics
            ViewBag.TotalMedicines = pharmacy.Inventory.Select(i => i.MedicineId).Distinct().Count();
            ViewBag.TotalStock = pharmacy.Inventory.Sum(i => i.Quantity);
            ViewBag.LowStockItems = pharmacy.Inventory.Count(i => i.Quantity < 10);
            ViewBag.OutOfStockItems = pharmacy.Inventory.Count(i => i.Quantity == 0);
            ViewBag.TotalPurchases = pharmacy.Purchases.Count();

            // Calculate monthly purchases for the chart
            var monthlyPurchases = pharmacy.Purchases
                .GroupBy(p => p.PurchaseDate.Month)
                .OrderBy(g => g.Key)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();

            // Create arrays for labels and data
            var months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var purchaseData = new int[12];

            foreach (var mp in monthlyPurchases)
            {
                purchaseData[mp.Month - 1] = mp.Count;
            }

            ViewBag.MonthLabels = months;
            ViewBag.MonthlyPurchases = purchaseData;

            return View(pharmacy);
        }

        [HttpGet]
        [Authorize(Roles = "Pharmacy")]
        [Route("Pharmacies/ViewMyAnalytics")]
        public async Task<IActionResult> ViewMyAnalytics()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Logged-in user not found.");
            }

            var pharmacy = await _context.Pharmacies
                .Include(p => p.Inventory)
                    .ThenInclude(i => i.Medicine)
                .Include(p => p.Purchases)
                    .ThenInclude(p => p.Medicine)
                .FirstOrDefaultAsync(p => p.Email == user.Email);

            if (pharmacy == null)
            {
                return NotFound($"No pharmacy found for email: {user.Email}");
            }

            // Calculate analytics
            ViewBag.TotalMedicines = pharmacy.Inventory.Select(i => i.MedicineId).Distinct().Count();
            ViewBag.TotalStock = pharmacy.Inventory.Sum(i => i.Quantity);
            ViewBag.LowStockItems = pharmacy.Inventory.Count(i => i.Quantity < 10);
            ViewBag.OutOfStockItems = pharmacy.Inventory.Count(i => i.Quantity == 0);
            ViewBag.TotalPurchases = pharmacy.Purchases.Count();

            // Calculate monthly purchases for the chart
            var monthlyPurchases = pharmacy.Purchases
                .GroupBy(p => p.PurchaseDate.Month)
                .OrderBy(g => g.Key)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();

            // Create arrays for labels and data
            var months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var purchaseData = new int[12];

            foreach (var mp in monthlyPurchases)
            {
                purchaseData[mp.Month - 1] = mp.Count;
            }

            ViewBag.MonthLabels = months;
            ViewBag.MonthlyPurchases = purchaseData;

            return View("ViewAnalytics", pharmacy);
        }


        // GET: Pharmacies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pharmacy = await _context.Pharmacies.FindAsync(id);
            if (pharmacy == null)
            {
                return NotFound();
            }
            return View(pharmacy);
        }

        // POST: Pharmacies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PharmacyId,LicenseNumber,PharmacyName,IsVerified,Address,PhoneNumber,Email")] Pharmacy pharmacy)
        {
            if (id != pharmacy.PharmacyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pharmacy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PharmacyExists(pharmacy.PharmacyId))
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
            return View(pharmacy);
        }

        // GET: Pharmacies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pharmacy = await _context.Pharmacies
                .FirstOrDefaultAsync(m => m.PharmacyId == id);
            if (pharmacy == null)
            {
                return NotFound();
            }

            return View(pharmacy);
        }



        // POST: Pharmacies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pharmacy = await _context.Pharmacies.FindAsync(id);
            if (pharmacy != null)
            {
                _context.Pharmacies.Remove(pharmacy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PharmacyExists(int id)
        {
            return _context.Pharmacies.Any(e => e.PharmacyId == id);
        }
    }
}
