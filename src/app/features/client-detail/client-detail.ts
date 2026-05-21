import { Component, DestroyRef, inject, signal, OnInit, ViewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef, GridReadyEvent, ValueFormatterParams } from 'ag-grid-community';
import { interval, switchMap } from 'rxjs';
import { ClientService } from '../../services/client.service';
import { UploadService } from '../../services/upload.service';
import { Client, Upload } from '../../models';
import { UploadActions } from './upload-actions';
import { UploadFormDialog } from './upload-form-dialog';
import { PaymentMappingDialog } from './payment-mapping-dialog';
import { UploadProgressBar } from './upload-progress-bar';

const UPLOAD_POLL_MS = 2000;

const uploadStatusClasses = {
  'status-pending': (params: { value?: string }) => params.value === 'Pending',
  'status-processing': (params: { value?: string }) => params.value === 'Processing',
  'status-completed': (params: { value?: string }) => params.value === 'Completed',
  'status-partial': (params: { value?: string }) => params.value === 'Partial',
  'status-failed': (params: { value?: string }) => params.value === 'Failed',
};

type DialogMode = 'none' | 'contract' | 'payment';

@Component({
  selector: 'app-client-detail',
  imports: [AgGridAngular, UploadFormDialog, PaymentMappingDialog],
  template: `
    <div class="page-container">
      <div class="header">
        <a href="javascript:void(0)" (click)="goBack()" class="back-link">&larr; Back to Clients</a>
        <h1>{{ client()?.name }}</h1>
      </div>
      <div class="uploads-header">
        <h2>Uploads</h2>
        <div class="actions-row">
          <button type="button" class="primary" (click)="openContract()">New Contract Upload</button>
          <button type="button" class="primary" (click)="openPayment()">New Payment Records</button>
        </div>
      </div>
      <ag-grid-angular
        class="grid"
        [rowData]="uploads()"
        [columnDefs]="columnDefs"
        [defaultColDef]="defaultColDef"
        (gridReady)="onGridReady($event)"
      />
    </div>
    @if (dialogMode() === 'contract') {
      <app-upload-form-dialog
        #contractDialog
        title="New Contract Upload"
        fileLabel="Contract files"
        (save)="onContractSave($event)"
        (cancel)="closeDialog()"
      />
    }
    @if (dialogMode() === 'payment') {
      <app-upload-form-dialog
        #paymentDialog
        title="New Payment Records"
        fileLabel="Excel payment files"
        accept=".xlsx,.xls"
        (save)="onPaymentSave($event)"
        (cancel)="closeDialog()"
      />
    }
    @if (mappingUploadId(); as uploadId) {
      <app-payment-mapping-dialog
        [uploadExternalId]="uploadId"
        (close)="onMappingClosed()"
      />
    }
  `,
  styles: [`
    .page-container { padding: 24px; }
    .grid { width: 100%; height: 500px; }
    ::ng-deep .status-pending { color: #757575; font-weight: 500; }
    ::ng-deep .status-processing { color: #1976d2; font-weight: 600; }
    ::ng-deep .status-completed { color: #2e7d32; font-weight: 600; }
    ::ng-deep .status-partial { color: #ed6c02; font-weight: 600; }
    ::ng-deep .status-failed { color: #d32f2f; font-weight: 600; }
    .header { margin-bottom: 16px; }
    .back-link { color: #1976d2; text-decoration: none; cursor: pointer; }
    .back-link:hover { text-decoration: underline; }
    h1 { margin-top: 8px; }
    .uploads-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
    .uploads-header h2 { margin: 0; }
    .actions-row { display: flex; gap: 8px; }
    button.primary {
      background: #1976d2;
      color: #fff;
      border: 1px solid transparent;
      border-radius: 4px;
      padding: 8px 16px;
      font: inherit;
      cursor: pointer;
    }
    button.primary:hover { background: #1565c0; }
  `],
})
export class ClientDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private clientService = inject(ClientService);
  private uploadService = inject(UploadService);
  private destroyRef = inject(DestroyRef);

  protected readonly client = signal<Client | null>(null);
  protected readonly uploads = signal<Upload[]>([]);
  protected readonly dialogMode = signal<DialogMode>('none');
  protected readonly mappingUploadId = signal<string | null>(null);
  private clientExternalId = '';

  @ViewChild('contractDialog') private contractDialog?: UploadFormDialog;
  @ViewChild('paymentDialog') private paymentDialog?: UploadFormDialog;

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true,
  };

  private dateFormatter = (params: ValueFormatterParams) => {
    if (!params.value) return '';
    return new Date(params.value).toLocaleString();
  };

  columnDefs: ColDef<Upload>[] = [
    {
      field: 'createdAt',
      headerName: 'Created',
      flex: 1,
      valueFormatter: this.dateFormatter,
    },
    {
      field: 'completedAt',
      headerName: 'Completed',
      flex: 1,
      valueFormatter: this.dateFormatter,
    },
    {
      field: 'status',
      headerName: 'Status',
      flex: 1,
      cellClassRules: uploadStatusClasses,
    },
    {
      headerName: 'Progress',
      cellRenderer: UploadProgressBar,
      sortable: false,
      filter: false,
      flex: 2,
    },
    { field: 'awaitingMappingCount', headerName: 'Needs Mapping', flex: 1 },
    {
      headerName: 'Actions',
      cellRenderer: UploadActions,
      cellRendererParams: {
        onDocuments: (upload: Upload) => this.viewDocuments(upload),
        onDelete: (upload: Upload) => this.deleteUpload(upload),
        onMap: (upload: Upload) => this.openMapping(upload),
      },
      sortable: false,
      filter: false,
      flex: 1.5,
    },
  ];

  ngOnInit(): void {
    this.clientExternalId = this.route.snapshot.paramMap.get('clientExternalId')!;
    this.clientService.getClient(this.clientExternalId).subscribe({
      next: (client) => this.client.set(client),
      error: (err) => console.error('Failed to load client', err),
    });
    this.reloadUploads();

    interval(UPLOAD_POLL_MS)
      .pipe(
        switchMap(() =>
          this.uploadService.getUploadsForClient(this.clientExternalId)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (uploads) => this.uploads.set(uploads),
        error: (err) => console.error('Polling uploads failed', err),
      });
  }

  onGridReady(event: GridReadyEvent): void {
    event.api.sizeColumnsToFit();
  }

  goBack(): void {
    this.router.navigate(['/clients']);
  }

  openContract(): void {
    this.dialogMode.set('contract');
  }

  openPayment(): void {
    this.dialogMode.set('payment');
  }

  closeDialog(): void {
    this.dialogMode.set('none');
  }

  onContractSave(files: File[]): void {
    this.contractDialog?.setSaving(true);
    this.contractDialog?.setProgress(0, files.length);
    this.uploadService
      .createDirectUpload(this.clientExternalId, files, (uploaded, total) => {
        this.contractDialog?.setProgress(uploaded, total);
      })
      .subscribe({
        next: () => {
          this.closeDialog();
          this.reloadUploads();
        },
        error: (err) => {
          console.error('Failed to create upload', err);
          this.contractDialog?.setSaving(false);
          this.contractDialog?.showError('Could not create upload. Check the API logs.');
        },
      });
  }

  onPaymentSave(files: File[]): void {
    this.paymentDialog?.setSaving(true);
    this.paymentDialog?.setProgress(0, files.length);
    this.uploadService
      .createPaymentRecordUpload(this.clientExternalId, files)
      .subscribe({
        next: (accepted) => {
          this.closeDialog();
          this.reloadUploads();
          if (accepted.files.some((f) => f.requiresMapping)) {
            this.mappingUploadId.set(accepted.uploadExternalId);
          }
        },
        error: (err) => {
          console.error('Failed to create payment-record upload', err);
          this.paymentDialog?.setSaving(false);
          this.paymentDialog?.showError(
            'Could not create upload. Check the API logs.'
          );
        },
      });
  }

  onMappingClosed(): void {
    this.mappingUploadId.set(null);
    this.reloadUploads();
  }

  private reloadUploads(): void {
    this.uploadService.getUploadsForClient(this.clientExternalId).subscribe({
      next: (uploads) => this.uploads.set(uploads),
      error: (err) => console.error('Failed to load uploads', err),
    });
  }

  private viewDocuments(upload: Upload): void {
    this.router.navigate([
      '/clients',
      this.clientExternalId,
      'uploads',
      upload.externalId,
      'documents',
    ]);
  }

  private deleteUpload(upload: Upload): void {
    if (!confirm(`Delete this upload and all of its documents?`)) {
      return;
    }
    this.uploadService.deleteUpload(upload.externalId).subscribe({
      next: () => this.reloadUploads(),
      error: (err) => console.error('Failed to delete upload', err),
    });
  }

  private openMapping(upload: Upload): void {
    this.mappingUploadId.set(upload.externalId);
  }
}
