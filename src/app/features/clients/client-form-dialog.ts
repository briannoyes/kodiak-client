import { Component, EventEmitter, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

export interface ClientFormValue {
  name: string;
  billingEmail: string | null;
}

@Component({
  selector: 'app-client-form-dialog',
  imports: [FormsModule],
  template: `
    <div class="overlay" (click)="onBackdropClick($event)">
      <div class="dialog" role="dialog" aria-modal="true" aria-labelledby="client-form-title">
        <h2 id="client-form-title">New Client</h2>
        <form #form="ngForm" (ngSubmit)="onSubmit()" novalidate>
          <label class="field">
            <span>Name</span>
            <input
              type="text"
              name="name"
              required
              maxlength="200"
              [(ngModel)]="name"
              #nameInput
              autofocus
            />
          </label>
          <label class="field">
            <span>Billing email <em>(optional)</em></span>
            <input
              type="email"
              name="billingEmail"
              maxlength="320"
              [(ngModel)]="billingEmail"
            />
          </label>
          @if (errorMessage()) {
            <p class="error" role="alert">{{ errorMessage() }}</p>
          }
          <div class="actions">
            <button type="button" class="secondary" (click)="onCancel()" [disabled]="saving()">
              Cancel
            </button>
            <button type="submit" class="primary" [disabled]="saving() || !name.trim()">
              {{ saving() ? 'Saving…' : 'Create' }}
            </button>
          </div>
        </form>
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
      min-width: 420px;
      max-width: 90vw;
      padding: 24px;
    }
    h2 { margin: 0 0 16px; font-size: 1.25rem; }
    .field { display: flex; flex-direction: column; gap: 4px; margin-bottom: 12px; }
    .field span { font-size: 0.875rem; color: #555; }
    .field em { color: #888; font-style: normal; font-size: 0.8rem; }
    .field input {
      padding: 8px 10px;
      border: 1px solid #ccc;
      border-radius: 4px;
      font: inherit;
    }
    .field input:focus { outline: 2px solid #1976d2; outline-offset: -1px; border-color: #1976d2; }
    .error { color: #d32f2f; margin: 8px 0 0; font-size: 0.875rem; }
    .actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 20px; }
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
export class ClientFormDialog {
  @Output() save = new EventEmitter<ClientFormValue>();
  @Output() cancel = new EventEmitter<void>();

  protected name = '';
  protected billingEmail = '';
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  setSaving(value: boolean): void {
    this.saving.set(value);
  }

  showError(message: string): void {
    this.errorMessage.set(message);
  }

  protected onSubmit(): void {
    const trimmedName = this.name.trim();
    if (!trimmedName) {
      this.errorMessage.set('Name is required.');
      return;
    }
    this.errorMessage.set(null);
    this.save.emit({
      name: trimmedName,
      billingEmail: this.billingEmail.trim() || null,
    });
  }

  protected onCancel(): void {
    if (this.saving()) return;
    this.cancel.emit();
  }

  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.onCancel();
    }
  }
}
