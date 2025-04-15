/**
 * Project management functionality
 */

if (typeof commentConnection === 'undefined') {
    var commentConnection = null;
}

function initializeCommentSignalR(projectId) {
    if (typeof signalR === 'undefined') {


        const script = document.createElement('script');
        script.src = "https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js";

        script.onload = function () {
            initializeSignalRConnection(projectId);
        };

        script.onerror = function () {
            snackbar.error("Failed to load SignalR. Real-time comments may not work properly.");
        };
        document.head.appendChild(script);
        return;
    }

    initializeSignalRConnection(projectId);
}

function initializeSignalRConnection(projectId) {
    if (commentConnection) {
        commentConnection.stop();
    }

    commentConnection = new signalR.HubConnectionBuilder()
        .withUrl("/commentHub")
        .withAutomaticReconnect()
        .build();

    commentConnection.on("ReceiveComment", function (comment) {
        appendComment(comment);
    });

    commentConnection.on("CommentDeleted", function (commentId) {

        let commentElement = $(`#comment-${commentId}`);
        if (commentElement.length === 0) {
            commentElement = $(`#${commentId}`);
        }

        if (commentElement.length === 0) {
            loadComments(projectId);
            return;
        }

        commentElement.css('background-color', '#ffcccb');
        commentElement.fadeOut(800, function () {
            $(this).remove();

            if ($('#commentsContainer .comment-item').length === 0) {
                $('#commentsContainer').html('<p class="text-center text-muted">No status updates yet. Add the first comment.</p>');
            }
        });
    });

    commentConnection.start()
        .then(function () {
            return commentConnection.invoke("JoinProjectGroup", projectId);
        })
        .then(function () {
        })
        .catch(function (err) {
        });
}

function formatDateString(dateString) {
    if (!dateString) return null;
    const date = new Date(dateString);
    if (isNaN(date)) return null;
    return date.toLocaleDateString();
}

function createRipple(event) {
    const button = event.currentTarget;
    const ripple = button.querySelector(".ripple");
    if (ripple) {
        ripple.remove();
    }

    const circle = document.createElement("span");
    const diameter = Math.max(button.clientWidth, button.clientHeight);
    const radius = diameter / 2;

    const rect = button.getBoundingClientRect();
    circle.style.width = circle.style.height = `${diameter}px`;
    circle.style.left = `${event.clientX - rect.left - radius}px`;
    circle.style.top = `${event.clientY - rect.top - radius}px`;
    circle.classList.add("ripple");

    button.appendChild(circle);
}

