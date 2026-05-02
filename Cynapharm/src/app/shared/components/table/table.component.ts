import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.css']
})
export class TableComponent {
  // Support both string[] and { key, label }[]
  @Input() columns: (string | { key: string; label: string })[] = [];
  @Input() data: any[] = [];

  getColumnKey(column: string | { key: string; label: string }): string {
    return typeof column === 'string' ? column : column.key;
  }

  getColumnLabel(column: string | { key: string; label: string }): string {
    return typeof column === 'string' ? column : column.label;
  }

  getColumnValue(row: any, column: string | { key: string; label: string }): any {
    const key = this.getColumnKey(column);
    
    // Try direct key first
    if (row[key] !== undefined) {
      return row[key];
    }
    
    // Try case-insensitive match
    const lowerKey = key.toLowerCase();
    for (const rowKey of Object.keys(row)) {
      if (rowKey.toLowerCase() === lowerKey) {
        return row[rowKey];
      }
    }
    
    // Try with underscore prefix/suffix variations
    const underscoreKey = '_' + key;
    if (row[underscoreKey] !== undefined) {
      return row[underscoreKey];
    }
    
    return undefined;
  }
}
