import { useState } from 'react'
import { readErrorMessage } from './apiErrors.js'

// Only for creating a new leave request - there is no editing afterwards, only
// Admin/HR approving or rejecting it (see ManageLeaveRequestsPage).
function LeaveRequestForm({ onCreated }) {
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [reason, setReason] = useState('')
  const [error, setError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)

    // Same rule as the backend (StartDate must be before EndDate) - checked here too,
    // so the user gets immediate feedback instead of a round-trip to the server.
    if (new Date(startDate) >= new Date(endDate)) {
      setError('Slutdatum måste vara efter startdatum.')
      return
    }

    setIsSubmitting(true)

    try {
      const response = await fetch('/api/leaverequests', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ startDate, endDate, reason }),
      })

      if (!response.ok) {
        throw new Error(await readErrorMessage(response))
      }

      setStartDate('')
      setEndDate('')
      setReason('')
      onCreated()
    } catch (err) {
      setError(err.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h3>Ny ansökan</h3>
      <p>
        <label>
          Startdatum
          <br />
          <input
            type="date"
            value={startDate}
            onChange={(event) => setStartDate(event.target.value)}
            required
          />
        </label>
      </p>
      <p>
        <label>
          Slutdatum
          <br />
          <input
            type="date"
            value={endDate}
            onChange={(event) => setEndDate(event.target.value)}
            required
          />
        </label>
      </p>
      <p>
        <label>
          Anledning (valfritt)
          <br />
          <textarea
            value={reason}
            onChange={(event) => setReason(event.target.value)}
            maxLength={1000}
            rows={3}
            cols={50}
          />
        </label>
      </p>
      {error && <p role="alert">{error}</p>}
      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Skickar...' : 'Skicka ansökan'}
      </button>
    </form>
  )
}

export default LeaveRequestForm
