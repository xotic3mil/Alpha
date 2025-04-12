/**
 * Time Entry Management functionality
 * Handles all time entry related operations for projects
 */

// Prevent multiple form submissions
let isSubmitting = false;

function loadProjectTimeEntries(projectId) {
    if (!projectId) {
        console.error("Missing project ID when loading time entries");
        return;
    }

    $('#projectTimeEntriesTable').html(`
        <tr>
            <td colspan="7" class="text-center py-3">
                <div class="spinner-border spinner-border-sm text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="text-muted mt-2">Loading time entries...</p>
            </td>
        </tr>
    `);

    $.ajax({
        url: `/TimeEntry/GetTimeEntriesByProject?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            console.log("Time entries response:", response);

            if (!response.success || !response.timeEntries || response.timeEntries.length === 0) {
                $('#projectTimeEntriesTable').html(`
                    <tr>
                        <td colspan="7" class="text-center py-3">
                            <p class="text-muted mb-0">No time entries found for this project</p>
                        </td>
                    </tr>
                `);
                return;
            }

            let html = '';
            response.timeEntries.forEach(entry => {
                html += formatTimeEntryListItem(entry);
            });

            $('#projectTimeEntriesTable').html(html);
        },
        error: function (error) {
            console.error('Error loading time entries:', error);
            snackbar.error('Failed to load time entries');
            $('#projectTimeEntriesTable').html(`
                <tr>
                    <td colspan="7" class="text-center py-3">
                        <p class="text-danger mb-0">Error loading time entries</p>
                    </td>
                </tr>
            `);
        }
    });
}

function formatTimeEntryListItem(entry) {
    const billableClass = entry.isBillable ? 'text-success' : 'text-muted';
    const billableText = entry.isBillable ? `$${entry.hourlyRate.toFixed(2)}/hr` : 'No';

    return `
        <tr>
            <td>${entry.date}</td>
            <td>${entry.userName || 'Unknown'}</td>
            <td>${entry.taskTitle || 'No Task'}</td>
            <td>${entry.hours.toFixed(2)}</td>
            <td>${entry.description || ''}</td>
            <td><span class="${billableClass}">${billableText}</span></td>
            <td>
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary" onclick="editTimeEntry('${entry.id}')">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteTimeEntry('${entry.id}')">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </td>
        </tr>
    `;
}

function loadTimeEntrySummary(projectId) {
    if (!projectId) return;

    $.ajax({
        url: `/TimeEntry/GetTimeEntrySummary?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            console.log("Time entry summary:", response);

            if (!response.success) {
                snackbar.error("Error loading time entry summary");
                console.error("Error loading time entry summary:", response.message);
                return;
            }

            const totalHours = response.totalHours || 0;
            const billableHours = response.billableHours || 0;
            const billableAmount = response.totalBillableAmount || 0;

            $('#timeEntryTotalHours').text(totalHours.toFixed(2));
            $('#timeEntryBillableHours').text(billableHours.toFixed(2));
            $('#timeEntryBillableAmount').text('$' + billableAmount.toFixed(2));
        },
        error: function (error) {
            console.error('Error loading time entry summary:', error);
            snackbar.error('Failed to load time entry summary');
        }
    });
}

function openCreateTimeEntryModal() {
    const projectId = $('#projectDetailId').val();
    console.log("Opening time entry modal for project:", projectId);

    if (!projectId) {
        console.error("Missing project ID");
        snackbar.error("Error: Project ID is missing");
        return;
    }

    // Reset form and clear any previous ID
    $('#createTimeEntryForm')[0].reset();

    if ($('#timeEntryId').length) {
        $('#timeEntryId').val('');
    }

    $('#timeEntryProjectId').val(projectId);

    // Set default values
    $('#timeEntryDate').val(new Date().toISOString().split('T')[0]);
    $('#timeEntryHours').val(1);
    $('#timeEntryIsBillable').prop('checked', true);
    $('#timeEntryRate').prop('disabled', false);

    // Reset modal UI
    $('#createTimeEntryModalLabel').text('Log Time');
    $('#saveTimeEntryBtn').text('Save Time Entry');

    // Load tasks for this project
    loadProjectTasksForSelect(projectId, 'timeEntryTask');

    // Show the modal
    $('#createTimeEntryModal').modal('show');
}

