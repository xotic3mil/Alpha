
let currentProjectId = null;
let isSubmitting = false;
let taskModalMode = 'create';


$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    currentProjectId = urlParams.get('projectId') || $('#tasksTable').data('project-id');


    loadTasks();
    loadUserProjects();
    setupEventListeners();
});

function setupEventListeners() {
    $('#createTaskBtn').on('click', function () {
        openTaskModal('create');
    });

    $('#saveTaskBtn').on('click', function () {
        if (taskModalMode === 'create') {
            createTask();
        } else {
            updateTask();
        }
    });

    $('.btn-group button[data-filter]').on('click', function () {
        $('.btn-group button').removeClass('active');
        $(this).addClass('active');
        applyTaskFilters();
    });

    $('#taskSearch').on('keyup', function () {
        applyTaskFilters();
    });

    $('#taskProject').on('change', function () {
        const projectId = $(this).val();
        if (projectId) {
            loadTaskAssignees(projectId);
        }
    });
}

function loadTasks() {
    $('#tasksTable').html(`
        <tr>
            <td colspan="7" class="text-center py-4">
                <div class="spinner-border spinner-border-sm text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="text-muted mt-2">Loading tasks...</p>
            </td>
        </tr>
    `);

    const url = currentProjectId
        ? `/ProjectTask/GetTasksByProject?projectId=${currentProjectId}`
        : '/ProjectTask/GetAllUserTasks';

    $.ajax({
        url: url,
        type: 'GET',
        success: function (response) {

            if (!response.success || !response.tasks || response.tasks.length === 0) {
                $('#tasksTable').html(`
                    <tr>
                        <td colspan="7" class="text-center py-4">
                            <p class="text-muted mb-0">No tasks found</p>
                        </td>
                    </tr>
                `);
                return;
            }

            let html = '';
            response.tasks.forEach(task => {
                html += formatTaskListItem(task);
            });

            $('#tasksTable').html(html);
            $('.btn-group button[data-filter="all"]').addClass('active');
        },
        error: function (error) {
            console.error('Error loading tasks:', error);
            $('#tasksTable').html(`
                <tr>
                    <td colspan="7" class="text-center py-4">
                        <p class="text-danger mb-0">Error loading tasks</p>
                    </td>
                </tr>
            `);
        }
    });
}

function loadUserProjects() {
    $.ajax({
        url: '/ProjectTask/GetUserProjects',
        type: 'GET',
        success: function (response) {

            if (!response.success || !response.projects || response.projects.length === 0) {
                $('#projectFilterList').html('<li><span class="dropdown-item">No projects found</span></li>');
                $('#taskProject').html('<option value="" disabled selected>No projects available</option>');
                return;
            }
            let filterHtml = '<li><a class="dropdown-item" href="/ProjectTask/Tasks">All Projects</a></li>';

            let selectHtml = '<option value="" disabled selected>Select project...</option>';

            response.projects.forEach(project => {
                filterHtml += `<li><a class="dropdown-item" href="/ProjectTask/Tasks?projectId=${project.id}">${project.name}</a></li>`;
                selectHtml += `<option value="${project.id}">${project.name}</option>`;
            });

            $('#projectFilterList').html(filterHtml);
            $('#taskProject').html(selectHtml);

            if (currentProjectId) {
                $('#taskProject').val(currentProjectId);
            }
        },
        error: function (error) {
            console.error('Error loading projects:', error);
            $('#projectFilterList').html('<li><span class="dropdown-item text-danger">Error loading projects</span></li>');
        }
    });
}

