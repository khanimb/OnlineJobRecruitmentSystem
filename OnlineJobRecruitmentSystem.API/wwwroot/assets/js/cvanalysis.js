let hasCv = false;

$(function () {
    const user = JSON.parse(localStorage.getItem('user') || 'null');
    if (!user || user.role !== 'JobSeeker') {
        window.location.href = 'login.html';
        return;
    }
    init();
});

async function init() {
    try {
        const r = await apiFetch('/JobSeeker/profile');
        hasCv = !!(r.data && r.data.cvUrl);
    } catch {
        hasCv = false;
    }

    if (!hasCv) {
        $('#cvPage').html(`
            <div class="empty-state">
                <div class="empty-icon"><i class="ti ti-file-off"></i></div>
                <div class="empty-title">No CV uploaded yet</div>
                <div class="empty-sub">Upload your CV in your dashboard to unlock AI-powered analysis.</div>
                <button class="btn-primary" style="margin-top:14px" onclick="window.location.href='jobseekerdashboard.html'">Go to dashboard</button>
            </div>
        `);
        return;
    }

    renderLayout();
    loadJobOptions();
    loadRecommendedJobs();
    loadHistory();
}

function renderLayout() {
    $('#cvPage').html(`
        <div class="profile-section">
            <div class="profile-section-title">Analyze your CV</div>
            <div class="cv-analyze-row">
                <div class="form-group">
                    <label>Compare against a specific job (optional)</label>
                    <select id="jobSelect">
                        <option value="">General analysis (no specific job)</option>
                    </select>
                </div>
                <button class="btn-primary" id="analyzeBtn" onclick="analyzeCv()"><i class="ti ti-sparkles"></i> Analyze</button>
            </div>
        </div>

        <div class="profile-section" id="resultsCard" style="display:none">
            <div class="profile-section-title">Analysis result</div>
            <div class="score-row" id="scoreRow"></div>
            <div id="strengthsArea"></div>
            <div id="weaknessesArea"></div>
            <div id="suggestionsArea"></div>
            <div id="missingSkillsArea"></div>
        </div>

        <div class="profile-section">
            <div class="profile-section-title">Recommended jobs for you</div>
            <div id="recommendedArea"><span style="color:#94a3b8;font-size:0.85rem">Loading...</span></div>
        </div>

        <div class="profile-section">
            <div class="profile-section-title">Analysis history</div>
            <div id="historyArea"><span style="color:#94a3b8;font-size:0.85rem">Loading...</span></div>
        </div>
    `);
}

async function loadJobOptions() {
    try {
        const r = await apiFetch('/Job');
        const jobs = (r.data && r.data.data) || r.data || [];
        $('#jobSelect').append(jobs.map(j => `<option value="${j.id}">${escapeHtml(j.title || j.jobTitle)}</option>`).join(''));
    } catch { }
}

function scoreClass(score) {
    if (score >= 70) return 'score-good';
    if (score >= 40) return 'score-mid';
    return 'score-bad';
}

function renderResult(result) {
    $('#resultsCard').show();

    let scoreHtml = `
        <div class="score-badge ${scoreClass(result.overallScore)}">
            <div class="score-num">${result.overallScore}</div>
            <div class="score-label">OVERALL</div>
        </div>`;
    if (result.matchScore !== undefined && result.matchScore !== null) {
        scoreHtml += `
        <div class="score-badge ${scoreClass(result.matchScore)}">
            <div class="score-num">${result.matchScore}</div>
            <div class="score-label">JOB MATCH</div>
        </div>`;
    }
    $('#scoreRow').html(scoreHtml);

    $('#strengthsArea').html(renderPoints('Strengths', result.strengths, 'strength', 'ti-circle-check'));
    $('#weaknessesArea').html(renderPoints('Weaknesses', result.weaknesses, 'weakness', 'ti-alert-circle'));
    $('#suggestionsArea').html(renderPoints('Suggestions', result.suggestions, 'suggestion', 'ti-bulb'));

    if (result.missingSkills && result.missingSkills.length) {
        $('#missingSkillsArea').html(`
            <div class="cv-list-title">Missing skills for this job</div>
            <div class="skills-wrap">${result.missingSkills.map(s => `<span class="tag tag-red">${escapeHtml(s)}</span>`).join('')}</div>
        `);
    } else {
        $('#missingSkillsArea').html('');
    }
}

function renderPoints(title, points, cssClass, icon) {
    if (!points || !points.length) return '';
    return `<div class="cv-list-title">${title}</div>` +
        points.map(p => `<div class="cv-point ${cssClass}"><i class="ti ${icon}"></i> ${escapeHtml(p)}</div>`).join('');
}

