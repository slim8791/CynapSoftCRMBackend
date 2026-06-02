import { describe, it, expect } from 'vitest';
import { FormControl, FormGroup } from '@angular/forms';
import { passwordMatchValidator } from './password-match.validator';

describe('passwordMatchValidator', () => {
  function buildGroup(pass: string, confirm: string): FormGroup {
    return new FormGroup(
      {
        password: new FormControl(pass),
        confirmPassword: new FormControl(confirm)
      },
      { validators: passwordMatchValidator('password', 'confirmPassword') }
    );
  }

  it('should return null when passwords match', () => {
    const group = buildGroup('Secret123', 'Secret123');
    expect(group.errors).toBeNull();
  });

  it('should return passwordMismatch error when passwords do not match', () => {
    const group = buildGroup('Secret123', 'Different');
    expect(group.errors).toEqual({ passwordMismatch: true });
  });

  it('should set confirmPassword control error when passwords do not match', () => {
    const group = buildGroup('abc', 'xyz');
    expect(group.get('confirmPassword')!.errors).toEqual({ passwordMismatch: true });
  });

  it('should clear confirmPassword control error when passwords match', () => {
    const group = buildGroup('abc', 'abc');
    expect(group.get('confirmPassword')!.errors).toBeNull();
  });

  it('should return null when control keys do not exist in group', () => {
    const validator = passwordMatchValidator('missing1', 'missing2');
    const group = new FormGroup({ a: new FormControl('') });
    expect(validator(group)).toBeNull();
  });
});
