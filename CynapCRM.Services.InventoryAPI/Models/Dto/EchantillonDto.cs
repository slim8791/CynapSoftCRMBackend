namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class EchantillonDto
    {
        public int Id_Distribution { get; set; }

        public int Id_Delegue { get; set; }

        public int? Id_Medecin { get; set; }

        public int? Id_Pharmacien { get; set; }

        public int Qte { get; set; }

        public string NumeroLot { get; set; }

        public DateTime DateDistribution { get; set; }
    }
}
