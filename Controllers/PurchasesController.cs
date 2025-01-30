using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewWebApplicationProject.Models;
using PharmacyApp.Data;

public class PurchasesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public PurchasesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Purchases
    [Authorize(Roles = "Pharmacy")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("User not found");

        var pharmacy = await _context.Pharmacies
            .FirstOrDefaultAsync(p => p.Email == user.Email);

        if (pharmacy == null)
            return NotFound($"No pharmacy found for email: {user.Email}");

        var purchases = await _context.Purchases
            .Include(p => p.Patient)
            .Include(p => p.Medicine)
            .Include(p => p.Pharmacy)
            .Where(p => p.PharmacyId == pharmacy.PharmacyId)
            .OrderBy(p => p.IsReceived)
            .ThenByDescending(p => p.PurchaseDate)
            .ToListAsync();

        ViewBag.PharmacyName = pharmacy.PharmacyName;
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

    // GET: Purchases/Details/5
    [Authorize(Roles = "Pharmacy")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var pharmacy = await _context.Pharmacies
            .FirstOrDefaultAsync(p => p.Email == user.Email);

        var purchase = await _context.Purchases
            .Include(p => p.Medicine)
            .Include(p => p.Patient)
            .Include(p => p.Pharmacy)
            .FirstOrDefaultAsync(m => m.PurchaseId == id && m.PharmacyId == pharmacy.PharmacyId);

        if (purchase == null) return NotFound();

        return View(purchase);
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

    // GET: Purchases/Edit/5
    [Authorize(Roles = "Pharmacy")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var pharmacy = await _context.Pharmacies
            .FirstOrDefaultAsync(p => p.Email == user.Email);

        var purchase = await _context.Purchases
            .Include(p => p.Patient)
            .Include(p => p.Medicine)
            .FirstOrDefaultAsync(p => p.PurchaseId == id && p.PharmacyId == pharmacy.PharmacyId);

        if (purchase == null) return NotFound();

        ViewBag.PatientName = purchase.Patient?.Name;
        ViewBag.MedicineName = purchase.Medicine?.Name;
        ViewBag.CurrentQuantity = purchase.Quantity;

        return View(purchase);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Pharmacy")]
    public async Task<IActionResult> Edit(int id, [Bind("PurchaseId,PatientId,PharmacyId,MedicineId,Quantity,PurchaseDate,IsReceived")] Purchase purchase)
    {
        if (id != purchase.PurchaseId)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var pharmacy = await _context.Pharmacies
            .FirstOrDefaultAsync(p => p.Email == user.Email);

        if (pharmacy == null || purchase.PharmacyId != pharmacy.PharmacyId)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
             
                var existingPurchase = await _context.Purchases.FindAsync(id);
                if (existingPurchase == null)
                    return NotFound();

              
                existingPurchase.Quantity = purchase.Quantity;
                existingPurchase.IsReceived = purchase.IsReceived;

                _context.Update(existingPurchase);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseExists(purchase.PurchaseId))
                    return NotFound();
                else
                    throw;
            }
        }

       
        var purchaseToRedisplay = await _context.Purchases
            .Include(p => p.Patient)
            .Include(p => p.Medicine)
            .FirstOrDefaultAsync(p => p.PurchaseId == id);

        ViewBag.PatientName = purchaseToRedisplay?.Patient?.Name;
        ViewBag.MedicineName = purchaseToRedisplay?.Medicine?.Name;
        return View(purchase);
    }

    // GET: Purchases/Delete/5
    [Authorize(Roles = "Pharmacy")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var pharmacy = await _context.Pharmacies
            .FirstOrDefaultAsync(p => p.Email == user.Email);

        var purchase = await _context.Purchases
            .Include(p => p.Medicine)
            .Include(p => p.Patient)
            .Include(p => p.Pharmacy)
            .FirstOrDefaultAsync(m => m.PurchaseId == id && m.PharmacyId == pharmacy.PharmacyId);

        if (purchase == null) return NotFound();

        return View(purchase);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Pharmacy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var pharmacy = await _context.Pharmacies
            .FirstOrDefaultAsync(p => p.Email == user.Email);

        var purchase = await _context.Purchases
            .FirstOrDefaultAsync(p => p.PurchaseId == id && p.PharmacyId == pharmacy.PharmacyId);

        if (purchase != null)
        {
            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool PurchaseExists(int id)
    {
        return _context.Purchases.Any(e => e.PurchaseId == id);
    }

    public async Task<IActionResult> ByPatient(int? id)
    {
        if (id == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var pharmacy = await _context.Pharmacies
            .FirstOrDefaultAsync(p => p.Email == user.Email);

        var purchases = await _context.Purchases
            .Include(p => p.Medicine)
            .Include(p => p.Patient)
            .Include(p => p.Pharmacy)
            .Where(p => p.PatientId == id && p.PharmacyId == pharmacy.PharmacyId)
            .ToListAsync();

        if (!purchases.Any())
            return NotFound("No purchases found for the specified patient ID.");

        ViewData["PatientName"] = purchases.FirstOrDefault()?.Patient?.Name ?? "Unknown Patient";
        return View(purchases);
    }
}