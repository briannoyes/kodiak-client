export enum UploadSourceType {
  Direct = 'Direct',
  Bulk = 'Bulk',
}

export enum UploadStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  Completed = 'Completed',
  Partial = 'Partial',
  Failed = 'Failed',
}

export interface Upload {
  id: number;
  externalId: string;
  clientId: number;
  sourceType: UploadSourceType;
  status: UploadStatus;
  totalCount: number | null;
  processedCount: number;
  dedupedCount: number;
  failedCount: number;
  awaitingMappingCount: number;
  createdAt: string;
  completedAt: string | null;
}

export interface PaymentRecordFileResult {
  originalFileName: string;
  documentExternalId: string;
  status: string;
  requiresMapping: boolean;
}

export interface PaymentRecordUploadAccepted {
  uploadExternalId: string;
  files: PaymentRecordFileResult[];
}

export interface HeaderGroup {
  fingerprint: string;
  headers: string[];
  documentExternalIds: string[];
}

export interface HeaderMappingApproval {
  mapping: Record<string, string>;
}

export interface UploadAccepted {
  uploadExternalId: string;
}

export interface DirectContractUploadFile {
  fileName: string;
  contentType: string | null;
  sizeBytes: number | null;
}

export interface DirectContractUploadRequest {
  clientExternalId: string;
  files: DirectContractUploadFile[];
}

export interface DirectContractUploadSlot {
  documentExternalId: string;
  fileName: string;
  blobPath: string;
  sasPutUrl: string;
  expiresAt: string;
}

export interface DirectContractUploadInitiated {
  uploadExternalId: string;
  files: DirectContractUploadSlot[];
}

export interface DirectContractUploadFinalizeRequest {
  documentExternalIds: string[];
}
