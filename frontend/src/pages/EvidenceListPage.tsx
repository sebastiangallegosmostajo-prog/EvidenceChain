import {
  useEffect,
  useState,
  type FormEvent,
} from 'react'
import {
  ApiError,
} from '../services/api'
import {
  getEvidenceList,
} from '../services/evidenceService'
import type {
  AuthSession,
} from '../types/auth'
import type {
  EvidenceListFilters,
  EvidenceListResult,
  IntegrityStatus,
  SortDirection,
} from '../types/evidence'

interface EvidenceListPageProps {
  session: AuthSession
  onLogout: () => void
  onSelectEvidence: (
    evidenceId: string,
  ) => void
}

function readFiltersFromUrl():
  EvidenceListFilters {
  const parameters =
    new URLSearchParams(
      window.location.search,
    )

  const pageValue =
    Number(parameters.get('page'))

  const pageSizeValue =
    Number(parameters.get('pageSize'))

  const integrityStatus =
    parameters.get('integrityStatus')

  const sortDirection =
    parameters.get('sortDirection')

  return {
    search:
      parameters.get('search') ?? '',

    custodianId:
      parameters.get('custodianId') ?? '',

    integrityStatus:
      integrityStatus === 'Unknown' ||
      integrityStatus === 'Intact' ||
      integrityStatus === 'Compromised'
        ? integrityStatus
        : '',

    sortDirection:
      sortDirection === 'asc'
        ? 'asc'
        : 'desc',

    page:
      Number.isInteger(pageValue) &&
      pageValue > 0
        ? pageValue
        : 1,

    pageSize:
      Number.isInteger(pageSizeValue) &&
      pageSizeValue > 0 &&
      pageSizeValue <= 100
        ? pageSizeValue
        : 10,
  }
}

