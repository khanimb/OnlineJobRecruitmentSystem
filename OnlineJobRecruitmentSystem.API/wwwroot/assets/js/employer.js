let myJobs = [], myApplicants = [];
let editingJobId = null;

function showTab(tab, el) {
    ['overview', 'jobs', 'applicants', 'contracts', 'reviews', 'premium', 'profile'].forEach(t => {
        $('#tab-' + t).hide();
    });
    $('#tab-' + tab).show();
    $('.nav-item').removeClass('active');
    if (el) $(el).addClass('active');
    const titles = {
        overview: ['Overview', "Welcome back! Here's what's happening."],
        jobs: ['My Jobs', 'Manage your job listings'],
        applicants: ['Applicants', 'Review and manage candidates'],
        contracts: ['Contracts', 'Manage contracts with hired candidates'],
        reviews: ['Reviews', 'Feedback from candidates and your own reviews'],
        premium: ['Premium', 'Unlock premium features'],
        profile: ['Company Profile', 'Update your company information']
    };
    $('#pageTitle').text(titles[tab][0]);
    $('#pageSubtitle').text(titles[tab][1]);
    if (tab === 'applicants') loadApplicants();
    if (tab === 'jobs') loadJobs();
    if (tab === 'profile') loadProfile();
    if (tab === 'contracts') loadContracts();
    if (tab === 'reviews') loadReviews();
    if (tab === 'premium') loadPremium();
}

function openModal() { $('#modalOverlay').addClass('open'); }
function closeModal() {
    $('#modalOverlay').removeClass('open');
    editingJobId = null;
    $('.modal-title').text('Post a New Job');
}
function closeModalOutside(e) { if (e.target.id === 'modalOverlay') closeModal(); }

// ── JOBS ──
async function loadJobs() {
    try { const r = await apiFetch('/Job/my-jobs'); myJobs = r.data || []; } catch (e) { myJobs = []; showToast('Failed to load jobs.', false); }
    renderJobs();
    $('#totalJobs').text(myJobs.length);
    $('#jobsBadge').text(myJobs.length);
    $('#jobsSubtitle').text(myJobs.length + ' positions posted');
    $('#jobFilterSelect').html('<option value="">All jobs</option>' + myJobs.map(j => `<option value="${j.id}">${escapeHtml(j.title)}</option>`).join(''));
}

function jobItemHTML(job, i) {
    const c = colors[i % colors.length];
    const letter = safeInitial(job.title || 'J');
    return `<div class="job-item">
        <div class="job-logo" style="background:${c.bg};color:${c.color}">${letter}</div>
        <div class="job-info">
            <div class="job-name">${escapeHtml(job.title || 'Job')}</div>
            <div class="job-meta"><span><i class="ti ti-map-pin"></i> ${escapeHtml(job.location || 'Remote')}</span><span><i class="ti ti-clock"></i> ${escapeHtml(job.jobType || 'Full-time')}</span></div>
        </div>
        <span class="job-status status-active">Active</span>
        <div class="job-actions">
            <button class="act-btn" title="View" onclick="window.location.href='jobdetail.html?id=${job.id}'"><i class="ti ti-eye"></i></button>
            <button class="act-btn" title="Edit" onclick="editJob(${job.id})"><i class="ti ti-edit"></i></button>
            <button class="act-btn danger" title="Delete" onclick="deleteJob(${job.id})"><i class="ti ti-trash"></i></button>
        </div>
    </div>`;
}

function renderJobs() {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-briefcase-off"></i></div><div class="empty-title">No jobs posted yet</div><div class="empty-sub">Click "Post a Job" to get started</div></div>`;
    renderList('#recentJobsList', myJobs.slice(0, 5), jobItemHTML, empty);
    renderList('#allJobsList', myJobs, jobItemHTML, empty);
}

