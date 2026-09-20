import { useState } from 'react'
import { useAuth } from './authContext.js'

function LoginPage() {
  const { login } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault() // stop the browser from reloading the page
    setError(null)
    setIsSubmitting(true)

    try {
      await login(email, password)
      // On success AuthProvider sets the user, and App switches to the dashboard
    } catch (err) {
      setError(err.message)
      setIsSubmitting(false)
    }
  }

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Logga in</h2>
      <form onSubmit={handleSubmit}>
        <p>
          <label>
            E-post
            <br />
            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
              autoComplete="username"
            />
          </label>
        </p>
        <p>
          <label>
            Lösenord
            <br />
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
              autoComplete="current-password"
            />
          </label>
        </p>
        {error && <p role="alert">{error}</p>}
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Loggar in...' : 'Logga in'}
        </button>
      </form>
    </main>
  )
}

export default LoginPage
