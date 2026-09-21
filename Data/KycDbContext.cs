using Microsoft.EntityFrameworkCore;
using KYCNintexApi.Models;

namespace KYCNintexApi.Data
{
    public class KycDbContext : DbContext
    {
        public KycDbContext(DbContextOptions<KycDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(e => e.CustomerID);
            });

            modelBuilder.Entity<CustomerDocument>(entity =>
            {
                entity.ToTable("CustomerDocuments");
                entity.HasKey(e => e.DocumentID);

                entity.HasOne(d => d.Customer)
                      .WithMany(c => c.Documents)
                      .HasForeignKey(d => d.CustomerID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
