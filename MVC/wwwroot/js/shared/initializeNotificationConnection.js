let notificationConnection = null;

function initializeNotificationConnection() {

    if (typeof signalR === 'undefined') {

        const script = document.createElement('script');
        script.src = "https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js";

        script.onload = function () {
            setupNotificationConnection();
        };

        script.onerror = function () {
            startNotificationPolling();
        };

        document.head.appendChild(script);
        return; 
    }

    setupNotificationConnection();
}

function setupNotificationConnection() {
    if (notificationConnection) {
        notificationConnection.stop();
    }

    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    notificationConnection.on("ReceiveNotification", function (notification) {

        const currentCount = parseInt($('#notificationBadge').text()) || 0;
        $('#notificationBadge').text(currentCount + 1).show();

        if ($('#notificationsContainer').is(':visible')) {
            prependNewNotification(notification);
        }

        if (typeof snackbar !== 'undefined') {
            snackbar.info(notification.title);
        }
    });

    notificationConnection.on("AllNotificationsRead", function () {
        $('#notificationBadge').hide();

        if ($('#notificationsContainer').is(':visible')) {
            loadNotifications();
        }
    });

    notificationConnection.on("NotificationRead", function (notificationId) {
        const currentCount = parseInt($('#notificationBadge').text()) || 0;
        if (currentCount > 1) {
            $('#notificationBadge').text(currentCount - 1);
        } else {
            $('#notificationBadge').hide();
        }

        $(`.notification-item[data-notification-id="${notificationId}"]`)
            .removeClass('unread')
            .addClass('read');
    });

    notificationConnection.start()
        .then(function () {

            const userId = $('#currentUserId').val();
            const isAdmin = $('#isAdmin').val() === 'true';
            const isProjectManager = $('#isProjectManager').val() === 'true';
            console.log("User roles - Admin:", isAdmin, "Raw value:", $('#isAdmin').val());

            if (isAdmin) {
                console.log("Admin detected, joining admin group");
                notificationConnection.invoke("JoinAdminGroup")
                    .then(() => console.log("Successfully joined admin group"))
                    .catch(err => console.error("Error joining admin group:", err));
            }

            if (isProjectManager) {
                notificationConnection.invoke("JoinProjectManagerGroup")
                    .catch(err => console.error("Error joining project manager group:", err));
            }
            

            if (userId) {
                notificationConnection.invoke("JoinUserGroup", userId)
                    .catch(err => console.error("Error joining user group:", err));
            }

            loadNotificationCount();
        })
        .catch(function (err) {
            console.error("Error starting notification SignalR:", err);

            startNotificationPolling();
        });
}

function handleSpecificNotification(notification) {
    const isAdmin = $('#isAdmin').val() === 'True';
    const isProjectManager = $('#isProjectManager').val() === 'True';

    switch (notification.type) {
        case "ProjectJoinRequest":
            if (isAdmin) {
                if (typeof toastr !== 'undefined') {
                    toastr.info(notification.message, notification.title, {
                        timeOut: 0, 
                        extendedTimeOut: 0,
                        closeButton: true,
                        onclick: function () {
                            window.location.href = `/AdminRequests?requestId=${notification.relatedEntityId}`;
                        }
                    });
                } else {
                    if (typeof snackbar !== 'undefined') {
                        snackbar.info(`${notification.title}: ${notification.message}`);
                    }
                }

                if (window.location.pathname.toLowerCase().includes('/adminrequests')) {
                    setTimeout(() => location.reload(), 1000);
                }
            }
            break;

        case "TaskAssigned":
        case "TaskCompleted":
            if (isProjectManager) {
                if (typeof toastr !== 'undefined') {
                    toastr.info(notification.message, notification.title, {
                        timeOut: 5000,
                        onclick: function () {
                            if (notification.relatedEntityId) {
                                window.location.href = `/Tasks/Details/${notification.relatedEntityId}`;
                            }
                        }
                    });
                }

                if (window.location.pathname.toLowerCase().includes('/tasks')) {
                    setTimeout(() => location.reload(), 1000);
                }
            }
            break;

        case "RequestApproved":
            if (window.location.pathname.toLowerCase().includes('/myrequests') &&
                notification.relatedEntityId) {
                const requestRow = $(`tr[data-request-id="${notification.relatedEntityId}"]`);
                if (requestRow.length) {
                    requestRow.find('td:nth-child(3) .badge')
                        .removeClass('bg-warning text-dark')
                        .addClass('bg-success')
                        .text('Approved');

                    setTimeout(() => location.reload(), 3000);
                }
            }
            break;

        case "RequestRejected":
            if (window.location.pathname.toLowerCase().includes('/myrequests') &&
                notification.relatedEntityId) {
                const requestRow = $(`tr[data-request-id="${notification.relatedEntityId}"]`);
                if (requestRow.length) {
                    requestRow.find('td:nth-child(3) .badge')
                        .removeClass('bg-warning text-dark')
                        .addClass('bg-danger')
                        .text('Rejected');

                    setTimeout(() => location.reload(), 3000);
                }
            }
            break;

        default:
            if (typeof snackbar !== 'undefined') {
                snackbar.info(notification.title);
            }
            break;
    }
}

