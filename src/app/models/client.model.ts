export enum ClientStatus {
  Active = 'Active',
  Suspended = 'Suspended',
  Archived = 'Archived',
}

export interface Client {
  id: number;
  externalId: string;
  name: string;
  status: ClientStatus;
  billingEmail: string | null;
  createdAt: string;
  updatedAt: string;
  documentCount: number;
  processingStatus: string;
}
