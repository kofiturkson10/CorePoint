import { Link } from 'react-router-dom'
import { useAuth } from './authContext.js'

function DashboardPage() {
  const { user, logout } = useAuth()

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Dashboard</h2>

      <section className="card" style={{ marginBottom: 'var(--space-5)' }}>
        <p style={{ margin: 0, fontSize: 'var(--font-size-lg)', fontWeight: 'var(--font-weight-semibold)' }}>
          Välkommen, {user.displayName}!
        </p>
        <p style={{ margin: 'var(--space-1) 0 0', fontSize: 'var(--font-size-sm)', color: 'var(--color-text-muted)' }}>
          Inloggad som {user.email}
        </p>
      </section>

      <nav aria-label="Huvudmeny">
        <ul className="nav-grid">
          <li>
            <Link className="nav-card" to="/employees">Visa medarbetare</Link>
          </li>
          <li>
            <Link className="nav-card" to="/news">Visa nyheter</Link>
          </li>
          <li>
            <Link className="nav-card" to="/documents">Visa dokument</Link>
          </li>
          <li>
            <Link className="nav-card" to="/leave-requests">Mina ansökningar</Link>
          </li>
          {(user.role === 'Admin' || user.role === 'HR') && (
            <li>
              <Link className="nav-card" to="/leave-requests/manage">Hantera ansökningar</Link>
            </li>
          )}
          <li>
            <Link className="nav-card" to="/bookings">Boka rum</Link>
          </li>
          {user.role === 'Admin' && (
            <li>
              <Link className="nav-card" to="/rooms">Hantera rum</Link>
            </li>
          )}
        </ul>
      </nav>

      <button type="button" className="btn btn-secondary" onClick={logout}>
        Logga ut
      </button>
    </main>
  )
}

export default DashboardPage
