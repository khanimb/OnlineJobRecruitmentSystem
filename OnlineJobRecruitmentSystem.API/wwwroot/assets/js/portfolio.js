const colors = [
    { bg: '#eff6ff', color: '#2563EB' }, { bg: '#ecfdf5', color: '#10b981' },
    { bg: '#fffbeb', color: '#f59e0b' }, { bg: '#f0fdf4', color: '#16a34a' }, { bg: '#fdf4ff', color: '#a855f7' }
];

let allUsers = [], allJobs = [], allApps = [];

function getUser() { const u = localStorage.getItem('user'); return u ? JSON.parse(u) : null; }
function logout() { localStorage.removeItem('token'); localStorage.removeItem('user'); window.location.href = 'login.html'; }

function showTab(tab, el) {
    ['overview', 'users', 'jobs', 'applications', 'payments'].forEach(t => document.getElementById('tab-' + t).style.display = 'none');
    document.getElementById('tab-' + tab).style.display = 'block';
    document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
    if (el) el.classList.add('active');
    const titles = {
        overview: ['Overview', 'Platform statistics and management.'],
        users: ['Users', 'Manage all registered users'],
        jobs: ['Jobs', 'Manage all job listings'],
        applications: ['Applications', 'View all applications'],
        payments: ['Payments', 'Transaction history']
    };
    document.getElementById('pageTitle').textContent = titles[tab][0];
    document.getElementById('pageSubtitle').textContent = titles[tab][1];
}

function showToast(msg, ok = true) {
    const t = document.getElementById('toast');
    t.querySelector('i').style.color = ok ? '#10b981' : '#ef4444';
    document.getElementById('toastMsg').textContent = msg;
    t.classList.add('show');
    setTimeout(() => t.classList.remove('show'), 3000);
}

async function loadUsers() {
    try { const r = await apiFetch('/Admin/users'); allUsers = r.data || []; } catch { allUsers = []; }
    renderUsers(allUsers);
    document.getElementById('totalUsers').textContent = allUsers.length;
    document.getElementById('usersBadge').textContent = allUsers.length;
    document.getElementById('usersSubtitle').textContent = allUsers.length + ' users';
}

async function loadJobs() {
    try { const r = await apiFetch('/Job'); allJobs = r.data?.data || []; } catch { allJobs = []; }
    renderJobs(allJobs);
    document.getElementById('totalJobs').textContent = allJobs.length;
    document.getElementById('jobsBadge').textContent = allJobs.length;
    document.getElementById('jobsSubtitle').textContent = allJobs.length + ' jobs';
}

async function loadApps() {
    try { const r = await apiFetch('/Admin/applications'); allApps = r.data || []; } catch { allApps = []; }
    renderApps(allApps);
    document.getElementById('totalApps').textContent = allApps.length;
    document.getElementById('appsBadge').textContent = allApps.length;
    document.getElementById('appsSubtitle').textContent = allApps.length + ' applications';
}

function filterUsers() {
    const q = document.getElementById('userSearch').value.toLowerCase();
    const filtered = allUsers.filter(u => (u.email || '').toLowerCase().includes(q) || (u.firstName || '').toLowerCase().includes(q) || (u.username || '').toLowerCase().includes(q));
    renderUsers(filtered);
}

function userItemHTML(user, i) {
    const c = colors[i % colors.length];
    const name = user.firstName ? `${user.firstName} ${user.lastName || ''}`.trim() : user.username || user.email;
    const roleColor = user.role === 'Admin' ? '#ef4444' : user.role === 'Employer' ? '#2563EB' : '#10b981';
    const roleBg = user.role === 'Admin' ? '#fef2f2' : user.role === 'Employer' ? '#eff6ff' : '#ecfdf5';
    return `<div class="job-item">
    <div class="job-logo" style="background:${c.bg};color:${c.color}">${name[0].toUpperCase()}</div>
    <div class="job-info">
      <div class="job-name">${name}</div>
      <div class="job-meta"><span><i class="ti ti-mail"></i> ${user.email || ''}</span></div>
    </div>
    <span class="app-badge" style="background:${roleBg};color:${roleColor}">${user.role || 'User'}</span>
    <div class="job-actions">
      <button class="act-btn danger" onclick="deleteUser(${user.id})" title="Delete"><i class="ti ti-trash"></i></button>
    </div>
  </div>`;
}

