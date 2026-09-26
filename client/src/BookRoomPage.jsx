import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { readErrorMessage } from './apiErrors.js'
import { useAuth } from './authContext.js'
import BookingForm from './BookingForm.jsx'

function formatDateTime(value) {
  return new Date(value).toLocaleString('sv-SE', { dateStyle: 'short', timeStyle: 'short' })
}

function BookRoomPage() {
  const { user } = useAuth()
  const [bookings, setBookings] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)
  const [cancelingId, setCancelingId] = useState(null)

  // Increasing this number makes the effect below fetch the list again
  const [reloadCount, setReloadCount] = useState(0)

  // Fetches every booking (not just the user's own) so people can see what's already
  // taken, on page load and again after every change (reloadCount).
  useEffect(() => {
    async function fetchBookings() {
      try {
        const response = await fetch('/api/bookings')
        if (!response.ok) {
          throw new Error(await readErrorMessage(response))
        }
        setBookings(await response.json())
        setError(null)
      } catch (err) {
        setError(err.message)
      } finally {
        setIsLoading(false)
      }
    }

    fetchBookings()
  }, [reloadCount])

  function handleCreated() {
    setReloadCount((count) => count + 1)
  }

  async function handleCancel(booking) {
    if (!window.confirm(`Avboka ${booking.roomName ?? 'rummet'} (${formatDateTime(booking.startTime)})?`)) {
      return
    }

    setError(null)
    setCancelingId(booking.id)

    try {
      const response = await fetch(`/api/bookings/${booking.id}`, { method: 'DELETE' })
      // 404 means it was already canceled elsewhere - the refresh below shows the real state
      if (!response.ok && response.status !== 404) {
        throw new Error(await readErrorMessage(response))
      }
      setReloadCount((count) => count + 1)
    } catch (err) {
      setError(err.message)
    } finally {
      setCancelingId(null)
    }
  }

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Boka rum</h2>
      <p>
        <Link to="/dashboard">Tillbaka till dashboarden</Link>
      </p>

      <BookingForm onCreated={handleCreated} />

      <h3>Alla bokningar</h3>
      {isLoading && <p>Laddar...</p>}
      {error && <p role="alert">{error}</p>}
      {!isLoading && !error && bookings.length === 0 && <p>Inga bokningar hittades.</p>}

      {bookings.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Rum</th>
              <th>Start</th>
              <th>Slut</th>
              <th>Bokad av</th>
              <th>Åtgärder</th>
            </tr>
          </thead>
          <tbody>
            {bookings.map((booking) => {
              // A booking is "mine" if its requester's email matches the logged-in user's -
              // the API only exposes ids, not who "I" am as an id, so email is the shared key.
              const isOwnBooking = booking.requesterEmail && booking.requesterEmail === user?.email
              return (
                <tr key={booking.id}>
                  <td>{booking.roomName ?? '(okänt rum)'}</td>
                  <td>{formatDateTime(booking.startTime)}</td>
                  <td>{formatDateTime(booking.endTime)}</td>
                  <td>{booking.requesterEmail || '(okänd användare)'}</td>
                  <td>
                    {isOwnBooking && (
                      <button
                        type="button"
                        onClick={() => handleCancel(booking)}
                        disabled={cancelingId === booking.id}
                      >
                        Avboka
                      </button>
                    )}
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

export default BookRoomPage
