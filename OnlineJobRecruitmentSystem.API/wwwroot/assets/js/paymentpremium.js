// ── PREMIUM ──
async function loadPremium() {
    try {
        const r = await apiFetch('/PaymentPremium/status');
        const status = r.data || {};
        const banner = $('#premiumStatusBanner');
        if (status.isPremium) {
            banner.attr('class', 'premium-status-banner active');
            $('#premiumStatusTitle').text('You are a Premium member');
            $('#premiumStatusSub').text(status.expiryDate ? `Valid until ${new Date(status.expiryDate).toLocaleDateString()}` : '');
        } else {
            banner.attr('class', 'premium-status-banner inactive');
            $('#premiumStatusTitle').text('You are not a Premium member');
            $('#premiumStatusSub').text('Choose a plan below to unlock premium features');
        }
    } catch { }

    try {
        const r = await apiFetch('/PaymentPremium');
        const items = r.data || [];
        if (!items.length) {
            $('#premiumHistoryList').html(`<div class="empty-state"><div class="empty-icon"><i class="ti ti-receipt-off"></i></div><div class="empty-title">No payments yet</div></div>`);
            return;
        }
        $('#premiumHistoryList').html(items.map(p => `
            <div class="premium-history-item">
                <span style="text-transform:capitalize">${escapeHtml(p.plan)} plan</span>
                <span>$${p.amount}</span>
                <span class="tag ${p.status === 'completed' ? 'tag-teal' : 'tag-gray'}">${escapeHtml(p.status)}</span>
                <span style="color:#94a3b8">${new Date(p.createdAt).toLocaleDateString()}</span>
            </div>
        `).join(''));
    } catch { }
}

async function subscribePlan(plan) {
    try {
        const r = await apiFetch('/PaymentPremium/create-checkout-session', { method: 'POST', body: JSON.stringify({ plan }) });
        if (r.data && r.data.url) window.location.href = r.data.url;
    } catch (e) { showToast(e.message || 'Could not start checkout', false); }
}