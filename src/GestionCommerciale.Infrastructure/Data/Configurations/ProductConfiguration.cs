using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Reference)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Reference)
            .IsUnique();

        builder.Property(p => p.Nom)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.PrixUnitaireHT)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.QuantiteEnStock)
            .IsRequired();

        builder.Property(p => p.DateCreation)
            .IsRequired();

        // 1 Produit est dans N Lignes de commande
        builder.HasMany(p => p.OrderLines)
            .WithOne(ol => ol.Product)
            .HasForeignKey(ol => ol.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
