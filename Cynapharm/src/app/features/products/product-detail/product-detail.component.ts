import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../product.service';
import { CardComponent } from '../../shared/components/card/card.component';
import { ButtonComponent } from '../../shared/components/button/button.component';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, CardComponent, ButtonComponent],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css'
})
export class ProductDetailComponent implements OnInit {
  product: any;
  loading: boolean = false;
  error: string = '';
  private productId: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.productId = params['id'];
      this.loadProductDetails();
    });
  }

  private loadProductDetails(): void {
    this.loading = true;
    this.error = '';

    this.productService.getProductById(this.productId).subscribe({
      next: (data) => {
        this.product = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load product details';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onEdit(): void {
    this.router.navigate(['/products', this.productId, 'edit']);
  }

  onDelete(): void {
    if (confirm('Are you sure you want to delete this product?')) {
      this.productService.deleteProduct(this.productId).subscribe({
        next: () => {
          this.router.navigate(['/products']);
        },
        error: (err) => {
          console.error('Error deleting product:', err);
        }
      });
    }
  }
}
