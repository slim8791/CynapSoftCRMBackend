import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { OrderService } from '../order.service';
import { CardComponent } from '../../shared/components/card/card.component';
import { ButtonComponent } from '../../shared/components/button/button.component';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, CardComponent, ButtonComponent],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css'
})
export class OrderDetailComponent implements OnInit {
  order: any;
  loading: boolean = false;
  error: string = '';
  private orderId: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.orderId = params['id'];
      this.loadOrderDetails();
    });
  }

  private loadOrderDetails(): void {
    this.loading = true;
    this.error = '';

    this.orderService.getOrderById(this.orderId).subscribe({
      next: (data) => {
        this.order = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load order details';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onEdit(): void {
    this.router.navigate(['/orders', this.orderId, 'edit']);
  }

  onDelete(): void {
    if (confirm('Are you sure you want to delete this order?')) {
      this.orderService.deleteOrder(this.orderId).subscribe({
        next: () => {
          this.router.navigate(['/orders']);
        },
        error: (err) => {
          console.error('Error deleting order:', err);
        }
      });
    }
  }
}
