function Header({ onRefresh }) {
  return (
    <header className="topbar">
      <div className="brand">
        <div className="brand-icon">
          D
        </div>

        <div>
          <div className="brand-name">
            DevNavigator
          </div>

          <div className="brand-subtitle">
            Code intelligence & navigation
          </div>
        </div>
      </div>

      <div className="topbar-actions">
        <button
          className="secondary-button"
          onClick={onRefresh}
        >
          Refresh
        </button>

        <button className="primary-button">
          + Add Repository
        </button>
      </div>
    </header>
  )
}

export default Header