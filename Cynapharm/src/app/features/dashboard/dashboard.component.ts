import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../dashboard.service';
import { CardComponent } from '../../shared/components/card/card.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  metrics: any;
  loading: boolean = false;
  error: string = '';

  constructor(private dashboardService: DashboardService) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  private loadDashboardData(): void {
    this.loading = true;
    this.error = '';

    this.dashboardService.getMetrics().subscribe({
      next: (data) => {
        this.metrics = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load dashboard data';
        this.loading = false;
        console.error(err);
      }
    });
  }
}
