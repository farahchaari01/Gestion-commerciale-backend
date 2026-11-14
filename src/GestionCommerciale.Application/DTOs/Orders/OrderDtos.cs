namespace GestionCommerciale.Application.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public string NumeroCommande { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientNom { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; }
    public string Statut { get; set; } = string.Empty;
    public decimal TotalHT { get; set; }
    public decimal TotalTTC { get; set; }
    public List<OrderLineDto> OrderLines { get; set; } = new();
}

public class OrderLineDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ReferenceProduit { get; set; } = string.Empty;
    public string NomProduit { get; set; } = string.Empty;
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal TotalLigne { get; set; }
}

public class CreateOrderDto
{
    public int ClientId { get; set; }
    public List<CreateOrderLineDto> OrderLines { get; set; } = new();
}

public class UpdateOrderDto
{
    public List<CreateOrderLineDto> OrderLines { get; set; } = new();
}

public class CreateOrderLineDto
{
    public int ProductId { get; set; }
    public int Quantite { get; set; }
}
