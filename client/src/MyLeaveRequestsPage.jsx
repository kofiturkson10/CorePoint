import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { readErrorMessage } from './apiErrors.js'
import LeaveRequestForm from './LeaveRequestForm.jsx'
import { leaveRequestStatusLabel } from './leaveRequestStatus.js'

function MyLeaveRequestsPage() {
  const [leaveRequests, setLeaveRequests] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)

  // Increasing this number makes the effect below fetch the list again
  const [reloadCount, setReloadCount] = useState(0)

  // Fetches the list on page load and again after every new application (reloadCount),
  // so the table always shows what is really in the database.
  useEffect(() => {
    async function fetchMine() {
      try {
        const response = await fetch('/api/leaverequests/mine')
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

    fetchMine()
  }, [reloadCount])

  function handleCreated() {
    setReloadCount((count) => count + 1)
  }

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Mina ansökningar</h2>
      <p>
        <Link to="/dashboard">Tillbaka till dashboarden</Link>
      </p>

      <LeaveRequestForm onCreated={handleCreated} />

      <h3>Tidigare ansökningar</h3>
      {isLoading && <p>Laddar...</p>}
      {error && <p role="alert">{error}</p>}
      {!isLoading && !error && leaveRequests.length === 0 && <p>Du har inga ansökningar än.</p>}

      {leaveRequests.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Startdatum</th>
              <th>Slutdatum</th>
              <th>Anledning</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {leaveRequests.map((leaveRequest) => (
              <tr key={leaveRequest.id}>
                <td>{new Date(leaveRequest.startDate).toLocaleDateString('sv-SE')}</td>
                <td>{new Date(leaveRequest.endDate).toLocaleDateString('sv-SE')}</td>
                <td>{leaveRequest.reason || '–'}</td>
                <td>{leaveRequestStatusLabel(leaveRequest.status)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </main>
  )
}

export default MyLeaveRequestsPage
