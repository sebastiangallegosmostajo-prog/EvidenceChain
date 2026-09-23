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