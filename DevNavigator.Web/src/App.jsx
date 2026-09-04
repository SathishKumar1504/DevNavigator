import { useEffect, useState } from 'react'
import './App.css'

import Header from './components/Header'
import RepositorySidebar from './components/RepositorySidebar'
import RepositoryHeader from './components/RepositoryHeader'
import RepositoryStats from './components/RepositoryStats'
import RepositoryFiles from './components/RepositoryFiles'
import FileSymbols from './components/FileSymbols'
import ArchitectureGraph from './components/ArchitectureGraph'

import {
  getRepositories,
  indexRepository,
} from './services/repositoryService'
import {
  getFileRelationships,
} from './services/relationshipService'

import { getFiles } from './services/fileService'

import { getFileSymbols } from './services/symbolService'

function App() {
  const [repositories, setRepositories] = useState([])
  const [selectedRepository, setSelectedRepository] =
    useState(null)

  const [indexing, setIndexing] = useState(false)

  const [files, setFiles] = useState([])
  const [filesLoading, setFilesLoading] = useState(false)

  const [selectedFile, setSelectedFile] = useState(null)

  const [symbols, setSymbols] = useState([])
  const [symbolsLoading, setSymbolsLoading] = useState(false)

  const [relationships, setRelationships] = useState([])
  const [relationshipsLoading, setRelationshipsLoading] =
    useState(false)

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    loadRepositories()
  }, [])

  useEffect(() => {
    if (!selectedRepository) {
      setFiles([])
      setSelectedFile(null)
      setSymbols([])
      setRelationships([])
      return
    }

    loadFiles(selectedRepository.id)
  }, [selectedRepository])

  async function loadRepositories() {
    try {
      setLoading(true)
      setError('')

      const items = await getRepositories()

      setRepositories(items)

      if (items.length > 0) {
        setSelectedRepository((current) => {
          if (current) {
            const existing = items.find(
              (repository) =>
                repository.id === current.id,
            )

            if (existing) {
              return existing
            }
          }

          return items[0]
        })
      }
    } catch (err) {
      console.error(err)
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  async function loadFiles(repositoryId) {
    try {
      setFilesLoading(true)
      setError('')
      setSelectedFile(null)
      setSymbols([])
      setRelationships([])

      const data = await getFiles(repositoryId)

      setFiles(data)
    } catch (err) {
      console.error(err)
      setError(err.message)
      setFiles([])
    } finally {
      setFilesLoading(false)
    }
  }

  async function loadFileSymbols(file) {
    try {
      setSymbolsLoading(true)
      setRelationshipsLoading(true)
      setError('')

      setSelectedFile(file)
      setSymbols([])
      setRelationships([])

      const [symbolData, relationshipData] =
        await Promise.all([
          getFileSymbols(file.id),
          getFileRelationships(file.id),
        ])

      setSymbols(symbolData)
      setRelationships(relationshipData)
    } catch (err) {
      console.error(err)
      setError(err.message)
      setSymbols([])
      setRelationships([])
    } finally {
      setSymbolsLoading(false)
      setRelationshipsLoading(false)
    }
  }

  async function handleIndexRepository(repositoryId) {
    try {
      setIndexing(true)
      setError('')

      await indexRepository(repositoryId)

      await loadRepositories()
    } catch (err) {
      console.error(err)
      setError(err.message)
    } finally {
      setIndexing(false)
    }
  }
  function handleArchitectureFileSelect(fileId) {
    console.log('Architecture clicked file:', fileId)
  console.log('Available files:', files)
  const file = files.find(
    (item) => item.id === fileId,
  )

  if (!file) {
    console.warn(
      `File ${fileId} was not found in the current file list.`,
    )

    return
  }

  loadFileSymbols(file)
}

  return (
    <div className="app">
      <Header onRefresh={loadRepositories} />

      <div className="dashboard">
        <RepositorySidebar
          repositories={repositories}
          selectedRepository={selectedRepository}
          loading={loading}
          onRepositorySelect={setSelectedRepository}
        />

        <main className="main-content">
          {error && (
            <div className="error-banner">
              {error}
            </div>
          )}

          {!selectedRepository && !loading && (
            <div className="empty-state">
              <h2>No repository selected</h2>

              <p>
                Add a repository to start exploring your
                codebase.
              </p>
            </div>
          )}

          {selectedRepository && (
            <>
              <RepositoryHeader
                repository={selectedRepository}
                onIndexRepository={handleIndexRepository}
                indexing={indexing}
              />

              <RepositoryStats
                repository={selectedRepository}
              />

              <RepositoryFiles
                files={files}
                filesLoading={filesLoading}
                selectedFile={selectedFile}
                onFileSelect={loadFileSymbols}
              />

              <FileSymbols
                selectedFile={selectedFile}
                symbols={symbols}
                symbolsLoading={symbolsLoading}
              />

             <ArchitectureGraph
  relationships={relationships}
  relationshipsLoading={relationshipsLoading}
  onFileSelect={handleArchitectureFileSelect}
/>
            </>
          )}
        </main>
      </div>
    </div>
  )
}

export default App