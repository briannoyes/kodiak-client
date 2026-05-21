import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Client } from '../models';
import { environment } from '../../environments/environment';

export interface CreateClientRequest {
  name: string;
  billingEmail: string | null;
}

@Injectable({ providedIn: 'root' })
export class ClientService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/clients`;

  getClients(): Observable<Client[]> {
    return this.http.get<Client[]>(this.baseUrl);
  }

  getClient(externalId: string): Observable<Client> {
    return this.http.get<Client>(`${this.baseUrl}/${externalId}`);
  }

  createClient(request: CreateClientRequest): Observable<Client> {
    return this.http.post<Client>(this.baseUrl, request);
  }

  deleteClient(externalId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${externalId}`);
  }
}
