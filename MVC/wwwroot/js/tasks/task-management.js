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
            console.log("Tasks response:", response);

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
                            onclick="toggleTaskCompletion('${task.id}'); return false;" 
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
                    <button class="btn btn-sm btn-outline-primary" onclick="editTask('${task.id}')">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteTask('${task.id}')">
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
            console.log("Task summary:", response);

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

function openCreateTaskModal() {
    const projectId = $('#projectDetailId').val();
    console.log("Opening create task modal for project:", projectId);

    if (!projectId) {
        console.error("Missing project ID when opening task modal");
        alert("Error: Project ID is missing. Please try again.");
        return;
    }

    $('#taskProjectId').val(projectId);

    $('#createTaskForm')[0].reset();
    $('#taskDueDate').val('');

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

function toggleTaskCompletion(taskId) {

    event.preventDefault();

    console.log("Toggling task completion for task:", taskId);

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
                alert('Failed to update task status: ' + (response.error || 'Unknown error'));
                return;
            }

            const projectId = $('#projectDetailId').val();

            // Reload the tasks and summary
            loadProjectTasks(projectId);
            loadTaskSummary(projectId);

            console.log("Task status updated successfully");
        },
        error: function (xhr, status, error) {
            console.error('Error updating task completion status:', error);
            console.error('Response text:', xhr.responseText);
            alert('Failed to update task status. Please try again.');
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
                alert('Failed to load task details.');
                return;
            }

            const task = response.result;
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
            alert('Failed to load task details. Please try again.');
        }
    });
}

function deleteTask(taskId) {
    if (!confirm('Are you sure you want to delete this task?')) return;

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
                alert('Failed to delete task: ' + (response.error || 'Unknown error'));
                return;
            }

            const projectId = $('#projectDetailId').val();
            loadProjectTasks(projectId);
            loadTaskSummary(projectId);

            if (typeof toastr !== 'undefined') {
                toastr.success('Task deleted successfully');
            } else {
                alert('Task deleted successfully');
            }
        },
        error: function (error) {
            console.error('Error deleting task:', error);
            alert('Failed to delete task. Please try again.');
        }
    });
}

$(document).ready(function () {
    console.log("Task management initialized");

    $(document).on('click', '#addTaskBtn', function (e) {
        e.preventDefault();
        console.log("Add task button clicked");
        openCreateTaskModal();
    });

    $('#tasks-tab').on('shown.bs.tab', function (e) {
        const projectId = $('#projectDetailId').val();
        console.log("Tasks tab shown, loading tasks for project:", projectId);
        if (projectId) {
            loadProjectTasks(projectId);
            loadTaskSummary(projectId);
        }
    });

    $('#saveTaskBtn').on('click', function () {
        console.log("Save task button clicked");
        const form = $('#createTaskForm');
        if (!form[0].checkValidity()) {
            form[0].reportValidity();
            return;
        }

        if (!$('#taskProjectId').val()) {
            console.error("ProjectId is missing");
            alert("Error: Project ID is missing");
            return;
        }

        const formData = new FormData(form[0]);
        formData.append('CreatedAt', new Date().toISOString());

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
                if (!response.succeeded) {
                    alert('Failed to create task: ' + response.error);
                    return;
                }

                const projectId = $('#projectDetailId').val();
                $('#createTaskModal').modal('hide');
                loadProjectTasks(projectId);
                loadTaskSummary(projectId);
            },
            error: function (xhr, status, error) {
                console.error("Error creating task:");
                console.error("Status:", status);
                console.error("Error:", error);
                console.error("Response:", xhr.responseText);
                alert('Failed to create task. Check the console for details.');
            }
        });
    });

    $('#updateTaskBtn').on('click', function () {
        const form = $('#editTaskForm');
        if (!form[0].checkValidity()) {
            form[0].reportValidity();
            return;
        }

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
                console.log("Update task response:", response);

                if (!response.succeeded) {
                    alert('Failed to update task: ' + (response.error || 'Unknown error'));
                    return;
                }

                const projectId = $('#projectDetailId').val();
                $('#editTaskModal').modal('hide');
                loadProjectTasks(projectId);
                loadTaskSummary(projectId);

                if (typeof toastr !== 'undefined') {
                    toastr.success('Task updated successfully');
                } else {
                    alert('Task updated successfully');
                }
            },
            error: function (error) {
                console.error('Error updating task:', error);
                alert('Failed to update task. Please try again.');
            }
        });
    });


    function saveTask() {
        const form = $('#createTaskForm');
        console.log("Save task clicked");

        if (!form[0].checkValidity()) {
            form[0].reportValidity();
            return;
        }

        if (!$('#taskProjectId').val()) {
            console.error("ProjectId is missing");
            alert("Error: Project ID is missing");
            return;
        }
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
                console.log("Task creation response:", response);

                if (!response.succeeded) {
                    console.error("Error creating task:", response.error);
                    alert('Failed to create task: ' + (response.error || 'Unknown error'));
                    return;
                }

                const projectId = $('#projectDetailId').val();
                $('#createTaskModal').modal('hide');

                loadProjectTasks(projectId);
                loadTaskSummary(projectId);
                alert('Task created successfully');
            },
            error: function (xhr, status, error) {
                console.error('Error creating task:', xhr.responseText);
                try {
                    const response = JSON.parse(xhr.responseText);
                    alert('Failed to create task: ' + (response.error || error));
                } catch (e) {
                    alert('Failed to create task. Please try again.');
                }
            }
        });
    }

    function updateTask() {
        const form = $('#editTaskForm');

        if (!form[0].checkValidity()) {
            form[0].reportValidity();
            return;
        }

        const formData = new FormData(form[0]);

        const dueDate = $('#editTaskDueDate').val();
        if (dueDate) {
            formData.delete('DueDate');
            formData.append('DueDate', new Date(dueDate + 'T00:00:00Z').toISOString());
        }

        if ($('#editTaskIsCompleted').prop('checked')) {
            formData.append('CompletedAt', new Date().toISOString());
        }

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
                console.log("Task update response:", response);

                if (!response.succeeded) {
                    alert('Failed to update task: ' + (response.error || 'Unknown error'));
                    return;
                }

                const projectId = $('#projectDetailId').val();
                $('#editTaskModal').modal('hide');
                loadProjectTasks(projectId);
                loadTaskSummary(projectId);

                alert('Task updated successfully');
            },
            error: function (error) {
                console.error('Error updating task:', error);
                alert('Failed to update task. Please try again.');
            }
        });
    }



});