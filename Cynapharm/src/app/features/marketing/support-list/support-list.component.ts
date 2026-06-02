import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { MarketingService } from '../marketing.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';

@Component({
  selector: 'app-support-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ButtonComponent,
    CardComponent
  ],
  templateUrl: './support-list.component.html',
  styleUrls: ['./support-list.component.css']
})
export class SupportListComponent implements OnInit {

  supports: any[] = [];
  loading = true;
  error = '';
  productId!: number;
  productName = '';

  columns = [
    { key: 'type', label: 'Type' },
    { key: 'campaignName', label: 'Campagne' },
    { key: 'isActive', label: 'Statut' },
    { key: 'fichiers', label: 'Fichiers' }
  ];

  constructor(
    private marketingService: MarketingService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.productId = Number(this.route.snapshot.paramMap.get('productId'));
    this.productName = this.route.snapshot.queryParamMap.get('productName') || 'Produit';

    if (!this.productId) {
      this.error = 'ID produit requis';
      this.loading = false;
      return;
    }

    this.loadSupports();
  }

  private loadSupports(): void {
    this.loading = true;
    this.error = '';

    this.marketingService.getSupportsByProductId(this.productId).subscribe({
      next: (response: any) => {
        console.log('✅ Supports response:', response);
        let supportsData = [];

        if (Array.isArray(response)) {
          supportsData = response;
        } else if (response && (response.result || response.Result)) {
          supportsData = response.result || response.Result;
        }

        console.log('✅ Parsed supports:', supportsData);
        this.supports = supportsData.map((s: any) => this.normalizeSupport(s));
        this.loading = false;
      },
      error: (err: any) => {
        console.error('❌ Supports load error:', err);
        this.loading = false;

        if (err.status === 404) {
          this.error = 'Aucun support marketing trouvé pour ce produit';
        } else if (err.status === 403) {
          this.error = 'Accès refusé';
        } else {
          this.error = err.error?.message || 'Impossible de charger les supports';
        }
      }
    });
  }

  private normalizeSupport(support: any): any {
    return {
      ...support,
      idSupportMarketting: support.idSupportMarketting ?? support.IdSupportMarketting,
      type: support.type ?? support.Type,
      idProduit: support.idProduit ?? support.IdProduit,
      isActive: support.isActive ?? support.IsActive ?? true,
      campaignName: support.campaignName ?? support.CampaignName,
      fichiers: support.fichiers ?? support.Fichiers ?? []
    };
  }

  getValue(support: any, key: string): any {
    if (key === 'isActive') {
      return support.isActive ? 'Actif' : 'Inactif';
    }
    if (key === 'fichiers') {
      return (support.fichiers || []).length;
    }
    return support[key] || '';
  }

  onView(support: any): void {
    this.router.navigate(['/marketing/supports', support.idSupportMarketting], {
      queryParams: { productId: this.productId }
    });
  }

  onEdit(support: any): void {
    this.router.navigate(['/marketing/supports', support.idSupportMarketting, 'edit'], {
      queryParams: { productId: this.productId }
    });
  }

  onDelete(support: any): void {
    if (confirm(`Supprimer le support "${support.type}" ?`)) {
      this.marketingService.deleteSupport(support.idSupportMarketting).subscribe({
        next: () => {
          console.log('✅ Support deleted');
          this.loadSupports();
        },
        error: (err: any) => {
          console.error('❌ Delete error:', err);
          alert('Impossible de supprimer le support');
        }
      });
    }
  }

  onBackToProduct(): void {
    this.router.navigate(['/products', this.productId]);
  }
}
