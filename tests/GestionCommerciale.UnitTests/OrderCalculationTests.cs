using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using Xunit;

namespace GestionCommerciale.UnitTests;

public class OrderCalculationTests
{
    private const decimal TvaRate = 0.19m; // 19% TVA

    [Fact]
    public void OrderLine_TotalLigne_ShouldBe_QuantityMultipliedByUnitPrice()
    {
        // Arrange
        int quantite = 3;
        decimal prixUnitaire = 120.50m;

        // Act
        decimal totalLigne = Math.Round(quantite * prixUnitaire, 2);

        // Assert
        Assert.Equal(361.50m, totalLigne);
    }

    [Fact]
    public void Order_TotalHT_ShouldBe_SumOfAllOrderLines()
    {
        // Arrange
        var lines = new List<OrderLine>
        {
            new OrderLine { Quantite = 2, PrixUnitaire = 50.00m, TotalLigne = 100.00m },
            new OrderLine { Quantite = 1, PrixUnitaire = 250.00m, TotalLigne = 250.00m },
            new OrderLine { Quantite = 4, PrixUnitaire = 15.25m, TotalLigne = 61.00m }
        };

        // Act
        decimal totalHT = lines.Sum(l => l.TotalLigne);

        // Assert
        Assert.Equal(411.00m, totalHT);
    }

    [Fact]
    public void Order_TotalTTC_ShouldBe_TotalHTWithFixed19PercentVat()
    {
        // Arrange
        decimal totalHT = 1000.00m;

        // Act
        decimal totalTTC = Math.Round(totalHT * (1 + TvaRate), 2, MidpointRounding.AwayFromZero);

        // Assert
        Assert.Equal(1190.00m, totalTTC);
    }

    [Theory]
    [InlineData(100.00, 119.00)]
    [InlineData(55.50, 66.05)]
    [InlineData(1271.00, 1512.49)]
    public void Order_TotalTTC_WithVariousAmounts_CalculatesCorrectly(decimal ht, decimal expectedTtc)
    {
        // Act
        decimal totalTTC = Math.Round(ht * (1 + TvaRate), 2, MidpointRounding.AwayFromZero);

        // Assert
        Assert.Equal(expectedTtc, totalTTC);
    }
}
