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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace PharmacyApp.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PurchasesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CreateForPatient(int pharmacyId, int medicineId)
        {
            var pharmacy = await _context.Pharmacies.FindAsync(pharmacyId);
            var medicine = await _context.Medicines.FindAsync(medicineId);

            if (pharmacy == null || medicine == null)
                return NotFound();

            ViewData["PharmacyName"] = pharmacy.PharmacyName;
            ViewData["MedicineName"] = medicine.Name;
            ViewData["MedicinePrice"] = medicine.Price;

            var purchase = new Purchase
            {
                PharmacyId = pharmacyId,
                MedicineId = medicineId
            };

            return View(purchase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CreateForPatient([Bind("PharmacyId,MedicineId,Quantity")] Purchase purchase)
        {
            if (!ModelState.IsValid)
                return View(purchase);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(purchase);
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == user.Email);
            if (patient == null)
            {
                ModelState.AddModelError("", $"No patient found for email {user.Email}");
                return View(purchase);
            }

            try
            {
                purchase.PatientId = patient.PatientId;
                purchase.PurchaseDate = DateTime.Now;
                purchase.IsReceived = false;

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(History));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating purchase: {ex.Message}");
                return View(purchase);
            }
        }

        public async Task<IActionResult> ByPatient(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchases = await _context.Purchases
                .Include(p => p.Medicine)
                .Include(p => p.Patient)
                .Include(p => p.Pharmacy)
                .Where(p => p.PatientId == id)
                .ToListAsync();

            if (purchases == null || !purchases.Any())
            {
                return NotFound("No purchases found for the specified patient ID.");
            }

            ViewData["PatientName"] = _context.Patients.FirstOrDefault(p => p.PatientId == id)?.Name ?? "Unknown Patient";
            return View(purchases);
        }

        [HttpGet]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found");

            var patient = await _context.Patients
                .Include(p => p.Purchases)
                    .ThenInclude(p => p.Medicine)
                .Include(p => p.Purchases)
                    .ThenInclude(p => p.Pharmacy)
                .FirstOrDefaultAsync(p => p.Email == user.Email);

            if (patient == null) return NotFound($"No patient found for email: {user.Email}");

            var purchases = patient.Purchases.OrderBy(p => p.PurchaseDate).ToList();
            ViewData["PatientName"] = patient.Name;

            return View(purchases);
        }


        // GET: Purchases
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Purchases.Include(p => p.Medicine).Include(p => p.Patient).Include(p => p.Pharmacy);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Purchases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases
                .Include(p => p.Medicine)
                .Include(p => p.Patient)
                .Include(p => p.Pharmacy)
                .FirstOrDefaultAsync(m => m.PurchaseId == id);
            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // GET: Purchases/Create
        public IActionResult Create()
        {
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "MedicineId");
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "Name");
            ViewData["PharmacyId"] = new SelectList(_context.Pharmacies, "PharmacyId", "PharmacyName");
            return View();
        }

        // POST: Purchases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PurchaseId,PatientId,PharmacyId,MedicineId,Quantity,PurchaseDate,IsReceived")] Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                _context.Add(purchase);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "MedicineId", purchase.MedicineId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "Name", purchase.PatientId);
            ViewData["PharmacyId"] = new SelectList(_context.Pharmacies, "PharmacyId", "PharmacyName", purchase.PharmacyId);
            return View(purchase);
        }

        // GET: Purchases/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "MedicineId", purchase.MedicineId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "Name", purchase.PatientId);
            ViewData["PharmacyId"] = new SelectList(_context.Pharmacies, "PharmacyId", "PharmacyName", purchase.PharmacyId);
            return View(purchase);
        }

        // POST: Purchases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PurchaseId,PatientId,PharmacyId,MedicineId,Quantity,PurchaseDate,IsReceived")] Purchase purchase)
        {
            if (id != purchase.PurchaseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseExists(purchase.PurchaseId))
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
            ViewData["MedicineId"] = new SelectList(_context.Medicines, "MedicineId", "MedicineId", purchase.MedicineId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "Name", purchase.PatientId);
            ViewData["PharmacyId"] = new SelectList(_context.Pharmacies, "PharmacyId", "PharmacyName", purchase.PharmacyId);
            return View(purchase);
        }

        // GET: Purchases/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases
                .Include(p => p.Medicine)
                .Include(p => p.Patient)
                .Include(p => p.Pharmacy)
                .FirstOrDefaultAsync(m => m.PurchaseId == id);
            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // POST: Purchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase != null)
            {
                _context.Purchases.Remove(purchase);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseExists(int id)
        {
            return _context.Purchases.Any(e => e.PurchaseId == id);
        }
    }
}
