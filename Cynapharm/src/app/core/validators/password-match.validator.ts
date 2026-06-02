import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function passwordMatchValidator(passwordKey: string, confirmPasswordKey: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = control.get(passwordKey);
    const confirmPassword = control.get(confirmPasswordKey);

    if (!password || !confirmPassword) {
      return null;
    }

    // Only validate if both fields have been touched/dirty to avoid premature errors
    if (confirmPassword.errors && !confirmPassword.errors['passwordMismatch']) {
      // return if another validator has already found an error on the matchingControl
      return null;
    }

    if (password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      confirmPassword.setErrors(null);
      return null;
    }
  };
}
