import { vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { DocumentListComponent } from './document-list.component';
import { DocumentService } from '../services/document.service';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

const activatedRouteMock = { snapshot: { paramMap: { get: () => null } } };

describe('DocumentListComponent', () => {
  let component: DocumentListComponent;
  let svcMock: { getAll: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    svcMock = { getAll: vi.fn().mockReturnValue(of([])) };
    await TestBed.configureTestingModule({
      imports: [DocumentListComponent],
      providers: [
        { provide: DocumentService, useValue: svcMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
    const fixture = TestBed.createComponent(DocumentListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load on init', () => {
    expect(component).toBeTruthy();
    expect(svcMock.getAll).toHaveBeenCalled();
  });

  it('load should populate docs on success', () => {
    const items = [{ type: 'Facture', id_Client: 1 }];
    svcMock.getAll.mockReturnValue(of(items));
    component.load();
    expect(component.docs).toEqual(items);
    expect(component.total).toBe(1);
    expect(component.loading).toBe(false);
  });

  it('load should set error on failure', () => {
    svcMock.getAll.mockReturnValue(throwError(() => new Error()));
    component.load();
    expect(component.error).toBe('Erreur chargement.');
  });

  it('onPage should update page and call load', () => {
    svcMock.getAll.mockReturnValue(of([]));
    component.onPage(5);
    expect(component.page).toBe(5);
  });
});
