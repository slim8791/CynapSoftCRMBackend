namespace CynapCRM.Services.DocAPI.Models.Dto
{
    public class DocumentDto
    {
        public int Numero_Doc { get; set; }
        public string Nom_Doc { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }

        public int Id_Commande { get; set; }
        public int? Id_Client { get; set; }

        public string TypeDocument { get; set; } = string.Empty;

        public string? CloudinaryUrl { get; set; }

    }
}
