function RepositoryStats({ repository }) {
  return (
    <section className="stats-grid">
      <div className="stat-card">
        <div className="stat-label">
          Files
        </div>

        <div className="stat-value">
          {repository.fileCount.toLocaleString()}
        </div>

        <div className="stat-description">
          Indexed source files
        </div>
      </div>

      <div className="stat-card">
        <div className="stat-label">
          Symbols
        </div>

        <div className="stat-value">
          —
        </div>

        <div className="stat-description">
          Classes, methods & messages
        </div>
      </div>

      <div className="stat-card">
        <div className="stat-label">
          Relationships
        </div>

        <div className="stat-value">
          —
        </div>

        <div className="stat-description">
          Calls, contains & consumes
        </div>
      </div>

      <div className="stat-card">
        <div className="stat-label">
          Last Indexed
        </div>

        <div className="stat-value small">
          {repository.lastIndexedAt
            ? new Date(
                repository.lastIndexedAt,
              ).toLocaleString()
            : 'Never'}
        </div>

        <div className="stat-description">
          Repository index status
        </div>
      </div>
    </section>
  )
}

export default RepositoryStats