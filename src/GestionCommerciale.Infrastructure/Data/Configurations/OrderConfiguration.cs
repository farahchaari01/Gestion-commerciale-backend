using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.NumeroCommande)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.NumeroCommande)
            .IsUnique();

        builder.Property(o => o.DateCommande)
            .IsRequired();

        builder.Property(o => o.Statut)
            .IsRequired();

        builder.Property(o => o.TotalHT)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.TotalTTC)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // 1 Commande a N Lignes (cascade delete)
        builder.HasMany(o => o.OrderLines)
            .WithOne(ol => ol.Order)
            .HasForeignKey(ol => ol.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
