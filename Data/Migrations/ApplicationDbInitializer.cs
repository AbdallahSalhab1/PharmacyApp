using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using NewWebApplicationProject.Models;

namespace PharmacyApp.Data
{
    public static class ApplicationDbInitializer
    {
        public static void Seed(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed Pharmacies
            if (!context.Pharmacies.Any())
            {
                var pharmacies = new List<Pharmacy>
                {
                    new Pharmacy
                    {
                        
                        LicenseNumber = "PH123456",
                        PharmacyName = "Health First Pharmacy",
                        IsVerified = true,
                        Address = "123 Main St, Springfield",
                        PhoneNumber = "123-456-7890",
                        Email = "contact@healthfirst.com"
                    },
                    new Pharmacy
                    {
                        
                        LicenseNumber = "PH654321",
                        PharmacyName = "WellCare Pharmacy",
                        IsVerified = true,
                        Address = "456 Elm St, Springfield",
                        PhoneNumber = "987-654-3210",
                        Email = "info@wellcare.com"
                    }
                };

                context.Pharmacies.AddRange(pharmacies);
                context.SaveChanges();
            }

            // Seed Medicines
            if (!context.Medicines.Any())
            {
                var medicines = new List<Medicine>
                {
                    new Medicine
                    {
                        
                        Name = "Paracetamol",
                        Brand = "PainRelief Co.",
                        DosageForm = "Tablet",
                        Strength = "500mg",
                        Description = "Used to treat mild to moderate pain and fever.",
                        Manufacturer = "MedPharm Inc.",
                        RequiresPrescription = false,
                        Price = 5.50
                    },
                    new Medicine
                    {
                        
                        Name = "Amoxicillin",
                        Brand = "Antibiotics Co.",
                        DosageForm = "Capsule",
                        Strength = "250mg",
                        Description = "Antibiotic used to treat bacterial infections.",
                        Manufacturer = "Global Pharma",
                        RequiresPrescription = true,
                        Price = 12.75
                    }
                };

                context.Medicines.AddRange(medicines);
                context.SaveChanges();
            }

            // Retrieve IDs for relationships
            var healthFirstPharmacyId = context.Pharmacies.FirstOrDefault(p => p.PharmacyName == "Health First Pharmacy")?.PharmacyId;
            var wellCarePharmacyId = context.Pharmacies.FirstOrDefault(p => p.PharmacyName == "WellCare Pharmacy")?.PharmacyId;

            var paracetamolId = context.Medicines.FirstOrDefault(m => m.Name == "Paracetamol")?.MedicineId;
            var amoxicillinId = context.Medicines.FirstOrDefault(m => m.Name == "Amoxicillin")?.MedicineId;

            // Seed Inventory
            if (!context.Inventories.Any())
            {
                var inventories = new List<Inventory>
                {
                    new Inventory
                    {
                        
                        PharmacyId = healthFirstPharmacyId ?? 0,
                        MedicineId = paracetamolId ?? 0,
                        Quantity = 100,
                        LastUpdated = DateTime.UtcNow
                    },
                    new Inventory
                    {
                        
                        PharmacyId = healthFirstPharmacyId ?? 0,
                        MedicineId = amoxicillinId ?? 0,
                        Quantity = 50,
                        LastUpdated = DateTime.UtcNow
                    },
                    new Inventory
                    {
                        
                        PharmacyId = wellCarePharmacyId ?? 0,
                        MedicineId = paracetamolId ?? 0,
                        Quantity = 200,
                        LastUpdated = DateTime.UtcNow
                    },
                    new Inventory
                    {
                        
                        PharmacyId = wellCarePharmacyId ?? 0,
                        MedicineId = amoxicillinId ?? 0,
                        Quantity = 75,
                        LastUpdated = DateTime.UtcNow
                    }
                };

                context.Inventories.AddRange(inventories);
                context.SaveChanges();
            }
        }
    }
}

