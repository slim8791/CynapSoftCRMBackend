namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class DistributeEchantillonRequestDto
    {
        public int Id_Delegue { get; set; }
        public int? Id_Medecin { get; set; }
        public int? Id_Pharmacien { get; set; }
        public int IdStock { get; set; }
        public int Qte { get; set; }

    }
}
