import {
  act,
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
  getEvidenceList,
} from '../services/evidenceService'
import type {
  EvidenceListResult,
} from '../types/evidence'
import {
  EvidenceListPage,
} from './EvidenceListPage'

vi.mock(
  '../services/evidenceService',
  () => ({
    getEvidenceList: vi.fn(),
  }),
)

const getEvidenceListMock =
  vi.mocked(getEvidenceList)

function createDeferred<T>() {
  let resolve:
    (value: T) => void =
      () => undefined

  let reject:
    (reason?: unknown) => void =
      () => undefined

  const promise =
    new Promise<T>(
      (
        resolvePromise,
        rejectPromise,
      ) => {
        resolve = resolvePromise
        reject = rejectPromise
      },
    )

  return {
    promise,
    resolve,
    reject,
  }
}

function createResult(
  code: string,
  description: string,
): EvidenceListResult {
  return {
    items: [
      {
        id:
          '11111111-1111-1111-1111-111111111111',
        code,
        description,
        currentCustodianId:
          '22222222-2222-2222-2222-222222222222',
        currentCustodianName:
          'Custodio Uno',
        createdAtUtc:
          '2026-09-23T10:00:00Z',
        lastEventAtUtc:
          '2026-09-23T12:00:00Z',
        integrityStatus:
          'Intact',
      },
    ],
    page: 1,
    pageSize: 10,
    totalCount: 1,
    totalPages: 1,
  }
}

describe(
  'EvidenceListPage',
  () => {
    beforeEach(() => {
      getEvidenceListMock.mockReset()

      window.history.replaceState(
        {},
        '',
        '/',
      )
    })

    it(
      'impide que una respuesta obsoleta reemplace la búsqueda actual',
      async () => {
        const user =
          userEvent.setup()

        const firstRequest =
          createDeferred<
            EvidenceListResult
          >()

        const secondRequest =
          createDeferred<
            EvidenceListResult
          >()

        getEvidenceListMock
          .mockReturnValueOnce(
            firstRequest.promise,
          )
          .mockReturnValueOnce(
            secondRequest.promise,
          )

        render(
          <EvidenceListPage
            session={{
                userId:
                    '33333333-3333-3333-3333-333333333333',
                name:
                    'Investigador Demo',
                email:
                    'investigador@evidencechain.local',
                role:
                    'Investigador',
                }}
            onLogout={vi.fn()}
            onSelectEvidence={
              vi.fn()
            }
          />,
        )

        await waitFor(() => {
          expect(
            getEvidenceListMock,
          ).toHaveBeenCalledTimes(1)
        })

        await user.type(
          screen.getByLabelText(
            /buscar/i,
          ),
          'EV-NUEVA',
        )

        await user.click(
          screen.getByRole(
            'button',
            {
              name: /aplicar/i,
            },
          ),
        )

        await waitFor(() => {
          expect(
            getEvidenceListMock,
          ).toHaveBeenCalledTimes(2)
        })

        await act(async () => {
          secondRequest.resolve(
            createResult(
              'EV-NUEVA',
              'Resultado de la búsqueda actual',
            ),
          )

          await secondRequest.promise
        })

        expect(
          await screen.findByText(
            'EV-NUEVA',
          ),
        ).toBeInTheDocument()

        await act(async () => {
          firstRequest.resolve(
            createResult(
              'EV-ANTIGUA',
              'Resultado obsoleto',
            ),
          )

          await firstRequest.promise
        })

        expect(
          screen.queryByText(
            'EV-ANTIGUA',
          ),
        ).not.toBeInTheDocument()

        expect(
          screen.getByText(
            'EV-NUEVA',
          ),
        ).toBeInTheDocument()
      },
    )
  },
)