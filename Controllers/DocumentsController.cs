using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KYCNintexApi.Data;
using KYCNintexApi.Dtos;

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
        [HttpGet("customers/{customerId:int}/documents")]
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
                resultList.Add(new CustomerDocumentResponseDto
                {
                    DocumentID = doc.DocumentID,
                    CustomerID = doc.CustomerID,
                    DocumentType = doc.DocumentType,
                    FileName = doc.FileName,
                    IssueDate = doc.IssueDate.ToString("yyyy-MM-dd"),
                    ExpiryDate = doc.ExpiryDate.ToString("yyyy-MM-dd"),
                    Status = doc.Status,
                    FileContentBase64 = Convert.ToBase64String(doc.FileContent)
                });
            }

            return Ok(resultList);
        }
    }
}
