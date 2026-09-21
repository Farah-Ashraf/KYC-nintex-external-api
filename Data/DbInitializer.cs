using System.Text;
using KYCNintexApi.Models;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace KYCNintexApi.Data
{
    public static class DbInitializer
    {
        public static void Initialize(KycDbContext context, IWebHostEnvironment env)
        {
            // Ensure schema tables are created in existing PostgreSQL database (e.g. Supabase)
            var databaseCreator = context.Database.GetService<IRelationalDatabaseCreator>();
            if (databaseCreator != null)
            {
                try
                {
                    databaseCreator.CreateTables();
                }
                catch
                {
                    // Ignore if tables already exist
                }
            }

            // Prepare local storage path for sample attachments
            string storagePath = Path.Combine(env.ContentRootPath, "Storage");
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            // Create sample files on disk if they don't exist
            string nationalId1Path = Path.Combine(storagePath, "Ahmed_Ali_NationalID.pdf");
            string passport1Path = Path.Combine(storagePath, "Ahmed_Ali_Passport.pdf");
            string utilityBill1Path = Path.Combine(storagePath, "Ahmed_Ali_UtilityBill.pdf");
            string nationalId2Path = Path.Combine(storagePath, "Sara_Hassan_NationalID.pdf");

            EnsureSampleFile(nationalId1Path, "%PDF-1.4 Header Sample Content for Ahmed Ali National ID Document #29805141201999");
            EnsureSampleFile(passport1Path, "%PDF-1.4 Header Sample Content for Ahmed Ali Passport Document #A98765432");
            EnsureSampleFile(utilityBill1Path, "%PDF-1.4 Header Sample Content for Electricity Utility Bill - Cairo Main Branch");
            EnsureSampleFile(nationalId2Path, "%PDF-1.4 Header Sample Content for Sara Hassan National ID Document #29511201208888");

            // Seed Customers if DB is empty
            if (!context.Customers.Any())
            {
                var customers = new List<Customer>
                {
                    new Customer
                    {
                        FullName = "Ahmed Mohamed Ali",
                        DateOfBirth = new DateTime(1988, 5, 14, 0, 0, 0, DateTimeKind.Utc),
                        PermanentAddress = "12 El Tahrir Square, Cairo, Egypt",
                        MailingAddress = "12 El Tahrir Square, Cairo, Egypt",
                        Phone = "+201001234567",
                        Email = "ahmed.ali@example.com",
                        Branch = "Cairo Main",
                        Status = "Approved",
                        DateOfInitiation = DateTime.UtcNow.AddDays(-10),
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-1)
                    },
                    new Customer
                    {
                        FullName = "Sara Hassan Ibrahim",
                        DateOfBirth = new DateTime(1995, 11, 20, 0, 0, 0, DateTimeKind.Utc),
                        PermanentAddress = "45 Corniche St, Alexandria, Egypt",
                        MailingAddress = "45 Corniche St, Alexandria, Egypt",
                        Phone = "+201229876543",
                        Email = "sara.hassan@example.com",
                        Branch = "Alexandria Branch",
                        Status = "Pending Review",
                        DateOfInitiation = DateTime.UtcNow.AddDays(-3),
                        LastUpdatedDate = DateTime.UtcNow
                    },
                    new Customer
                    {
                        FullName = "Omar Mahmoud El-Sayed",
                        DateOfBirth = new DateTime(1991, 3, 8, 0, 0, 0, DateTimeKind.Utc),
                        PermanentAddress = "78 Pyramids Road, Giza, Egypt",
                        MailingAddress = "78 Pyramids Road, Giza, Egypt",
                        Phone = "+201115556677",
                        Email = "omar.sayed@example.com",
                        Branch = "Giza Branch",
                        Status = "Approved",
                        DateOfInitiation = DateTime.UtcNow.AddDays(-30),
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-5)
                    }
                };

                context.Customers.AddRange(customers);
                context.SaveChanges();

                // Get saved IDs
                int cust1Id = customers[0].CustomerID;
                int cust2Id = customers[1].CustomerID;

                var documents = new List<CustomerDocument>
                {
                    new CustomerDocument
                    {
                        CustomerID = cust1Id,
                        DocumentType = "National ID",
                        FileName = "Ahmed_Ali_NationalID.pdf",
                        FilePath = nationalId1Path,
                        IssueDate = new DateTime(2021, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                        ExpiryDate = new DateTime(2028, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Verified"
                    },
                    new CustomerDocument
                    {
                        CustomerID = cust1Id,
                        DocumentType = "Passport",
                        FileName = "Ahmed_Ali_Passport.pdf",
                        FilePath = passport1Path,
                        IssueDate = new DateTime(2022, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                        ExpiryDate = new DateTime(2032, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Verified"
                    },
                    new CustomerDocument
                    {
                        CustomerID = cust1Id,
                        DocumentType = "Utility Bill",
                        FileName = "Ahmed_Ali_UtilityBill.pdf",
                        FilePath = utilityBill1Path,
                        IssueDate = new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                        ExpiryDate = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Verified"
                    },
                    new CustomerDocument
                    {
                        CustomerID = cust2Id,
                        DocumentType = "National ID",
                        FileName = "Sara_Hassan_NationalID.pdf",
                        FilePath = nationalId2Path,
                        IssueDate = new DateTime(2020, 3, 12, 0, 0, 0, DateTimeKind.Utc),
                        ExpiryDate = new DateTime(2027, 3, 12, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Pending Verification"
                    }
                };

                context.CustomerDocuments.AddRange(documents);
                context.SaveChanges();
            }
        }

        private static void EnsureSampleFile(string filePath, string textContent)
        {
            if (!File.Exists(filePath))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(textContent + "\n" + Guid.NewGuid().ToString("N"));
                File.WriteAllBytes(filePath, bytes);
            }
        }
    }
}
