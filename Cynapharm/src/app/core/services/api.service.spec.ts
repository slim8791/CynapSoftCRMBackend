import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ApiService } from './api.service';
import { HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;
  const base = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ApiService]
    });
    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('get should send GET request to correct URL', () => {
    service.get('/test').subscribe();
    const req = httpMock.expectOne(`${base}/test`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('get should attach HttpParams when provided', () => {
    const params = new HttpParams().set('page', '1');
    service.get('/test', params).subscribe();
    const req = httpMock.expectOne(r => r.url === `${base}/test` && r.params.get('page') === '1');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('post should send POST request with body', () => {
    const body = { name: 'test' };
    service.post('/test', body).subscribe();
    const req = httpMock.expectOne(`${base}/test`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);
    req.flush({});
  });

  it('put should send PUT request with body', () => {
    const body = { id: 1 };
    service.put('/test/1', body).subscribe();
    const req = httpMock.expectOne(`${base}/test/1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(body);
    req.flush({});
  });

  it('delete should send DELETE request to correct URL', () => {
    service.delete('/test/1').subscribe();
    const req = httpMock.expectOne(`${base}/test/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('patch should send PATCH request with body', () => {
    const body = { field: 'value' };
    service.patch('/test/1', body).subscribe();
    const req = httpMock.expectOne(`${base}/test/1`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual(body);
    req.flush({});
  });

  it('unwrapResponse should return Result when IsSuccess is not false and Result exists', () => {
    const response = { IsSuccess: true, Result: [1, 2, 3] };
    expect(service.unwrapResponse(response)).toEqual([1, 2, 3]);
  });

  it('unwrapResponse should return raw response when Result is undefined', () => {
    const response = { data: 'raw' };
    expect(service.unwrapResponse(response)).toEqual(response);
  });

  it('unwrapResponse should return raw response when IsSuccess is false', () => {
    const response = { IsSuccess: false, Result: [] };
    expect(service.unwrapResponse(response)).toEqual(response);
  });
});
