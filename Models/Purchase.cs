using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewWebApplicationProject.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }

        [Required]
        public int? PatientId { get; set; } // Foreign key to Patient
        public Patient? Patient { get; set; }

        [Required]
        public int? PharmacyId { get; set; } // Foreign key to Pharmacy
        public Pharmacy? Pharmacy { get; set; }

        [Required]
        public int? MedicineId { get; set; } // Foreign key to Medicine
        public Medicine? Medicine { get; set; }

        public int Quantity { get; set; } // Quantity of medicine purchased
        
        public DateTime PurchaseDate { get; set; } // Date of purchase

        public bool IsReceived { get; set; }


    }

}
