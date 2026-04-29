import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
  name: 'dateFormat',
  standalone: true
})
export class DateFormatPipe implements PipeTransform {
  private datePipe = new DatePipe('en-US');

  transform(value: any, format: string = 'medium'): string | null {
    return this.datePipe.transform(value, format);
  }
}
