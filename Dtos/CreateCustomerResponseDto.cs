namespace KYCNintexApi.Dtos
{
    public class CreateCustomerResponseDto
    {
        public CustomerResponseDto Customer { get; set; } = new();
        public List<CustomerDocumentResponseDto> Documents { get; set; } = new();
    }
}
