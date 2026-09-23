import {
  useEffect,
  useState,
} from 'react'
import {
  ApiError,
} from '../services/api'
import {
  getEvidenceChain,
  getEvidenceDetail,
  verifyEvidenceChain,
} from '../services/evidenceService'
import type {
  AuthSession,
} from '../types/auth'
import type {
  ChainVerificationResult,
  EvidenceChainResult,
  EvidenceDetail,
  IntegrityStatus,
} from '../types/evidence'

interface EvidenceDetailPageProps {
  evidenceId: string
  session: AuthSession
  onBack: () => void
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

function getIntegrityLabel(
  status: IntegrityStatus,
): string {
  switch (status) {
    case 'Intact':
      return 'Íntegra'

    case 'Compromised':
      return 'Comprometida'

    default:
      return 'Sin verificar'
  }
}

function getEventLabel(
  eventType: string,
): string {
  const labels:
    Record<string, string> = {
      EvidenceRegistered:
        'Evidencia registrada',
      TransferRequested:
        'Transferencia solicitada',
      TransferAccepted:
        'Transferencia aceptada',
      TransferRejected:
        'Transferencia rechazada',
    }

  return labels[eventType] ??
    eventType
}

function shortenHash(hash: string): string {
  if (hash.length <= 28) {
    return hash
  }

  return `${hash.slice(0, 16)}…${hash.slice(-10)}`
}

export function EvidenceDetailPage({
  evidenceId,
  session,
  onBack,
  onLogout,
}: EvidenceDetailPageProps) {
  const [detail, setDetail] =
    useState<EvidenceDetail | null>(null)

  const [chain, setChain] =
    useState<EvidenceChainResult | null>(
      null,
    )

  const [verification, setVerification] =
    useState<ChainVerificationResult | null>(
      null,
    )

  const [isLoading, setIsLoading] =
    useState(true)

  const [isVerifying, setIsVerifying] =
    useState(false)

  const [error, setError] =
    useState<string | null>(null)

  useEffect(() => {
    const controller =
      new AbortController()

    async function loadEvidence() {
      setIsLoading(true)
      setError(null)

      try {
        const [
          detailResult,
          chainResult,
        ] = await Promise.all([
          getEvidenceDetail(
            evidenceId,
            controller.signal,
          ),
          getEvidenceChain(
            evidenceId,
            controller.signal,
          ),
        ])

        setDetail(detailResult)
        setChain(chainResult)
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

        if (exception instanceof ApiError) {
          setError(exception.message)
        } else {
          setError(
            'No fue posible cargar la evidencia.',
          )
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      }
    }

    void loadEvidence()

    return () => {
      controller.abort()
    }
  }, [evidenceId, onLogout])

  async function handleVerify() {
    setIsVerifying(true)
    setError(null)

    try {
      const verificationResult =
        await verifyEvidenceChain(
          evidenceId,
        )

      setVerification(
        verificationResult,
      )

      const [
        updatedDetail,
        updatedChain,
      ] = await Promise.all([
        getEvidenceDetail(evidenceId),
        getEvidenceChain(evidenceId),
      ])

      setDetail(updatedDetail)
      setChain(updatedChain)
    } catch (exception) {
      if (
        exception instanceof ApiError &&
        exception.status === 401
      ) {
        onLogout()
        return
      }

      if (exception instanceof ApiError) {
        setError(exception.message)
      } else {
        setError(
          'No fue posible verificar la cadena.',
        )
      }
    } finally {
      setIsVerifying(false)
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
            onClick={onLogout}
          >
            Cerrar sesión
          </button>
        </div>
      </header>

      <section className="evidence-container">
        <button
          className="back-button"
          type="button"
          onClick={onBack}
        >
          ← Volver a la bandeja
        </button>

        {error && (
          <div
            className="error-message"
            role="alert"
          >
            {error}
          </div>
        )}

        {isLoading ? (
          <div className="detail-state">
            Cargando evidencia...
          </div>
        ) : detail && chain ? (
          <>
            <div className="detail-heading">
              <div>
                <p className="eyebrow">
                  Detalle de evidencia
                </p>

                <h1>{detail.code}</h1>

                <p>{detail.description}</p>
              </div>

              <div className="detail-heading-actions">
                <span
                  className={
                    `integrity-badge large ${
                      detail.integrityStatus
                    }`
                  }
                >
                  {getIntegrityLabel(
                    detail.integrityStatus,
                  )}
                </span>

                <button
                  type="button"
                  disabled={isVerifying}
                  onClick={handleVerify}
                >
                  {isVerifying
                    ? 'Verificando...'
                    : 'Verificar integridad'}
                </button>
              </div>
            </div>

            <section className="detail-grid">
              <article className="information-card">
                <span>Custodio actual</span>
                <strong>
                  {detail.currentCustodianName}
                </strong>
                <small>
                  {detail.currentCustodianEmail}
                </small>
              </article>

              <article className="information-card">
                <span>Fecha de registro</span>
                <strong>
                  {formatDate(
                    detail.createdAtUtc,
                  )}
                </strong>
              </article>

              <article className="information-card">
                <span>Último evento</span>
                <strong>
                  {formatDate(
                    detail.lastEventAtUtc,
                  )}
                </strong>
              </article>

              <article className="information-card">
                <span>Eventos de custodia</span>
                <strong>
                  {chain.events.length}
                </strong>
              </article>
            </section>

            {verification && (
              <section
                className={
                  verification.isIntact
                    ? 'verification-card success'
                    : 'verification-card failure'
                }
              >
                <div>
                  <strong>
                    {verification.isIntact
                      ? 'Cadena íntegra'
                      : 'Cadena comprometida'}
                  </strong>

                  <p>
                    Se verificaron{' '}
                    {verification.eventCount}{' '}
                    eventos el{' '}
                    {formatDate(
                      verification.verifiedAtUtc,
                    )}
                    .
                  </p>
                </div>

                {verification.failureReason && (
                  <p>
                    {verification.failureReason}
                  </p>
                )}

                {verification.firstInvalidSequenceNumber && (
                  <p>
                    Primer evento inválido:
                    secuencia{' '}
                    {
                      verification.firstInvalidSequenceNumber
                    }.
                  </p>
                )}
              </section>
            )}

            {chain.anomalies.length > 0 && (
              <section className="anomaly-section">
                <h2>Anomalías detectadas</h2>

                {chain.anomalies.map(
                  (anomaly) => (
                    <article
                      className="anomaly-card"
                      key={anomaly.transferId}
                    >
                      <strong>
                        {anomaly.severity} ·{' '}
                        {anomaly.type}
                      </strong>

                      <p>
                        {anomaly.explanation}
                      </p>

                      <small>
                        Vencimiento:{' '}
                        {formatDate(
                          anomaly.expiresAtUtc,
                        )}
                      </small>
                    </article>
                  ),
                )}
              </section>
            )}

            <section className="timeline-section">
              <div className="section-heading">
                <div>
                  <p className="eyebrow">
                    Trazabilidad
                  </p>

                  <h2>
                    Cadena de custodia
                  </h2>
                </div>

                <span>
                  {chain.events.length}{' '}
                  eventos
                </span>
              </div>

              <div className="timeline">
                {chain.events.map(
                  (event) => (
                    <article
                      className={
                        event.hasAnomaly
                          ? 'timeline-event anomaly'
                          : 'timeline-event'
                      }
                      key={event.id}
                    >
                      <div className="timeline-marker">
                        {event.sequenceNumber}
                      </div>

                      <div className="timeline-content">
                        <div className="event-heading">
                          <div>
                            <strong>
                              {getEventLabel(
                                event.eventType,
                              )}
                            </strong>

                            <span>
                              {formatDate(
                                event.occurredAtUtc,
                              )}
                            </span>
                          </div>

                          <span>
                            {event.actorName}
                          </span>
                        </div>

                        <p>{event.details}</p>

                        {(event.fromCustodianName ||
                          event.toCustodianName) && (
                          <div className="custody-change">
                            <span>
                              {event.fromCustodianName ??
                                'Sin custodio'}
                            </span>

                            <strong>→</strong>

                            <span>
                              {event.toCustodianName ??
                                'Sin custodio'}
                            </span>
                          </div>
                        )}

                        {event.hasAnomaly && (
                          <div className="event-anomaly">
                            {event.anomalyExplanation}
                          </div>
                        )}

                        <details className="hash-details">
                          <summary>
                            Información criptográfica
                          </summary>

                          <div>
                            <span>Hash anterior</span>
                            <code title={event.previousHash}>
                              {shortenHash(
                                event.previousHash,
                              )}
                            </code>
                          </div>

                          <div>
                            <span>Hash del evento</span>
                            <code title={event.hash}>
                              {shortenHash(
                                event.hash,
                              )}
                            </code>
                          </div>
                        </details>
                      </div>
                    </article>
                  ),
                )}
              </div>
            </section>
          </>
        ) : (
          <div className="detail-state">
            No se encontró la evidencia.
          </div>
        )}
      </section>
    </main>
  )
}