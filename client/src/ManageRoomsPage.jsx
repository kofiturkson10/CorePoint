import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { readErrorMessage } from './apiErrors.js'
import { useAuth } from './authContext.js'
import RoomForm from './RoomForm.jsx'

// Only Admin may use this page - see the App.jsx / DashboardPage.jsx RBAC notes.
function ManageRoomsPage() {
  const { user } = useAuth()
  const canManage = user?.role === 'Admin'

  const [rooms, setRooms] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)
  const [deletingId, setDeletingId] = useState(null)

  // Increasing this number makes the effect below fetch the list again
  const [reloadCount, setReloadCount] = useState(0)

  useEffect(() => {
    // Not Admin: the backend would answer 403 anyway, so skip the call entirely.
    // The component returns the access-denied message below before this ever matters.
    if (!canManage) {
      return
    }

    async function fetchRooms() {
      try {
        const response = await fetch('/api/rooms')
        if (!response.ok) {
          throw new Error(await readErrorMessage(response))
        }
        setRooms(await response.json())
        setError(null)
      } catch (err) {
        setError(err.message)
      } finally {
        setIsLoading(false)
      }
    }

    fetchRooms()
  }, [canManage, reloadCount])

  function handleCreated() {
    setReloadCount((count) => count + 1)
  }

  async function handleDelete(room) {
    if (!window.confirm(`Ta bort ${room.name}?`)) {
      return
    }

    setError(null)
    setDeletingId(room.id)

    try {
      const response = await fetch(`/api/rooms/${room.id}`, { method: 'DELETE' })
      // 404 means it was already deleted elsewhere - the refresh below shows the real state
      if (!response.ok && response.status !== 404) {
        throw new Error(await readErrorMessage(response))
      }
      setReloadCount((count) => count + 1)
    } catch (err) {
      setError(err.message)
    } finally {
      setDeletingId(null)
    }
  }

  if (!canManage) {
    return (
      <main>
        <h1>Företagsportal</h1>
        <h2>Hantera rum</h2>
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
      <h2>Hantera rum</h2>
      <p>
        <Link to="/dashboard">Tillbaka till dashboarden</Link>
      </p>

      <RoomForm onCreated={handleCreated} />

      {isLoading && <p>Laddar...</p>}
      {error && <p role="alert">{error}</p>}
      {!isLoading && !error && rooms.length === 0 && <p>Inga rum hittades.</p>}

      {rooms.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Namn</th>
              <th>Kapacitet</th>
              <th>Plats</th>
              <th>Åtgärder</th>
            </tr>
          </thead>
          <tbody>
            {rooms.map((room) => (
              <tr key={room.id}>
                <td>{room.name}</td>
                <td>{room.capacity}</td>
                <td>{room.location || '–'}</td>
                <td>
                  <button
                    type="button"
                    onClick={() => handleDelete(room)}
                    disabled={deletingId === room.id}
                  >
                    Ta bort
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </main>
  )
}

export default ManageRoomsPage
