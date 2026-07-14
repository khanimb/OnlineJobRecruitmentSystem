let allUsers = [], allJobs = [], allApps = [], allPayments = [], allReviews = [], allContracts = [];

function showTab(tab, el) {
    ['overview', 'users', 'jobs', 'applications', 'payments', 'reviews', 'contracts'].forEach(t => $('#tab-' + t).hide());
    $('#tab-' + tab).show();
    $('.nav-item').removeClass('active');
    if (el) $(el).addClass('active');
    const titles = {
        overview: ['Overview', 'Platform statistics and management.'],
        users: ['Users', 'Manage all registered users'],
        jobs: ['Jobs', 'Manage all job listings'],
        applications: ['Applications', 'View all applications'],
        payments: ['Payments', 'Transaction history'],
        reviews: ['Reviews', 'Platform-wide reviews'],
        contracts: ['Contracts', 'Active and completed contracts']
    };
    $('#pageTitle').text(titles[tab][0]);
    $('#pageSubtitle').text(titles[tab][1]);
}

async function loadUsers() {
    try { const r = await apiFetch('/Admin/users'); allUsers = r.data || []; } catch { allUsers = []; }
    renderUsers(allUsers);
    $('#totalUsers').text(allUsers.length);
    $('#usersBadge').text(allUsers.length);
    $('#usersSubtitle').text(allUsers.length + ' users');
}

async function loadJobs() {
    try { const r = await apiFetch('/Job'); allJobs = r.data?.data || []; } catch { allJobs = []; }
    renderJobs(allJobs);
    $('#totalJobs').text(allJobs.length);
    $('#jobsBadge').text(allJobs.length);
    $('#jobsSubtitle').text(allJobs.length + ' jobs');
}

async function loadApps() {
    try { const r = await apiFetch('/Admin/applications'); allApps = r.data || []; } catch { allApps = []; }
    renderApps(allApps);
    $('#totalApps').text(allApps.length);
    $('#appsBadge').text(allApps.length);
    $('#appsSubtitle').text(allApps.length + ' applications');
}

async function loadPayments() {
    try { const r = await apiFetch('/Admin/payments'); allPayments = r.data || []; } catch { allPayments = []; }
    renderPayments(allPayments);
}

function paymentItemHTML(p, i) {
    const c = colors[i % colors.length];
    return `<div class="job-item">
    <div class="job-logo" style="background:${c.bg};color:${c.color}">${safeInitial(p.employerEmail)}</div>
    <div class="job-info">
      <div class="job-name">${escapeHtml(p.employerEmail)}</div>
      <div class="job-meta"><span><i class="ti ti-package"></i> ${escapeHtml(p.plan)}</span><span><i class="ti ti-calendar"></i> ${new Date(p.createdAt).toLocaleDateString()}</span></div>
    </div>
    <span class="job-status ${p.status === 'completed' ? 'status-active' : 'status-expired'}">${escapeHtml(p.status)}</span>
    <div style="font-weight:700;color:#16a34a;min-width:70px;text-align:right">$${Number(p.amount).toFixed(2)}</div>
  </div>`;
}

function renderPayments(payments) {
    renderList('#allPaymentsList', payments, paymentItemHTML,
        `<div class="empty-state"><div class="empty-icon"><i class="ti ti-credit-card-off"></i></div><div class="empty-title">No payments yet</div><div class="empty-sub">Stripe payments will appear here</div></div>`);
}

function renderReviews(reviews) {
    renderList('#allReviewsList', reviews, reviewItemHTML,
        `<div class="empty-state"><div class="empty-icon"><i class="ti ti-star-off"></i></div><div class="empty-title">No reviews found</div></div>`);
}

async function loadStats() {
    try {
        const r = await apiFetch('/Admin/stats');
        $('#totalRevenue').text('$' + Number(r.data.totalRevenue || 0).toLocaleString());
    } catch { }
}

async function loadReviews() {
    try { const r = await apiFetch('/Admin/reviews'); allReviews = r.data || []; } catch { allReviews = []; }
    renderReviews(allReviews);
}

function reviewItemHTML(rv, i) {
    const c = colors[i % colors.length];
    return `<div class="job-item">
    <div class="job-logo" style="background:${c.bg};color:${c.color}">${safeInitial(rv.reviewerEmail)}</div>
    <div class="job-info">
      <div class="job-name">${escapeHtml(rv.reviewerEmail)} → ${escapeHtml(rv.revieweeEmail)}</div>
      <div class="job-meta"><span><i class="ti ti-star-filled"></i> ${rv.rating}/5</span><span>${escapeHtml(rv.comment || '')}</span></div>
    </div>
    <div class="job-actions">
      <button class="act-btn danger" onclick="deleteReview(${rv.id})" title="Delete"><i class="ti ti-trash"></i></button>
    </div>
  </div>`;
}

