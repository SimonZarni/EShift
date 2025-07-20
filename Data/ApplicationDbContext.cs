using EShift123.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EShift123.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Assistant> Assistants { get; set; }
        public DbSet<Container> Containers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Lorry> Lorries { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Load> Loads { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<LoadProduct> LoadProducts { get; set; }
        public DbSet<TransportUnit> TransportUnits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configure Relationships with Explicit Delete Behaviors ---
            // This is the most common way to resolve the "multiple cascade paths" error.

            // Customer (One) to Job (Many)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Jobs)
                .WithOne(j => j.Customer)
                .HasForeignKey(j => j.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a Customer cascades to their Jobs

            // Customer (One) to Product (Many) - THIS IS THE KEY FIX FOR YOUR ERROR
            // If you delete a Customer, their Products are NOT automatically deleted by this path.
            // You must delete products manually first, or set up application logic.
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Customer)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.NoAction); // IMPORTANT: Prevents customer deletion if products exist, no cascade.
                                                    // Use NoAction to explicitly resolve the cycle conflict.


            // Job (One) to Load (Many)
            modelBuilder.Entity<Job>()
                .HasMany(j => j.Loads)
                .WithOne(l => l.Job)
                .HasForeignKey(l => l.JobId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a Job cascades to its Loads

            // Load (One) to LoadProduct (Many)
            modelBuilder.Entity<Load>()
                .HasMany(l => l.LoadProducts)
                .WithOne(lp => lp.Load)
                .HasForeignKey(lp => lp.LoadId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a Load cascades to its LoadProducts

            // Product (One) to LoadProduct (Many)
            // Deleting a Product should NOT delete LoadProducts. Instead, restrict deletion of Product.
            modelBuilder.Entity<Product>()
                .HasMany(p => p.LoadProducts)
                .WithOne(lp => lp.Product)
                .HasForeignKey(lp => lp.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Deleting a Product is restricted if it's in a LoadProduct


            // TransportUnit (One) to Load (Many)
            // Deleting a TransportUnit should NOT delete Loads it was responsible for.
            modelBuilder.Entity<TransportUnit>()
                .HasMany(tu => tu.Loads)
                .WithOne(l => l.TransportUnit)
                .HasForeignKey(l => l.TransportUnitId)
                .OnDelete(DeleteBehavior.Restrict); // Deleting TransportUnit is restricted if it has associated Loads


            // Lorry (One) to TransportUnit (Many)
            modelBuilder.Entity<Lorry>()
                .HasMany(l => l.TransportUnits)
                .WithOne(tu => tu.Lorry)
                .HasForeignKey(tu => tu.LorryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Driver (One) to TransportUnit (Many)
            modelBuilder.Entity<Driver>()
                .HasMany(d => d.TransportUnits)
                .WithOne(tu => tu.Driver)
                .HasForeignKey(tu => tu.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Assistant (One) to TransportUnit (Many) - Nullable FK
            modelBuilder.Entity<Assistant>()
                .HasMany(a => a.TransportUnits)
                .WithOne(tu => tu.Assistant)
                .HasForeignKey(tu => tu.AssistantId)
                .IsRequired(false) // Matches nullable AssistantId
                .OnDelete(DeleteBehavior.SetNull); // If Assistant is deleted, set FK to NULL in TransportUnit


            // This is crucial for Identity tables to be created correctly
            base.OnModelCreating(modelBuilder);
        }
    }
}
