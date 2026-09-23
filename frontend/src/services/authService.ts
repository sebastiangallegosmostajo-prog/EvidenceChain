import {
  apiRequest,
} from './api'
import type {
  AuthSession,
  LoginRequest,
  LoginResult,
} from '../types/auth'

const accessTokenKey =
  'evidencechain.accessToken'

const sessionKey =
  'evidencechain.session'

export async function login(
  request: LoginRequest,
): Promise<AuthSession> {
  const result =
    await apiRequest<LoginResult>(
      '/api/v1/auth/login',
      {
        method: 'POST',
        body: JSON.stringify(request),
      },
    )

  const session: AuthSession = {
    userId: result.userId,
    name: result.name,
    email: result.email,
    role: result.role,
  }

  sessionStorage.setItem(
    accessTokenKey,
    result.accessToken,
  )

  sessionStorage.setItem(
    sessionKey,
    JSON.stringify(session),
  )

  return session
}

export function getStoredSession():
  AuthSession | null {
  const serializedSession =
    sessionStorage.getItem(sessionKey)

  if (!serializedSession) {
    return null
  }

  try {
    return JSON.parse(
      serializedSession,
    ) as AuthSession
  } catch {
    clearSession()

    return null
  }
}

export function clearSession(): void {
  sessionStorage.removeItem(accessTokenKey)
  sessionStorage.removeItem(sessionKey)
}