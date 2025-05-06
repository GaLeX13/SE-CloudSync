document.getElementById('fileInput').addEventListener('change', function (e) {
    const files = e.target.files;
    uploadFiles(files);
});

document.getElementById('uploadArea').addEventListener('drop', function (e) {
    e.preventDefault();
    const files = e.dataTransfer.files;
    uploadFiles(files);
});

document.getElementById('uploadArea').addEventListener('dragover', function (e) {
    e.preventDefault();
});

let sortMode = 'date';

function setSortMode(mode) {
    sortMode = mode;
    loadSuggestedFiles();
}


function uploadFiles(files) {
    const fileList = document.getElementById('fileList');
    fileList.innerHTML = '';

    Array.from(files).forEach(file => {
        const formData = new FormData();
        formData.append('file', file);

        fetch('/api/upload/file', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                const li = document.createElement('li');
                li.classList.add('file-card');

                li.innerHTML = `
                    <div class="file-icon">${getFileIcon(data.name)}</div>
                    <div class="file-details">
                        <div class="file-name" title="${data.name}">${truncateName(data.name, 25)}</div>
                        <div class="file-size">${formatFileSize(data.size)}</div>
                    </div>
                    <div class="file-actions">
                        <a href="/api/upload/download?fileName=${encodeURIComponent(data.name)}" class="download-btn" title="Descarcă" download>⬇</a>
                        <button class="delete-btn" onclick="deleteFile('${data.name}')">🗑</button>
                    </div>
                `;

                fileList.appendChild(li);
                loadSuggestedFiles();
            })
            .catch(error => {
                console.error('Upload error:', error);
                alert('A apărut o eroare la încărcare. Verifică dacă ești logat.');
            });
    });
}


function loadSuggestedFiles() {
    fetch('/api/upload/list')
        .then(response => response.json())
        .then(files => {
            // Aplicăm sortare, dacă este setat un mod de sortare
            if (window.sortMode === 'name') {
                files.sort((a, b) => a.name.localeCompare(b.name));
            } else if (window.sortMode === 'date') {
                files.sort((a, b) => new Date(b.uploadedAt) - new Date(a.uploadedAt));
            } else if (window.sortMode === 'size') {
                files.sort((a, b) => b.size - a.size);
            }

            const container = document.querySelector('.dropdown:nth-of-type(2) .dropdown-content');
            container.innerHTML = '';

            files.forEach(file => {
                const div = document.createElement('div');
                div.classList.add('file-card');

                const uploadDate = new Date(file.uploadedAt).toLocaleString();

                div.innerHTML = `
                    <div class="file-icon">${getFileIcon(file.name)}</div>
                    <div class="file-details">
                        <div class="file-name" title="${file.name}">${truncateName(file.name, 25)}</div>
                        <div class="file-size">${formatFileSize(file.size)} · ${uploadDate}</div>
                    </div>
                    <div class="file-actions">
                        <a href="/api/upload/download?fileName=${encodeURIComponent(file.name)}"
                           class="download-btn" title="Descarcă" download>⬇</a>
                        <button class="delete-btn" onclick="deleteFile('${file.name}')">🗑</button>
                    </div>
                `;
                container.appendChild(div);
            });
        })
        .catch(error => {
            console.error('Failed to load files:', error);
        });
}

// Funcție globală pentru a seta modul de sortare și a reîncărca lista
function setSortMode(mode) {
    window.sortMode = mode;
    loadSuggestedFiles();
}

function deleteFile(fileName) {
    if (!confirm(`Sigur vrei să ștergi fișierul "${fileName}"?`)) return;

    fetch(`/api/upload/delete?fileName=${encodeURIComponent(fileName)}`, {
        method: 'DELETE'
    })
        .then(res => {
            if (res.ok) {
                loadSuggestedFiles();
                document.getElementById('fileList').innerHTML = '';
            } else {
                alert("Eroare la ștergerea fișierului.");
            }
        })
        .catch(err => {
            console.error("Delete error:", err);
        });
}

function getFileIcon(fileName) {
    const ext = fileName.split('.').pop().toLowerCase();
    if (['png', 'jpg', 'jpeg', 'gif'].includes(ext)) return '🖼';
    if (['pdf'].includes(ext)) return '📄';
    if (['doc', 'docx'].includes(ext)) return '📝';
    if (['xls', 'xlsx'].includes(ext)) return '📊';
    if (['zip', 'rar'].includes(ext)) return '🗜';
    return '📁';
}

function truncateName(name, maxLength) {
    return name.length > maxLength ? name.slice(0, maxLength - 3) + '...' : name;
}

function formatFileSize(size) {
    return size > 1024 * 1024
        ? (size / 1024 / 1024).toFixed(1) + ' MB'
        : (size / 1024).toFixed(1) + ' KB';
}

function updateStorageBar() {
    fetch('/api/upload/storage')
        .then(response => response.json())
        .then(data => {
            const used = data.used;
            const total = 15 * 1024 * 1024 * 1024; // 15 GB
            const percent = Math.min((used / total) * 100, 100).toFixed(0);

            const bar = document.querySelector('.storage-fill');
            const wrapper = document.querySelector('.storage-bar-wrapper');

            if (bar) bar.style.width = `${percent}%`;
            if (wrapper) {
                wrapper.innerHTML = `
                    Spațiu de stocare folosit: ${percent}%. Dacă rămâi fără spațiu, nu mai poți să creezi, să editezi sau să încarci fișiere.
                    <div class="storage-bar"><div class="storage-fill" style="width: ${percent}%;"></div></div>
                `;
            }
        })
        .catch(err => console.error("Nu s-a putut actualiza bara de stocare:", err));
}

window.addEventListener('DOMContentLoaded', () => {
    loadSuggestedFiles();
    updateStorageBar();
});
