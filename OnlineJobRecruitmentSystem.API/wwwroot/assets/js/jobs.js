let allJobs = [];

async function loadJobs() {
    showLoading('#jobsList');
    const salary = parseInt($('#salaryFilter').val()) || null;
    const qs = new URLSearchParams({ pageSize: 200, page: 1 });
    if (salary) qs.set('salaryMin', salary);
    try {
        const r = await apiFetch('/Job?' + qs.toString());
        allJobs = r.data?.data || [];
    } catch (e) { allJobs = []; showToast('Failed to load jobs. Please try again.', false); }
    applyFilters();
}

function applyFilters() {
    const keyword = ($('#searchInput').val() || '').toLowerCase();
    const location = ($('#locationInput').val() || '').toLowerCase();
    const salary = parseInt($('#salaryFilter').val()) || 0;
    const sort = $('#sortSelect').val();

    const checkedTypes = $('.filter-check input[type=checkbox]:checked').map(function () { return $(this).val(); }).get()
        .filter(v => ['FullTime', 'PartTime', 'Remote', 'Contract', 'Internship'].includes(v));

    const checkedCats = $('.filter-check input[type=checkbox]:checked').map(function () { return $(this).val(); }).get()
        .filter(v => ['Technology', 'Finance', 'Healthcare', 'Education', 'Marketing', 'Design'].includes(v));

    let filtered = allJobs.filter(job => {
        const title = (job.title || job.jobTitle || '').toLowerCase();
        const loc = (job.location || '').toLowerCase();
        const type = job.jobType || job.type || '';
        const cat = job.category || '';
        const minSal = job.salaryMin || 0;

        if (keyword && !title.includes(keyword)) return false;
        if (location && !loc.includes(location)) return false;
        if (salary && minSal < salary) return false;
        if (checkedTypes.length && !checkedTypes.includes(type)) return false;
        if (checkedCats.length && !checkedCats.includes(cat)) return false;
        return true;
    });

    if (sort === 'salary') filtered.sort((a, b) => (b.salaryMin || 0) - (a.salaryMin || 0));

    $('#jobsCount').text(`${filtered.length} job${filtered.length !== 1 ? 's' : ''} found`);
    renderJobs(filtered);
}

function resetFilters() {
    $('#searchInput').val('');
    $('#locationInput').val('');
    $('#salaryFilter').val('');
    $('.filter-check input').prop('checked', false);
    applyFilters();
}

function renderJobs(jobs) {
    if (!jobs.length) {
        $('#jobsList').html(`<div class="empty-state"><div class="empty-icon"><i class="ti ti-briefcase-off"></i></div><div class="empty-title">No jobs found</div><div class="empty-sub">Try adjusting your filters</div></div>`);
        return;
    }
    const html = jobs.map((job, i) => {
        const c = colors[i % colors.length];
        const title = escapeHtml(job.title || job.jobTitle || 'Job');
        const company = escapeHtml(job.companyName || 'Company');
        const loc = escapeHtml(job.location || 'Remote');
        const type = escapeHtml(job.jobType || 'Full-time');
        const sal = job.salaryMin ? `$${job.salaryMin.toLocaleString()} / mo` : 'Negotiable';
        return `<div class="job-card-list" onclick="window.location.href='jobdetail.html?id=${job.id}'">
      <div class="job-logo" style="background:${c.bg};color:${c.color};width:48px;height:48px;border-radius:12px;display:flex;align-items:center;justify-content:center;font-weight:700;font-size:1.1rem;flex-shrink:0">${safeInitial(job.companyName)}</div>
      <div class="job-info" style="flex:1;min-width:0">
        <div class="job-name" style="font-size:1rem;font-weight:600;color:#0f172a">${title}</div>
        <div class="job-meta" style="font-size:0.8rem;color:#94a3b8;margin-top:4px;display:flex;gap:14px;flex-wrap:wrap">
          <span><i class="ti ti-building"></i> ${company}</span>
          <span><i class="ti ti-map-pin"></i> ${loc}</span>
          <span><i class="ti ti-clock"></i> ${type}</span>
        </div>
      </div>
      <div style="display:flex;flex-direction:column;align-items:flex-end;gap:8px;flex-shrink:0">
        <span style="font-size:1rem;font-weight:700;color:#2563EB">${sal}</span>
        <button class="apply-btn" onclick="event.stopPropagation();window.location.href='jobdetail.html?id=${job.id}'">View details</button>
      </div>
    </div>`;
    }).join('');
    $('#jobsList').html(html);
}

$(function () {
    const params = new URLSearchParams(window.location.search);
    if (params.get('keyword')) $('#searchInput').val(params.get('keyword'));
    if (params.get('location')) $('#locationInput').val(params.get('location'));
    if (params.get('category')) {
        $('.filter-check input').each(function () {
            if ($(this).val() === params.get('category')) $(this).prop('checked', true);
        });
    }
    loadJobs();
});