function RepositoryFiles({
  files,
  filesLoading,
  selectedFile,
  onFileSelect,
}) {
  return (
    <section className="content-card">
      <div className="card-header">
        <div>
          <h2>
            Repository Files
          </h2>

          <p>
            Browse the files indexed in
            this repository.
          </p>
        </div>
      </div>

      <div className="files-list">
        {filesLoading && (
          <div className="sidebar-message">
            Loading files...
          </div>
        )}

        {!filesLoading &&
          files.length === 0 && (
            <div className="sidebar-message">
              No files found.
            </div>
          )}

        {!filesLoading &&
          files.map((file) => (
            <button
              key={file.id}
              className={
                selectedFile?.id === file.id
                  ? 'file-item selected'
                  : 'file-item'
              }
              onClick={() =>
                onFileSelect(file)
              }
            >
              <div className="file-icon">
                {file.extension
                  ?.replace('.', '')
                  .toUpperCase() ||
                  'FILE'}
              </div>

              <div className="file-info">
                <div className="file-name">
                  {file.fileName}
                </div>

                <div className="file-path">
                  {file.relativePath}
                </div>
              </div>
            </button>
          ))}
      </div>
    </section>
  )
}

export default RepositoryFiles