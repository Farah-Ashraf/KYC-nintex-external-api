using Microsoft.AspNetCore.Http;

namespace KYCNintexApi.Dtos
{
    public class CreateCustomerRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PermanentAddress { get; set; } = string.Empty;
        public string MailingAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public IFormFile? Documents { get; set; }
        public List<string> DocumentTypes { get; set; } = new();
        public List<DateTime?> IssueDates { get; set; } = new();
        public List<DateTime?> ExpiryDates { get; set; } = new();
        public List<string> DocumentStatuses { get; set; } = new();
    }
}
