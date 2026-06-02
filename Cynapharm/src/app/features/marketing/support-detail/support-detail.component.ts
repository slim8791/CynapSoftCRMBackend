import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MarketingService } from '../marketing.service';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-support-detail',
  standalone: true,
  imports: [
    CommonModule,
    CardComponent,
    ButtonComponent
  ],
  templateUrl: './support-detail.component.html',
  styleUrls: ['./support-detail.component.css']
})
export class SupportDetailComponent implements OnInit {

  support: any = null;
  loading = true;
  error = '';

  productId!: number;
  private supportId!: number;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private marketingService: MarketingService
  ) {}

  ngOnInit(): void {
    this.supportId = Number(this.route.snapshot.paramMap.get('id'));
    this.productId = Number(this.route.snapshot.queryParamMap.get('productId'));

    this.loadSupport();
  }

  private loadSupport(): void {
    this.loading = true;
    this.marketingService.getSupportById(this.supportId).subscribe({
      next: (response: any) => {
        const data = response.result || response;
        this.support = this.normalizeSupport(data);
        console.log('✅ Support loaded:', this.support);
        this.loading = false;
      },
      error: (err: any) => {
        console.error('❌ Load error:', err);
        this.loading = false;
        if (err.status === 404) {
          this.error = 'Support introuvable';
        } else if (err.status === 403) {
          this.error = 'Accès refusé';
        } else {
          this.error = 'Impossible de charger le support';
        }
      }
    });
  }

  private normalizeSupport(data: any): any {
    return {
      Id: data.idSupportMarketting ?? data.id ?? data.Id,
      Type: data.type ?? data.Type,
      CampaignName: data.campaignName ?? data.CampaignName,
      IsActive: data.isActive ?? data.IsActive ?? true,
      IdProduit: data.idProduit ?? data.IdProduit,
      Fichiers: (data.fichiers ?? data.Fichiers ?? []).map((f: any) => this.normalizeFichier(f))
    };
  }

  private normalizeFichier(file: any): any {
    return {
      IdFichier: file.idFichier ?? file.Id,
      NomFichier: file.nomFichier ?? file.Name,
      Url: file.url ?? file.Url,
      Extension: file.extension ?? file.Extension,
      Taille: file.taille ?? file.Taille
    };
  }

  getValue(obj: any, key: string): any {
    return obj?.[key] ?? '';
  }

  onEdit(): void {
    this.router.navigate(['/marketing/supports', this.supportId, 'edit'], {
      queryParams: { productId: this.productId }
    });
  }

  onDelete(): void {
    if (confirm('Supprimer ce support ?')) {
      this.loading = true;
      this.marketingService.deleteSupport(this.supportId).subscribe({
        next: () => {
          console.log('✅ Support deleted');
          this.router.navigate(['/marketing/supports'], {
            queryParams: { productId: this.productId }
          });
        },
        error: (err: any) => {
          console.error('❌ Delete error:', err);
          this.loading = false;
          this.error = 'Impossible de supprimer le support';
        }
      });
    }
  }

  onBack(): void {
    this.router.navigate(['/marketing/supports'], {
      queryParams: { productId: this.productId }
    });
  }
}
