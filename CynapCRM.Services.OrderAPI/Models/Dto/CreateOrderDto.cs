namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class CreateOrderDto
    {
        public int Id_Client { get; set; }

        // On demande juste la liste des produits et quantités
        public List<LigneCommandeDto> Lignes { get; set; } = new();
        public bool IsFinalValidation { get; set; }
        public const decimal TauxTVA = 0.19m;
    }
}
