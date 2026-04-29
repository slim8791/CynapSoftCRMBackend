import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[appHighlight]',
  standalone: true
})
export class HighlightDirective {
  @Input() highlightColor: string = 'yellow';
  @Input() textColor: string = 'black';

  constructor(private el: ElementRef) { }

  @HostListener('mouseenter')
  onMouseEnter(): void {
    this.el.nativeElement.style.backgroundColor = this.highlightColor;
    this.el.nativeElement.style.color = this.textColor;
  }

  @HostListener('mouseleave')
  onMouseLeave(): void {
    this.el.nativeElement.style.backgroundColor = 'transparent';
    this.el.nativeElement.style.color = 'black';
  }
}
