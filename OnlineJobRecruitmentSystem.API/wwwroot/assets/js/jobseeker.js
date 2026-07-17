let myApplications = [], mySaved = [];


function showTab(tab, el) {
    ['overview', 'applications', 'saved', 'browse', 'profile', 'portfolio', 'jobalert', 'contracts', 'reviews', 'premium'].forEach(t => {
        $('#tab-' + t).hide();
    });
    $('#tab-' + tab).show();
    $('.nav-item').removeClass('active');
    if (el) $(el).addClass('active');
    const titles = {
        overview: ['Overview', "Here's your job search summary."],
        applications: ['My Applications', 'Track your application status'],
        saved: ['Saved Jobs', 'Jobs you bookmarked'],
        browse: ['Browse Jobs', 'Find your next opportunity'],
        profile: ['My Profile', 'Update your personal information'],
        portfolio: ['Portfolio', 'Showcase your work to employers'],
        jobalert: ['Job Alerts', 'Get notified about matching jobs'],
        contracts: ['My Contracts', 'Contracts with employers'],
        reviews: ['Reviews', 'Feedback from employers and your own reviews'],
        premium: ['Premium', 'Unlock premium features']
    };
    $('#pageTitle').text(titles[tab][0]);
    $('#pageSubtitle').text(titles[tab][1]);
    if (tab === 'saved') loadSaved();
    if (tab === 'profile') loadProfile();
    if (tab === 'browse') loadBrowseJobs();
    if (tab === 'portfolio') loadPortfolio();
    if (tab === 'jobalert') loadJobAlerts();
    if (tab === 'contracts') loadContracts();
    if (tab === 'reviews') loadReviews();
    if (tab === 'premium') loadPremium();
}

// ── APPLICATIONS ──
async function loadApplications() {
    try { const r = await apiFetch('/JobApplication/mine'); myApplications = r.data || []; } catch { myApplications = []; }
    renderApplications();
    $('#totalApplications').text(myApplications.length);
    $('#appsBadge').text(myApplications.length);
    $('#appsSubtitle').text(myApplications.length + ' applications');
    $('#pendingCount').text(myApplications.filter(a => a.status === 'Applied').length);
    $('#acceptedCount').text(myApplications.filter(a => a.status === 'Shortlisted').length);
}

function appItemHTML(app, i) {
    const c = colors[i % colors.length];
    const rawTitle = app.jobTitle || 'Position';
    const rawCompany = app.companyName || 'Company';
    const badgeMap = { Applied: 'badge-new', Reviewed: 'badge-review', Shortlisted: 'badge-hired', Rejected: 'badge-rejected' };
    return `<div class="job-item" style="cursor:pointer" onclick="window.location.href='/assets/pages/jobdetail.html?id=${app.jobPostId}'">
    <div class="job-logo" style="background:${c.bg};color:${c.color}">${safeInitial(rawCompany)}</div>
        <div class="job-info">
        <div class="job-name">${escapeHtml(rawTitle)}</div>
        <div class="job-meta"><span><i class="ti ti-building"></i> ${escapeHtml(rawCompany)}</span><span><i class="ti ti-calendar"></i> ${app.appliedAt ? new Date(app.appliedAt).toLocaleDateString() : 'Recently'}</span></div>
    </div>
    <span class="app-badge ${badgeMap[app.status] || 'badge-new'}">${app.status || 'Applied'}</span>
    </div>`;
}

function renderApplications() {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-file-off"></i></div><div class="empty-title">No applications yet</div><div class="empty-sub">Start applying to jobs!</div></div>`;
    renderList('#recentApplicationsList', myApplications.slice(0, 3), appItemHTML, empty);
    renderList('#allApplicationsList', myApplications, appItemHTML, empty);
}

// ── SAVED ──
async function loadSaved() {
    try { const r = await apiFetch('/JobSeeker/saved'); mySaved = r.data || []; } catch { mySaved = []; }
    renderSaved();
}

