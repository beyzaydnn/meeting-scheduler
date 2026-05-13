import { Routes, Route } from 'react-router-dom'
import HomePage from './pages/HomePage'
import MeetingPage from './pages/MeetingPage'

function App() {
  return (
    <div className='container'>
      <h1>Meeting Scheduler</h1>
      <Routes>
        <Route path='/' element={<HomePage />} />
        <Route path='/m/:shareCode' element={<MeetingPage />} />
      </Routes>
    </div>
  )
}

export default App
