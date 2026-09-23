import {
  render,
  screen,
  waitFor,
} from '@testing-library/react'
import userEvent from
  '@testing-library/user-event'
import {
  beforeEach,
  describe,
  expect,
  it,
  vi,
} from 'vitest'
import {
  acceptTransfer,
  getPendingTransfers,
  rejectTransfer,
} from '../services/custodyTransferService'
import {
  PendingTransfersPage,
} from './PendingTransfersPage'

vi.mock(
  '../services/custodyTransferService',
  () => ({
    getPendingTransfers:
      vi.fn(),
    acceptTransfer:
      vi.fn(),
    rejectTransfer:
      vi.fn(),
  }),
)

const getPendingTransfersMock =
  vi.mocked(getPendingTransfers)

const acceptTransferMock =
  vi.mocked(acceptTransfer)

const rejectTransferMock =
  vi.mocked(rejectTransfer)

describe(
  'PendingTransfersPage',
  () => {
    beforeEach(() => {
      getPendingTransfersMock
        .mockReset()

      acceptTransferMock
        .mockReset()

      rejectTransferMock
        .mockReset()
    })

    it(
      'acepta una transferencia pendiente',
      async () => {
        const user =
          userEvent.setup()

        getPendingTransfersMock
          .mockResolvedValueOnce({
            totalCount: 1,
            items: [
              {
                transferId:
                  '33333333-3333-3333-3333-333333333333',
                evidenceId:
                  '11111111-1111-1111-1111-111111111111',
                evidenceCode:
                  'EV-TEST-001',
                fromCustodianId:
                  '44444444-4444-4444-4444-444444444444',
                fromCustodianName:
                  'Custodio Uno',
                toCustodianId:
                  '22222222-2222-2222-2222-222222222222',
                requestedById:
                  '55555555-5555-5555-5555-555555555555',
                requestedByName:
                  'Investigador Demo',
                requestedAtUtc:
                  '2026-09-23T12:00:00Z',
                expiresAtUtc:
                  '2026-09-24T12:00:00Z',
                isExpired:
                  false,
                status:
                  'Pending',
                rowVersion:
                  'AQIDBA==',
              },
            ],
          })
          .mockResolvedValue({
            totalCount: 0,
            items: [],
          })

        acceptTransferMock
          .mockResolvedValue({
            transferId:
              '33333333-3333-3333-3333-333333333333',
            evidenceId:
              '11111111-1111-1111-1111-111111111111',
            fromCustodianId:
              '44444444-4444-4444-4444-444444444444',
            toCustodianId:
              '22222222-2222-2222-2222-222222222222',
            status:
              'Accepted',
            respondedAtUtc:
              '2026-09-23T12:05:00Z',
            rejectionReason:
              null,
            rowVersion:
              'BQYHCA==',
          })

        render(
          <PendingTransfersPage
            session={{
              userId:
                '22222222-2222-2222-2222-222222222222',
              name:
                'Custodio Dos',
              email:
                'custodio2@evidencechain.local',
              role:
                'Custodian',
            }}
            onOpenEvidences={
              vi.fn()
            }
            onLogout={vi.fn()}
          />,
        )

        expect(
          await screen.findByText(
            'EV-TEST-001',
          ),
        ).toBeInTheDocument()

        await user.click(
          screen.getByRole(
            'button',
            {
              name: /aceptar/i,
            },
          ),
        )

        await waitFor(() => {
          expect(acceptTransferMock)
            .toHaveBeenCalledWith(
              '33333333-3333-3333-3333-333333333333',
              'AQIDBA==',
            )
        })

        expect(
          await screen.findByText(
            /fue aceptada/i,
          ),
        ).toBeInTheDocument()
      },
    )
  },
)