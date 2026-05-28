import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private readonly endpoint = '/products';

  constructor(private apiService: ApiService) {}

  private unwrapResult<T>(response: any): T {
    if (response == null) return response;
    if (response.Result !== undefined) return response.Result;
    if (response.result !== undefined) return response.result;
    return response;
  }

  // ── CRUD de base ─────────────────────────────────────

  getProducts(): Observable<any[]> {
    return this.apiService
      .get<any>(this.endpoint)
      .pipe(map(r => this.unwrapResult<any[]>(r)));
  }

  /** GET /products/filter?page=1&pageSize=1000 — returns all products (active + inactive + archived) */
  getProductsAll(): Observable<any[]> {
    const params = new HttpParams()
      .set('page', '1')
      .set('pageSize', '1000');
    return this.apiService
      .get<any>(`${this.endpoint}/filter`, params)
      .pipe(
        map(r => this.unwrapResult<any[]>(r) ?? []),
        catchError(err => {
          if (err.status === 404) return of([]);
          throw err;
        })
      );
  }

  getCategories(): Observable<string[]> {
    return this.apiService
      .get<any>(`${this.endpoint}/categories`)
      .pipe(
        map(r => this.unwrapResult<string[]>(r) ?? []),
        catchError(() => of([]))
      );
  }

  getVisibleProducts(): Observable<any[]> {
    return this.apiService
      .get<any>(`${this.endpoint}/visible`)
      .pipe(
        map(r => this.unwrapResult<any[]>(r) ?? []),
        catchError(() => of([]))
      );
  }

  getProductById(id: string | number): Observable<any> {
    return this.apiService
      .get<any>(`${this.endpoint}/${id}`)
      .pipe(map(r => this.unwrapResult<any>(r)));
  }

  createProduct(data: any): Observable<any> {
    return this.apiService.post<any>(this.endpoint, data);
  }

  updateProduct(_id: string | number, data: any): Observable<any> {
    // ✅ Backend utilise POST pour créer ET mettre à jour (CreateOrUpdateProduct)
    return this.apiService.post<any>(this.endpoint, data);
  }

  // ── Actions métier ────────────────────────────────────

  deleteProduct(id: string): Observable<any> {
    return this.apiService.put<any>(`${this.endpoint}/${id}/deactivate`, {});
  }

  hardDeleteProduct(id: string): Observable<any> {
    return this.apiService.delete<any>(`${this.endpoint}/${id}`);
  }

  activateProduct(id: string): Observable<any> {
    return this.apiService.put<any>(`${this.endpoint}/${id}/activate`, {});
  }

  archiveProduct(id: string): Observable<any> {
    return this.apiService.put<any>(`${this.endpoint}/${id}/archive`, {});
  }

  unarchiveProduct(id: string): Observable<any> {
    return this.apiService.put<any>(`${this.endpoint}/${id}/unarchive`, {});
  }

  // ── Recherche — aligne sur SearchProductsAsync ────────
  // keyword >= 3 chars, filtré par isActive ET allowArchived
  searchProducts(
    keyword: string,
    isActive: boolean,
    allowArchived: boolean,
    limit = 10
  ): Observable<any[]> {
    const params = new HttpParams()
      .set('keyword', keyword)
      .set('isActive', String(isActive))
      .set('allowArchived', String(allowArchived))
      .set('limit', String(limit));

    return this.apiService
      .get<any>(`${this.endpoint}/search`, params)
      .pipe(map(r => this.unwrapResult<any[]>(r) ?? []));
  }

  getLotsByProduct(productId: number): Observable<any[]> {
    return this.apiService
      .get<any>(`${this.endpoint}/lots/product/${productId}`)
      .pipe(
        map(r => this.unwrapResult<any[]>(r) ?? []),
        catchError(() => of([]))
      );
  }

  // ── Filtre paginé — aligne sur FilterProductsAsync ────
  filterProducts(
    page: number,
    pageSize: number,
    keyword?: string,
    isActive?: boolean,
    allowArchived?: boolean,
    category?: string
  ): Observable<any[]> {
    let params = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));

    if (keyword)                  params = params.set('keyword',      keyword);
    if (isActive !== undefined)   params = params.set('isActive',     String(isActive));
    if (allowArchived !== undefined) params = params.set('allowArchived', String(allowArchived));
    if (category)                 params = params.set('category',     category);

    return this.apiService
      .get<any>(`${this.endpoint}/filter`, params)
      .pipe(map(r => this.unwrapResult<any[]>(r) ?? []));
  }
}