// ── CONTRACTS ──
let myContracts = [];
let contractContext = { jobPostId: null, jobSeekerProfileId: null };

function openContractModal(jobPostId, jobSeekerProfileId, candidateName) {
    contractContext = { jobPostId, jobSeekerProfileId };
    $('#contractCandidateName').val(candidateName);
    $('#contractAmount').val('');
    $('#contractModalOverlay').addClass('open');
}
function closeContractModal() { $('#contractModalOverlay').removeClass('open'); }
function closeContractModalOutside(e) { if (e.target.id === 'contractModalOverlay') closeContractModal(); }

async function createContract() {
    const amount = parseFloat($('#contractAmount').val());
    if (!amount || amount <= 0) { showToast('Please enter a valid amount', false); return; }
    try {
        await apiFetch('/Contract', {
            method: 'POST',
            body: JSON.stringify({ jobPostId: contractContext.jobPostId, jobSeekerProfileId: contractContext.jobSeekerProfileId, amount })
        });
        showToast('Contract created!');
        closeContractModal();
        loadContracts();
    } catch (e) { showToast(e.message, false); }
}

async function loadContracts() {
    try { const r = await apiFetch('/Contract'); myContracts = r.data || []; } catch { myContracts = []; }
    renderContracts();
}

function renderContracts() {
    if (!$('#contractsList').length) return;
    if (!myContracts.length) {
        $('#contractsList').html(`<div class="empty-state"><div class="empty-icon"><i class="ti ti-file-off"></i></div><div class="empty-title">No contracts yet</div><div class="empty-sub">Contracts appear here once created</div></div>`);
        return;
    }
    const user = getUser();
    const isEmployer = user.role === 'Employer';
    const statusTag = { Active: 'tag-blue', Completed: 'tag-teal', Cancelled: 'tag-gray', Disputed: 'tag-red' };

    $('#contractsList').html(myContracts.map((c, i) => {
        const col = colors[i % colors.length];
        const partyName = isEmployer ? c.jobSeekerName : c.employerName;
        const reviewTargetId = isEmployer ? c.jobSeekerUserId : c.employerUserId;
        return `<div class="job-item">
            <div class="job-logo" style="background:${col.bg};color:${col.color}">${(partyName || '?')[0].toUpperCase()}</div>
            <div class="job-info">
                <div class="job-name">${c.jobTitle}${partyName ? ' — ' + partyName : ''}</div>
                <div class="job-meta"><span><i class="ti ti-currency-dollar"></i> $${c.amount}</span><span><i class="ti ti-calendar"></i> ${new Date(c.createdAt).toLocaleDateString()}</span></div>
            </div>
            <span class="tag ${statusTag[c.status] || 'tag-gray'}">${c.status}</span>
            <div style="display:flex;gap:8px;margin-left:10px">
                ${isEmployer && c.status === 'Active' ? `<button class="btn-outline" onclick="updateContractStatus(${c.id}, 'Completed')">Mark Done</button>` : ''}
                ${isEmployer && c.status === 'Completed' && !c.isPaid ? `<button class="btn-primary" onclick="payContract(${c.id})"><i class="ti ti-credit-card"></i> Pay</button>` : ''}
                ${isEmployer && c.status === 'Completed' && c.isPaid ? `<span class="tag tag-teal">Paid</span>` : ''}
                ${c.status === 'Completed' ? `<button class="btn-outline" onclick="openReviewModal(${reviewTargetId})"><i class="ti ti-star"></i> Leave Review</button>` : ''}
            </div>
        </div>`;
    }).join(''));
}

async function updateContractStatus(id, status) {
    try {
        await apiFetch(`/Contract/${id}`, { method: 'PUT', body: JSON.stringify({ status }) });
        showToast('Contract updated!');
        loadContracts();
    } catch (e) { showToast(e.message, false); }
}

async function payContract(contractId) {
    try {
        const r = await apiFetch(`/ContractPayment/${contractId}`, { method: 'POST' });
        if (r.data && r.data.url) window.location.href = r.data.url;
    } catch (e) { showToast(e.message || 'Payment failed to start', false); }
}