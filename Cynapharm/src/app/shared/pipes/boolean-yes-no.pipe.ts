import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'booleanYesNo', standalone: true })
export class BooleanYesNoPipe implements PipeTransform {
  transform(value: boolean | null | undefined, trueLabel = 'Oui', falseLabel = 'Non'): string {
    if (value == null) return '—';
    return value ? trueLabel : falseLabel;
  }
}
