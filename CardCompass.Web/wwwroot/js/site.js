// CardCompass Interactive Client Script
document.addEventListener('DOMContentLoaded', function () {
    // 1. Comparison tray management
    initCompareTray();

    // 2. Favorite toggle AJAX
    initFavoriteButtons();

    // 3. Calculator real-time updates
    initCalculatorSync();
});

// Comparison Tray State & Storage
const CC_COMPARE_KEY = 'cardcompass_compare_ids';

function getComparedCardIds() {
    try {
        const stored = localStorage.getItem(CC_COMPARE_KEY);
        return stored ? JSON.parse(stored) : [];
    } catch (e) {
        return [];
    }
}

function setComparedCardIds(ids) {
    try {
        localStorage.setItem(CC_COMPARE_KEY, JSON.stringify(ids));
        updateCompareTrayUI();
    } catch (e) {
        console.error(e);
    }
}

function toggleCardComparison(cardId, cardName) {
    let ids = getComparedCardIds();
    const index = ids.indexOf(cardId);

    if (index > -1) {
        ids.splice(index, 1);
        showToast('Removed', `${cardName || 'Card'} removed from comparison tray.`);
    } else {
        if (ids.length >= 3) {
            showToast('Limit Reached', 'You can compare up to 3 cards at a time. Remove one to add another.', 'warning');
            return;
        }
        ids.push(cardId);
        showToast('Added to Compare', `${cardName || 'Card'} added to comparison tray.`, 'success');
    }

    setComparedCardIds(ids);
}

function updateCompareTrayUI() {
    const ids = getComparedCardIds();
    const tray = document.getElementById('ccCompareTray');
    const badge = document.getElementById('ccCompareCount');
    const btn = document.getElementById('ccCompareLaunchBtn');

    if (!tray) return;

    if (ids.length > 0) {
        tray.classList.remove('d-none');
        if (badge) badge.textContent = ids.length;
        if (btn) btn.href = `/Compare?ids=${ids.join(',')}`;
    } else {
        tray.classList.add('d-none');
    }

    // Update active compare buttons in catalogue
    document.querySelectorAll('.btn-add-compare').forEach(el => {
        const cId = parseInt(el.getAttribute('data-card-id'), 10);
        if (ids.includes(cId)) {
            el.classList.add('active');
            el.innerHTML = '<i class="bi bi-check-lg me-1"></i> Comparing';
        } else {
            el.classList.remove('active');
            el.innerHTML = '<i class="bi bi-plus-lg me-1"></i> Compare';
        }
    });
}

function initCompareTray() {
    updateCompareTrayUI();

    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-add-compare');
        if (btn) {
            e.preventDefault();
            const cardId = parseInt(btn.getAttribute('data-card-id'), 10);
            const cardName = btn.getAttribute('data-card-name');
            toggleCardComparison(cardId, cardName);
        }

        const clearBtn = e.target.closest('#ccClearCompareBtn');
        if (clearBtn) {
            e.preventDefault();
            setComparedCardIds([]);
            showToast('Cleared', 'Comparison tray cleared.');
        }
    });
}

// Favorite Toggle AJAX
function initFavoriteButtons() {
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-toggle-favorite');
        if (!btn) return;

        e.preventDefault();
        const cardId = btn.getAttribute('data-card-id');

        fetch(`/Account/ToggleFavorite?cardId=${cardId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(res => {
            if (res.status === 401) {
                window.location.href = `/Account/Login?returnUrl=${encodeURIComponent(window.location.pathname + window.location.search)}`;
                return null;
            }
            return res.json();
        })
        .then(data => {
            if (!data) return;
            if (data.isFavorite) {
                btn.classList.add('text-danger');
                btn.classList.remove('text-muted');
                btn.innerHTML = '<i class="bi bi-heart-fill"></i>';
                showToast('Favorite Saved', 'Card added to your saved favorites.', 'success');
            } else {
                btn.classList.remove('text-danger');
                btn.classList.add('text-muted');
                btn.innerHTML = '<i class="bi bi-heart"></i>';
                showToast('Favorite Removed', 'Card removed from your favorites.');
            }
        })
        .catch(err => console.error(err));
    });
}

// Calculator Sliders Sync
function initCalculatorSync() {
    const inputs = document.querySelectorAll('.cc-spend-slider');
    inputs.forEach(slider => {
        const targetId = slider.getAttribute('data-target-input');
        const numInput = document.getElementById(targetId);
        if (!numInput) return;

        slider.addEventListener('input', function () {
            numInput.value = this.value;
            updateCalculatorTotal();
        });

        numInput.addEventListener('input', function () {
            slider.value = this.value;
            updateCalculatorTotal();
        });
    });

    updateCalculatorTotal();
}

function updateCalculatorTotal() {
    const totalEl = document.getElementById('ccTotalSpendDisplay');
    if (!totalEl) return;

    let total = 0;
    document.querySelectorAll('.cc-spend-number').forEach(input => {
        const val = parseFloat(input.value) || 0;
        total += val;
    });

    totalEl.textContent = '₹' + total.toLocaleString('en-IN');
}

// Toast notification helper
function showToast(title, message, type = 'info') {
    const container = document.getElementById('ccToastContainer');
    if (!container) return;

    const toastId = 'toast-' + Date.now();
    const bgClass = type === 'success' ? 'bg-success text-white' : (type === 'warning' ? 'bg-warning text-dark' : 'bg-dark text-white');
    
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center ${bgClass} border-0 show shadow-lg mb-2" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}</strong>: ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close" onclick="document.getElementById('${toastId}').remove()"></button>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', toastHtml);

    setTimeout(() => {
        const el = document.getElementById(toastId);
        if (el) el.remove();
    }, 4000);
}
