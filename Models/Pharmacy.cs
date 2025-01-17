using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewWebApplicationProject.Models
{
    public class Pharmacy
    {
        public int PharmacyId { get; set; }
        public string LicenseNumber { get; set; }

        [Required]
        public string PharmacyName { get; set; }
        public bool IsVerified { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        // Navigation property for inventory
        public virtual ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }


}