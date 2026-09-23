const configuredBaseUrl =
  import.meta.env.VITE_API_BASE_URL

if (!configuredBaseUrl) {
  throw new Error(
    'La variable VITE_API_BASE_URL no está configurada.',
  )
}

export const API_BASE_URL =
  configuredBaseUrl.replace(/\/$/, '')

interface ApiProblem {
  status?: number
  title?: string
  detail?: string
  traceId?: string
  currentState?: unknown
}

export class ApiError extends Error {
  readonly status: number
  readonly problem?: ApiProblem

  constructor(
    message: string,
    status: number,
    problem?: ApiProblem,
  ) {
    super(message)

    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

export async function apiRequest<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const headers =
    new Headers(options.headers)

  if (
    options.body &&
    !headers.has('Content-Type')
  ) {
    headers.set(
      'Content-Type',
      'application/json',
    )
  }

  const accessToken =
    sessionStorage.getItem(
      'evidencechain.accessToken',
    )

  if (
    accessToken &&
    !headers.has('Authorization')
  ) {
    headers.set(
      'Authorization',
      `Bearer ${accessToken}`,
    )
  }

  let response: Response

  try {
    response = await fetch(
      `${API_BASE_URL}${path}`,
      {
        ...options,
        headers,
      },
    )
  } catch {
    throw new ApiError(
      'No fue posible conectarse con la API.',
      0,
    )
  }

  if (!response.ok) {
    let problem: ApiProblem | undefined

    try {
      problem =
        (await response.json()) as ApiProblem
    } catch {
      problem = undefined
    }

    throw new ApiError(
      problem?.detail ??
        problem?.title ??
        'La solicitud no pudo completarse.',
      response.status,
      problem,
    )
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}