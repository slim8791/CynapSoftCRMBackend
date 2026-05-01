import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="forbidden-container">
      <div class="forbidden-content">
        <h1>403</h1>
        <h2>Access Forbidden</h2>
        <p>You don't have permission to access this resource.</p>
        <button routerLink="/dashboard" class="btn-primary">
          Back to Dashboard
        </button>
      </div>
    </div>
  `,
  styles: [`
    .forbidden-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }

    .forbidden-content {
      text-align: center;
      background: white;
      padding: 40px;
      border-radius: 8px;
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
    }

    h1 {
      font-size: 72px;
      margin: 0;
      color: #667eea;
    }

    h2 {
      font-size: 24px;
      margin: 10px 0;
      color: #333;
    }

    p {
      color: #666;
      margin: 15px 0 25px;
      font-size: 16px;
    }

    .btn-primary {
      background-color: #667eea;
      color: white;
      padding: 12px 30px;
      border: none;
      border-radius: 4px;
      font-size: 16px;
      cursor: pointer;
      transition: background-color 0.3s;
    }

    .btn-primary:hover {
      background-color: #764ba2;
    }
  `]
})
export class ForbiddenComponent {}
