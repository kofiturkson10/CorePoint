import { useEffect, useState } from 'react'
import { readErrorMessage } from './apiErrors.js'

function BookingForm({ onCreated }) {
  const [rooms, setRooms] = useState([])
  const [roomId, setRoomId] = useState('')
  const [startTime, setStartTime] = useState('')
  const [endTime, setEndTime] = useState('')
  const [error, setError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  // Fetched once on mount to fill the room dropdown - rooms are managed on a separate page.
  useEffect(() => {
    async function fetchRooms() {
      try {
        const response = await fetch('/api/rooms')
        if (!response.ok) {
          return
        }
        const data = await response.json()
        setRooms(data)
        if (data.length > 0) {
          setRoomId(String(data[0].id))
        }
      } catch {
        // Dropdown just stays empty - the booking list below still works
      }
    }

    fetchRooms()
  }, [])

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)

    // Same rule as the backend (StartTime must be before EndTime) - checked here too,
    // so the user gets immediate feedback instead of a round-trip to the server.
    if (new Date(startTime) >= new Date(endTime)) {
      setError('Sluttid måste vara efter starttid.')
      return
    }

    setIsSubmitting(true)

    try {
      const response = await fetch('/api/bookings', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ roomId: Number(roomId), startTime, endTime }),
      })

      if (!response.ok) {
        throw new Error(await readErrorMessage(response))
      }

      setStartTime('')
      setEndTime('')
      onCreated()
    } catch (err) {
      setError(err.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h3>Ny bokning</h3>
      <p>
        <label>
          Rum
          <br />
          <select value={roomId} onChange={(event) => setRoomId(event.target.value)} required>
            {rooms.length === 0 && <option value="">Inga rum tillgängliga</option>}
            {rooms.map((room) => (
              <option key={room.id} value={room.id}>
                {room.name} ({room.capacity} platser)
              </option>
            ))}
          </select>
        </label>
      </p>
      <p>
        <label>
          Starttid
          <br />
          <input
            type="datetime-local"
            value={startTime}
            onChange={(event) => setStartTime(event.target.value)}
            required
          />
        </label>
      </p>
      <p>
        <label>
          Sluttid
          <br />
          <input
            type="datetime-local"
            value={endTime}
            onChange={(event) => setEndTime(event.target.value)}
            required
          />
        </label>
      </p>
      {error && <p role="alert">{error}</p>}
      <button type="submit" disabled={isSubmitting || rooms.length === 0}>
        {isSubmitting ? 'Bokar...' : 'Boka rum'}
      </button>
    </form>
  )
}

export default BookingForm
