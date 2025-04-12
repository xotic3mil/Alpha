// Time & Task Global Navigation Functions

// Open most recent project with time tab selected
function openRecentTimeEntries() {
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

// Open time entry modal for most recent project
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

// Open most recent project with tasks tab selected
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

// Open create task modal for most recent project
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

// Helper function to open project details with specific tab
function openProjectDetailsWithTab(projectId, tabId) {
    loadProjectDetails(projectId, function () {
        $(`#${tabId}`).tab('show');
    });
}

// Helper function to show alert when no project is available
function showNoProjectAlert() {
    alert('You need to be assigned to at least one project to use this feature. Please join or create a project first.');
}