function loadTaskAssignees(projectId) {
    $('#taskAssignee').html('<option value="">Loading users...</option>');

    $.ajax({
        url: `/ProjectMembership/GetProjectMembers?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            if (!response.success || !response.members || response.members.length === 0) {
                $('#taskAssignee').html('<option value="">No team members available</option>');
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

            $('#taskAssignee').html(options);
        },
        error: function (error) {
            console.error('Error loading team members:', error);
            $('#taskAssignee').html('<option value="">Error loading users</option>');
        }
    });
}

function openTaskModal(mode, taskId = null) {
    taskModalMode = mode;

    $('#taskForm')[0].reset();

    if (mode === 'create') {
        $('#taskModalLabel').text('Create New Task');
        $('#saveTaskBtn').text('Create Task');
        $('#taskId').val('');
        $('#taskIsCompleted').prop('disabled', false);

        if (currentProjectId) {
            $('#taskProject').val(currentProjectId);
            loadTaskAssignees(currentProjectId);
        }

        $('#taskModal').modal('show');
    } else {
        $('#taskModalLabel').text('Edit Task');
        $('#saveTaskBtn').text('Update Task');

        loadTaskDetails(taskId);
    }
}

function loadTaskDetails(taskId) {
    $.ajax({
        url: `/ProjectTask/GetTaskById?id=${taskId}`,
        type: 'GET',
        success: function (response) {
            if (!response.succeeded) {
                alert('Failed to load task details: ' + (response.error || 'Unknown error'));
                return;
            }

            const task = response.result;
            $('#taskId').val(task.id);
            $('#taskProjectId').val(task.projectId);
            $('#taskTitle').val(task.title);
            $('#taskDescription').val(task.description);
            $('#taskPriority').val(task.priority);
            $('#estimatedHours').val(task.estimatedHours);
            $('#taskIsCompleted').prop('checked', task.isCompleted);

            if (task.dueDate) {
                const dueDate = new Date(task.dueDate);
                $('#taskDueDate').val(dueDate.toISOString().split('T')[0]);
            } else {
                $('#taskDueDate').val('');
            }

            $('#taskProject').val(task.projectId);
            loadTaskAssignees(task.projectId);

            setTimeout(() => {
                if (task.assignedToId) {
                    $('#taskAssignee').val(task.assignedToId);
                } else {
                    $('#taskAssignee').val('');
                }

                $('#taskModal').modal('show');
            }, 500);
        },
        error: function (error) {
            console.error('Error loading task details:', error);
            alert('Failed to load task details. Please try again.');
        }
    });
}

function createTask() {
    if (isSubmitting) return;

    const form = $('#taskForm');
    if (!form[0].checkValidity()) {
        form[0].reportValidity();
        return;
    }

    isSubmitting = true;

    const projectId = $('#taskProject').val();
    if (!projectId) {
        alert("Please select a project.");
        isSubmitting = false;
        return;
    }

    const formData = new FormData(form[0]);

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
            isSubmitting = false;

            if (!response.succeeded) {
                alert('Failed to create task: ' + (response.error || 'Unknown error'));
                return;
            }

            $('#taskModal').modal('hide');
            loadTasks();

            if (typeof toastr !== 'undefined') {
                toastr.success('Task created successfully');
            } else {
                alert('Task created successfully');
            }
        },
        error: function (xhr, status, error) {
            isSubmitting = false;
            console.error('Error creating task:', error);
            console.error('Response:', xhr.responseText);

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
    if (isSubmitting) return;

    const form = $('#taskForm');
    if (!form[0].checkValidity()) {
        form[0].reportValidity();
        return;
    }

    isSubmitting = true;

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
            isSubmitting = false;

            if (!response.succeeded) {
                alert('Failed to update task: ' + (response.error || 'Unknown error'));
                return;
            }

            $('#taskModal').modal('hide');
            loadTasks();

            if (typeof toastr !== 'undefined') {
                toastr.success('Task updated successfully');
            } else {
                alert('Task updated successfully');
            }
        },
        error: function (xhr, status, error) {
            isSubmitting = false;
            console.error('Error updating task:', error);
            console.error('Response:', xhr.responseText);

            try {
                const response = JSON.parse(xhr.responseText);
                alert('Failed to update task: ' + (response.error || error));
            } catch (e) {
                alert('Failed to update task. Please try again.');
            }
        }
    });
}

function toggleTaskCompletion(taskId) {
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
            if (!response.succeeded) {
                alert('Failed to update task status: ' + (response.error || 'Unknown error'));
                return;
            }

            loadTasks();
        },
        error: function (error) {
            console.error('Error updating task status:', error);
            alert('Failed to update task status. Please try again.');
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

            loadTasks();

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

function formatTaskListItem(task) {
    const priorityClass = getPriorityClass(task.priority);
    const status = getTaskStatusDetails(task.isCompleted);
    const dueDate = task.dueDate ? formatDateString(task.dueDate) : 'No due date';
    const assigneeName = task.assignedTo || 'Unassigned';
    const projectName = task.projectName || 'Unknown Project';

    return `
        <tr class="task-item ${task.isCompleted ? 'completed-task' : 'open-task'}">
            <td>
                <div class="d-flex align-items-center">
                    <div class="form-check me-2">
                        <input class="form-check-input" type="checkbox" 
                            ${task.isCompleted ? 'checked' : ''} 
                            onclick="toggleTaskCompletion('${task.id}')" 
                            id="taskComplete_${task.id}">
                    </div>
                    <div>
                        <div class="fw-medium ${task.isCompleted ? 'text-decoration-line-through text-muted' : ''}">${task.title}</div>
                        <div class="small text-muted">${task.description || 'No description'}</div>
                    </div>
                </div>
            </td>
            <td><span class="badge bg-light text-dark">${projectName}</span></td>
            <td>${assigneeName}</td>
            <td>${dueDate}</td>
            <td><span class="badge ${priorityClass}">${task.priority}</span></td>
            <td><span class="badge ${status.class}">${status.text}</span></td>
            <td>
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary" onclick="openTaskModal('edit', '${task.id}')">
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

function getPriorityClass(priority) {
    switch (priority) {
        case 'Low': return 'bg-secondary';
        case 'Medium': return 'bg-primary';
        case 'High': return 'bg-warning text-dark';
        case 'Urgent': return 'bg-danger';
        default: return 'bg-secondary';
    }
}

function getTaskStatusDetails(isCompleted) {
    return {
        text: isCompleted ? 'Completed' : 'Open',
        class: isCompleted ? 'bg-success' : 'bg-secondary'
    };
}

function formatDateString(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString();
}

function applyTaskFilters() {
    const searchTerm = $('#taskSearch').val().toLowerCase();
    const filter = $('.btn-group button.active').data('filter');

    $('.task-item').each(function () {
        const $row = $(this);
        const title = $row.find('.fw-medium').text().toLowerCase();
        const description = $row.find('.small.text-muted').text().toLowerCase();
        const isCompleted = $row.hasClass('completed-task');

        const matchesSearch = searchTerm === '' ||
            title.includes(searchTerm) ||
            description.includes(searchTerm);

        let matchesStatus = true;
        if (filter === 'open') {
            matchesStatus = !isCompleted;
        } else if (filter === 'completed') {
            matchesStatus = isCompleted;
        }

        if (matchesSearch && matchesStatus) {
            $row.show();
        } else {
            $row.hide();
        }
    });

    if ($('.task-item:visible').length === 0) {
        if ($('#noResultsRow').length === 0) {
            $('#tasksTable').append(`
                <tr id="noResultsRow">
                    <td colspan="7" class="text-center py-4">
                        <p class="text-muted mb-0">No matching tasks found</p>
                    </td>
                </tr>
            `);
        }
    } else {
        $('#noResultsRow').remove();
    }
}