function openProjectDetails(id) {
    if (!id) {
        console.error("Missing project ID");
        return;
    }

    console.log("Opening project details for ID:", id);

    $('#projectDetailId').val(id);
    sessionStorage.setItem('currentProjectId', id);

    $.ajax({
        url: `/Project/GetProjectByIdWithDetails?id=${id}`,
        type: 'GET',
        dataType: 'json',
        success: function (project) {
            $('#projectDetailDeleteId').val(project.id);
            $('#projectDetailId').val(project.id);


            $('#taskProjectId').val(project.id);
            $('#timeEntryProjectId').val(project.id);

            $('#projectDetailImage').attr('src', project.imageUrl || '/images/project-template-1.svg');
            $('#projectDetailName').text(project.name || 'Unnamed Project');
            $('#projectDetailDescription').html(project.description || 'No description available');

            $('#projectDetailServiceName').text(project.service?.serviceName || 'No Service');
            $('#projectDetailServiceDescription').text(project.service?.serviceDescription || 'No description available');
            $('#projectDetailBudget').text(project.service?.budget ? `$${project.service.budget.toLocaleString()}` : 'N/A');

            $('#projectDetailStatus').text(project.status?.statusName || 'No Status');
            const statusColor = project.status?.colorCode || '#4caf50';
            $('#projectDetailStatusIndicator').css('background-color', statusColor);
            $('#projectDetailHeader').css('background-color', statusColor + '15');

            $('#projectDetailStartDate').text(formatDateString(project.startDate) || 'Not set');
            $('#projectDetailEndDate').text(formatDateString(project.endDate) || 'Not set');

            if (project.endDate) {
                const endDate = new Date(project.endDate);
                const now = new Date();
                const daysLeft = Math.ceil((endDate - now) / (1000 * 60 * 60 * 24));
                $('#projectDetailTimeRemaining').text(daysLeft > 0 ? `${daysLeft} days left` : 'Overdue');
            } else {
                $('#projectDetailTimeRemaining').text('-');
            }

            if (project.customer) {
                $('#projectDetailCustomerName').text(project.customer.companyName || '-');
                $('#projectDetailCustomerContact').text(project.customer.contactName || '-');
                $('#projectDetailCustomerEmail').text(project.customer.email || '-');
                $('#projectDetailCustomerPhone').text(project.customer.phoneNumber || '-');

                let address = [];
                if (project.customer.address) address.push(project.customer.address);
                if (project.customer.city) address.push(project.customer.city);
                if (project.customer.country) address.push(project.customer.country);

                $('#projectDetailCustomerAddress').text(address.length > 0 ? address.join(', ') : '-');
                $('#viewCustomerLink').attr('href', `/Customer/Details/${project.customer.id}`);
            }
            const token = $('input[name="__RequestVerificationToken"]').first().val();
            $('#commentForm').html(`
                <input type="hidden" name="__RequestVerificationToken" value="${token}" />
                <input type="hidden" id="commentProjectId" name="ProjectId" value="${project.id}" />
                <div class="input-group">
                    <input type="text"
                           id="commentContent"
                           name="Content"
                           class="form-control formBorder"
                           placeholder="Add a status update comment..."
                           required />
                    <button type="submit" class="btn btn-primary">Post</button>
                </div>
            `);

            loadProjectMembers(project.id);
            loadComments(project.id);
            loadPendingRequests(project.id);

            if (typeof loadProjectTasks === 'function') {
                loadProjectTasks(project.id);
                loadTaskSummary(project.id);
            }

            if (typeof loadTimeEntries === 'function') {
                loadTimeEntries(project.id);
                loadTimeEntrySummary(project.id);
            }

            $('#projectDetailsModal').modal('show');

            $('#projectDetailsModal').on('shown.bs.modal', function (e) {
                setTimeout(() => {
                    initializeCommentSignalR(id);
                }, 500);
            });

        },
        error: function (error) {
            snackbar.error('Failed to load project details. Please try again.');
        }

    });
}

function ensureSignalRLoaded() {
    return new Promise((resolve) => {
        if (typeof signalR !== 'undefined') {
            resolve();
            return;
        }

        const script = document.createElement('script');
        script.src = "https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js";
        script.onload = resolve;
        document.head.appendChild(script);
    });
}

