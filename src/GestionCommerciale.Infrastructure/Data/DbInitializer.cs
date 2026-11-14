
using Microsoft.EntityFrameworkCore;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace GestionCommerciale.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Crée la base si elle n'existe pas
        await context.Database.EnsureCreatedAsync();

        // 1. Initialisation des clients
        if (!await context.Clients.AnyAsync())
        {
            var clientsToAdd = new List<Client>
            {
                new Client
                {
                    Nom = "Dupont",
                    PrenomOuRaisonSociale = "Jean",
                    Email = "jean.dupont@email.com",
                    Telephone = "+33 6 12 34 56 78",
                    Adresse = "12 Rue de la République, 75001 Paris",
                    DateCreation = DateTime.UtcNow
                },
                new Client
                {
                    Nom = "Tech Solutions",
                    PrenomOuRaisonSociale = "SARL TechPlus",
                    Email = "contact@techplus.fr",
                    Telephone = "+33 1 40 50 60 70",
                    Adresse = "45 Avenue des Champs, 69002 Lyon",
                    DateCreation = DateTime.UtcNow
                },
                new Client
                {
                    Nom = "Ben Ali",
                    PrenomOuRaisonSociale = "Sonia",
                    Email = "sonia.benali@email.com",
                    Telephone = "+216 98 76 54 32",
                    Adresse = "8 Rue de Carthage, Tunis",
                    DateCreation = DateTime.UtcNow
                }
            };

            await context.Clients.AddRangeAsync(clientsToAdd);
            await context.SaveChangesAsync();
        }

        // Récupération des clients existants
        var clients = await context.Clients
            .OrderBy(c => c.Id)
            .ToListAsync();

        // 2. Initialisation des produits
        if (!await context.Products.AnyAsync())
        {
            var productsToAdd = new List<Product>
            {
                new Product
                {
                    Reference = "PRD-LAPTOP",
                    Nom = "Ordinateur Portable Dell XPS 15",
                    Description = "Intel i7, 16GB RAM, 512GB SSD",
                    PrixUnitaireHT = 1200.00m,
                    QuantiteEnStock = 15,
                    DateCreation = DateTime.UtcNow
                },
                new Product
                {
                    Reference = "PRD-MOUSE",
                    Nom = "Souris Ergonomique Sans Fil",
                    Description = "Souris optique Bluetooth rechargeable",
                    PrixUnitaireHT = 35.50m,
                    QuantiteEnStock = 50,
                    DateCreation = DateTime.UtcNow
                },
                new Product
                {
                    Reference = "PRD-SCREEN",
                    Nom = "Écran 27 pouces 4K UHD",
                    Description = "Moniteur IPS 144Hz HDR400",
                    PrixUnitaireHT = 350.00m,
                    QuantiteEnStock = 20,
                    DateCreation = DateTime.UtcNow
                },
                new Product
                {
                    Reference = "PRD-KEYBOARD",
                    Nom = "Clavier Mécanique Rétroéclairé",
                    Description = "Switches Red silencieux, layout AZERTY",
                    PrixUnitaireHT = 85.00m,
                    QuantiteEnStock = 30,
                    DateCreation = DateTime.UtcNow
                }
            };

            await context.Products.AddRangeAsync(productsToAdd);
            await context.SaveChangesAsync();
        }

        // Récupération des produits existants
        var products = await context.Products
            .OrderBy(p => p.Id)
            .ToListAsync();

        // 3. Initialisation d'une commande exemple
        if (!await context.Orders.AnyAsync()
            && clients.Count > 0
            && products.Count >= 2)
        {
            var client = clients[0];
            var p1 = products[0];
            var p2 = products[1];

            var totalHT = 1271.00m;
            var totalTTC = Math.Round(totalHT * 1.19m, 2);

            var sampleOrder = new Order
            {
                NumeroCommande = "CMD-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-0001",
                ClientId = client.Id,
                DateCommande = DateTime.UtcNow,
                Statut = OrderStatus.Brouillon,
                TotalHT = totalHT,
                TotalTTC = totalTTC,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        ProductId = p1.Id,
                        Quantite = 1,
                        PrixUnitaire = p1.PrixUnitaireHT,
                        TotalLigne = 1200.00m
                    },
                    new OrderLine
                    {
                        ProductId = p2.Id,
                        Quantite = 2,
                        PrixUnitaire = p2.PrixUnitaireHT,
                        TotalLigne = 71.00m
                    }
                }
            };

            await context.Orders.AddAsync(sampleOrder);
            await context.SaveChangesAsync();
        }

        // 4. Initialisation du compte administrateur
        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@gestioncommerciale.com",
                PasswordHash = HashPassword("Admin123!"),
                Role = "Admin",
                DateCreation = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}





//--------------------------------------- l'ancien code qui a géneré 2 erreurs---------------------------------------///

// using Microsoft.EntityFrameworkCore;
// using GestionCommerciale.Domain.Entities;
// using GestionCommerciale.Domain.Enums;
// using System.Security.Cryptography;
// using System.Text;

// namespace GestionCommerciale.Infrastructure.Data;

// public static class DbInitializer
// {
//     public static async Task InitializeAsync(ApplicationDbContext context)
//     {
//         // Crée la base si elle n'existe pas (idéal pour SQLite /dev rapide)
//         await context.Database.EnsureCreatedAsync();

