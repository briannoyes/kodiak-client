import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UploadService } from '../../services/upload.service';
import { HeaderGroup, HeaderMappingApproval } from '../../models';

const PAYMENT_FIELDS = [
  'VendorID',
  'VendorName',
  'Company',
  'InvoiceDate',
  'CheckDate',
  'CheckNumber',
  'InvoiceAmount',
  'CheckAmount',
  'VoidDate',
  'CheckStatus',
  'PhysicianVendor',
] as const;

type GroupMapping = Record<string, string>;

@Component({
  selector: 'app-payment-mapping-dialog',
  imports: [FormsModule],
  template: `
    <div class="overlay" (click)="onBackdropClick($event)">
      <div class="dialog" role="dialog" aria-modal="true" aria-labelledby="map-title">
        <h2 id="map-title">Map Payment Headers</h2>
        @if (loading()) {
          <p>Loading header groups…</p>
        } @else if (groups().length === 0) {
          <p>No documents in this upload require mapping.</p>
        } @else {
          <p class="hint">
            {{ groups().length }}
            {{ groups().length === 1 ? 'header group needs' : 'header groups need' }}
            mapping. Map each canonical payment field to a source column.
          </p>
          @for (group of groups(); track group.fingerprint) {
            <div class="group">
              <h3>
                {{ group.documentExternalIds.length }}
                {{ group.documentExternalIds.length === 1 ? 'document' : 'documents' }}
                — columns:
                <span class="cols">{{ group.headers.join(', ') }}</span>
              </h3>
              <table class="mapping">
                <thead>
                  <tr><th>Payment Field</th><th>Source Column</th></tr>
                </thead>
                <tbody>
                  @for (field of paymentFields; track field) {
                    <tr>
                      <td>{{ field }}</td>
                      <td>
                        <select
                          [(ngModel)]="mappings[group.fingerprint][field]"
                          [disabled]="savingFingerprint() === group.fingerprint"
                        >
                          <option value="">(not mapped)</option>
                          @for (header of group.headers; track header) {
                            <option [value]="header">{{ header }}</option>
                          }
                        </select>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
              <div class="group-actions">
                <button
                  type="button"
                  class="primary"
                  (click)="onSaveGroup(group)"
                  [disabled]="savingFingerprint() !== null"
                >
                  {{ savingFingerprint() === group.fingerprint ? 'Saving…' : 'Save Mapping' }}
                </button>
              </div>
            </div>
          }
        }
        @if (errorMessage()) {
          <p class="error" role="alert">{{ errorMessage() }}</p>
        }
        <div class="actions">
          <button type="button" class="secondary" (click)="onClose()" [disabled]="savingFingerprint() !== null">
            Close
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.45);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }
    .dialog {
      background: #fff;
      border-radius: 8px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
      min-width: 600px;
      max-width: 90vw;
      max-height: 90vh;
      overflow-y: auto;
      padding: 24px;
    }
    h2 { margin: 0 0 16px; font-size: 1.25rem; }
    .hint { color: #555; font-size: 0.875rem; margin: 0 0 16px; }
    .group {
      border: 1px solid #e0e0e0;
      border-radius: 6px;
      padding: 16px;
      margin-bottom: 16px;
    }
    .group h3 { margin: 0 0 12px; font-size: 1rem; font-weight: 500; }
    .group .cols { color: #1976d2; font-family: monospace; font-size: 0.9em; }
    table.mapping { width: 100%; border-collapse: collapse; }
    table.mapping th, table.mapping td { text-align: left; padding: 6px 8px; }
    table.mapping th { background: #f5f5f5; font-weight: 500; font-size: 0.875rem; }
    table.mapping select { width: 100%; padding: 4px; font: inherit; }
    .group-actions { display: flex; justify-content: flex-end; margin-top: 12px; }
    .error { color: #d32f2f; margin: 8px 0 0; font-size: 0.875rem; }
    .actions { display: flex; justify-content: flex-end; margin-top: 20px; }
    button {
      padding: 8px 16px;
      border-radius: 4px;
      border: 1px solid transparent;
      cursor: pointer;
      font: inherit;
    }
    button:disabled { cursor: not-allowed; opacity: 0.6; }
    .primary { background: #1976d2; color: #fff; }
    .primary:hover:not(:disabled) { background: #1565c0; }
    .secondary { background: #fff; border-color: #ccc; color: #333; }
    .secondary:hover:not(:disabled) { background: #f5f5f5; }
  `],
})
export class PaymentMappingDialog implements OnInit {
  @Input({ required: true }) uploadExternalId!: string;
  @Output() close = new EventEmitter<void>();

  private uploadService = inject(UploadService);

  protected readonly paymentFields = PAYMENT_FIELDS;
  protected readonly groups = signal<HeaderGroup[]>([]);
  protected readonly loading = signal(true);
  protected readonly savingFingerprint = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected mappings: Record<string, GroupMapping> = {};

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.uploadService.getHeaderGroups(this.uploadExternalId).subscribe({
      next: (groups) => {
        this.groups.set(groups);
        for (const g of groups) {
          if (!this.mappings[g.fingerprint]) {
            this.mappings[g.fingerprint] = Object.fromEntries(
              PAYMENT_FIELDS.map((f) => [f, ''])
            );
          }
        }
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load header groups', err);
        this.errorMessage.set('Could not load header groups.');
        this.loading.set(false);
      },
    });
  }

  protected onSaveGroup(group: HeaderGroup): void {
    const raw = this.mappings[group.fingerprint] ?? {};
    const filtered: Record<string, string> = {};
    for (const [field, column] of Object.entries(raw)) {
      if (column) filtered[field] = column;
    }
    if (Object.keys(filtered).length === 0) {
      this.errorMessage.set('Map at least one field before saving.');
      return;
    }

    this.errorMessage.set(null);
    this.savingFingerprint.set(group.fingerprint);
    const approval: HeaderMappingApproval = { mapping: filtered };
    this.uploadService
      .approveMapping(this.uploadExternalId, group.fingerprint, approval)
      .subscribe({
        next: () => {
          this.savingFingerprint.set(null);
          this.groups.update((current) =>
            current.filter((g) => g.fingerprint !== group.fingerprint)
          );
          delete this.mappings[group.fingerprint];
        },
        error: (err) => {
          console.error('Failed to save mapping', err);
          this.errorMessage.set('Could not save mapping. Check the API logs.');
          this.savingFingerprint.set(null);
        },
      });
  }

  protected onClose(): void {
    if (this.savingFingerprint() !== null) return;
    this.close.emit();
  }

  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }
}
