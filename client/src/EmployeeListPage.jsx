import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { readErrorMessage } from './apiErrors.js'
import EmployeeForm from './EmployeeForm.jsx'

function EmployeeListPage() {
  const [employees, setEmployees] = useState([])
  const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)
  // What the form is showing: null = hidden, 'new' = create, otherwise the employee being edited
  const [formTarget, setFormTarget] = useState(null)
  const [deletingId, setDeletingId] = useState(null)

  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)

  // Increasing this number makes the effect below fetch the list again
  const [reloadCount, setReloadCount] = useState(0)

  // Fetches the current page on load and again after every change (search, page, reloadCount),
  // so the table always shows what is really in the database.
  useEffect(() => {
    async function fetchEmployees() {
      setIsLoading(true)
      try {
        const params = new URLSearchParams({ page: String(page), pageSize: '10' })
        if (search) {
          params.set('search', search)
        }

        const response = await fetch(`/api/employees?${params}`)
        if (!response.ok) {
          throw new Error(await readErrorMessage(response))
        }
        const data = await response.json()
        setEmployees(data.items)
        setTotalPages(data.totalPages)
        setError(null)
      } catch (err) {
        setError(err.message)
      } finally {
        setIsLoading(false)
      }
    }

    fetchEmployees()
  }, [search, page, reloadCount])

  function reloadEmployees() {
    setReloadCount((count) => count + 1)
  }

  function handleSearchChange(value) {
    setSearch(value)
    setPage(1) // a new search may have far fewer pages, so start over at page 1
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
        <Link to="/dashboard">Tillbaka till dashboarden</Link>
      </p>

      <p>
        <label>
          Sök
          <br />
          <input
            type="search"
            value={search}
            onChange={(event) => handleSearchChange(event.target.value)}
            placeholder="Namn, avdelning eller e-post"
          />
        </label>
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

      {totalPages > 1 && (
        <p>
          <button type="button" onClick={() => setPage((p) => p - 1)} disabled={page <= 1}>
            Föregående
          </button>{' '}
          Sida {page} av {totalPages}{' '}
          <button
            type="button"
            onClick={() => setPage((p) => p + 1)}
            disabled={page >= totalPages}
          >
            Nästa
          </button>
        </p>
      )}
    </main>
  )
}

export default EmployeeListPage
