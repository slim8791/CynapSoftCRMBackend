import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-paginator',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './paginator.component.html',
  styleUrls: ['./paginator.component.css']
})
export class PaginatorComponent implements OnChanges {
  @Input() page       = 1;
  @Input() pageSize   = 10;
  @Input() total      = 0;
  @Output() pageChange = new EventEmitter<number>();

  totalPages = 0;
  pages:     number[] = [];
  from       = 0;
  to         = 0;

  ngOnChanges(): void {
    this.totalPages = Math.ceil(this.total / this.pageSize) || 1;
    this.from       = Math.min((this.page - 1) * this.pageSize + 1, this.total);
    this.to         = Math.min(this.page * this.pageSize, this.total);

    const maxVisible = 5;
    let start = Math.max(1, this.page - Math.floor(maxVisible / 2));
    const end = Math.min(this.totalPages, start + maxVisible - 1);
    start = Math.max(1, end - maxVisible + 1);
    this.pages = Array.from({ length: end - start + 1 }, (_, i) => start + i);
  }

  go(n: number): void {
    if (n >= 1 && n <= this.totalPages) this.pageChange.emit(n);
  }
}
