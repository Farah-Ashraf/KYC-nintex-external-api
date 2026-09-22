using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KYCNintexApi.Data;
using KYCNintexApi.Dtos;

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

    }
}
