import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastSubject = new BehaviorSubject<ToastMessage[]>([]);
  public toasts$: Observable<ToastMessage[]> = this.toastSubject.asObservable();

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }

  show(message: string, type: ToastMessage['type'] = 'info', duration: number = 5000): void {
    const toast: ToastMessage = {
      id: this.generateId(),
      type,
      message,
      duration
    };

    const currentToasts = this.toastSubject.value;
    this.toastSubject.next([...currentToasts, toast]);

    // Auto dismiss
    timer(duration).subscribe(() => this.remove(toast.id));
  }

  showSuccess(message: string, duration?: number): void {
    this.show(message, 'success', duration);
  }

  showError(message: string, duration?: number): void {
    this.show(message, 'error', duration);
  }

  showWarning(message: string, duration?: number): void {
    this.show(message, 'warning', duration);
  }

  private remove(id: string): void {
    const currentToasts = this.toastSubject.value;
    this.toastSubject.next(currentToasts.filter(t => t.id !== id));
  }

  removeAll(): void {
    this.toastSubject.next([]);
  }

  clear(): void {
    this.removeAll();
  }
}

