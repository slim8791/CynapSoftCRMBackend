import { Component, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastMessage } from '../../services/toast.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container" *ngIf="toasts.length > 0">
      <div 
        *ngFor="let toast of toasts" 
        class="toast" 
        [ngClass]="'toast-' + toast.type"
        role="alert"
        aria-live="assertive"
      >
        <div class="toast-message">{{ toast.message }}</div>
        <button class="toast-close" (click)="removeToast(toast.id)" aria-label="Close">
          ×
        </button>
      </div>
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 400px;
    }

    .toast {
      padding: 16px 20px;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      display: flex;
      justify-content: space-between;
      align-items: center;
      animation: slideIn 0.3s ease-out;
      max-width: 400px;
      word-wrap: break-word;
    }

    .toast-success {
      background: linear-gradient(135deg, #10b981, #059669);
      color: white;
      border-left: 4px solid #059669;
    }

    .toast-error {
      background: linear-gradient(135deg, #ef4444, #dc2626);
      color: white;
      border-left: 4px solid #dc2626;
    }

    .toast-warning {
      background: linear-gradient(135deg, #f59e0b, #d97706);
      color: white;
      border-left: 4px solid #d97706;
    }

    .toast-info {
      background: linear-gradient(135deg, #3b82f6, #2563eb);
      color: white;
      border-left: 4px solid #2563eb;
    }

    .toast-message {
      flex-grow: 1;
      font-size: 14px;
      line-height: 1.4;
    }

    .toast-close {
      background: none;
      border: none;
      color: inherit;
      font-size: 20px;
      font-weight: bold;
      cursor: pointer;
      padding: 0 8px;
      opacity: 0.8;
      line-height: 1;
    }

    .toast-close:hover {
      opacity: 1;
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    @media (max-width: 768px) {
      .toast-container {
        top: 10px;
        left: 10px;
        right: 10px;
        max-width: none;
      }
    }
  `]
})
export class ToastComponent implements OnDestroy {
  toasts: ToastMessage[] = [];
  private subscription?: Subscription;

  constructor(private toastService: ToastService) {
    this.subscription = this.toastService.toasts$.subscribe(toasts => {
      this.toasts = toasts;
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  removeToast(id: string): void {
    this.toastService.clear(); // Simplified - clears all
  }
}

