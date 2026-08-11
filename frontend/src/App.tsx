import { useEffect, useState } from 'react'
import './App.css'

type HelloResponse = {
  message: string
  timestampUtc: string
}

function App() {
  const [status, setStatus] = useState<'loading' | 'connected' | 'error'>('loading')
  const [data, setData] = useState<HelloResponse | null>(null)

  useEffect(() => {
    fetch('http://localhost:5080/api/hello')
      .then((res) => {
        if (!res.ok) throw new Error('Backend responded with an error')
        return res.json()
      })
      .then((json: HelloResponse) => {
        setData(json)
        setStatus('connected')
      })
      .catch(() => setStatus('error'))
  }, [])

  return (
    <div className="page">
      <h1>PR Review Buddy</h1>
      <p className="subtitle">Local development heartbeat check</p>

      {status === 'loading' && <p className="status loading">Checking connection to backend…</p>}

      {status === 'connected' && data && (
        <div className="status connected">
          <p>✅ Frontend is successfully talking to the backend.</p>
          <p className="detail">"{data.message}"</p>
          <p className="detail">Backend time: {new Date(data.timestampUtc).toLocaleString()}</p>
        </div>
      )}

      {status === 'error' && (
        <div className="status error">
          <p>⚠️ Could not reach the backend.</p>
          <p className="detail">Make sure the backend is running at http://localhost:5080</p>
        </div>
      )}
    </div>
  )
}

export default App
