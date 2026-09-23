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
  ApiError,
} from '../services/api'
import {
  requestTransfer,
} from '../services/custodyTransferService'
import {
  getCustodians,
} from '../services/userService'
import type {
  RequestCustodyTransferResult,
} from '../types/custodyTransfer'
import {
  RequestTransferPanel,
} from './RequestTransferPanel'

vi.mock(
  '../services/userService',
  () => ({
    getCustodians: vi.fn(),
  }),
)

vi.mock(
  '../services/custodyTransferService',
  () => ({
    requestTransfer: vi.fn(),
  }),
)

const getCustodiansMock =
  vi.mocked(getCustodians)

const requestTransferMock =
  vi.mocked(requestTransfer)

const evidenceId =
  '11111111-1111-1111-1111-111111111111'

const currentCustodianId =
  '44444444-4444-4444-4444-444444444444'

const targetCustodianId =
  '22222222-2222-2222-2222-222222222222'

function configureCustodians() {
  getCustodiansMock.mockResolvedValue([
    {
      id: targetCustodianId,
      name: 'Custodio Dos',
      email:
        'custodio2@evidencechain.local',
    },
  ])
}

function renderPanel(
  onRequested = vi.fn(),
) {
  render(
    <RequestTransferPanel
      evidenceId={evidenceId}
      currentCustodianId={
        currentCustodianId
      }
      onRequested={onRequested}
      onLogout={vi.fn()}
    />,
  )

  return {
    onRequested,
  }
}

describe(
  'RequestTransferPanel',
  () => {
    beforeEach(() => {
      getCustodiansMock.mockReset()
      requestTransferMock.mockReset()
    })

    it(
      'solicita una transferencia al custodio seleccionado',
      async () => {
        const user =
          userEvent.setup()

        const onRequested =
          vi.fn()

        configureCustodians()

        requestTransferMock
          .mockResolvedValue({
            transferId:
              '33333333-3333-3333-3333-333333333333',
            evidenceId,
            fromCustodianId:
              currentCustodianId,
            toCustodianId:
              targetCustodianId,
            status: 'Pending',
            requestedAtUtc:
              '2026-09-23T12:00:00Z',
            expiresAtUtc:
              '2026-09-24T12:00:00Z',
            rowVersion:
              'AQIDBA==',
          })

        renderPanel(onRequested)

        const selector =
          await screen.findByLabelText(
            /custodio destinatario/i,
          )

        await user.selectOptions(
          selector,
          targetCustodianId,
        )

        await user.click(
          screen.getByRole(
            'button',
            {
              name:
                /solicitar transferencia/i,
            },
          ),
        )

        await waitFor(() => {
          expect(requestTransferMock)
            .toHaveBeenCalledWith(
              evidenceId,
              targetCustodianId,
              expect.any(String),
            )
        })

        expect(
          await screen.findByText(
            /transferencia solicitada correctamente/i,
          ),
        ).toBeInTheDocument()

        expect(onRequested)
          .toHaveBeenCalledOnce()
      },
    )

    it(
      'revierte el estado pendiente ante un conflicto 409',
      async () => {
        const user =
          userEvent.setup()

        const onRequested =
          vi.fn()

        let rejectRequest:
          (
            reason?: unknown,
          ) => void = () => undefined

        configureCustodians()

        requestTransferMock
          .mockImplementation(
            () =>
              new Promise<
                RequestCustodyTransferResult
              >((_, reject) => {
                rejectRequest = reject
              }),
          )

        renderPanel(onRequested)

        const selector =
          await screen.findByLabelText(
            /custodio destinatario/i,
          )

        await user.selectOptions(
          selector,
          targetCustodianId,
        )

        await user.click(
          screen.getByRole(
            'button',
            {
              name:
                /solicitar transferencia/i,
            },
          ),
        )

        expect(
          screen.getByText(
            /pendiente de confirmación/i,
          ),
        ).toBeInTheDocument()

        rejectRequest(
          new ApiError(
            'La transferencia cambió de estado. Actualiza e intenta nuevamente.',
            409,
          ),
        )

        expect(
          await screen.findByRole('alert'),
        ).toHaveTextContent(
          /cambió de estado/i,
        )

        await waitFor(() => {
          expect(
            screen.queryByText(
              /pendiente de confirmación/i,
            ),
          ).not.toBeInTheDocument()
        })

        expect(selector)
          .toHaveValue(
            targetCustodianId,
          )

        expect(onRequested)
          .not.toHaveBeenCalled()
      },
    )
  },
)