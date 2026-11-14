# Mini-Projet de Gestion Commerciale - Backend .NET 8

Bienvenue dans le backend du projet de **Gestion Commerciale** développé en **.NET 8** avec une architecture **Clean Architecture**, **Entity Framework Core**, **SQL Server** (avec bascule automatique vers **SQLite**), documentation **Swagger / OpenAPI** et suite de **tests unitaires xUnit**.

---

## 1. Fonctionnalités Implémentées

### 1.1 Gestion des Clients
- `GET /api/clients` : Liste de tous les clients avec nombre de commandes
- `GET /api/clients/{id}` : Consultation détaillée d'un client
- `POST /api/clients` : Création d'un client (vérification de l'unicité de l'email)
- `PUT /api/clients/{id}` : Modification des informations d'un client
- `DELETE /api/clients/{id}` : Suppression d'un client (sécurisée : impossible si commandes rattachées)

### 1.2 Gestion des Produits
- `GET /api/products` : Liste de tous les produits avec stock et prix
- `GET /api/products/{id}` : Consultation détaillée d'un produit
- `POST /api/products` : Création d'un produit (référence unique, prix et stock positifs)
- `PUT /api/products/{id}` : Modification d'un produit existant
- `DELETE /api/products/{id}` : Suppression d'un produit (sécurisée)

### 1.3 Gestion des Commandes & Lignes de commande
- `GET /api/orders` : Liste des commandes avec totaux HT/TTC, client et statut
- `GET /api/orders/{id}` : Consultation complète de la commande avec toutes ses lignes
- `POST /api/orders` : Création d'une commande (statut initial `Brouillon`)
  - Contrôle que le client existe (impossible de créer une commande sans client)
  - Contrôle que la quantité commandée est `> 0`
  - Contrôle que la quantité commandée est `<= stock disponible`
  - Calcul automatique : `TotalLigne = Quantité × Prix unitaire`
  - Calcul automatique : `TotalHT = Somme des lignes`
  - Calcul automatique : `TotalTTC = TotalHT × 1.19` (TVA fixe 19%)
- `PUT /api/orders/{id}` : Modification des lignes d'une commande en statut `Brouillon`
- `DELETE /api/orders/{id}` : Suppression d'une commande (interdite si déjà validée)
- `POST /api/orders/{id}/validate` : **Validation de la commande**
  - Vérifie que la commande est en statut `Brouillon`
  - Vérifie la disponibilité des stocks
  - **Décrémente automatiquement le stock des produits commandés**
  - Passe le statut à `Validee`
- `POST /api/orders/{id}/cancel` : **Annulation de la commande** (restitue le stock si elle était validée)

### 1.4 Modèle de données et relations

- **Client – Order (One-to-Many)** : Un client peut passer plusieurs commandes, tandis que chaque commande est associée à un seul client.
- **Order – OrderLine (One-to-Many)** : Une commande contient une ou plusieurs lignes de commande.
- **Product – OrderLine (One-to-Many)** : Un produit peut apparaître dans plusieurs lignes de commande.
- Chaque ligne de commande référence un produit, avec sa quantité et son prix unitaire. Chaque commande est ainsi composée de lignes de commande permettant de calculer automatiquement ses montants HT et TTC.

### 1.5 Bonus & Qualité
- **Authentification JWT** :
-`POST /api/auth/login` (compte par défaut : `admin`/ `Admin123!`)
-`POST /api/auth/register` 
- **Documentation Swagger** : Interactive avec support JWT Bearer accessible directement à la racine
- **Gestion globale des erreurs** : Middleware `ProblemDetails` (RFC 7807) capturant 400, 404, 409 et 500
- **CORS** : Préconfiguré pour l'application Frontend Angular (`http://localhost:4200`)
- **Tests unitaires xUnit** : Couverture des calculs de totaux, TVA 19% et règles de validation de stock

---

## 2. Structure de la Solution (Clean Architecture)

