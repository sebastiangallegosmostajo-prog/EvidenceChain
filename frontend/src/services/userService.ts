import {
  apiRequest,
} from './api'
import type {
  CustodianListItem,
} from '../types/custodyTransfer'

export function getCustodians(
  signal?: AbortSignal,
): Promise<CustodianListItem[]> {
  return apiRequest<
    CustodianListItem[]
  >(
    '/api/v1/users/custodians',
    {
      method: 'GET',
      signal,
    },
  )
}