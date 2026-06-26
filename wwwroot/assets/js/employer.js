const colors = [
    { bg: '#eff6ff', color: '#2563EB' }, { bg: '#ecfdf5', color: '#10b981' },
    { bg: '#fffbeb', color: '#f59e0b' }, { bg: '#f0fdf4', color: '#16a34a' }, { bg: '#fdf4ff', color: '#a855f7' }
];
let myJobs = [], myApplicants = [];

function getUser() { const u = localStorage.getItem('user'); return u ? JSON.parse(u) : null; }
function logout() { localStorage.removeItem('token'); localStorage.removeItem('user'); window.location.href = 'login.html'; }

function showTab(tab, el) {
    ['overview', 'jobs', 'applicants', 'profile'].forEach(t => {
        const e = document.getElementById('tab-' + t);
        if (e) e.style.display = 'none';
    });
    document.getElementById('tab-' + tab).style.display = 'block';
    document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
    if (el) el.classList.add('active');
    const titles = {
        overview: ['Overview', "Welcome back! Here's what's happening."],
        jobs: ['My Jobs', 'Manage your job listings'],
        applicants: ['Applicants', 'Review and manage candidates'],
        profile: ['Company Profile', 'Update your company information']
    };
    document.getElementById('pageTitle').textContent = titles[tab][0];
    document.getElementById('pageSubtitle').textContent = titles[tab][1];
    if (tab === 'applicants') loadApplicants();
    if (tab === 'jobs') loadJobs();
    if (tab === 'profile') loadProfile();
}

function openModal() { document.getElementById('modalOverlay').classList.add('open'); }
function closeModal() { document.getElementById('modalOverlay').classList.remove('open'); }
function closeModalOutside(e) { if (e.target === document.getElementById('modalOverlay')) closeModal(); }

function showToast(msg, ok = true) {
    const t = document.getElementById('toast');
    t.querySelector('i').style.color = ok ? '#10b981' : '#ef4444';
    document.getElementById('toastMsg').textContent = msg;
    t.classList.add('show');
    setTimeout(() => t.classList.remove('show'), 3000);
}

// ── JOBS ──
async function loadJobs() {
    try { const r = await apiFetch('/Job/my-jobs'); myJobs = r.data || []; } catch { myJobs = []; }
    renderJobs();
    document.getElementById('totalJobs').textContent = myJobs.length;
    document.getElementById('jobsBadge').textContent = myJobs.length;
    document.getElementById('jobsSubtitle').textContent = myJobs.length + ' positions posted';
}

function jobItemHTML(job, i) {
    const c = colors[i % colors.length];
    const letter = (job.title || 'J')[0].toUpperCase();
    return `<div class="job-item">
        <div class="job-logo" style="background:${c.bg};color:${c.color}">${letter}</div>
        <div class="job-info">
            <div class="job-name">${job.title || 'Job'}</div>
            <div class="job-meta"><span><i class="ti ti-map-pin"></i> ${job.location || 'Remote'}</span><span><i class="ti ti-clock"></i> ${job.jobType || 'Full-time'}</span></div>
        </div>
        <span class="job-status status-active">Active</span>
        <div class="job-actions">
            <button class="act-btn danger" title="Delete" onclick="deleteJob(${job.id})"><i class="ti ti-trash"></i></button>
        </div>
    </div>`;
}

function renderJobs() {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-briefcase-off"></i></div><div class="empty-title">No jobs posted yet</div><div class="empty-sub">Click "Post a Job" to get started</div></div>`;
    if (!myJobs.length) {
        document.getElementById('recentJobsList').innerHTML = empty;
        document.getElementById('allJobsList').innerHTML = empty;
        return;
    }
    document.getElementById('recentJobsList').innerHTML = myJobs.slice(0, 5).map(jobItemHTML).join('');
    document.getElementById('allJobsList').innerHTML = myJobs.map(jobItemHTML).join('');
}

// ── APPLICANTS ──
async function loadApplicants() {
    try { const r = await apiFetch('/Employer/applications'); myApplicants = r.data || []; } catch { myApplicants = []; }
    renderApplicants();
    document.getElementById('totalApps').textContent = myApplicants.length;
    document.getElementById('appsBadge').textContent = myApplicants.length;
    document.getElementById('miniNew').textContent = myApplicants.filter(a => a.status === 'Applied').length;
}

