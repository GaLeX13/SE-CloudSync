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
                const ext = file.name.split('.').pop().toLowerCase();
                let preview = '';

                if (['png', 'jpg', 'jpeg', 'gif'].includes(ext)) {
                    preview = `<img src="/uploads/${encodeURIComponent(file.name)}" class="preview-thumb" alt="preview" />`;
                } else if (ext === 'pdf') {
                    preview = `<iframe src="/uploads/${encodeURIComponent(file.name)}" class="preview-pdf"></iframe>`;
                }

                div.innerHTML = `
                    <div class="file-icon">${getFileIcon(file.name)}</div>
                    <div class="file-details">
                <div class="file-name" title="${file.name}" onclick="openPreview('${file.name}')" style="cursor:pointer; color:#007bff;">
                ${truncateName(file.name, 25)}
                </div>

                        <div class="file-size">${formatFileSize(file.size)} · ${uploadDate}</div>
                        ${preview}
                    </div>
                    <div class="file-actions">
                        <a href="/api/upload/download?fileName=${encodeURIComponent(file.name)}"
                           class="download-btn" title="Descarcă" download>⬇</a>
                        <button class="delete-btn" onclick="deleteFile('${file.name}')">🗑</button>
                        <button class="rename-btn" onclick="renameFile('${file.name}')">📝</button>
                    </div>
                `;
                container.appendChild(div);
            });
        })
        .catch(error => {
            console.error('Failed to load files:', error);
        });
}

function renameFile(oldFileName) {
    const newFileName = prompt("Introduceți noul nume pentru fișier:");
    if (!newFileName || newFileName.trim() === "") return;

    fetch('/api/upload/rename', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            oldFileName: oldFileName,
            newFileName: newFileName
        })
    })
        .then(response => {
            if (response.ok) {
                loadSuggestedFiles();
            } else if (response.status === 409) {
                alert("Un fișier cu acest nume există deja.");
            } else {
                alert("Redenumirea a eșuat.");
            }
        });
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

function openPreview(fileName) {
    const ext = fileName.split('.').pop().toLowerCase();
    const content = document.getElementById("previewContent");

    if (['png', 'jpg', 'jpeg', 'gif'].includes(ext)) {
        content.innerHTML = `<img src="/uploads/${encodeURIComponent(fileName)}" alt="preview">`;
    } else if (ext === 'pdf') {
        content.innerHTML = `<iframe src="/uploads/${encodeURIComponent(fileName)}" width="100%" height="500px"></iframe>`;
    } else {
        content.innerHTML = `<p>Previzualizare indisponibilă pentru acest tip de fișier.</p>`;
    }

    document.getElementById("previewModal").style.display = "block";
}

function closePreview() {
    document.getElementById("previewModal").style.display = "none";
    document.getElementById("previewContent").innerHTML = "";
}

window.addEventListener('DOMContentLoaded', () => {
    loadSuggestedFiles();
    updateStorageBar();
});
