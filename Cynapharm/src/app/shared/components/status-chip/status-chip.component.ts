import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-chip',
  standalone: true,
  templateUrl: './status-chip.component.html',
  styleUrls: ['./status-chip.component.css']
})
export class StatusChipComponent {
  @Input() label   = '';
  @Input() variant: 'success' | 'danger' | 'warning' | 'info' | 'secondary' | 'default' = 'default';
}