function loadProjectTasksForSelect(projectId, selectId) {
    $(`#${selectId}`).html('<option value="">Loading tasks...</option>');

    $.ajax({
        url: `/ProjectTask/GetTasksByProject?projectId=${projectId}`,
        type: 'GET',
        success: function (response) {
            console.log("Tasks for select:", response);

            if (!response.success || !response.tasks || response.tasks.length === 0) {
                $(`#${selectId}`).html('<option value="">No tasks available</option>');
                return;
            }

            let options = '<option value="">None (general project work)</option>';
            response.tasks.forEach(task => {
                options += `<option value="${task.id}">${task.title}</option>`;
            });

            $(`#${selectId}`).html(options);
        },
        error: function (error) {
            console.error('Error loading tasks for select:', error);
            $(`#${selectId}`).html('<option value="">Error loading tasks</option>');
            snackbar.error('Failed to load project tasks');
        }
    });
}

function editTimeEntry(timeEntryId) {
    console.log("Editing time entry:", timeEntryId);

    // Reset form first
    $('#createTimeEntryForm')[0].reset();

    $.ajax({
        url: `/TimeEntry/GetTimeEntryById?id=${timeEntryId}`,
        type: 'GET',
        success: function (response) {
            console.log("Time entry data:", response);

            if (!response.succeeded) {
                snackbar.error('Failed to load time entry: ' + (response.error || 'Unknown error'));
                return;
            }

            const timeEntry = response.result;

            // Add or set the ID field
            if (!$('#timeEntryId').length) {
                $('#createTimeEntryForm').prepend('<input type="hidden" id="timeEntryId" name="Id">');
            }

            $('#timeEntryId').val(timeEntryId);
            $('#timeEntryProjectId').val(timeEntry.projectId);

            // Format the date for the form
            const entryDate = new Date(timeEntry.date);
            const formattedDate = entryDate.toISOString().split('T')[0];
            $('#timeEntryDate').val(formattedDate);

            $('#timeEntryHours').val(timeEntry.hours);
            $('#timeEntryDescription').val(timeEntry.description);
            $('#timeEntryIsBillable').prop('checked', timeEntry.isBillable);
            $('#timeEntryRate').val(timeEntry.hourlyRate);

            // Enable/disable rate field based on billable status
            $('#timeEntryRate').prop('disabled', !timeEntry.isBillable);

            // Load tasks for this project
            loadProjectTasksForSelect(timeEntry.projectId, 'timeEntryTask');

            // Update the modal UI to show we're editing
            $('#createTimeEntryModalLabel').text('Edit Time Entry');
            $('#saveTimeEntryBtn').text('Update Time Entry');

            // Wait for tasks to load before setting selected task
            setTimeout(() => {
                if (timeEntry.taskId) {
                    $('#timeEntryTask').val(timeEntry.taskId);
                } else {
                    $('#timeEntryTask').val('');
                }

                // Show the modal
                $('#createTimeEntryModal').modal('show');
            }, 500);
        },
        error: function (xhr, status, error) {
            console.error('Error loading time entry:', error);
            console.error('Response:', xhr.responseText);
            snackbar.error('Failed to load time entry details');
        }
    });
}

