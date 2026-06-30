const BASE_URL = 'http://localhost:5179/api';

async function apiFetch(endpoint, options = {}) {
    const token = localStorage.getItem('token');
    const headers = { 'Content-Type': 'application/json' };
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const response = await fetch(`${BASE_URL}${endpoint}`, {
        ...options,
        headers: { ...headers, ...options.headers }
    });

    if (!response.ok) {
        const error = await response.json().catch(() => ({}));
        throw new Error(error.message || 'Something went wrong');
    }

    return response.json();
}