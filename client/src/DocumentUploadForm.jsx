import { useState } from 'react'
import { readErrorMessage } from './apiErrors.js'

// Must match the limit in DocumentsController. The backend checks it too; this just gives a faster answer.
const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024

function DocumentUploadForm({ onUploaded, onCancel }) {
  const [file, setFile] = useState(null)
  const [error, setError] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)

    if (file.size > MAX_FILE_SIZE_BYTES) {
      setError('Filen är för stor. Maxstorlek är 10 MB.')
      return
    }

    setIsSubmitting(true)

    try {
      // Files are sent as multipart/form-data. The field name "file" must match the parameter
      // name in DocumentsController. No Content-Type header is set on purpose: the browser adds
      // it together with the boundary that separates the parts.
      const formData = new FormData()
      formData.append('file', file)

      const response = await fetch('/api/documents', { method: 'POST', body: formData })

      if (!response.ok) {
        throw new Error(await readErrorMessage(response))
      }

      // The parent refreshes the list and hides this form
      onUploaded()
    } catch (err) {
      setError(err.message)
      setIsSubmitting(false)
    }
  }

  return (
    <form className="card" onSubmit={handleSubmit} style={{ marginBottom: 'var(--space-4)' }}>
      <h3>Ladda upp dokument</h3>
      <div className="field">
        <label className="field-label" htmlFor="document-file">
          Fil
        </label>
        <input
          className="input"
          id="document-file"
          type="file"
          onChange={(event) => setFile(event.target.files[0] ?? null)}
          required
        />
        <span className="field-hint">Max 10 MB</span>
      </div>
      {error && (
        <p className="alert alert-error" role="alert">
          {error}
        </p>
      )}
      <button className="btn btn-primary" type="submit" disabled={isSubmitting || file === null}>
        {isSubmitting ? 'Laddar upp...' : 'Ladda upp'}
      </button>{' '}
      <button
        className="btn btn-secondary"
        type="button"
        onClick={onCancel}
        disabled={isSubmitting}
      >
        Avbryt
      </button>
    </form>
  )
}

export default DocumentUploadForm
