using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KYCNintexApi.Models
{
    [Table("CustomerDocuments")]
    public class CustomerDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("DocumentID")]
        public int DocumentID { get; set; }

        [Required]
        [Column("CustomerID")]
        public int CustomerID { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("DocumentType")]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("FileName")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [Column("FilePath")]
        public string FilePath { get; set; } = string.Empty;

        [Column("IssueDate", TypeName = "date")]
        public DateTime IssueDate { get; set; }

        [Column("ExpiryDate", TypeName = "date")]
        public DateTime ExpiryDate { get; set; }

        [MaxLength(50)]
        [Column("Status")]
        public string Status { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("CustomerID")]
        public Customer? Customer { get; set; }
    }
}
