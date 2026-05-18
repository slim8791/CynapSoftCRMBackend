import { describe, it, expect } from 'vitest';
import { FormControl, FormGroup } from '@angular/forms';
import { dateRangeValidator } from './date-range.validator';

describe('dateRangeValidator', () => {
  function buildGroup(start: string, end: string): FormGroup {
    return new FormGroup(
      {
        startDate: new FormControl(start),
        endDate: new FormControl(end)
      },
      { validators: dateRangeValidator('startDate', 'endDate') }
    );
  }

  it('should return null when startDate is before endDate', () => {
    const group = buildGroup('2024-01-01', '2024-12-31');
    expect(group.errors).toBeNull();
  });

  it('should return dateRange error when startDate is after endDate', () => {
    const group = buildGroup('2024-12-31', '2024-01-01');
    expect(group.errors).toEqual({ dateRange: true });
  });

  it('should return null when startDate equals endDate', () => {
    const group = buildGroup('2024-06-15', '2024-06-15');
    expect(group.errors).toBeNull();
  });

  it('should return null when startDate is empty', () => {
    const group = buildGroup('', '2024-12-31');
    expect(group.errors).toBeNull();
  });

  it('should return null when endDate is empty', () => {
    const group = buildGroup('2024-01-01', '');
    expect(group.errors).toBeNull();
  });

  it('should return null when control keys do not exist in group', () => {
    const validator = dateRangeValidator('nonExistent', 'alsoMissing');
    const group = new FormGroup({ a: new FormControl('') });
    expect(validator(group)).toBeNull();
  });
});
