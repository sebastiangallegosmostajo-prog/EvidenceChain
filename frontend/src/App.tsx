import {
  useCallback,
  useEffect,
  useState,
} from 'react'
import './App.css'
import {
  EvidenceDetailPage,
} from './pages/EvidenceDetailPage'
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

const listUrlKey =
  'evidencechain.listUrl'

function getEvidenceIdFromPath():
  string | null {
  const match =
    window.location.pathname.match(
      /^\/evidence\/([0-9a-f-]{36})\/?$/i,
    )

  return match?.[1] ?? null
}

function App() {
  const [session, setSession] =
    useState<AuthSession | null>(
      getStoredSession,
    )

  const [
    selectedEvidenceId,
    setSelectedEvidenceId,
  ] = useState<string | null>(
    getEvidenceIdFromPath,
  )

  useEffect(() => {
    const handlePopState = () => {
      setSelectedEvidenceId(
        getEvidenceIdFromPath(),
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

  const handleLogout =
    useCallback(() => {
      clearSession()
      setSession(null)
      setSelectedEvidenceId(null)

      window.history.replaceState(
        {},
        '',
        '/',
      )
    }, [])

  const handleSelectEvidence =
    useCallback((evidenceId: string) => {
      sessionStorage.setItem(
        listUrlKey,
        `${window.location.pathname}${window.location.search}`,
      )

      window.history.pushState(
        {},
        '',
        `/evidence/${evidenceId}`,
      )

      setSelectedEvidenceId(
        evidenceId,
      )
    }, [])

  const handleBackToList =
    useCallback(() => {
      const listUrl =
        sessionStorage.getItem(
          listUrlKey,
        ) ?? '/'

      window.history.pushState(
        {},
        '',
        listUrl,
      )

      setSelectedEvidenceId(null)
    }, [])

  if (!session) {
    return (
      <LoginPage
        onLogin={setSession}
      />
    )
  }

  if (selectedEvidenceId) {
    return (
      <EvidenceDetailPage
        evidenceId={selectedEvidenceId}
        session={session}
        onBack={handleBackToList}
        onLogout={handleLogout}
      />
    )
  }

  return (
    <EvidenceListPage
      session={session}
      onLogout={handleLogout}
      onSelectEvidence={
        handleSelectEvidence
      }
    />
  )
}

export default App