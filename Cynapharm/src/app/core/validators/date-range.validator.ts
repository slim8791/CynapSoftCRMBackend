import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function dateRangeValidator(startDateKey: string, endDateKey: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const startControl = control.get(startDateKey);
    const endControl = control.get(endDateKey);

    if (!startControl || !endControl) {
      return null;
    }

    if (!startControl.value || !endControl.value) {
      return null; // Don't validate if one is missing (let required validators handle this)
    }

    const startDate = new Date(startControl.value);
    const endDate = new Date(endControl.value);

    // Reset previous dateRange error
    if (endControl.errors && !endControl.errors['dateRange']) {
        return null;
    }

    if (startDate > endDate) {
      endControl.setErrors({ dateRange: true });
      return { dateRange: true };
    } else {
      endControl.setErrors(null);
      return null;
    }
  };
}
