using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CynapCRM.MessageBus.Events
{
    public record OrderCreatedEvent
    {
        public int OrderId { get; init; }
        public int ClientId { get; init; }
        public DateTime OrderDate { get; init; }
        public decimal MontantTotalHT { get; init; }
        public List<OrderLineItem> Lines { get; init; } = new List<OrderLineItem>();
    }
    public record OrderLineItem
    {
        public int ProductId { get; init; }
        public int Quantity { get; init; }
        public string? NumeroLot { get; init; }
    }
}
