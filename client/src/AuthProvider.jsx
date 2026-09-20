import { useEffect, useState } from 'react'
import { AuthContext } from './authContext.js'

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  // True until we know whether the browser already has a valid login cookie
  const [isLoading, setIsLoading] = useState(true)

  // On page load, ask the backend "who am I?". The browser sends the auth cookie
  // automatically, so this restores the login after a page refresh.
  useEffect(() => {
    async function restoreSession() {
      try {
        const response = await fetch('/api/auth/me')
        if (response.ok) {
          setUser(await response.json())
        }
      } catch {
        // Backend unreachable - treat as logged out
      } finally {
        setIsLoading(false)
      }
    }

    restoreSession()
  }, [])

  async function login(email, password) {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    })

    if (response.status === 401) {
      throw new Error('Fel e-postadress eller lösenord.')
    }
    if (!response.ok) {
      throw new Error(`Något gick fel (HTTP ${response.status}).`)
    }

    setUser(await response.json())
  }

  async function logout() {
    await fetch('/api/auth/logout', { method: 'POST' })
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}
