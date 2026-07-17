using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CynapCRM.MessageBus.Events
{
    public record OrderStatusChangedEvent
    {
        public int OrderId { get; init; }
        public int ClientId { get; init; }
        public string OldStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
        public DateTime ChangedAt { get; init; }
        public List<OrderLineItem> Lines { get; init; } = new List<OrderLineItem>();

    }
}
