﻿function openRecentTimeEntries() {
    // Get the most recent project the user has access to
    $.ajax({
        url: '/Project/GetMostRecentProject',
        type: 'GET',
        success: function (response) {
            if (response.success && response.projectId) {
                openProjectDetailsWithTab(response.projectId, 'time-tab');
            } else {
                // If no recent project, show a message
                showNoProjectAlert();
            }
        },
        error: function (error) {
            console.error('Error getting recent project:', error);
            showNoProjectAlert();
        }
    });
}

function openLogTime() {
    $.ajax({
        url: '/Project/GetMostRecentProject',
        type: 'GET',
        success: function (response) {
            if (response.success && response.projectId) {
                // First open the project details
                loadProjectDetails(response.projectId, function () {
                    // Then after it's loaded, click the "Log Time" button
                    $('#addTimeEntryBtn').click();
                });
            } else {
                showNoProjectAlert();
            }
        },
        error: function (error) {
            console.error('Error getting recent project:', error);
            showNoProjectAlert();
        }
    });
}

function openMyTasks() {
    $.ajax({
        url: '/Project/GetMostRecentProject',
        type: 'GET',
        success: function (response) {
            if (response.success && response.projectId) {
                openProjectDetailsWithTab(response.projectId, 'tasks-tab');
            } else {
                showNoProjectAlert();
            }
        },
        error: function (error) {
            console.error('Error getting recent project:', error);
            showNoProjectAlert();
        }
    });
}

function openCreateTask() {
    $.ajax({
        url: '/Project/GetMostRecentProject',
        type: 'GET',
        success: function (response) {
            if (response.success && response.projectId) {
                // First open the project details
                loadProjectDetails(response.projectId, function () {
                    // Then switch to the tasks tab
                    $('#tasks-tab').tab('show');

                    // Then click the "New Task" button
                    setTimeout(function () {
                        $('#addTaskBtn').click();
                    }, 300);
                });
            } else {
                showNoProjectAlert();
            }
        },
        error: function (error) {
            console.error('Error getting recent project:', error);
            showNoProjectAlert();
        }
    });
}

function openProjectDetails(id) {
    if (!id) {
        console.error("Missing project ID");
        return;
    }

    $.ajax({
        url: `/Project/GetProjectByIdWithDetails?id=${id}`,
        type: 'GET',
        dataType: 'json',
        success: function (project) {
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

            loadProjectMembers(project.id);
            loadPendingRequests(project.id);
            loadComments(project.id);

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

function formatDateString(dateString) {
    if (!dateString) return null;
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return null;
    return date.toLocaleDateString();
}

function showNoProjectAlert() {
    alert('You need to be assigned to at least one project to use this feature. Please join or create a project first.');
}