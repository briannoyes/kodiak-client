import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';
import { Upload, UploadStatus } from '../../models';

@Component({
  selector: 'app-upload-progress-bar',
  template: `
    <div class="wrap" [title]="tooltip">
      <div class="bar" [class.terminal]="terminal" [class.failed]="failed">
        @if (knownTotal) {
          <div class="fill" [style.width.%]="percent"></div>
        } @else {
          <div class="indeterminate"></div>
        }
        <span class="label">{{ label }}</span>
      </div>
    </div>
  `,
  styles: [`
    :host { display: flex; align-items: center; height: 100%; width: 100%; }
    .wrap { width: 100%; }
    .bar {
      position: relative;
      width: 100%;
      height: 18px;
      background: #eee;
      border-radius: 3px;
      overflow: hidden;
    }
    .fill {
      position: absolute;
      inset: 0 auto 0 0;
      background: #1976d2;
      transition: width 0.3s ease;
    }
    .bar.terminal .fill { background: #2e7d32; }
    .bar.failed .fill { background: #d32f2f; }
    .indeterminate {
      position: absolute;
      inset: 0;
      background: repeating-linear-gradient(
        45deg,
        #bbb 0 8px,
        #d4d4d4 8px 16px
      );
      animation: slide 1.2s linear infinite;
    }
    @keyframes slide {
      from { background-position: 0 0; }
      to { background-position: 32px 0; }
    }
    .label {
      position: relative;
      display: block;
      text-align: center;
      font-size: 0.75rem;
      line-height: 18px;
      color: #fff;
      text-shadow: 0 0 2px rgba(0, 0, 0, 0.6);
    }
  `],
})
export class UploadProgressBar implements ICellRendererAngularComp {
  protected percent = 0;
  protected label = '';
  protected tooltip = '';
  protected terminal = false;
  protected failed = false;
  protected knownTotal = false;

  agInit(params: ICellRendererParams<Upload>): void {
    this.update(params);
  }

  refresh(params: ICellRendererParams<Upload>): boolean {
    this.update(params);
    return true;
  }

  private update(params: ICellRendererParams<Upload>): void {
    const upload = params.data;
    if (!upload) return;

    const processed = upload.processedCount + upload.dedupedCount + upload.failedCount;
    const total = upload.totalCount;
    this.knownTotal = total !== null && total > 0;
    this.percent = this.knownTotal && total ? Math.min(100, (processed / total) * 100) : 0;
    this.terminal =
      upload.status === UploadStatus.Completed ||
      upload.status === UploadStatus.Partial;
    this.failed = upload.status === UploadStatus.Failed;

    if (this.knownTotal) {
      this.label = `${processed} / ${total} (${this.percent.toFixed(0)}%)`;
    } else if (upload.status === UploadStatus.Pending) {
      this.label = 'Pending';
    } else {
      this.label = 'Discovering…';
    }
    this.tooltip =
      `Status: ${upload.status} — ` +
      `processed ${upload.processedCount}, deduped ${upload.dedupedCount}, ` +
      `failed ${upload.failedCount}, awaiting mapping ${upload.awaitingMappingCount}`;
  }
}
