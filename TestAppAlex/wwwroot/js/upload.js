// === upload.js ===

let sortMode = 'date';
let selectedFiles = new Set();
let searchTerm = "";
let activeExtensions = new Set();
let dateFilter = null;
let locationFilter = null; // folderId

function setSortMode(mode) {
    sortMode = mode;
    loadSuggestedFiles();
}

function toggleSelectAll(masterCheckbox) {
    const checkboxes = document.querySelectorAll('.file-checkbox');
    checkboxes.forEach(cb => {
        cb.checked = masterCheckbox.checked;
        const fileName = cb.closest('.file-card').querySelector('.file-name').title;
        if (masterCheckbox.checked) selectedFiles.add(fileName);
        else selectedFiles.delete(fileName);
    });
    updateMassActionButtons();
}

function toggleFileSelection(fileName, checkbox) {
    if (checkbox.checked) {
        selectedFiles.add(fileName);
    } else {
        selectedFiles.delete(fileName);
    }
    updateMassActionButtons();
}

function updateMassActionButtons() {
    const container = document.getElementById("bulkActions");
    if (!container) return;

    container.style.display = selectedFiles.size > 0 ? "block" : "none";
}

function massDownload() {
    if (!selectedFiles.size) return alert("N-ai selectat fișiere.");

    const filesArray = Array.from(selectedFiles);

    fetch('/api/upload/download-multiple', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(filesArray)
    })
        .then(res => {
            if (!res.ok) throw new Error("Eroare la descărcarea fișierelor.");
            return res.blob();
        })
        .then(blob => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'files.zip';
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        })
        .catch(err => {
            alert("Eroare la descărcarea fișierelor.");
            console.error(err);
        });
}


function massDelete() {
    if (!selectedFiles.size) return alert("N-ai selectat fișiere.");
    if (!confirm("Sigur vrei să ștergi fișierele selectate?")) return;

    const filesArray = Array.from(selectedFiles);

    fetch('/api/upload/delete-multiple', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(filesArray)
    })
        .then(res => {
            if (!res.ok) throw new Error("Eroare la ștergerea fișierelor.");
            return res.text();
        })
        .then(() => {
            selectedFiles.clear();
            loadSuggestedFiles();
            updateMassActionButtons();
        })
        .catch(err => {
            alert("Eroare la ștergerea fișierelor.");
            console.error(err);
        });
}


function moveSelected() {
    alert("Funcționalitatea de mutare în dosar urmează să fie implementată.");
}

function createFolder() {
    const folderName = prompt("Nume pentru folder:");
    if (!folderName || folderName.trim() === "") return;

    fetch('/api/upload/create-folder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ folderName })
    })
        .then(res => {
            if (!res.ok) throw new Error("Eroare la creare sau duplicat.");
            return res.json();
        })
        .then(folder => {
            alert(`Folder "${folder.name}" creat cu succes!`);
            loadFolders();
        })
        .catch(err => alert(err.message));
}

function loadFolders() {
    const container = document.querySelector('#foldersSection .dropdown-content');
    if (!container) return;

    fetch('/api/upload/folders')
        .then(res => res.json())
        .then(folders => {
            container.innerHTML = "";

            if (folders.length === 0) {
                container.innerHTML = `<p id="noFoldersText">You do not have any folders</p>`;
                return;
            }

            folders.forEach(f => {
                const div = document.createElement('div');
                div.classList.add('file-card');

                div.innerHTML = `
                    <div class="file-icon">📁</div>
                    <div class="file-details">
                        <div class="file-name" title="${f.name}" onclick="openFolder(${f.id}, '${f.name.replace(/'/g, "\\'")}')" style="cursor: pointer; color: #007bff;">
                            ${truncateName(f.name, 25)}
                        </div>
                        <div class="file-size">Creat la: ${new Date(f.createdAt).toLocaleString()}</div>
                    </div>
                    <div class="file-actions">
                        <a href="/api/upload/download-folder?id=${f.id}" title="Descarcă folderul" class="download-btn">⬇</a>
                        <button class="delete-btn" onclick="deleteFolder(${f.id})">🗑</button>
                    </div>
                `;

                container.appendChild(div);
            });
        })
        .catch(err => {
            console.error("Eroare la încărcarea folderelor:", err);
        });
}


function deleteFolder(folderId) {
    if (!confirm("Sigur vrei să ștergi acest folder?")) return;

    fetch(`/api/upload/delete-folder?id=${folderId}`, { method: 'DELETE' })
        .then(res => {
            if (res.ok) {
                loadFolders();
            } else {
                alert("Eroare la ștergerea folderului.");
            }
        })
        .catch(err => console.error("Delete folder error:", err));
}

