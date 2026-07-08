const HUB_BASE = FILE_BASE_URL;
let currentUser = null;
let hubConnection = null;
let inbox = [];
let activeUserId = null;
let activeUserName = '';

$(function () {
    currentUser = JSON.parse(localStorage.getItem('user') || 'null');
    if (!currentUser || !localStorage.getItem('token')) {
        window.location.href = 'login.html';
        return;
    }
    $('#backBtn').on('click', function () {
        window.location.href = currentUser.role === 'Employer' ? 'employerdashboard.html' : 'jobseekerdashboard.html';
    });

    connectHub();
    loadInbox();

    const params = new URLSearchParams(window.location.search);
    const targetId = params.get('userId');
    const targetName = params.get('name');
    if (targetId) {
        setTimeout(() => openConversation(parseInt(targetId), targetName || 'User'), 300);
    }
});

function connectHub() {
    hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${HUB_BASE}/hubs/chat`, { accessTokenFactory: () => localStorage.getItem('token') })
        .withAutomaticReconnect()
        .build();

    hubConnection.on('ReceiveMessage', function (msg) {
        if (activeUserId && msg.senderId === activeUserId) {
            appendMessageToThread(msg, false);
            hubConnection.invoke('MarkAsRead', activeUserId).catch(() => { });
        } else {
            showToast('New message received');
        }
        loadInbox();
    });

    hubConnection.start().catch(err => console.error('SignalR connection failed:', err));
}

async function loadInbox() {
    try {
        const r = await apiFetch('/Message/inbox');
        inbox = r.data || [];
    } catch {
        inbox = [];
    }
    renderInbox();
}

function otherPerson(msg) {
    if (msg.senderId === currentUser.id) return { id: msg.receiverId, name: msg.receiverName };
    return { id: msg.senderId, name: msg.senderName };
}

function renderInbox() {
    const list = $('#convList');
    if (!inbox.length) {
        list.html('<div style="padding:20px;color:#94a3b8;font-size:0.85rem;">No conversations yet.</div>');
        return;
    }
    list.html(inbox.map(m => {
        const other = otherPerson(m);
        const unread = !m.isRead && m.senderId !== currentUser.id;
        const activeClass = other.id === activeUserId ? 'active' : '';
        return `<div class="msg-conv-item ${activeClass}" data-id="${other.id}" data-name="${other.name}">
            <div class="msg-avatar">${(other.name || '?')[0].toUpperCase()}</div>
            <div class="msg-conv-info">
                <div class="msg-conv-name"><span>${other.name}</span>${unread ? '<span class="msg-unread-dot"></span>' : ''}</div>
                <div class="msg-conv-preview">${m.content}</div>
            </div>
        </div>`;
    }).join(''));

    $('.msg-conv-item').off('click').on('click', function () {
        openConversation(parseInt($(this).data('id')), $(this).data('name'));
    });
}

async function openConversation(userId, userName) {
    activeUserId = userId;
    activeUserName = userName;
    $('.msg-conv-item').removeClass('active');
    $(`.msg-conv-item[data-id="${userId}"]`).addClass('active');

    $('#msgMain').html(`
        <div class="msg-main-header"><div class="msg-avatar" style="width:34px;height:34px;font-size:0.85rem">${(userName || '?')[0].toUpperCase()}</div> ${userName}</div>
        <div class="msg-list" id="msgList"></div>
        <div class="msg-composer">
            <input type="text" id="msgInput" placeholder="Type a message..." />
            <button class="btn-primary" id="sendBtn"><i class="ti ti-send"></i></button>
        </div>
    `);

    $('#sendBtn').on('click', sendMessage);
    $('#msgInput').on('keydown', function (e) { if (e.key === 'Enter') sendMessage(); });

    try {
        const r = await apiFetch('/Message/conversation/' + userId);
        const messages = r.data || [];
        $('#msgList').html(messages.map(m => renderBubble(m)).join(''));
        scrollToBottom();
    } catch (err) {
        showToast(err.message || 'Failed to load conversation', false);
    }

    if (hubConnection && hubConnection.state === signalR.HubConnectionState.Connected) {
        hubConnection.invoke('MarkAsRead', userId).catch(() => { });
    }
}

function renderBubble(m) {
    const mine = m.senderId === currentUser.id;
    const time = new Date(m.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    return `<div class="msg-row ${mine ? 'me' : 'them'}">
        <div>
            <div class="msg-bubble">${m.content}</div>
            <div class="msg-time" style="text-align:${mine ? 'right' : 'left'}">${time}</div>
        </div>
    </div>`;
}

function appendMessageToThread(m, mine) {
    $('#msgList').append(renderBubble({ ...m, senderId: mine ? currentUser.id : m.senderId }));
    scrollToBottom();
}

function scrollToBottom() {
    const list = $('#msgList');
    if (list.length) list.scrollTop(list[0].scrollHeight);
}

async function sendMessage() {
    const content = $('#msgInput').val().trim();
    if (!content || !activeUserId) return;
    $('#msgInput').val('');

    try {
        if (hubConnection && hubConnection.state === signalR.HubConnectionState.Connected) {
            await hubConnection.invoke('SendMessage', activeUserId, content);
        } else {
            await apiFetch('/Message', { method: 'POST', body: JSON.stringify({ receiverId: activeUserId, content }) });
        }
        appendMessageToThread({ content, createdAt: new Date().toISOString() }, true);
        loadInbox();
    } catch (err) {
        showToast(err.message || 'Failed to send message', false);
    }
}