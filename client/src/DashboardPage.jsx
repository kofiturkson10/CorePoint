import { useAuth } from './authContext.js'

function DashboardPage({ onShowEmployees }) {
  const { user, logout } = useAuth()

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Dashboard</h2>
      <p>Välkommen, {user.displayName}!</p>
      <p>Inloggad som {user.email}</p>
      <p>
        <button type="button" onClick={onShowEmployees}>
          Visa medarbetare
        </button>
      </p>
      <button type="button" onClick={logout}>
        Logga ut
      </button>
    </main>
  )
}

export default DashboardPage
