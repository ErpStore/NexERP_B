/**
 * LEARNING — Feature services
 * Injectable services are Angular's equivalent of C# DI services.
 * Keep HTTP calls here; keep UI state (signals) in components.
 */
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CurrencySearchParams,
  CurrencyVm,
  PagedCurrencyResult
} from './models/currency.model';

@Injectable({ providedIn: 'root' })
export class CurrencyService {
  private readonly baseUrl = `${environment.apiBaseUrl}/api/currencies`;

  constructor(private readonly http: HttpClient) {}

  search(params: CurrencySearchParams): Observable<PagedCurrencyResult> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber)
      .set('pageSize', params.pageSize);

    if (params.currName) httpParams = httpParams.set('currName', params.currName);
    if (params.createdBy) httpParams = httpParams.set('createdBy', params.createdBy);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);

    return this.http.get<PagedCurrencyResult>(this.baseUrl, { params: httpParams });
  }

  getById(id: number): Observable<CurrencyVm> {
    return this.http.get<CurrencyVm>(`${this.baseUrl}/${id}`);
  }

  create(vm: CurrencyVm): Observable<CurrencyVm> {
    return this.http.post<CurrencyVm>(this.baseUrl, vm);
  }

  update(id: number, vm: CurrencyVm): Observable<CurrencyVm> {
    return this.http.put<CurrencyVm>(`${this.baseUrl}/${id}`, vm);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
