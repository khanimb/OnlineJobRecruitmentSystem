

async function loadComments() {
    const id = new URLSearchParams(window.location.search).get('id');
    try {
        const r = await apiFetch('/Comment/jobpost/' + id);
        renderComments(r.data);
    } catch {
        $('#commentsList').html('<p>Could not load comments.</p>');
    }
}

function renderComments(comments) {
    const user = getUser ? getUser() : null;

    if (!comments || comments.length === 0) {
        $('#commentsList').html('<p style="color:#6b7280">No comments yet.</p>');
    } else {
        $('#commentsList').html(comments.map(c => `
            <div class="comment-item" id="comment-${c.id}" style="padding:12px 0; border-bottom:1px solid #f3f4f6;">
                <div style="font-weight:600;">${escapeHtml(c.username)}</div>
                <div class="comment-text" style="color:#374151; margin:4px 0;">${escapeHtml(c.text)}</div>
                <div style="font-size:12px; color:#9ca3af;">${new Date(c.createdAt).toLocaleString()}</div>
                ${getCurrentUserId() === c.userId ? `
                    <button onclick="editComment(${c.id})" style="font-size:12px; color:#2563eb; background:none; border:none; cursor:pointer; margin-right:8px;">Edit</button>
                    <button onclick="deleteComment(${c.id})" style="font-size:12px; color:#ef4444; background:none; border:none; cursor:pointer;">Delete</button>
                ` : ''}
            </div>
        `).join(''));
    }

    if (user) {
        $('#commentForm').show();
    }
}

async function postComment() {
    const user = getUser ? getUser() : null;
    if (!user) { window.location.href = 'login.html'; return; }

    const id = new URLSearchParams(window.location.search).get('id');
    const text = $('#commentText').val().trim();
    if (!text) return;

    try {
        await apiFetch('/Comment', {
            method: 'POST',
            body: JSON.stringify({ jobPostId: parseInt(id), text })
        });
        $('#commentText').val('');
        loadComments();
        showToast('Comment posted!');
    } catch (err) {
        showToast(err.message || 'Failed to post comment', false);
    }
}

function editComment(id) {
    const commentDiv = $('#comment-' + id);
    const textDiv = commentDiv.find('.comment-text');
    const currentText = textDiv.text();
    textDiv.html(`
        <textarea id="editText-${id}" rows="2" style="width:100%; padding:8px; border-radius:6px; border:1px solid #e5e7eb;">${escapeHtml(currentText)}</textarea>
        <button onclick="saveEdit(${id})" style="margin-top:6px;" class="btn-primary">Save</button>
    `);
}

async function saveEdit(id) {
    const text = $('#editText-' + id).val().trim();
    if (!text) return;

    try {
        await apiFetch('/Comment/' + id, {
            method: 'PUT',
            body: JSON.stringify({ text })
        });
        loadComments();
        showToast('Comment updated!');
    } catch (err) {
        showToast(err.message || 'Failed to update comment', false);
    }
}

async function deleteComment(id) {
    try {
        await apiFetch('/Comment/' + id, { method: 'DELETE' });
        loadComments();
        showToast('Comment deleted!');
    } catch (err) {
        showToast(err.message || 'Failed to delete comment', false);
    }
}

$(function () {
    loadComments();
});