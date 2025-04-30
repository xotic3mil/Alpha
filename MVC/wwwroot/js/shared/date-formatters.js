/**
 * Shared date formatting utilities
 */

/**
 * Format a date string to a localized date string
 * @param {string|Date} dateString - The date to format
 * @param {boolean} includeTime - Whether to include time in the formatted result
 * @returns {string|null} - Formatted date or null if invalid
 */
function formatDateString(dateString, includeTime = false) {
    if (!dateString) return null;

    const date = new Date(dateString);
    if (isNaN(date)) return null;

    return includeTime
        ? date.toLocaleString()
        : date.toLocaleDateString();
}

/**
 * Calculate and format days remaining until a target date
 * @param {string|Date} endDate - The target date
 * @returns {string} - Formatted string showing days remaining
 */
function calculateDaysRemaining(endDate) {
    if (!endDate) return 'No end date';

    const end = new Date(endDate);
    if (isNaN(end)) return 'Invalid date';

    const now = new Date();
    const daysLeft = Math.ceil((end - now) / (1000 * 60 * 60 * 24));

    if (daysLeft < 0) return 'Overdue';
    if (daysLeft === 0) return 'Due today';
    return `${daysLeft} day${daysLeft !== 1 ? 's' : ''} left`;
}

/**
 * Format a date for form inputs (YYYY-MM-DD)
 * @param {string|Date} date - The date to format
 * @returns {string|null} - Date formatted for input fields or null if invalid
 */
function formatDateForInput(date) {
    if (!date) return null;

    const d = new Date(date);
    if (isNaN(d)) return null;

    return d.toISOString().split('T')[0];
}

/**
 * Format a relative time (e.g., "2 days ago", "Just now")
 * @param {string|Date} date - The date to format as relative time
 * @returns {string} - Formatted relative time string
 */
function formatRelativeTime(date) {
    if (!date) return '';

    const d = new Date(date);
    if (isNaN(d)) return '';

    const now = new Date();
    const diffSeconds = Math.floor((now - d) / 1000);

    if (diffSeconds < 60) return 'Just now';
    if (diffSeconds < 3600) return `${Math.floor(diffSeconds / 60)} minute${Math.floor(diffSeconds / 60) !== 1 ? 's' : ''} ago`;
    if (diffSeconds < 86400) return `${Math.floor(diffSeconds / 3600)} hour${Math.floor(diffSeconds / 3600) !== 1 ? 's' : ''} ago`;
    if (diffSeconds < 604800) return `${Math.floor(diffSeconds / 86400)} day${Math.floor(diffSeconds / 86400) !== 1 ? 's' : ''} ago`;
    if (diffSeconds < 2592000) return `${Math.floor(diffSeconds / 604800)} week${Math.floor(diffSeconds / 604800) !== 1 ? 's' : ''} ago`;

    return formatDateString(date, true);
}