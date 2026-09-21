using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KYCNintexApi.Models
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("CustomerID")]
        public int CustomerID { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;

        [Column("DateOfBirth", TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        [Column("PermanentAddress")]
        public string PermanentAddress { get; set; } = string.Empty;

        [Column("MailingAddress")]
        public string MailingAddress { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("Phone")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("Branch")]
        public string Branch { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("Status")]
        public string Status { get; set; } = string.Empty;

        [Column("DateOfInitiation")]
        public DateTime DateOfInitiation { get; set; } = DateTime.UtcNow;

        [Column("LastUpdatedDate")]
        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<CustomerDocument> Documents { get; set; } = new List<CustomerDocument>();
    }
}
