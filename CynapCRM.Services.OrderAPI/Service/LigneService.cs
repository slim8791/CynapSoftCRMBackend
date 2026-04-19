using AutoMapper;
using CynapCRM.Services.OrderAPI.Data;
using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.OrderAPI.Service
{
    public class LigneService : ILigneService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        public LigneService(IMapper mapper, AppDbContext db)
        {
            _mapper = mapper;
            _db = db;
        }
        public async Task<LigneCommandeDto?> CreateOrUpdateLigneCommandeAsync(LigneCommandeDto ligneDto)
        {

            var commande = await _db.Commandes
                            .Include(c => c.Lignes)
                            .FirstOrDefaultAsync(c => c.Id_Commande == ligneDto.Id_Commande);

            if (commande == null)
                return null;

            // ✅ Commande modifiable uniquement
            if (commande.Statut != EtatCommande.Brouillon &&
                commande.Statut != EtatCommande.EnAttente)
                return null;

            LigneCommande ligne;

            if (ligneDto.Id_Ligne == 0)
            {
                // ➕ Nouvelle ligne
                ligne = _mapper.Map<LigneCommande>(ligneDto);
                commande.Lignes.Add(ligne);
            }
            else
            {
                // ✏️ Mise à jour
                ligne = commande.Lignes
                    .FirstOrDefault(l => l.Id_Ligne == ligneDto.Id_Ligne);

                if (ligne == null)
                    return null;

                _mapper.Map(ligneDto, ligne);
            }

            // ✅ Recalcul du montant HT
            commande.MontantTotalHT = commande.Lignes.Sum(l =>
                (l.PrixUnitaire * l.Quantite) * (1 - (l.Remise / 100)));

            commande.MontantTTC = commande.MontantTotalHT *
                (1 + CreateOrderDto.TauxTVA);

            await _db.SaveChangesAsync();
            return _mapper.Map<LigneCommandeDto>(ligne);

        }

        public async Task<bool> RemoveLigneCommandeAsync(int ligneId)
        {

            var ligne = await _db.LignesCommandes
                            .Include(l => l.Commande)
                            .ThenInclude(c => c.Lignes)
                            .FirstOrDefaultAsync(l => l.Id_Ligne == ligneId);

            if (ligne == null)
                return false;

            var commande = ligne.Commande;

            if (commande == null ||
                (commande.Statut != EtatCommande.Brouillon &&
                 commande.Statut != EtatCommande.EnAttente))
                return false;

            _db.LignesCommandes.Remove(ligne);

            //  Recalcul des montants
            commande.MontantTotalHT = commande.Lignes
                .Where(l => l.Id_Ligne != ligneId)
                .Sum(l =>
                    (l.PrixUnitaire * l.Quantite) *
                    (1 - (l.Remise / 100)));

            commande.MontantTTC = commande.MontantTotalHT *
                (1 + CreateOrderDto.TauxTVA);

            await _db.SaveChangesAsync();
            return true;



        }
    }
}
