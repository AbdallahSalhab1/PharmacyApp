using System.ComponentModel.DataAnnotations.Schema;

namespace NewWebApplicationProject.Models
{
    public class Medicine
    {
        public int MedicineId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string DosageForm { get; set; } // E.g., Tablet, Syrup
        public string Strength { get; set; } // E.g., 500mg
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public bool RequiresPrescription { get; set; }
        public double Price { get; set; }

        // Navigation property for inventory
        public virtual ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }

}
