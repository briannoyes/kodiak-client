import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Document } from '../models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DocumentService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiBaseUrl;

  getDocumentsForUpload(uploadExternalId: string): Observable<Document[]> {
    return this.http.get<Document[]>(
      `${this.baseUrl}/uploads/${uploadExternalId}/documents`
    );
  }

  deleteDocument(
    uploadExternalId: string,
    documentExternalId: string
  ): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/uploads/${uploadExternalId}/documents/${documentExternalId}`
    );
  }
}
