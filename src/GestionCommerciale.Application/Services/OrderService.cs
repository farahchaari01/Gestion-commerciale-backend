using Microsoft.EntityFrameworkCore;
using GestionCommerciale.Application.DTOs.Orders;
using GestionCommerciale.Application.Interfaces;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.Domain.Exceptions;

namespace GestionCommerciale.Application.Services;

public class OrderService : IOrderService
{
    private readonly IApplicationDbContext _context;
    private const decimal TvaRate = 0.19m; // 19% TVA fixe

    public OrderService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Client)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .OrderByDescending(o => o.DateCommande)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Client)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            throw new NotFoundException("Commande", id);

        return MapToDto(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        // Règle 1: Il ne doit pas être possible de créer une commande sans client
        if (dto.ClientId <= 0)
            throw new ValidationException(nameof(dto.ClientId), "Un client valide doit être associé à la commande.");

        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == dto.ClientId, cancellationToken);

        if (client == null)
            throw new NotFoundException("Client", dto.ClientId);

        // Validation des lignes
        if (dto.OrderLines == null || !dto.OrderLines.Any())
            throw new ValidationException("OrderLines", "La commande doit comporter au moins une ligne de produit.");

        var orderLines = new List<OrderLine>();
        decimal totalHT = 0;

        foreach (var lineDto in dto.OrderLines)
        {
            // Règle 2: Quantité > 0
            if (lineDto.Quantite <= 0)
                throw new BusinessRuleException("La quantité commandée doit être strictement supérieure à zéro.");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == lineDto.ProductId, cancellationToken);

            if (product == null)
                throw new NotFoundException("Produit", lineDto.ProductId);

            // Règle 3: Quantité <= stock disponible
            if (lineDto.Quantite > product.QuantiteEnStock)
            {
                throw new BusinessRuleException(
                    $"Stock insuffisant pour le produit '{product.Nom}' (Réf: {product.Reference}). " +
                    $"Stock disponible: {product.QuantiteEnStock}, Quantité demandée: {lineDto.Quantite}.");
            }

            // Calcul du total de la ligne: Quantité * Prix unitaire
            decimal totalLigne = Math.Round(lineDto.Quantite * product.PrixUnitaireHT, 2, MidpointRounding.AwayFromZero);
            totalHT += totalLigne;

            orderLines.Add(new OrderLine
            {
                ProductId = product.Id,
                Quantite = lineDto.Quantite,
                PrixUnitaire = product.PrixUnitaireHT,
                TotalLigne = totalLigne
            });
        }

        // Calcul du total HT et total TTC avec TVA de 19%
        totalHT = Math.Round(totalHT, 2, MidpointRounding.AwayFromZero);
        decimal totalTTC = Math.Round(totalHT * (1 + TvaRate), 2, MidpointRounding.AwayFromZero);

        var order = new Order
        {
            NumeroCommande = GenerateOrderNumber(),
            ClientId = client.Id,
            DateCommande = DateTime.UtcNow,
            Statut = OrderStatus.Brouillon,
            TotalHT = totalHT,
            TotalTTC = totalTTC,
            OrderLines = orderLines
        };

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    public async Task<OrderDto> UpdateAsync(int id, UpdateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderLines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            throw new NotFoundException("Commande", id);

        // Une commande validée ou annulée ne peut plus être modifiée
        if (order.Statut != OrderStatus.Brouillon)
            throw new BusinessRuleException($"Impossible de modifier la commande '{order.NumeroCommande}' car elle est déjà {order.Statut}.");

        if (dto.OrderLines == null || !dto.OrderLines.Any())
            throw new ValidationException("OrderLines", "La commande doit comporter au moins une ligne de produit.");

        // Supprimer les anciennes lignes
        _context.OrderLines.RemoveRange(order.OrderLines);

        var newOrderLines = new List<OrderLine>();
        decimal totalHT = 0;

        foreach (var lineDto in dto.OrderLines)
        {
            if (lineDto.Quantite <= 0)
                throw new BusinessRuleException("La quantité commandée doit être strictement supérieure à zéro.");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == lineDto.ProductId, cancellationToken);

            if (product == null)
                throw new NotFoundException("Produit", lineDto.ProductId);

            if (lineDto.Quantite > product.QuantiteEnStock)
            {
                throw new BusinessRuleException(
                    $"Stock insuffisant pour le produit '{product.Nom}'. " +
                    $"Stock disponible: {product.QuantiteEnStock}, Quantité demandée: {lineDto.Quantite}.");
            }

            decimal totalLigne = Math.Round(lineDto.Quantite * product.PrixUnitaireHT, 2, MidpointRounding.AwayFromZero);
            totalHT += totalLigne;

            newOrderLines.Add(new OrderLine
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantite = lineDto.Quantite,
                PrixUnitaire = product.PrixUnitaireHT,
                TotalLigne = totalLigne
            });
        }

        order.TotalHT = Math.Round(totalHT, 2, MidpointRounding.AwayFromZero);
        order.TotalTTC = Math.Round(order.TotalHT * (1 + TvaRate), 2, MidpointRounding.AwayFromZero);
        order.OrderLines = newOrderLines;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderLines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            throw new NotFoundException("Commande", id);

        if (order.Statut == OrderStatus.Validee)
            throw new BusinessRuleException("Impossible de supprimer une commande validée.");

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderDto> ValidateOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            throw new NotFoundException("Commande", id);

        if (order.Statut == OrderStatus.Validee)
            throw new BusinessRuleException("Cette commande est déjà validée.");

        if (order.Statut == OrderStatus.Annulee)
            throw new BusinessRuleException("Impossible de valider une commande annulée.");

        if (!order.OrderLines.Any())
            throw new BusinessRuleException("Impossible de valider une commande sans lignes.");

        // Transaction pour mise à jour cohérente du stock
        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            // Vérifier et décrémenter le stock pour chaque produit
            foreach (var line in order.OrderLines)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == line.ProductId, cancellationToken);

                if (product == null)
                    throw new NotFoundException("Produit", line.ProductId);

                if (product.QuantiteEnStock < line.Quantite)
                {
                    throw new BusinessRuleException(
                        $"Validation impossible : Stock insuffisant pour '{product.Nom}'. " +
                        $"Stock restant: {product.QuantiteEnStock}, Quantité requise: {line.Quantite}.");
                }

                // Mise à jour du stock
                product.QuantiteEnStock -= line.Quantite;
            }

            order.Statut = OrderStatus.Validee;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    public async Task<OrderDto> CancelOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            throw new NotFoundException("Commande", id);

        if (order.Statut == OrderStatus.Annulee)
            throw new BusinessRuleException("Cette commande est déjà annulée.");

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            // Si la commande était validée, on restitue le stock
            if (order.Statut == OrderStatus.Validee)
            {
                foreach (var line in order.OrderLines)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == line.ProductId, cancellationToken);

                    if (product != null)
                    {
                        product.QuantiteEnStock += line.Quantite;
                    }
                }
            }

            order.Statut = OrderStatus.Annulee;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    private static string GenerateOrderNumber()
    {
        return $"CMD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            NumeroCommande = order.NumeroCommande,
            ClientId = order.ClientId,
            ClientNom = order.Client != null 
                ? (!string.IsNullOrWhiteSpace(order.Client.PrenomOuRaisonSociale) 
                    ? $"{order.Client.Nom} ({order.Client.PrenomOuRaisonSociale})" 
                    : order.Client.Nom) 
                : string.Empty,
            DateCommande = order.DateCommande,
            Statut = order.Statut.ToString(),
            TotalHT = order.TotalHT,
            TotalTTC = order.TotalTTC,
            OrderLines = order.OrderLines.Select(ol => new OrderLineDto
            {
                Id = ol.Id,
                ProductId = ol.ProductId,
                ReferenceProduit = ol.Product?.Reference ?? string.Empty,
                NomProduit = ol.Product?.Nom ?? string.Empty,
                Quantite = ol.Quantite,
                PrixUnitaire = ol.PrixUnitaire,
                TotalLigne = ol.TotalLigne
            }).ToList()
        };
    }
}
