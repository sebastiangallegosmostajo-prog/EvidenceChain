import {
  useCallback,
  useEffect,
  useState,
} from 'react'
import {
  ApiError,
} from '../services/api'
import {
  acceptTransfer,
  getPendingTransfers,
  rejectTransfer,
} from '../services/custodyTransferService'
import type {
  AuthSession,
} from '../types/auth'
import type {
  PendingCustodyTransfer,
} from '../types/custodyTransfer'

interface PendingTransfersPageProps {
  session: AuthSession
  onOpenEvidences: () => void
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

export function PendingTransfersPage({
  session,
  onOpenEvidences,
  onLogout,
}: PendingTransfersPageProps) {
  const [transfers, setTransfers] =
    useState<PendingCustodyTransfer[]>(
      [],
    )

  const [rejectionReasons, setRejectionReasons] =
    useState<Record<string, string>>(
      {},
    )

  const [rejectingId, setRejectingId] =
    useState<string | null>(null)

  const [processingId, setProcessingId] =
    useState<string | null>(null)

  const [isLoading, setIsLoading] =
    useState(true)

  const [error, setError] =
    useState<string | null>(null)

  const [success, setSuccess] =
    useState<string | null>(null)

  const loadTransfers =
    useCallback(
      async (signal?: AbortSignal) => {
        setIsLoading(true)
        setError(null)

        try {
          const result =
            await getPendingTransfers(
              signal,
            )

          setTransfers(result.items)
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
              'No fue posible cargar las transferencias.',
            )
          }
        } finally {
          if (!signal?.aborted) {
            setIsLoading(false)
          }
        }
      },
      [onLogout],
    )

  useEffect(() => {
    const controller =
        new AbortController()

    const timeoutId =
        window.setTimeout(() => {
        void loadTransfers(
            controller.signal,
        )
        }, 0)

    return () => {
        window.clearTimeout(timeoutId)
        controller.abort()
    }
    }, [loadTransfers])

  async function handleAccept(
    transfer: PendingCustodyTransfer,
  ) {
    setProcessingId(
      transfer.transferId,
    )
    setError(null)
    setSuccess(null)

    try {
      await acceptTransfer(
        transfer.transferId,
        transfer.rowVersion,
      )

      setSuccess(
        `La transferencia de ${transfer.evidenceCode} fue aceptada.`,
      )

      await loadTransfers()
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
          'No fue posible aceptar la transferencia.',
        )
      }
    } finally {
      setProcessingId(null)
    }
  }

  async function handleReject(
    transfer: PendingCustodyTransfer,
  ) {
    const reason =
      rejectionReasons[
        transfer.transferId
      ]?.trim()

    if (!reason) {
      setError(
        'Debes indicar el motivo del rechazo.',
      )
      return
    }

    setProcessingId(
      transfer.transferId,
    )
    setError(null)
    setSuccess(null)

    try {
      await rejectTransfer(
        transfer.transferId,
        transfer.rowVersion,
        reason,
      )

      setSuccess(
        `La transferencia de ${transfer.evidenceCode} fue rechazada.`,
      )

      setRejectingId(null)

      await loadTransfers()
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
          'No fue posible rechazar la transferencia.',
        )
      }
    } finally {
      setProcessingId(null)
    }
  }

  return (
    <main className="authenticated-layout">
      <header className="application-header">
        <div className="header-brand">
          <span className="brand-mark small">
            EC
          </span>

          <div>
            <strong>EvidenceChain</strong>
            <small>
              Cadena de custodia digital
            </small>
          </div>
        </div>

        <div className="user-menu">
          <div>
            <strong>{session.name}</strong>
            <small>{session.role}</small>
          </div>

          <button
            className="secondary-button"
            type="button"
            onClick={onOpenEvidences}
          >
            Ver evidencias
          </button>

          <button
            className="secondary-button"
            type="button"
            onClick={onLogout}
          >
            Cerrar sesión
          </button>
        </div>
      </header>

      <section className="evidence-container">
        <div className="page-heading">
          <div>
            <p className="eyebrow">
              Transferencias
            </p>

            <h1>
              Solicitudes pendientes
            </h1>

            <p>
              Revisa las evidencias que
              requieren tu aceptación.
            </p>
          </div>

          <div className="result-counter">
            <strong>
              {transfers.length}
            </strong>

            <span>
              pendientes
            </span>
          </div>
        </div>

        {error && (
          <div
            className="error-message"
            role="alert"
          >
            {error}
          </div>
        )}

        {success && (
          <div
            className="success-message"
            role="status"
          >
            {success}
          </div>
        )}

        {isLoading ? (
          <div className="detail-state">
            Cargando transferencias...
          </div>
        ) : transfers.length === 0 ? (
          <div className="empty-transfer-state">
            <strong>
              No tienes transferencias
              pendientes
            </strong>

            <p>
              Las nuevas solicitudes
              aparecerán en esta bandeja.
            </p>
          </div>
        ) : (
          <div className="transfer-grid">
            {transfers.map(
              (transfer) => (
                <article
                  className={
                    transfer.isExpired
                      ? 'transfer-card expired'
                      : 'transfer-card'
                  }
                  key={transfer.transferId}
                >
                  <div className="transfer-card-heading">
                    <div>
                      <span>
                        Evidencia
                      </span>

                      <h2>
                        {
                          transfer.evidenceCode
                        }
                      </h2>
                    </div>

                    <span
                      className={
                        transfer.isExpired
                          ? 'expiration-badge expired'
                          : 'expiration-badge'
                      }
                    >
                      {transfer.isExpired
                        ? 'Vencida'
                        : 'Pendiente'}
                    </span>
                  </div>

                  <dl className="transfer-details">
                    <div>
                      <dt>
                        Custodio anterior
                      </dt>

                      <dd>
                        {
                          transfer.fromCustodianName
                        }
                      </dd>
                    </div>

                    <div>
                      <dt>
                        Solicitada por
                      </dt>

                      <dd>
                        {
                          transfer.requestedByName
                        }
                      </dd>
                    </div>

                    <div>
                      <dt>
                        Solicitud
                      </dt>

                      <dd>
                        {formatDate(
                          transfer.requestedAtUtc,
                        )}
                      </dd>
                    </div>

                    <div>
                      <dt>
                        Vencimiento
                      </dt>

                      <dd>
                        {formatDate(
                          transfer.expiresAtUtc,
                        )}
                      </dd>
                    </div>
                  </dl>

                  {rejectingId ===
                    transfer.transferId && (
                    <label className="rejection-field">
                      Motivo del rechazo

                      <textarea
                        value={
                          rejectionReasons[
                            transfer.transferId
                          ] ?? ''
                        }
                        onChange={(event) =>
                          setRejectionReasons(
                            {
                              ...rejectionReasons,
                              [
                                transfer.transferId
                              ]:
                                event.target.value,
                            },
                          )
                        }
                        placeholder="Describe el motivo"
                        rows={3}
                      />
                    </label>
                  )}

                  <div className="transfer-actions">
                    <button
                      type="button"
                      disabled={
                        processingId !== null
                      }
                      onClick={() =>
                        void handleAccept(
                          transfer,
                        )
                      }
                    >
                      {processingId ===
                      transfer.transferId
                        ? 'Procesando...'
                        : 'Aceptar'}
                    </button>

                    {rejectingId ===
                    transfer.transferId ? (
                      <>
                        <button
                          className="danger-button"
                          type="button"
                          disabled={
                            processingId !==
                            null
                          }
                          onClick={() =>
                            void handleReject(
                              transfer,
                            )
                          }
                        >
                          Confirmar rechazo
                        </button>

                        <button
                          className="secondary-button"
                          type="button"
                          disabled={
                            processingId !==
                            null
                          }
                          onClick={() =>
                            setRejectingId(
                              null,
                            )
                          }
                        >
                          Cancelar
                        </button>
                      </>
                    ) : (
                      <button
                        className="danger-button"
                        type="button"
                        disabled={
                          processingId !==
                          null
                        }
                        onClick={() =>
                          setRejectingId(
                            transfer.transferId,
                          )
                        }
                      >
                        Rechazar
                      </button>
                    )}
                  </div>
                </article>
              ),
            )}
          </div>
        )}
      </section>
    </main>
  )
}