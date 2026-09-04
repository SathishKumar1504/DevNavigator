const API_BASE = 'http://localhost:5044/api'

export async function getRepositories() {
  const response = await fetch(
    `${API_BASE}/repositories`,
  )

  if (!response.ok) {
    throw new Error(
      `Failed to load repositories (${response.status})`,
    )
  }

  const data = await response.json()

  return data.value ?? data
}

export async function indexRepository(repositoryId) {
  const response = await fetch(
    `${API_BASE}/index/${repositoryId}`,
    {
      method: 'POST',
    },
  )

  if (!response.ok) {
    throw new Error(
      `Indexing failed (${response.status})`,
    )
  }

  return await response.json()
}