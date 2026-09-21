import { useEffect, useState } from 'react'
import { readErrorMessage } from './apiErrors.js'
import EmployeeForm from './EmployeeForm.jsx'

function EmployeeListPage({ onBack }) {
  const [employees, setEmployees] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)
  // What the form is showing: null = hidden, 'new' = create, otherwise the employee being edited
  const [formTarget, setFormTarget] = useState(null)
  const [deletingId, setDeletingId] = useState(null)

  // Increasing this number makes the effect below fetch the list again
  const [reloadCount, setReloadCount] = useState(0)

  // Fetches the list on page load and again after every change (reloadCount),
  // so the table always shows what is really in the database.
  useEffect(() => {
    async function fetchEmployees() {
      try {
        const response = await fetch('/api/employees')
        if (!response.ok) {
          throw new Error(await readErrorMessage(response))
        }
        setEmployees(await response.json())
        setError(null)
      } catch (err) {
        setError(err.message)
      } finally {
        setIsLoading(false)
      }
    }

    fetchEmployees()
  }, [reloadCount])

  function reloadEmployees() {
    setReloadCount((count) => count + 1)
  }

  function handleSaved() {
    setFormTarget(null)
    reloadEmployees()
  }

  async function handleDelete(employee) {
    if (!window.confirm(`Ta bort ${employee.name}?`)) {
      return
    }

    setError(null)
    setDeletingId(employee.id)

    try {
      const response = await fetch(`/api/employees/${employee.id}`, { method: 'DELETE' })
      // 404 means it was already deleted elsewhere - the refresh below shows the real state
      if (!response.ok && response.status !== 404) {
        throw new Error(await readErrorMessage(response))
      }

      if (formTarget !== 'new' && formTarget?.id === employee.id) {
        setFormTarget(null)
      }
      reloadEmployees()
    } catch (err) {
      setError(err.message)
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Medarbetare</h2>
      <p>
        <button type="button" onClick={onBack}>
          Tillbaka till dashboarden
        </button>
      </p>

      {formTarget === null ? (
        <p>
          <button type="button" onClick={() => setFormTarget('new')}>
            Lägg till medarbetare
          </button>
        </p>
      ) : (
        // key makes React start with a fresh form when switching between employees
        <EmployeeForm
          key={formTarget === 'new' ? 'new' : formTarget.id}
          employee={formTarget === 'new' ? null : formTarget}
          onSaved={handleSaved}
          onCancel={() => setFormTarget(null)}
        />
      )}

      {isLoading && <p>Laddar...</p>}
      {error && <p role="alert">{error}</p>}
      {!isLoading && !error && employees.length === 0 && <p>Inga medarbetare hittades.</p>}

      {employees.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Namn</th>
              <th>Avdelning</th>
              <th>Roll</th>
              <th>E-post</th>
              <th>Åtgärder</th>
            </tr>
          </thead>
          <tbody>
            {employees.map((employee) => (
              <tr key={employee.id}>
                <td>{employee.name}</td>
                <td>{employee.department}</td>
                <td>{employee.role}</td>
                <td>{employee.email}</td>
                <td>
                  <button type="button" onClick={() => setFormTarget(employee)}>
                    Redigera
                  </button>{' '}
                  <button
                    type="button"
                    onClick={() => handleDelete(employee)}
                    disabled={deletingId === employee.id}
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

export default EmployeeListPage
