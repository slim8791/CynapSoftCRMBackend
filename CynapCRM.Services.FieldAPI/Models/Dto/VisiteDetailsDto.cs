namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class VisiteDetailsDto
    {
        public VisiteDto Visite { get; set; }

        public RapportVisiteDto? Rapport { get; set; }
    }
}
