import { useState, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { getMeeting, joinMeeting, getResults, spinWheel } from '../api'

function MeetingPage() {
  const { shareCode } = useParams()
  const [meeting, setMeeting] = useState(null)
  const [name, setName] = useState('')
  const [selectedDates, setSelectedDates] = useState([])
  const [results, setResults] = useState(null)
  const [spinResult, setSpinResult] = useState(null)
  const [submitted, setSubmitted] = useState(false)
  const [copied, setCopied] = useState(false)
  const [spinning, setSpinning] = useState(false)
  const [wheelRotation, setWheelRotation] = useState(0)

  useEffect(() => {
    loadMeeting()
    const interval = setInterval(loadMeeting, 5000)
    return () => clearInterval(interval)
  }, [shareCode])

  const loadMeeting = async () => { const data = await getMeeting(shareCode); setMeeting(data) }

  const getDates = () => {
    if (!meeting) return []
    const dates = []
    const today = new Date(); today.setHours(0,0,0,0)
    const deadline = new Date(meeting.deadline); deadline.setHours(0,0,0,0)
    let current = new Date(today)
    while (current <= deadline) {
      const day = current.getDay()
      if (meeting.includeWeekends || (day !== 0 && day !== 6)) dates.push(new Date(current))
      current.setDate(current.getDate() + 1)
    }
    return dates
  }

  const toggleDate = (date) => {
    const dateStr = date.toISOString().split('T')[0]
    setSelectedDates(prev => prev.includes(dateStr) ? prev.filter(d => d !== dateStr) : [...prev, dateStr])
  }

  const handleJoin = async (e) => {
    e.preventDefault()
    await joinMeeting(shareCode, { name, dates: selectedDates.map(d => d + 'T00:00:00Z') })
    setSubmitted(true)
    loadMeeting()
  }

  const handleResults = async () => { const data = await getResults(shareCode); setResults(data) }

  const handleSpin = async () => {
    if (spinning) return
    setSpinning(true)
    setSpinResult(null)
    const extraSpins = 360 * 5 + Math.random() * 360
    setWheelRotation(prev => prev + extraSpins)
    const data = await spinWheel(shareCode)
    setTimeout(() => {
      if (data) setSpinResult(data.selectedDate)
      setSpinning(false)
    }, 3000)
  }

  const formatDate = (d) => new Date(d).toLocaleDateString('en-US', { weekday: 'short', day: 'numeric', month: 'short' })

  const generateWheelBackground = (dates) => {
    const colors = ['#667eea','#764ba2','#5a67d8','#9f7aea','#4c51bf','#6b46c1','#7c3aed','#8b5cf6']
    const segAngle = 360 / dates.length
    const stops = dates.map((_, i) => {
      const color = colors[i % colors.length]
      return color + ' ' + (segAngle * i) + 'deg ' + (segAngle * (i + 1)) + 'deg'
    })
    return 'conic-gradient(' + stops.join(', ') + ')'
  }

  const copyLink = () => { navigator.clipboard.writeText(shareUrl); setCopied(true); setTimeout(() => setCopied(false), 2000) }

  if (!meeting) return <div className='card loading'>Loading...</div>
  const shareUrl = window.location.origin + '/meeting-scheduler/m/' + shareCode

  return (
    <>
      <div className='card'>
        <div className='meeting-header'>
          <h2>{meeting.creatorName}</h2>
          <span className='badge-mode'>{meeting.selectAvailable ? 'Select available' : 'Select unavailable'}</span>
        </div>
        <p><strong>Deadline:</strong> {formatDate(meeting.deadline)}</p>
        <div className='share-link'>
          <span>{shareUrl}</span>
          <button className='btn btn-small' onClick={copyLink}>{copied ? 'Copied!' : 'Copy'}</button>
        </div>
        <div className='participants-list'>
          <strong>Participants ({meeting.participants?.length || 0}):</strong>
          <div>
            {meeting.participants?.map(p => (<span key={p.id} className='participant-badge'>{p.name}</span>))}
            {(!meeting.participants || meeting.participants.length === 0) && <span className='empty-state'>No one has joined yet. Share the link!</span>}
          </div>
        </div>
      </div>

      {!submitted ? (
        <div className='card'>
          <h3>Join this Meeting</h3>
          <form onSubmit={handleJoin}>
            <div className='form-group'>
              <label>Your Name</label>
              <input type='text' placeholder='Enter your name' value={name} onChange={e => setName(e.target.value)} required />
            </div>
            <label className='date-label'>{meeting.selectAvailable ? 'Select the days you are available:' : 'Select the days you are NOT available:'}</label>
            <p className='date-hint'>{selectedDates.length} day(s) selected</p>
            <div className='date-grid'>
              {getDates().map(date => {
                const dateStr = date.toISOString().split('T')[0]
                return (<div key={dateStr} className={'date-cell ' + (selectedDates.includes(dateStr) ? 'selected' : '')} onClick={() => toggleDate(date)}>{formatDate(date)}</div>)
              })}
            </div>
            <button type='submit' className='btn btn-full' disabled={selectedDates.length === 0}>Submit</button>
          </form>
        </div>
      ) : (
        <div className='card success-card'><p>Your selection has been saved!</p></div>
      )}

      <div className='card'>
        <h3>Results</h3>
        <button className='btn' onClick={handleResults}>Show Common Days</button>
        {results && (
          <>
            <p style={{marginTop:'1rem'}}>Common days from <strong>{results.participantCount}</strong> participant(s):</p>
            {results.commonDates?.length > 0 ? (
              <>
                <div className='wheel-container'>
                  <div className='wheel-pointer'>&#9660;</div>
                  <div className='wheel' style={{transform: 'rotate(' + wheelRotation + 'deg)', background: generateWheelBackground(results.commonDates)}}>
                    {results.commonDates.map((d, i) => {
                      const segAngle = 360 / results.commonDates.length
                      const rotate = segAngle * i + segAngle / 2 - 90
                      return (<span key={d} className='wheel-text' style={{transform: 'rotate(' + rotate + 'deg) translateX(90px)'}}>{formatDate(d)}</span>)
                    })}
                  </div>
                </div>
                <button className='btn btn-spin' onClick={handleSpin} disabled={spinning}>{spinning ? 'Spinning...' : 'Spin the Wheel!'}</button>
                {spinResult && <div className='spin-result'>{formatDate(spinResult)}</div>}
                <ul className='results-list'>{results.commonDates.map(d => (<li key={d}>{formatDate(d)}</li>))}</ul>
              </>
            ) : (<p className='empty-state'>No common available days found</p>)}
          </>
        )}
      </div>
    </>
  )
}

export default MeetingPage