function renderUsers(users) {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-user-off"></i></div><div class="empty-title">No users found</div></div>`;
    if (!users.length) { document.getElementById('recentUsersList').innerHTML = empty; document.getElementById('allUsersList').innerHTML = empty; return; }
    document.getElementById('recentUsersList').innerHTML = users.slice(0, 5).map(userItemHTML).join('');
    document.getElementById('allUsersList').innerHTML = users.map(userItemHTML).join('');
}

function jobItemHTML(job, i) {
    const c = colors[i % colors.length];
    const title = job.title || job.jobTitle || 'Job';
    return `<div class="job-item">
    <div class="job-logo" style="background:${c.bg};color:${c.color}">${title[0].toUpperCase()}</div>
    <div class="job-info">
      <div class="job-name">${title}</div>
      <div class="job-meta"><span><i class="ti ti-map-pin"></i> ${job.location || 'Remote'}</span><span><i class="ti ti-clock"></i> ${job.jobType || 'Full-time'}</span></div>
    </div>
    <span class="job-status ${job.isActive ? 'status-active' : 'status-expired'}">${job.isActive ? 'Active' : 'Expired'}</span>
    <div class="job-actions">
      <button class="act-btn danger" onclick="deleteJob(${job.id})" title="Delete"><i class="ti ti-trash"></i></button>
    </div>
  </div>`;
}

function renderJobs(jobs) {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-briefcase-off"></i></div><div class="empty-title">No jobs found</div></div>`;
    if (!jobs.length) { document.getElementById('recentJobsList').innerHTML = empty; document.getElementById('allJobsList').innerHTML = empty; return; }
    document.getElementById('recentJobsList').innerHTML = jobs.slice(0, 5).map(jobItemHTML).join('');
    document.getElementById('allJobsList').innerHTML = jobs.map(jobItemHTML).join('');
}

function renderApps(apps) {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-file-off"></i></div><div class="empty-title">No applications found</div></div>`;
    const el = document.getElementById('allAppsList');
    if (!apps.length) { el.innerHTML = empty; return; }
    const badgeMap = { Pending: 'badge-review', Accepted: 'badge-hired', Rejected: 'badge-rejected', Reviewing: 'badge-new' };
    el.innerHTML = apps.map((app, i) => {
        const c = colors[i % colors.length];
        const name = app.applicantName || app.userName || 'Applicant';
        return `<div class="applicant-item">
      <div class="app-avatar" style="background:${c.bg};color:${c.color}">${name[0].toUpperCase()}</div>
      <div><div class="app-name">${name}</div><div class="app-role">${app.jobTitle || 'Position'}</div></div>
      <span class="app-badge ${badgeMap[app.status] || 'badge-new'}">${app.status || 'Pending'}</span>
    </div>`;
    }).join('');
}

async function deleteUser(id) {
    if (!confirm('Delete this user?')) return;
    try { await apiFetch('/Admin/users/' + id, { method: 'DELETE' }); showToast('User deleted'); await loadUsers(); }
    catch (err) { showToast(err.message || 'Failed', false); }
}

async function deleteJob(id) {
    if (!confirm('Delete this job?')) return;
    try { await apiFetch('/Job/' + id, { method: 'DELETE' }); showToast('Job deleted'); await loadJobs(); }
    catch (err) { showToast(err.message || 'Failed', false); }
}

window.addEventListener('DOMContentLoaded', async () => {
    const user = getUser();
    if (!user || user.role !== 'Admin') { window.location.href = 'login.html'; return; }
    await Promise.all([loadUsers(), loadJobs(), loadApps()]);
});