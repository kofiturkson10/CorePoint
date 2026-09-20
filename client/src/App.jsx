import { useAuth } from './authContext.js'
import DashboardPage from './DashboardPage.jsx'
import LoginPage from './LoginPage.jsx'

function App() {
  const { user, isLoading } = useAuth()

  // Avoid flashing the login form while we check if the user is already logged in
  if (isLoading) {
    return <p>Laddar...</p>
  }

  return user ? <DashboardPage /> : <LoginPage />
}

export default App