async function deleteReview(id) {
    if (!confirm('Delete this review?')) return;
    try { await apiFetch('/Admin/reviews/' + id, { method: 'DELETE' }); showToast('Review deleted'); await loadReviews(); }
    catch (err) { showToast(err.message || 'Failed', false); }
}

async function loadContracts() {
    try { const r = await apiFetch('/Admin/contracts'); allContracts = r.data || []; } catch { allContracts = []; }
    renderContracts(allContracts);
}

function contractItemHTML(ct, i) {
    const c = colors[i % colors.length];
    return `<div class="job-item">
    <div class="job-logo" style="background:${c.bg};color:${c.color}">${safeInitial(ct.jobTitle)}</div>
    <div class="job-info">
      <div class="job-name">${escapeHtml(ct.jobTitle)}</div>
      <div class="job-meta"><span><i class="ti ti-building"></i> ${escapeHtml(ct.employerName)}</span><span><i class="ti ti-user"></i> ${escapeHtml(ct.jobSeekerName)}</span></div>
    </div>
    <span class="job-status ${ct.status === 'Completed' ? 'status-active' : 'status-expired'}">${escapeHtml(ct.status)}</span>
    <div style="font-weight:700;color:#16a34a;min-width:70px;text-align:right">$${Number(ct.amount).toFixed(2)}</div>
  </div>`;
}

function renderContracts(contracts) {
    renderList('#allContractsList', contracts, contractItemHTML,
        `<div class="empty-state"><div class="empty-icon"><i class="ti ti-file-off"></i></div><div class="empty-title">No contracts found</div></div>`);
}

function filterUsers() {
    const q = $('#userSearch').val().toLowerCase();
    const filtered = allUsers.filter(u => (u.email || '').toLowerCase().includes(q) || (u.firstName || '').toLowerCase().includes(q) || (u.username || '').toLowerCase().includes(q));
    renderUsers(filtered);
}

function userItemHTML(user, i) {
    const c = colors[i % colors.length];
    const rawName = user.firstName ? `${user.firstName} ${user.lastName || ''}`.trim() : user.username || user.email;
    const name = escapeHtml(rawName);
    return `<div class="job-item">
    <div class="job-logo" style="background:${c.bg};color:${c.color}">${safeInitial(rawName)}</div>
    <div class="job-info">
      <div class="job-name">${name}</div>
      <div class="job-meta"><span><i class="ti ti-mail"></i> ${escapeHtml(user.email || '')}</span></div>
    </div>
    <select onchange="updateUserRole(${user.id}, this.value)" style="border:1px solid #e2e8f0;border-radius:6px;padding:4px 8px;font-size:0.75rem;font-family:'Inter',sans-serif;">
      <option value="JobSeeker" ${user.role === 'JobSeeker' ? 'selected' : ''}>JobSeeker</option>
      <option value="Employer" ${user.role === 'Employer' ? 'selected' : ''}>Employer</option>
      <option value="Admin" ${user.role === 'Admin' ? 'selected' : ''}>Admin</option>
    </select>
    <div class="job-actions">
      <button class="act-btn" title="View" onclick="toggleUserDetail(${user.id})"><i class="ti ti-info-circle"></i></button>
      <button class="act-btn danger" onclick="deleteUser(${user.id})" title="Delete"><i class="ti ti-trash"></i></button>
    </div>
  </div>
  <div id="user-detail-${user.id}" style="display:none;padding:12px 16px;background:#f8fafc;border-radius:10px;margin:-6px 0 10px"></div>`;
}

function renderUsers(users) {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-user-off"></i></div><div class="empty-title">No users found</div></div>`;
    renderList('#recentUsersList', users.slice(0, 5), userItemHTML, empty);
    renderList('#allUsersList', users, userItemHTML, empty);
}

