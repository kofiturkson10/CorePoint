import { useState } from 'react'
import { readErrorMessage } from './apiErrors.js'

// Used for both creating (newsItem = null) and editing (newsItem = the item being edited).
// Author and publish date are set by the backend, so they are not part of the form.
function NewsForm({ newsItem, onSaved, onCancel }) {
  const isEditing = newsItem !== null
  const [title, setTitle] = useState(newsItem?.title ?? '')
  const [content, setContent] = useState(newsItem?.content ?? '')
  const [error, setError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)
    setIsSubmitting(true)

    try {
      // Create = POST /api/news, edit = PUT /api/news/{id}
      const response = await fetch(isEditing ? `/api/news/${newsItem.id}` : '/api/news', {
        method: isEditing ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title, content }),
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
    <form className="card" onSubmit={handleSubmit} style={{ marginBottom: 'var(--space-4)' }}>
      <h3>{isEditing ? 'Redigera nyhet' : 'Ny nyhet'}</h3>
      <div className="field">
        <label className="field-label" htmlFor="news-title">
          Titel
        </label>
        <input
          className="input"
          id="news-title"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          required
          maxLength={200}
        />
        <span className="field-hint">Max 200 tecken</span>
      </div>
      <div className="field">
        <label className="field-label" htmlFor="news-content">
          Innehåll
        </label>
        <textarea
          className="input"
          id="news-content"
          value={content}
          onChange={(event) => setContent(event.target.value)}
          required
          maxLength={10000}
          rows={6}
          cols={50}
        />
      </div>
      {error && (
        <p className="alert alert-error" role="alert">
          {error}
        </p>
      )}
      <button className="btn btn-primary" type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Sparar...' : 'Spara'}
      </button>{' '}
      <button className="btn btn-secondary" type="button" onClick={onCancel} disabled={isSubmitting}>
        Avbryt
      </button>
    </form>
  )
}

export default NewsForm