// ── APPLICANTS ──
async function loadApplicants(jobId, status) {
    const endpoint = jobId ? `/Employer/applications/${jobId}` : `/Employer/applications${status ? '?status=' + status : ''}`;
    try { const r = await apiFetch(endpoint); myApplicants = r.data || []; } catch (e) { myApplicants = []; showToast('Failed to load applicants.', false); }
    renderApplicants();
    $('#totalApps').text(myApplicants.length);
    $('#appsBadge').text(myApplicants.length);
    $('#miniNew').text(myApplicants.filter(a => a.status === 'Applied').length);
    $('#miniReview').text(myApplicants.filter(a => a.status === 'Reviewed').length);
    $('#miniShortlisted').text(myApplicants.filter(a => a.status === 'Shortlisted').length);
}

function renderApplicants() {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-user-off"></i></div><div class="empty-title">No applicants yet</div><div class="empty-sub">Applicants will appear here</div></div>`;
    const badgeMap = { Applied: 'badge-new', Reviewed: 'badge-review', Shortlisted: 'badge-hired', Rejected: 'badge-rejected' };
    const nextStatusMap = { Applied: ['Reviewed'], Reviewed: ['Shortlisted', 'Rejected'], Shortlisted: [], Rejected: [] };
    const statusNames = ['Applied', 'Reviewed', 'Shortlisted', 'Rejected'];
    const templateFn = (app, i) => {
        const c = colors[i % colors.length];
        const rawName = app.jobSeeker?.fullName || 'Applicant';
        const statusName = typeof app.status === 'number' ? statusNames[app.status] : app.status;
        const cvUrl = app.jobSeeker?.cvUrl;
        const userId = app.jobSeeker?.userId || '';
        const nextOptions = (nextStatusMap[statusName] || []).map(s => `<option value="${s}">${s}</option>`).join('');
        return `<div class="applicant-item">
    <div class="app-avatar" style="background:${c.bg};color:${c.color}">${safeInitial(rawName)}</div>
    <div style="flex:1">
        <div class="app-name">${escapeHtml(rawName)}</div>
        <div class="app-role">${escapeHtml(app.jobTitle || 'Position')} · ${new Date(app.appliedAt).toLocaleDateString()}</div>
        ${app.jobSeeker?.skills ? `<div style="font-size:0.75rem;color:#64748b;margin-top:2px">${escapeHtml(app.jobSeeker.skills)}</div>` : ''}
    </div>
    <a href="/assets/pages/viewprofile.html?id=${userId}" class="act-btn" title="View Profile"><i class="ti ti-user"></i></a>
    ${cvUrl ? `<a href="${FILE_BASE_URL}${cvUrl}" target="_blank" class="act-btn" title="View CV"><i class="ti ti-file-text"></i></a>` : ''}
    <span class="app-badge ${badgeMap[statusName] || 'badge-new'}">${statusName}</span>
    ${statusName === 'Shortlisted' ? `<button class="btn-outline" onclick="openContractModal(${app.jobPostId}, ${app.jobSeeker?.id}, '${rawName.replace(/'/g, "\\'")}')"><i class="ti ti-file-text"></i> Contract</button>` : ''}
    ${nextOptions ? `
    <select class="status-select" onchange="updateStatus(${app.id}, this.value)">
        <option value="">Change status</option>
        ${nextOptions}
    </select>` : ''}
</div>`;
    };
    renderList('#recentAppsList', myApplicants, templateFn, empty);
    renderList('#allApplicantsList', myApplicants, templateFn, empty);
}

async function updateStatus(appId, status) {
    if (!status) return;
    try {
        await apiFetch(`/Employer/applications/${appId}/status`, { method: 'PUT', body: JSON.stringify({ status, notes: '' }) });
        showToast(`Status updated to ${status}`);
        await loadApplicants();
    } catch (e) { showToast(e.message, false); }
}

// ── POST JOB ──
async function postJob() {
    const dto = {
        title: $('#jobTitle').val().trim(),
        location: $('#jobLocation').val().trim(),
        description: $('#jobDesc').val().trim(),
        requirements: $('#jobReqs').val().trim(),
        jobType: $('#jobType').val(),
        category: $('#jobCategory').val(),
        salaryMin: parseInt($('#salaryMin').val()) || 0,
        salaryMax: parseInt($('#salaryMax').val()) || 0,
        budget: $('#jobBudget').val() ? parseFloat($('#jobBudget').val()) : null,
        paymentType: $('#jobPaymentType').val(),
        deadline: $('#jobDeadline').val() || null
    };
    if (!dto.title || !dto.location || !dto.description) { showToast('Please fill required fields', false); return; }

    try {
        if (editingJobId) {
            const existing = myJobs.find(j => j.id === editingJobId);
            dto.isActive = existing ? existing.isActive : true;
            await apiFetch('/Job/' + editingJobId, { method: 'PUT', body: JSON.stringify(dto) });
            showToast('Job updated successfully!');
        } else {
            await apiFetch('/Job', { method: 'POST', body: JSON.stringify(dto) });
            showToast('Job posted successfully!');
        }
        closeModal();
        await loadJobs();
    } catch (err) { showToast(err.message || 'Failed to save job', false); }
}

function editJob(id) {
    const job = myJobs.find(j => j.id === id);
    if (!job) return;
    editingJobId = id;
    $('#jobTitle').val(job.title);
    $('#jobLocation').val(job.location);
    $('#jobType').val(job.jobType);
    $('#jobCategory').val(job.category);
    $('#salaryMin').val(job.salaryMin);
    $('#salaryMax').val(job.salaryMax);
    $('#jobDesc').val(job.description);
    $('#jobReqs').val(job.requirements);
    $('#jobBudget').val(job.budget || '');
    $('#jobPaymentType').val(job.paymentType || 'Fixed');
    $('#jobDeadline').val(job.deadline ? job.deadline.split('T')[0] : '');
    $('.modal-title').text('Edit Job');
    openModal();
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
        if ($('#companyName').length) $('#companyName').val(p.companyName || '');
        if ($('#companyDesc').length) $('#companyDesc').val(p.description || '');
        if ($('#companyWebsite').length) $('#companyWebsite').val(p.website || '');
        if (p.logoUrl) $('#logoPreview').attr('src', FILE_BASE_URL + p.logoUrl).show();
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

async function loadDashboard() {
    try {
        const r = await apiFetch('/Employer/dashboard');
        $('#totalShortlisted').text(r.data.shortlisted);
        $('#totalRejected').text(r.data.rejected);
    } catch { }
}

async function uploadLogo() {
    const file = $('#logoFile')[0].files[0];
    if (!file) { showToast('Please select a file', false); return; }
    const formData = new FormData();
    formData.append('file', file);
    try {
        const result = await apiFetch('/Employer/upload-logo', { method: 'POST', body: formData });
        showToast('Logo uploaded successfully!');
        $('#logoPreview').attr('src', FILE_BASE_URL + result.data).show();
    } catch (e) {
        showToast(e.message || 'Upload failed', false);
    }
}

async function saveProfile() {
    const dto = {
        companyName: $('#companyName').val(),
        description: $('#companyDesc').val(),
        website: $('#companyWebsite').val()
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
async function renderChart() {
    const el = $('#chartArea');
    if (!el.length) return;
    try {
        const r = await apiFetch('/Analytics');
        const trend = r.data.applicationTrend || [];
        const trendMap = {};
        trend.forEach(t => { trendMap[t.date] = t.count; });

        const last7 = [];
        for (let i = 6; i >= 0; i--) {
            const d = new Date();
            d.setDate(d.getDate() - i);
            const key = d.toISOString().split('T')[0];
            last7.push({ date: d, count: trendMap[key] || 0 });
        }
        const max = Math.max(...last7.map(t => t.count), 1);
        el.html(last7.map((t, i) =>
            `<div class="bar-wrap"><div class="bar ${i === last7.length - 1 ? 'active' : ''}" style="height:${(t.count / max) * 130}px"></div><span class="bar-label">${t.date.toLocaleDateString('en', { weekday: 'short', timeZone: 'UTC' })}</span></div>`
        ).join(''));
    } catch { el.html(''); }
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
    if (!user || user.role !== 'Employer') { window.location.href = 'login.html'; return; }
    const name = user.firstName || user.username || user.email || 'Employer';
    $('#sidebarName').text(name);
    $('#sidebarAvatar').text(name[0].toUpperCase());
    renderChart();
    Promise.all([loadJobs(), loadApplicants(), loadDashboard()]).then(() => {
        if (location.hash === '#premium') {
            showTab('premium', $('.nav-item[onclick*="premium"]').get(0));
        }
    });
});