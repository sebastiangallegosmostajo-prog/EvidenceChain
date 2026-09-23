import {
  useState,
} from 'react'
import './App.css'
import {
  LoginPage,
} from './pages/LoginPage'
import {
  clearSession,
  getStoredSession,
} from './services/authService'
import type {
  AuthSession,
} from './types/auth'

function App() {
  const [session, setSession] =
    useState<AuthSession | null>(
      getStoredSession,
    )

  function handleLogout() {
    clearSession()
    setSession(null)
  }

  if (!session) {
    return (
      <LoginPage
        onLogin={setSession}
      />
    )
  }

  return (
    <main className="authenticated-layout">
      <header className="application-header">
        <div>
          <span className="brand-mark small">
            EC
          </span>

          <strong>EvidenceChain</strong>
        </div>

        <button
          className="secondary-button"
          type="button"
          onClick={handleLogout}
        >
          Cerrar sesión
        </button>
      </header>

      <section className="welcome-card">
        <p className="eyebrow">
          Sesión iniciada
        </p>

        <h1>
          Bienvenido, {session.name}
        </h1>

        <p>
          Tu perfil es{' '}
          <strong>{session.role}</strong>.
        </p>

        <p>
          En el siguiente paso construiremos
          aquí la bandeja de evidencias.
        </p>
      </section>
    </main>
  )
}

export default App