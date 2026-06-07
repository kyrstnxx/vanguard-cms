document.addEventListener('DOMContentLoaded', function () {

    const zone = document.querySelector('.upload-zone');
    const input = document.getElementById('photoInput');
    const label = document.getElementById('fileName');

    if (zone && input) {
        zone.addEventListener('dragover', function (e) {
            e.preventDefault();
            zone.style.background = '#D1FAE5';
        });
        zone.addEventListener('dragleave', function () {
            zone.style.background = '';
        });
        zone.addEventListener('drop', function (e) {
            e.preventDefault();
            zone.style.background = '';
            const files = e.dataTransfer.files;
            if (files.length > 0) {
                input.files = files;
                if (label) label.textContent = files[0].name;
            }
        });
        input.addEventListener('change', function () {
            if (label && input.files.length > 0)
                label.textContent = input.files[0].name;
        });
    }

    document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
        setTimeout(function () {
            bootstrap.Alert.getOrCreateInstance(alert)?.close();
        }, 4000);
    });

    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.sb-item').forEach(function (link) {
        const href = link.getAttribute('href')?.toLowerCase();
        if (href && path.startsWith(href) && href !== '/') {
            link.classList.add('active');
        }
    });
});