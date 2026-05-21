import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';
import { Upload } from '../../models';

interface UploadActionsParams extends ICellRendererParams {
  onDocuments: (upload: Upload) => void;
  onDelete: (upload: Upload) => void;
  onMap: (upload: Upload) => void;
}

@Component({
  selector: 'app-upload-actions',
  template: `
    <a href="javascript:void(0)" (click)="onDocuments()">Documents</a>
    @if (needsMapping()) {
      &nbsp;|&nbsp;
      <a href="javascript:void(0)" (click)="onMap()" class="map-link">Map</a>
    }
    &nbsp;|&nbsp;
    <a href="javascript:void(0)" (click)="onDelete()" class="delete-link">Delete</a>
  `,
  styles: [`
    :host { display: flex; align-items: center; height: 100%; }
    a { cursor: pointer; color: #1976d2; text-decoration: none; }
    a:hover { text-decoration: underline; }
    .delete-link { color: #d32f2f; }
    .map-link { color: #ed6c02; }
  `],
})
export class UploadActions implements ICellRendererAngularComp {
  private params!: UploadActionsParams;

  agInit(params: UploadActionsParams): void {
    this.params = params;
  }

  refresh(params: UploadActionsParams): boolean {
    this.params = params;
    return true;
  }

  needsMapping(): boolean {
    return (this.params.data?.awaitingMappingCount ?? 0) > 0;
  }

  onDocuments(): void {
    this.params.onDocuments(this.params.data);
  }

  onDelete(): void {
    this.params.onDelete(this.params.data);
  }

  onMap(): void {
    this.params.onMap(this.params.data);
  }
}
