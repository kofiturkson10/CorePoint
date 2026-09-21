import { useState } from 'react'
import { useAuth } from './authContext.js'
import DashboardPage from './DashboardPage.jsx'
import EmployeeListPage from './EmployeeListPage.jsx'
import LoginPage from './LoginPage.jsx'

function App() {
  const { user, isLoading } = useAuth()
  // Which page to show when logged in. A simple state is enough for two pages;
  // a router (react-router) becomes useful when there are more pages.
  const [page, setPage] = useState('dashboard')

  // Avoid flashing the login form while we check if the user is already logged in
  if (isLoading) {
    return <p>Laddar...</p>
  }

  if (!user) {
    return <LoginPage />
  }

  if (page === 'employees') {
    return <EmployeeListPage onBack={() => setPage('dashboard')} />
  }

  return <DashboardPage onShowEmployees={() => setPage('employees')} />
}

export default App
