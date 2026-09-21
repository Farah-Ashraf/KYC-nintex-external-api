namespace KYCNintexApi.Dtos
{
    public class CustomerDocumentListResponseDto
    {
        public List<CustomerDocumentResponseDto> Documents { get; set; } = new List<CustomerDocumentResponseDto>();
    }
}
