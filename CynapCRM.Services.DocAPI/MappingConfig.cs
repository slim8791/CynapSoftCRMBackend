using AutoMapper;
using CynapCRM.Services.DocAPI.Models;
using CynapCRM.Services.DocAPI.Models.Dto;



namespace CynapCRM.Services.DocAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        {
            CreateMap<DocumentDto, Document>().ReverseMap();
            CreateMap<BonCommandeDto, BonCommande>().ReverseMap();
            CreateMap<BonLivraisonDto, BonLivraison>().ReverseMap();
            CreateMap<FactureDto, Facture>().ReverseMap();
        }
    }
}
