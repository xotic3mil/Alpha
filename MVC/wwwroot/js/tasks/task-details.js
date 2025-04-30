function editTask(taskId) {
    $.ajax({
        url: `/ProjectTask/GetTaskById?id=${taskId}`,
        type: 'GET',
        success: function (response) {
            console.log("Edit task response:", response);

            if (!response.succeeded) {
                alert('Failed to load task details: ' + (response.error || 'Unknown error'));
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
                $('#editTaskDueDate').val(formatDateForInput(dueDate));
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