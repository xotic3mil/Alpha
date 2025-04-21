/**
 * Task management functionality
 */

function loadProjectTasks(projectId) {
    if (!projectId) {
        console.error("Missing project ID when loading tasks");
        return;
    }

    $('#projectTasksTable').html(`
        <tr>
            <td colspan="6" class="text-center py-3">
                <div class="spinner-border spinner-border-sm text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="text-muted mt-2">Loading tasks...</p>
            </td>
        </tr>
    `);

    $.ajax({
        url: `/ProjectTask/GetTasksByProject?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
    
            if (!response.success || !response.tasks || response.tasks.length === 0) {
                $('#projectTasksTable').html(`
                    <tr>
                        <td colspan="6" class="text-center py-3">
                            <p class="text-muted mb-0">No tasks found for this project</p>
                        </td>
                    </tr>
                `);
                return;
            }

            let html = '';
            response.tasks.forEach(task => {
                html += formatTaskListItem(task);
            });

            $('#projectTasksTable').html(html);
        },
        error: function (error) {
            console.error('Error loading tasks:', error);
            $('#projectTasksTable').html(`
                <tr>
                    <td colspan="6" class="text-center py-3">
                        <p class="text-danger mb-0">Error loading tasks</p>
                    </td>
                </tr>
            `);
        }
    });
}

function formatTaskListItem(task) {
    const priorityClass = getPriorityClass(task.priority);
    const status = getTaskStatusDetails(task.isCompleted);
    const dueDate = task.dueDate ? formatDateString(task.dueDate) : 'No due date';
    let assigneeName = 'Unassigned';
    if (task.assignedTo) {
        assigneeName = task.assignedTo.name || task.assignedTo.userName || task.assignedTo.email || 'Unassigned';
    }

    return `
        <tr>
            <td>
                <div class="d-flex align-items-center">
                    <div class="form-check me-2">
                        <input class="form-check-input" type="checkbox" 
                            ${task.isCompleted ? 'checked' : ''} 
                            onclick="toggleTaskCompletion('${task.id}', '${task.projectId}'); return false;" 
                            id="taskComplete_${task.id}">
                    </div>
                    <div>
                        <div class="fw-medium">${task.title}</div>
                        <div class="small text-muted">${task.description || 'No description'}</div>
                    </div>
                </div>
            </td>
            <td>${assigneeName}</td>
            <td>${dueDate}</td>
            <td><span class="badge ${priorityClass}">${task.priority}</span></td>
            <td><span class="badge ${status.class}">${status.text}</span></td>
            <td>
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary" onclick="editTask('${task.id}')" data-project-id="${task.projectId}">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteTask('${task.id}', '${task.projectId}')" data-project-id="${task.projectId}">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </td>
        </tr>
    `;
}


function loadTaskSummary(projectId) {
    if (!projectId) return;

    $.ajax({
        url: `/ProjectTask/GetTaskSummary?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
 
            if (!response.success) {
                console.error("Error loading task summary:", response.message);
                return;
            }

            const totalTasks = response.totalTasks || 0;
            const completedTasks = response.completedTasks || 0;
            const percentage = response.completionPercentage || 0;

            $('#taskProgressBar').css('width', `${percentage}%`).attr('aria-valuenow', percentage);
            $('#taskCompletionText').text(`${completedTasks} of ${totalTasks} tasks completed`);
            $('#taskCompletionPercentage').text(`${percentage}%`);
        },
        error: function (error) {
            console.error('Error loading task summary:', error);
        }
    });
}

function getPriorityClass(priority) {
    switch (priority) {
        case 'Low': return 'bg-secondary';
        case 'Medium': return 'bg-primary';
        case 'High': return 'bg-warning';
        case 'Urgent': return 'bg-danger';
        default: return 'bg-secondary';
    }
}

function getProjectId() {
    let projectId = $('#projectDetailId').val();

    if (!projectId || projectId === '') {
        projectId = $('#taskProjectId').val();
    }

    if (!projectId || projectId === '') {
        projectId = $('#editTaskProjectId').val();
    }

    if (!projectId || projectId === '') {
        projectId = sessionStorage.getItem('currentProjectId');
    }
    return projectId;
}

function openCreateTaskModal() {
    const projectId = getProjectId();

    if (!projectId || projectId === '') {
        console.error("Missing project ID:", projectId);
        snackbar.error("Error: Project ID is missing. Please try again.");
        return;
    }

    console.log("Creating task for project:", projectId);

    $('#taskProjectId').val(projectId);

    $('#createTaskForm')[0].reset();

    if ($('#taskDueDate').length) {
        $('#taskDueDate').val('');
    }

    loadTaskAssignees(projectId, 'taskAssignee');
    $('#createTaskModal').modal('show');
}

