let currentTaskId = null;
let currentTaskIdForAssignment = null;
let currentModalTaskId = null;
let attachmentsModalInstance = null;

function showTaskContextMenu(event, taskId) {
    event.preventDefault();
    currentTaskId = taskId;

    const menu = document.getElementById("taskContextMenu");
    menu.style.left = `${event.pageX}px`;
    menu.style.top = `${event.pageY}px`;
    menu.style.display = "block";

    document.addEventListener("click", hideTaskContextMenu);
}

function hideTaskContextMenu() {
    document.getElementById("taskContextMenu").style.display = "none";
    document.removeEventListener("click", hideTaskContextMenu);
}

function openCreateModal(event) {
    if (event) {
        const target = event.target.closest('.task-card');
        if (target) return;
        event.preventDefault();
    }

    let modal = new bootstrap.Modal(document.getElementById('createTaskModal'));
    modal.show();
}

function openCreateModalFromButton() {
    openCreateModal();
}

document.getElementById('createTaskForm').addEventListener('submit', function (e) {
    e.preventDefault();

    const formData = new FormData(this);
    const data = Object.fromEntries(formData.entries());

    fetch('/ProjectTasks/Create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    }).then(response => {
        if (!response.ok) throw new Error("Ошибка при создании задачи");
        location.reload();
    }).catch(err => {
        alert(err.message);
    });
});

document.getElementById('editTaskForm').addEventListener('submit', function (e) {
    e.preventDefault();

    const formData = new FormData(this);
    const data = Object.fromEntries(formData.entries());

    data.ProjectId = document.querySelector('input[name="ProjectId"]').value;

    fetch('/ProjectTasks/Edit', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    }).then(response => {
        if (!response.ok) throw new Error("Ошибка при сохранении задачи");
        location.reload();
    }).catch(err => {
        alert(err.message);
    });
});

function editTask() {
    fetch(`/ProjectTasks/Get/${currentTaskId}`)
        .then(response => {
            if (!response.ok) throw new Error("Не удалось загрузить задачу");
            return response.json();
        })
        .then(task => {
            document.getElementById("editTaskId").value = task.id;
            document.getElementById("editTaskTitle").value = task.title;
            document.getElementById("editTaskDescription").value = task.description;
            document.getElementById("editTaskStatus").value = task.status;
            document.getElementById("editTaskDueDate").value = task.dueDate?.substring(0, 10) ?? "";

            let modal = new bootstrap.Modal(document.getElementById('editTaskModal'));
            modal.show();
        })
        .catch(err => {
            alert(err.message);
        });
}

function confirmDeleteTask() {
    if (!currentTaskId) {
        alert("ID задачи не выбран");
        return;
    }

    const taskCard = document.querySelector(`.task-card[data-task-id='${currentTaskId}']`);
    const taskTitle = taskCard?.querySelector('.card-title')?.textContent ?? "(без названия)";
    document.getElementById("deleteTaskTitle").textContent = taskTitle;

    const modal = new bootstrap.Modal(document.getElementById('deleteTaskModal'));
    modal.show();
}

function deleteTaskConfirmed() {
    if (!currentTaskId) return;

    fetch('/ProjectTasks/Delete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(currentTaskId)
    }).then(response => {
        console.log("Status code:", response.status);

        if (response.status === 403) {
            alert("У вас нет доступа для удаления этой задачи.");
            return;
        }

        if (response.status === 404) {
            alert("Задача не найдена");
            return;
        }

        if (!response.ok) {
            alert("Произошла ошибка при удалении");
            return;
        }

        location.reload();
    });
}

function viewTask() {
    fetch(`/ProjectTasks/Get/${currentTaskId}`)
        .then(response => {
            if (!response.ok) throw new Error("Не удалось загрузить задачу");
            return response.json();
        })
        .then(task => {
            document.getElementById("viewTaskTitle").textContent = task.title;
            document.getElementById("viewTaskDescription").textContent = task.description?.trim() || "(Нет описания)";
            document.getElementById("viewTaskStatus").textContent = getLocalizedStatus(task.status);
            document.getElementById("viewTaskDueDate").textContent = task.dueDate?.substring(0, 10) || "(Без срока)";

            loadModalComments(task.id);

            const modal = new bootstrap.Modal(document.getElementById('viewTaskModal'));
            modal.show();
        })
        .catch(err => {
            alert("Ошибка: " + err.message);
        });
}

