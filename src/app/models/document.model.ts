export enum DocumentType {
  Contract = 'Contract',
  PaymentRecord = 'PaymentRecord',
}

export enum DocumentStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  AwaitingMapping = 'AwaitingMapping',
  Completed = 'Completed',
  DedupSkipped = 'DedupSkipped',
  Failed = 'Failed',
}

export interface Document {
  id: number;
  externalId: string;
  documentType: DocumentType;
  uploadId: number | null;
  clientId: number;
  clientExternalId: string;
  originalFileName: string | null;
  contentType: string | null;
  sizeBytes: number | null;
  status: DocumentStatus;
  errorMessage: string | null;
  uploadedAt: string;
  processedAt: string | null;
}
