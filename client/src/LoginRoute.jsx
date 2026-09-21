import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from './authContext.js'
import LoginPage from './LoginPage.jsx'

// The /login route: shows the login form, but sends already logged-in users onwards.
// This is also what moves the user away from /login right after a successful login.
function LoginRoute() {
  const { user, isLoading } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return <p>Laddar...</p>
  }

  if (user) {
    const destination = location.state?.from?.pathname ?? '/dashboard'
    return <Navigate to={destination} replace />
  }

  return <LoginPage />
}

export default LoginRoute
