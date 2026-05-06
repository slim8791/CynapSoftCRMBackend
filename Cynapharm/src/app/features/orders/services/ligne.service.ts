import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';

// PascalCase — OrderAPI has no JsonNamingPolicy.CamelCase
export interface CreateOrUpdateLigneDto {
  Id_Commande:  number;
  Id_Produit:   number;
  Id_Ligne:     number;   // 0 = create, >0 = update
  Quantite:     number;
  Remise:       number;
  PrixUnitaire: number;
}

@Injectable({ providedIn: 'root' })
export class LigneService {

  private readonly base = '/orders/lignes';

  constructor(private api: ApiService) {}

  createOrUpdate(dto: CreateOrUpdateLigneDto): Observable<any> {
    return this.api.post<any>(this.base, dto);
  }

  delete(ligneId: number): Observable<any> {
    return this.api.delete<any>(`${this.base}/${ligneId}`);
  }
}
