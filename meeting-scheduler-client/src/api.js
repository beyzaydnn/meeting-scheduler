const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5001/api';

export async function createMeeting(data) {
  const res = await fetch(API_BASE + '/meetings', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  return res.json();
}

export async function getMeeting(shareCode) {
  const res = await fetch(API_BASE + '/meetings/' + shareCode);
  if (!res.ok) return null;
  return res.json();
}

export async function joinMeeting(shareCode, data) {
  const res = await fetch(API_BASE + '/meetings/' + shareCode + '/join', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  return res.json();
}

export async function getResults(shareCode) {
  const res = await fetch(API_BASE + '/meetings/' + shareCode + '/results');
  return res.json();
}

export async function spinWheel(shareCode) {
  const res = await fetch(API_BASE + '/meetings/' + shareCode + '/spin');
  if (!res.ok) return null;
  return res.json();
}
