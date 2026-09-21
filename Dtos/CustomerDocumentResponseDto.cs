namespace KYCNintexApi.Dtos
{
    public class CustomerDocumentResponseDto
    {
        public int DocumentID { get; set; }
        public int CustomerID { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string IssueDate { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Base64 encoded file content binary wrapped inside JSON for Nintex buffer processing.
        /// </summary>
        public string FileContentBase64 { get; set; } = string.Empty;
    }
}
