// ═══════════════════════════════════════════════════════════════════════════════
// BLINDMATCH CLIENT UTILITIES
// ═══════════════════════════════════════════════════════════════════════════════

/**
 * Initialize character counters on form fields.
 * Usage: Add data-maxlength="2000" data-counter="abstractCounter" to input
 */
document.addEventListener('DOMContentLoaded', function () {
    // Character Counter Initialization
    const counterFields = document.querySelectorAll('[data-counter]');
    counterFields.forEach(field => {
        const maxLength = field.getAttribute('data-maxlength');
        const counterId = field.getAttribute('data-counter');

        if (maxLength && counterId) {
            // Create counter element if it doesn't exist
            let counterElement = document.getElementById(counterId);
            if (!counterElement) {
                counterElement = document.createElement('small');
                counterElement.id = counterId;
                counterElement.className = 'text-muted d-block mt-2';
                field.parentNode.appendChild(counterElement);
            }

            // Update counter on input
            const updateCounter = () => {
                const current = field.value.length;
                counterElement.textContent = `${current} / ${maxLength} characters`;

                // Change color if approaching limit
                if (current > maxLength * 0.8) {
                    counterElement.classList.add('text-warning');
                    counterElement.classList.remove('text-muted');
                } else {
                    counterElement.classList.remove('text-warning');
                    counterElement.classList.add('text-muted');
                }
            };

            field.addEventListener('input', updateCounter);
            updateCounter(); // Initial update
        }
    });

    // Auto-dismiss Alerts
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        if (alert.querySelector('[data-bs-dismiss]')) {
            // Has dismiss button, let user close it
            return;
        }
        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Search Filter for Tables
    const searchInputs = document.querySelectorAll('[data-search-table]');
    searchInputs.forEach(searchInput => {
        const tableId = searchInput.getAttribute('data-search-table');
        const table = document.getElementById(tableId);

        if (table) {
            searchInput.addEventListener('keyup', function () {
                const searchTerm = this.value.toLowerCase();
                const rows = table.querySelectorAll('tbody tr');

                rows.forEach(row => {
                    const text = row.textContent.toLowerCase();
                    row.style.display = text.includes(searchTerm) ? '' : 'none';
                });
            });
        }
    });

    // Form validation feedback
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            if (!this.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            this.classList.add('was-validated');
        });
    });
});