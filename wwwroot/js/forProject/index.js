function confirmDeleteProject(name, id) {
    document.getElementById('projectToDeleteName').textContent = name;
    document.getElementById('deleteProjectForm').action = '/Projects/Delete/' + id;
    const modal = new bootstrap.Modal(document.getElementById('deleteProjectModal'));
    modal.show();
}

document.addEventListener('DOMContentLoaded', function () {
    const projectCards = document.querySelectorAll('.project-card');

    projectCards.forEach(card => {
        card.addEventListener('contextmenu', function (e) {
            e.preventDefault();

            document.querySelectorAll('.context-menu').forEach(menu => {
                menu.style.display = 'none';
            });

            const menu = this.closest('.project-item').querySelector('.context-menu');
            if (menu) {
                menu.style.display = 'block';
                menu.style.left = `${e.clientX}px`;
                menu.style.top = `${e.clientY}px`;
            }
        });
    });

    document.addEventListener('click', function () {
        document.querySelectorAll('.context-menu').forEach(menu => {
            menu.style.display = 'none';
        });
    });
});