function createTimeEntry() {
    // Prevent double submission
    if (isSubmitting) {
        console.log("Form submission already in progress");
        return;
    }

    const form = $('#createTimeEntryForm');

    // Validate the form
    if (!form[0].checkValidity()) {
        form[0].reportValidity();
        return;
    }

    // Make sure we have a project ID
    const projectId = $('#timeEntryProjectId').val();
    if (!projectId) {
        snackbar.error("Error: Project ID is missing");
        return;
    }

    // Make sure hours is valid
    const hours = $('#timeEntryHours').val();
    if (!hours || parseFloat(hours) <= 0) {
        snackbar.error("Error: Hours must be greater than 0");
        $('#timeEntryHours').focus();
        return;
    }

    // Make sure description is provided
    const description = $('#timeEntryDescription').val().trim();
    if (!description) {
        snackbar.error("Error: Description is required");
        $('#timeEntryDescription').focus();
        return;
    }

    // Make sure date is valid
    const date = $('#timeEntryDate').val();
    if (!date) {
        snackbar.error("Error: Date is required");
        $('#timeEntryDate').focus();
        return;
    }

    // Make sure all required fields have values
    const taskId = $('#timeEntryTask').val() || null; // This can be optional

    isSubmitting = true;

    // Create form data from the form
    const formData = new FormData(form[0]);

    formData.delete('Id');

    // Handle the boolean value explicitly
    formData.delete('IsBillable');
    formData.append('IsBillable', $('#timeEntryIsBillable').prop('checked').toString());


    // Set hourly rate based on billable status
    if ($('#timeEntryIsBillable').prop('checked')) {
        const rate = $('#timeEntryRate').val() || "0";
        formData.set('HourlyRate', rate);
    } else {
        formData.set('HourlyRate', "0");
    }

    // If TaskId is empty string, set it to null
    if (formData.get('TaskId') === '') {
        formData.delete('TaskId');
    }

    console.log("Creating new time entry");

    // Log form data for debugging
    console.log("Form data being submitted:");
    for (let pair of formData.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
    }

    $.ajax({
        url: '/TimeEntry/CreateAjax',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (response) {
            isSubmitting = false;
            console.log("Time entry creation response:", response);

            if (!response.succeeded) {
                let errorMessage = response.error || 'Unknown error';
                if (response.details && response.details.length > 0) {
                    errorMessage += ': ' + response.details.join(', ');
                }
                snackbar.error(`Failed to create time entry: ${errorMessage}`);
                return;
            }

            // Hide the modal
            $('#createTimeEntryModal').modal('hide');

            // Reset the form for next use
            $('#createTimeEntryForm')[0].reset();

            // Refresh the data
            loadProjectTimeEntries(projectId);
            loadTimeEntrySummary(projectId);

            // Show success message
            snackbar.success('Time entry created successfully');
        },
        error: function (xhr, status, error) {
            isSubmitting = false;
            console.error('Error creating time entry:');
            console.error('Status:', status);
            console.error('Error:', error);
            console.error('Response:', xhr.responseText);

            try {
                const response = JSON.parse(xhr.responseText);
                const errorMessages = response.details ? response.details.join(', ') : response.error;
                snackbar.error('Failed to create time entry: ' + errorMessages);
            } catch (e) {
                snackbar.error('Failed to create time entry. Please try again.');
            }
        }
    });
}

