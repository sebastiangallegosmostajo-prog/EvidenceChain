import {
  useEffect,
  useState,
  type FormEvent,
} from 'react'
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
  CustodianListItem,
  RequestCustodyTransferResult,
} from '../types/custodyTransfer'

interface RequestTransferPanelProps {
  evidenceId: string
  currentCustodianId: string
  onRequested: () => void
  onLogout: () => void
}

function formatDate(value: string): string {
  return new Intl.DateTimeFormat(
    'es-PE',
    {
      dateStyle: 'medium',
      timeStyle: 'short',
    },
  ).format(new Date(value))
}

export function RequestTransferPanel({
  evidenceId,
  currentCustodianId,
  onRequested,
  onLogout,
}: RequestTransferPanelProps) {
  const [custodians, setCustodians] =
    useState<CustodianListItem[]>([])

  const [
    selectedCustodianId,
    setSelectedCustodianId,
  ] = useState('')

  const [
    idempotencyKey,
    setIdempotencyKey,
  ] = useState(() =>
    crypto.randomUUID(),
  )

  const [result, setResult] =
    useState<
      RequestCustodyTransferResult | null
    >(null)

  const [isLoading, setIsLoading] =
    useState(true)

  const [isSubmitting, setIsSubmitting] =
    useState(false)

  const [error, setError] =
    useState<string | null>(null)

  useEffect(() => {
    const controller =
      new AbortController()

    async function loadCustodians() {
      setIsLoading(true)

      try {
        const response =
          await getCustodians(
            controller.signal,
          )

        setCustodians(
          response.filter(
            (custodian) =>
              custodian.id !==
                currentCustodianId,
          ),
        )
      } catch (exception) {
        if (
          exception instanceof DOMException &&
          exception.name === 'AbortError'
        ) {
          return
        }

        if (
          exception instanceof ApiError &&
          exception.status === 401
        ) {
          onLogout()
          return
        }

        if (
          exception instanceof ApiError
        ) {
          setError(exception.message)
        } else {
          setError(
            'No fue posible cargar los custodios.',
          )
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      }
    }

    void loadCustodians()

    return () => {
      controller.abort()
    }
  }, [
    currentCustodianId,
    onLogout,
  ])

  function handleCustodianChange(
    custodianId: string,
  ) {
    setSelectedCustodianId(
      custodianId,
    )

    setResult(null)
    setError(null)

    setIdempotencyKey(
      crypto.randomUUID(),
    )
  }

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    if (!selectedCustodianId) {
      setError(
        'Selecciona el custodio destinatario.',
      )
      return
    }

    setIsSubmitting(true)
    setError(null)
    setResult(null)

    try {
      const response =
        await requestTransfer(
          evidenceId,
          selectedCustodianId,
          idempotencyKey,
        )

      setResult(response)
      setSelectedCustodianId('')

      setIdempotencyKey(
        crypto.randomUUID(),
      )

      onRequested()
    } catch (exception) {
      if (
        exception instanceof ApiError &&
        exception.status === 401
      ) {
        onLogout()
        return
      }

      if (
        exception instanceof ApiError
      ) {
        setError(exception.message)
      } else {
        setError(
          'No fue posible solicitar la transferencia.',
        )
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="request-transfer-panel">
      <div>
        <p className="eyebrow">
          Cambio de custodia
        </p>

        <h2>
          Solicitar transferencia
        </h2>

        <p>
          Selecciona el custodio que deberá
          aceptar o rechazar la solicitud.
        </p>
      </div>

      <form
        className="request-transfer-form"
        onSubmit={handleSubmit}
      >
        <label>
          Custodio destinatario

          <select
            value={selectedCustodianId}
            disabled={
              isLoading ||
              isSubmitting
            }
            onChange={(event) =>
              handleCustodianChange(
                event.target.value,
              )
            }
            required
          >
            <option value="">
              {isLoading
                ? 'Cargando custodios...'
                : 'Selecciona un custodio'}
            </option>

            {custodians.map(
              (custodian) => (
                <option
                  key={custodian.id}
                  value={custodian.id}
                >
                  {custodian.name} —{' '}
                  {custodian.email}
                </option>
              ),
            )}
          </select>
        </label>

        <button
          type="submit"
          disabled={
            isLoading ||
            isSubmitting ||
            !selectedCustodianId
          }
        >
          {isSubmitting
            ? 'Solicitando...'
            : 'Solicitar transferencia'}
        </button>
      </form>

      {error && (
        <div
          className="error-message"
          role="alert"
        >
          {error}
        </div>
      )}

      {result && (
        <div
          className="success-message"
          role="status"
        >
          Transferencia solicitada
          correctamente. Vence el{' '}
          {formatDate(
            result.expiresAtUtc,
          )}
          .
        </div>
      )}
    </section>
  )
}