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
    // marginBottom separates the card from the table below it
    <form className="card" style={{ marginBottom: 'var(--space-5)' }} onSubmit={handleSubmit}>
      <h3>{isEditing ? 'Redigera medarbetare' : 'Ny medarbetare'}</h3>
      <div className="field">
        <label className="field-label" htmlFor="employee-name">
          Namn
        </label>
        <input
          id="employee-name"
          className="input"
          value={name}
          onChange={(event) => setName(event.target.value)}
          required
          maxLength={100}
        />
      </div>
      <div className="field">
        <label className="field-label" htmlFor="employee-department">
          Avdelning
        </label>
        <input
          id="employee-department"
          className="input"
          value={department}
          onChange={(event) => setDepartment(event.target.value)}
          required
          maxLength={100}
        />
      </div>
      <div className="field">
        <label className="field-label" htmlFor="employee-role">
          Roll
        </label>
        <input
          id="employee-role"
          className="input"
          value={role}
          onChange={(event) => setRole(event.target.value)}
          required
          maxLength={100}
        />
      </div>
      <div className="field">
        <label className="field-label" htmlFor="employee-email">
          E-post
        </label>
        <input
          id="employee-email"
          className="input"
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          required
          maxLength={200}
        />
      </div>
      {error && (
        <p className="alert alert-error" role="alert">
          {error}
        </p>
      )}
      <div style={{ display: 'flex', gap: 'var(--space-2)' }}>
        <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
          {isSubmitting ? 'Sparar...' : 'Spara'}
        </button>
        <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isSubmitting}>
          Avbryt
        </button>
      </div>
    </form>
  )
}

export default EmployeeForm
