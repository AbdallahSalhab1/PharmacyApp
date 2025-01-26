// Models/Inventory.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewWebApplicationProject.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }

        // Foreign key to Pharmacy
        
        public int? PharmacyId { get; set; }
        public Pharmacy? Pharmacy { get; set; } // Navigation property

        // Foreign key to Medicine
        [Required]
        public int? MedicineId { get; set; }
        public Medicine? Medicine { get; set; } // Navigation property

        public int Quantity { get; set; } // Stock quantity
        public DateTime LastUpdated { get; set; } // Last stock update timestamp
    }

}