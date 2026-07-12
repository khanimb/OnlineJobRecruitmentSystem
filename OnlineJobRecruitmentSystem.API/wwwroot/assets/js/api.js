const FILE_BASE_URL = window.location.origin;
const BASE_URL = `${FILE_BASE_URL}/api`;

const colors = [
    { bg: '#DBEAFE', color: '#2563EB' },
    { bg: '#DCFCE7', color: '#16A34A' },
    { bg: '#FEF3C7', color: '#D97706' },
    { bg: '#F3E8FF', color: '#9333EA' },
    { bg: '#FEE2E2', color: '#DC2626' },
    { bg: '#E0F2FE', color: '#0284C7' },
    { bg: '#FCE7F3', color: '#DB2777' },
    { bg: '#E2E8F0', color: '#475569' }
];

async function apiFetch(endpoint, options = {}, isRetry = false) {
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
        if (xhr.status === 401 && token && !isRetry) {
            const refreshed = await tryRefreshToken();
            if (refreshed) return apiFetch(endpoint, options, true);

            localStorage.removeItem('token');
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('user');
            window.location.href = '/assets/pages/login.html';
            return;
        }
        const error = xhr.responseJSON || {};
        throw new Error(error.message || 'Something went wrong');
    }
}

async function tryRefreshToken() {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) return false;
    try {
        const res = await $.ajax({
            url: `${BASE_URL}/Auth/refresh-token`,
            method: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: JSON.stringify({ refreshToken })
        });
        localStorage.setItem('token', res.data.token);
        localStorage.setItem('refreshToken', res.data.refreshToken);
        return true;
    } catch {
        return false;
    }
}

function escapeHtml(str) {
    return String(str ?? '').replace(/[&<>"']/g, m => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[m]));
}

function safeInitial(str) {
    const m = String(str ?? '').match(/[a-zA-Z0-9]/);
    return m ? m[0].toUpperCase() : '?';
}

function showToast(msg, ok = true) {
    const t = $('#toast');
    t.find('i').css('color', ok ? '#10b981' : '#ef4444');
    $('#toastMsg').text(msg);
    t.addClass('show');
    setTimeout(() => t.removeClass('show'), 3000);
}

function renderList(containerSel, items, templateFn, emptyHtml) {
    const el = $(containerSel);
    if (!el.length) return;
    if (!items.length) {
        el.html(emptyHtml || '<div class="empty-state"><div class="empty-title">Nothing here yet</div></div>');
        return;
    }
    el.html(items.map(templateFn).join(''));
}