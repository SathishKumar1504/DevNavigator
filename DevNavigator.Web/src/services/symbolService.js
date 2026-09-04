const API_BASE = 'http://localhost:5044/api'

export async function getFileSymbols(fileId) {
  const response = await fetch(
    `${API_BASE}/symbols/file/${fileId}`,
  )

  if (!response.ok) {
    throw new Error(
      `Failed to load symbols (${response.status})`,
    )
  }

  return await response.json()
}