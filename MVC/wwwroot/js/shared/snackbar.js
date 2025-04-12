/**
 * Snackbar notification system for Alpha
 */

class SnackbarService {
    constructor() {
        this.containerId = 'snackbar-container';
        this.autoHideDelay = 5000; // 5 seconds
        this.ensureContainer();
    }

    /**
     * Ensures the snackbar container exists in the DOM
     */
    ensureContainer() {
        if (!document.getElementById(this.containerId)) {
            const container = document.createElement('div');
            container.id = this.containerId;
            container.className = 'snackbar-container';
            document.body.appendChild(container);
        }
    }

    /**
     * Show a success notification
     * @param {string} message - The message to display
     * @param {number} duration - Optional custom duration in milliseconds
     */
    success(message, duration = this.autoHideDelay) {
        this.show(message, 'success', duration);
    }

    /**
     * Show an error notification
     * @param {string} message - The message to display
     * @param {number} duration - Optional custom duration in milliseconds
     */
    error(message, duration = this.autoHideDelay) {
        this.show(message, 'error', duration);
    }

    /**
     * Show an info notification
     * @param {string} message - The message to display
     * @param {number} duration - Optional custom duration in milliseconds
     */
    info(message, duration = this.autoHideDelay) {
        this.show(message, 'info', duration);
    }

    /**
     * Show a warning notification
     * @param {string} message - The message to display
     * @param {number} duration - Optional custom duration in milliseconds
     */
    warning(message, duration = this.autoHideDelay) {
        this.show(message, 'warning', duration);
    }

    /**
     * Create and display a snackbar
     * @param {string} message - The message to display
     * @param {string} type - The type of snackbar (success, error, info, warning)
     * @param {number} duration - Duration in milliseconds
     */
    show(message, type, duration) {
        this.ensureContainer();
        const container = document.getElementById(this.containerId);

        // Create snackbar element
        const snackbar = document.createElement('div');
        snackbar.className = `snackbar ${type}`;

        const content = document.createElement('div');
        content.className = 'snackbar-content';
        content.textContent = message;

        const closeBtn = document.createElement('button');
        closeBtn.className = 'snackbar-close';
        closeBtn.innerHTML = '&times;';
        closeBtn.addEventListener('click', () => this.dismiss(snackbar));

        snackbar.appendChild(content);
        snackbar.appendChild(closeBtn);
        container.appendChild(snackbar);

        // Trigger animation
        setTimeout(() => {
            snackbar.classList.add('show');
        }, 10);

        // Auto dismiss after duration
        const timerId = setTimeout(() => {
            this.dismiss(snackbar);
        }, duration);

        // Store timer ID so we can clear it if manually dismissed
        snackbar.dataset.timerId = timerId;
    }

    /**
     * Dismiss a snackbar with animation
     * @param {HTMLElement} snackbar - The snackbar element to dismiss
     */
    dismiss(snackbar) {
        // Clear the auto-dismiss timer
        clearTimeout(snackbar.dataset.timerId);

        // Add the fade-out class
        snackbar.classList.add('fade-out');

        // Remove the element after animation completes
        setTimeout(() => {
            if (snackbar.parentNode) {
                snackbar.parentNode.removeChild(snackbar);
            }
        }, 500); // Match the animation duration
    }

    /**
     * Clear all snackbars
     */
    clearAll() {
        const container = document.getElementById(this.containerId);
        if (container) {
            while (container.firstChild) {
                this.dismiss(container.firstChild);
            }
        }
    }
}

// Create the global snackbar service
const snackbar = new SnackbarService();