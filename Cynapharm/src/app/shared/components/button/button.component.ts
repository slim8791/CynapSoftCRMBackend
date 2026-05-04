import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './button.component.html',
  styleUrls: ['./button.component.css']
})
export class ButtonComponent {
  @Input() label: string = 'Click me';
  @Input() variant: 'primary' | 'secondary' | 'success' | 'danger' | 'info' = 'primary';
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() disabled: boolean = false;
  @Output() clicked = new EventEmitter<void>();

  onClick(): void {
    if (!this.disabled) {
      this.clicked.emit();
      // For submit buttons, the form will naturally submit via HTML form behavior
      // The click handler ensures the event is captured and forwarded
    }
  }
}
