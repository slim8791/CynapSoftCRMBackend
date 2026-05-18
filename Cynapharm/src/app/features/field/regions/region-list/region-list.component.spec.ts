import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { RegionListComponent } from './region-list.component';
import { RegionService } from '../services/region.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

const activatedRouteMock = { snapshot: { paramMap: { get: () => null } } };

describe('RegionListComponent', () => {
  let component: RegionListComponent;
  let svcMock: { getAll: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    svcMock = { getAll: vi.fn().mockReturnValue(of([])) };
    await TestBed.configureTestingModule({
      imports: [RegionListComponent],
      providers: [
        { provide: RegionService, useValue: svcMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(RegionListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load on init', () => {
    expect(component).toBeTruthy();
    expect(svcMock.getAll).toHaveBeenCalled();
  });

  it('load should populate regions on success', () => {
    const items = [{ id_Region: 1, nomRegion: 'Tunis', codePostal: '1000', id_User_Delegue: 1 }];
    svcMock.getAll.mockReturnValue(of(items));
    component.load();
    expect(component.regions).toEqual(items);
    expect(component.loading).toBe(false);
  });

  it('load should set error on failure', () => {
    svcMock.getAll.mockReturnValue(throwError(() => new Error()));
    component.load();
    expect(component.error).toBe('Impossible de charger les régions.');
  });
});
