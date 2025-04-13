/**
 * Project management functionality
 */

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
    console.log("Opening project details for ID:", id);

    $.ajax({
        url: `/Project/GetProjectByIdWithDetails?id=${id}`,
        type: 'GET',
        dataType: 'json',
        success: function (project) {
            console.log("Project data:", project);

            $('#projectDetailDeleteId').val(project.id);
            $('#projectDetailId').val(project.id);

            $('#projectDetailImage').attr('src', project.imageUrl || '/images/project-template-1.svg');
            $('#projectDetailName').text(project.name || 'Unnamed Project');
            $('#projectDetailDescription').text(project.description || 'No description available');

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

        },
        error: function (error) {
            console.error('Error fetching project details:', error);
            alert('Failed to load project details. Please try again.');
        }
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
            console.log("Team members response:", response);
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

    console.log("Opening team member modal for project:", projectId);

    $.ajax({
        url: `/ProjectMembership/GetAvailableUsers?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            console.log("API response:", response);
            $('#userSelect').empty();

            if (!response.success) {
                console.error("Error from API:", response.message || "Unknown error");

                // Check if the error is specifically about no available users
                if (response.message && (
                    response.message.includes("No available users found") ||
                    response.message.includes("All users have been assigned")
                )) {
                    // Show user-friendly message
                    $('#userSelect').html('<option value="" selected disabled>All users have been assigned to projects</option>');

                    // Disable the add button since there's no one to add
                    $('#addTeamMemberButton').prop('disabled', true);
                } else {
                    // For other errors, show generic error message
                    $('#userSelect').html('<option value="" selected disabled>Error loading users. Please try again.</option>');
                }
                return;
            }

            if (response.users && response.users.length > 0) {
                $('#userSelect').append('<option value="" selected disabled>Select a user to add</option>');
                response.users.forEach(user => {
                    $('#userSelect').append(`<option value="${user.id}">${user.name}</option>`);
                });
                // Enable the add button
                $('#addTeamMemberButton').prop('disabled', false);
            } else {
                // No users available (but success=true case)
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
            // Ensure the modal shows regardless of success or error
            $('#addTeamMemberModal').modal('show');
        }
    });
}

function addMemberToProject() {
    const projectId = $('#addTeamMemberProjectId').val();
    const userId = $('#userSelect').val();

    if (!userId) {
        alert('Please select a user');
        return;
    }

    console.log("Adding user", userId, "to project", projectId);

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
                if (typeof toastr !== 'undefined') {
                    toastr.success('Team member added successfully');
                } else {
                    alert('Team member added successfully');
                }
            } else {
                if (typeof toastr !== 'undefined') {
                    toastr.error(response.error || 'Failed to add team member');
                } else {
                    alert(response.error || 'Failed to add team member');
                }
            }
        },
        error: function (xhr, status, error) {
            console.error("Error adding team member:", error);
            console.error("Response:", xhr.responseText);
            if (typeof toastr !== 'undefined') {
                toastr.error('Error adding team member');
            } else {
                alert('Error adding team member');
            }
        }
    });
}

function removeTeamMember(projectId, userId) {
    if (!confirm('Are you sure you want to remove this team member from the project?')) {
        return;
    }

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
                if (typeof toastr !== 'undefined') {
                    toastr.success('Team member removed successfully');
                } else {
                    alert('Team member removed successfully');
                }
            } else {
                if (typeof toastr !== 'undefined') {
                    toastr.error(response.error || 'Failed to remove team member');
                } else {
                    alert(response.error || 'Failed to remove team member');
                }
            }
        },
        error: function (error) {
            console.error("Error removing user from project:", error);
            if (typeof toastr !== 'undefined') {
                toastr.error('An error occurred while removing the user');
            } else {
                alert('An error occurred while removing the user');
            }
        }
    });
}

function approveRequest(requestId) {
    if (!confirm('Are you sure you want to approve this request?')) {
        return;
    }

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
                if (typeof toastr !== 'undefined') {
                    toastr.success('Request approved successfully');
                } else {
                    alert('Request approved successfully');
                }
            } else {
                if (typeof toastr !== 'undefined') {
                    toastr.error(response.error || 'Failed to approve request');
                } else {
                    alert(response.error || 'Failed to approve request');
                }
            }
        },
        error: function (error) {
            console.error("Error approving request:", error);
            alert('An error occurred while approving the request');
        }
    });
}

function rejectRequest(requestId) {
    if (!confirm('Are you sure you want to reject this request?')) {
        return;
    }

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
                if (typeof toastr !== 'undefined') {
                    toastr.success('Request rejected');
                } else {
                    alert('Request rejected');
                }
            } else {
                if (typeof toastr !== 'undefined') {
                    toastr.error(response.error || 'Failed to reject request');
                } else {
                    alert(response.error || 'Failed to reject request');
                }
            }
        },
        error: function (error) {
            console.error("Error rejecting request:", error);
            alert('An error occurred while rejecting the request');
        }
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
                console.log(`Comment ${comment.id} canDelete:`, canDelete);

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
            console.error('Failed to load comments', xhr);
            commentsContainer.html(`
                <div class="alert alert-danger">
                    Failed to load comments. <a href="#" onclick="loadProjectComments('${projectId}'); return false;">Try again</a>
                </div>
            `);
        }
    });
}

function appendComment(comment) {
    const commentDate = new Date(comment.createdAt);
    const formattedDate = commentDate.toLocaleString();

    const commentHtml = `
        <div class="comment ${comment.isCurrentUser ? 'current-user' : ''}" data-comment-id="${comment.id}">
            <div class="comment-header">
                <div class="comment-user">
                    <img src="${comment.authorAvatarUrl || '/images/member-template-1.svg'}" alt="${comment.authorName}" class="comment-avatar">
                    <div>
                        <p class="comment-username">${comment.userName}</p>
                        <p class="comment-time">${formattedDate}</p>
                    </div>
                </div>
                ${comment.isCurrentUser ? `
                <div class="comment-actions">
                    <button class="btn btn-sm" onclick="deleteComment('${comment.id}', '${comment.projectId}')">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
                ` : ''}
            </div>
            <div class="comment-content">${comment.content}</div>
        </div>
    `;

    $('#commentsContainer').prepend(commentHtml);
}

function deleteComment(commentId, projectId) {
    if (!confirm("Delete this status comment?")) {
        return;
    }

    const token = $('input[name="__RequestVerificationToken"]').first().val();

    $.ajax({
        url: `/Comment/Delete?id=${commentId}&projectId=${projectId}`,
        type: 'POST',
        data: {
            __RequestVerificationToken: token
        },
        success: function (response) {
            // Updated selector to match the HTML structure
            $(`#comment-${commentId}`).slideUp(200, function () {
                $(this).remove();

                if ($('#commentsContainer').children().length === 0) {
                    $('#commentsContainer').html('<p class="text-muted text-center">No status updates yet. Add the first comment.</p>');
                }
            });
        },
        error: function (error) {
            console.error("Error deleting comment:", error);

            if (error.status === 403) {
                alert("You don't have permission to delete this comment.");
            } else if (error.status === 401) {
                alert("Your session has expired. Please log in again.");
                window.location.href = "/Auth/Login";
            } else {
                alert("Failed to delete comment. Please try again.");
            }
        }
    });
}


