function RepositorySidebar({
  repositories,
  selectedRepository,
  loading,
  onRepositorySelect,
}) {
  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <div className="sidebar-title">
          Repositories
        </div>

        <div className="repository-count">
          {repositories.length}
        </div>
      </div>

      {loading && (
        <div className="sidebar-message">
          Loading repositories...
        </div>
      )}

      {!loading && repositories.length === 0 && (
        <div className="sidebar-message">
          No repositories found.
        </div>
      )}

      <div className="repository-list">
        {repositories.map((repository) => (
          <button
            key={repository.id}
            className={
              selectedRepository?.id === repository.id
                ? 'repository-item selected'
                : 'repository-item'
            }
            onClick={() =>
              onRepositorySelect(repository)
            }
          >
            <div className="repository-icon">
              {repository.name
                .charAt(0)
                .toUpperCase()}
            </div>

            <div className="repository-info">
              <div className="repository-name">
                {repository.name}
              </div>

              <div className="repository-files">
                {repository.fileCount.toLocaleString()}{' '}
                files
              </div>
            </div>
          </button>
        ))}
      </div>
    </aside>
  )
}

export default RepositorySidebar