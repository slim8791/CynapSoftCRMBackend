import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './table.component.html',
  styleUrl: './table.component.css'
})
export class TableComponent {
  @Input() columns: string[] = [];
  @Input() data: any[] = [];

  getColumnValue(row: any, column: string): any {
    const keys = column.split('.');
    let value = row;

    for (const key of keys) {
      value = value[key];
      if (value === undefined) break;
    }

    return value;
  }
}