async function analyzeCv() {
    const jobPostId = $('#jobSelect').val();
    const $btn = $('#analyzeBtn');
    $btn.prop('disabled', true).html('<i class="ti ti-loader-2"></i> Analyzing...');

    try {
        const body = jobPostId ? { jobPostId: parseInt(jobPostId) } : {};
        const r = await apiFetch('/CvAnalysis/analyze', { method: 'POST', body: JSON.stringify(body) });
        renderResult(r.data);
        loadHistory();
        showToast('Analysis complete!');
    } catch (err) {
        showToast(err.message || 'Analysis failed', false);
    } finally {
        $btn.prop('disabled', false).html('<i class="ti ti-sparkles"></i> Analyze');
    }
}

async function loadRecommendedJobs() {
    try {
        const r = await apiFetch('/CvAnalysis/recommended-jobs');
        const jobs = r.data || [];
        if (!jobs.length) {
            $('#recommendedArea').html('<span style="color:#94a3b8;font-size:0.85rem">No recommendations available yet.</span>');
            return;
        }
        $('#recommendedArea').html(jobs.map(j => `
            <div class="rec-job-item" onclick="window.location.href='jobdetail.html?id=${j.jobPostId}'">
                <div class="rec-job-info">
                    <div class="rec-job-title">${escapeHtml(j.jobTitle)}</div>
                    <div class="rec-job-company">${escapeHtml(j.companyName)}</div>
                    <div class="rec-job-reason">${escapeHtml(j.reason || '')}</div>
                </div>
                <span class="tag tag-blue">${j.matchScore}% match</span>
            </div>
        `).join(''));
    } catch {
        $('#recommendedArea').html('<span style="color:#94a3b8;font-size:0.85rem">No recommendations available yet.</span>');
    }
}

async function loadHistory() {
    try {
        const r = await apiFetch('/CvAnalysis/history');
        const items = r.data || [];
        if (!items.length) {
            $('#historyArea').html('<span style="color:#94a3b8;font-size:0.85rem">No past analyses yet.</span>');
            return;
        }
        $('#historyArea').html(items.map(h => `
            <div class="cv-history-item" onclick="loadHistoryDetail(${h.id})">
               <span>${escapeHtml(h.jobTitle || 'General analysis')}</span>
               <span class="tag tag-gray">${new Date(h.createdAt).toLocaleDateString()} · Score ${h.overallScore}</span>
               <button class="act-btn danger" title="Delete" onclick="deleteHistory(event, ${h.id})"><i class="ti ti-trash"></i></button>
            </div>
`).join(''));
    } catch {
        $('#historyArea').html('<span style="color:#94a3b8;font-size:0.85rem">No past analyses yet.</span>');
    }
}

async function deleteHistory(e, id) {
    e.stopPropagation();
    if (!confirm('Delete this analysis?')) return;
    try { await apiFetch('/CvAnalysis/history/' + id, { method: 'DELETE' }); loadHistory(); }
    catch (err) { showToast(err.message || 'Could not delete', false); }
}

async function loadHistoryDetail(id) {
    try {
        const r = await apiFetch('/CvAnalysis/history/' + id);
        renderResult(r.data);
        $('html, body').animate({ scrollTop: $('#resultsCard').offset().top - 20 }, 300);
    } catch (err) {
        showToast(err.message || 'Could not load analysis', false);
    }
}

async function sendChatMessage() {
    const input = $('#chatInput');
    const msg = input.val().trim();
    if (!msg) return;
    input.val('');
    $('#chatMessages').append(`<div style="align-self:flex-end;background:#2563EB;color:#fff;padding:8px 14px;border-radius:12px;max-width:80%;font-size:0.85rem">${escapeHtml(msg)}</div>`);
    const loadingId = 'chat-loading-' + Date.now();
    $('#chatMessages').append(`<div id="${loadingId}" style="align-self:flex-start;background:#f1f5f9;color:#334155;padding:8px 14px;border-radius:12px;max-width:80%;font-size:0.85rem">Thinking...</div>`);
    $('#chatMessages').scrollTop($('#chatMessages')[0].scrollHeight);

    try {
        const r = await apiFetch('/CvAnalysis/chat', { method: 'POST', body: JSON.stringify({ message: msg }) });
        $('#' + loadingId).text(r.data || 'No response.');
    } catch (e) {
        $('#' + loadingId).text(e.message || 'Something went wrong.').css('color', '#ef4444');
    }
    $('#chatMessages').scrollTop($('#chatMessages')[0].scrollHeight);
}