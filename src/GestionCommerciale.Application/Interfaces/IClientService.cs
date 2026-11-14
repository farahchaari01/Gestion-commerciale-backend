using GestionCommerciale.Application.DTOs.Clients;

namespace GestionCommerciale.Application.Interfaces;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClientDto> CreateAsync(CreateClientDto dto, CancellationToken cancellationToken = default);
    Task<ClientDto> UpdateAsync(int id, UpdateClientDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
