import { Component } from '@angular/core';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';
import { Client } from '../../models';

interface ClientActionsParams extends ICellRendererParams {
  onOpen: (client: Client) => void;
  onDelete: (client: Client) => void;
}

@Component({
  selector: 'app-client-actions',
  template: `
    <a href="javascript:void(0)" (click)="onOpen()">Open</a>
    &nbsp;|&nbsp;
    <a href="javascript:void(0)" (click)="onDelete()" class="delete-link">Delete</a>
  `,
  styles: [`
    :host { display: flex; align-items: center; height: 100%; }
    a { cursor: pointer; color: #1976d2; text-decoration: none; }
    a:hover { text-decoration: underline; }
    .delete-link { color: #d32f2f; }
  `],
})
export class ClientActions implements ICellRendererAngularComp {
  private params!: ClientActionsParams;

  agInit(params: ClientActionsParams): void {
    this.params = params;
  }

  refresh(params: ClientActionsParams): boolean {
    this.params = params;
    return true;
  }

  onOpen(): void {
    this.params.onOpen(this.params.data);
  }

  onDelete(): void {
    this.params.onDelete(this.params.data);
  }
}
