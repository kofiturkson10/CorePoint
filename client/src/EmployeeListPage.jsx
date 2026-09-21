import { useEffect, useState } from 'react'

function EmployeeListPage({ onBack }) {
  const [employees, setEmployees] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    async function fetchEmployees() {
      try {
        const response = await fetch('/api/employees')
        if (response.status === 401) {
          throw new Error('Din session har gått ut. Ladda om sidan och logga in igen.')
        }
        if (!response.ok) {
          throw new Error(`Kunde inte hämta medarbetare (HTTP ${response.status}).`)
        }
        setEmployees(await response.json())
      } catch (err) {
        setError(err.message)
      } finally {
        setIsLoading(false)
      }
    }

    fetchEmployees()
  }, [])

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Medarbetare</h2>
      <p>
        <button type="button" onClick={onBack}>
          Tillbaka till dashboarden
        </button>
      </p>

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
            </tr>
          </thead>
          <tbody>
            {employees.map((employee) => (
              <tr key={employee.id}>
                <td>{employee.name}</td>
                <td>{employee.department}</td>
                <td>{employee.role}</td>
                <td>{employee.email}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </main>
  )
}

export default EmployeeListPage
