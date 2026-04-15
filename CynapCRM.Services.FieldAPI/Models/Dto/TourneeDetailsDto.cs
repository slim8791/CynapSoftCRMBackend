namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class TourneeDetailsDto
    {
        public TourneeDto Tournee { get; set; }

        public List<VisiteDto> Visites { get; set; } = new();
    }
}