function loadProjectMembers(projectId) {
    $('#projectDetailTeamMembers').html(`
        <div class="text-center py-3">
            <div class="spinner-border spinner-border-sm text-primary" role="status">
                <span class="visually-hidden">Loading team members...</span>
            </div>
            <p class="text-muted mt-2">Loading team members...</p>
        </div>
    `);

    $.ajax({
        url: `/ProjectMembership/GetProjectMembers?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            $('#projectDetailTeamMembers').empty();

            if (!response.success) {
                $('#projectDetailTeamMembers').html('<p class="text-danger">Failed to load team members</p>');
                return;
            }

            if (response.members && response.members.length > 0) {
                response.members.forEach(member => {
                    const userName = member.userName || member.name || member.user?.name || 'Unknown User';
                    const roleName = member.roleName || member.role || 'Team Member';
                    const avatarUrl = member.avatarUrl || member.user?.avatarUrl || '/images/member-template-1.svg';
                    const userId = member.userId || member.id || member.user?.id;

                    const memberHtml = `
                        <div class="d-flex align-items-center justify-content-between py-2">
                            <div class="d-flex align-items-center">
                                <img src="${avatarUrl}" alt="${userName}" class="rounded-circle me-2" width="32" height="32">
                                <div>
                                    <div class="fw-medium">${userName}</div>
                                    <div class="small text-muted">${roleName}</div>
                                </div>
                            </div>
                            ${response.canManageMembers ? `
                                <button class="btn btn-sm btn-outline-danger" 
                                       onclick="removeTeamMember('${projectId}', '${userId}')">
                                    <i class="bi bi-person-x"></i>
                                </button>
                            ` : ''}
                        </div>
                    `;
                    $('#projectDetailTeamMembers').append(memberHtml);
                });
            } else {
                $('#projectDetailTeamMembers').html('<p class="text-muted">No team members yet.</p>');
            }
        },
        error: function (xhr, status, error) {
            console.error("Error loading team members:", error);
            console.error("Response:", xhr.responseText);
            $('#projectDetailTeamMembers').html('<p class="text-danger">Failed to load team members</p>');
        }
    });
}

function loadPendingRequests(projectId) {
    $.ajax({
        url: `/ProjectMembership/GetPendingRequests?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            if (!response.success) {
                return;
            }

            if (!response.requests || response.requests.length === 0) {
                $('#pendingRequestsSection').hide();
                return;
            }

            $('#pendingRequests').empty();
            $('#pendingRequestsSection').show();

            response.requests.forEach(request => {
                const requestHtml = `
                    <div class="d-flex align-items-center justify-content-between py-2 border-bottom">
                        <div class="d-flex align-items-center">
                            <img src="${request.userAvatarUrl || '/images/member-template-1.svg'}" alt="${request.userName}" class="rounded-circle me-2" width="32" height="32">
                            <div>
                                <div class="fw-medium">${request.userName}</div>
                                <div class="small text-muted">Requested ${new Date(request.requestedAt).toLocaleDateString()}</div>
                            </div>
                        </div>
                        <div>
                            <button class="btn btn-sm btn-outline-success me-1" onclick="approveRequest('${request.id}')">
                                <i class="bi bi-check"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-danger" onclick="rejectRequest('${request.id}')">
                                <i class="bi bi-x"></i>
                            </button>
                        </div>
                    </div>
                `;
                $('#pendingRequests').append(requestHtml);
            });
        },
        error: function (error) {
            console.error("Error loading pending requests:", error);
        }
    });
}

