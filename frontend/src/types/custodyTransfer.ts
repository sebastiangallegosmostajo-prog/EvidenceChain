export interface PendingCustodyTransfer {
  transferId: string
  evidenceId: string
  evidenceCode: string
  fromCustodianId: string
  fromCustodianName: string
  toCustodianId: string
  requestedById: string
  requestedByName: string
  requestedAtUtc: string
  expiresAtUtc: string
  isExpired: boolean
  status: string
  rowVersion: string
}

export interface PendingCustodyTransfersResult {
  items: PendingCustodyTransfer[]
  totalCount: number
}

export interface CustodyTransferActionResult {
  transferId: string
  evidenceId: string
  fromCustodianId: string
  toCustodianId: string
  status: string
  respondedAtUtc: string | null
  rejectionReason: string | null
  rowVersion: string
}

export interface CustodianListItem {
  id: string
  name: string
  email: string
}

export interface RequestCustodyTransferResult {
  transferId: string
  evidenceId: string
  fromCustodianId: string
  toCustodianId: string
  status: string
  requestedAtUtc: string
  expiresAtUtc: string
  rowVersion: string
}