//         // old code :
//         // if (await context.Clients.AnyAsync())
//         // {
//         //     return; // Déjà initialisé
//         // }

//         //new code:
//         if (!await context.Clients.AnyAsync())
//         {
//         await context.Clients.AddRangeAsync(clients);
//         await context.SaveChangesAsync();
//         }
//         if (!await context.Products.AnyAsync())
//         {
//         await context.Products.AddRangeAsync(products);
//         await context.SaveChangesAsync();
//         }        


//         // 1. Initialisation des Clients
//         var clients = new List<Client>
//         {
//             new Client
//             {
//                 Nom = "Dupont",
//                 PrenomOuRaisonSociale = "Jean",
//                 Email = "jean.dupont@email.com",
//                 Telephone = "+33 6 12 34 56 78",
//                 Adresse = "12 Rue de la République, 75001 Paris",
//                 DateCreation = DateTime.UtcNow
//             },
//             new Client
//             {
//                 Nom = "Tech Solutions",
//                 PrenomOuRaisonSociale = "SARL TechPlus",
//                 Email = "contact@techplus.fr",
//                 Telephone = "+33 1 40 50 60 70",
//                 Adresse = "45 Avenue des Champs, 69002 Lyon",
//                 DateCreation = DateTime.UtcNow
//             },
//             new Client
//             {
//                 Nom = "Ben Ali",
//                 PrenomOuRaisonSociale = "Sonia",
//                 Email = "sonia.benali@email.com",
//                 Telephone = "+216 98 76 54 32",
//                 Adresse = "8 Rue de Carthage, Tunis",
//                 DateCreation = DateTime.UtcNow
//             }
//         };

//         await context.Clients.AddRangeAsync(clients);
//         await context.SaveChangesAsync();

//         // 2. Initialisation des Produits
//         var products = new List<Product>
//         {
//             new Product
//             {
//                 Reference = "PRD-LAPTOP",
//                 Nom = "Ordinateur Portable Dell XPS 15",
//                 Description = "Intel i7, 16GB RAM, 512GB SSD",
//                 PrixUnitaireHT = 1200.00m,
//                 QuantiteEnStock = 15,
//                 DateCreation = DateTime.UtcNow
//             },
//             new Product
//             {
//                 Reference = "PRD-MOUSE",
//                 Nom = "Souris Ergonomique Sans Fil",
//                 Description = "Souris optique Bluetooth rechargeable",
//                 PrixUnitaireHT = 35.50m,
//                 QuantiteEnStock = 50,
//                 DateCreation = DateTime.UtcNow
//             },
//             new Product
//             {
//                 Reference = "PRD-SCREEN",
//                 Nom = "Écran 27 pouces 4K UHD",
//                 Description = "Moniteur IPS 144Hz HDR400",
//                 PrixUnitaireHT = 350.00m,
//                 QuantiteEnStock = 20,
//                 DateCreation = DateTime.UtcNow
//             },
//             new Product
//             {
//                 Reference = "PRD-KEYBOARD",
//                 Nom = "Clavier Mécanique Rétroéclairé",
//                 Description = "Switches Red silencieux, layout AZERTY",
//                 PrixUnitaireHT = 85.00m,
//                 QuantiteEnStock = 30,
//                 DateCreation = DateTime.UtcNow
//             }
//         };

//         await context.Products.AddRangeAsync(products);
//         await context.SaveChangesAsync();

//         // 3. Initialisation d'une commande exemple (Brouillon)
//         var client = clients[0];
//         var p1 = products[0]; // 1200.00 HT * 1 = 1200.00
//         var p2 = products[1]; // 35.50 HT * 2 = 71.00
//         var totalHT = 1271.00m;
//         var totalTTC = Math.Round(totalHT * 1.19m, 2);

//         var sampleOrder = new Order
//         {
//             NumeroCommande = "CMD-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-0001",
//             ClientId = client.Id,
//             DateCommande = DateTime.UtcNow,
//             Statut = OrderStatus.Brouillon,
//             TotalHT = totalHT,
//             TotalTTC = totalTTC,
//             OrderLines = new List<OrderLine>
//             {
//                 new OrderLine
//                 {
//                     ProductId = p1.Id,
//                     Quantite = 1,
//                     PrixUnitaire = p1.PrixUnitaireHT,
//                     TotalLigne = 1200.00m
//                 },
//                 new OrderLine
//                 {
//                     ProductId = p2.Id,
//                     Quantite = 2,
//                     PrixUnitaire = p2.PrixUnitaireHT,
//                     TotalLigne = 71.00m
//                 }
//             }
//         };

//         await context.Orders.AddAsync(sampleOrder);

//         // 4. Initialisation d'un utilisateur Admin pour le test JWT (mot de passe: Admin123!)
//         var adminUser = new User
//         {
//             Username = "admin",
//             Email = "admin@gestioncommerciale.com",
//             PasswordHash = HashPassword("Admin123!"),
//             Role = "Admin",
//             DateCreation = DateTime.UtcNow
//         };

//         await context.Users.AddAsync(adminUser);
//         await context.SaveChangesAsync();
//     }

//     private static string HashPassword(string password)
//     {
//         using var sha256 = SHA256.Create();
//         var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
//         return Convert.ToBase64String(bytes);
//     }
// }
