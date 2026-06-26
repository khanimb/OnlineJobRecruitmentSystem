const colors = [
    { bg: '#eff6ff', color: '#2563EB' }, { bg: '#ecfdf5', color: '#10b981' },
    { bg: '#fffbeb', color: '#f59e0b' }, { bg: '#f0fdf4', color: '#16a34a' }, { bg: '#fdf4ff', color: '#a855f7' }
];
let myApplications = [], mySaved = [];

function getUser() { const u = localStorage.getItem('user'); return u ? JSON.parse(u) : null; }
function logout() { localStorage.removeItem('token'); localStorage.removeItem('user'); window.location.href = 'login.html'; }

function showTab(tab, el) {
    ['overview', 'applications', 'saved', 'browse', 'profile'].forEach(t => {
        const e = document.getElementById('tab-' + t);
        if (e) e.style.display = 'none';
    });
    document.getElementById('tab-' + tab).style.display = 'block';
    document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
    if (el) el.classList.add('active');
    const titles = {
        overview: ['Overview', "Here's your job search summary."],
        applications: ['My Applications', 'Track your application status'],
        saved: ['Saved Jobs', 'Jobs you bookmarked'],
        browse: ['Browse Jobs', 'Find your next opportunity'],
        profile: ['My Profile', 'Update your personal information']
    };
    document.getElementById('pageTitle').textContent = titles[tab][0];
    document.getElementById('pageSubtitle').textContent = titles[tab][1];
    if (tab === 'saved') loadSaved();
    if (tab === 'profile') loadProfile();
    if (tab === 'browse') loadBrowseJobs();
}

function showToast(msg, ok = true) {
    const t = document.getElementById('toast');
    t.querySelector('i').style.color = ok ? '#10b981' : '#ef4444';
    document.getElementById('toastMsg').textContent = msg;
    t.classList.add('show');
    setTimeout(() => t.classList.remove('show'), 3000);
}

// ── APPLICATIONS ──
async function loadApplications() {
    try {
        const r = await apiFetch('/Application/mine');
        myApplications = r.data || [];
    } catch { myApplications = []; }
    renderApplications();
    document.getElementById('totalApplications').textContent = myApplications.length;
    document.getElementById('appsBadge').textContent = myApplications.length;
    document.getElementById('appsSubtitle').textContent = myApplications.length + ' applications';
    document.getElementById('pendingCount').textContent = myApplications.filter(a => a.status === 'Applied').length;
    document.getElementById('acceptedCount').textContent = myApplications.filter(a => a.status === 'Shortlisted').length;
}

function appItemHTML(app, i) {
    const c = colors[i % colors.length];
    const title = app.jobTitle || 'Position';
    const company = app.companyName || 'Company';
    const badgeMap = { Applied: 'badge-new', Reviewed: 'badge-review', Shortlisted: 'badge-hired', Rejected: 'badge-rejected' };
    return `<div class="job-item">
        <div class="job-logo" style="background:${c.bg};color:${c.color}">${company[0].toUpperCase()}</div>
        <div class="job-info">
            <div class="job-name">${title}</div>
            <div class="job-meta"><span><i class="ti ti-building"></i> ${company}</span><span><i class="ti ti-calendar"></i> ${app.appliedAt ? new Date(app.appliedAt).toLocaleDateString() : 'Recently'}</span></div>
        </div>
        <span class="app-badge ${badgeMap[app.status] || 'badge-new'}">${app.status || 'Applied'}</span>
    </div>`;
}

function renderApplications() {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-file-off"></i></div><div class="empty-title">No applications yet</div><div class="empty-sub">Start applying to jobs!</div></div>`;
    if (!myApplications.length) {
        document.getElementById('recentApplicationsList').innerHTML = empty;
        document.getElementById('allApplicationsList').innerHTML = empty;
        return;
    }
    document.getElementById('recentApplicationsList').innerHTML = myApplications.slice(0, 3).map(appItemHTML).join('');
    document.getElementById('allApplicationsList').innerHTML = myApplications.map(appItemHTML).join('');
}

// ── SAVED ──
async function loadSaved() {
    try {
        const r = await apiFetch('/JobSeeker/saved');
        mySaved = r.data || [];
    } catch { mySaved = []; }
    renderSaved();
}

function renderSaved() {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-bookmark-off"></i></div><div class="empty-title">No saved jobs</div><div class="empty-sub">Bookmark jobs to see them here</div></div>`;
    document.getElementById('savedCount').textContent = mySaved.length;
    document.getElementById('savedBadge').textContent = mySaved.length;
    if (!mySaved.length) {
        document.getElementById('recentSavedList').innerHTML = empty;
        document.getElementById('allSavedList').innerHTML = empty;
        return;
    }
    const html = mySaved.map((s, i) => {
        const c = colors[i % colors.length];
        return `<div class="job-item">
            <div class="job-logo" style="background:${c.bg};color:${c.color}">${s.job.companyName[0].toUpperCase()}</div>
            <div class="job-info">
                <div class="job-name">${s.job.title}</div>
                <div class="job-meta"><span><i class="ti ti-building"></i> ${s.job.companyName}</span><span><i class="ti ti-map-pin"></i> ${s.job.location}</span></div>
            </div>
            <div style="display:flex;gap:8px;align-items:center">
                <button class="btn-outline" onclick="window.location.href='/assets/pages/jobdetail.html?id=${s.job.id}'">View</button>
                <button class="btn-danger" onclick="unsaveJob(${s.job.id})">Remove</button>
            </div>
        </div>`;
    }).join('');
    document.getElementById('recentSavedList').innerHTML = mySaved.slice(0, 3).map((s, i) => {
        const c = colors[i % colors.length];
        return `<div class="job-item">
            <div class="job-logo" style="background:${c.bg};color:${c.color}">${s.job.companyName[0].toUpperCase()}</div>
            <div class="job-info">
                <div class="job-name">${s.job.title}</div>
                <div class="job-meta"><span><i class="ti ti-building"></i> ${s.job.companyName}</span></div>
            </div>
            <span class="tag tag-gray">${s.job.jobType}</span>
        </div>`;
    }).join('');
    document.getElementById('allSavedList').innerHTML = html;
}

