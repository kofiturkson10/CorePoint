// Turns a failed fetch response into a readable message.
// The backend sends { message } for conflicts and { errors: { Field: [...] } } for validation errors.
export async function readErrorMessage(response) {
  if (response.status === 401) {
    return 'Din session har gått ut. Ladda om sidan och logga in igen.'
  }

  try {
    const body = await response.json()
    if (body.message) {
      return body.message
    }
    if (body.errors) {
      return Object.values(body.errors).flat().join(' ')
    }
  } catch {
    // Response had no JSON body - fall through to the generic message
  }

  return `Något gick fel (HTTP ${response.status}).`
}
