function ArchitectureGraph({
  relationships = [],
  relationshipsLoading = false,
  onFileSelect,
}) {
  if (relationshipsLoading) {
    return (
      <section className="content-card architecture-card">
        <div className="card-header">
          <div>
            <h2>Architecture Navigation</h2>

            <p>
              Explore relationships between symbols,
              methods and files.
            </p>
          </div>
        </div>

        <div className="graph-message">
          Loading architecture relationships...
        </div>
      </section>
    )
  }

  if (relationships.length === 0) {
    return (
      <section className="content-card architecture-card">
        <div className="card-header">
          <div>
            <h2>Architecture Navigation</h2>

            <p>
              Explore relationships between symbols,
              methods and files.
            </p>
          </div>
        </div>

        <div className="graph-message">
          No relationships found for this file.
        </div>
      </section>
    )
  }

  function handleNodeClick(fileId) {
    if (!onFileSelect || !fileId) {
      return
    }

    onFileSelect(fileId)
  }

  const groupedRelationships = relationships.reduce(
  (groups, relationship) => {
    const key =
      `${relationship.from.symbolId}-${relationship.relationshipType}`

    if (!groups[key]) {
      groups[key] = {
        from: relationship.from,
        relationshipType:
          relationship.relationshipType,
        targets: [],
      }
    }

    groups[key].targets.push({
      relationshipId:
        relationship.relationshipId,
      symbol: relationship.to,
    })

    return groups
  },
  {},
)

  return (
    <section className="content-card architecture-card">
      <div className="card-header">
        <div>
          <h2>Architecture Navigation</h2>

          <p>
            Explore relationships between symbols,
            methods and files.
          </p>
        </div>

        <div className="relationship-count">
          {relationships.length} relationships
        </div>
      </div>

      <div className="architecture-graph">
        {Object.values(groupedRelationships).map(
          (group) => (
            <div
              className="relationship-group"
              key={`${group.from.symbolId}-${group.relationshipType}`}
            >
              <div className="relationship-group-content">

                {/* FROM NODE */}
                <button
                  type="button"
                  className="graph-node"
                  onClick={() =>
                    handleNodeClick(
                      group.from.fileId,
                    )
                  }
                >
                  <span className="node-type">
                    {group.from.symbolType}
                  </span>

                  <strong>
                    {group.from.name}
                  </strong>

                  <small>
                    File ID: {group.from.fileId}
                  </small>
                </button>

                {/* TARGETS */}
                <div className="relationship-targets">
                  {group.targets.map((target) => (
  <div
    className="relationship-row"
    key={target.relationshipId}
  >
                      <div className="graph-connection">
                        <span>
                          {group.relationshipType}
                        </span>

                        <div className="graph-arrow">
                          →
                        </div>
                      </div>

                      <button
                        type="button"
                        className="graph-node"
                        onClick={() =>
                          handleNodeClick(
                            target.symbol.fileId,
                          )
                        }
                      >
                        <span className="node-type">
                          {target.symbol.symbolType}
                        </span>

                        <strong>
                          {target.symbol.name}
                        </strong>

                        <small>
                          File ID: {target.symbol.fileId}
                        </small>
                      </button>
                    </div>
                  ))}
                </div>

              </div>
            </div>
          ),
        )}
      </div>
    </section>
  )
}

export default ArchitectureGraph