using Microsoft.EntityFrameworkCore;
using GestionCommerciale.Application.DTOs.Products;
using GestionCommerciale.Application.Interfaces;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Exceptions;

namespace GestionCommerciale.Application.Services;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;

    public ProductService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Reference = p.Reference,
                Nom = p.Nom,
                Description = p.Description,
                PrixUnitaireHT = p.PrixUnitaireHT,
                QuantiteEnStock = p.QuantiteEnStock,
                DateCreation = p.DateCreation
            })
            .OrderBy(p => p.Nom)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Reference = p.Reference,
                Nom = p.Nom,
                Description = p.Description,
                PrixUnitaireHT = p.PrixUnitaireHT,
                QuantiteEnStock = p.QuantiteEnStock,
                DateCreation = p.DateCreation
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
            throw new NotFoundException("Produit", id);

        return product;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Reference))
            throw new ValidationException(nameof(dto.Reference), "La référence du produit est obligatoire.");

        if (string.IsNullOrWhiteSpace(dto.Nom))
            throw new ValidationException(nameof(dto.Nom), "Le nom du produit est obligatoire.");

        if (dto.PrixUnitaireHT < 0)
            throw new ValidationException(nameof(dto.PrixUnitaireHT), "Le prix unitaire HT ne peut pas être négatif.");

        if (dto.QuantiteEnStock < 0)
            throw new ValidationException(nameof(dto.QuantiteEnStock), "La quantité en stock ne peut pas être négative.");

        var refExists = await _context.Products
            .AnyAsync(p => p.Reference.ToLower() == dto.Reference.ToLower(), cancellationToken);

        if (refExists)
            throw new BusinessRuleException($"Un produit avec la référence '{dto.Reference}' existe déjà.");

        var product = new Product
        {
            Reference = dto.Reference.Trim().ToUpper(),
            Nom = dto.Nom.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            PrixUnitaireHT = dto.PrixUnitaireHT,
            QuantiteEnStock = dto.QuantiteEnStock,
            DateCreation = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(product.Id, cancellationToken);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Produit", id);

        if (string.IsNullOrWhiteSpace(dto.Reference))
            throw new ValidationException(nameof(dto.Reference), "La référence du produit est obligatoire.");

        if (string.IsNullOrWhiteSpace(dto.Nom))
            throw new ValidationException(nameof(dto.Nom), "Le nom du produit est obligatoire.");

        if (dto.PrixUnitaireHT < 0)
            throw new ValidationException(nameof(dto.PrixUnitaireHT), "Le prix unitaire HT ne peut pas être négatif.");

        if (dto.QuantiteEnStock < 0)
            throw new ValidationException(nameof(dto.QuantiteEnStock), "La quantité en stock ne peut pas être négative.");

        var refExists = await _context.Products
            .AnyAsync(p => p.Reference.ToLower() == dto.Reference.ToLower() && p.Id != id, cancellationToken);

        if (refExists)
            throw new BusinessRuleException($"Un autre produit utilise déjà la référence '{dto.Reference}'.");

        product.Reference = dto.Reference.Trim().ToUpper();
        product.Nom = dto.Nom.Trim();
        product.Description = dto.Description?.Trim() ?? string.Empty;
        product.PrixUnitaireHT = dto.PrixUnitaireHT;
        product.QuantiteEnStock = dto.QuantiteEnStock;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(product.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.OrderLines)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Produit", id);

        if (product.OrderLines.Any())
            throw new BusinessRuleException("Impossible de supprimer ce produit car il est référencé dans des commandes existantes.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