function getLocalizedStatus(status) {
    switch (status) {
        case "ToDo": return "📋 К выполнению";
        case "InProgress": return "⏳ В процессе";
        case "Done": return "✅ Готово";
        default: return status;
    }
}

function assignUserToTask() {
    currentTaskIdForAssignment = currentTaskId;
    console.log("Opening assignee modal for task:", currentTaskIdForAssignment);

    const modal = new bootstrap.Modal(document.getElementById('assignAssigneeModal'));
    modal.show();

    loadAvailableMembers();
    loadCurrentAssignees();
}

function loadAvailableMembers() {
    const select = document.getElementById('assigneeSelect');
    select.innerHTML = '<option value="">Загрузка...</option>';

    fetch(`/api/TaskAssignees/AvailableMembers?taskId=${currentTaskIdForAssignment}`)
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            return response.json();
        })
        .then(data => {
            console.log("Available members data:", data);

            select.innerHTML = '<option value="">Выберите исполнителя...</option>';

            if (data.success && data.members && data.members.length > 0) {
                data.members.forEach(member => {
                    const option = document.createElement('option');
                    option.value = member.id;
                    option.textContent = member.userName || `User ${member.id}`;
                    select.appendChild(option);
                });
            } else {
                select.innerHTML = '<option value="">Нет доступных участников</option>';
            }
        })
        .catch(error => {
            console.error('Error loading members:', error);
            select.innerHTML = `<option value="">Ошибка: ${error.message}</option>`;
        });
}

function loadCurrentAssignees() {
    const list = document.getElementById('assigneesList');
    list.innerHTML = '<li class="list-group-item">Загрузка...</li>';

    fetch(`/api/TaskAssignees/List?taskId=${currentTaskIdForAssignment}`)
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            return response.json();
        })
        .then(assignees => {
            console.log("Current assignees:", assignees);

            list.innerHTML = '';

            if (!assignees || assignees.length === 0) {
                list.innerHTML = '<li class="list-group-item">Нет назначенных исполнителей</li>';
                return;
            }

            assignees.forEach(assignee => {
                const item = document.createElement('li');
                item.className = 'list-group-item d-flex justify-content-between align-items-center';

                const userInfo = document.createElement('div');
                userInfo.className = 'd-flex align-items-center';

                const img = document.createElement('img');
                img.src = assignee.avatarUrl || '/images/default-avatar.png';
                img.className = 'rounded-circle me-2';
                img.width = 24;
                img.height = 24;
                img.onerror = function () { this.src = '/images/default-avatar.png'; };

                const nameSpan = document.createElement('span');
                nameSpan.textContent = assignee.userName || `User ${assignee.id}`;

                userInfo.appendChild(img);
                userInfo.appendChild(nameSpan);

                const removeBtn = document.createElement('button');
                removeBtn.className = 'btn btn-sm btn-outline-danger';
                removeBtn.innerHTML = '&times;';
                removeBtn.onclick = () => removeAssignee(assignee.id);

                item.appendChild(userInfo);
                item.appendChild(removeBtn);
                list.appendChild(item);
            });
        })
        .catch(error => {
            console.error('Error loading assignees:', error);
            list.innerHTML = `<li class="list-group-item">Ошибка: ${error.message}</li>`;
        });
}

function removeAssignee(userId) {
    if (!confirm('Удалить этого исполнителя из задачи?')) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch('/api/TaskAssignees/Remove', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({
            taskId: currentTaskIdForAssignment,
            userId: userId
        })
    })
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            return response.json();
        })
        .then(data => {
            if (data.success) {
                loadCurrentAssignees();
                loadAvailableMembers();

                updateTaskCardAssignees(currentTaskIdForAssignment);
            } else {
                alert(data.message || 'Ошибка при удалении исполнителя');
            }
        })
        .catch(error => {
            console.error('Error removing assignee:', error);
            alert('Произошла ошибка при удалении исполнителя: ' + error.message);
        });
}

