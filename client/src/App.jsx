import { Navigate, Route, Routes } from 'react-router-dom'
import DashboardPage from './DashboardPage.jsx'
import DocumentListPage from './DocumentListPage.jsx'
import EmployeeListPage from './EmployeeListPage.jsx'
import LoginRoute from './LoginRoute.jsx'
import NewsListPage from './NewsListPage.jsx'
import ProtectedRoute from './ProtectedRoute.jsx'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginRoute />} />

      {/* Everything inside this route requires login */}
      <Route element={<ProtectedRoute />}>
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/employees" element={<EmployeeListPage />} />
        <Route path="/news" element={<NewsListPage />} />
        <Route path="/documents" element={<DocumentListPage />} />
      </Route>

      {/* "/" and any unknown address go to the dashboard (which redirects to /login if needed) */}
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  )
}

export default App
