using CynapCRM.Services.OrderAPI.Models.Dto;

public interface IOrderService
{ 
    Task<IEnumerable<CommandeDto>> GetAllOrdersAsync(int page, int pageSize);

    Task<CommandeDto> GetOrderByIdAsync(int orderId);

    Task<IEnumerable<CommandeDto>> GetOrdersByClientIdAsync(int clientId);

    Task<CommandeDto> CreateOrderAsync(CreateOrderDto orderDto);

    Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);

    Task<bool> DeleteOrderAsync(int idCommande);

}