function updateTaskCardAssignees(taskId) {
    fetch(`/api/TaskAssignees/List?taskId=${taskId}`)
        .then(response => response.json())
        .then(assignees => {
            const taskCard = document.querySelector(`.task-card[data-task-id="${taskId}"]`);
            if (!taskCard) return;

            let assigneesContainer = taskCard.querySelector('.task-assignees');
            if (!assigneesContainer) {
                assigneesContainer = document.createElement('div');
                assigneesContainer.className = 'task-assignees mt-2';
                taskCard.querySelector('.card-body').appendChild(assigneesContainer);
            }

            assigneesContainer.innerHTML = '';

            if (assignees && assignees.length > 0) {
                assignees.forEach(assignee => {
                    const assigneeElement = document.createElement('div');
                    assigneeElement.className = 'd-inline-flex align-items-center me-2 mb-1';

                    assigneeElement.innerHTML = `
                        <img src="${assignee.avatarUrl || '/images/default-avatar.png'}"
                             class="rounded-circle me-1"
                             width="24"
                             height="24"
                             onerror="this.src='/images/default-avatar.png'">
                        <small>${assignee.userName}</small>
                    `;

                    assigneesContainer.appendChild(assigneeElement);
                });
            } else {
                assigneesContainer.innerHTML = '<small class="text-muted">Нет назначенных исполнителей</small>';
            }
        });
}

document.getElementById('assignAssigneeForm').addEventListener('submit', function (e) {
    e.preventDefault();

    const userId = document.getElementById('assigneeSelect').value;
    if (!userId) {
        alert('Пожалуйста, выберите исполнителя');
        return;
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch('/api/TaskAssignees/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({
            taskId: currentTaskIdForAssignment,
            userId: userId
        })
    })
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            return response.json();
        })
        .then(data => {
            console.log("Assignment response:", data);

            if (data.success) {
                loadCurrentAssignees();
                loadAvailableMembers();

                updateTaskCardAssignees(currentTaskIdForAssignment);

                document.getElementById('assigneeSelect').value = '';

                const modal = bootstrap.Modal.getInstance(document.getElementById('assignAssigneeModal'));
                modal.hide();
            } else {
                alert(data.message || 'Ошибка при назначении исполнителя');
            }
        })
        .catch(error => {
            console.error('Error assigning user:', error);
            alert('Произошла ошибка при назначении исполнителя: ' + error.message);
        });
});

function openTaskModal(task) {
    currentModalTaskId = task.id;

    document.getElementById('viewTaskTitle').textContent = task.title;
    document.getElementById('viewTaskDescription').textContent = task.description;
    document.getElementById('viewTaskStatus').textContent = task.status;
    document.getElementById('viewTaskDueDate').textContent = task.dueDate ?? '—';

    loadModalComments(task.id);

    new bootstrap.Modal(document.getElementById('viewTaskModal')).show();
}

document.getElementById('modalCommentForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const content = document.getElementById('modalCommentInput').value.trim();
    if (!content || !currentTaskId) {
        alert('Введите текст комментария');
        return;
    }

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const formData = new FormData();
        formData.append('taskId', currentTaskId);
        formData.append('content', content);

        const response = await fetch('/api/Comments/Add', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        });

        if (response.ok) {
            document.getElementById('modalCommentInput').value = '';
            await loadModalComments(currentTaskId);
        } else {
            const error = await response.text();
            alert(`Ошибка: ${error}`);
        }
    } catch (err) {
        console.error('Ошибка при отправке комментария:', err);
        alert('Произошла ошибка при отправке комментария');
    }
});

