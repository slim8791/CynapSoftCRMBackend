using AutoMapper;
using CynapCRM.Services.OrderAPI.Data;
using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Humanizer;
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
        public async Task<LigneCommandeDto?> CreateOrUpdateLigneCommandeAsync(CreateOrUpdateLigneCommandeDto ligneDto)
        {

            var commande = await _db.Commandes
                            .Include(c => c.Lignes)
                            .FirstOrDefaultAsync(c => c.Id_Commande == ligneDto.Id_Commande);


            if (commande == null || ligneDto.Quantite <= 0)
                return null;

            LigneCommande ligne;

            if (ligneDto.Id_Ligne == 0)
            {
                // CREATE — only allowed while order is still editable
                if (commande.Statut != EtatCommande.Brouillon &&
                    commande.Statut != EtatCommande.EnAttente)
                    return null;

                ligne = _mapper.Map<LigneCommande>(ligneDto);
                ligne.Commande = commande;
                ligne.NumeroLot = null;
                commande.Lignes.Add(ligne);
            }
            else
            {
                // UPDATE — lot assignment is allowed up to Expediee (4)
                var allowedStatuts = new[] { 0, 1, 2, 3, 4 };
                if (!allowedStatuts.Contains((int)commande.Statut))
                    return null;

                // FIX 1: query LignesCommandes directly instead of navigation property
                ligne = await _db.LignesCommandes
                    .FirstOrDefaultAsync(l =>
                        l.Id_Ligne    == ligneDto.Id_Ligne &&
                        l.Id_Commande == ligneDto.Id_Commande);

                if (ligne == null)
                    return null;

                // Lot-only assignment: fast path, save and return immediately
                if (!string.IsNullOrEmpty(ligneDto.NumeroLot))
                {
                    ligne.NumeroLot = ligneDto.NumeroLot;
                    await _db.SaveChangesAsync();
                    return _mapper.Map<LigneCommandeDto>(ligne);
                }

                // Full field update — only while order is still editable
                if (commande.Statut != EtatCommande.Brouillon &&
                    commande.Statut != EtatCommande.EnAttente)
                    return null;

                _mapper.Map(ligneDto, ligne);
            }

            // FIX 2: recalculate from LignesCommandes directly to avoid stale navigation cache
            var toutesLignes = await _db.LignesCommandes
                .Where(l => l.Id_Commande == ligneDto.Id_Commande)
                .ToListAsync();

            commande.MontantTotalHT = toutesLignes.Sum(l =>
                l.PrixUnitaire * l.Quantite * (1 - l.Remise / 100));

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

            // FIX: retirer de la collection EN MÉMOIRE avant de recalculer
            commande.Lignes.Remove(ligne);
            _db.LignesCommandes.Remove(ligne);

            // FIX: recalcul propre — la ligne supprimée n'est plus dans la collection
            commande.MontantTotalHT = commande.Lignes.Sum(l =>
                (l.PrixUnitaire * l.Quantite) * (1 - (l.Remise / 100)));

            commande.MontantTTC = commande.MontantTotalHT *
                (1 + CreateOrderDto.TauxTVA);

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
