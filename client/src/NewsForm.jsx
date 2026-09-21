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
    <form onSubmit={handleSubmit}>
      <h3>{isEditing ? 'Redigera nyhet' : 'Ny nyhet'}</h3>
      <p>
        <label>
          Titel
          <br />
          <input
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            required
            maxLength={200}
          />
        </label>
      </p>
      <p>
        <label>
          Innehåll
          <br />
          <textarea
            value={content}
            onChange={(event) => setContent(event.target.value)}
            required
            maxLength={10000}
            rows={6}
            cols={50}
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

export default NewsForm