function startNotificationPolling() {
    loadNotificationCount();
    setInterval(loadNotificationCount, 20000);
}

function prependNewNotification(notification) {
    if (typeof formatNotificationTime !== 'function') {
        window.formatNotificationTime = function (date) {
            const now = new Date();
            const diffMs = now - date;
            const diffMins = Math.floor(diffMs / 60000);

            if (diffMins < 1) return 'Just now';
            if (diffMins < 60) return `${diffMins}m ago`;

            const diffHours = Math.floor(diffMins / 60);
            if (diffHours < 24) return `${diffHours}h ago`;

            const diffDays = Math.floor(diffHours / 24);
            if (diffDays < 7) return `${diffDays}d ago`;

            return date.toLocaleDateString();
        };
    }

    if (typeof getNotificationIcon !== 'function') {
        window.getNotificationIcon = function (type) {
            switch (type) {
                case 'ProjectJoinRequest': return 'bi-person-plus';
                case 'RequestApproved': return 'bi-check-circle';
                case 'RequestRejected': return 'bi-x-circle';
                case 'TaskAssigned': return 'bi-list-task';
                case 'TaskCompleted': return 'bi-check2-all';
                default: return 'bi-bell';
            }
        };
    }

    if (typeof getNotificationColorClass !== 'function') {
        window.getNotificationColorClass = function (type) {
            switch (type) {
                case 'ProjectJoinRequest': return 'bg-primary text-white';
                case 'RequestApproved': return 'bg-success text-white';
                case 'RequestRejected': return 'bg-danger text-white';
                case 'TaskAssigned': return 'bg-info text-white';
                case 'TaskCompleted': return 'bg-success text-white';
                default: return 'bg-secondary text-white';
            }
        };
    }

    const date = new Date(notification.createdAt);
    const formattedDate = formatNotificationTime(date);
    const iconClass = getNotificationIcon(notification.type);
    const colorClass = getNotificationColorClass(notification.type);

    let actionButton = '';
    if (notification.type === 'ProjectJoinRequest' && notification.relatedEntityId) {
        actionButton = `
        <a href="/AdminRequests?requestId=${notification.relatedEntityId}"
            class="btn btn-sm btn-primary rounded-3 px-3 mt-2">
            Review Request
        </a>
        `;
    } else if ((notification.type === 'RequestApproved' || notification.type === 'RequestRejected') &&
        notification.relatedEntityId) {
        actionButton = `
        <a href="/MyRequests"
            class="btn btn-sm btn-outline-primary rounded-3 px-3 mt-2">
            View Requests
        </a>
        `;
    }

    const notificationHtml = `
    <div class="notification-item p-3 border-bottom position-relative unread"
         data-notification-id="${notification.id}">
        <div class="d-flex">
            <div class="notification-icon me-3">
                <div class="icon-circle ${colorClass}">
                    <i class="bi ${iconClass}"></i>
                </div>
            </div>
            <div class="notification-content flex-grow-1">
                <div class="d-flex justify-content-between align-items-start">
                    <h6 class="mb-1 fw-medium">${notification.title}</h6>
                    <small class="text-muted ms-2">${formattedDate}</small>
                </div>
                <p class="text-muted small mb-2">${notification.message}</p>
                <div class="d-flex justify-content-between align-items-center">
                    ${actionButton}
                    <button class="btn btn-sm btn-link p-0 text-muted small" onclick="markAsRead('${notification.id}')">
                        <i class="bi bi-check-circle me-1"></i>Mark as read
                    </button>
                </div>
            </div>
        </div>
    </div>`;

    if ($('#notificationsContainer .empty-state').length) {
        $('#notificationsContainer').empty();
    }

    $('#notificationsContainer').prepend(notificationHtml);
}

$(document).ready(function () {
    const userId = $('#currentUserId').val();
    const isAdminRaw = $('#isAdmin').val();
    console.log("Init - User ID:", userId);
    console.log("Init - Is Admin (raw):", isAdminRaw);
    console.log("Init - Is Admin (check):", isAdminRaw === 'true');
    console.log("Init - isAdmin element exists:", $('#isAdmin').length > 0);
    initializeNotificationConnection();
});