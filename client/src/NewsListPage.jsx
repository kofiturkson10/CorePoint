import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { readErrorMessage } from './apiErrors.js'
import NewsForm from './NewsForm.jsx'

function NewsListPage() {
  const [newsItems, setNewsItems] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)
  // What the form is showing: null = hidden, 'new' = create, otherwise the item being edited
  const [formTarget, setFormTarget] = useState(null)
  const [deletingId, setDeletingId] = useState(null)

  // Increasing this number makes the effect below fetch the list again
  const [reloadCount, setReloadCount] = useState(0)

  // Fetches the list on page load and again after every change (reloadCount),
  // so the page always shows what is really in the database.
  useEffect(() => {
    async function fetchNews() {
      try {
        const response = await fetch('/api/news')
        if (!response.ok) {
          throw new Error(await readErrorMessage(response))
        }
        setNewsItems(await response.json())
        setError(null)
      } catch (err) {
        setError(err.message)
      } finally {
        setIsLoading(false)
      }
    }

    fetchNews()
  }, [reloadCount])

  function reloadNews() {
    setReloadCount((count) => count + 1)
  }

  function handleSaved() {
    setFormTarget(null)
    reloadNews()
  }

  async function handleDelete(newsItem) {
    if (!window.confirm(`Ta bort "${newsItem.title}"?`)) {
      return
    }

    setError(null)
    setDeletingId(newsItem.id)

    try {
      const response = await fetch(`/api/news/${newsItem.id}`, { method: 'DELETE' })
      // 404 means it was already deleted elsewhere - the refresh below shows the real state
      if (!response.ok && response.status !== 404) {
        throw new Error(await readErrorMessage(response))
      }

      if (formTarget !== 'new' && formTarget?.id === newsItem.id) {
        setFormTarget(null)
      }
      reloadNews()
    } catch (err) {
      setError(err.message)
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Nyheter</h2>
      <p>
        <Link to="/dashboard">Tillbaka till dashboarden</Link>
      </p>

      {formTarget === null ? (
        <p>
          <button type="button" onClick={() => setFormTarget('new')}>
            Lägg till nyhet
          </button>
        </p>
      ) : (
        // key makes React start with a fresh form when switching between items
        <NewsForm
          key={formTarget === 'new' ? 'new' : formTarget.id}
          newsItem={formTarget === 'new' ? null : formTarget}
          onSaved={handleSaved}
          onCancel={() => setFormTarget(null)}
        />
      )}

      {isLoading && <p>Laddar...</p>}
      {error && <p role="alert">{error}</p>}
      {!isLoading && !error && newsItems.length === 0 && <p>Inga nyheter hittades.</p>}

      {newsItems.map((newsItem) => (
        <article key={newsItem.id}>
          <h3>{newsItem.title}</h3>
          <p>
            <small>
              {newsItem.author} · {new Date(newsItem.publishedAt).toLocaleString('sv-SE')}
            </small>
          </p>
          {/* pre-wrap keeps the line breaks the author typed */}
          <p style={{ whiteSpace: 'pre-wrap' }}>{newsItem.content}</p>
          <button type="button" onClick={() => setFormTarget(newsItem)}>
            Redigera
          </button>{' '}
          <button
            type="button"
            onClick={() => handleDelete(newsItem)}
            disabled={deletingId === newsItem.id}
          >
            Ta bort
          </button>
        </article>
      ))}
    </main>
  )
}

export default NewsListPage
