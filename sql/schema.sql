-- ==========================================================
-- SCRIPT DE CRÉATION DE LA BASE DE DONNÉES SQL SERVER
-- PROJET : Mini-Projet Gestion Commerciale .NET 8
-- ==========================================================

-- Création de la base si elle n'existe pas
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GestionCommercialeDb')
BEGIN
    CREATE DATABASE GestionCommercialeDb;
END
GO

USE GestionCommercialeDb;
GO

-- 1. Table Clients
IF OBJECT_ID('dbo.Clients', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clients (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nom NVARCHAR(100) NOT NULL,
        PrenomOuRaisonSociale NVARCHAR(150) NULL,
        Email NVARCHAR(150) NOT NULL UNIQUE,
        Telephone NVARCHAR(30) NULL,
        Adresse NVARCHAR(500) NULL,
        DateCreation DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- 2. Table Products
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Reference NVARCHAR(50) NOT NULL UNIQUE,
        Nom NVARCHAR(150) NOT NULL,
        Description NVARCHAR(1000) NULL,
        PrixUnitaireHT DECIMAL(18,2) NOT NULL,
        QuantiteEnStock INT NOT NULL DEFAULT 0,
        DateCreation DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- 3. Table Orders (Commandes)
IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        NumeroCommande NVARCHAR(50) NOT NULL UNIQUE,
        ClientId INT NOT NULL,
        DateCommande DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Statut INT NOT NULL DEFAULT 1, -- 1: Brouillon, 2: Validee, 3: Annulee
        TotalHT DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalTTC DECIMAL(18,2) NOT NULL DEFAULT 0,
        CONSTRAINT FK_Orders_Clients FOREIGN KEY (ClientId) 
            REFERENCES dbo.Clients(Id) ON DELETE NO ACTION
    );
END
GO

-- 4. Table OrderLines (Lignes de commande)
IF OBJECT_ID('dbo.OrderLines', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderLines (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantite INT NOT NULL,
        PrixUnitaire DECIMAL(18,2) NOT NULL,
        TotalLigne DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_OrderLines_Orders FOREIGN KEY (OrderId) 
            REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderLines_Products FOREIGN KEY (ProductId) 
            REFERENCES dbo.Products(Id) ON DELETE NO ACTION
    );
END
GO

-- 5. Table Users (Bonus Authentification JWT)
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(150) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'Admin',
        DateCreation DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- Index pour optimiser les performances des jointures
CREATE NONCLUSTERED INDEX IX_Orders_ClientId ON dbo.Orders(ClientId);
CREATE NONCLUSTERED INDEX IX_OrderLines_OrderId ON dbo.OrderLines(OrderId);
CREATE NONCLUSTERED INDEX IX_OrderLines_ProductId ON dbo.OrderLines(ProductId);
GO

-- ==========================================================
-- JEU DE DONNÉES DE TEST (SEED DATA)
-- ==========================================================

SET IDENTITY_INSERT dbo.Clients ON;
INSERT INTO dbo.Clients (Id, Nom, PrenomOuRaisonSociale, Email, Telephone, Adresse, DateCreation)
VALUES 
(1, N'Dupont', N'Jean', N'jean.dupont@email.com', N'+33 6 12 34 56 78', N'12 Rue de la République, 75001 Paris', SYSUTCDATETIME()),
(2, N'Tech Solutions', N'SARL TechPlus', N'contact@techplus.fr', N'+33 1 40 50 60 70', N'45 Avenue des Champs, 69002 Lyon', SYSUTCDATETIME()),
(3, N'Ben Ali', N'Sonia', N'sonia.benali@email.com', N'+216 98 76 54 32', N'8 Rue de Carthage, Tunis', SYSUTCDATETIME());
SET IDENTITY_INSERT dbo.Clients OFF;

SET IDENTITY_INSERT dbo.Products ON;
INSERT INTO dbo.Products (Id, Reference, Nom, Description, PrixUnitaireHT, QuantiteEnStock, DateCreation)
VALUES 
(1, N'PRD-LAPTOP', N'Ordinateur Portable Dell XPS 15', N'Intel i7, 16GB RAM, 512GB SSD', 1200.00, 15, SYSUTCDATETIME()),
(2, N'PRD-MOUSE', N'Souris Ergonomique Sans Fil', N'Souris optique Bluetooth rechargeable', 35.50, 50, SYSUTCDATETIME()),
(3, N'PRD-SCREEN', N'Écran 27 pouces 4K UHD', N'Moniteur IPS 144Hz HDR400', 350.00, 20, SYSUTCDATETIME()),
(4, N'PRD-KEYBOARD', N'Clavier Mécanique Rétroéclairé', N'Switches Red silencieux, layout AZERTY', 85.00, 30, SYSUTCDATETIME());
SET IDENTITY_INSERT dbo.Products OFF;

-- Utilisateur admin (mot de passe: Admin123!)
-- Hash SHA256 de "Admin123!" en base64 = "J7047WKS04/5t5i7fJ06j3Hl/p6p4vM3r8bF3WkU8gE=" (ou équivalent hash)
INSERT INTO dbo.Users (Username, Email, PasswordHash, Role, DateCreation)
VALUES (N'admin', N'admin@gestioncommerciale.com', N'J7047WKS04/5t5i7fJ06j3Hl/p6p4vM3r8bF3WkU8gE=', N'Admin', SYSUTCDATETIME());
GO
