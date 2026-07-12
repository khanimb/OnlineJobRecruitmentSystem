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
    const user = getUser();
    const isEmployer = user.role === 'Employer';
    const statusTag = { Active: 'tag-blue', Completed: 'tag-teal', Cancelled: 'tag-gray', Disputed: 'tag-red' };

    renderList('#contractsList', myContracts, (c, i) => {
        const col = colors[i % colors.length];
        const partyName = isEmployer ? c.jobSeekerName : c.employerName;
        const reviewTargetId = isEmployer ? c.jobSeekerUserId : c.employerUserId;
        return `<div class="job-item">
            <div class="job-logo" style="background:${col.bg};color:${col.color}">${(partyName || '?')[0].toUpperCase()}</div>
            <div class="job-info">
                <div class="job-name">${escapeHtml(c.jobTitle)}${partyName ? ' — ' + escapeHtml(partyName) : ''}</div>
                <div class="job-meta"><span><i class="ti ti-currency-dollar"></i> $${c.amount}</span><span><i class="ti ti-calendar"></i> ${new Date(c.createdAt).toLocaleDateString()}</span></div>
            </div>
            <span class="tag ${statusTag[c.status] || 'tag-gray'}">${c.status}</span>
            <div style="display:flex;gap:8px;margin-left:10px">
                ${isEmployer && c.status === 'Active' ? `<button class="btn-outline" onclick="updateContractStatus(${c.id}, 'Completed')">Mark Done</button>` : ''}
                ${isEmployer && c.status === 'Completed' && !c.isPaid ? `<button class="btn-primary" onclick="payContract(${c.id})"><i class="ti ti-credit-card"></i> Pay</button>` : ''}
                ${isEmployer && c.status === 'Completed' && c.isPaid ? `<span class="tag tag-teal">Paid</span>` : ''}
                ${c.status === 'Completed' ? `<button class="btn-outline" onclick="openReviewModal(${reviewTargetId})"><i class="ti ti-star"></i> Leave Review</button>` : ''}
                <button class="btn-outline" onclick="toggleContractDetail(${c.id})"><i class="ti ti-info-circle"></i></button>
            </div>
        </div>
        <div id="contract-detail-${c.id}" style="display:none;padding:12px 16px;background:#f8fafc;border-radius:10px;margin:-6px 0 10px"></div>`;
    }, `<div class="empty-state"><div class="empty-icon"><i class="ti ti-file-off"></i></div><div class="empty-title">No contracts yet</div><div class="empty-sub">Contracts appear here once created</div></div>`);
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
        const r = await apiFetch('/ContractPayment', { method: 'POST', body: JSON.stringify({ contractId }) });
        if (r.data && r.data.url) window.location.href = r.data.url;
    } catch (e) { showToast(e.message || 'Payment failed to start', false); }
}

        async function toggleContractDetail(id) {
    const box = $('#contract-detail-' + id);
    if (box.is(':visible')) { box.hide(); return; }
    box.show().html('<span style="color:#94a3b8;font-size:0.85rem">Loading...</span>');
    try {
        const cr = await apiFetch('/Contract/' + id);
        const c = cr.data;
        let html = `<div style="font-size:0.85rem;line-height:1.6">
            <div><b>Payment type:</b> ${escapeHtml(c.paymentType)}</div>
            <div><b>Completed at:</b> ${c.completedAt ? new Date(c.completedAt).toLocaleDateString() : '—'}</div>
        </div>`;
        if (c.isPaid) {
            const pr = await apiFetch('/ContractPayment/' + id);
            const payments = pr.data || [];
            if (payments.length) {
                html += '<div style="margin-top:10px;font-size:0.85rem"><b>Payments</b></div>';
                html += payments.map(p => `<div style="display:flex;justify-content:space-between;font-size:0.8rem;padding:4px 0">
                    <span>${escapeHtml(p.status)} · ${new Date(p.createdAt).toLocaleDateString()}</span>
                    <span>Total $${p.totalAmount} — fee $${p.platformFee} — payout $${p.jobSeekerAmount}</span>
                </div>`).join('');
            }
        }
        box.html(html);
    } catch { box.html('<span style="color:#ef4444;font-size:0.85rem">Could not load details.</span>'); }
}