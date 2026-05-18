using AutoMapper;
using CynapCRM.Services.FieldAPI.Models;
using CynapCRM.Services.FieldAPI.Models.Dto;



namespace CynapCRM.Services.DocAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        {

            CreateMap<Region, RegionDto>().ReverseMap();
            CreateMap<Objectif_Delegue, ObjectifDelegueDto>().ReverseMap();
            CreateMap<Planning_Visite, PlanningVisiteDto>().ReverseMap();

            CreateMap<Visite, VisiteDto>()
                .ForMember(d => d.IdVisite, o => o.MapFrom(s => s.Id_Visite))
                .ForMember(d => d.Id_User_Delegue, o => o.MapFrom(s => s.Id_User_Delegue))
                .ForMember(d => d.IdMedecin, o => o.MapFrom(s => s.Id_Medecin))
                .ForMember(d => d.IdPharmacien, o => o.MapFrom(s => s.Id_Pharmacien))
                .ForMember(d => d.IdPlanning, o => o.MapFrom(s => s.Id_Planning))
                .ForMember(d => d.HasRapport, o => o.MapFrom(s => s.Rapport != null));
            
            CreateMap<Visite, VisiteDetailsDto>().ReverseMap();

            CreateMap<Rapport_Visite, RapportVisiteDto>()
                .ForMember(d => d.Date, opt => opt.MapFrom(s => s.DateRapport))
                .ReverseMap()
                .ForMember(s => s.DateRapport, opt => opt.MapFrom(d => d.Date));

            CreateMap<ActiviteHistoriqueDto, ActiviteHistoriqueDto>();
            CreateMap<PerformanceDto, PerformanceDto>();


        }
    }
}
