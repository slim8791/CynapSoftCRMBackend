import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { LotService } from '../lot.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';

@Component({
  selector: 'app-lot-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ButtonComponent,
    CardComponent
  ],
  templateUrl: './lot-list.component.html',
  styleUrls: ['./lot-list.component.css']
})

export class LotListComponent implements OnInit {

  lots: any[] = [];
  loading = true;
  error = '';
  productId: number | null = null;
  productName: string = '';

  columns = [
    { key: 'numero', label: 'Lot #' },
    { key: 'quantite', label: 'Quantité' },
    { key: 'dateExpiration', label: 'Expiration' },
    { key: 'status', label: 'Status' }
  ];

  constructor(
    private lotService: LotService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.productId = params['productId'] ? Number(params['productId']) : null;
      this.productName = params['productName'] ?? '';
      this.loadLots();
    });
  }

  private loadLots(): void {
    this.loading = true;
    this.error = '';

    if (this.productId) {
      console.log('[LotList] Loading lots for product ID:', this.productId);
      this.lotService.getLotsByProductId(this.productId).subscribe({
        next: (response: any) => {
          console.log('[LotList] GetLotsByProductId response:', response);
          
          const data = response?.Result ?? response?.result ?? response ?? [];
          console.log('[LotList] Extracted data:', data, 'Type:', typeof data, 'IsArray:', Array.isArray(data));

          if (Array.isArray(data)) {
            this.lots = data.map(l => this.normalizeLot(l));
            console.log('[LotList] Loaded', this.lots.length, 'lots for product', this.productId);
          } else {
            console.warn('[LotList] Data is not an array:', data);
            this.lots = [];
          }

          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          console.error('[LotList] GetLotsByProductId error:', err);
          this.loading = false;
          this.error = err.error?.message || 'Erreur lors du chargement des lots';
          this.cdr.markForCheck();
        }
      });
    } else {
      console.log('[LotList] Loading all lots');
      this.lotService.getAllLots().subscribe({
        next: (response: any) => {
          console.log('[LotList] GetAllLots response:', response);
          
          const data = response?.Result ?? response?.result ?? response ?? [];
          console.log('[LotList] Extracted data:', data, 'Type:', typeof data, 'IsArray:', Array.isArray(data));

          if (Array.isArray(data)) {
            this.lots = data.map(l => this.normalizeLot(l));
            console.log('[LotList] Loaded', this.lots.length, 'lots');
          } else {
            console.warn('[LotList] Data is not an array:', data);
            this.lots = [];
          }

          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err: any) => {
          console.error('[LotList] GetAllLots error:', err);
          this.loading = false;
          this.error = err.error?.message || 'Erreur lors du chargement des lots';
          this.cdr.markForCheck();
        }
      });
    }
  }

  private normalizeLot(lot: any): any {
    return {
      ...lot,
      numero: lot.numero ?? lot.Numero,
      quantite: lot.quantite ?? lot.Quantite,
      dateExpiration: lot.dateExpiration ?? lot.DateExpiration,
      isExpired: lot.isExpired ?? lot.IsExpired ?? false,
      isOutOfStock: lot.isOutOfStock ?? lot.IsOutOfStock ?? false
    };
  }

  getStatusText(lot: any): string {
    if (lot.isExpired) return 'Expired';
    if (lot.isOutOfStock) return 'Out of stock';
    return 'Active';
  }

  getStatusClass(lot: any): string {
    if (lot.isExpired) return 'status-expired';
    if (lot.isOutOfStock) return 'status-low-stock';
    return 'status-active';
  }

  getValue(lot: any, key: string): any {
    if (key === 'dateExpiration') {
      return lot.dateExpiration
        ? new Date(lot.dateExpiration).toLocaleDateString()
        : 'N/A';
    }
    return lot[key] ?? '';
  }

  onView(lot: any): void {
    const extras = { queryParams: this.productId ? {productId: this.productId} : {} };
    this.router.navigate(['/lots', lot.numero], extras);
  }

  onEdit(lot: any): void {
    const extras = { queryParams: this.productId ? {productId: this.productId} : {} };
    this.router.navigate(['/lots', lot.numero, 'edit'], extras);
  }

  onDelete(lot: any): void {
    if (!confirm(`Supprimer le lot ${lot.numero}?`)) return;

    this.lotService.deleteLot(lot.numero).subscribe({
      next: () => {
        console.log('[LotList] Lot deleted successfully');
        this.loadLots();
      },
      error: (err) => {
        console.error('[LotList] Error deleting lot:', err);
        alert('Erreur lors de la suppression du lot');
      }
    });
  }
}
