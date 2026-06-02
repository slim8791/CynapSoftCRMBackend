import { describe, it, expect } from 'vitest';
import { FormControl } from '@angular/forms';
import { noWhitespaceValidator } from './no-whitespace.validator';

describe('noWhitespaceValidator', () => {
  it('should return null for a non-empty, non-whitespace value', () => {
    const ctrl = new FormControl('hello');
    expect(noWhitespaceValidator(ctrl)).toBeNull();
  });

  it('should return whitespace error for a value containing only spaces', () => {
    const ctrl = new FormControl('   ');
    expect(noWhitespaceValidator(ctrl)).toEqual({ whitespace: true });
  });

  it('should return whitespace error for an empty string', () => {
    const ctrl = new FormControl('');
    expect(noWhitespaceValidator(ctrl)).toEqual({ whitespace: true });
  });

  it('should return null for a value with leading/trailing spaces but non-empty content', () => {
    const ctrl = new FormControl('  hello  ');
    expect(noWhitespaceValidator(ctrl)).toBeNull();
  });

  it('should return whitespace error for a null value', () => {
    const ctrl = new FormControl(null);
    expect(noWhitespaceValidator(ctrl)).toEqual({ whitespace: true });
  });
});
