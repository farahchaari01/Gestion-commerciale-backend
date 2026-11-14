using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Infrastructure.Data.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");

        builder.HasKey(ol => ol.Id);

        builder.Property(ol => ol.OrderId)
            .IsRequired();

        builder.Property(ol => ol.ProductId)
            .IsRequired();

        builder.Property(ol => ol.Quantite)
            .IsRequired();

        builder.Property(ol => ol.PrixUnitaire)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ol => ol.TotalLigne)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}