function loadTaskAssignees(projectId, dropdownId) {
    $(`#${dropdownId}`).html('<option value="">Loading users...</option>');

    $.ajax({
        url: `/ProjectMembership/GetProjectMembers?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            console.log("Assignee data:", response);

            if (!response.success || !response.members || response.members.length === 0) {
                $(`#${dropdownId}`).html('<option value="">No team members available</option>');
                return;
            }

            let options = '<option value="">Unassigned</option>';
            response.members.forEach(member => {
                const userName = member.userName || member.name || 'Unknown User';
                const userId = member.userId || member.id;

                if (userId) {
                    options += `<option value="${userId}">${userName}</option>`;
                }
            });

            $(`#${dropdownId}`).html(options);
        },
        error: function (error) {
            console.error('Error loading team members:', error);
            $(`#${dropdownId}`).html('<option value="">Error loading users</option>');
        }
    });
}

function toggleTaskCompletion(taskId, taskProjectId) {
    event.preventDefault();

    console.log("Toggling task completion for task:", taskId);

    const projectId = taskProjectId || getProjectId();

    if (!projectId) {
        console.error("Missing project ID when toggling task completion");
        snackbar.error("Error: Project ID is missing");
        return;
    }

    sessionStorage.setItem('currentProjectId', projectId);

    $.ajax({
        url: '/ProjectTask/Complete',
        type: 'POST',
        data: {
            id: taskId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
        },
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (response) {
            console.log("Task completion toggle response:", response);

            if (!response.succeeded) {
                console.error("Failed to update task status:", response.error);
                snackbar.error('Failed to update task status: ' + (response.error || 'Unknown error'));
                return;
            }

            console.log("Refreshing tasks after status change with project ID:", projectId);

            refreshSingleProjectCard(projectId);

            setTimeout(() => {
                loadProjectTasks(projectId);
                loadTaskSummary(projectId);
            }, 300);

            console.log("Task status updated successfully");
        },
        error: function (xhr, status, error) {
            console.error('Error updating task completion status:', error);
            console.error('Response text:', xhr.responseText);
            snackbar.error('Failed to update task status. Please try again.');
        }
    });
}

function getTaskStatusDetails(isCompleted) {
    return {
        text: isCompleted ? 'Completed' : 'Open',
        class: isCompleted ? 'bg-success' : 'bg-secondary'
    };
}

function editTask(taskId) {
    $.ajax({
        url: `/ProjectTask/GetTaskById?id=${taskId}`,
        type: 'GET',
        success: function (response) {
            if (!response.succeeded) {
                snackbar.error('Failed to load task details.');
                return;
            }

            const task = response.result;

            sessionStorage.setItem('currentProjectId', task.projectId);

            $('#editTaskId').val(task.id);
            $('#editTaskProjectId').val(task.projectId);
            $('#editTaskTitle').val(task.title);
            $('#editTaskDescription').val(task.description);
            $('#editTaskPriority').val(task.priority);
            $('#editEstimatedHours').val(task.estimatedHours);
            $('#editTaskIsCompleted').prop('checked', task.isCompleted);

            if (task.dueDate) {
                const dueDate = new Date(task.dueDate);
                $('#editTaskDueDate').val(dueDate.toISOString().split('T')[0]);
            } else {
                $('#editTaskDueDate').val('');
            }

            loadTaskAssignees(task.projectId, 'editTaskAssignee');
            refreshSingleProjectCard(task.projectId);

            setTimeout(() => {
                if (task.assignedToId) {
                    $('#editTaskAssignee').val(task.assignedToId);
                } else {
                    $('#editTaskAssignee').val('');
                }
                $('#editTaskModal').modal('show');
            }, 500);
        },
        error: function (error) {
            console.error('Error loading task details:', error);
            snackbar.error('Failed to load task details. Please try again.');
        }
    });
}