function createUrl(
  filters: EvidenceListFilters,
): string {
  const parameters =
    new URLSearchParams()

  if (filters.search) {
    parameters.set(
      'search',
      filters.search,
    )
  }

  if (filters.custodianId) {
    parameters.set(
      'custodianId',
      filters.custodianId,
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

  return `/?${parameters.toString()}`
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

export function EvidenceListPage({
  session,
  onLogout,
  onSelectEvidence,
}: EvidenceListPageProps) {
  const [filters, setFilters] =
    useState<EvidenceListFilters>(
      readFiltersFromUrl,
    )

  const [draftSearch, setDraftSearch] =
    useState(filters.search)

  const [
    draftCustodianId,
    setDraftCustodianId,
  ] = useState(filters.custodianId)

  const [result, setResult] =
    useState<EvidenceListResult | null>(
      null,
    )

  const [isLoading, setIsLoading] =
    useState(true)

  const [error, setError] =
    useState<string | null>(null)

  function navigateToFilters(
    nextFilters: EvidenceListFilters,
    replace = false,
  ) {
    const url =
      createUrl(nextFilters)

    if (replace) {
      window.history.replaceState(
        {},
        '',
        url,
      )
    } else {
      window.history.pushState(
        {},
        '',
        url,
      )
    }

    setFilters(nextFilters)
  }

  useEffect(() => {
    const handlePopState = () => {
      const nextFilters =
        readFiltersFromUrl()

      setFilters(nextFilters)

      setDraftSearch(
        nextFilters.search,
      )

      setDraftCustodianId(
        nextFilters.custodianId,
      )
    }

    window.addEventListener(
      'popstate',
      handlePopState,
    )

    return () => {
      window.removeEventListener(
        'popstate',
        handlePopState,
      )
    }
  }, [])

  useEffect(() => {
    const controller =
      new AbortController()

    async function loadEvidences() {
      setIsLoading(true)
      setError(null)

      try {
        const response =
          await getEvidenceList(
            filters,
            controller.signal,
          )

        setResult(response)
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
            'No fue posible cargar las evidencias.',
          )
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      }
    }

    void loadEvidences()

    return () => {
      controller.abort()
    }
  }, [filters, onLogout])

  function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    navigateToFilters({
      ...filters,
      search: draftSearch.trim(),
      custodianId:
        draftCustodianId.trim(),
      page: 1,
    })
  }

  function handleStatusChange(
    status: '' | IntegrityStatus,
  ) {
    navigateToFilters({
      ...filters,
      integrityStatus: status,
      page: 1,
    })
  }

  function handleSortChange(
    sortDirection: SortDirection,
  ) {
    navigateToFilters({
      ...filters,
      sortDirection,
      page: 1,
    })
  }

  function handlePageChange(
    page: number,
  ) {
    navigateToFilters({
      ...filters,
      page,
    })
  }

  function clearFilters() {
    const clearedFilters:
      EvidenceListFilters = {
        search: '',
        custodianId: '',
        integrityStatus: '',
        sortDirection: 'desc',
        page: 1,
        pageSize: 10,
      }

    setDraftSearch('')
    setDraftCustodianId('')

    navigateToFilters(
      clearedFilters,
    )
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
            <strong>
              {session.name}
            </strong>

            <small>
              {session.role}
            </small>
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
        <div className="page-heading">
          <div>
            <p className="eyebrow">
              Gestión de evidencias
            </p>

            <h1>
              Bandeja de evidencias
            </h1>

            <p>
              Consulta evidencias y revisa
              su estado de integridad.
            </p>
          </div>

          {result && (
            <div className="result-counter">
              <strong>
                {result.totalCount}
              </strong>

              <span>
                evidencias
              </span>
            </div>
          )}
        </div>

        <form
          className="filter-panel"
          onSubmit={handleSubmit}
        >
          <label>
            Buscar

            <input
              type="search"
              value={draftSearch}
              onChange={(event) =>
                setDraftSearch(
                  event.target.value,
                )
              }
              placeholder="Código o descripción"
            />
          </label>

          <label>
            Custodio

            <input
              type="text"
              value={draftCustodianId}
              onChange={(event) =>
                setDraftCustodianId(
                  event.target.value,
                )
              }
              placeholder="GUID del custodio"
            />
          </label>

          <label>
            Integridad

            <select
              value={
                filters.integrityStatus
              }
              onChange={(event) =>
                handleStatusChange(
                  event.target.value as
                    | ''
                    | IntegrityStatus,
                )
              }
            >
              <option value="">
                Todos
              </option>

              <option value="Unknown">
                Sin verificar
              </option>

              <option value="Intact">
                Íntegra
              </option>

              <option value="Compromised">
                Comprometida
              </option>
            </select>
          </label>

          <label>
            Orden

            <select
              value={
                filters.sortDirection
              }
              onChange={(event) =>
                handleSortChange(
                  event.target.value as
                    SortDirection,
                )
              }
            >
              <option value="desc">
                Más recientes
              </option>

              <option value="asc">
                Más antiguas
              </option>
            </select>
          </label>

          <div className="filter-actions">
            <button type="submit">
              Aplicar
            </button>

            <button
              className="secondary-button"
              type="button"
              onClick={clearFilters}
            >
              Limpiar
            </button>
          </div>
        </form>

        {error && (
          <div
            className="error-message"
            role="alert"
          >
            {error}
          </div>
        )}

        <div className="table-card">
          {isLoading ? (
            <div className="table-state">
              Cargando evidencias...
            </div>
          ) : result &&
            result.items.length > 0 ? (
            <>
              <div className="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>Código</th>
                      <th>Descripción</th>
                      <th>
                        Custodio actual
                      </th>
                      <th>
                        Último evento
                      </th>
                      <th>Integridad</th>
                      <th>Acción</th>
                    </tr>
                  </thead>

                  <tbody>
                    {result.items.map(
                      (evidence) => (
                        <tr key={evidence.id}>
                          <td>
                            <strong>
                              {evidence.code}
                            </strong>
                          </td>

                          <td>
                            {
                              evidence.description
                            }
                          </td>

                          <td>
                            {
                              evidence.currentCustodianName
                            }
                          </td>

                          <td>
                            {formatDate(
                              evidence.lastEventAtUtc,
                            )}
                          </td>

                          <td>
                            <span
                              className={
                                `integrity-badge ${
                                  evidence.integrityStatus
                                }`
                              }
                            >
                              {getIntegrityLabel(
                                evidence.integrityStatus,
                              )}
                            </span>
                          </td>

                          <td>
                            <button
                              className="table-action"
                              type="button"
                              onClick={() =>
                                onSelectEvidence(
                                  evidence.id,
                                )
                              }
                            >
                              Ver detalle
                            </button>
                          </td>
                        </tr>
                      ),
                    )}
                  </tbody>
                </table>
              </div>

              <footer className="pagination">
                <span>
                  Página {result.page} de{' '}
                  {Math.max(
                    result.totalPages,
                    1,
                  )}
                </span>

                <div>
                  <button
                    className="secondary-button"
                    type="button"
                    disabled={
                      result.page <= 1
                    }
                    onClick={() =>
                      handlePageChange(
                        result.page - 1,
                      )
                    }
                  >
                    Anterior
                  </button>

                  <button
                    className="secondary-button"
                    type="button"
                    disabled={
                      result.totalPages === 0 ||
                      result.page >=
                        result.totalPages
                    }
                    onClick={() =>
                      handlePageChange(
                        result.page + 1,
                      )
                    }
                  >
                    Siguiente
                  </button>
                </div>
              </footer>
            </>
          ) : (
            <div className="table-state">
              No se encontraron evidencias
              con los filtros seleccionados.
            </div>
          )}
        </div>
      </section>
    </main>
  )
}