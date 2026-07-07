const BASE_URL = 'http://localhost:5179/api';

async function apiFetch(endpoint, options = {}) {
    const token = localStorage.getItem('token');
    const isFormData = options.body instanceof FormData;

    const headers = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const settings = {
        url: `${BASE_URL}${endpoint}`,
        method: options.method || 'GET',
        data: options.body,
        headers: { ...headers, ...options.headers },
        dataType: 'json'
    };

    if (isFormData) {
        settings.processData = false;
        settings.contentType = false;
    } else {
        settings.contentType = 'application/json';
        headers['Content-Type'] = 'application/json';
    }

    try {
        return await $.ajax(settings);
    } catch (xhr) {
        const error = xhr.responseJSON || {};
        throw new Error(error.message || 'Something went wrong');
    }
}