function loadSuggestedFiles(term = searchTerm) {
    fetch('/api/upload/list')
        .then(response => response.json())
        .then(files => {
            const lowerSearch = term.toLowerCase();
            files = files.filter(f => f.name.toLowerCase().includes(lowerSearch));

            //  Filtru dupa extensie
            if (activeExtensions.size > 0) {
                files = files.filter(f => {
                    const ext = f.name.split('.').pop().toLowerCase();
                    return activeExtensions.has(ext);
                });
            }

            //  Filtru după data
            if (dateFilter) {
                const now = new Date();
                files = files.filter(f => {
                    const uploaded = new Date(f.uploadedAt);
                    const diffMs = now - uploaded;

                    if (dateFilter === "hour") return diffMs <= 3600 * 1000;
                    if (dateFilter === "day") return diffMs <= 24 * 3600 * 1000;
                    if (dateFilter === "week") return diffMs <= 7 * 24 * 3600 * 1000;
                    return true;
                });
            }

            if (locationFilter !== null) {
                files = files.filter(f => f.folderId === locationFilter);
            }


            //  Sortare
            if (sortMode === 'name') {
                files.sort((a, b) => a.name.localeCompare(b.name));
            } else if (sortMode === 'date') {
                files.sort((a, b) => new Date(b.uploadedAt) - new Date(a.uploadedAt));
            } else if (sortMode === 'size') {
                files.sort((a, b) => b.size - a.size);
            }

            //  Randare
            const container = document.querySelector('.dropdown:nth-of-type(2) .dropdown-content');
            container.innerHTML = '';

            files.forEach(file => {
                const uploadDate = new Date(file.uploadedAt).toLocaleString();
                const ext = file.name.split('.').pop().toLowerCase();
                let preview = '';

                if (['png', 'jpg', 'jpeg', 'gif'].includes(ext)) {
                    preview = `<img src="/uploads/${encodeURIComponent(file.name)}" class="preview-thumb" alt="preview" />`;
                } else if (ext === 'pdf') {
                    preview = `<iframe src="/uploads/${encodeURIComponent(file.name)}" class="preview-pdf"></iframe>`;
                }

                const isChecked = selectedFiles.has(file.name) ? 'checked' : '';
                const folderInfo = file.folderId ? `<div class="in-folder-tag">📁 în folder</div>` : '';

                const div = document.createElement('div');
                div.classList.add('file-card');
                div.innerHTML = `
                    <input type="checkbox" class="file-checkbox" onchange="toggleFileSelection('${file.name}', this)" ${isChecked}>
                    <div class="file-icon">${getFileIcon(file.name)}</div>
                    <div class="file-details">
                        <div class="file-name" title="${file.name}" onclick="openPreview('${file.name}')" style="cursor:pointer; color:#007bff;">
                            ${truncateName(file.name, 25)}
                        </div>
                        <div class="file-size">${formatFileSize(file.size)} · ${uploadDate}</div>
                        ${folderInfo}
                        ${preview}
                    </div>
                    <div class="file-actions">
                        <a href="/api/upload/download?fileName=${encodeURIComponent(file.name)}" class="download-btn" title="Descarcă" download>⬇</a>
                        <button class="delete-btn" onclick="deleteFile('${file.name}')">🗑</button>
                        <button class="rename-btn" onclick="renameFile('${file.name}')">📝</button>
                    </div>
                `;
                container.appendChild(div);
            });

            updateMassActionButtons();
        })
        .catch(error => {
            console.error('Failed to load files:', error);
        });
}

function toggleLocationFilter() {
    const popup = document.getElementById('locationFilterPopup');
    popup.style.display = popup.style.display === 'block' ? 'none' : 'block';

    if (popup.style.display === 'block') {
        const list = document.getElementById('locationFolderList');
        list.innerHTML = "(Se încarcă...)";
        fetch('/api/upload/folders')
            .then(res => res.json())
            .then(folders => {
                if (!folders.length) {
                    list.innerHTML = "<p>Nu ai foldere create</p>";
                    return;
                }

                list.innerHTML = folders.map(f =>
                    `<label><input type="radio" name="locFolder" value="${f.id}"> ${f.name}</label><br>`
                ).join('');
            });
    }
}

function applyLocationFilter() {
    const selected = document.querySelector('input[name="locFolder"]:checked');
    locationFilter = selected ? parseInt(selected.value) : null;
    loadSuggestedFiles();
}


