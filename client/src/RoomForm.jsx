import { useState } from 'react'
import { readErrorMessage } from './apiErrors.js'

function RoomForm({ onCreated }) {
  const [name, setName] = useState('')
  const [capacity, setCapacity] = useState('')
  const [location, setLocation] = useState('')
  const [error, setError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)
    setIsSubmitting(true)

    try {
      const response = await fetch('/api/rooms', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, capacity: Number(capacity), location }),
      })

      if (!response.ok) {
        throw new Error(await readErrorMessage(response))
      }

      setName('')
      setCapacity('')
      setLocation('')
      onCreated()
    } catch (err) {
      setError(err.message)
      setIsSubmitting(false)
    }
  }

  return (
    <form className="card" style={{ marginBottom: 'var(--space-5)' }} onSubmit={handleSubmit}>
      <h3>Nytt rum</h3>
      <div className="field">
        <label className="field-label" htmlFor="room-name">
          Namn
        </label>
        <input
          id="room-name"
          className="input"
          value={name}
          onChange={(event) => setName(event.target.value)}
          required
          maxLength={100}
        />
      </div>
      <div className="field">
        <label className="field-label" htmlFor="room-capacity">
          Kapacitet
        </label>
        <input
          id="room-capacity"
          className="input"
          type="number"
          min="1"
          value={capacity}
          onChange={(event) => setCapacity(event.target.value)}
          required
        />
      </div>
      <div className="field">
        <label className="field-label" htmlFor="room-location">
          Plats (valfritt)
        </label>
        <input
          id="room-location"
          className="input"
          value={location}
          onChange={(event) => setLocation(event.target.value)}
          maxLength={200}
        />
      </div>
      {error && (
        <p className="alert alert-error" role="alert">
          {error}
        </p>
      )}
      <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
        {isSubmitting ? 'Sparar...' : 'Spara'}
      </button>
    </form>
  )
}

export default RoomForm
