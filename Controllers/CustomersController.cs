using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KYCNintexApi.Data;
using KYCNintexApi.Dtos;
using KYCNintexApi.Models;

namespace KYCNintexApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private readonly KycDbContext _context;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(KycDbContext context, ILogger<CustomersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves detailed information for a specific customer by CustomerID.
        /// </summary>
        /// <param name="id">The unique Customer ID.</param>
        /// <returns>Customer information record.</returns>
        /// <response code="200">Returns customer details successfully.</response>
        /// <response code="404">If no customer is found with the provided ID.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerResponseDto>> GetCustomerById(int id)
        {
            _logger.LogInformation("Fetching customer info for CustomerID: {CustomerID}", id);

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
            {
                _logger.LogWarning("Customer with CustomerID {CustomerID} not found.", id);
                return NotFound(new { message = $"Customer with CustomerID {id} was not found." });
            }

            var dto = new CustomerResponseDto
            {
                CustomerID = customer.CustomerID,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth.ToString("yyyy-MM-dd"),
                PermanentAddress = customer.PermanentAddress,
                MailingAddress = customer.MailingAddress,
                Phone = customer.Phone,
                Email = customer.Email,
                Branch = customer.Branch,
                Status = customer.Status,
                DateOfInitiation = customer.DateOfInitiation,
                LastUpdatedDate = customer.LastUpdatedDate
            };

            return Ok(dto);
        }

        /// <summary>
        /// Creates a customer and stores uploaded documents in the database.
        /// </summary>
        /// <param name="request">Customer fields and document files submitted as multipart/form-data.</param>
        /// <response code="201">The customer and documents were created successfully.</response>
        /// <response code="400">The request is invalid or contains no documents.</response>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CreateCustomerResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateCustomerResponseDto>> CreateCustomer([FromForm] CreateCustomerRequestDto request)
        {
            var uploadedFiles = Request.Form.Files;

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                ModelState.AddModelError(nameof(request.FullName), "FullName is required.");
            }

            if (uploadedFiles.Count == 0)
            {
                ModelState.AddModelError(nameof(request.Documents), "At least one document is required.");
            }

            if (request.DocumentTypes.Count > 0 && request.DocumentTypes.Count != uploadedFiles.Count)
            {
                ModelState.AddModelError(nameof(request.DocumentTypes), "Provide one DocumentTypes value for each document.");
            }

            if (request.IssueDates.Count > 0 && request.IssueDates.Count != uploadedFiles.Count)
            {
                ModelState.AddModelError(nameof(request.IssueDates), "Provide one IssueDates value for each document.");
            }

            if (request.ExpiryDates.Count > 0 && request.ExpiryDates.Count != uploadedFiles.Count)
            {
                ModelState.AddModelError(nameof(request.ExpiryDates), "Provide one ExpiryDates value for each document.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var customer = new Customer
            {
                FullName = request.FullName.Trim(),
                DateOfBirth = request.DateOfBirth,
                PermanentAddress = request.PermanentAddress,
                MailingAddress = request.MailingAddress,
                Phone = request.Phone,
                Email = request.Email,
                Branch = request.Branch,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending Review" : request.Status,
                DateOfInitiation = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var documents = new List<CustomerDocument>();
            for (int index = 0; index < uploadedFiles.Count; index++)
            {
                var uploadedFile = uploadedFiles[index];
                if (uploadedFile.Length == 0)
                {
                    ModelState.AddModelError($"Documents[{index}]", "The document file cannot be empty.");
                    continue;
                }

                await using var stream = new MemoryStream();
                await uploadedFile.CopyToAsync(stream);

                documents.Add(new CustomerDocument
                {
                    CustomerID = customer.CustomerID,
                    DocumentType = GetValue(request.DocumentTypes, index, "Other"),
                    FileName = Path.GetFileName(uploadedFile.FileName),
                    FileContent = stream.ToArray(),
                    IssueDate = GetDateValue(request.IssueDates, index),
                    ExpiryDate = GetDateValue(request.ExpiryDates, index),
                    Status = GetValue(request.DocumentStatuses, index, "Pending Verification")
                });
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                return ValidationProblem(ModelState);
            }

            _context.CustomerDocuments.AddRange(documents);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = new CreateCustomerResponseDto
            {
                Customer = ToCustomerResponse(customer),
                Documents = documents.Select(ToDocumentResponse).ToList()
            };

            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerID }, response);
        }

        private static string GetValue(List<string> values, int index, string defaultValue)
        {
            return index < values.Count && !string.IsNullOrWhiteSpace(values[index])
                ? values[index]
                : defaultValue;
        }

        private static DateTime GetDateValue(List<DateTime?> values, int index)
        {
            return values.Count > index && values[index].HasValue
                ? values[index]!.Value
                : DateTime.UtcNow.Date;
        }

        private static CustomerResponseDto ToCustomerResponse(Customer customer)
        {
            return new CustomerResponseDto
            {
                CustomerID = customer.CustomerID,
                FullName = customer.FullName,
                DateOfBirth = customer.DateOfBirth.ToString("yyyy-MM-dd"),
                PermanentAddress = customer.PermanentAddress,
                MailingAddress = customer.MailingAddress,
                Phone = customer.Phone,
                Email = customer.Email,
                Branch = customer.Branch,
                Status = customer.Status,
                DateOfInitiation = customer.DateOfInitiation,
                LastUpdatedDate = customer.LastUpdatedDate
            };
        }

        private static CustomerDocumentResponseDto ToDocumentResponse(CustomerDocument document)
        {
            return new CustomerDocumentResponseDto
            {
                DocumentID = document.DocumentID,
                CustomerID = document.CustomerID,
                DocumentType = document.DocumentType,
                FileName = document.FileName,
                IssueDate = document.IssueDate.ToString("yyyy-MM-dd"),
                ExpiryDate = document.ExpiryDate.ToString("yyyy-MM-dd"),
                Status = document.Status,
                FileContentBase64 = Convert.ToBase64String(document.FileContent)
            };
        }

    }
}
