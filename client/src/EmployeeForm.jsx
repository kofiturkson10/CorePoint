import { useState } from 'react'
import { readErrorMessage } from './apiErrors.js'

// Used for both creating (employee = null) and editing (employee = the row being edited)
function EmployeeForm({ employee, onSaved, onCancel }) {
  const isEditing = employee !== null
  const [name, setName] = useState(employee?.name ?? '')
  const [department, setDepartment] = useState(employee?.department ?? '')
  const [role, setRole] = useState(employee?.role ?? '')
  const [email, setEmail] = useState(employee?.email ?? '')
  const [error, setError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)
    setIsSubmitting(true)

    try {
      // Create = POST /api/employees, edit = PUT /api/employees/{id}
      const response = await fetch(isEditing ? `/api/employees/${employee.id}` : '/api/employees', {
        method: isEditing ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, department, role, email }),
      })

      if (!response.ok) {
        throw new Error(await readErrorMessage(response))
      }

      // The parent refreshes the list and hides this form
      onSaved()
    } catch (err) {
      setError(err.message)
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h3>{isEditing ? 'Redigera medarbetare' : 'Ny medarbetare'}</h3>
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
          Avdelning
          <br />
          <input
            value={department}
            onChange={(event) => setDepartment(event.target.value)}
            required
            maxLength={100}
          />
        </label>
      </p>
      <p>
        <label>
          Roll
          <br />
          <input
            value={role}
            onChange={(event) => setRole(event.target.value)}
            required
            maxLength={100}
          />
        </label>
      </p>
      <p>
        <label>
          E-post
          <br />
          <input
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
            maxLength={200}
          />
        </label>
      </p>
      {error && <p role="alert">{error}</p>}
      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Sparar...' : 'Spara'}
      </button>{' '}
      <button type="button" onClick={onCancel} disabled={isSubmitting}>
        Avbryt
      </button>
    </form>
  )
}

export default EmployeeForm
