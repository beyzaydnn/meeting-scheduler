import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { createMeeting } from '../api'

function HomePage() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    creatorName: '',
    deadline: '',
    includeWeekends: false,
    selectAvailable: true
  })

  const handleSubmit = async (e) => {
    e.preventDefault()
    const result = await createMeeting(form)
    if (result.shareCode) {
      navigate('/m/' + result.shareCode)
    }
  }

  return (
    <div className='card'>
      <h2>Create a New Meeting</h2>
      <p className='subtitle'>Find the perfect day for your group to meet.</p>
      <form onSubmit={handleSubmit}>
        <div className='form-group'>
          <label>Event Name</label>
          <input type='text' placeholder='e.g. Team Dinner, Study Group' value={form.creatorName} onChange={e => setForm({ ...form, creatorName: e.target.value })} required />
        </div>
        <div className='form-group'>
          <label>Deadline</label>
          <input type='date' value={form.deadline} onChange={e => setForm({ ...form, deadline: e.target.value })} min={new Date().toISOString().split('T')[0]} required />
        </div>
        <div className='checkbox-group'>
          <input type='checkbox' id='weekends' checked={form.includeWeekends} onChange={e => setForm({ ...form, includeWeekends: e.target.checked })} />
          <label htmlFor='weekends'>Include weekends</label>
        </div>
        <div className='form-group'>
          <label>Selection Mode</label>
          <select value={form.selectAvailable} onChange={e => setForm({ ...form, selectAvailable: e.target.value === 'true' })}>
            <option value='true'>Select available days</option>
            <option value='false'>Select unavailable days</option>
          </select>
        </div>
        <button type='submit' className='btn btn-full'>Create & Get Link</button>
      </form>
    </div>
  )
}

export default HomePage
