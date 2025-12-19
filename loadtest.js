// JavaScript source code
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '10s', target: 50 },  // In first 10s, increasing to 50 virtual users
    { duration: '30s', target: 50 },  // 50 users send continously
    { duration: '10s', target: 0 },   
  ],
};

export default function () {
    const payload = JSON.stringify({
        vehicleId: Math.floor(Math.random() * 3) + 1,
        speed: Math.floor(Math.random() * 150),
        temperature: 25,
        latitude: 21.0285,
        longitude: 105.8542,
        timestamp: new Date().toISOString()
    });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const res = http.post('http://localhost:5294/api/telemetry', payload, params);

    check(res, {
        'status is 200 or 202': (r) => r.status === 200 || r.status === 202,
    });

    if (res.status !== 200 && res.status !== 202) {
        console.log(`Error: ${res.status} - ${res.body}`);
    }

  sleep(1); 
}