function renderApplicants() {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-user-off"></i></div><div class="empty-title">No applicants yet</div><div class="empty-sub">Applicants will appear here</div></div>`;
    if (!myApplicants.length) {
        document.getElementById('recentAppsList').innerHTML = empty;
        document.getElementById('allApplicantsList').innerHTML = empty;
        return;
    }
    const badgeMap = { Applied: 'badge-new', Reviewed: 'badge-review', Shortlisted: 'badge-hired', Rejected: 'badge-rejected' };
    const html = myApplicants.map((app, i) => {
        const c = colors[i % colors.length];
        const name = app.jobSeeker?.fullName || 'Applicant';
        const cvUrl = app.jobSeeker?.cvUrl;
        const userId = app.jobSeeker?.userId || '';
        return `<div class="applicant-item">
    <div class="app-avatar" style="background:${c.bg};color:${c.color}">${name[0].toUpperCase()}</div>
    <div style="flex:1">
        <div class="app-name">${name}</div>
        <div class="app-role">${app.jobTitle || 'Position'} · ${new Date(app.appliedAt).toLocaleDateString()}</div>
        ${app.jobSeeker?.skills ? `<div style="font-size:0.75rem;color:#64748b;margin-top:2px">${app.jobSeeker.skills}</div>` : ''}
    </div>
    <a href="/assets/pages/profile.html?id=${userId}" class="act-btn" title="View Profile"><i class="ti ti-user"></i></a>
    ${cvUrl ? `<a href="http://localhost:5076${cvUrl}" target="_blank" class="act-btn" title="View CV"><i class="ti ti-file-text"></i></a>` : ''}
    <span class="app-badge ${badgeMap[app.status] || 'badge-new'}">${app.status || 'Applied'}</span>
    <select class="status-select" onchange="updateStatus(${app.id}, this.value)">
        <option value="">Change status</option>
        <option value="Reviewed">Reviewed</option>
        <option value="Shortlisted">Shortlisted</option>
        <option value="Rejected">Rejected</option>
    </select>
</div>`;
    }).join('');
    document.getElementById('recentAppsList').innerHTML = html;
    document.getElementById('allApplicantsList').innerHTML = html;
}

async function updateStatus(appId, status) {
    if (!status) return;
    try {
        await apiFetch(`/Employer/applications/${appId}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status, notes: '' })
        });
        showToast(`Status updated to ${status}`);
        await loadApplicants();
    } catch (e) { showToast(e.message, false); }
}

// ── POST JOB ──
async function postJob() {
    const title = document.getElementById('jobTitle').value.trim();
    const location = document.getElementById('jobLocation').value.trim();
    const description = document.getElementById('jobDesc').value.trim();
    if (!title || !location || !description) { showToast('Please fill required fields', false); return; }
    try {
        await apiFetch('/Job', {
            method: 'POST', body: JSON.stringify({
                title, location, description,
                requirements: document.getElementById('jobReqs').value.trim(),
                jobType: document.getElementById('jobType').value,
                category: document.getElementById('jobCategory').value,
                salaryMin: parseInt(document.getElementById('salaryMin').value) || 0,
                salaryMax: parseInt(document.getElementById('salaryMax').value) || 0,
                deadline: document.getElementById('jobDeadline').value || null
            })
        });
        closeModal();
        showToast('Job posted successfully!');
        ['jobTitle', 'jobLocation', 'jobDesc', 'jobReqs'].forEach(id => document.getElementById(id).value = '');
        await loadJobs();
    } catch (err) { showToast(err.message || 'Failed to post job', false); }
}

async function deleteJob(id) {
    if (!confirm('Delete this job?')) return;
    try { await apiFetch('/Job/' + id, { method: 'DELETE' }); showToast('Job deleted'); await loadJobs(); }
    catch (err) { showToast(err.message || 'Failed', false); }
}

// ── PROFILE ──
async function loadProfile() {
    try {
        const r = await apiFetch('/Employer/profile');
        const p = r.data;
        if (document.getElementById('companyName')) document.getElementById('companyName').value = p.companyName || '';
        if (document.getElementById('companyDesc')) document.getElementById('companyDesc').value = p.description || '';
        if (document.getElementById('companyWebsite')) document.getElementById('companyWebsite').value = p.website || '';
    } catch { }
}

async function saveProfile() {
    const dto = {
        companyName: document.getElementById('companyName').value,
        description: document.getElementById('companyDesc').value,
        website: document.getElementById('companyWebsite').value
    };
    try {
        let exists = false;
        try { await apiFetch('/Employer/profile'); exists = true; } catch { }
        if (exists) {
            await apiFetch('/Employer/profile', { method: 'PUT', body: JSON.stringify(dto) });
        } else {
            await apiFetch('/Employer/profile', { method: 'POST', body: JSON.stringify(dto) });
        }
        showToast('Profile saved!');
    } catch (e) { showToast(e.message, false); }
}

// ── CHART ──
function renderChart() {
    const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'], values = [3, 7, 5, 9, 6, 4, 8], max = Math.max(...values);
    const el = document.getElementById('chartArea');
    if (el) el.innerHTML = days.map((d, i) =>
        `<div class="bar-wrap"><div class="bar ${i === 6 ? 'active' : ''}" style="height:${(values[i] / max) * 130}px"></div><span class="bar-label">${d}</span></div>`).join('');
}

// ── INIT ──
window.addEventListener('DOMContentLoaded', async () => {
    const user = getUser();
    if (!user || user.role !== 'Employer') { window.location.href = 'login.html'; return; }
    const name = user.firstName || user.username || user.email || 'Employer';
    document.getElementById('sidebarName').textContent = name;
    document.getElementById('sidebarAvatar').textContent = name[0].toUpperCase();
    renderChart();
    await Promise.all([loadJobs(), loadApplicants()]);
});