function openAddTeamMemberModal() {
    const projectId = $('#projectDetailId').val();

    if (!projectId) {
        console.error("Project ID not found");
        return;
    }

    $('#addTeamMemberProjectId').val(projectId);
    $('#userSelect').html('<option value="" selected disabled>Loading users...</option>');


    $.ajax({
        url: `/ProjectMembership/GetAvailableUsers?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            $('#userSelect').empty();

            if (!response.success) {
                console.error("Error from API:", response.message || "Unknown error");

                if (response.message && (
                    response.message.includes("No available users found") ||
                    response.message.includes("All users have been assigned")
                )) {
                    $('#userSelect').html('<option value="" selected disabled>All users have been assigned to projects</option>');

                    $('#addTeamMemberButton').prop('disabled', true);
                } else {
                    $('#userSelect').html('<option value="" selected disabled>Error loading users. Please try again.</option>');
                }
                return;
            }

            if (response.users && response.users.length > 0) {
                $('#userSelect').append('<option value="" selected disabled>Select a user to add</option>');
                response.users.forEach(user => {
                    $('#userSelect').append(`<option value="${user.id}">${user.name}</option>`);
                });
                $('#addTeamMemberButton').prop('disabled', false);
            } else {
                $('#userSelect').html('<option value="" selected disabled>All users have been assigned to projects</option>');
                $('#addTeamMemberButton').prop('disabled', true);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error loading users:", error);
            console.error("Response:", xhr.responseText);
            $('#userSelect').html('<option value="" selected disabled>Error loading users. Please try again.</option>');
        },
        complete: function () {
            $('#addTeamMemberModal').modal('show');
        }
    });
}

function addMemberToProject() {
    const projectId = $('#addTeamMemberProjectId').val();
    const userId = $('#userSelect').val();

    if (!userId) {
        snackbar.warning('Please select a user');
        return;
    }

    $.ajax({
        url: '/ProjectMembership/AddUserToProject',
        type: 'POST',
        data: {
            projectId: projectId,
            userId: userId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
        },
        success: function (response) {
            if (response.success) {
                $('#addTeamMemberModal').modal('hide');
                loadProjectMembers(projectId);
                snackbar.success('Team member added successfully');
            } else {
                snackbar.error(response.error || 'Failed to add team member');
            }
        },
        error: function (xhr, status, error) {
            console.error("Error adding team member:", error);
            console.error("Response:", xhr.responseText);
            snackbar.error('Error adding team member');
        }
    });
}

function confirmAction(message, callback) {
    $('#confirmationModalBody').text(message);

    $('#confirmActionBtn').off('click');
    $('#cancelActionBtn').off('click');

    $('#confirmActionBtn').on('click', function () {
        $('#confirmationModal').modal('hide');
        callback(true);
    });

    $('#cancelActionBtn').on('click', function () {
        $('#confirmationModal').modal('hide');
        callback(false);
    });

    $('#confirmationModal').modal('show');
}

function removeTeamMember(projectId, userId) {
    confirmAction('Are you sure you want to remove this team member from the project?', function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/ProjectMembership/RemoveUserFromProject',
            type: 'POST',
            data: {
                projectId: projectId,
                userId: userId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
            },
            success: function (response) {
                if (response.success) {
                    loadProjectMembers(projectId);
                    snackbar.success('Team member removed successfully');
                } else {
                    snackbar.error(response.error || 'Failed to remove team member');
                }
            },
            error: function (error) {
                console.error("Error removing user from project:", error);
                snackbar.error('An error occurred while removing the user');
            }
        });
    });
}

function approveRequest(requestId) {
    confirmAction('Are you sure you want to approve this request?', function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/ProjectMembership/ApproveProjectRequest',
            type: 'POST',
            data: {
                requestId: requestId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
            },
            success: function (response) {
                if (response.success) {
                    const projectId = $('#projectDetailId').val();
                    loadPendingRequests(projectId);
                    loadProjectMembers(projectId);
                    snackbar.success('Request approved successfully');
                } else {
                    snackbar.error(response.error || 'Failed to approve request');
                }
            },
            error: function (error) {
                console.error("Error approving request:", error);
                snackbar.error('An error occurred while approving the request');
            }
        });
    });
}

function rejectRequest(requestId) {
    confirmAction('Are you sure you want to reject this request?', function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/ProjectMembership/RejectProjectRequest',
            type: 'POST',
            data: {
                requestId: requestId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
            },
            success: function (response) {
                if (response.success) {
                    const projectId = $('#projectDetailId').val();
                    loadPendingRequests(projectId);
                    snackbar.success('Request rejected');
                } else {
                    snackbar.error(response.error || 'Failed to reject request');
                }
            },
            error: function (error) {
                console.error("Error rejecting request:", error);
                snackbar.error('An error occurred while rejecting the request');
            }
        });
    });
}

function loadComments(projectId) {
    const commentsContainer = $('#commentsContainer');
    commentsContainer.html(`
        <div class="text-center py-3">
            <div class="spinner-border spinner-border-sm text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="text-muted mt-2">Loading comments...</p>
        </div>
    `);

    $.ajax({
        url: `/Comment/GetProjectComments?projectId=${projectId}`,
        method: 'GET',
        success: function (comments) {
            commentsContainer.empty();

            if (comments.length === 0) {
                commentsContainer.html('<p class="text-center text-muted">No comments yet</p>');
                return;
            }

            comments.forEach(function (comment) {
                const canDelete = comment.canDelete === true;

                const deleteButton = canDelete ?
                    `<button class="btn btn-sm text-danger delete-comment" data-id="${comment.id}" data-project-id="${projectId}">
                       <i class="bi bi-trash"></i>
                     </button>` : '';

                commentsContainer.append(`
                    <div class="comment-item mb-3" id="comment-${comment.id}">
                        <div class="d-flex justify-content-between">
                            <div class="d-flex">
                                <img src="${comment.userImage}" alt="User" class="rounded-circle me-2" width="32" height="32">
                                <div>
                                    <p class="mb-0 fw-medium">${comment.userName}</p>
                                    <p class="mb-0 text-muted small">${comment.dateFormatted}</p>
                                </div>
                            </div>
                            ${deleteButton}
                        </div>
                        <p class="mt-2 mb-0">${comment.content}</p>
                    </div>
                `);
            });

            $('.delete-comment').click(function () {
                const commentId = $(this).data('id');
                const projectId = $(this).data('project-id');
                deleteComment(commentId, projectId);
            });
        },
        error: function (xhr) {
            commentsContainer.html(`
                <div class="alert alert-danger">
                    Failed to load comments. <a href="#" onclick="loadProjectComments('${projectId}'); return false;">Try again</a>
                </div>
            `);
        }
    });
}

function appendComment(comment) {

    if ($('#commentsContainer .text-center.text-muted').length > 0) {
        $('#commentsContainer').empty();
    }

    let displayDate = comment.dateFormatted || 'Just now';

    if (comment.createdAt && !comment.dateFormatted) {
        try {
            const date = new Date(comment.createdAt);
            if (!isNaN(date.getTime())) {
                displayDate = date.toLocaleString();
            }
        } catch (e) {
            console.error("Error formatting date:", e);
        }
    }

    const commentHtml = `
        <div class="comment-item mb-3" id="comment-${comment.id}">
            <div class="d-flex justify-content-between">
                <div class="d-flex">
                    <img src="${comment.userImage || '/images/default-avatar.svg'}" 
                         alt="User" class="rounded-circle me-2" width="32" height="32">
                    <div>
                        <p class="mb-0 fw-medium">${comment.userName}</p>
                        <p class="mb-0 text-muted small">${displayDate}</p>
                    </div>
                </div>
                ${comment.canDelete ? `
                <button class="btn btn-sm text-danger delete-comment" data-id="${comment.id}" data-project-id="${comment.projectId || $('#commentProjectId').val()}">
                   <i class="bi bi-trash"></i>
                </button>` : ''}
            </div>
            <p class="mt-2 mb-0">${comment.content}</p>
        </div>
    `;

    $('#commentsContainer').prepend(commentHtml);

    if (comment.canDelete) {
        $(`#comment-${comment.id} .delete-comment`).click(function () {
            const commentId = $(this).data('id');
            const projectId = $(this).data('project-id');
            deleteComment(commentId, projectId);
        });
    }
}