function deleteTask(taskId, entryProjectId) {
    if (!confirm('Are you sure you want to delete this task?')) return;

    let projectId = entryProjectId;

    if (!projectId) {
        projectId = getProjectId();
    }

    if (!projectId) {
        console.error("Cannot delete: Missing project ID");
        snackbar.error("Error: Project ID is missing");
        return;
    }

    sessionStorage.setItem('currentProjectId', projectId);

    $.ajax({
        url: '/ProjectTask/Delete',
        type: 'POST',
        data: {
            id: taskId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
        },
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (response) {
            if (!response.succeeded) {
                snackbar.error('Failed to delete task: ' + (response.error || 'Unknown error'));
                return;
            }

            console.log("Task deleted successfully, refreshing with project ID:", projectId);

            setTimeout(() => {
                loadProjectTasks(projectId);
                loadTaskSummary(projectId);
            }, 300);

            refreshSingleProjectCard(projectId);
            snackbar.success('Task deleted successfully');
        },
        error: function (xhr, status, error) {
            console.error('Error deleting task:', error);
            console.error('Response text:', xhr.responseText);
            snackbar.error('Failed to delete task. Please try again.');
        }
    });
}

function saveTask() {
    const form = $('#createTaskForm');

    if (form.data('submitting')) {
        console.log("Form already submitting, ignoring duplicate request");
        return;
    }

    if (!form[0].checkValidity()) {
        form[0].reportValidity();
        return;
    }

    let projectId = $('#taskProjectId').val();
    if (!projectId || projectId === '') {
        projectId = getProjectId();
        $('#taskProjectId').val(projectId);
    }

    if (!projectId || projectId === '') {
        console.error("ProjectId is missing");
        snackbar.error("Error: Project ID is missing");
        return;
    }

    sessionStorage.setItem('currentProjectId', projectId);

    form.data('submitting', true);

    const token = $('input[name="__RequestVerificationToken"]').first().val();
    const formData = new FormData(form[0]);

    console.log("Form data being submitted:");
    for (let pair of formData.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
    }

    $.ajax({
        url: '/ProjectTask/Create',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (response) {
            form.data('submitting', false);

            console.log("Task creation response:", response);

            if (!response.succeeded) {
                console.error("Error creating task:", response.error);
                snackbar.error('Failed to create task: ' + (response.error || 'Unknown error'));
                return;
            }

            $('#createTaskModal').modal('hide');
            form[0].reset();
            refreshSingleProjectCard(projectId);
            
            console.log("Task created successfully, refreshing with project ID:", projectId);

            setTimeout(() => {
                loadProjectTasks(projectId);
                loadTaskSummary(projectId);
            }, 300);

            snackbar.success('Task created successfully');
        },
        error: function (xhr, status, error) {
            form.data('submitting', false);

            console.error('Error creating task:', xhr.responseText);
            try {
                const response = JSON.parse(xhr.responseText);
                snackbar.error('Failed to create task: ' + (response.error || error));
            } catch (e) {
                snackbar.error('Failed to create task. Please try again.');
            }
        }
    });
}

$(document).ready(function () {

    $(document).off('click', '#addTaskBtn').on('click', '#addTaskBtn', function (e) {
        e.preventDefault();
        openCreateTaskModal();
    });

    $('#tasks-tab').on('shown.bs.tab', function (e) {
        const projectId = $('#projectDetailId').val();
        if (projectId) {
            loadProjectTasks(projectId);
            loadTaskSummary(projectId);
        }
    });

    $('#saveTaskBtn').on('click', function () {
        saveTask();
    });

    $('#updateTaskBtn').on('click', function () {
        const form = $('#editTaskForm');
        if (!form[0].checkValidity()) {
            form[0].reportValidity();
            return;
        }

        const projectId = $('#editTaskProjectId').val() || getProjectId();

        if (!projectId) {
            console.error("Missing project ID when updating task");
            snackbar.error("Error: Project ID is missing");
            return;
        }

        sessionStorage.setItem('currentProjectId', projectId);

        const formData = new FormData(form[0]);

        $.ajax({
            url: '/ProjectTask/Edit',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
               
                if (!response.succeeded) {
                    snackbar.error('Failed to update task: ' + (response.error || 'Unknown error'));
                    return;
                }

                const projectId = $('#projectDetailId').val();
                $('#editTaskModal').modal('hide');
                setTimeout(() => {
                loadProjectTasks(projectId);
                    loadTaskSummary(projectId);
                }, 300);

                snackbar.success('Task updated successfully');
            },
            error: function (error) {
                console.error('Error updating task:', error);
                snackbar.error('Failed to update task. Please try again.');
            }
        });
    });

    $('#createTaskModal, #editTaskModal').on('hidden.bs.modal', function () {
        const projectId = getProjectId();
        console.log("Task modal closed, checking if refresh needed with project ID:", projectId);

        if (projectId) {
            setTimeout(() => {
                loadProjectTasks(projectId);
                loadTaskSummary(projectId);
                
            }, 300);
        }
    });
});