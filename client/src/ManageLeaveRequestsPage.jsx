import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { readErrorMessage } from './apiErrors.js'
import { useAuth } from './authContext.js'
import { LEAVE_REQUEST_STATUS, leaveRequestStatusLabel } from './leaveRequestStatus.js'

// Only Admin/HR may use this page - see the App.jsx / DashboardPage.jsx RBAC notes.
function ManageLeaveRequestsPage() {
  const { user } = useAuth()
  const canManage = user?.role === 'Admin' || user?.role === 'HR'

  const [leaveRequests, setLeaveRequests] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)
  const [reviewingId, setReviewingId] = useState(null)

  // Increasing this number makes the effect below fetch the list again
  const [reloadCount, setReloadCount] = useState(0)

  useEffect(() => {
    // Not Admin/HR: the backend would answer 403 anyway, so skip the call entirely.
    // The component returns the access-denied message below before this ever matters.
    if (!canManage) {
      return
    }

    async function fetchAll() {
      try {
        const response = await fetch('/api/leaverequests')
        if (!response.ok) {
          throw new Error(await readErrorMessage(response))
        }
        setLeaveRequests(await response.json())
        setError(null)
      } catch (err) {
        setError(err.message)
      } finally {
        setIsLoading(false)
      }
    }

    fetchAll()
  }, [canManage, reloadCount])

  async function handleReview(leaveRequest, action) {
    setError(null)
    setReviewingId(leaveRequest.id)

    try {
      const response = await fetch(`/api/leaverequests/${leaveRequest.id}/${action}`, {
        method: 'PUT',
      })
      if (!response.ok) {
        throw new Error(await readErrorMessage(response))
      }
      // Refetches the list, so the new status is shown without a manual page reload
      setReloadCount((count) => count + 1)
    } catch (err) {
      setError(err.message)
    } finally {
      setReviewingId(null)
    }
  }

  if (!canManage) {
    return (
      <main>
        <h1>Företagsportal</h1>
        <h2>Hantera ansökningar</h2>
        <p role="alert">Du har inte behörighet att se den här sidan.</p>
        <p>
          <Link to="/dashboard">Tillbaka till dashboarden</Link>
        </p>
      </main>
    )
  }

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Hantera ansökningar</h2>
      <p>
        <Link to="/dashboard">Tillbaka till dashboarden</Link>
      </p>

      {isLoading && <p>Laddar...</p>}
      {error && <p role="alert">{error}</p>}
      {!isLoading && !error && leaveRequests.length === 0 && <p>Inga ansökningar hittades.</p>}

      {leaveRequests.length > 0 && (
        <table>
          <thead>
            <tr>
              {/* No name is shown: the API only exposes the applicant's user id, not a display name */}
              <th>Sökande (användar-ID)</th>
              <th>Startdatum</th>
              <th>Slutdatum</th>
              <th>Anledning</th>
              <th>Status</th>
              <th>Åtgärder</th>
            </tr>
          </thead>
          <tbody>
            {leaveRequests.map((leaveRequest) => {
              const isPending = leaveRequest.status === LEAVE_REQUEST_STATUS.PENDING
              return (
                <tr key={leaveRequest.id}>
                  <td>{leaveRequest.employeeId}</td>
                  <td>{new Date(leaveRequest.startDate).toLocaleDateString('sv-SE')}</td>
                  <td>{new Date(leaveRequest.endDate).toLocaleDateString('sv-SE')}</td>
                  <td>{leaveRequest.reason || '–'}</td>
                  <td>{leaveRequestStatusLabel(leaveRequest.status)}</td>
                  <td>
                    <button
                      type="button"
                      onClick={() => handleReview(leaveRequest, 'approve')}
                      disabled={!isPending || reviewingId === leaveRequest.id}
                    >
                      Godkänn
                    </button>{' '}
                    <button
                      type="button"
                      onClick={() => handleReview(leaveRequest, 'reject')}
                      disabled={!isPending || reviewingId === leaveRequest.id}
                    >
                      Avslå
                    </button>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      )}
    </main>
  )
}

export default ManageLeaveRequestsPage
