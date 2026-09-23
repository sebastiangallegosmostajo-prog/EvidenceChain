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
  PendingTransfersPage,
} from './pages/PendingTransfersPage'
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

function isPendingTransfersPath():
  boolean {
  return window.location.pathname ===
    '/transfers/pending'
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

  const [
    showingPendingTransfers,
    setShowingPendingTransfers,
  ] = useState(
    isPendingTransfersPath,
  )

  const synchronizeRoute =
    useCallback(() => {
      setSelectedEvidenceId(
        getEvidenceIdFromPath(),
      )

      setShowingPendingTransfers(
        isPendingTransfersPath(),
      )
    }, [])

  useEffect(() => {
    window.addEventListener(
      'popstate',
      synchronizeRoute,
    )

    return () => {
      window.removeEventListener(
        'popstate',
        synchronizeRoute,
      )
    }
  }, [synchronizeRoute])

  const handleLogin =
    useCallback(
      (authenticatedSession:
        AuthSession) => {
        setSession(
          authenticatedSession,
        )

        if (
          authenticatedSession.role ===
          'Custodian'
        ) {
          window.history.replaceState(
            {},
            '',
            '/transfers/pending',
          )
        } else {
          window.history.replaceState(
            {},
            '',
            '/',
          )
        }

        synchronizeRoute()
      },
      [synchronizeRoute],
    )

  const handleLogout =
    useCallback(() => {
      clearSession()
      setSession(null)

      window.history.replaceState(
        {},
        '',
        '/',
      )

      synchronizeRoute()
    }, [synchronizeRoute])

  const handleSelectEvidence =
    useCallback(
      (evidenceId: string) => {
        sessionStorage.setItem(
          listUrlKey,
          `${window.location.pathname}${window.location.search}`,
        )

        window.history.pushState(
          {},
          '',
          `/evidence/${evidenceId}`,
        )

        synchronizeRoute()
      },
      [synchronizeRoute],
    )

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

      synchronizeRoute()
    }, [synchronizeRoute])

  const handleOpenPendingTransfers =
    useCallback(() => {
      window.history.pushState(
        {},
        '',
        '/transfers/pending',
      )

      synchronizeRoute()
    }, [synchronizeRoute])

  const handleOpenEvidences =
    useCallback(() => {
      window.history.pushState(
        {},
        '',
        '/',
      )

      synchronizeRoute()
    }, [synchronizeRoute])

  if (!session) {
    return (
      <LoginPage
        onLogin={handleLogin}
      />
    )
  }

  if (
    showingPendingTransfers &&
    session.role === 'Custodian'
  ) {
    return (
      <PendingTransfersPage
        session={session}
        onOpenEvidences={
          handleOpenEvidences
        }
        onLogout={handleLogout}
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
      onOpenPendingTransfers={
        session.role === 'Custodian'
          ? handleOpenPendingTransfers
          : undefined
      }
    />
  )
}

export default App