import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { OrderService } from '../order.service';
import { CardComponent } from '../../shared/components/card/card.component';
import { ButtonComponent } from '../../shared/components/button/button.component';

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, CardComponent, ButtonComponent],
  templateUrl: './order-form.component.html',
  styleUrl: './order-form.component.css'
})
export class OrderFormComponent implements OnInit {
  orderForm: FormGroup;
  isEditMode = false;
  loading = false;
  error = '';
  success = false;
  private orderId = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService
  ) {
    this.orderForm = this.fb.group({
      orderNumber: ['', Validators.required],
      status: ['', Validators.required],
      totalAmount: [0, [Validators.required, Validators.min(0)]],
      createdDate: ['', Validators.required],
      items: ['[]', Validators.required]
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.orderId = params['id'];
        this.loadOrderData();
      }
    });
  }

  private loadOrderData(): void {
    this.orderService.getOrderById(this.orderId).subscribe({
      next: (data) => {
        this.orderForm.patchValue({
          orderNumber: data.orderNumber,
          status: data.status,
          totalAmount: data.totalAmount,
          createdDate: data.createdDate?.split('T')[0] || data.createdDate,
          items: JSON.stringify(data.items || [])
        });
      },
      error: (err) => {
        this.error = 'Failed to load order data';
        console.error(err);
      }
    });
  }

  onSubmit(): void {
    if (!this.orderForm.valid) {
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = false;

    const rawData = this.orderForm.value;
    let items = [];

    try {
      items = JSON.parse(rawData.items || '[]');
    } catch {
      this.error = 'Items must be valid JSON.';
      this.loading = false;
      return;
    }

    const orderData = {
      orderNumber: rawData.orderNumber,
      status: rawData.status,
      totalAmount: rawData.totalAmount,
      createdDate: rawData.createdDate,
      items
    };

    const request = this.isEditMode
      ? this.orderService.updateOrder(this.orderId, orderData)
      : this.orderService.createOrder(orderData);

    request.subscribe({
      next: () => {
        this.loading = false;
        this.success = true;
        setTimeout(() => {
          this.router.navigate(['/orders']);
        }, 1200);
      },
      error: (err) => {
        this.error = `Failed to ${this.isEditMode ? 'update' : 'create'} order`;
        this.loading = false;
        console.error(err);
      }
    });
  }
}
