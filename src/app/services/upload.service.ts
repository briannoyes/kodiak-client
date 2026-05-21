import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import {
  DirectContractUploadFinalizeRequest,
  DirectContractUploadInitiated,
  DirectContractUploadRequest,
  HeaderGroup,
  HeaderMappingApproval,
  PaymentRecordUploadAccepted,
  Upload,
  UploadAccepted,
} from '../models';
import { environment } from '../../environments/environment';

export type UploadProgressCallback = (uploaded: number, total: number) => void;

@Injectable({ providedIn: 'root' })
export class UploadService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiBaseUrl;

  getUploadsForClient(clientExternalId: string): Observable<Upload[]> {
    return this.http.get<Upload[]>(
      `${this.baseUrl}/clients/${clientExternalId}/uploads`
    );
  }

  getUploadProgress(uploadExternalId: string): Observable<Upload> {
    return this.http.get<Upload>(
      `${this.baseUrl}/uploads/${uploadExternalId}`
    );
  }

  deleteUpload(uploadExternalId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/uploads/${uploadExternalId}`
    );
  }

  createPaymentRecordUpload(
    clientExternalId: string,
    files: File[]
  ): Observable<PaymentRecordUploadAccepted> {
    const formData = new FormData();
    formData.append('clientExternalId', clientExternalId);
    for (const file of files) {
      formData.append('files', file, file.name);
    }
    return this.http.post<PaymentRecordUploadAccepted>(
      `${this.baseUrl}/uploads/payment-records`,
      formData
    );
  }

  getHeaderGroups(uploadExternalId: string): Observable<HeaderGroup[]> {
    return this.http.get<HeaderGroup[]>(
      `${this.baseUrl}/uploads/${uploadExternalId}/header-groups`
    );
  }

  approveMapping(
    uploadExternalId: string,
    fingerprint: string,
    approval: HeaderMappingApproval
  ): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/uploads/${uploadExternalId}/header-groups/${fingerprint}/mapping`,
      approval
    );
  }

  // Three-phase contract upload (see KodiakMultiSelectContractUploadSequence.puml):
  // 1) POST /direct/begin to get per-file SAS PUT slots
  // 2) PUT each file directly to its SAS URL
  // 3) POST /direct/{id}/finalize with the uploaded DocumentExternalIds
  createDirectUpload(
    clientExternalId: string,
    files: File[],
    onProgress: UploadProgressCallback
  ): Observable<UploadAccepted> {
    return new Observable<UploadAccepted>((subscriber) => {
      let cancelled = false;
      (async () => {
        try {
          const beginRequest: DirectContractUploadRequest = {
            clientExternalId,
            files: files.map((f) => ({
              fileName: f.name,
              contentType: f.type || null,
              sizeBytes: f.size,
            })),
          };
          const initiated = await firstValueFrom(
            this.http.post<DirectContractUploadInitiated>(
              `${this.baseUrl}/uploads/direct/begin`,
              beginRequest
            )
          );
          if (cancelled) return;

          const total = initiated.files.length;
          onProgress(0, total);

          const uploadedDocumentIds: string[] = [];
          for (let i = 0; i < total; i++) {
            const slot = initiated.files[i];
            const file = files[i];
            await firstValueFrom(
              this.http.put(slot.sasPutUrl, file, {
                headers: new HttpHeaders({
                  'x-ms-blob-type': 'BlockBlob',
                  'Content-Type': file.type || 'application/octet-stream',
                }),
              })
            );
            if (cancelled) return;
            uploadedDocumentIds.push(slot.documentExternalId);
            onProgress(i + 1, total);
          }

          const finalizeRequest: DirectContractUploadFinalizeRequest = {
            documentExternalIds: uploadedDocumentIds,
          };
          const accepted = await firstValueFrom(
            this.http.post<UploadAccepted>(
              `${this.baseUrl}/uploads/direct/${initiated.uploadExternalId}/finalize`,
              finalizeRequest
            )
          );
          if (cancelled) return;
          subscriber.next(accepted);
          subscriber.complete();
        } catch (err) {
          if (!cancelled) subscriber.error(err);
        }
      })();
      return () => {
        cancelled = true;
      };
    });
  }
}
