namespace CynapCRM.Services.ProductAPI.Models.Dto
{
    public class FichierDto
    {

        public int Id_Fichier { get; set; }
        public string NomFichier { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long Taille { get; set; }

        public int Id_Support { get; set; }

    }
}
