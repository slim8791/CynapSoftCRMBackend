import { AbstractControl, ValidationErrors } from '@angular/forms';

export function noWhitespaceValidator(control: AbstractControl): ValidationErrors | null {
  const isWhitespace = (control.value || '').toString().trim().length === 0;
  const isValid = !isWhitespace;
  return isValid ? null : { whitespace: true };
}
