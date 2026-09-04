const API_BASE = 'http://localhost:5044/api'

export async function getFileRelationships(fileId) {
  const response = await fetch(
    `${API_BASE}/symbols/relationships/file/${fileId}`,
  )

  if (!response.ok) {
    throw new Error(
      `Failed to load relationships (${response.status})`,
    )
  }

  return await response.json()
}