function loadModalComments(taskId) {
    const commentsList = document.getElementById("modalCommentsList");
    commentsList.innerHTML = '<div class="text-center py-3"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Загрузка...</span></div></div>';

    fetch(`/api/Comments/List?taskId=${taskId}`)
        .then(async response => {
            if (!response.ok) {
                const error = await response.json().catch(() => null);
                throw new Error(error?.message || 'Не удалось загрузить комментарии');
            }
            return response.json();
        })
        .then(comments => {
            if (!comments || !comments.length) {
                commentsList.innerHTML = '<p class="text-muted">Комментариев пока нет</p>';
                return;
            }

            commentsList.innerHTML = comments.map(comment => `
                <div class="comment-container d-flex mb-3 p-3 rounded bg-light">
                    <img src="${comment.avatarUrl || '/images/default-avatar.png'}"
                         class="avatar rounded-circle me-3 shadow-sm"
                         width="48" height="48"
                         onerror="this.src='/images/default-avatar.png'">
                    <div class="flex-grow-1">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <div>
                                <strong class="me-2">${comment.userName || 'Пользователь'}</strong>
                                <small class="text-muted">${new Date(comment.createdAt).toLocaleString('ru-RU')}</small>
                            </div>
                            ${comment.canDelete ? `
                            <button class="btn btn-sm btn-outline-danger delete-comment"
                                    data-id="${comment.id}"
                                    title="Удалить комментарий">
                                Удалить
                            </button>` : ''}
                        </div>
                        <div class="comment-text ps-2">${comment.content || ''}</div>
                    </div>
                </div>
            `).join('');

            document.querySelectorAll('.delete-comment').forEach(btn => {
                btn.addEventListener('click', handleDeleteComment);
            });
        })
        .catch(error => {
            console.error('Ошибка:', error);
            commentsList.innerHTML = `
            <div class="alert alert-danger">
                ${error.message || 'Произошла ошибка при загрузке комментариев'}
                <button onclick="loadModalComments(${taskId})" class="btn btn-sm btn-link">Попробовать снова</button>
            </div>`;
        });
}

