import { Component, DestroyRef, inject, signal, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef, GridReadyEvent, ValueFormatterParams } from 'ag-grid-community';
import { interval, switchMap } from 'rxjs';
import { DocumentService } from '../../services/document.service';
import { Document } from '../../models';
import { DocumentActions } from './document-actions';

const DOCUMENT_POLL_MS = 2000;

const documentStatusClasses = {
  'doc-pending': (params: { value?: string }) => params.value === 'Pending',
  'doc-processing': (params: { value?: string }) => params.value === 'Processing',
  'doc-awaitingmapping': (params: { value?: string }) => params.value === 'AwaitingMapping',
  'doc-completed': (params: { value?: string }) => params.value === 'Completed',
  'doc-dedupskipped': (params: { value?: string }) => params.value === 'DedupSkipped',
  'doc-failed': (params: { value?: string }) => params.value === 'Failed',
};

@Component({
  selector: 'app-documents',
  imports: [AgGridAngular],
  template: `
    <div class="page-container">
      <a href="javascript:void(0)" (click)="goBack()" class="back-link">&larr; Back to Uploads</a>
      <h1>Documents</h1>
      <ag-grid-angular
        class="grid"
        [rowData]="documents()"
        [columnDefs]="columnDefs"
        [defaultColDef]="defaultColDef"
        (gridReady)="onGridReady($event)"
      />
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; }
    .grid { width: 100%; height: 600px; }
    .back-link { color: #1976d2; text-decoration: none; cursor: pointer; }
    .back-link:hover { text-decoration: underline; }
    ::ng-deep .doc-pending { color: #757575; font-weight: 500; }
    ::ng-deep .doc-processing { color: #1976d2; font-weight: 600; }
    ::ng-deep .doc-awaitingmapping { color: #ed6c02; font-weight: 600; }
    ::ng-deep .doc-completed { color: #2e7d32; font-weight: 600; }
    ::ng-deep .doc-dedupskipped { color: #9e9e9e; font-style: italic; }
    ::ng-deep .doc-failed { color: #d32f2f; font-weight: 600; }
  `],
})
export class Documents implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private documentService = inject(DocumentService);
  private destroyRef = inject(DestroyRef);

  protected readonly documents = signal<Document[]>([]);
  private clientExternalId = '';
  private uploadExternalId = '';

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true,
  };

  private dateFormatter = (params: ValueFormatterParams) => {
    if (!params.value) return '';
    return new Date(params.value).toLocaleString();
  };

  private fileSizeFormatter = (params: ValueFormatterParams) => {
    if (params.value == null) return '';
    const kb = params.value / 1024;
    if (kb < 1024) return `${kb.toFixed(1)} KB`;
    return `${(kb / 1024).toFixed(1)} MB`;
  };

  columnDefs: ColDef<Document>[] = [
    { field: 'originalFileName', headerName: 'File Name', flex: 2 },
    { field: 'documentType', headerName: 'Type', flex: 1 },
    { field: 'contentType', headerName: 'Content Type', flex: 1 },
    {
      field: 'sizeBytes',
      headerName: 'Size',
      flex: 1,
      valueFormatter: this.fileSizeFormatter,
    },
    {
      field: 'status',
      headerName: 'Status',
      flex: 1,
      cellClassRules: documentStatusClasses,
    },
    {
      field: 'uploadedAt',
      headerName: 'Uploaded',
      flex: 1,
      valueFormatter: this.dateFormatter,
    },
    {
      field: 'processedAt',
      headerName: 'Processed',
      flex: 1,
      valueFormatter: this.dateFormatter,
    },
    { field: 'errorMessage', headerName: 'Error', flex: 1 },
    {
      headerName: 'Actions',
      cellRenderer: DocumentActions,
      cellRendererParams: {
        onDelete: (document: Document) => this.deleteDocument(document),
      },
      sortable: false,
      filter: false,
      flex: 1,
    },
  ];

  ngOnInit(): void {
    this.clientExternalId = this.route.snapshot.paramMap.get('clientExternalId')!;
    this.uploadExternalId = this.route.snapshot.paramMap.get('uploadExternalId')!;
    this.reloadDocuments();

    interval(DOCUMENT_POLL_MS)
      .pipe(
        switchMap(() =>
          this.documentService.getDocumentsForUpload(this.uploadExternalId)
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (docs) => this.documents.set(docs),
        error: (err) => console.error('Polling documents failed', err),
      });
  }

  onGridReady(event: GridReadyEvent): void {
    event.api.sizeColumnsToFit();
  }

  goBack(): void {
    this.router.navigate(['/clients', this.clientExternalId]);
  }

  private deleteDocument(document: Document): void {
    if (!confirm(`Delete "${document.originalFileName ?? 'this document'}"?`)) {
      return;
    }
    this.documentService
      .deleteDocument(this.uploadExternalId, document.externalId)
      .subscribe({
        next: () => this.reloadDocuments(),
        error: (err) => console.error('Failed to delete document', err),
      });
  }

  private reloadDocuments(): void {
    this.documentService.getDocumentsForUpload(this.uploadExternalId).subscribe({
      next: (docs) => this.documents.set(docs),
      error: (err) => console.error('Failed to load documents', err),
    });
  }
}
