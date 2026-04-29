import { Directive, EventEmitter, HostListener, Input, Output } from '@angular/core';

@Directive({
  selector: '[appDebounceClick]',
  standalone: true
})
export class DebounceClickDirective {
  @Input() debounceTime: number = 500;
  @Output() debounceClick = new EventEmitter<void>();

  private isClicking = false;

  @HostListener('click')
  clickEvent(): void {
    if (!this.isClicking) {
      this.isClicking = true;
      this.debounceClick.emit();

      setTimeout(() => {
        this.isClicking = false;
      }, this.debounceTime);
    }
  }
}
