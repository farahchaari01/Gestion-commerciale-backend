using Microsoft.EntityFrameworkCore;
using GestionCommerciale.Application.DTOs.Clients;
using GestionCommerciale.Application.Interfaces;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Exceptions;

namespace GestionCommerciale.Application.Services;

public class ClientService : IClientService
{
    private readonly IApplicationDbContext _context;

    public ClientService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClientDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .AsNoTracking()
            .Select(c => new ClientDto
            {
                Id = c.Id,
                Nom = c.Nom,
                PrenomOuRaisonSociale = c.PrenomOuRaisonSociale,
                Email = c.Email,
                Telephone = c.Telephone,
                Adresse = c.Adresse,
                DateCreation = c.DateCreation,
                NombreCommandes = c.Orders.Count
            })
            .OrderByDescending(c => c.DateCreation)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClientDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                Nom = c.Nom,
                PrenomOuRaisonSociale = c.PrenomOuRaisonSociale,
                Email = c.Email,
                Telephone = c.Telephone,
                Adresse = c.Adresse,
                DateCreation = c.DateCreation,
                NombreCommandes = c.Orders.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (client == null)
        {
            throw new NotFoundException("Client", id);
        }

        return client;
    }

    public async Task<ClientDto> CreateAsync(CreateClientDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Nom))
            throw new ValidationException(nameof(dto.Nom), "Le nom du client est obligatoire.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException(nameof(dto.Email), "L'adresse email est obligatoire.");

        var emailExists = await _context.Clients
            .AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower(), cancellationToken);

        if (emailExists)
            throw new BusinessRuleException($"Un client avec l'email '{dto.Email}' existe déjà.");

        var client = new Client
        {
            Nom = dto.Nom.Trim(),
            PrenomOuRaisonSociale = dto.PrenomOuRaisonSociale?.Trim() ?? string.Empty,
            Email = dto.Email.Trim().ToLower(),
            Telephone = dto.Telephone?.Trim() ?? string.Empty,
            Adresse = dto.Adresse?.Trim() ?? string.Empty,
            DateCreation = DateTime.UtcNow
        };

        await _context.Clients.AddAsync(client, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(client.Id, cancellationToken);
    }

    public async Task<ClientDto> UpdateAsync(int id, UpdateClientDto dto, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (client == null)
            throw new NotFoundException("Client", id);

        if (string.IsNullOrWhiteSpace(dto.Nom))
            throw new ValidationException(nameof(dto.Nom), "Le nom du client est obligatoire.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException(nameof(dto.Email), "L'adresse email est obligatoire.");

        var emailExists = await _context.Clients
            .AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower() && c.Id != id, cancellationToken);

        if (emailExists)
            throw new BusinessRuleException($"Un autre client utilise déjà l'email '{dto.Email}'.");

        client.Nom = dto.Nom.Trim();
        client.PrenomOuRaisonSociale = dto.PrenomOuRaisonSociale?.Trim() ?? string.Empty;
        client.Email = dto.Email.Trim().ToLower();
        client.Telephone = dto.Telephone?.Trim() ?? string.Empty;
        client.Adresse = dto.Adresse?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(client.Id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (client == null)
            throw new NotFoundException("Client", id);

        if (client.Orders.Any())
            throw new BusinessRuleException("Impossible de supprimer ce client car il possède déjà des commandes.");

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
