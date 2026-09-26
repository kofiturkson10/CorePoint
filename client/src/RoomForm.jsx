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
    <form onSubmit={handleSubmit}>
      <h3>Nytt rum</h3>
      <p>
        <label>
          Namn
          <br />
          <input
            value={name}
            onChange={(event) => setName(event.target.value)}
            required
            maxLength={100}
          />
        </label>
      </p>
      <p>
        <label>
          Kapacitet
          <br />
          <input
            type="number"
            min="1"
            value={capacity}
            onChange={(event) => setCapacity(event.target.value)}
            required
          />
        </label>
      </p>
      <p>
        <label>
          Plats (valfritt)
          <br />
          <input
            value={location}
            onChange={(event) => setLocation(event.target.value)}
            maxLength={200}
          />
        </label>
      </p>
      {error && <p role="alert">{error}</p>}
      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Sparar...' : 'Spara'}
      </button>
    </form>
  )
}

export default RoomForm
