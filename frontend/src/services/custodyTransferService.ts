import {
  apiRequest,
} from './api'
import type {
  CustodyTransferActionResult,
  PendingCustodyTransfersResult,
} from '../types/custodyTransfer'

export function getPendingTransfers(
  signal?: AbortSignal,
): Promise<PendingCustodyTransfersResult> {
  return apiRequest<
    PendingCustodyTransfersResult
  >(
    '/api/v1/custody-transfers/pending',
    {
      method: 'GET',
      signal,
    },
  )
}

export function acceptTransfer(
  transferId: string,
  rowVersion: string,
): Promise<CustodyTransferActionResult> {
  return apiRequest<
    CustodyTransferActionResult
  >(
    `/api/v1/custody-transfers/${transferId}/accept`,
    {
      method: 'POST',
      headers: {
        'If-Match':
          `"${rowVersion}"`,
      },
    },
  )
}

export function rejectTransfer(
  transferId: string,
  rowVersion: string,
  reason: string,
): Promise<CustodyTransferActionResult> {
  return apiRequest<
    CustodyTransferActionResult
  >(
    `/api/v1/custody-transfers/${transferId}/reject`,
    {
      method: 'POST',
      headers: {
        'If-Match':
          `"${rowVersion}"`,
      },
      body: JSON.stringify({
        reason,
      }),
    },
  )
}