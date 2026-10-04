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
    <form className="card" onSubmit={handleSubmit} style={{ marginBottom: 'var(--space-4)' }}>
      <h3>Ny ansökan</h3>
      <div className="field">
        <label className="field-label" htmlFor="leave-start-date">
          Startdatum
        </label>
        <input
          className="input"
          id="leave-start-date"
          type="date"
          value={startDate}
          onChange={(event) => setStartDate(event.target.value)}
          required
        />
      </div>
      <div className="field">
        <label className="field-label" htmlFor="leave-end-date">
          Slutdatum
        </label>
        <input
          className="input"
          id="leave-end-date"
          type="date"
          value={endDate}
          onChange={(event) => setEndDate(event.target.value)}
          required
        />
      </div>
      <div className="field">
        <label className="field-label" htmlFor="leave-reason">
          Anledning (valfritt)
        </label>
        <textarea
          className="input"
          id="leave-reason"
          value={reason}
          onChange={(event) => setReason(event.target.value)}
          maxLength={1000}
          rows={3}
        />
        <span className="field-hint">Max 1000 tecken</span>
      </div>
      {error && (
        <p className="alert alert-error" role="alert">
          {error}
        </p>
      )}
      <button className="btn btn-primary" type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Skickar...' : 'Skicka ansökan'}
      </button>
    </form>
  )
}

export default LeaveRequestForm
