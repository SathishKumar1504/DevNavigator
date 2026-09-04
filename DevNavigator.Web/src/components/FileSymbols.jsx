function FileSymbols({
  selectedFile,
  symbols,
  symbolsLoading,
}) {
  return (
    <section className="content-card">
      <div className="card-header">
        <div>
          <h2>
            {selectedFile
              ? `Symbols — ${selectedFile.fileName}`
              : 'File Symbols'}
          </h2>

          <p>
            {selectedFile
              ? selectedFile.relativePath
              : 'Select a file to explore its symbols.'}
          </p>
        </div>
      </div>

      <div className="symbols-list">
        {symbolsLoading && (
          <div className="sidebar-message">
            Loading symbols...
          </div>
        )}

        {!symbolsLoading &&
          !selectedFile && (
            <div className="sidebar-message">
              Select a file to explore its
              symbols.
            </div>
          )}

        {!symbolsLoading &&
          selectedFile &&
          symbols.length === 0 && (
            <div className="sidebar-message">
              No symbols found.
            </div>
          )}

        {!symbolsLoading &&
          symbols.map((symbol) => (
            <div
              key={symbol.id}
              className="symbol-item"
            >
              <div className="symbol-type">
                {symbol.symbolType}
              </div>

              <div className="symbol-name">
                {symbol.name}
              </div>

              <div className="symbol-line">
                Line {symbol.lineNumber}
              </div>

              {symbol.importPath && (
                <div className="symbol-import">
                  {symbol.importPath}
                </div>
              )}
            </div>
          ))}
      </div>
    </section>
  )
}

export default FileSymbols