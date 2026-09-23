export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResult {
  accessToken: string
  tokenType: string
  userId: string
  name: string
  email: string
  role: string
}

export type AuthSession = Omit<
  LoginResult,
  'accessToken' | 'tokenType'
>