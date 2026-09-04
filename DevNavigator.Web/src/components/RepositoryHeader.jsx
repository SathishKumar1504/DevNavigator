function RepositoryHeader({
  repository,
  onIndexRepository,
}) {
  return (
    <section className="page-header">
      <div>
        <div className="breadcrumb">
          Repositories / {repository.name}
        </div>

        <h1>
          {repository.name}
        </h1>

        <p className="repository-path">
          {repository.rootPath}
        </p>
      </div>

      <button
        className="primary-button"
        onClick={() =>
          onIndexRepository(repository.id)
        }
      >
        Index Repository
      </button>
    </section>
  )
}

export default RepositoryHeader