```
GestionCommerciale/
│
├── GestionCommerciale.sln                     # Solution VS Code/ Visual Studio 2022 
│
├── src/
│   ├── GestionCommerciale.Domain/             # Entités, Enums, Exceptions métier (indépendant)
│   │   ├── Entities/                          # Client, Product, Order, OrderLine, User
│   │   ├── Enums/                             # OrderStatus (Brouillon, Validee, Annulee)
│   │   └── Exceptions/                        # NotFoundException, BusinessRuleException, ValidationException
│   │
│   ├── GestionCommerciale.Application/        # Logique applicative & DTOs
│   │   ├── DTOs/                              # ClientDtos, ProductDtos, OrderDtos, AuthDtos
│   │   ├── Interfaces/                        # IClientService, IProductService, IOrderService, IApplicationDbContext
│   │   └── Services/                          # ClientService, ProductService, OrderService, AuthService
│   │
│   ├── GestionCommerciale.Infrastructure/     # Accès aux données & EF Core
│   │   ├── Data/                              # ApplicationDbContext, DbInitializer (seed automatique)
│   │   └── Data/Configurations/               # Fluent API pour SQL Server / SQLite
│   │
│   └── GestionCommerciale.Api/                # API ASP.NET Core 8 Web API
│       ├── Controllers/                       # ClientsController, ProductsController, OrdersController, AuthController
│       ├── Middlewares/                       # ExceptionHandlingMiddleware
│       ├── appsettings.json                   # Configuration DB (SQL Server / SQLite), JWT, CORS
│       └── Program.cs                         # Pipeline HTTP, Swagger, CORS Angular, Initialisation DB
│
├── tests/
│   └── GestionCommerciale.UnitTests/          # Tests unitaires xUnit
│       ├── OrderCalculationTests.cs           # Vérification mathématique des calculs HT, TTC (19%)
│       └── OrderStockValidationTests.cs       # Vérification du rejet si stock insuffisant & décrémentation
│
└── sql/
    └── schema.sql                             # Script SQL de création complète de la base de données
```

---

## 3. Prérequis

- Kit de développement **.NET 8 SDK** (ou version supérieure comme .NET 11)
- **Visual Studio 2022** (avec la charge de travail *Développement Web et ASP.NET*) ou **Visual Studio Code** (avec l'extension C# Dev Kit)
- **SQL Server** (LocalDB ou SQL Server Express) **OU** vous pouvez utiliser **SQLite** sans rien installer !

---

## 4. Démarrage Rapide

### Option A : Depuis Visual Studio 2022
1. Dézippez le dossier du projet.
2. Double-cliquez sur le fichier **`GestionCommerciale.sln`**.
3. Définissez **`GestionCommerciale.Api`** comme projet de démarrage (si ce n'est pas déjà le cas).
4. Appuyez sur **F5** (ou Ctrl+F5).
5. Votre navigateur s'ouvrira automatiquement sur l'interface **Swagger** (`https://localhost:7xxx/` ou `http://localhost:5000/`).

### Option B : Depuis le Terminal (CLI .NET)
```bash
cd src/GestionCommerciale.Api  # pour se placer dans le dossier de l'API
dotnet run # pour lancer l'application
```
Accédez ensuite à l'interface Swagger sur votre navigateur :
👉 **`http://localhost:5000`** ou **`https://localhost:5001`**

---

## 5. Configuration de la Base de Données

Le projet est configuré par défaut pour utiliser **SQL Server LocalDB** :
```json
"DatabaseProvider": "SqlServer",
"ConnectionStrings": {
  "SqlServerConnection": "Server=localhost\\SQLEXPRESS;Database=GestionCommercialeDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

> **Astuce :** Si vous n'avez pas SQL Server installé sur votre machine, changez simplement `"DatabaseProvider": "Sqlite"` dans le fichier `src/GestionCommerciale.Api/appsettings.json`. L'application fonctionnera immédiatement avec une base locale SQLite sans aucune configuration préalable !

---

## 6. Lancement des Tests Unitaires

Pour exécuter la suite de tests automatisés :
```bash
dotnet test
```

---

## 7. Scénario de Test de Recrutement Validé

L'API est prête avec des données d'exemple injectées automatiquement au premier démarrage :
1. **Consulter les clients** : `GET /api/clients` (Jean Pierre Dupont, TechPlus, etc)
2. **Consulter les produits et stocks** : `GET /api/products` (PC Portable, Écran, Souris, Clavier etc)
3. **Créer une commande** : `POST /api/orders`
4. **Ajouter des lignes** : Calcul automatique des totaux HT et TTC (TVA 19%).
5. **Valider la commande** : `POST /api/orders/{id}/validate` -> Le stock des produits est automatiquement décrémenté !
6. **Vérifier** : `GET /api/products` -> Le stock disponible a bien diminué.
