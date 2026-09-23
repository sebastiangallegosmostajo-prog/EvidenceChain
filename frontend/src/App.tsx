import {
  useCallback,
  useState,
} from 'react'
import './App.css'
import {
  EvidenceListPage,
} from './pages/EvidenceListPage'
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

  const handleLogout =
    useCallback(() => {
      clearSession()
      setSession(null)

      window.history.replaceState(
        {},
        '',
        window.location.pathname,
      )
    }, [])

  if (!session) {
    return (
      <LoginPage
        onLogin={setSession}
      />
    )
  }

  return (
    <EvidenceListPage
      session={session}
      onLogout={handleLogout}
    />
  )
}

export default App