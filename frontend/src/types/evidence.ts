export type IntegrityStatus =
  | 'Unknown'
  | 'Intact'
  | 'Compromised'

export type SortDirection =
  | 'asc'
  | 'desc'

export interface EvidenceListItem {
  id: string
  code: string
  description: string
  currentCustodianId: string
  currentCustodianName: string
  createdAtUtc: string
  lastEventAtUtc: string
  integrityStatus: IntegrityStatus
}

export interface EvidenceListResult {
  items: EvidenceListItem[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface EvidenceListFilters {
  search: string
  custodianId: string
  integrityStatus: '' | IntegrityStatus
  sortDirection: SortDirection
  page: number
  pageSize: number
}

export interface EvidenceDetail {
  id: string
  code: string
  description: string
  currentCustodianId: string
  currentCustodianName: string
  currentCustodianEmail: string
  createdAtUtc: string
  lastEventAtUtc: string
  integrityStatus: IntegrityStatus
}

export interface EvidenceChainEvent {
  id: string
  sequenceNumber: number
  eventType: string
  actorId: string
  actorName: string
  fromCustodianId: string | null
  fromCustodianName: string | null
  toCustodianId: string | null
  toCustodianName: string | null
  transferId: string | null
  occurredAtUtc: string
  details: string
  previousHash: string
  hash: string
  hasAnomaly: boolean
  anomalySeverity: string | null
  anomalyExplanation: string | null
}

export interface EvidenceAnomaly {
  type: string
  severity: string
  explanation: string
  transferId: string
  requestedAtUtc: string
  expiresAtUtc: string
}

export interface EvidenceChainResult {
  evidenceId: string
  evidenceCode: string
  integrityStatus: IntegrityStatus
  events: EvidenceChainEvent[]
  anomalies: EvidenceAnomaly[]
}

export interface ChainVerificationResult {
  evidenceId: string
  isIntact: boolean
  integrityStatus: IntegrityStatus
  eventCount: number
  firstInvalidEventId: string | null
  firstInvalidSequenceNumber: number | null
  failureReason: string | null
  expectedPreviousHash: string | null
  actualPreviousHash: string | null
  storedHash: string | null
  calculatedHash: string | null
  verifiedAtUtc: string
}