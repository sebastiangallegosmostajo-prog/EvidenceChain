import {
  useState,
  type FormEvent,
} from 'react'
import {
  ApiError,
} from '../services/api'
import {
  login,
} from '../services/authService'
import type {
  AuthSession,
} from '../types/auth'

interface LoginPageProps {
  onLogin: (session: AuthSession) => void
}

export function LoginPage({
  onLogin,
}: LoginPageProps) {
  const [email, setEmail] =
    useState('')

  const [password, setPassword] =
    useState('')

  const [error, setError] =
    useState<string | null>(null)

  const [isSubmitting, setIsSubmitting] =
    useState(false)

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    setError(null)
    setIsSubmitting(true)

    try {
      const session =
        await login({
          email: email.trim(),
          password,
        })

      onLogin(session)
    } catch (exception) {
      if (exception instanceof ApiError) {
        setError(exception.message)
      } else {
        setError(
          'Ocurrió un error inesperado.',
        )
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login-layout">
      <section className="login-introduction">
        <span className="brand-mark">
          EC
        </span>

        <p className="eyebrow">
          Gestión forense
        </p>

        <h1>
          Cadena de custodia digital
        </h1>

        <p className="introduction-copy">
          Administra evidencias, transferencias
          y verificaciones de integridad desde
          una plataforma segura y trazable.
        </p>
      </section>

      <section className="login-panel">
        <form
          className="login-card"
          onSubmit={handleSubmit}
        >
          <div>
            <p className="eyebrow">
              EvidenceChain
            </p>

            <h2>Iniciar sesión</h2>

            <p className="form-description">
              Ingresa tus credenciales para
              acceder al sistema.
            </p>
          </div>

          <label>
            Correo electrónico

            <input
              type="email"
              value={email}
              onChange={(event) =>
                setEmail(event.target.value)
              }
              autoComplete="email"
              placeholder="usuario@evidencechain.local"
              required
            />
          </label>

          <label>
            Contraseña

            <input
              type="password"
              value={password}
              onChange={(event) =>
                setPassword(event.target.value)
              }
              autoComplete="current-password"
              placeholder="Ingresa tu contraseña"
              required
            />
          </label>

          {error && (
            <div
              className="error-message"
              role="alert"
            >
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={isSubmitting}
          >
            {isSubmitting
              ? 'Ingresando...'
              : 'Ingresar'}
          </button>
        </form>
      </section>
    </main>
  )
}