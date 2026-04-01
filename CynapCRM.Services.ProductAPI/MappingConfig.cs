using AutoMapper;
using CynapCRM.Services.ProductAPI.Models;
using CynapCRM.Services.ProductAPI.Models.Dto;


namespace CynapCRM.Services.ProductAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        {
            // CreateMap<Source, Destination>()
            CreateMap<Produit, ProduitDto>().ReverseMap();
            CreateMap<Lot, LotDto>().ReverseMap();
            CreateMap<Promotion, PromotionDto>().ReverseMap();
            CreateMap<Support_Marketting, SupportMarketingDto>().ReverseMap();
            CreateMap<Fichier, FichierDto>().ReverseMap();
        }
    }
}
