let allJobs = [];

async function loadJobs() {
    try {
        const r = await apiFetch('/Job');
        allJobs = r.data || [];
    } catch {
        allJobs = [];
    }
    applyFilters();
}

function applyFilters() {
    const keyword = (document.getElementById('searchInput').value || '').toLowerCase();
    const location = (document.getElementById('locationInput').value || '').toLowerCase();
    const salary = parseInt(document.getElementById('salaryFilter').value) || 0;
    const sort = document.getElementById('sortSelect').value;

    const checkedTypes = [...document.querySelectorAll('.filter-check input[type=checkbox]:checked')]
        .filter(c => ['Full-time', 'Part-time', 'Remote', 'Contract', 'Internship'].includes(c.value))
        .map(c => c.value);

    const checkedCats = [...document.querySelectorAll('.filter-check input[type=checkbox]:checked')]
        .filter(c => ['Technology', 'Finance', 'Healthcare', 'Education', 'Marketing', 'Design'].includes(c.value))
        .map(c => c.value);

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

    document.getElementById('jobsCount').textContent = `${filtered.length} job${filtered.length !== 1 ? 's' : ''} found`;
    renderJobs(filtered);
}

function resetFilters() {
    document.getElementById('searchInput').value = '';
    document.getElementById('locationInput').value = '';
    document.getElementById('salaryFilter').value = '';
    document.querySelectorAll('.filter-check input').forEach(c => c.checked = false);
    applyFilters();
}

const colors = [
    { bg: '#eff6ff', color: '#2563EB' }, { bg: '#ecfdf5', color: '#10b981' },
    { bg: '#fffbeb', color: '#f59e0b' }, { bg: '#f0fdf4', color: '#16a34a' }, { bg: '#fdf4ff', color: '#a855f7' }
];

function renderJobs(jobs) {
    const el = document.getElementById('jobsList');
    if (!jobs.length) {
        el.innerHTML = `<div class="empty-state"><div class="empty-icon"><i class="ti ti-briefcase-off"></i></div><div class="empty-title">No jobs found</div><div class="empty-sub">Try adjusting your filters</div></div>`;
        return;
    }
    el.innerHTML = jobs.map((job, i) => {
        const c = colors[i % colors.length];
        const title = job.title || job.jobTitle || 'Job';
        const company = job.companyName || 'Company';
        const loc = job.location || 'Remote';
        const type = job.jobType || 'Full-time';
        const sal = job.salaryMin ? `$${job.salaryMin.toLocaleString()} / mo` : 'Negotiable';
        return `<div class="job-card-list" onclick="window.location.href='jobdetail.html?id=${job.id}'">
      <div class="job-logo" style="background:${c.bg};color:${c.color};width:48px;height:48px;border-radius:12px;display:flex;align-items:center;justify-content:center;font-weight:700;font-size:1.1rem;flex-shrink:0">${company[0].toUpperCase()}</div>
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
}

window.addEventListener('DOMContentLoaded', () => {
    const params = new URLSearchParams(window.location.search);
    if (params.get('keyword')) document.getElementById('searchInput').value = params.get('keyword');
    if (params.get('location')) document.getElementById('locationInput').value = params.get('location');
    if (params.get('category')) {
        document.querySelectorAll('.filter-check input').forEach(c => { if (c.value === params.get('category')) c.checked = true; });
    }
    loadJobs();
});