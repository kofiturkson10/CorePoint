import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from './authContext.js'

// Wraps all pages that require login. Renders the matching child page (<Outlet />)
// only when the user is logged in, otherwise redirects to /login.
function ProtectedRoute() {
  const { user, isLoading } = useAuth()
  const location = useLocation()

  // Wait for the "who am I?" check so a refresh on /employees doesn't bounce to /login
  if (isLoading) {
    return <p>Laddar...</p>
  }

  if (!user) {
    // Remember where the user wanted to go, so we can send them back after login
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return <Outlet />
}

export default ProtectedRoute
