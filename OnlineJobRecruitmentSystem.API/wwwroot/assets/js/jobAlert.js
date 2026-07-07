// ── JOB ALERTS ──
let myAlerts = [];
let editingAlertId = null;

async function loadJobAlerts() {
    try { const r = await apiFetch('/JobAlert'); myAlerts = r.data || []; } catch { myAlerts = []; }
    renderJobAlerts();
}

function renderJobAlerts() {
    if (!myAlerts.length) {
        $('#alertsList').html(`<div class="empty-state"><div class="empty-icon"><i class="ti ti-bell-off"></i></div><div class="empty-title">No job alerts yet</div><div class="empty-sub">Get notified when matching jobs are posted</div></div>`);
        return;
    }
    $('#alertsList').html(myAlerts.map((a, i) => {
        const c = colors[i % colors.length];
        return `<div class="job-item">
            <div class="job-logo" style="background:${c.bg};color:${c.color}"><i class="ti ti-bell"></i></div>
            <div class="job-info">
                <div class="job-name">${a.keyword || 'Any keyword'}</div>
                <div class="job-meta"><span><i class="ti ti-map-pin"></i> ${a.location || 'Any location'}</span><span><i class="ti ti-repeat"></i> ${a.frequency}</span></div>
            </div>
            <span class="tag ${a.isActive ? 'tag-teal' : 'tag-gray'}">${a.isActive ? 'Active' : 'Paused'}</span>
            <div style="display:flex;gap:8px;margin-left:10px">
                <button class="btn-outline" onclick="toggleJobAlert(${a.id}, ${a.isActive})">${a.isActive ? '<i class="ti ti-player-pause"></i>' : '<i class="ti ti-player-play"></i>'}</button>
                <button class="btn-outline" onclick="editJobAlert(${a.id})"><i class="ti ti-edit"></i></button>
                <button class="btn-danger" onclick="deleteJobAlert(${a.id})"><i class="ti ti-trash"></i></button>
            </div>
        </div>`;
    }).join(''));
}

async function submitJobAlert() {
    const dto = {
        keyword: $('#alertKeyword').val().trim(),
        location: $('#alertLocation').val().trim(),
        frequency: $('#alertFrequency').val()
    };
    try {
        if (editingAlertId) {
            const existing = myAlerts.find(a => a.id === editingAlertId);
            await apiFetch(`/JobAlert/${editingAlertId}`, { method: 'PUT', body: JSON.stringify({ ...dto, isActive: existing ? existing.isActive : true }) });
            showToast('Job alert updated!');
        } else {
            await apiFetch('/JobAlert', { method: 'POST', body: JSON.stringify(dto) });
            showToast('Job alert created!');
        }
        cancelAlertEdit();
        loadJobAlerts();
    } catch (e) { showToast(e.message, false); }
}

async function toggleJobAlert(id, currentlyActive) {
    const alert = myAlerts.find(a => a.id === id);
    if (!alert) return;
    try {
        await apiFetch(`/JobAlert/${id}`, { method: 'PUT', body: JSON.stringify({ keyword: alert.keyword, location: alert.location, frequency: alert.frequency, isActive: !currentlyActive }) });
        showToast(currentlyActive ? 'Alert paused' : 'Alert resumed');
        loadJobAlerts();
    } catch (e) { showToast(e.message, false); }
}

function editJobAlert(id) {
    const alert = myAlerts.find(a => a.id === id);
    if (!alert) return;
    editingAlertId = id;
    $('#alertKeyword').val(alert.keyword || '');
    $('#alertLocation').val(alert.location || '');
    $('#alertFrequency').val(alert.frequency);
    $('#alertFormTitle').text('Edit Job Alert');
    $('#alertSubmitBtn').text('Update Alert');
    $('#alertCancelBtn').css('display', 'inline-flex');
}

function cancelAlertEdit() {
    editingAlertId = null;
    $('#alertKeyword').val('');
    $('#alertLocation').val('');
    $('#alertFrequency').val('daily');
    $('#alertFormTitle').text('Create Job Alert');
    $('#alertSubmitBtn').text('Create Alert');
    $('#alertCancelBtn').hide();
}

async function deleteJobAlert(id) {
    if (!confirm('Delete this job alert?')) return;
    try {
        await apiFetch(`/JobAlert/${id}`, { method: 'DELETE' });
        showToast('Job alert deleted');
        loadJobAlerts();
    } catch (e) { showToast(e.message, false); }
}