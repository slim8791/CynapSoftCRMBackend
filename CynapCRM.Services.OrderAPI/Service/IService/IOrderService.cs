using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;

public interface IOrderService
{
    Task<IEnumerable<CommandeDto>> GetOrdersByDateRangeAsync(
    DateTime startDate, DateTime endDate,
    int page = 1, int pageSize = 20);
    Task<bool> CancelOrderAsync(int idCommande, string motif);  
    Task<OrderDashboardDto> GetOrdersDashboardAsync();
    Task<IEnumerable<CommandeDto>> GetOrdersByStatusAsync(
    EtatCommande statut, int page = 1, int pageSize = 20);
    Task<IEnumerable<CommandeDto>> GetAllOrdersAsync(
        int page,
        int pageSize,
        EtatCommande? statut = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    Task<CommandeDto> GetOrderByIdAsync(int orderId);

    Task<IEnumerable<CommandeDto>> GetOrdersByClientIdAsync(
        int clientId, int page = 1, int pageSize = 20);
    Task<CommandeDto> CreateOrderAsync(CreateOrderDto orderDto);

    Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);

    Task<bool> DeleteOrderAsync(int idCommande);

}