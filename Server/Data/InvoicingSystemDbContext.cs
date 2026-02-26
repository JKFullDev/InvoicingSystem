using Microsoft.EntityFrameworkCore;
using InvoicingSystem.Server.Data.Models;

namespace InvoicingSystem.Server.Data
{
    public partial class InvoicingSystemDbContext : DbContext
    {
        // 1. CONSTRUCTOR
        // Este bloque es para que .NET le pase la cadena de conexion desde Progrma.cs
        public InvoicingSystemDbContext(DbContextOptions<InvoicingSystemDbContext> options) : base(options)
        {
        }

        // 2. DBSETS
        // El nombre de la propiedad (Customers) tiene que coincidir con el nombre de la tabla SQL
        public DbSet<Customers> Customers { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<TaxRates> TaxRates { get; set; }
        public DbSet<PaymentTerms> PaymentTerms { get; set; }
        public DbSet<SalesInvoiceHeaders> SalesInvoiceHeaders { get; set; }
        public DbSet<SalesInvoiceLines> SalesInvoiceLines { get; set; }


        // 3. CONFIGURACIÓN
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de Customer
            modelBuilder.Entity<Customers>(entity =>
            {
                // HasKey Define la clave primaria
                entity.HasKey(e => e.CustomerId);
                //Como el id lo pongo manualmente, hago que no lo genere automaticamente la DB
                entity.Property(e => e.CustomerId).ValueGeneratedNever();
            });

            // Configuración de PaymentTerms 
            modelBuilder.Entity<PaymentTerms>(entity => {
                entity.HasKey(e => e.PaymentTermsId);
                entity.Property(e => e.PaymentTermsId).ValueGeneratedNever();
            });

            // Configuración de Products 
            modelBuilder.Entity<Products>(entity => {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).ValueGeneratedNever();
            });

            // Configuración de TaxRates 
            modelBuilder.Entity<TaxRates>(entity => {
                entity.HasKey(e => e.TaxRateId);
                entity.Property(e => e.TaxRateId).ValueGeneratedNever();
            });

            // Configuración de SalesInvoiceHeader
            modelBuilder.Entity<SalesInvoiceHeaders>(entity =>
            {
                entity.HasKey(e => e.SalesInvoiceHeaderId);
                entity.Property(e => e.SalesInvoiceHeaderId).ValueGeneratedNever();

                //Relación con Customers
                entity.HasOne<Customers>(d => d.Customer)
                    .WithMany()
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)  //Si borro un cliente no se borrarán las facturas a su nombre
                    .HasConstraintName("FK_Invoices_Customers");

                // Relación con PaymentTerms
                entity.HasOne<PaymentTerms>(d => d.PaymentTerms)
                    .WithMany()
                    .HasForeignKey(d => d.PaymentTermsId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoices_PaymentTerms");
            });

            // Configuración de SalesInvoiceLine (relación de 1 a muchos)
            modelBuilder.Entity<SalesInvoiceLines>(entity =>
            {
                entity.HasKey(e => e.SalesInvoiceLineId);
                entity.Property(e => e.SalesInvoiceLineId).ValueGeneratedNever();

                // Relacion con SalesInvoiceHeader
                entity.HasOne<SalesInvoiceHeaders>(d => d.SalesInvoiceHeader)
                    .WithMany(p => p.Lines)
                    .HasForeignKey(d => d.SalesInvoiceHeaderId)
                    .HasConstraintName("FK_Lines_Headers");

                // Relación con Product
                entity.HasOne<Products>(d => d.Product)
                    .WithMany()
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Lines_Products");

                // Relación con TaxRates
                entity.HasOne<TaxRates>(d => d.TaxRate)
                    .WithMany()
                    .HasForeignKey(d => d.TaxRateId)
                    .HasConstraintName("FK_Lines_TaxRates");
            });

            


            // llamada al método parcial por si quiero extenderlo en otro archivo
            OnModelCreatingPartial(modelBuilder);

        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
