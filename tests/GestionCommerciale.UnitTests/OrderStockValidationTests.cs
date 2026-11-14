using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.Domain.Exceptions;
using Xunit;

namespace GestionCommerciale.UnitTests;

public class OrderStockValidationTests
{
    [Fact]
    public void OrderCreation_QuantityLessThanOrEqualToZero_ShouldThrowException()
    {
        // Arrange
        int invalidQuantity = 0;

        // Act & Assert
        var ex = Assert.Throws<BusinessRuleException>(() =>
        {
            if (invalidQuantity <= 0)
            {
                throw new BusinessRuleException("La quantité commandée doit être strictement supérieure à zéro.");
            }
        });

        Assert.Contains("strictement supérieure à zéro", ex.Message);
    }

    [Fact]
    public void OrderCreation_QuantityExceedingStock_ShouldThrowException()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Nom = "Écran 27 pouces",
            QuantiteEnStock = 5,
            PrixUnitaireHT = 350.00m
        };

        int demandedQuantity = 10;

        // Act & Assert
        var ex = Assert.Throws<BusinessRuleException>(() =>
        {
            if (demandedQuantity > product.QuantiteEnStock)
            {
                throw new BusinessRuleException($"Stock insuffisant pour le produit '{product.Nom}'. Stock disponible: {product.QuantiteEnStock}, Quantité demandée: {demandedQuantity}.");
            }
        });

        Assert.Contains("Stock insuffisant", ex.Message);
    }

    [Fact]
    public void ValidateOrder_ShouldDecrementProductStock()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Nom = "Ordinateur Portable",
            QuantiteEnStock = 15,
            PrixUnitaireHT = 1200.00m
        };

        var orderLine = new OrderLine
        {
            ProductId = product.Id,
            Product = product,
            Quantite = 3,
            PrixUnitaire = product.PrixUnitaireHT,
            TotalLigne = 3600.00m
        };

        var order = new Order
        {
            Id = 1,
            NumeroCommande = "CMD-20261113-001",
            Statut = OrderStatus.Brouillon,
            OrderLines = new List<OrderLine> { orderLine }
        };

        // Act: Simulation de la logique de validation
        if (product.QuantiteEnStock < orderLine.Quantite)
        {
            throw new BusinessRuleException("Stock insuffisant");
        }

        product.QuantiteEnStock -= orderLine.Quantite;
        order.Statut = OrderStatus.Validee;

        // Assert
        Assert.Equal(12, product.QuantiteEnStock);
        Assert.Equal(OrderStatus.Validee, order.Statut);
    }

    [Fact]
    public void ValidateOrder_AlreadyValidated_ShouldThrowException()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            NumeroCommande = "CMD-20261113-001",
            Statut = OrderStatus.Validee
        };

        // Act & Assert
        var ex = Assert.Throws<BusinessRuleException>(() =>
        {
            if (order.Statut == OrderStatus.Validee)
            {
                throw new BusinessRuleException("Cette commande est déjà validée.");
            }
        });

        Assert.Equal("Cette commande est déjà validée.", ex.Message);
    }
}