function jobItemHTML(job, i) {
    const c = colors[i % colors.length];
    const rawTitle = job.title || job.jobTitle || 'Job';
    const title = escapeHtml(rawTitle);
    return `<div class="job-item">
    <div class="job-logo" style="background:${c.bg};color:${c.color}">${safeInitial(rawTitle)}</div>
    <div class="job-info">
      <div class="job-name">${title}</div>
      <div class="job-meta"><span><i class="ti ti-map-pin"></i> ${escapeHtml(job.location || 'Remote')}</span><span><i class="ti ti-clock"></i> ${escapeHtml(job.jobType || 'Full-time')}</span></div>
    </div>
        <span class="job-status ${job.isActive ? 'status-active' : 'status-expired'}">${job.isActive ? 'Active' : 'Expired'}</span>
    <div class="job-actions">
      <button class="act-btn" onclick="toggleJobStatus(${job.id}, ${job.isActive})" title="${job.isActive ? 'Deactivate' : 'Activate'}"><i class="ti ${job.isActive ? 'ti-eye-off' : 'ti-eye'}"></i></button>
      <button class="act-btn danger" onclick="deleteJob(${job.id})" title="Delete"><i class="ti ti-trash"></i></button>
    </div>
  </div>`;
}

function renderJobs(jobs) {
    const empty = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-briefcase-off"></i></div><div class="empty-title">No jobs found</div></div>`;
    renderList('#recentJobsList', jobs.slice(0, 5), jobItemHTML, empty);
    renderList('#allJobsList', jobs, jobItemHTML, empty);
}

function renderApps(apps) {
    const badgeMap = { Pending: 'badge-review', Accepted: 'badge-hired', Rejected: 'badge-rejected', Reviewing: 'badge-new' };
    renderList('#allAppsList', apps, (app, i) => {
        const c = colors[i % colors.length];
        const rawName = app.applicantName || app.userName || 'Applicant';
        return `<div class="applicant-item">
      <div class="app-avatar" style="background:${c.bg};color:${c.color}">${safeInitial(rawName)}</div>
      <div><div class="app-name">${escapeHtml(rawName)}</div><div class="app-role">${escapeHtml(app.jobTitle || 'Position')}</div></div>
      <span class="app-badge ${badgeMap[app.status] || 'badge-new'}">${app.status || 'Pending'}</span>
    </div>`;
    }, `<div class="empty-state"><div class="empty-icon"><i class="ti ti-file-off"></i></div><div class="empty-title">No applications found</div></div>`);
}

async function deleteUser(id) {
    if (!confirm('Delete this user?')) return;
    try { await apiFetch('/Admin/users/' + id, { method: 'DELETE' }); showToast('User deleted'); await loadUsers(); }
    catch (err) { showToast(err.message || 'Failed', false); }
}

async function deleteJob(id) {
    if (!confirm('Delete this job?')) return;
    try { await apiFetch('/Admin/jobs/' + id, { method: 'DELETE' }); showToast('Job deleted'); await loadJobs(); }
    catch (err) { showToast(err.message || 'Failed', false); }
}

async function updateUserRole(id, role) {
    try { await apiFetch(`/Admin/users/${id}/role`, { method: 'PUT', body: JSON.stringify(role) }); showToast('User role updated'); await loadUsers(); }
    catch (err) { showToast(err.message || 'Failed', false); }
}

async function toggleJobStatus(id, currentlyActive) {
    try { await apiFetch(`/Admin/jobs/${id}/status`, { method: 'PUT', body: JSON.stringify(!currentlyActive) }); showToast('Job status updated'); await loadJobs(); }
    catch (err) { showToast(err.message || 'Failed', false); }
}

async function toggleUserDetail(id) {
    const box = $('#user-detail-' + id);
    if (box.is(':visible')) { box.hide(); return; }
    box.show().html('<span style="color:#94a3b8;font-size:0.85rem">Loading...</span>');
    try {
        const r = await apiFetch('/Admin/users/' + id);
        const u = r.data;
        box.html(`<div style="font-size:0.85rem;line-height:1.6">
            <div><b>Username:</b> ${escapeHtml(u.username)}</div>
            <div><b>Email verified:</b> ${u.isEmailVerified ? 'Yes' : 'No'}</div>
        </div>`);
    } catch { box.html('<span style="color:#ef4444;font-size:0.85rem">Could not load details.</span>'); }
}

$(function () {
    $('#sidebarToggleBtn').on('click', function () { $('.sidebar').toggleClass('open'); });
    const user = getUser();
    if (!user || user.role !== 'Admin') { window.location.href = 'login.html'; return; }
    Promise.all([loadUsers(), loadJobs(), loadApps(), loadPayments(), loadStats(), loadReviews(), loadContracts()]);
});