async function handleDeleteComment(e) {
    e.preventDefault();
    const btn = e.currentTarget;
    const commentId = parseInt(btn.dataset.id);
    const taskId = currentModalTaskId;

    if (!confirm('Вы уверены, что хотите удалить комментарий?')) return;

    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span>';

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const response = await fetch(`/api/Comments/Delete?commentId=${commentId}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token,
                'Cache-Control': 'no-cache'
            }
        });

        if (!response.ok) {
            const error = await response.json().catch(() => null);
            throw new Error(error?.message || 'Не удалось удалить комментарий');
        }

        const commentElement = btn.closest('.comment-container');
        commentElement.style.opacity = '0';
        setTimeout(() => {
            commentElement.remove();
            updateEmptyState();
        }, 300);

    } catch (error) {
        console.error('Ошибка удаления:', error);
        showToast(error.message || 'Произошла ошибка при удалении', 'error');
    } finally {
        btn.disabled = false;
        btn.innerHTML = 'Удалить';
    }
}

function updateEmptyState() {
    const commentsList = document.getElementById("modalCommentsList");
    if (commentsList.children.length === 0) {
        commentsList.innerHTML = '<p class="text-muted">Комментариев пока нет</p>';
    }
}

function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `toast show align-items-center text-white bg-${type}`;
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3000);
}

function openAttachmentsModal() {
    if (!currentTaskId) return;

    fetch(`/Attachments/GetAttachments?taskId=${currentTaskId}`)
        .then(res => res.json())
        .then(data => {
            const container = document.getElementById('attachmentsList');
            container.innerHTML = '';

            if (data.length === 0) {
                container.innerHTML = '<p class="text-muted">Вложений пока нет.</p>';
            } else {
                data.forEach(att => {
                    const item = document.createElement('div');
                    item.className = 'd-flex justify-content-between align-items-center border-bottom py-2';
                    item.innerHTML = `
                        <div>
                            <a href="${att.url}" target="_blank">${att.url.split('/').pop()}</a><br>
                            <small class="text-muted">Загружено: ${att.userName} (${att.uploadDate})</small>
                        </div>
                        ${att.canDelete ? `<button class="btn btn-sm btn-danger" onclick="deleteAttachment(${att.id})">Удалить</button>` : ''}
                    `;
                    container.appendChild(item);
                });
            }

            // Проверка прав на загрузку
            fetch(`/Attachments/CanUpload?taskId=${currentTaskId}`)
                .then(res => res.json())
                .then(json => {
                    document.getElementById('uploadForm').style.display = json.canUpload ? 'block' : 'none';
                });

            // Закрытие предыдущего экземпляра (если открыт)
            if (attachmentsModalInstance) {
                attachmentsModalInstance.hide();
            }

            // Открытие нового экземпляра
            const modalElement = document.getElementById('attachmentsModal');
            attachmentsModalInstance = new bootstrap.Modal(modalElement, {
                backdrop: true,
                keyboard: true
            });
            attachmentsModalInstance.show();
        });
}

document.getElementById('uploadForm').addEventListener('submit', function (e) {
    e.preventDefault();
    const formData = new FormData(this);
    fetch(`/Attachments/Upload?taskId=${currentTaskId}`, {
        method: 'POST',
        body: formData
    })
        .then(res => {
            if (!res.ok) throw new Error("Ошибка загрузки");
            return res.json();
        })
        .then(() => openAttachmentsModal()) // перезагружаем список
        .catch(err => alert(err.message));
});

function deleteAttachment(id) {
    if (!confirm("Удалить вложение?")) return;
    fetch(`/Attachments/Delete?id=${id}`, {
        method: 'POST'
    })
        .then(res => {
            if (!res.ok) throw new Error("Ошибка удаления");
            openAttachmentsModal(); // обновим список
        })
        .catch(err => alert(err.message));
}

function initDragAndDrop() {
    const taskCards = document.querySelectorAll('.task-card');
    const columns = document.querySelectorAll('.kanban-column');

    // Делаем карточки перетаскиваемыми
    taskCards.forEach(card => {
        card.setAttribute('draggable', 'true'); // Это критически важно!
        card.addEventListener('dragstart', handleDragStart);
        card.addEventListener('dragend', handleDragEnd);
    });

    // Настройка зон для перетаскивания
    columns.forEach(column => {
        column.addEventListener('dragover', handleDragOver);
        column.addEventListener('dragleave', handleDragLeave);
        column.addEventListener('drop', handleDrop);
    });
}

function handleDragStart(e) {
    this.classList.add('dragging');
    e.dataTransfer.setData('text/plain', this.dataset.taskId);

    // Важно для Firefox
    e.dataTransfer.effectAllowed = 'move';

    setTimeout(() => {
        this.style.opacity = '0.4';
    }, 0);
}

function handleDragEnd() {
    this.classList.remove('dragging');
    this.style.opacity = '1';
}

function handleDragOver(e) {
    e.preventDefault(); // Это обязательно!
    this.classList.add('drag-over');

    // Определяем позицию для вставки
    const draggingCard = document.querySelector('.dragging');
    if (!draggingCard) return;

    const afterElement = getDragAfterElement(this, e.clientY);
    const container = this.querySelector('.task-container');

    if (afterElement == null) {
        container.appendChild(draggingCard);
    } else {
        container.insertBefore(draggingCard, afterElement);
    }
}

function handleDragLeave() {
    this.classList.remove('drag-over');
}

function handleDrop(e) {
    e.preventDefault(); // Это обязательно!
    this.classList.remove('drag-over');

    const taskId = e.dataTransfer.getData('text/plain');
    const taskCard = document.querySelector(`.task-card[data-task-id="${taskId}"]`);
    const newStatus = this.dataset.status;

    // Если статус не изменился - ничего не делаем
    if (taskCard.dataset.status === newStatus) return;

    updateTaskStatus(taskId, newStatus)
        .then(() => {
            taskCard.dataset.status = newStatus;
        })
        .catch(error => {
            console.error('Ошибка:', error);
            // Возвращаем карточку на место при ошибке
            const originalColumn = document.querySelector(`.kanban-column[data-status="${taskCard.dataset.status}"]`);
            originalColumn.querySelector('.task-container').appendChild(taskCard);
            alert('Не удалось обновить статус задачи');
        });
}

function getDragAfterElement(container, y) {
    const draggableElements = [...container.querySelectorAll('.task-card:not(.dragging)')];

    return draggableElements.reduce((closest, child) => {
        const box = child.getBoundingClientRect();
        const offset = y - box.top - box.height / 2;

        if (offset < 0 && offset > closest.offset) {
            return { offset: offset, element: child };
        } else {
            return closest;
        }
    }, { offset: Number.NEGATIVE_INFINITY }).element;
}

function updateTaskStatus(taskId, newStatus) {
    const formData = new FormData();
    formData.append('id', taskId);
    formData.append('status', newStatus);

    return fetch('/ProjectTasks/UpdateStatus', {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: formData
    }).then(response => {
        if (!response.ok) throw new Error('Ошибка сервера');
        return response.text();
    });
}

document.addEventListener('DOMContentLoaded', function () {
    initDragAndDrop();
});