function updateTimeEntry() {
    // Prevent double submission
    if (isSubmitting) {
        console.log("Form submission already in progress");
        return;
    }

    const form = $('#createTimeEntryForm');

    // Validate the form
    if (!form[0].checkValidity()) {
        form[0].reportValidity();
        return;
    }

    // Check if we have an ID to update
    const timeEntryId = $('#timeEntryId').val();
    if (!timeEntryId) {
        snackbar.error("Error: Time entry ID is missing");
        return;
    }

    // Make sure we have a project ID
    const projectId = $('#timeEntryProjectId').val();
    if (!projectId) {
        snackbar.error("Error: Project ID is missing");
        return;
    }

    isSubmitting = true;

    // Create form data from the form
    const formData = new FormData(form[0]);

    // Handle the boolean value properly
    formData.delete('IsBillable');
    formData.append('IsBillable', $('#timeEntryIsBillable').prop('checked'));

    console.log("Updating time entry:", timeEntryId);

    // Log form data for debugging
    console.log("Form data being submitted:");
    for (let pair of formData.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
    }

    $.ajax({
        url: '/TimeEntry/UpdateAjax',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (response) {
            isSubmitting = false;
            console.log("Time entry update response:", response);

            if (!response.succeeded) {
                let errorMessage = response.error || 'Unknown error';
                if (response.details && response.details.length > 0) {
                    errorMessage += ': ' + response.details.join(', ');
                }
                snackbar.error(`Failed to update time entry: ${errorMessage}`);
                return;
            }

            // Hide the modal
            $('#createTimeEntryModal').modal('hide');

            // Reset the form for next use
            $('#createTimeEntryForm')[0].reset();
            $('#createTimeEntryModalLabel').text('Log Time');
            $('#saveTimeEntryBtn').text('Save Time Entry');

            // Clear ID field for next creation
            $('#timeEntryId').val('');

            // Refresh the data
            loadProjectTimeEntries(projectId);
            loadTimeEntrySummary(projectId);

            // Show success message
            snackbar.success('Time entry updated successfully');
        },
        error: function (xhr, status, error) {
            isSubmitting = false;
            console.error('Error updating time entry:');
            console.error('Status:', status);
            console.error('Error:', error);
            console.error('Response:', xhr.responseText);

            try {
                const response = JSON.parse(xhr.responseText);
                const errorMessages = response.details ? response.details.join(', ') : response.error;
                snackbar.error('Failed to update time entry: ' + errorMessages);
            } catch (e) {
                snackbar.error('Failed to update time entry. Please try again.');
            }
        }
    });
}

function deleteTimeEntry(timeEntryId) {
    if (!confirm('Are you sure you want to delete this time entry?')) {
        return;
    }

    $.ajax({
        url: '/TimeEntry/DeleteAjax',
        type: 'POST',
        data: {
            id: timeEntryId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
        },
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (response) {
            if (!response.succeeded) {
                snackbar.error('Failed to delete time entry: ' + (response.error || 'Unknown error'));
                return;
            }

            const projectId = $('#projectDetailId').val();
            loadProjectTimeEntries(projectId);
            loadTimeEntrySummary(projectId);

            snackbar.success('Time entry deleted successfully');
        },
        error: function (xhr, status, error) {
            console.error('Error deleting time entry:', error);
            console.error('Response:', xhr.responseText);
            snackbar.error('Failed to delete time entry. Please try again.');
        }
    });
}

$(document).ready(function () {
    console.log("Time entry management script loaded");

    // Add time entry button
    $(document).on('click', '#addTimeEntryBtn', function (e) {
        e.preventDefault();
        console.log("Add time entry button clicked");
        openCreateTimeEntryModal();
    });

    // Handle billable checkbox
    $(document).on('change', '#timeEntryIsBillable', function () {
        if ($(this).prop('checked')) {
            $('#timeEntryRate').prop('disabled', false);
        } else {
            $('#timeEntryRate').prop('disabled', true);
        }
    });

    // Save time entry button with conditional function call
    $(document).on('click', '#saveTimeEntryBtn', function (e) {
        e.preventDefault();
        const timeEntryId = $('#timeEntryId').val();
        const isEdit = timeEntryId && timeEntryId !== '';

        console.log("Save time entry button clicked, isEdit:", isEdit);
        if (isEdit) {
            updateTimeEntry();
        } else {
            createTimeEntry();
        }
    });

    // Load time entries when tab is shown
    $('#time-tab').on('shown.bs.tab', function (e) {
        const projectId = $('#projectDetailId').val();
        console.log("Time tab shown, loading time entries for project:", projectId);
        if (projectId) {
            loadProjectTimeEntries(projectId);
            loadTimeEntrySummary(projectId);
        }
    });
});