async function unsaveJob(jobId) {
    try {
        await apiFetch(`/JobSeeker/saved/${jobId}`, { method: 'DELETE' });
        showToast('Removed from saved jobs');
        loadSaved();
    } catch (e) { showToast(e.message, false); }
}

// ── BROWSE ──
async function loadBrowseJobs() {
    const el = document.getElementById('browseJobsList');
    el.innerHTML = '<div style="padding:20px;color:#888">Loading...</div>';
    try {
        const r = await apiFetch('/Job');
        const jobs = r.data || [];
        if (!jobs.length) { el.innerHTML = '<div class="empty-state"><p>No jobs available</p></div>'; return; }
        el.innerHTML = jobs.map((j, i) => {
            const c = colors[i % colors.length];
            return `<div class="job-item">
                <div class="job-logo" style="background:${c.bg};color:${c.color}">${(j.companyName || j.title)[0].toUpperCase()}</div>
                <div class="job-info">
                    <div class="job-name">${j.title}</div>
                    <div class="job-meta"><span><i class="ti ti-map-pin"></i> ${j.location}</span><span><i class="ti ti-briefcase"></i> ${j.jobType}</span></div>
                </div>
                <div style="display:flex;gap:8px;align-items:center">
                    <button class="btn-outline" onclick="saveJob(${j.id})"><i class="ti ti-bookmark"></i></button>
                    <button class="btn-primary" onclick="window.location.href='/assets/pages/jobdetail.html?id=${j.id}'">Apply</button>
                </div>
            </div>`;
        }).join('');
    } catch (e) { el.innerHTML = '<div class="empty-state"><p>Error loading jobs</p></div>'; }
}

async function saveJob(jobId) {
    try {
        await apiFetch(`/JobSeeker/saved/${jobId}`, { method: 'POST' });
        showToast('Job saved!');
    } catch (e) { showToast(e.message, false); }
}

// ── PROFILE ──
async function loadProfile() {
    try {
        const r = await apiFetch('/JobSeeker/profile');
        const p = r.data;
        document.getElementById('profileFullName').value = p.fullName || '';
        document.getElementById('profilePhone').value = p.phone || '';
        document.getElementById('profileSkills').value = p.skills || '';
        document.getElementById('profileExperience').value = p.workExperience || '';
        if (p.cvUrl) {
            const cvLink = document.getElementById('cvLink');
            cvLink.href = 'http://localhost:5076' + p.cvUrl;
            cvLink.style.display = 'inline-flex';
        }
    } catch { }
}

async function saveProfile() {
    const dto = {
        fullName: document.getElementById('profileFullName').value,
        phone: document.getElementById('profilePhone').value,
        skills: document.getElementById('profileSkills').value,
        workExperience: document.getElementById('profileExperience').value
    };
    try {
        let exists = false;
        try { await apiFetch('/JobSeeker/profile'); exists = true; } catch { }
        if (exists) {
            await apiFetch('/JobSeeker/profile', { method: 'PUT', body: JSON.stringify(dto) });
        } else {
            await apiFetch('/JobSeeker/profile', { method: 'POST', body: JSON.stringify(dto) });
        }
        showToast('Profile saved!');
        document.getElementById('sidebarName').textContent = dto.fullName;
    } catch (e) { showToast(e.message, false); }
}

async function uploadCv() {
    const file = document.getElementById('cvFile').files[0];
    if (!file) { showToast('Please select a file', false); return; }
    const formData = new FormData();
    formData.append('file', file);
    const token = localStorage.getItem('token');
    try {
        const res = await fetch('http://localhost:5076/api/JobSeeker/upload-cv', {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` },
            body: formData
        });
        const result = await res.json();
        if (result.success) {
            showToast('CV uploaded successfully!');
            const cvLink = document.getElementById('cvLink');
            cvLink.href = 'http://localhost:5076' + result.data;
            cvLink.style.display = 'inline-flex';
        } else { showToast(result.message, false); }
    } catch { showToast('Upload failed', false); }
}

// ── INIT ──
window.addEventListener('DOMContentLoaded', async () => {
    const user = getUser();
    if (!user || user.role !== 'JobSeeker') { window.location.href = 'login.html'; return; }
    const name = user.firstName || user.username || user.email || 'Job Seeker';
    document.getElementById('sidebarName').textContent = name;
    document.getElementById('sidebarAvatar').textContent = name[0].toUpperCase();
    await loadApplications();
    await loadSaved();
});