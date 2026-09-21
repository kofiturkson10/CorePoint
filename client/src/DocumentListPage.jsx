import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { readErrorMessage } from './apiErrors.js'
import DocumentUploadForm from './DocumentUploadForm.jsx'

function formatFileSize(bytes) {
  if (bytes < 1024) {
    return `${bytes} B`
  }
  if (bytes < 1024 * 1024) {
    return `${(bytes / 1024).toFixed(1)} kB`
  }
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function DocumentListPage() {
  const [documents, setDocuments] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(null)
  const [isUploadFormOpen, setIsUploadFormOpen] = useState(false)
  const [downloadingId, setDownloadingId] = useState(null)

  // Increasing this number makes the effect below fetch the list again
  const [reloadCount, setReloadCount] = useState(0)

  // Fetches the list on page load and again after every upload (reloadCount),
  // so the page always shows what is really in storage.
  useEffect(() => {
    async function fetchDocuments() {
      try {
        const response = await fetch('/api/documents')
        if (!response.ok) {
          throw new Error(await readErrorMessage(response))
        }
        setDocuments(await response.json())
        setError(null)
      } catch (err) {
        setError(err.message)
      } finally {
        setIsLoading(false)
      }
    }

    fetchDocuments()
  }, [reloadCount])

  function handleUploaded() {
    setIsUploadFormOpen(false)
    setReloadCount((count) => count + 1)
  }

  // Downloads with fetch (not a plain link) so that errors such as an expired session
  // or a file that no longer exists can be shown on the page.
  async function handleDownload(documentInfo) {
    setError(null)
    setDownloadingId(documentInfo.id)

    try {
      const response = await fetch(`/api/documents/${documentInfo.id}`)
      if (!response.ok) {
        throw new Error(await readErrorMessage(response))
      }

      // Put the file in a temporary in-memory URL and click a hidden link to it,
      // which makes the browser save the file under its original name.
      const blob = await response.blob()
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = documentInfo.fileName
      link.click()
      URL.revokeObjectURL(url)
    } catch (err) {
      setError(err.message)
    } finally {
      setDownloadingId(null)
    }
  }

  return (
    <main>
      <h1>Företagsportal</h1>
      <h2>Dokument</h2>
      <p>
        <Link to="/dashboard">Tillbaka till dashboarden</Link>
      </p>

      {isUploadFormOpen ? (
        <DocumentUploadForm
          onUploaded={handleUploaded}
          onCancel={() => setIsUploadFormOpen(false)}
        />
      ) : (
        <p>
          <button type="button" onClick={() => setIsUploadFormOpen(true)}>
            Ladda upp dokument
          </button>
        </p>
      )}

      {isLoading && <p>Laddar...</p>}
      {error && <p role="alert">{error}</p>}
      {!isLoading && !error && documents.length === 0 && <p>Inga dokument hittades.</p>}

      {documents.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Filnamn</th>
              <th>Storlek</th>
              <th>Uppladdad av</th>
              <th>Uppladdad</th>
            </tr>
          </thead>
          <tbody>
            {documents.map((documentInfo) => (
              <tr key={documentInfo.id}>
                <td>
                  <button
                    type="button"
                    onClick={() => handleDownload(documentInfo)}
                    disabled={downloadingId === documentInfo.id}
                  >
                    {documentInfo.fileName}
                  </button>
                </td>
                <td>{formatFileSize(documentInfo.sizeBytes)}</td>
                <td>{documentInfo.uploadedBy}</td>
                <td>{new Date(documentInfo.uploadedAt).toLocaleString('sv-SE')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </main>
  )
}

export default DocumentListPage
