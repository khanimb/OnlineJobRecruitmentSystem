// ── PORTFOLIO ──
let myPortfolio = [];
let editingPortfolioId = null;

async function loadPortfolio() {
    try { const r = await apiFetch('/Portfolio'); myPortfolio = r.data || []; } catch { myPortfolio = []; }
    renderPortfolio();
}

function renderPortfolio() {
    if (!myPortfolio.length) {
        $('#portfolioGrid').html(`<div class="empty-state"><div class="empty-icon"><i class="ti ti-photo-off"></i></div><div class="empty-title">No portfolio items yet</div><div class="empty-sub">Add your work to stand out to employers</div></div>`);
        return;
    }
    $('#portfolioGrid').html(myPortfolio.map(p => `
        <div class="portfolio-item">
            <img src="${FILE_BASE_URL}${p.fileUrl}" alt="${escapeHtml(p.title || '')}" />
            <div class="portfolio-item-body">
                <div class="portfolio-item-title">${escapeHtml(p.title || 'Untitled')}</div>
                <div class="portfolio-item-desc">${escapeHtml(p.description || '')}</div>
                <div class="portfolio-item-actions">
                    <button onclick="editPortfolioItem(${p.id})"><i class="ti ti-edit"></i> Edit</button>
                    <button class="danger" onclick="deletePortfolioItem(${p.id})"><i class="ti ti-trash"></i> Delete</button>
                </div>
            </div>
        </div>
    `).join(''));
}

async function submitPortfolioItem() {
    const title = $('#portfolioTitle').val().trim();
    const description = $('#portfolioDescription').val().trim();
    const file = $('#portfolioFile')[0].files[0];

    if (!editingPortfolioId && !file) { showToast('Please select an image', false); return; }

    const formData = new FormData();
    formData.append('Title', title);
    formData.append('Description', description);
    if (file) formData.append('File', file);

    try {
        if (editingPortfolioId) {
            await apiFetch(`/Portfolio/${editingPortfolioId}`, { method: 'PUT', body: formData });
            showToast('Portfolio item updated!');
        } else {
            await apiFetch('/Portfolio', { method: 'POST', body: formData });
            showToast('Portfolio item added!');
        }
        cancelPortfolioEdit();
        loadPortfolio();
    } catch (e) { showToast(e.message, false); }
}

function editPortfolioItem(id) {
    const item = myPortfolio.find(p => p.id === id);
    if (!item) return;
    editingPortfolioId = id;
    $('#portfolioTitle').val(item.title || '');
    $('#portfolioDescription').val(item.description || '');
    $('#portfolioFormTitle').text('Edit Portfolio Item');
    $('#portfolioSubmitBtn').text('Update Item');
    $('#portfolioCancelBtn').css('display', 'inline-flex');
    $('#tab-portfolio')[0].scrollIntoView({ behavior: 'smooth' });
}

function cancelPortfolioEdit() {
    editingPortfolioId = null;
    $('#portfolioTitle').val('');
    $('#portfolioDescription').val('');
    $('#portfolioFile').val('');
    $('#portfolioFormTitle').text('Add Portfolio Item');
    $('#portfolioSubmitBtn').text('Add Item');
    $('#portfolioCancelBtn').hide();
}

async function deletePortfolioItem(id) {
    if (!confirm('Delete this portfolio item?')) return;
    try {
        await apiFetch(`/Portfolio/${id}`, { method: 'DELETE' });
        showToast('Portfolio item deleted');
        loadPortfolio();
    } catch (e) { showToast(e.message, false); }
}