// renameFile(fileName) 
function renameFile(oldName) {
    const newName = prompt("Noul nume pentru fișier:", oldName);
    if (!newName || newName.trim() === "" || newName === oldName) return;

    fetch('/api/upload/rename', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ oldFileName: oldName, newFileName: newName })
    })
        .then(res => {
            if (!res.ok) throw new Error("Eroare la redenumire.");
            return res.text();
        })
        .then(() => {
            alert("Fișier redenumit cu succes!");
            loadSuggestedFiles();
            if (currentFolderId !== null) openFolder(currentFolderId, currentFolderName); 
        })
        .catch(err => {
            alert("Redenumirea a eșuat.");
            console.error(err);
        });
}

function truncateName(name, maxLength) {
    return name.length > maxLength ? name.slice(0, maxLength - 3) + '...' : name;
}

function formatFileSize(size) {
    return size > 1024 * 1024
        ? (size / 1024 / 1024).toFixed(1) + ' MB'
        : (size / 1024).toFixed(1) + ' KB';
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

function closeFolderModal() {
    document.getElementById("folderModal").style.display = "none";
    document.getElementById("folderFilesContent").innerHTML = "";
}

function moveSelected() {
    fetch('/api/upload/folders')
        .then(res => res.json())
        .then(folders => {
            if (!folders.length) return alert("Nu ai foldere create.");

            const folderId = prompt("ID-ul folderului în care vrei să muți fișierele:\n" +
                folders.map(f => `${f.id} - ${f.name}`).join("\n"));

            if (!folderId) return;

            const body = {
                folderId: parseInt(folderId),
                fileNames: Array.from(selectedFiles)
            };

            fetch('/api/upload/move-to-folder', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            })
                .then(res => {
                    if (!res.ok) throw new Error("Eroare la mutare.");
                    return res.text();
                })
                .then(() => {
                    alert("Fișierele au fost mutate.");
                    selectedFiles.clear();
                    loadSuggestedFiles();
                    updateMassActionButtons();
                })
                .catch(err => alert(err.message));
        });
}

let selectedFolderFiles = new Set();

function toggleFolderSelectAll(master) {
    document.querySelectorAll(".folder-file-checkbox").forEach(cb => {
        cb.checked = master.checked;
        const name = cb.dataset.filename;
        if (master.checked) selectedFolderFiles.add(name);
        else selectedFolderFiles.delete(name);
    });
    updateFolderMassActionButtons();
}

function toggleFolderFileSelection(cb) {
    const name = cb.dataset.filename;
    if (cb.checked) selectedFolderFiles.add(name);
    else selectedFolderFiles.delete(name);
    updateFolderMassActionButtons();
}

function updateFolderMassActionButtons() {
    const bar = document.getElementById("folderBulkActions");
    if (bar) bar.style.display = selectedFolderFiles.size > 0 ? "block" : "none";
}

function folderMassDownload() {
    selectedFolderFiles.forEach(fileName => {
        const a = document.createElement('a');
        a.href = `/api/upload/download?fileName=${encodeURIComponent(fileName)}`;
        a.download = fileName;
        a.click();
    });
    selectedFolderFiles.clear();
    updateFolderMassActionButtons();
}

function folderMassDelete() {
    if (!confirm("Ștergi fișierele selectate din folder?")) return;

    const promises = Array.from(selectedFolderFiles).map(file =>
        fetch(`/api/upload/delete?fileName=${encodeURIComponent(file)}`, { method: 'DELETE' })
    );

    Promise.all(promises)
        .then(() => {
            selectedFolderFiles.clear();
            closeFolderModal();
            setTimeout(() => openFolder(currentFolderId, currentFolderName), 300); 
        });
}

document.getElementById('fileInput')?.addEventListener('change', async function () {
    const files = this.files;
    if (!files.length) return;

    let allFailed = true;

    for (let file of files) {
        const formData = new FormData();
        formData.append('file', file);

        try {
            const res = await fetch('/api/upload/file', {
                method: 'POST',
                body: formData
            });

            if (!res.ok) {
                const text = await res.text();
                console.warn(`Upload failed for ${file.name}:`, res.status, text);
                continue;
            }

            const contentType = res.headers.get("Content-Type") || "";
            const data = contentType.includes("application/json")
                ? await res.json()
                : { name: file.name };

            console.log("Fișier urcat:", data.name);
            allFailed = false;
        } catch (err) {
            console.error(`Eroare la fișierul ${file.name}:`, err);
        }
    }

    loadSuggestedFiles();
    updateStorageBar?.();

    if (allFailed) {
        alert("Toate fișierele au eșuat la upload.");
    }

    this.value = "";
});



