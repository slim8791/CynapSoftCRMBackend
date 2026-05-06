﻿using AutoMapper;
using CynapCRM.Services.ProductAPI.Models;
using CynapCRM.Services.ProductAPI.Models.Dto;


namespace CynapCRM.Services.ProductAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        {
            // Entity <-> DTO mapping with explicit field names
            CreateMap<Produit, ProduitDto>()
                .ForMember(d => d.Prix_Vente, opt => opt.MapFrom(s => s.PrixVente))
                .ReverseMap()
                .ForMember(s => s.PrixVente, opt => opt.MapFrom(d => d.Prix_Vente));
                
            CreateMap<Lot, LotDto>().ReverseMap();
            CreateMap<Promotion, PromotionDto>().ReverseMap();
            CreateMap<Support_Marketting, SupportMarketingDto>().ReverseMap();
            CreateMap<Fichier, FichierDto>().ReverseMap();
        }
    }
}
