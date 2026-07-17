using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CynapCRM.MessageBus.Events
{
    public record StockDistributedEvent
    {
        public int DelegueId { get; init; }
        public int MedecinId { get; init; }
        public int PharmacienId { get; init; }
        public int StockId { get; init; }
        public int Quantite { get; init; }
        public string NumeroLot { get; init; } = string.Empty;
        public DateTime DateDistribution { get; init; }
    }
}
