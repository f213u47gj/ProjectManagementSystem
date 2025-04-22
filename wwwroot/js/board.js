/*// Глобальные переменные
let currentTaskId = null;

// Показ контекстного меню задачи
function showTaskContextMenu(event, taskId) {
    event.preventDefault();
    currentTaskId = taskId;

    const menu = document.getElementById("taskContextMenu");
    menu.style.left = `${event.pageX}px`;
    menu.style.top = `${event.pageY}px`;
    menu.style.display = "block";

    document.addEventListener("click", function hideMenu() {
        menu.style.display = "none";
        document.removeEventListener("click", hideMenu);
    });
}

// Открытие модального окна вложений
function openAttachmentsModal() {
    if (!currentTaskId) return;

    $('#attachmentTaskId').val(currentTaskId);
    $('#attachmentsModal').modal('show');
    loadAttachments(currentTaskId);
    checkUploadPermissions(currentTaskId);
}

// Загрузка вложений
function loadAttachments(taskId) {
    const container = $('#attachmentsList');
    container.html('<div class="text-center py-3"><div class="spinner-border"></div><p>Загрузка вложений...</p></div>');

    $.get(`/Attachments/GetAttachments?taskId=${taskId}`, function (attachments) {
        if (!attachments || attachments.length === 0) {
            container.html('<div class="text-center text-muted">Нет вложений</div>');
            return;
        }

        let html = '<div class="list-group">';
        attachments.forEach(att => {
            const icon = getFileIcon(att.fileName);
            html += `
                <div class="list-group-item d-flex justify-content-between align-items-center" data-id="${att.id}">
                    <div class="d-flex align-items-center">
                        <i class="fas ${icon} me-3 fs-4"></i>
                        <div>
                            <a href="${att.url}" target="_blank" class="text-decoration-none d-block">${att.fileName}</a>
                            <small class="text-muted">${att.uploadDate} • ${att.userName ?? att.userId}</small>
                        </div>
                    </div>
                    ${att.canDelete ? `
                    <button class="btn btn-sm btn-outline-danger delete-attachment" title="Удалить">
                        <i class="fas fa-trash"></i>
                    </button>` : ''}
                </div>
            `;
        });
        html += '</div>';
        container.html(html);
    }).fail(() => {
        container.html('<div class="alert alert-danger">Ошибка загрузки вложений</div>');
    });
}

// Проверка прав на загрузку
function checkUploadPermissions(taskId) {
    $.get(`/Attachments/CanUpload?taskId=${taskId}`, function (res) {
        $('#uploadAttachmentForm').toggle(res.canUpload);
    }).fail(() => {
        $('#uploadAttachmentForm').hide();
    });
}

// Получение иконки по типу файла
function getFileIcon(fileName) {
    const ext = fileName?.split('.').pop().toLowerCase();
    const icons = {
        pdf: 'fa-file-pdf text-danger',
        doc: 'fa-file-word text-primary', docx: 'fa-file-word text-primary',
        xls: 'fa-file-excel text-success', xlsx: 'fa-file-excel text-success',
        ppt: 'fa-file-powerpoint text-warning', pptx: 'fa-file-powerpoint text-warning',
        jpg: 'fa-file-image text-info', jpeg: 'fa-file-image text-info',
        png: 'fa-file-image text-info', gif: 'fa-file-image text-info',
        svg: 'fa-file-image text-info', webp: 'fa-file-image text-info',
        zip: 'fa-file-archive text-secondary', rar: 'fa-file-archive text-secondary',
        '7z': 'fa-file-archive text-secondary', tar: 'fa-file-archive text-secondary', gz: 'fa-file-archive text-secondary',
        cs: 'fa-file-code text-primary', js: 'fa-file-code text-warning',
        html: 'fa-file-code text-danger', htm: 'fa-file-code text-danger',
        css: 'fa-file-code text-info', py: 'fa-file-code text-success',
        json: 'fa-file-code text-secondary', sql: 'fa-database text-primary',
        cshtml: 'fa-file-code text-primary', txt: 'fa-file-alt text-muted',
        md: 'fa-markdown text-muted'
    };
    return icons[ext] || 'fa-file text-muted';
}

// Отправка формы загрузки вложения
$('#uploadAttachmentForm').on('submit', function (e) {
    e.preventDefault();
    const form = $(this);
    const formData = new FormData(form[0]);
    const submitBtn = form.find('button[type="submit"]');

    submitBtn.prop('disabled', true).html(`
        <span class="spinner-border spinner-border-sm" role="status"></span>
        Загрузка...
    `);

    $.ajax({
        url: '/Attachments/Upload',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function () {
            form[0].reset();
            loadAttachments(currentTaskId);
            showToast('Файл успешно загружен', 'success');
        },
        error: function (xhr) {
            showToast(xhr.responseText || 'Ошибка загрузки файла', 'danger');
        },
        complete: function () {
            submitBtn.prop('disabled', false).html('<i class="fas fa-upload me-2"></i>Загрузить');
        }
    });
});

// Удаление вложения
$(document).on('click', '.delete-attachment', function () {
    const attachmentId = $(this).closest('.list-group-item').data('id');
    if (!confirm('Удалить вложение?')) return;

    $.post('/Attachments/Delete', { id: attachmentId })
        .done(() => {
            loadAttachments(currentTaskId);
            showToast('Вложение удалено', 'success');
        })
        .fail((xhr) => {
            showToast(xhr.responseText || 'Ошибка удаления', 'danger');
        });
});

// Показ уведомления
function showToast(message, type = 'success') {
    const toast = $(`
        <div class="toast show align-items-center text-white bg-${type} border-0 position-fixed bottom-0 end-0 m-3" role="alert">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `);
    $('body').append(toast);
    setTimeout(() => toast.remove(), 3000);
}
*/