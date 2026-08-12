import { useEffect, useState } from 'react'
import './App.css'

type PullRequest = {
  number: number
  title: string
  authorLogin: string
  htmlUrl: string
  isDraft: boolean
  createdAt: string
  state: string
  source: 'GitHub' | 'AzureDevOps'
}

type PrResponse = {
  prs: PullRequest[]
  warnings: string[]
}

function SourceBadge({ source }: { source: PullRequest['source'] }) {
  const label = source === 'GitHub' ? 'GitHub' : 'Azure DevOps'
  return <span className={`badge source-${source}`}>{label}</span>
}

function App() {
  const [status, setStatus] = useState<'loading' | 'loaded' | 'error'>('loading')
  const [prs, setPrs] = useState<PullRequest[]>([])
  const [warnings, setWarnings] = useState<string[]>([])
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    fetch('http://localhost:5080/api/prs')
      .then(async (res) => {
        if (!res.ok) {
          const body = await res.text()
          throw new Error(body || 'Backend returned an error')
        }
        return res.json()
      })
      .then((json: PrResponse) => {
        setPrs(json.prs)
        setWarnings(json.warnings || [])
        setStatus('loaded')
      })
      .catch((err) => {
        setErrorMessage(err.message)
        setStatus('error')
      })
  }, [])

  return (
    <div className="page">
      <h1>PR Review Buddy</h1>
      <p className="subtitle">Unified PR Queue — GitHub + Azure DevOps</p>

      {status === 'loading' && <p className="status loading">Fetching pull requests…</p>}

      {status === 'error' && (
        <div className="status error">
          <p>⚠️ Could not load pull requests.</p>
          <p className="detail">{errorMessage}</p>
        </div>
      )}

      {status === 'loaded' && warnings.length > 0 && (
        <div className="status warning">
          {warnings.map((w, i) => (
            <p key={i} className="detail">⚠️ {w}</p>
          ))}
        </div>
      )}

      {status === 'loaded' && prs.length === 0 && (
        <p className="status loading">No open pull requests found on either platform.</p>
      )}

      {status === 'loaded' && prs.length > 0 && (
        <ul className="pr-list">
          {prs.map((pr) => (
            <li key={`${pr.source}-${pr.number}`} className="pr-card">
              <div className="pr-card-header">
                <a href={pr.htmlUrl} target="_blank" rel="noreferrer" className="pr-title">
                  #{pr.number} {pr.title}
                </a>
                <div className="badge-group">
                  <SourceBadge source={pr.source} />
                  {pr.isDraft && <span className="badge draft">Draft</span>}
                </div>
              </div>
              <p className="pr-meta">
                Opened by <strong>{pr.authorLogin}</strong> on {new Date(pr.createdAt).toLocaleDateString()}
              </p>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

export default App
