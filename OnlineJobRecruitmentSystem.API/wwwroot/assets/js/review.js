// ── REVIEWS ──
let selectedRating = 0;
let reviewRevieweeId = null;
let editingReviewId = null;

function openReviewModal(revieweeId) {
    editingReviewId = null;
    reviewRevieweeId = revieweeId;
    selectedRating = 0;
    $('#reviewComment').val('');
    $('#reviewModalTitle').text('Leave a Review');
    $('#starPicker i').removeClass('active');
    $('#reviewModalOverlay').addClass('open');
}
function closeReviewModal() { $('#reviewModalOverlay').removeClass('open'); }
function closeReviewModalOutside(e) { if (e.target.id === 'reviewModalOverlay') closeReviewModal(); }

function setRating(val) {
    selectedRating = val;
    $('#starPicker i').each(function () {
        $(this).toggleClass('active', parseInt($(this).data('val')) <= val);
    });
}

async function submitReview() {
    if (!selectedRating) { showToast('Please select a rating', false); return; }
    const comment = $('#reviewComment').val().trim();
    try {
        if (editingReviewId) {
            await apiFetch('/Review/' + editingReviewId, { method: 'PUT', body: JSON.stringify({ rating: selectedRating, comment }) });
            showToast('Review updated!');
        } else {
            await apiFetch('/Review', { method: 'POST', body: JSON.stringify({ revieweeId: reviewRevieweeId, rating: selectedRating, comment }) });
            showToast('Review submitted!');
        }
        closeReviewModal();
        loadReviews();
    } catch (e) { showToast(e.message, false); }
}

function starsHtml(rating) {
    return '<i class="ti ti-star-filled"></i>'.repeat(rating) + '<i class="ti ti-star"></i>'.repeat(5 - rating);
}

async function loadReviews() {
    const user = getUser();
    let received = [], written = [];
    try { const r = await apiFetch('/Review/user/' + user.id); received = r.data || []; } catch { }
    try { const r = await apiFetch('/Review/my'); written = r.data || []; } catch { }
    try {
        const r = await apiFetch('/Review/user/' + user.id + '/rating');
        $('#receivedRatingSub').text(received.length ? `Average rating: ${(r.data || 0).toFixed(1)} / 5 (${received.length} reviews)` : 'No ratings yet');
    } catch { }
    renderReceivedReviews(received);
    renderWrittenReviews(written);
}

function renderReceivedReviews(items) {
    if (!items.length) {
        $('#receivedReviewsList').html(`<div class="empty-state"><div class="empty-icon"><i class="ti ti-star-off"></i></div><div class="empty-title">No reviews received yet</div></div>`);
        return;
    }
    $('#receivedReviewsList').html(items.map(r => `
        <div class="review-item" style="padding:16px 24px">
            <div class="review-top">
                <div class="review-name">${escapeHtml(r.reviewerName)}</div>
                <div class="review-stars">${starsHtml(r.rating)}</div>
            </div>
            ${r.comment ? `<div class="review-comment">${escapeHtml(r.comment)}</div>` : ''}
            <div class="review-date">${new Date(r.createdAt).toLocaleDateString()}</div>
        </div>
    `).join(''));
}

function renderWrittenReviews(items) {
    if (!items.length) {
        $('#writtenReviewsList').html(`<div class="empty-state"><div class="empty-icon"><i class="ti ti-star-off"></i></div><div class="empty-title">No reviews written yet</div><div class="empty-sub">Leave a review after completing a contract</div></div>`);
        return;
    }
    $('#writtenReviewsList').html(items.map(r => `
        <div class="review-item" style="padding:16px 24px">
            <div class="review-top">
                <div class="review-name">${escapeHtml(r.revieweeName)}</div>
                <div class="review-stars">${starsHtml(r.rating)}</div>
            </div>
            ${r.comment ? `<div class="review-comment">${escapeHtml(r.comment)}</div>` : ''}
            <div class="review-date">${new Date(r.createdAt).toLocaleDateString()}</div>
            <div class="portfolio-item-actions" style="margin-top:8px">
                <button class="edit-review-btn" data-id="${r.id}" data-rating="${r.rating}" data-comment="${escapeHtml(r.comment || '')}"><i class="ti ti-edit"></i> Edit</button>
                <button class="danger" onclick="deleteReview(${r.id})"><i class="ti ti-trash"></i> Delete</button>
            </div>
        </div>
    `).join(''));

    $('.edit-review-btn').off('click').on('click', function () {
        editReview(parseInt($(this).data('id')), parseInt($(this).data('rating')), $(this).data('comment'));
    });
}

function editReview(id, rating, comment) {
    editingReviewId = id;
    $('#reviewModalTitle').text('Edit Review');
    $('#reviewComment').val(comment);
    $('#starPicker i').removeClass('active');
    setRating(rating);
    $('#reviewModalOverlay').addClass('open');
}

async function deleteReview(id) {
    if (!confirm('Delete this review?')) return;
    try {
        await apiFetch('/Review/' + id, { method: 'DELETE' });
        showToast('Review deleted');
        loadReviews();
    } catch (e) { showToast(e.message, false); }
}