function renderSaved() {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-bookmark-off"></i></div><div class="empty-title">No saved jobs</div><div class="empty-sub">Bookmark jobs to see them here</div></div>`;
    $('#savedCount').text(mySaved.length);
    $('#savedBadge').text(mySaved.length);

    renderList('#allSavedList', mySaved, (s, i) => {
        const c = colors[i % colors.length];
        return `<div class="job-item">
            <div class="job-logo" style="background:${c.bg};color:${c.color}">${safeInitial(s.job.companyName)}</div>
            <div class="job-info">
                <div class="job-name">${escapeHtml(s.job.title)}</div>
                <div class="job-meta"><span><i class="ti ti-building"></i> ${escapeHtml(s.job.companyName)}</span><span><i class="ti ti-map-pin"></i> ${escapeHtml(s.job.location)}</span></div>
            </div>
            <div style="display:flex;gap:8px;align-items:center">
                <button class="btn-outline" onclick="window.location.href='/assets/pages/jobdetail.html?id=${s.job.id}'">View</button>
                <button class="btn-danger" onclick="unsaveJob(${s.job.id})">Remove</button>
            </div>
        </div>`;
    }, empty);

    renderList('#recentSavedList', mySaved.slice(0, 3), (s, i) => {
        const c = colors[i % colors.length];
        return `<div class="job-item" style="cursor:pointer" onclick="window.location.href='/assets/pages/jobdetail.html?id=${s.job.id}'">
        <div class="job-logo" style="background:${c.bg};color:${c.color}">${safeInitial(s.job.companyName)}</div>
        <div class="job-info">
        <div class="job-name">${escapeHtml(s.job.title)}</div>
        <div class="job-meta"><span><i class="ti ti-building"></i> ${escapeHtml(s.job.companyName)}</span></div>
        </div>
         <span class="tag tag-gray">${s.job.jobType}</span>
        </div>`;
    }, empty);
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
    $('#browseJobsList').html('<div style="padding:20px;color:#888">Loading...</div>');
    try {
        const r = await apiFetch('/Job');
        const jobs = r.data.data || [];
        renderList('#browseJobsList', jobs, (j, i) => {
            const c = colors[i % colors.length];
            return `<div class="job-item">
                <div class="job-logo" style="background:${c.bg};color:${c.color}">${safeInitial(j.companyName || j.title)}</div>
                <div class="job-info">
                    <div class="job-name">${escapeHtml(j.title)}</div>
                    <div class="job-meta"><span><i class="ti ti-map-pin"></i> ${escapeHtml(j.location)}</span><span><i class="ti ti-briefcase"></i> ${escapeHtml(j.jobType)}</span></div>
                </div>
                <div style="display:flex;gap:8px;align-items:center">
                    <button class="btn-outline" onclick="saveJob(${j.id})"><i class="ti ti-bookmark"></i></button>
                    <button class="btn-primary" onclick="window.location.href='/assets/pages/jobdetail.html?id=${j.id}'">Apply</button>
                </div>
            </div>`;
        }, '<div class="empty-state"><p>No jobs available</p></div>');
    } catch (e) { $('#browseJobsList').html('<div class="empty-state"><p>Error loading jobs</p></div>'); }
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
        $('#profileFullName').val(p.fullName || '');
        $('#profilePhone').val(p.phone || '');
        $('#profileSkills').val(p.skills || '');
        $('#profileExperience').val(p.workExperience || '');
        if (p.cvUrl) $('#cvLink').attr('href', FILE_BASE_URL + p.cvUrl).css('display', 'inline-flex');
    } catch { }
    loadAccountInfo();
}

async function loadAccountInfo() {
    try {
        const r = await apiFetch('/Account/profile');
        const line = `${escapeHtml(r.data.email)} · ${escapeHtml(r.data.role)}`;
        if ($('#accountInfoLine').length) { $('#accountInfoLine').html(line); return; }
        $('#tab-profile .dash-card-header').first().after(`<div id="accountInfoLine" style="padding:12px 28px 0;font-size:0.8rem;color:#64748b">${line}</div>`);
    } catch { }
}

async function saveProfile() {
    const dto = {
        fullName: $('#profileFullName').val(),
        phone: $('#profilePhone').val(),
        skills: $('#profileSkills').val(),
        workExperience: $('#profileExperience').val()
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
        $('#sidebarName').text(dto.fullName);
    } catch (e) { showToast(e.message, false); }
}

async function uploadCv() {
    const file = $('#cvFile')[0].files[0];
    if (!file) { showToast('Please select a file', false); return; }
    const formData = new FormData();
    formData.append('file', file);
    try {
        const result = await apiFetch('/JobSeeker/upload-cv', { method: 'POST', body: formData });
        showToast('CV uploaded successfully!');
        $('#cvLink').attr('href', FILE_BASE_URL + result.data).css('display', 'inline-flex');
    } catch (e) {
        showToast(e.message || 'Upload failed', false);
    }
}

async function downloadCvPdf() {
    try {
        const token = localStorage.getItem('token');
        const res = await fetch(`${BASE_URL}/Account/cv/download`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) throw new Error('Could not generate CV.');
        const blob = await res.blob();
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'CV.pdf';
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);
    } catch (err) {
        showToast(err.message || 'Failed to download CV', false);
    }
}

// ── INIT ──
$(function () {
    $('#sidebarToggleBtn').on('click', function () { $('.sidebar').toggleClass('open'); });
    $('.dash-main').on('click', function (e) {
        if ($('.sidebar').hasClass('open') && !$(e.target).closest('.sidebar').length && !$(e.target).closest('#sidebarToggleBtn').length) {
            $('.sidebar').removeClass('open');
        }
    });
    const user = getUser();
    if (!user || user.role !== 'JobSeeker') { window.location.href = 'login.html'; return; }
    const name = user.firstName || user.username || user.email || 'Job Seeker';
    $('#sidebarName').text(name);
    $('#sidebarAvatar').text(name[0].toUpperCase());
    Promise.all([loadApplications(), loadSaved()]).then(() => {
        if (location.hash === '#premium') {
            showTab('premium', $('.nav-item[onclick*="premium"]').get(0));
        }
    });
});