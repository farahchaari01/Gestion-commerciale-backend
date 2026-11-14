namespace GestionCommerciale.Domain.Entities;

public class Client
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string PrenomOuRaisonSociale { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