function deleteComment(commentId, projectId) {
    confirmAction("Delete this status comment?", function (confirmed) {
        if (!confirmed) return;

        const token = $('input[name="__RequestVerificationToken"]').first().val();

        $.ajax({
            url: `/Comment/Delete?id=${commentId}&projectId=${projectId}`,
            type: 'POST',
            data: {
                __RequestVerificationToken: token
            },
            success: function (response) {
                $(`#comment-${commentId}`).slideUp(200, function () {
                    $(this).remove();

                    if ($('#commentsContainer').children().length === 0) {
                        $('#commentsContainer').html('<p class="text-muted text-center">No status updates yet. Add the first comment.</p>');
                    }
                });

                refreshSingleProjectCard(projectId);
            },
            error: function (error) {
                console.error("Error deleting comment:", error);

                if (error.status === 403) {
                    snackbar.error("You don't have permission to delete this comment.");
                } else if (error.status === 401) {
                    snackbar.error("Your session has expired. Please log in again.");
                    setTimeout(() => {
                        window.location.href = "/Auth/Login";
                    }, 2000);
                } else {
                    snackbar.error("Failed to delete comment. Please try again.");
                }
            }
        });
    });
}

function refreshSingleProjectCard(projectId) {
    $.ajax({
        url: `/Project/GetSingleProjectCardPartial?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            const cardElement = $(`div.card[onclick*="${projectId}"]`).closest('.col-12');
            if (cardElement.length) {
                cardElement.replaceWith(response);
                console.log('Project card refreshed successfully');
            }
        },
        error: function (error) {
            console.error('Error refreshing project card:', error);
        }
    });
}

$(document).off('submit', '#commentForm').on('submit', '#commentForm', function (e) {
    e.preventDefault();

    const projectId = $('#commentProjectId').val();
    const content = $('#commentContent').val().trim();

    if (!content) {
        return;
    }

    if (!projectId) {
        snackbar.error("Error: Cannot add comment without project ID. Please try again.");
        return;
    }

    $.ajax({
        url: '/Comment/Create',
        type: 'POST',
        data: $(this).serialize(),
        success: function (response) {
            $('#commentContent').val('');

            if ($('#commentsContainer .text-center.text-muted').length > 0) {
                $('#commentsContainer').empty();
            }

            if (response.comment) {
                const comment = response.comment;
                const newCommentHtml = `
                    <div class="comment-item mb-3" id="comment-${comment.id}">
                        <div class="d-flex justify-content-between">
                            <div class="d-flex">
                                <img src="${comment.userImage || '/images/default-avatar.svg'}" 
                                     alt="User" class="rounded-circle me-2" width="32" height="32">
                                <div>
                                    <p class="mb-0 fw-medium">${comment.userName}</p>
                                    <p class="mb-0 text-muted small">${comment.dateFormatted || 'Just now'}</p>
                                </div>
                            </div>
                            <button class="btn btn-sm text-danger delete-comment" data-id="${comment.id}" data-project-id="${projectId}">
                               <i class="bi bi-trash"></i>
                            </button>
                        </div>
                        <p class="mt-2 mb-0">${comment.content}</p>
                    </div>
                `;
                $('#commentsContainer').prepend(newCommentHtml);

                $(`#comment-${comment.id} .delete-comment`).click(function () {
                    const commentId = $(this).data('id');
                    const projectId = $(this).data('project-id');
                    deleteComment(commentId, projectId);
                });
                refreshSingleProjectCard(projectId);
            } else {
                loadComments(projectId);

                refreshSingleProjectCard(projectId);
            }
        },
        error: function (error) {
            snackbar.error("Failed to add comment. Please try again.");
        }
    });
});

$('#projectDetailsModal').on('hidden.bs.modal', function () {
    if (commentConnection) {
        const projectId = $('#commentProjectId').val();
        if (projectId) {
            commentConnection.invoke("LeaveProjectGroup", projectId)
        }
        commentConnection.stop();
    }
});


$(document).ready(function () {
    if ($('#confirmationModal').length === 0) {
        $('body').append(`
            <div class="modal fade" id="confirmationModal" tabindex="-1" aria-labelledby="confirmationModalLabel" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="confirmationModalLabel">Confirm Action</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body" id="confirmationModalBody">
                            Are you sure you want to proceed?
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" id="cancelActionBtn">Cancel</button>
                            <button type="button" class="btn btn-primary" id="confirmActionBtn">Confirm</button>
                        </div>
                    </div>
                </div>
            </div>
        `);
    }
});