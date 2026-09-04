const API_BASE = 'http://localhost:5044/api'

export async function getFiles(repositoryId) {
  const response = await fetch(
    `${API_BASE}/files?repositoryId=${repositoryId}`,
  )

  if (!response.ok) {
    throw new Error(
      `Failed to load files (${response.status})`,
    )
  }

  return await response.json()
}