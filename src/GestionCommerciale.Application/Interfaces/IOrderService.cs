using GestionCommerciale.Application.DTOs.Orders;

namespace GestionCommerciale.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateAsync(int id, UpdateOrderDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderDto> ValidateOrderAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderDto> CancelOrderAsync(int id, CancellationToken cancellationToken = default);
}
