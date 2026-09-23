import {
  apiRequest,
} from './api'
import type {
  ChainVerificationResult,
  EvidenceChainResult,
  EvidenceDetail,
  EvidenceListFilters,
  EvidenceListResult,
} from '../types/evidence'

export async function getEvidenceList(
  filters: EvidenceListFilters,
  signal?: AbortSignal,
): Promise<EvidenceListResult> {
  const parameters =
    new URLSearchParams()

  if (filters.search.trim()) {
    parameters.set(
      'search',
      filters.search.trim(),
    )
  }

  if (filters.custodianId.trim()) {
    parameters.set(
      'custodianId',
      filters.custodianId.trim(),
    )
  }

  if (filters.integrityStatus) {
    parameters.set(
      'integrityStatus',
      filters.integrityStatus,
    )
  }

  parameters.set(
    'sortDirection',
    filters.sortDirection,
  )

  parameters.set(
    'page',
    filters.page.toString(),
  )

  parameters.set(
    'pageSize',
    filters.pageSize.toString(),
  )

  return apiRequest<EvidenceListResult>(
    `/api/v1/evidence?${parameters.toString()}`,
    {
      method: 'GET',
      signal,
    },
  )
}

export function getEvidenceDetail(
  evidenceId: string,
  signal?: AbortSignal,
): Promise<EvidenceDetail> {
  return apiRequest<EvidenceDetail>(
    `/api/v1/evidence/${evidenceId}`,
    {
      method: 'GET',
      signal,
    },
  )
}

export function getEvidenceChain(
  evidenceId: string,
  signal?: AbortSignal,
): Promise<EvidenceChainResult> {
  return apiRequest<EvidenceChainResult>(
    `/api/v1/evidence/${evidenceId}/chain`,
    {
      method: 'GET',
      signal,
    },
  )
}

export function verifyEvidenceChain(
  evidenceId: string,
): Promise<ChainVerificationResult> {
  return apiRequest<ChainVerificationResult>(
    `/api/v1/evidence/${evidenceId}/chain/verify`,
    {
      method: 'GET',
    },
  )
}