$(document).off('submit', '#commentForm').on('submit', '#commentForm', function (e) {
    e.preventDefault();

    const projectId = $('#commentProjectId').val();
    const content = $('#commentContent').val().trim();

    console.log("Submitting comment for project:", projectId);
    console.log("Comment content:", content);

    if (!content) {
        console.log("Empty comment, not submitting");
        return;
    }

    if (!projectId) {
        console.log("Missing project ID, cannot submit");
        alert("Error: Cannot add comment without project ID. Please try again.");
        return;
    }

    $.ajax({
        url: '/Comment/Create',
        type: 'POST',
        data: $(this).serialize(),
        success: function (response) {
            console.log("Comment created:", response);
            $('#commentContent').val('');

            // Clear "No comments" message if it exists
            if ($('#commentsContainer .text-center.text-muted').length > 0) {
                $('#commentsContainer').empty();
            }

            // If response contains the new comment data, add it to the DOM
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

                // Add event handler for delete button
                $(`#comment-${comment.id} .delete-comment`).click(function () {
                    const commentId = $(this).data('id');
                    const projectId = $(this).data('project-id');
                    deleteComment(commentId, projectId);
                });
            } else {
                // If response doesn't contain the comment data, reload all comments
                loadComments(projectId);
            }
        },
        error: function (error) {
            console.error("Error submitting comment:", error);
            alert("Failed to add comment. Please try again.");
        }
    });
});