import {
  apiRequest,
} from './api'
import type {
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