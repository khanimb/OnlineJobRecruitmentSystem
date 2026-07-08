const BASE_URL = 'http://localhost:5179/api';
const FILE_BASE_URL = 'http://localhost:5179';

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
        if (xhr.status === 401 && token) {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            window.location.href = '/assets/pages/login.html';
            return;
        }
        const error = xhr.responseJSON || {};
        throw new Error(error.message || 'Something went wrong');
    }
}

function showToast(msg, ok = true) {
    const t = $('#toast');
    t.find('i').css('color', ok ? '#10b981' : '#ef4444');
    $('#toastMsg').text(msg);
    t.addClass('show');
    setTimeout(() => t.removeClass('show'), 3000);
}