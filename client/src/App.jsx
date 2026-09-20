import { useEffect, useState } from 'react'

function App() {
  const [health, setHealth] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    async function fetchHealth() {
      try {
        const response = await fetch('/api/health')
        if (!response.ok) {
          throw new Error(`HTTP ${response.status}`)
        }
        setHealth(await response.json())
      } catch (err) {
        setError(err.message)
      }
    }

    fetchHealth()
  }, [])

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Backend status</h2>
      {error && <p>Kunde inte nå backend: {error}</p>}
      {!error && !health && <p>Laddar...</p>}
      {health && <pre>{JSON.stringify(health, null, 2)}</pre>}
    </main>
  )
}

export default App
