namespace KYCNintexApi.Dtos
{
    public class CustomerResponseDto
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
        public string MailingAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateOfInitiation { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
