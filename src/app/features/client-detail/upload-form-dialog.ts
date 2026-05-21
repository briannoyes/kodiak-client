import { Component, EventEmitter, Input, Output, computed, signal } from '@angular/core';

@Component({
  selector: 'app-upload-form-dialog',
  template: `
    <div class="overlay" (click)="onBackdropClick($event)">
      <div class="dialog" role="dialog" aria-modal="true" aria-labelledby="upload-form-title">
        <h2 id="upload-form-title">{{ title }}</h2>
        <label class="field">
          <span>{{ fileLabel }}</span>
          <input
            type="file"
            multiple
            [accept]="accept"
            (change)="onFilesSelected($event)"
            [disabled]="saving()"
          />
        </label>
        @if (files().length > 0) {
          <ul class="file-list">
            @for (f of files(); track f.name) {
              <li>{{ f.name }} <em>({{ formatSize(f.size) }})</em></li>
            }
          </ul>
        }
        @if (errorMessage()) {
          <p class="error" role="alert">{{ errorMessage() }}</p>
        }
        <div class="actions">
          <button type="button" class="secondary" (click)="onCancel()" [disabled]="saving()">
            Cancel
          </button>
          <button
            type="button"
            class="primary"
            (click)="onSubmit()"
            [disabled]="saving() || files().length === 0"
          >
            {{ buttonLabel() }}
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
      min-width: 420px;
      max-width: 90vw;
      padding: 24px;
    }
    h2 { margin: 0 0 16px; font-size: 1.25rem; }
    .field { display: flex; flex-direction: column; gap: 4px; margin-bottom: 12px; }
    .field span { font-size: 0.875rem; color: #555; }
    .field input { font: inherit; }
    .file-list {
      list-style: none;
      padding: 0;
      margin: 0 0 12px;
      max-height: 160px;
      overflow-y: auto;
      font-size: 0.875rem;
    }
    .file-list li { padding: 2px 0; }
    .file-list em { color: #888; font-style: normal; }
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
export class UploadFormDialog {
  @Input() title = 'New Upload';
  @Input() fileLabel = 'Files';
  @Input() accept = '';
  @Output() save = new EventEmitter<File[]>();
  @Output() cancel = new EventEmitter<void>();

  protected readonly files = signal<File[]>([]);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly uploadedCount = signal(0);
  protected readonly totalCount = signal(0);

  protected readonly buttonLabel = computed(() => {
    if (!this.saving()) return 'Upload';
    const total = this.totalCount();
    if (total === 0) return 'Uploading…';
    return `Uploading ${this.uploadedCount()} / ${total}`;
  });

  setSaving(value: boolean): void {
    this.saving.set(value);
    if (!value) {
      this.uploadedCount.set(0);
      this.totalCount.set(0);
    }
  }

  setProgress(uploaded: number, total: number): void {
    this.uploadedCount.set(uploaded);
    this.totalCount.set(total);
  }

  showError(message: string): void {
    this.errorMessage.set(message);
  }

  protected onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const selected = input.files ? Array.from(input.files) : [];
    this.files.set(selected);
    this.errorMessage.set(null);
  }

  protected onSubmit(): void {
    const selected = this.files();
    if (selected.length === 0) {
      this.errorMessage.set('Select at least one file.');
      return;
    }
    this.errorMessage.set(null);
    this.save.emit(selected);
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

  protected formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
