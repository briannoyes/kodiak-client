import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';
import { Document } from '../../models';

interface DocumentActionsParams extends ICellRendererParams {
  onDelete: (document: Document) => void;
}

@Component({
  selector: 'app-document-actions',
  template: `
    <a href="javascript:void(0)" (click)="onDelete()" class="delete-link">Delete</a>
  `,
  styles: [`
    :host { display: flex; align-items: center; height: 100%; }
    a { cursor: pointer; text-decoration: none; }
    a:hover { text-decoration: underline; }
    .delete-link { color: #d32f2f; }
  `],
})
export class DocumentActions implements ICellRendererAngularComp {
  private params!: DocumentActionsParams;

  agInit(params: DocumentActionsParams): void {
    this.params = params;
  }

  refresh(params: DocumentActionsParams): boolean {
    this.params = params;
    return true;
  }

  onDelete(): void {
    this.params.onDelete(this.params.data);
  }
}
