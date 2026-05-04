import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { LotService } from '../lot.service';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-lot-detail',
  standalone: true,
  imports: [CommonModule, CardComponent, ButtonComponent],
  templateUrl: './lot-detail.component.html',
  styleUrls: ['./lot-detail.component.css']
})
export class LotDetailComponent implements OnInit {

  lot: any = null;
  loading = true;
  error = '';
  productId: number | null = null;

  private numeroLot = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private lotService: LotService
  ) {}

  ngOnInit(): void {
    this.numeroLot = this.route.snapshot.paramMap.get('numero') || '';
    this.productId = Number(this.route.snapshot.queryParamMap.get('productId')) || null;

    if (!this.numeroLot) {
      this.error = 'Lot number is missing';
      this.loading = false;
      return;
    }

    this.loadLotDetails();
  }

  private loadLotDetails(): void {
    this.loading = true;
    this.error = '';

    this.lotService.getLotByNumero(this.numeroLot).subscribe({
      next: (response: any) => {
        console.log('✅ Lot response:', response);
        const lotData = response?.Result ?? response?.result ?? response;
        this.lot = this.normalizeLot(lotData);
        console.log('✅ Lot details:', this.lot);
        this.loading = false;
      },
      error: (err: any) => {
        console.error('❌ Error:', err);
        this.loading = false;
        this.error = err.error?.message || 'Failed to load lot details';
      }
    });
  }

  private normalizeLot(lot: any): any {
    return {
      numero: lot.numero ?? lot.Numero,
      quantite: lot.quantite ?? lot.Quantite,
      dateExpiration: lot.dateExpiration ?? lot.DateExpiration,
      isExpired: lot.isExpired ?? lot.IsExpired ?? false,
      isOutOfStock: lot.isOutOfStock ?? lot.IsOutOfStock ?? false
    };
  }

  onEdit(): void {
    const extras: any = {};
    if (this.productId) {
      extras.queryParams = { productId: this.productId };
    }
    this.router.navigate(['/lots', this.numeroLot, 'edit'], extras);
  }

  onDelete(): void {
    if (confirm(`Delete lot ${this.numeroLot}?`)) {
      this.lotService.deleteLot(this.numeroLot).subscribe({
        next: () => {
          console.log('✅ Lot deleted');
          const extras: any = {};
          if (this.productId) {
            extras.queryParams = { productId: this.productId };
          }
          this.router.navigate(['/lots'], extras);
        },
        error: (err: any) => {
          console.error('❌ Delete error:', err);
          alert('Failed to delete lot');
        }
      });
    }
  }

  onBackToList(): void {
    const extras: any = {};
    if (this.productId) {
      extras.queryParams = { productId: this.productId };
    }
    this.router.navigate(['/lots'], extras);
  }
}
