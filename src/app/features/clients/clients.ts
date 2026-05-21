import { Component, DestroyRef, inject, signal, OnInit, ViewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { AgGridAngular } from 'ag-grid-angular';
import { ColDef, GridReadyEvent } from 'ag-grid-community';
import { interval, switchMap } from 'rxjs';
import { ClientService } from '../../services/client.service';
import { Client } from '../../models';
import { ClientActions } from './client-actions';
import { ClientFormDialog, ClientFormValue } from './client-form-dialog';

const CLIENT_POLL_MS = 5000;

const clientStatusClasses = {
  'client-active': (params: { value?: string }) => params.value === 'Active',
  'client-suspended': (params: { value?: string }) => params.value === 'Suspended',
  'client-archived': (params: { value?: string }) => params.value === 'Archived',
};

@Component({
  selector: 'app-clients',
  imports: [AgGridAngular, ClientFormDialog],
  template: `
    <div class="page-container">
      <div class="header">
        <h1>Clients</h1>
        <button type="button" class="primary" (click)="openCreate()">New Client</button>
      </div>
      <ag-grid-angular
        class="grid"
        [rowData]="clients()"
        [columnDefs]="columnDefs"
        [defaultColDef]="defaultColDef"
        (gridReady)="onGridReady($event)"
      />
    </div>
    @if (showCreate()) {
      <app-client-form-dialog
        #dialog
        (save)="onCreateSave($event)"
        (cancel)="closeCreate()"
      />
    }
  `,
  styles: [`
    .page-container { padding: 24px; }
    .header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .header h1 { margin: 0; }
    .grid { width: 100%; height: 600px; }
    ::ng-deep .client-active { color: #2e7d32; font-weight: 600; }
    ::ng-deep .client-suspended { color: #ed6c02; font-weight: 600; }
    ::ng-deep .client-archived { color: #757575; font-style: italic; }
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
export class Clients implements OnInit {
  private clientService = inject(ClientService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected readonly clients = signal<Client[]>([]);
  protected readonly showCreate = signal(false);

  @ViewChild('dialog') private dialog?: ClientFormDialog;

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true,
  };

  columnDefs: ColDef<Client>[] = [
    { field: 'name', headerName: 'Name', flex: 2 },
    { field: 'documentCount', headerName: 'Documents', flex: 1 },
    {
      field: 'processingStatus',
      headerName: 'Processing Status',
      flex: 1,
      cellClassRules: clientStatusClasses,
    },
    {
      headerName: 'Actions',
      cellRenderer: ClientActions,
      cellRendererParams: {
        onOpen: (client: Client) => this.openClient(client),
        onDelete: (client: Client) => this.deleteClient(client),
      },
      sortable: false,
      filter: false,
      flex: 1,
    },
  ];

  ngOnInit(): void {
    this.loadClients();

    interval(CLIENT_POLL_MS)
      .pipe(
        switchMap(() => this.clientService.getClients()),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (clients) => this.clients.set(clients),
        error: (err) => console.error('Polling clients failed', err),
      });
  }

  onGridReady(event: GridReadyEvent): void {
    event.api.sizeColumnsToFit();
  }

  openCreate(): void {
    this.showCreate.set(true);
  }

  closeCreate(): void {
    this.showCreate.set(false);
  }

  onCreateSave(value: ClientFormValue): void {
    this.dialog?.setSaving(true);
    this.clientService.createClient(value).subscribe({
      next: (created) => {
        this.clients.update((current) => [...current, created]);
        this.closeCreate();
      },
      error: (err) => {
        console.error('Failed to create client', err);
        this.dialog?.setSaving(false);
        this.dialog?.showError('Could not create client. Check the API logs.');
      },
    });
  }

  private loadClients(): void {
    this.clientService.getClients().subscribe({
      next: (clients) => this.clients.set(clients),
      error: (err) => console.error('Failed to load clients', err),
    });
  }

  private openClient(client: Client): void {
    this.router.navigate(['/clients', client.externalId]);
  }

  private deleteClient(client: Client): void {
    if (!confirm(`Are you sure you want to delete client "${client.name}"?`)) {
      return;
    }
    this.clientService.deleteClient(client.externalId).subscribe({
      next: () => this.loadClients(),
      error: (err) => console.error('Failed to delete client', err),
    });
  }
}
