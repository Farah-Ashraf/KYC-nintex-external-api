using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KYCNintexApi.Data;
using KYCNintexApi.Dtos;
using System.Text;

namespace KYCNintexApi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class DocumentsController : ControllerBase
    {
        private readonly KycDbContext _context;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(KycDbContext context, ILogger<DocumentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all documents for a specific customer by CustomerID, including Base64-encoded file binary content.
        /// </summary>
        /// <param name="customerId">The unique Customer ID.</param>
        /// <returns>List of document records with Base64 attachment strings.</returns>
        /// <response code="200">Returns customer documents successfully.</response>
        /// <response code="404">If the customer does not exist.</response>
        [HttpGet("api/customers/{customerId:int}/documents")]
        [ProducesResponseType(typeof(IEnumerable<CustomerDocumentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CustomerDocumentResponseDto>>> GetDocumentsByCustomerId(int customerId)
        {
            _logger.LogInformation("Fetching documents for CustomerID: {CustomerID}", customerId);

            bool customerExists = await _context.Customers.AnyAsync(c => c.CustomerID == customerId);
            if (!customerExists)
            {
                _logger.LogWarning("Customer with CustomerID {CustomerID} not found.", customerId);
                return NotFound(new { message = $"Customer with CustomerID {customerId} was not found." });
            }

            var documents = await _context.CustomerDocuments
                .AsNoTracking()
                .Where(d => d.CustomerID == customerId)
                .ToListAsync();

            var resultList = new List<CustomerDocumentResponseDto>();

            foreach (var doc in documents)
            {
                string base64Content = await GetFileContentAsBase64Async(doc.FilePath, doc.FileName);

                resultList.Add(new CustomerDocumentResponseDto
                {
                    DocumentID = doc.DocumentID,
                    CustomerID = doc.CustomerID,
                    DocumentType = doc.DocumentType,
                    FileName = doc.FileName,
                    FilePath = doc.FilePath,
                    IssueDate = doc.IssueDate.ToString("yyyy-MM-dd"),
                    ExpiryDate = doc.ExpiryDate.ToString("yyyy-MM-dd"),
                    Status = doc.Status,
                    FileContentBase64 = base64Content
                });
            }

            return Ok(resultList);
        }

        /// <summary>
        /// Retrieves customer documents by customerId query parameter (Nintex integration query endpoint).
        /// </summary>
        /// <param name="customerId">The unique Customer ID.</param>
        [HttpGet("api/documents")]
        [ProducesResponseType(typeof(IEnumerable<CustomerDocumentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CustomerDocumentResponseDto>>> GetDocumentsByQuery([FromQuery] int customerId)
        {
            return await GetDocumentsByCustomerId(customerId);
        }

        /// <summary>
        /// Retrieves a single document by DocumentID with Base64 encoded file content.
        /// </summary>
        /// <param name="documentId">The unique Document ID.</param>
        [HttpGet("api/documents/{documentId:int}")]
        [ProducesResponseType(typeof(CustomerDocumentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDocumentResponseDto>> GetDocumentById(int documentId)
        {
            var doc = await _context.CustomerDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DocumentID == documentId);

            if (doc == null)
            {
                return NotFound(new { message = $"Document with DocumentID {documentId} was not found." });
            }

            string base64Content = await GetFileContentAsBase64Async(doc.FilePath, doc.FileName);

            var dto = new CustomerDocumentResponseDto
            {
                DocumentID = doc.DocumentID,
                CustomerID = doc.CustomerID,
                DocumentType = doc.DocumentType,
                FileName = doc.FileName,
                FilePath = doc.FilePath,
                IssueDate = doc.IssueDate.ToString("yyyy-MM-dd"),
                ExpiryDate = doc.ExpiryDate.ToString("yyyy-MM-dd"),
                Status = doc.Status,
                FileContentBase64 = base64Content
            };

            return Ok(dto);
        }

        private async Task<string> GetFileContentAsBase64Async(string filePath, string fileName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
                {
                    byte[] bytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    return Convert.ToBase64String(bytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file from path: {FilePath}", filePath);
            }

            // Fallback content if file path does not exist on disk
            byte[] fallbackBytes = Encoding.UTF8.GetBytes($"[ATTACHMENT DATA FOR {fileName}]");
            return Convert.ToBase64String(fallbackBytes);
        }
    }
}
