const NOTIF_HUB_BASE = FILE_BASE_URL;
let notifConnection = null;

$(function () {
    if (!localStorage.getItem('token')) return;

    loadNotifications();
    connectNotifHub();

    $('#notifBellBtn').on('click', function (e) {
        e.stopPropagation();
        $('#notifDropdown').toggleClass('open');
    });
    $('#notifDropdown').on('click', function (e) { e.stopPropagation(); });
    $(document).on('click', function () {
        $('#notifDropdown').removeClass('open');
    });
});

function notifIcon(type) {
    const map = { contract: 'ti-file-text', payment: 'ti-credit-card', review: 'ti-star', application: 'ti-briefcase', message: 'ti-message' };
    return map[type] || 'ti-bell';
}

function timeAgo(dateStr) {
    const diff = (Date.now() - new Date(dateStr)) / 1000;
    if (diff < 60) return 'just now';
    if (diff < 3600) return Math.floor(diff / 60) + 'm ago';
    if (diff < 86400) return Math.floor(diff / 3600) + 'h ago';
    return Math.floor(diff / 86400) + 'd ago';
}

async function loadNotifications() {
    try {
        const r = await apiFetch('/Notification');
        const items = r.data || [];
        renderDropdown(items);
        $('#notifDot').toggle(items.some(n => !n.isRead));
    } catch { }
}

function renderDropdown(items) {
    const list = $('#notifList');
    if (!items.length) {
        list.html('<div style="padding:16px;color:#94a3b8;font-size:0.8rem">No notifications yet.</div>');
        return;
    }
    list.html(items.slice(0, 8).map(n => `
        <div class="notif-item ${n.isRead ? '' : 'unread'}" onclick="notifClick(${n.id})">
            <div class="notif-icon"><i class="ti ${notifIcon(n.type)}"></i></div>
            <div>
                <div class="notif-item-title">${escapeHtml(n.title)}</div>
                <div class="notif-item-msg">${escapeHtml(n.message)}</div>
                <div class="notif-item-time">${timeAgo(n.createdAt)}</div>
            </div>
        </div>
    `).join(''));
}

async function notifClick(id) {
    try {
        await apiFetch('/Notification/' + id, { method: 'PUT' });
        loadNotifications();
    } catch { }
}

async function markAllRead() {
    try {
        await apiFetch('/Notification/read-all', { method: 'PUT' });
        loadNotifications();
    } catch { }
}

function connectNotifHub() {
    notifConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${NOTIF_HUB_BASE}/hubs/notification`, { accessTokenFactory: () => localStorage.getItem('token') })
        .withAutomaticReconnect()
        .build();

    notifConnection.on('ReceiveNotification', function () {
        loadNotifications();
    });

    notifConnection.start().catch(err => console.error('Notification hub failed:', err));
}