let currentFolderId = null;
let currentFolderName = "";
function openFolder(folderId, folderName) {
    const title = document.getElementById("folderModalTitle");
    const content = document.getElementById("folderFilesContent");
    title.textContent = `📁 ${folderName}`;
    content.innerHTML = "Se încarcă...";

    fetch(`/api/upload/folder-files?id=${folderId}`)
        .then(res => res.json())
        .then(files => {
            if (!files.length) {
                content.innerHTML = "<p>Acest folder este gol.</p>";
                return;
            }

            content.innerHTML = `
                <label style="display: flex; align-items: center; gap: 5px; margin-bottom: 10px;">
                    <input type="checkbox" id="folderSelectAll" onchange="toggleFolderSelectAll(this)" />
                    Selectează toate fișierele
                </label>
                <div id="folderBulkActions" style="display:none; margin-bottom: 10px;">
                    <button onclick="folderMassDownload()">⬇</button>
                    <button onclick="folderMassDelete()">🗑</button>
                </div>
            `;

            files.forEach(file => {
                const fileEl = document.createElement("div");
                fileEl.classList.add("file-card");
                const uploadDate = new Date(file.uploadedAt).toLocaleString();

                fileEl.innerHTML = `
                    <input type="checkbox" class="folder-file-checkbox" data-filename="${file.name}" onchange="toggleFolderFileSelection(this)">
                    <div class="file-icon">${getFileIcon(file.name)}</div>
                    <div class="file-details">
                        <div class="file-name">${truncateName(file.name, 25)}</div>
                        <div class="file-size">${formatFileSize(file.size)} · ${uploadDate}</div>
                    </div>
                    <div class="file-actions">
                        <a href="/api/upload/download?fileName=${encodeURIComponent(file.name)}" class="download-btn" title="Descarcă" download>⬇</a>
                        <button class="delete-btn" onclick="deleteFile('${file.name}', true)">🗑</button>
                    </div>
                `;

                content.appendChild(fileEl);
            });
        })
        .catch(err => {
            content.innerHTML = "<p>Eroare la încărcarea fișierelor.</p>";
        });

    document.getElementById("folderModal").style.display = "block";
}
function updateStorageBar() {
    fetch('/api/upload/storage')
        .then(res => res.json())
        .then(data => {
            const usedBytes = data.used;
            const maxBytes = 100 * 1024 * 1024; // 100 MB
            const percentage = Math.min((usedBytes / maxBytes) * 100, 100).toFixed(0);

            const bar = document.querySelector('.storage-fill');
            const text = document.querySelector('.storage-bar-wrapper p');

            if (bar) bar.style.width = `${percentage}%`;
            if (text) text.textContent = `Spațiu de stocare folosit: ${percentage}%. Dacă rămâi fără spațiu, nu mai poți să creezi, să editezi sau să încarci fișiere.`;
        })
        .catch(err => console.error('Eroare la calcularea spațiului folosit:', err));
}


function deleteFile(fileName, isInFolder = false) {
    if (!confirm(`Sigur vrei să ștergi fișierul ${fileName}?`)) return;

    fetch(`/api/upload/delete?fileName=${encodeURIComponent(fileName)}`, {
        method: 'DELETE'
    })
        .then(res => {
            if (!res.ok) throw new Error("Eroare la ștergere.");
            if (isInFolder) {
                // reincarca folderul activ
                closeFolderModal();
                setTimeout(() => openFolder(currentFolderId, currentFolderName), 300);
            } else {
                loadSuggestedFiles();
            }
        })
        .catch(err => {
            alert("A apărut o eroare la ștergere.");
            console.error(err);
        });
}

function toggleTypeFilter() {
    const popup = document.getElementById('typeFilterPopup');
    popup.style.display = popup.style.display === 'block' ? 'none' : 'block';
}
function toggleDateFilter() {
    const popup = document.getElementById('dateFilterPopup');
    popup.style.display = popup.style.display === 'block' ? 'none' : 'block';
}

function applyFilters() {
    // actualizeaza extensii
    activeExtensions.clear();
    document.querySelectorAll('#typeFilterPopup input[type="checkbox"]:checked')
        .forEach(cb => activeExtensions.add(cb.value));

    // actualizeaza filtrul de data
    const selectedDate = document.querySelector('#dateFilterPopup input[type="radio"]:checked');
    dateFilter = selectedDate ? selectedDate.value : null;

    loadSuggestedFiles(searchTerm);
}



window.addEventListener('DOMContentLoaded', () => {
    loadSuggestedFiles();
    loadFolders();
    updateStorageBar?.();
});
document.querySelector('.search-box')?.addEventListener('input', function () {
    searchTerm = this.value;
    loadSuggestedFiles();
});

